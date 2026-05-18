using System;
using DG.Tweening;
using UnityEngine;
using UnityEngine.UI;

namespace Shababeek.Interactions.Core
{
    public class EyelidEffect : MonoBehaviour
    {
        [Header("Eyelid References")]
        [SerializeField] private RectTransform topLid;
        [SerializeField] private RectTransform bottomLid;

        [Header("Defaults")]
        [SerializeField] private float defaultDuration = 2f;
        [SerializeField] private Ease closeEase = Ease.InQuad;
        [SerializeField] private Ease openEase = Ease.OutQuad;

        [Header("Split Ratio")]
        [Tooltip("How much of the duration the top lid uses. 0.7 = top finishes 70% through, bottom at 100%.")]
        [Range(0f, 1f)]
        [SerializeField] private float topLidSpeedWeight = 0.7f;

        [Header("Fade")]
        [SerializeField] private bool useFade = true;
        [Tooltip("Close: fade reaches full black this fraction early (0.3 = done at 70% of duration). Open: stays black this fraction before fading out.")]
        [Range(0f, 0.9f)]
        [SerializeField] private float fadeOffset = 0.3f;

        private Image _topImage;
        private Image _bottomImage;
        private Sequence _activeSequence;

        private void Awake()
        {
            EnsureImagesInitialized();
        }

        /// <summary>Scale + fade lids in to cover the screen.</summary>
        public Sequence Close(float duration = -1f, Ease ease = Ease.Unset, Action onComplete = null)
        {
            duration = duration < 0 ? defaultDuration : duration;
            var e = ease == Ease.Unset ? closeEase : ease;
            float topTime = duration * topLidSpeedWeight;

            Kill();
            _activeSequence = DOTween.Sequence();

            _activeSequence.Append(TweenScaleY(topLid, 1f, topTime, e));
            _activeSequence.Insert(0, TweenScaleY(bottomLid, 1f, duration, e));

            if (useFade)
            {
                // fade completes early — full black before scale finishes
                _activeSequence.Insert(0, TweenAlpha(_topImage, 1f, topTime * (1f - fadeOffset), e));
                _activeSequence.Insert(0, TweenAlpha(_bottomImage, 1f, duration * (1f - fadeOffset), e));
            }

            if (onComplete != null)
                _activeSequence.OnComplete(() => onComplete());

            return _activeSequence;
        }

        /// <summary>Scale + fade lids out to reveal the screen.</summary>
        public Sequence Open(float duration = -1f, Ease ease = Ease.Unset, Action onComplete = null)
        {
            duration = duration < 0 ? defaultDuration : duration;
            var e = ease == Ease.Unset ? openEase : ease;
            float topTime = duration * topLidSpeedWeight;

            Kill();
            _activeSequence = DOTween.Sequence();

            _activeSequence.Append(TweenScaleY(topLid, 0f, topTime, e));
            _activeSequence.Insert(0, TweenScaleY(bottomLid, 0f, duration, e));

            if (useFade)
            {
                // stays full black for offset%, then fades out
                _activeSequence.Insert(topTime * fadeOffset, TweenAlpha(_topImage, 0f, topTime * (1f - fadeOffset), e));
                _activeSequence.Insert(duration * fadeOffset, TweenAlpha(_bottomImage, 0f, duration * (1f - fadeOffset), e));
            }

            if (onComplete != null)
                _activeSequence.OnComplete(() => onComplete());

            return _activeSequence;
        }

        /// <summary>Close then open — full blink.</summary>
        public Sequence Blink(float closeDuration = -1f, float holdDuration = 0f, float openDuration = -1f, Action onComplete = null)
        {
            closeDuration = closeDuration < 0 ? defaultDuration : closeDuration;
            openDuration = openDuration < 0 ? defaultDuration : openDuration;

            Kill();
            _activeSequence = DOTween.Sequence();

            _activeSequence.Append(Close(closeDuration).SetAutoKill(false));

            if (holdDuration > 0f)
                _activeSequence.AppendInterval(holdDuration);

            _activeSequence.Append(Open(openDuration).SetAutoKill(false));

            if (onComplete != null)
                _activeSequence.OnComplete(() => onComplete());

            return _activeSequence;
        }

        /// <summary>Snap lids to fully open (scaleY = 0, alpha = 0).</summary>
        public void SetOpen()
        {
            EnsureImagesInitialized();
            Kill();
            SetScaleY(topLid, 0f);
            SetScaleY(bottomLid, 0f);
            SetAlpha(_topImage, 0f);
            SetAlpha(_bottomImage, 0f);
        }

        /// <summary>Snap lids to fully closed (scaleY = 1, alpha = 1).</summary>
        public void SetClosed()
        {
            EnsureImagesInitialized();
            Kill();
            SetScaleY(topLid, 1f);
            SetScaleY(bottomLid, 1f);
            SetAlpha(_topImage, 1f);
            SetAlpha(_bottomImage, 1f);
        }

        private void EnsureImagesInitialized()
        {
            if (_topImage == null) _topImage = topLid.GetComponent<Image>();
            if (_bottomImage == null) _bottomImage = bottomLid.GetComponent<Image>();
        }

        private static Tweener TweenScaleY(RectTransform rt, float target, float duration, Ease ease) =>
            DOTween.To(
                () => rt.localScale.y,
                y => rt.localScale = new Vector3(rt.localScale.x, y, rt.localScale.z),
                target, duration
            ).SetEase(ease);

        private static Tweener TweenAlpha(Image img, float target, float duration, Ease ease) =>
            DOTween.To(
                () => img.color.a,
                a => img.color = new Color(img.color.r, img.color.g, img.color.b, a),
                target, duration
            ).SetEase(ease);

        private static void SetScaleY(RectTransform rt, float y) =>
            rt.localScale = new Vector3(rt.localScale.x, y, rt.localScale.z);

        private static void SetAlpha(Image img, float a)
        {
            if (img == null) return;
            img.color = new Color(img.color.r, img.color.g, img.color.b, a);
        }

        private void Kill() { _activeSequence?.Kill(); _activeSequence = null; }

        private void OnDestroy() => Kill();
    }
}
