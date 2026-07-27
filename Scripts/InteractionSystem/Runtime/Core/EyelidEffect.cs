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

        [Tooltip("How fast lid alpha reaches full black on partial (drowsy) closes. 1.5 = opaque lids by two thirds closed, so a clear slit is left instead of a grey haze.")]
        [Range(1f, 4f)]
        [SerializeField] private float partialLidOpacityGain = 1.5f;

        private Image _topImage;
        private Image _bottomImage;
        private int _animationVersion;
        private float _closedAmount;

        /// <summary>
        /// How closed the lids currently are, averaged across both lids.
        /// 0 = fully open, 1 = fully closed. Updated every animation frame so external
        /// effects (post-processing, audio) can follow the blink instead of snapping.
        /// </summary>
        public float ClosedAmount => _closedAmount;

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

        /// <summary>
        /// Animate lids to a partial closed amount — 0 fully open, 1 fully closed.
        /// Lids go opaque well before they meet, so a drowsy slit stays visible through the gap.
        /// </summary>
        public void SetPartial(float closedAmount, float duration = -1f, Action onComplete = null)
        {
            EnsureImagesInitialized();
            _ = RunTo(++_animationVersion, Mathf.Clamp01(closedAmount), duration < 0 ? defaultDuration : duration, onComplete);
        }

        /// <summary>Snap lids to a partial closed amount with no animation.</summary>
        public void SetPartialImmediate(float closedAmount)
        {
            EnsureImagesInitialized();
            _animationVersion++;
            closedAmount = Mathf.Clamp01(closedAmount);
            float alpha = PartialAlpha(closedAmount);
            ApplyState(closedAmount, closedAmount, alpha, alpha);
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

        private async Awaitable RunTo(int version, float target, float duration, Action onComplete)
        {
            bool closing = target >= _closedAmount;
            await AnimateLids(version, duration, closing ? closeEase : openEase, target, PartialAlpha(target), closing);
            if (version != _animationVersion) return;
            onComplete?.Invoke();
        }

        /// <summary>Lid alpha for a given closed amount — reaches full black before the lids meet.</summary>
        private float PartialAlpha(float closedAmount) =>
            Mathf.Clamp01(closedAmount * Mathf.Max(1f, partialLidOpacityGain));

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
        private async Awaitable AnimateLids(int version, float duration, EaseType ease, bool closing) =>
            await AnimateLids(version, duration, ease, closing ? 1f : 0f, closing ? 1f : 0f, closing);

        /// <summary>
        /// Drives one lid phase toward an arbitrary scale/alpha target, so partial (drowsy)
        /// states animate with the same split-lid timing as a full close or open.
        /// </summary>
        private async Awaitable AnimateLids(int version, float duration, EaseType ease, float scaleTarget, float alphaTarget, bool closing)
        {
            duration = Mathf.Max(0.01f, duration);
            float weight = Mathf.Max(0.01f, topLidSpeedWeight);
            float fadeScale = Mathf.Max(0.01f, 1f - fadeOffset);

            // Animate from the current live state so interrupted transitions blend naturally.
            float startTopScale = topLid ? topLid.localScale.y : 0f;
            float startBottomScale = bottomLid ? bottomLid.localScale.y : 0f;
            float startTopAlpha = _topImage ? _topImage.color.a : 0f;
            float startBottomAlpha = _bottomImage ? _bottomImage.color.a : 0f;

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
                    Mathf.Lerp(startTopScale, scaleTarget, topScaleT),
                    Mathf.Lerp(startBottomScale, scaleTarget, bottomScaleT),
                    useFade ? Mathf.Lerp(startTopAlpha, alphaTarget, topFadeT) : startTopAlpha,
                    useFade ? Mathf.Lerp(startBottomAlpha, alphaTarget, bottomFadeT) : startBottomAlpha);

                await Awaitable.NextFrameAsync();
                elapsed += Time.deltaTime;
            }

            if (this == null || version != _animationVersion) return;
            ApplyState(scaleTarget, scaleTarget,
                useFade ? alphaTarget : startTopAlpha,
                useFade ? alphaTarget : startBottomAlpha);
        }

        private void ApplyState(float topScale, float bottomScale, float topAlpha, float bottomAlpha)
        {
            _closedAmount = Mathf.Clamp01((topScale + bottomScale) * 0.5f);

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
