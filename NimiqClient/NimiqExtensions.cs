using System.Linq;
using System.Text.Json;

namespace Nimiq
{
    // <summary>Class Extensions</summary>
    static public class Extensions
    {
        /// <summary>Convert a JsonElement into its underlying objects.</summary>
        public static object TryGetObject(this JsonElement jsonElement)
        {
            object result = null;

            switch (jsonElement.ValueKind)
            {
                case JsonValueKind.Null:
                    result = null;
                    break;
                case JsonValueKind.Number:
                    result = jsonElement.GetDouble();
                    break;
                case JsonValueKind.False:
                    result = false;
                    break;
                case JsonValueKind.True:
                    result = true;
                    break;
                case JsonValueKind.Undefined:
                    result = null;
                    break;
                case JsonValueKind.String:
                    result = jsonElement.GetString();
                    break;
                case JsonValueKind.Object:
                    result = jsonElement.EnumerateObject()
                        .ToDictionary(k => k.Name, p => TryGetObject(p.Value));
                    break;
                case JsonValueKind.Array:
                    result = jsonElement.EnumerateArray()
                        .Select(o => TryGetObject(o))
                        .ToArray();
                    break;
            }

            return result;
        }
    }
}
