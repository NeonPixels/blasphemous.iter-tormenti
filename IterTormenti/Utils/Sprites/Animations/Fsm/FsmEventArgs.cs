using System;

namespace IterTormenti.Utils.Sprites.Animations.Fsm
{
    public class FsmEventArgs : EventArgs
    {
        public FsmEventArgs(string name)
        {
            Name = name;
            AnimationIndex = ANIMATION.INVALID_INDEX;
            Synched = true;
        }
        public string Name { get; private set;}
        public int AnimationIndex { get; set;}

        public bool Synched { get; set; }
    }
}