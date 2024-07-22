using System;

namespace IterTormenti.Utils.Sprites.Animations
{
    public class AnimationEventArgs : EventArgs
    {
        public const string ON_ANIMATION_END = "ON_ANIMATION_END";

        public AnimationEventArgs(string name)
        {
            Event = name;
        }
        
        public string Event { get; private set;}
        
        public string Name { get; set;}
    }
}
