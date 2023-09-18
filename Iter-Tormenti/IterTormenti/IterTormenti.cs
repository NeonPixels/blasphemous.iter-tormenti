using ModdingAPI;

namespace IterTormenti
{
    public class IterTormenti : PersistentMod
    {
        public override string PersistentID => "ID_ITER_TORMENTI";

        // Save file info
        public Config GameSettings { get; private set; }

        public IterTormenti() : base(PluginInfo.PLUGIN_GUID, PluginInfo.PLUGIN_NAME, PluginInfo.PLUGIN_VERSION)
        { }

        protected override void Initialize()
        {
            Log($"{PluginInfo.PLUGIN_NAME} has been initialized");
        }

        public override ModPersistentData SaveGame()
        {
            Log("SaveGame");

            return new IterTormentiPersistenceData
            {
                config = GameSettings
            };
        }

        public override void LoadGame(ModPersistentData data)
        {
            Log("LoadGame");

            IterTormentiPersistenceData persistenceData = data as IterTormentiPersistenceData;

            GameSettings = persistenceData.config;                       
        }

        public override void NewGame(bool NGPlus)
        {
            Log("NewGame { NGPlus: " + NGPlus + " }");
        }

        public override void ResetGame()
        {
            Log("ResetGame");

            GameSettings = new Config();
        }       
    }
}
