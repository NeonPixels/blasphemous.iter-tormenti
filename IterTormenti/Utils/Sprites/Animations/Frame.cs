using IterTormenti.Utils.Audio;

namespace IterTormenti.Utils.Sprites.Animations
{
    // TODO: Documentation
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

        public int Index {get; private set;}
        public float Delay {get; private set;}

        public AudioEventArgs Audio { get; set; }

        public override string ToString()
        {
            return $"{{Index: {Index}, Delay: {Delay}, Audio:{(null==Audio?"NULL":Audio.Name)}}}";
        }
    }
}