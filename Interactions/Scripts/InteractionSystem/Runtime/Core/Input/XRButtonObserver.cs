using System;

namespace Shababeek.Interactions.Core
{
    /// <summary>
    /// Observes XR button state changes and provides callbacks for button events.
    /// </summary>
    public class XRButtonObserver : IObserver<VRButtonState>
    {
        //todo: rewrite to use uniRX
        private readonly Action onComplete;
        private readonly Action<Exception> onExceptionRaised;
        private readonly Action<VRButtonState> onButtonStateChanged;

        public void OnCompleted() => onComplete();
        public void OnError(Exception error) => onExceptionRaised(error);
        public void OnNext(VRButtonState vrButtonState) => onButtonStateChanged(vrButtonState);

        public XRButtonObserver(Action<VRButtonState> onButtonStateChanged, Action onComplete, Action<Exception> onExceptionRaised)
        {
            this.onComplete = onComplete;
            this.onExceptionRaised = onExceptionRaised;
            this.onButtonStateChanged = onButtonStateChanged;
        }
    }
}