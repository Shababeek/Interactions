using System;
using System.Collections.Generic;
using UniRx;
using UnityEngine;

namespace Shababeek.Utilities
{
    /// <summary>
    /// Scriptable variable that stores a list of strings.
    /// Useful for dialogue options, item names, tags, or any list of text values.
    /// </summary>
    [CreateAssetMenu(menuName = "Shababeek/Scriptable System/Variables/StringListVariable")]
    public class StringListVariable : ScriptableVariable<List<string>>
    {
        
        public override string ToString()
        {
            return $"StringListVariable({value.Count} items)";
        }
    }
}
