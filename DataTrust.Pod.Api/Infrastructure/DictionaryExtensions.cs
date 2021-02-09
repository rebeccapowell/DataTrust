using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace DataTrust.Pod.Api.Infrastructure
{
    public static class DictionaryExtensions
    {
        // Works in C#3/VS2008:
        public static void Merge<K, V>(this IDictionary<K, V> target, IDictionary<K, V> source, bool overwrite = false)
        {
            source.ToList().ForEach(_ => {
                if ((!target.ContainsKey(_.Key)) || overwrite)
                    target[_.Key] = _.Value;
            });
        }
    }
}
