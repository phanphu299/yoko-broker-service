using System.Collections.Generic;
using System.Dynamic;

namespace AHI.Broker.Function.Extension
{
    public static class DictionaryExtensions
    {
        public static dynamic ToDynamicObject(this IDictionary<string, object> dict)
        {
            var eo = new ExpandoObject();
            var eoColl = (ICollection<KeyValuePair<string, object>>)eo;

            foreach (var kvp in dict)
            {
                eoColl.Add(kvp);
            }
            return eo;
        }
    }
}