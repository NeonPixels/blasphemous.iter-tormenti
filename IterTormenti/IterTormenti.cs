using Blasphemous.ModdingAPI;
using Blasphemous.ModdingAPI.Persistence;
using Blasphemous.Framework.Penitence;
using System.Collections.Generic;

namespace IterTormenti
{
    public class IterTormenti : BlasMod, IPersistentMod
    {
        public string PersistentID => "ID_ITER_TORMENTI";

        // Save file info
        public Config GameSettings { get; private set; }

        public IterTormenti() : base(ModInfo.MOD_ID, ModInfo.MOD_NAME, ModInfo.MOD_AUTHOR, ModInfo.MOD_VERSION)
        { }

        protected override void OnInitialize()
        {
            LocalizationHandler.RegisterDefaultLanguage("es");
            Log($"{ModInfo.MOD_NAME} has been initialized");
        }

        public SaveData SaveGame()
        {
            Log("SaveGame");

            return new IterTormentiSaveData
            {
                config = GameSettings
            };
        }

        public void LoadGame(SaveData  data)
        {
            Log("LoadGame");

            IterTormentiSaveData saveGameData = data as IterTormentiSaveData;

            GameSettings = saveGameData.config;                       
        }

        public void ResetGame()
        {
            Log("ResetGame");

            GameSettings = new Config();
        }

        public List<ComboPenitence> ComboPenitenceList { get; } = new(){
            new PenitenceAB(),
            new PenitenceBC(),
            new PenitenceCA(),
            new PenitenceABC()
        };

        protected override void OnRegisterServices(ModServiceProvider provider)
        {
            foreach (ComboPenitence penitence in ComboPenitenceList)
            {
                provider.RegisterPenitence(penitence);
            }
        }

        protected override void OnLevelPreloaded(string oldLevel, string newLevel)
        {
            if(newLevel == "D08Z01S01") // Bridge
            {
                Esdras.BossfightChanges.Apply();
            }
        }
    }
}
