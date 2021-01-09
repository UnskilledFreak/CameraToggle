using System;
using System.Collections.Generic;
using System.Globalization;
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
                float _ => (float)Convert.ToDecimal(stringValue),
                _ => stringValue
            };
        }

        public string GetSaveStr()
        {
            return string.Concat(Key, "=", Value);
        }

        public override string ToString()
        {
            return Value.ToString();
        }
    }
}