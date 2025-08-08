using System;
using UnityEngine;
using UnityEngine.Events;

namespace Shababeek.Core
{
    [Serializable]
    public class FloatUnityEvent : UnityEvent<float>{}
    [Serializable]
    public class Vector3UnityEvent : UnityEvent<Vector3>{}
    [Serializable]
    public class IntUnityEvent : UnityEvent<int>{}
    [Serializable]
    public class StringUnityEvent : UnityEvent<string> { }
    [Serializable]
    public class     Vector2UnityEvent : UnityEvent<Vector2>{}

}