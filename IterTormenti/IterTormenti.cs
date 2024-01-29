using Blasphemous.ModdingAPI;
using Blasphemous.ModdingAPI.Persistence;

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
    }
}
