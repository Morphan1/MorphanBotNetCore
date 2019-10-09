using System;
using System.Collections;
using System.Collections.Generic;
using System.Text;

namespace MorphanBotNetCore.Util
{
    public class FactoryDictionary<K, V> : Dictionary<K, Func<V>>
    {
        public void Add(KeyValuePair<K, Func<V>> pair)
        {
            Add(pair.Key, pair.Value);
        }

        public bool TryCreate(K key, out V created)
        {
            if (TryGetValue(key, out Func<V> factory))
            {
                created = factory();
                return true;
            }
            created = default;
            return false;
        }
    }
}
