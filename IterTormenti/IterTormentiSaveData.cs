using System;
using Blasphemous.ModdingAPI.Persistence;

namespace IterTormenti
{
    [Serializable]
    public class IterTormentiSaveData : SaveData
    {
        public IterTormentiSaveData() : base("ID_ITER_TORMENTI")
        { }

        public Config config;
    }
}
