using System;
using System.Collections.Generic;

namespace FiveRP.Gamemode.Library
{
    public static class Config
    {
        public static Dictionary<string, string> ConfigDictionary = new Dictionary<string, string>();

        private static bool _loadedDictionary = false;

        public static void LoadConfig(string file)
        {
            try
            {
                var lines = System.IO.File.ReadAllLines(file);

                string key, value;

                // Clear the ConfigDictionary dictionary, just in case
                ClearConfigDictionaryDictionary();

                foreach (var line in lines)
                {
                    if (line.IndexOf("=") >= 0 && !line.StartsWith("//") && line.StartsWith("#"))
                    {
                        key = line.Substring(0, line.IndexOf("="));
                        value = line.Substring(line.IndexOf("=") + 1,
                            line.Length - line.IndexOf("=") - 1).Replace("\\n", Environment.NewLine);
                        ConfigDictionary.Add(key, value);
                    }
                }

                _loadedDictionary = true;
            } catch(Exception ex)
            {
                Logging.LogError("Config failed to load " + file + " - EX: " + ex);
                Logging.Log("Make sure there is a file named server.cfg in your GTANetworkServer.exe directory.", ConsoleColor.Green);
            }
        }

        public static void ClearConfigDictionaryDictionary()
        {
            ConfigDictionary.Clear();

            _loadedDictionary = false;
        }

        public static string GetKeyString(string key)
        {
            if (!_loadedDictionary)
            {
                Logging.LogError("Config dictionary not loaded!");
                return key;
            }

            if (ConfigDictionary.ContainsKey(key) == true)
            {
                return ConfigDictionary[key];
            }
            else
            {
                Logging.LogError("Value for given key was not found: " + key);
                return "";
            }
        }

        public static int GetKeyInt(string key)
        {
            var akey = 0;
            if (!_loadedDictionary)
            {
                Logging.LogError("Config dictionary not loaded!");
                return akey;
            }

            if (ConfigDictionary.ContainsKey(key) == true)
            {
                akey = int.Parse(ConfigDictionary[key]);
                return akey;
            }
            else
            {
                Logging.LogError("Value for given key was not found: " + key);
                return -1;
            }
        }
    }
}
