using System;
using UnityEngine;
using UnityEngine.UI;

namespace Shababeek.Interactions.Core
{
    /// <summary>
    /// Eyelid blink overlay: scales and fades two lid images to cover/reveal the screen.
    /// Self-contained Awaitable animation — no external tween library required.
    /// </summary>
    public class EyelidEffect : MonoBehaviour
    {
        /// <summary>Easing applied to lid animation progress.</summary>
        public enum EaseType
        {
            Linear,
            InQuad,
            OutQuad,
            InOutQuad
        }

        [Header("Eyelid References")]
        [Tooltip("Top eyelid rect; scaled on Y and faded.")]
        [SerializeField] private RectTransform topLid;

        [Tooltip("Bottom eyelid rect; scaled on Y and faded.")]
        [SerializeField] private RectTransform bottomLid;

        [Header("Defaults")]
        [Tooltip("Default duration in seconds for close/open when none is passed.")]
        [SerializeField] private float defaultDuration = 2f;

        [Tooltip("Easing used when closing the lids.")]
        [SerializeField] private EaseType closeEase = EaseType.InQuad;

        [Tooltip("Easing used when opening the lids.")]
        [SerializeField] private EaseType openEase = EaseType.OutQuad;

        [Header("Split Ratio")]
        [Tooltip("How much of the duration the top lid uses. 0.7 = top finishes 70% through, bottom at 100%.")]
        [Range(0f, 1f)]
        [SerializeField] private float topLidSpeedWeight = 0.7f;

        [Header("Fade")]
        [Tooltip("Whether to fade lid alpha alongside the scale animation.")]
        [SerializeField] private bool useFade = true;

        [Tooltip("Close: fade reaches full black this fraction early (0.3 = done at 70% of duration). Open: stays black this fraction before fading out.")]
        [Range(0f, 0.9f)]
        [SerializeField] private float fadeOffset = 0.3f;

        private Image _topImage;
        private Image _bottomImage;
        private int _animationVersion;

        private void Awake()
        {
            EnsureImagesInitialized();
        }

        /// <summary>Scale + fade lids in to cover the screen.</summary>
        public void Close(float duration = -1f, Action onComplete = null)
        {
            EnsureImagesInitialized();
            _ = RunClose(++_animationVersion, duration < 0 ? defaultDuration : duration, onComplete);
        }

        /// <summary>Scale + fade lids out to reveal the screen.</summary>
        public void Open(float duration = -1f, Action onComplete = null)
        {
            EnsureImagesInitialized();
            _ = RunOpen(++_animationVersion, duration < 0 ? defaultDuration : duration, onComplete);
        }

        /// <summary>Close then open — full blink, with an optional hold while fully closed.</summary>
        public void Blink(float closeDuration = -1f, float holdDuration = 0f, float openDuration = -1f, Action onComplete = null)
        {
            EnsureImagesInitialized();
            _ = RunBlink(++_animationVersion,
                closeDuration < 0 ? defaultDuration : closeDuration,
                holdDuration,
                openDuration < 0 ? defaultDuration : openDuration,
                onComplete);
        }

        /// <summary>Snap lids to fully open (scaleY = 0, alpha = 0).</summary>
        public void SetOpen()
        {
            EnsureImagesInitialized();
            _animationVersion++;
            ApplyState(0f, 0f, 0f, 0f);
        }

        /// <summary>Snap lids to fully closed (scaleY = 1, alpha = 1).</summary>
        public void SetClosed()
        {
            EnsureImagesInitialized();
            _animationVersion++;
            ApplyState(1f, 1f, 1f, 1f);
        }

        private async Awaitable RunClose(int version, float duration, Action onComplete)
        {
            await AnimateLids(version, duration, closeEase, closing: true);
            if (version != _animationVersion) return;
            onComplete?.Invoke();
        }

        private async Awaitable RunOpen(int version, float duration, Action onComplete)
        {
            await AnimateLids(version, duration, openEase, closing: false);
            if (version != _animationVersion) return;
            onComplete?.Invoke();
        }

        private async Awaitable RunBlink(int version, float closeDuration, float holdDuration, float openDuration, Action onComplete)
        {
            await AnimateLids(version, closeDuration, closeEase, closing: true);
            if (version != _animationVersion) return;

            if (holdDuration > 0f)
            {
                await Awaitable.WaitForSecondsAsync(holdDuration);
                if (version != _animationVersion || this == null) return;
            }

            await AnimateLids(version, openDuration, openEase, closing: false);
            if (version != _animationVersion) return;

            onComplete?.Invoke();
        }

        /// <summary>
        /// Drives one close or open phase. The top lid completes at topLidSpeedWeight of the
        /// duration, the bottom at the full duration; fades finish early (close) or start late
        /// (open) by fadeOffset, matching the original behavior.
        /// </summary>
        private async Awaitable AnimateLids(int version, float duration, EaseType ease, bool closing)
        {
            duration = Mathf.Max(0.01f, duration);
            float weight = Mathf.Max(0.01f, topLidSpeedWeight);
            float fadeScale = Mathf.Max(0.01f, 1f - fadeOffset);

            // Animate from the current live state so interrupted transitions blend naturally.
            float startTopScale = topLid ? topLid.localScale.y : 0f;
            float startBottomScale = bottomLid ? bottomLid.localScale.y : 0f;
            float startTopAlpha = _topImage ? _topImage.color.a : 0f;
            float startBottomAlpha = _bottomImage ? _bottomImage.color.a : 0f;
            float target = closing ? 1f : 0f;

            float elapsed = 0f;
            while (elapsed < duration)
            {
                if (this == null || version != _animationVersion) return;

                float p = elapsed / duration;

                float topScaleT = Evaluate(ease, Mathf.Clamp01(p / weight));
                float bottomScaleT = Evaluate(ease, p);

                float topFadeT, bottomFadeT;
                if (closing)
                {
                    // Fade completes early — full black before the scale finishes.
                    topFadeT = Evaluate(ease, Mathf.Clamp01(p / (weight * fadeScale)));
                    bottomFadeT = Evaluate(ease, Mathf.Clamp01(p / fadeScale));
                }
                else
                {
                    // Stays full black for the offset fraction, then fades out.
                    topFadeT = Evaluate(ease, Mathf.Clamp01((p - weight * fadeOffset) / (weight * fadeScale)));
                    bottomFadeT = Evaluate(ease, Mathf.Clamp01((p - fadeOffset) / fadeScale));
                }

                ApplyState(
                    Mathf.Lerp(startTopScale, target, topScaleT),
                    Mathf.Lerp(startBottomScale, target, bottomScaleT),
                    useFade ? Mathf.Lerp(startTopAlpha, target, topFadeT) : startTopAlpha,
                    useFade ? Mathf.Lerp(startBottomAlpha, target, bottomFadeT) : startBottomAlpha);

                await Awaitable.NextFrameAsync();
                elapsed += Time.deltaTime;
            }

            if (this == null || version != _animationVersion) return;
            ApplyState(target, target, useFade ? target : startTopAlpha, useFade ? target : startBottomAlpha);
        }

        private void ApplyState(float topScale, float bottomScale, float topAlpha, float bottomAlpha)
        {
            if (topLid) SetScaleY(topLid, topScale);
            if (bottomLid) SetScaleY(bottomLid, bottomScale);
            SetAlpha(_topImage, topAlpha);
            SetAlpha(_bottomImage, bottomAlpha);
        }

        private static float Evaluate(EaseType ease, float t)
        {
            t = Mathf.Clamp01(t);
            return ease switch
            {
                EaseType.InQuad => t * t,
                EaseType.OutQuad => t * (2f - t),
                EaseType.InOutQuad => t < 0.5f ? 2f * t * t : 1f - 2f * (1f - t) * (1f - t),
                _ => t
            };
        }

        private void EnsureImagesInitialized()
        {
            if (_topImage == null && topLid) _topImage = topLid.GetComponent<Image>();
            if (_bottomImage == null && bottomLid) _bottomImage = bottomLid.GetComponent<Image>();
        }

        private static void SetScaleY(RectTransform rt, float y) =>
            rt.localScale = new Vector3(rt.localScale.x, y, rt.localScale.z);

        private static void SetAlpha(Image img, float a)
        {
            if (img == null) return;
            img.color = new Color(img.color.r, img.color.g, img.color.b, a);
        }

        private void OnDestroy()
        {
            _animationVersion++;
        }
    }
}
