using IterTormenti.Utils.Audio;

namespace IterTormenti.Utils.Sprites.Animations
{
    /// <summary>
    /// Information on an sprite animation frame.
    /// </summary>
    public class Frame
    {
        public Frame(int index, float delay = -1.0f)
        {
            Index = index;
            Delay = delay;
            Audio = null;
        }

        public Frame(ref Frame source)
        {
            Clone(ref source);
        }

        public void Clone(ref Frame source)
        {
            Index = source.Index;
            Delay = source.Delay;
            Audio = source.Audio;
        }

        /// <summary>
        /// Index of the sprite to use.
        /// </summary>
        public int Index {get; private set;}
        
        /// <summary>
        /// Delay, in seconds, before the next frame.
        /// Negative values indicate the field is not used, and the
        /// default delay defined in the animation will be used instead.
        /// Default: -1.0f
        /// </summary>
        public float Delay {get; private set;}

        /// <summary>
        /// Audio event to issue, leave empty for no audio.
        /// </summary>
        public AudioEventArgs Audio { get; set; }

        public override string ToString()
        {
            return $"{{Index: {Index}, Delay: {Delay}, Audio:{(null==Audio?"NULL":Audio.Name)}}}";
        }
    }
}
