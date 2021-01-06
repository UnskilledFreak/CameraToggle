using System;
using System.Collections.Generic;
using System.Linq;

namespace Mover
{
    public class ConfigProperty<T>
    {
        private string Key { get; }
        public dynamic Value { get; set; }
        
        public ConfigProperty(string key, T defaultValue)
        {
            Key = key;
            Value = defaultValue;
        }

        public void LoadFromStr(IEnumerable<string[]> file)
        {
            var stringValue = file.FirstOrDefault(x => x[0] == Key)?[1];

            if (stringValue == null)
            {
                throw new Exception($"could not find {Key} in config file!");
            }

            Value = Value switch
            {
                bool _ => stringValue.ToLower() == "true",
                int _ => Convert.ToInt32(stringValue),
                float _ => Convert.ToDecimal(stringValue),
                _ => stringValue
            };
        }

        public string GetSaveStr()
        {
            var propString = Value switch
            {
                bool property => property ? "True" : "False",
                string property2 => string.Concat("\"", property2, "\""),
                _ => Value.ToString()
            };
            
            return string.Concat(Key, "=", propString);
        }

        public override string ToString()
        {
            return Value.ToString();
        }
    }
}