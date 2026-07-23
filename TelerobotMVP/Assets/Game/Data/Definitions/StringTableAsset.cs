using System;
using System.Collections.Generic;
using UnityEngine;

namespace Telerobot.Game.Data
{
    [Serializable]
    public sealed class StringEntry
    {
        public string key;
        [TextArea] public string value;
    }

    public sealed class StringTableAsset : ScriptableObject
    {
        public List<StringEntry> entries = new List<StringEntry>();

        public string Get(string key)
        {
            var entry = entries.Find(item => item.key == key);
            return entry == null ? key : entry.value;
        }
    }
}
