using System;
using System.Threading;
using UnityEngine;
using UnityEngine.XR;

namespace Shababeek.Interactions
{
    /// <summary>
    /// Plays HapticPattern curves by sampling them at a fixed interval and forwarding impulses
    /// to a sender. Stateless; callers own cancellation (typically destroyCancellationToken).
    /// </summary>
    public static class HapticPatternPlayer
    {
        private const float StepInterval = 0.02f;
        private const float ImpulseOverlap = 1.5f;

        /// <summary>
        /// Plays a pattern through the given impulse sender (amplitude, duration).
        /// Short patterns collapse to a single impulse.
        /// </summary>
        public static async Awaitable Play(HapticPattern pattern, Action<float, float> sendImpulse,
            CancellationToken cancellationToken = default)
        {
            if (pattern == null || sendImpulse == null) return;

            float duration = pattern.Duration;
            if (duration <= StepInterval * ImpulseOverlap)
            {
                sendImpulse(pattern.Evaluate(0f), Mathf.Max(duration, 0.01f));
                return;
            }

            float elapsed = 0f;
            while (elapsed < duration)
            {
                cancellationToken.ThrowIfCancellationRequested();
                float amp = pattern.Evaluate(elapsed / duration);
                // Impulses slightly outlast the sample interval so consecutive samples
                // blend without audible/tactile gaps.
                if (amp > 0.001f) sendImpulse(amp, StepInterval * ImpulseOverlap);
                await Awaitable.WaitForSecondsAsync(StepInterval, cancellationToken);
                elapsed += StepInterval;
            }
        }

        /// <summary>Plays a pattern on the controller at the given XR node.</summary>
        public static async Awaitable PlayOnNode(HapticPattern pattern, XRNode node,
            CancellationToken cancellationToken = default)
        {
            await Play(pattern, (amp, dur) =>
            {
                var device = InputDevices.GetDeviceAtXRNode(node);
                if (device.isValid) device.SendHapticImpulse(0, amp, dur);
            }, cancellationToken);
        }
    }
}
