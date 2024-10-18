using System.Text.Json;
using System.Diagnostics;
using CRB.Models;

namespace CRB
{
    class OldProgram
    {
        static void OldMain()
        {
            //Turn lua into a c# collection
            var routeDict = LuaHandler.GetLuaTable("");
            //Convert route dictionary to Collection that contains Routes and their Waypoint objects
            //var routes = RouteExporter.ConvertDictionaryToRouteCollection(routeDict);
            //Convert a .fpl file to FlightPlan object
            //FlightPlan flightPlan = FlightPlan.ConvertXMLtoFlightPlan("");

            //string json = JsonSerializer.Serialize(flightPlan);
            //Debug.WriteLine(json);

            //Convert Flight Plan to RouteCollection
            //Routing.RouteCollection routeCollection = Routing.ConvertFlightPlanToRouteCollection(flightPlan);
            //Routing.ExportRouteCollectionToLuaTable(routeCollection);
            
            //TODO: Convert RouteCollection to .lua file
            
            //TODO: Create reverse functions

            //TODO: Do the rest of the GUI shit

        }
    }
}

