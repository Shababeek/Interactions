using System.Collections.Generic;
using UnityEngine;

namespace Kandooz.InteractionSystem.Core
{
    public class VariableTweener : MonoBehaviour
    {
        public float tweenScale = 1f;
        private List<ITweenable> values= new();

        private void OnEnable()
        {
            values = new List<ITweenable>();
        }
        public void AddTweenable(ITweenable value)
        {
            values.Add(value);
        }

        public void RemoveTweenable(ITweenable value)
        {
            try
            {
                values.Remove(value);
            }
            catch
            {
                // ignored
            }
        }
        void Update()
        {
            for (int i = values.Count - 1; i >= 0; i--)
            {
                if (values[i].Tween(Time.deltaTime * tweenScale))
                {
                    values.RemoveAt(i);
                }
            }
        }
    }
}