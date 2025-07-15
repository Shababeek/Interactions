using Kandooz.ScriptableSystem;

namespace Kandooz.Kuest
{
    public enum SequenceStatus
    {
        Inactive,
        Started,
        Completed
    }
    public abstract class SequenceNode : GameEvent<SequenceStatus>
    {
        public abstract void Begin();

    }
}