using System;

namespace Shababeek.Interactions.Core
{
    /// <summary>
    /// Observes XR button state changes.
    /// </summary>
    public class XRButtonObserver : IObserver<VRButtonState>
    {
        //todo: rewrite to use uniRX
        private readonly Action onComplete;
        private readonly Action<Exception> onExceptionRaised;
        private readonly Action<VRButtonState> onButtonStateChanged;

        /// <summary>Called when observation is complete.</summary>
        public void OnCompleted() => onComplete();
        /// <summary>Called when an error occurs during observation.</summary>
        public void OnError(Exception error) => onExceptionRaised(error);
        /// <summary>Called when button state changes.</summary>
        public void OnNext(VRButtonState vrButtonState) => onButtonStateChanged(vrButtonState);

        /// <summary>Initializes the XR button observer with callback actions.</summary>
        public XRButtonObserver(Action<VRButtonState> onButtonStateChanged, Action onComplete, Action<Exception> onExceptionRaised)
        {
            this.onComplete = onComplete;
            this.onExceptionRaised = onExceptionRaised;
            this.onButtonStateChanged = onButtonStateChanged;
        }
    }
}