using System;
using ModdingAPI;

namespace IterTormenti
{
    [Serializable]
    public class IterTormentiPersistenceData : ModPersistentData
    {
        public IterTormentiPersistenceData() : base("ID_ITER_TORMENTI")
        { }

        public Config config;
    }
}
