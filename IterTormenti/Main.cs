using BepInEx;
using UnityEngine;

namespace IterTormenti
{
    [BepInPlugin(ModInfo.MOD_ID, ModInfo.MOD_NAME, ModInfo.MOD_VERSION)]
    [BepInDependency("Blasphemous.ModdingAPI", "2.1.0")]
    [BepInDependency("Blasphemous.Framework.Penitence", "0.1.0")]
    public class Main : BaseUnityPlugin
    {
        public static IterTormenti IterTormenti { get; private set; }

        private void Start()
        {
            IterTormenti = new IterTormenti();
        }
    }
}
