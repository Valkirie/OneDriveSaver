using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;

namespace OneDriveSaver
{
    public static class EnvironmentManager
    {
        static Dictionary<string, string> EnviromentVars = new Dictionary<string, string>(StringComparer.InvariantCultureIgnoreCase);

        static EnvironmentManager()
        {
            SortedDictionary<string, string> TempVars = new();
            foreach (DictionaryEntry de in Environment.GetEnvironmentVariables())
            {
                string key = $"%{de.Key.ToString()}%";
                string value = de.Value.ToString();

                if (value == "")
                    continue;

                if (IsDigitsOnly(value))
                    continue;

                if (!TempVars.ContainsKey(key))
                    TempVars.Add(key, value);
            }

            var items = from pair in TempVars orderby pair.Value.Length descending select pair;

            foreach (KeyValuePair<string, string> pair in items)
                EnviromentVars.Add(pair.Key, pair.Value);

            return;
        }

        public static string ContractEnvironmentVariables(string path)
        {
            string filename = path;

            foreach (KeyValuePair<string, string> item in EnviromentVars.Where(a => filename.Contains(a.Value, StringComparison.InvariantCultureIgnoreCase)))
            {
                filename = filename.Replace(item.Value, item.Key, StringComparison.InvariantCultureIgnoreCase);
                break;
            }

            return filename;
        }

        private static bool IsDigitsOnly(string str)
        {
            foreach (char c in str)
            {
                if (c < '0' || c > '9')
                    return false;
            }

            return true;
        }
    }
}
