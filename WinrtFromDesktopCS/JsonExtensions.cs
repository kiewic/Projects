using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Windows.Data.Json;

namespace WinrtFromDesktopCS
{
    public static class JsonExtensions
    {
        public static void AddStringValue(this JsonObject jsonObject, string key, string value)
        {
            jsonObject.Add(key, JsonValue.CreateStringValue(value));
        }
    }
}
