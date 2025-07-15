using System;

namespace Kandooz.InteractionSystem.Core
{
    public class XRButtonObserver : IObserver<ButtonState>
    {
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