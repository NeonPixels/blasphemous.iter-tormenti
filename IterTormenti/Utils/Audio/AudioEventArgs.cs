using System;

namespace IterTormenti.Utils.Audio
{
    // TODO: extra audio parameters? Like duration, channels, etc?

    public class AudioEventArgs : EventArgs
    {
        public AudioEventArgs(string name = "")
        {
            Name = name;
        }

        public string Name { get; set; }
    }
}