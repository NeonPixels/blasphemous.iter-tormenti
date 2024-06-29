namespace IterTormenti.Utils.Sprites.Animations.Fsm
{
    // TODO: Documentation
    public class Transition
    {
        public Transition(string eventName, string targetState)
        {
            Event = eventName;
            TargetState = targetState;
        }

        public Transition(Transition source)
        {
            Clone(source);
        }

        public void Clone(Transition source)
        {
            Event = source.Event;
            TargetState = source.TargetState;
        }

        public string Event { get; private set; }

        public string TargetState { get; private set;}
    }
}