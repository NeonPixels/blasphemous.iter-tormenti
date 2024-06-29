namespace IterTormenti.Utils.Sprites.Animations
{
    // TODO: Documentation
    public class Frame
    {
        public Frame(int index, float delay = -1.0f)
        {
            Index = index;
            Delay = delay;
        }

        public Frame(ref Frame source)
        {
            Clone(ref source);
        }

        public void Clone(ref Frame source)
        {
            Index = source.Index;
            Delay = source.Delay;
        }

        public int Index {get; private set;}
        public float Delay {get; private set;}

        public override string ToString()
        {
            return $"{{Index: {Index}, Delay: {Delay}}}";
        }
    }
}