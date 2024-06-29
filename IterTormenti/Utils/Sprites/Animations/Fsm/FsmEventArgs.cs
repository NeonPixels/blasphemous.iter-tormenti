using System;

namespace IterTormenti.Utils.Sprites.Animations.Fsm
{
    public class FsmEventArgs : EventArgs
    {
        public FsmEventArgs(string name)
        {
            Name = name;
            AnimationName = ANIMATION.NO_ANIMATION;
            Synched = true;
        }
        public string Name { get; private set;}
        public string AnimationName { get; set;}

        public bool Synched { get; set; }
    }
}