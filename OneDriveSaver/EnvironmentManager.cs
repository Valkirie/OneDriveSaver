using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;

namespace OneDriveSaver
{
    public static class EnvironmentManager
    {
        static Dictionary<string, string> EnviromentVars = new Dictionary<string, string>();

        static EnvironmentManager()
        {
            SortedDictionary<string, string> TempVars = new SortedDictionary<string, string>();
            foreach (DictionaryEntry de in Environment.GetEnvironmentVariables())
            {
                string key = $"%{de.Key.ToString().ToLower()}%";
                string value = $"{de.Value.ToString().ToLower()}";

                if (value == "")
                    continue;

                if (!TempVars.ContainsKey(key))
                    TempVars.Add(key, value);
            }

            /* Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData);
            foreach (Environment.SpecialFolder special in (Environment.SpecialFolder[])Enum.GetValues(typeof(Environment.SpecialFolder)))
            {
                string key = $"%{special.ToString().ToLower()}%";
                string value = Environment.GetFolderPath(special).ToString().ToLower();

                if (value == "")
                    continue;

                if (!TempVars.ContainsKey(key))
                    TempVars.Add(key, value);
            } */

            var items = from pair in TempVars orderby pair.Value.Length descending select pair;

            foreach (KeyValuePair<string, string> pair in items)
                EnviromentVars.Add(pair.Key, pair.Value);

            return;
        }

        public static string ContractEnvironmentVariables(string path)
        {
            string filename = path.ToLower();

            foreach (KeyValuePair<string, string> item in EnviromentVars)
            {
                if (filename.Contains(item.Value))
                {
                    filename = filename.Replace(item.Value, item.Key);
                    break;
                }
            }

            return filename;
        }
    }
}
