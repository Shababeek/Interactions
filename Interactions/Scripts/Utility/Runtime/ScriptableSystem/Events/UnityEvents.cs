using System;
using UnityEngine;
using UnityEngine.Events;

namespace Shababeek.Utilities
{
    /// <summary>
    /// UnityEvent that passes a float value.
    /// </summary>
    [Serializable]
    public class FloatUnityEvent : UnityEvent<float> { }
    
    /// <summary>
    /// UnityEvent that passes a Vector3 value.
    /// </summary>
    [Serializable]
    public class Vector3UnityEvent : UnityEvent<Vector3> { }
    
    /// <summary>
    /// UnityEvent that passes an int value.
    /// </summary>
    [Serializable]
    public class IntUnityEvent : UnityEvent<int> { }
    
    /// <summary>
    /// UnityEvent that passes a string value.
    /// </summary>
    [Serializable]
    public class StringUnityEvent : UnityEvent<string> { }
    
    /// <summary>
    /// UnityEvent that passes a Vector2 value.
    /// </summary>
    [Serializable]
    public class Vector2UnityEvent : UnityEvent<Vector2> { }
}