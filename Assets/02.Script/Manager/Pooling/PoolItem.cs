using System.Collections.Generic;
using UnityEngine;

namespace Pooling
{
    [System.Serializable]
    public class PoolItem
    {
        public string Key;
        public List<GameObject> Items = new List<GameObject>(); // Ensure there's an initializer

        // Constructor isn't strictly necessary for serialization, but can be useful for other operations
        public PoolItem(string key)
        {
            this.Key = key;
        }
    }
}