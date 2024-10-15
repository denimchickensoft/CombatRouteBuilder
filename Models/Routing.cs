using System.Text;
using System.IO;
using System.Diagnostics;
using CRB.Models;

namespace CRB;

public class Routing{
    public class RouteCollection
    {
        public Dictionary<string, Route> Routes { get; set; }

        public RouteCollection()
        {
            Routes = new Dictionary<string, Route>();
        }
    }

    public class Route
    {
        public Dictionary<int, Waypoint> Waypoints { get; set; }

        public Route()
        {
            Waypoints = new Dictionary<int, Waypoint>();
        }
    }

    public class Waypoint
    {
        public double Alt { get; set; }
        public string Type { get; set; }
        public double ETA { get; set; }
        public bool ETALocked { get; set; }
        public double Y { get; set; }
        public double X { get; set; }
        public string Name { get; set; }
        public string Action { get; set; }
        public string AltType { get; set; }
        public bool SpeedLocked { get; set; }

        public Waypoint(
            double alt,
            string type,
            double eta,
            bool etaLocked,
            double y,
            double x,
            string name,
            string action,
            string altType,
            bool speedLocked)
        {
            Alt = alt;
            Type = type;
            ETA = eta;
            ETALocked = etaLocked;
            Y = y;
            X = x;
            Name = name;
            Action = action;
            AltType = altType;
            SpeedLocked = speedLocked;
        }
    }


    public static RouteCollection ConvertDictionaryToRouteCollection(Dictionary<object, object> dictionary)
    {
        var routeCollection = new RouteCollection();
        var routes = new Dictionary<string, Route>();

        foreach (var routeEntry in dictionary)
        {
            if (routeEntry.Key is string routeName && routeEntry.Value is Dictionary<object, object> routeTable)
            {
                var route = new Route();
                var waypoints = new Dictionary<int, Waypoint>();

                foreach (var waypointEntry in routeTable)
                {
                    if (waypointEntry.Key is long waypointKeyLong && waypointEntry.Value is Dictionary<object, object> waypointDict)
                    {
                        int waypointKey = (int)waypointKeyLong; // Convert Int64 to int

                        var waypoint = new Waypoint(
                            Convert.ToDouble(waypointDict["alt"]),
                            waypointDict["type"].ToString(),
                            Convert.ToDouble(waypointDict["ETA"]),
                            Convert.ToBoolean(waypointDict["ETA_locked"]),
                            Convert.ToDouble(waypointDict["y"]),
                            Convert.ToDouble(waypointDict["x"]),
                            waypointDict["name"].ToString(),
                            waypointDict["action"].ToString(),
                            waypointDict["alt_type"].ToString(),
                            Convert.ToBoolean(waypointDict["speed_locked"])
                        );

                        waypoints[waypointKey] = waypoint;
                    }
                    else
                    {
                        Debug.WriteLine("Waypoint Key is not long or Value is not Dictionary<object, object>");
                    }
                }

                route.Waypoints = waypoints;
                routes[routeName] = route;
            }
            else
            {
                Debug.WriteLine("Route Key is not string or Value is not Dictionary<object, object>");
            }
        }

        routeCollection.Routes = routes;
        return routeCollection;
    }

    public static RouteCollection ConvertFlightPlanToRouteCollection(FlightPlan flightPlan){

        // Initialize the RouteCollection
        RouteCollection routeCollection = new RouteCollection();

        // Map the Route from the JSON to your RouteCollection
        Route route = new Route();

        // Iterate through each RoutePoint in the JSON and map it to the Waypoint class
        for (int i = 0; i < flightPlan.Route.RoutePoints.Count; i++)
        {
            var routePoint = flightPlan.Route.RoutePoints[i];
            var waypoint = flightPlan.WaypointTable.Waypoints
                .Where(w => w.Identifier == routePoint.WaypointIdentifier);

            if (waypoint != null)
            {
                //convert Lat Lon to DCS TM
                (double,double) latlon = Calculator.ConvertToMerc(Convert.ToDouble(waypoint.FirstOrDefault().Lat), Convert.ToDouble(waypoint.FirstOrDefault().Lon), "SYRIA");
                
                Waypoint newWaypoint = new Waypoint(
                    alt: 0, // Altitude data is not present in the JSON, default to 0
                    type: "Turning Point",
                    eta: 0, // ETA data is not present in the JSON, default to 0
                    etaLocked: false, // Default to false since data is not available
                    y: latlon.Item2,
                    x: latlon.Item1,
                    name: waypoint.FirstOrDefault().Identifier,
                    action: "Turning Point", // Action data is not present, default to "None"
                    altType: "BARO", // AltType data is not present, default to "None"
                    speedLocked: false // Default to false since data is not available
                );

                route.Waypoints.Add(i, newWaypoint);
            }
        }

        // Add the route to the RouteCollection with the key being the RouteName
        routeCollection.Routes.Add(flightPlan.Route.RouteName, route);

        // Now, routeCollection contains the mapped data
        PrintRouteCollection(routeCollection);
        return routeCollection;

    }

    public static void ExportRouteCollectionToLuaTable(RouteCollection routeCollection)
    {
        StringBuilder luaTable = new StringBuilder();

        luaTable.AppendLine("presets = {");

        foreach (var routeEntry in routeCollection.Routes)
        {
            string routeName = routeEntry.Key;
            Route route = routeEntry.Value;

            luaTable.AppendLine($"  [\"{routeName}\"] = {{");

            foreach (var waypointEntry in route.Waypoints)
            {
                int index = waypointEntry.Key;
                //skip departure waypoint TODO: check how to handle this
                if (index == 0) continue;
                Waypoint waypoint = waypointEntry.Value;

                luaTable.AppendLine($"      [{index}] = {{");
                luaTable.AppendLine($"        [\"alt\"] = {waypoint.Alt},");
                luaTable.AppendLine($"        [\"type\"] = \"{waypoint.Type}\",");
                luaTable.AppendLine($"        [\"ETA\"] = {waypoint.ETA},");
                luaTable.AppendLine($"        [\"ETA_locked\"] = {waypoint.ETALocked.ToString().ToLower()},");
                luaTable.AppendLine($"        [\"y\"] = {waypoint.Y},");
                luaTable.AppendLine($"        [\"x\"] = {waypoint.X},");
                luaTable.AppendLine($"        [\"name\"] = \"{waypoint.Name}\",");
                luaTable.AppendLine($"        [\"action\"] = \"{waypoint.Action}\",");
                luaTable.AppendLine($"        [\"alt_type\"] = \"{waypoint.AltType}\",");
                luaTable.AppendLine($"        [\"speed_locked\"] = {waypoint.SpeedLocked.ToString().ToLower()}");
                luaTable.AppendLine("      },");
            }

            luaTable.AppendLine("    }");
        }

        luaTable.AppendLine("}");

        string lua = luaTable.ToString();
        //Debug.WriteLine(lua);

        //write to file
        string luaFilePath = "new.lua"; // Replace with actual path

        // Read the contents of the .lua file
        File.WriteAllText(luaFilePath, lua);

    }


    public static void PrintRouteCollection(RouteCollection routeCollection)
    {
        foreach (var routeEntry in routeCollection.Routes)
        {
            string routeName = routeEntry.Key;
            Route route = routeEntry.Value;

            Debug.WriteLine($"Route Name: {routeName}");
            Debug.WriteLine("Waypoints:");

            foreach (var waypointEntry in route.Waypoints)
            {
                int index = waypointEntry.Key;
                Waypoint waypoint = waypointEntry.Value;

                Debug.WriteLine($"  Waypoint {index}:");
                Debug.WriteLine($"    Name: {waypoint.Name}");
                Debug.WriteLine($"    Type: {waypoint.Type}");
                Debug.WriteLine($"    Lat: {waypoint.Y}");
                Debug.WriteLine($"    Lon: {waypoint.X}");
                Debug.WriteLine($"    Alt: {waypoint.Alt}");
                Debug.WriteLine($"    ETA: {waypoint.ETA}");
                Debug.WriteLine($"    ETALocked: {waypoint.ETALocked}");
                Debug.WriteLine($"    Action: {waypoint.Action}");
                Debug.WriteLine($"    AltType: {waypoint.AltType}");
                Debug.WriteLine($"    SpeedLocked: {waypoint.SpeedLocked}");
            }
            Debug.WriteLine("\r"); // Add an empty line after each route for better readability
        }
    }

}

