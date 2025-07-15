using System;
using UnityEngine;

namespace Kandooz.InteractionSystem.Core
{
    public class TweenableFloat : ITweenable
    {
        public event Action<float> onChange;
        public event Action onFinished;
        private float start;
        private float target;
        private float value;
        private float rate;
        private float t;
        private VariableTweener lerper;
        public float Value
        {
            get
            {
                return value;
            }

            set
            {
#if UNITY_EDITOR
                if (UnityEditor.EditorApplication.isPlaying)
#endif
                {
                    t = 0;
                    start = this.value;
                    target = value;
                    lerper.AddTweenable(this);
                }
#if UNITY_EDITOR
                else
                {
                    this.value = value;
                    onChange(value);
                }
#endif
            }
        }

        public float Rate {  set => rate = value; }

        public TweenableFloat(VariableTweener lerper,Action<float> onChange=null, float rate = 2f, float value = 0)
        {
            start = target = this.value = value;
            this.rate = rate;
            this.t = 0;
            this.onChange = onChange;
            this.lerper = lerper;
        }


        public void Subscribe(Action<ITweenable> action)
        {
            throw new NotImplementedException();
        }

        public void Unsubscribe(Action<ITweenable> action)
        {
            throw new NotImplementedException();
        }

        public bool Tween(float scaledDetaTime)
        {
            t += rate * scaledDetaTime;
            this.value = Mathf.Lerp(start, target, t);
            onChange(value);

            if (t >= 1)
            {
                onFinished?.Invoke();
                return true;
            }
            return false;
        }

    }

}