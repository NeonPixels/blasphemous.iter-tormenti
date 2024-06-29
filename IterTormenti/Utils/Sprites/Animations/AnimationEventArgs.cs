using System;

namespace IterTormenti.Utils.Sprites.Animations
{
    public class AnimationEventArgs : EventArgs
    {
        public AnimationEventArgs(string name)
        {
            Name = name;
        }
        public string Name { get; private set;}
    }
}