using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Windows.Data.Json;

namespace FeedsNotifier
{
    static class JsonExtensions
    {
        // JsonArray extensions.

        public static bool ContainsStringValue(this JsonArray jsonArray, string value, out IJsonValue selectedValue)
        {
            selectedValue = null;

            foreach (IJsonValue jsonValue in jsonArray)
            {
                if (jsonValue.ValueType == JsonValueType.String)
                {
                    string currentValue = jsonValue.GetString();
                    if (String.Compare(value, currentValue, StringComparison.OrdinalIgnoreCase) == 0)
                    {
                        selectedValue = jsonValue;
                        return true;
                    }
                }
            }

            return false;
        }

        public static string[] ToStringArray(this JsonArray jsonArray)
        {
            List<string> values = new List<string>();

            foreach (IJsonValue jsonValue in jsonArray)
            {
                if (jsonValue.ValueType == JsonValueType.String)
                {
                    string value = jsonValue.GetString();
                    values.Add(value);
                }
            }

            return values.ToArray();
        }

        // JsonObject extensions.

        public static string GetNamedStringOrEmptyString(this JsonObject jsonObject, string name)
        {
            if (!jsonObject.ContainsKey(name))
            {
                return String.Empty;
            }
            return jsonObject.GetNamedString(name);
        }

        public static JsonArray GetNamedArrayOrEmptyArray(this JsonObject jsonObject, string name)
        {
            if (!jsonObject.ContainsKey(name))
            {
                return new JsonArray();
            }
            return jsonObject.GetNamedArray(name);
        }

        public static JsonArray GetOrCreateNamedArray(this JsonObject jsonObject, string name)
        {
            if (!jsonObject.ContainsKey(name))
            {
                jsonObject.SetNamedValue(name, new JsonArray());
            }
            return jsonObject.GetNamedArray(name);
        }

        public static string ConcatenateKeys(this JsonObject jsonObject)
        {
            return String.Join(", ", jsonObject.Keys);
        }
    }
}