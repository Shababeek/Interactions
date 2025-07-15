using System;

namespace Shababeek.Interactions.Core
{
    /// <summary>
    /// Observes XR button state changes and provides callbacks for button events.
    /// </summary>
    public class XRButtonObserver : IObserver<ButtonState>
    {
        //todo: rewrite to use uniRX
        private readonly Action onComplete;
        private readonly Action<Exception> onExceptionRaised;
        private readonly Action<ButtonState> onButtonStateChanged;

        public void OnCompleted() => onComplete();
        public void OnError(Exception error) => onExceptionRaised(error);
        public void OnNext(ButtonState buttonState) => onButtonStateChanged(buttonState);

        public XRButtonObserver(Action<ButtonState> onButtonStateChanged, Action onComplete, Action<Exception> onExceptionRaised)
        {
            this.onComplete = onComplete;
            this.onExceptionRaised = onExceptionRaised;
            this.onButtonStateChanged = onButtonStateChanged;
        }
    }
}