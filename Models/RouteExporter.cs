using System.Text;
using System.IO;
using Caliburn.Micro;
using NLua;
using System.Diagnostics;
using System.Windows;
using System.Collections.Generic;
using System.Collections;
namespace CRB.Models;

public class RouteExporter
{
    //DO IT
    public static string DoTheLuaLua(BindableCollection<RouteItem> routeItems, string presetName, string region, string luaPath, FlightPlan fpl)
    {
        if (routeItems.Count > 0 && presetName != null && region != null && luaPath != null && fpl != null)
        {
            //get name of map used by DCS for RouteToolPresets
            string mapName = Maps.Projections.TheatreToProjectionMap[region].MapName;
            //create path to RouteToolPresets file
            string luaFile = Path.Combine(luaPath, mapName + ".lua");

            //gets Lua Presets from file, if they exist (otherwise create it), then parses presets to dict
            var presets = GetLua(luaFile, mapName);

            //create new preset from route
            var newPreset = RouteToPreset(routeItems, presetName, region, fpl);


            //presets not empty, but contained error
            if (presets != null && presets.ContainsKey("error"))
            {
                return presets["error"].ToString();
            }
            //presets not empty, but already contained preset with same name
            else if (presets != null && presets.ContainsKey(presetName))
            {
                //ask the user if they want to overwrite the preset
                var result = System.Windows.MessageBox.Show($"A route named {presetName} already exists. Do you want to overwrite it?", "Overwrite route?", MessageBoxButton.YesNo, MessageBoxImage.Question);
                if (result == MessageBoxResult.Yes)
                {
                    presets[presetName] = newPreset;
                    string status = ConvertToLuaAndExportToFile(presets, luaFile);
                    return status;
                }
                else
                {
                    return "Route export canceled.";
                }
            }
            //presets not empty, and does not contain preset name
            else if (presets != null)
            {
                presets[presetName] = newPreset;
                string status = ConvertToLuaAndExportToFile(presets, luaFile);
                return status;
            }
            //presets don't exist, create new
            else
            {
                presets = new SortedDictionary<object, object>();
                presets[presetName] = newPreset;
                string status = ConvertToLuaAndExportToFile(presets, luaFile);
                return status;
            }
        }
        else
        {
            Dictionary<string, object> vars = new Dictionary<string, object>
            {
                { "Route", routeItems },
                { "Route Name", presetName },
                { "Map", region },
                { "Lua Path", luaPath },
                { "Flight Plan File", fpl }
            };
            List<string> missingVars = new List<string>();

            foreach (var kvp in vars)
            {
                if (kvp.Value == null)
                {
                    missingVars.Add(kvp.Key);
                }
            }


            return $"Missing {string.Join(" & ", missingVars)} information.";

            
        }
        
    }

    public static SortedDictionary<object, object> GetLua(string luaFile, string mapName)
    {

        //check if file exists, if not return null.  any null return is treated as need for a new file.
        if (File.Exists(luaFile))
        {
            string luaScript = File.ReadAllText(luaFile);

            //if file exists, check to see if the lua is valid, if not throw the exception and ask the user if they want to continue
            try
            {
                using (Lua lua = new Lua())
                {
                    lua.DoString(luaScript);

                    // grab the routes table - named 'presets'
                    var luaTable = lua["presets"] as LuaTable;

                    // Check if the Lua table exists
                    if (luaTable != null)
                    {
                        // Convert the Lua table to a C# dictionary (including nested tables)
                        var dictionary = ParseLua(luaTable);
                        return dictionary;
                    }
                    else
                    {
                        Debug.WriteLine("No presets found.");
                        return null;
                    }
                }
            }
            catch (Exception e)
            {
                var result = System.Windows.MessageBox.Show($"CRB found an error in your \"{mapName}.lua\" file.\nDo you want to discard the file and continue?", "Overwrite corrupt file?", MessageBoxButton.YesNo, MessageBoxImage.Question);
                
                Debug.WriteLine("Found an error in lua file:");
                Debug.WriteLine(e);
                
                //if the user clicks yes, return null to overwrite file
                if (result == MessageBoxResult.Yes)
                {
                    return null;
                }
                //return a dictionary with the error message
                else
                {
                    SortedDictionary<object, object> dictionary = new SortedDictionary<object, object>();
                    dictionary.Add("error", e.Message);
                    return dictionary;
                }
            }
            
        }
        else
        {
            return null;
        }
    }

    static SortedDictionary<object, object> ParseLua(LuaTable luaTable)
    {
        SortedDictionary<object, object> dictionary = new SortedDictionary<object, object>();

        foreach (var key in luaTable.Keys)
        {
            var value = luaTable[key];

            // Check if the value is another LuaTable (nested table)
            if (value is LuaTable)
            {
                // Recursively convert the nested table
                dictionary[key] = ParseLua(value as LuaTable);
            }
            else
            {
                // Add the key-value pair to the dictionary
                dictionary[key] = value;
            }
        }
        return dictionary;
    }

    static SortedDictionary<object, object> RouteToPreset(BindableCollection<RouteItem> routeItems, string presetName, string region,FlightPlan fpl)
    {
        // Create the dictionary that will represent the preset

        // Each route will be a nested dictionary where the key is the waypoint number
        var waypointsDictionary = new SortedDictionary<object, object>();

        //Skip the first item in the route (if origin airport) to comply with DCS' behavior

        Waypoint origin = fpl.WaypointTable.Waypoints[0];
        int skipper;
        if (origin.Type == "AIRPORT" && origin.Identifier == routeItems[0].Name)
        {
            skipper = 1;
        }
        else
        {
            skipper = 0;
        }

        foreach (var routeItem in routeItems.Skip(skipper))
        {
            //if first item was not origin airport, we need to increase index of each subsequent waypoint
            if (skipper == 0  && routeItem.Index == 0)
            {
                routeItem.Index++;
            }

            //convert each lat/lon to mercator
            double N, E;
            (N, E) = Calculator.ConvertToMerc(routeItem.Lat, routeItem.Lon, region);

            var waypointDictionary = new SortedDictionary<object, object>
            {
                { "speed_locked", false },
                { "ETA_locked", routeItem.ETALocked },
                { "x", N },  //Convert to mercator coordinates
                { "y", E },  //same
                { "alt", routeItem.Alt * 0.3048 },  //Convert to meters
                { "alt_type", routeItem.AltType },
                { "ETA", routeItem.ETA.TotalSeconds },
                { "name", routeItem.Name },
                { "type", "Turning Point" },  // Default until we find other options?
                { "action", "Turning Point" }  //Default until we find other options?
            };

            waypointsDictionary[routeItem.Index] = waypointDictionary;
        }

        // Assuming we have one route in this example, with name "RouteName"

        return waypointsDictionary;
    }

    public static StringBuilder DictToLua(SortedDictionary<object,object> dictionary, int indentLevel = 0)
    {
        StringBuilder sb = new StringBuilder();
        string indent = new string('\t', indentLevel); // Tab indentation

        sb.AppendLine(indent + "{");

        foreach (var kvp in dictionary)
        {
            string key = kvp.Key is string ? $"[\"{kvp.Key}\"]" : $"[{kvp.Key}]"; // format for preset number or key name

            if (kvp.Value is SortedDictionary<object, object> nestedDict)
            {
                // Recursively convert the nested table
                sb.Append(indent + $"\t{key} = \n{DictToLua(nestedDict, indentLevel + 1)},");
            }
            else
            {
                string value;

                if (kvp.Value is string)
                {
                    value = $"\"{kvp.Value}\""; // Enclose string values in quotes
                }
                else if (kvp.Value is bool)
                {
                    value = (bool)kvp.Value ? "true" : "false"; // Convert bools to Lua true/false
                }
                else
                {
                    value = kvp.Value.ToString();
                }

                sb.Append(indent + $"\t{key} = {value},");
            }

            sb.AppendLine();
        }

        sb.AppendLine(indent + "}");
        return sb;
    }

    public static string ConvertToLuaAndExportToFile(SortedDictionary<object, object> presets, string luaFile)
    {
        try
        {
            StringBuilder sb = DictToLua(presets);
            sb.Insert(0, "presets = ");
            string lua = sb.ToString();
            File.WriteAllText(luaFile, lua);
        }
        catch (Exception e)
        {
            return e.ToString();
        }
        return "Route exported successfully.";
    }

}