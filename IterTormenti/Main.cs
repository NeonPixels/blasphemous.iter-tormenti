﻿using BepInEx;

namespace IterTormenti
{
    [BepInPlugin(ModInfo.MOD_ID, ModInfo.MOD_NAME, ModInfo.MOD_VERSION)]
    [BepInDependency("Blasphemous.ModdingAPI", "2.1.0")]
    [BepInDependency("Blasphemous.Framework.Penitence", "0.2.0")]
    public class Main : BaseUnityPlugin
    {
        public static Main Instance { get; private set; }
        public static IterTormenti IterTormenti { get; private set; }

        private void Start()
        {
            Instance = this;
            IterTormenti = new IterTormenti();
        }
    }
}
