
namespace IterTormenti
{
    [System.Serializable]
    public class Config
    {
        // Main Settings
                
        public string VersionCreated { get; set; }

        // Create Config with default options
        public Config()
        {
            VersionCreated = ModInfo.MOD_VERSION;
        }
    }
}
