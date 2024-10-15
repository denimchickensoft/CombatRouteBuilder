using NLua;
using System.IO;
using System.Diagnostics;
namespace CRB;

static class LuaHandler
{
    public static Dictionary<object,object> GetLuaTable(string luaFilePath)
    {
        // Path to the .lua file

    // Read the contents of the .lua file
    string luaScript = File.ReadAllText(luaFilePath);

    // Initialize Lua interpreter
    using (Lua lua = new Lua())
    {
        // Run the Lua script
        lua.DoString(luaScript);

        // Assume the Lua table is named 'myTable' in the Lua script
        var luaTable = lua["presets"] as LuaTable;

        // Check if the Lua table exists
        if (luaTable != null)
        {
            // Convert the Lua table to a C# dictionary (including nested tables)
            var dictionary = ConvertLuaTableToDictionary(luaTable);
                // Output the dictionary
            //PrintDictionary(dictionary);
            return dictionary;
        }
        else
        {
            Debug.WriteLine("No Lua table named 'myTable' was found in the script.");
            return null;
        }
    }
    }


    // Function to recursively convert a LuaTable to a Dictionary
    static Dictionary<object, object> ConvertLuaTableToDictionary(LuaTable luaTable)
    {
        Dictionary<object, object> dictionary = new Dictionary<object, object>();

        foreach (var key in luaTable.Keys)
        {
            var value = luaTable[key];

            // Check if the value is another LuaTable (nested table)
            if (value is LuaTable)
            {
                // Recursively convert the nested table
                dictionary[key] = ConvertLuaTableToDictionary(value as LuaTable);
            }
            else
            {
                // Add the key-value pair to the dictionary
                dictionary[key] = value;
            }
        }

        return dictionary;
    }

    // Function to print the dictionary (including nested dictionaries)
    public static void PrintDictionary(Dictionary<object, object> dictionary, int level = 0)
    {
        foreach (var item in dictionary)
        {
            // Indent based on the level of nesting
            string indent = new string(' ', level * 4);

            if (item.Value is Dictionary<object, object>)
            {
                Debug.WriteLine($"{indent}Key: {item.Key}, Value: (nested table)");
                // Recursively print the nested dictionary
                PrintDictionary(item.Value as Dictionary<object, object>, level + 1);
            }
            else
            {
                Debug.WriteLine($"{indent}Key: {item.Key}, Value: {item.Value}");
            }
        }
    }

    static class Presets{

    }
}

