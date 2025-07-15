using System;
using UnityEngine;
using UnityEngine.Events;

namespace Shababeek.Core
{
    [Serializable]
    public class FloatUnityEvent : UnityEvent<float>
    {
    }
    [Serializable]
    public class Vector3UnityEvent : UnityEvent<Vector3>
    {
    }
}