using Blasphemous.ModdingAPI;
using Blasphemous.ModdingAPI.Persistence;
using Blasphemous.Framework.Penitence;
using System.Collections.Generic;
using Blasphemous.Framework.Levels;
using Blasphemous.Framework.Levels.Loaders;
using System.Collections;
using UnityEngine;
using Framework.Managers;

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
            ModLog.Info($"{ModInfo.MOD_NAME} has been initialized");
        }

        public SaveData SaveGame()
        {
            return new IterTormentiSaveData
            {
                config = GameSettings
            };
        }

        public void LoadGame(SaveData  data)
        {
            IterTormentiSaveData saveGameData = data as IterTormentiSaveData;

            GameSettings = saveGameData.config;                       
        }

        public void ResetGame()
        {
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

            provider.RegisterObjectCreator( "item-ground-custom",
                                            new ObjectCreator(
                                                new SceneLoader("D02Z02S14_LOGIC", "LOGIC/INTERACTABLES/ACT_Collectible"),
                                                new CustomGroundItemModifier() ) );
        }

        protected override void OnLevelPreloaded(string oldLevel, string newLevel)
        {
            if(newLevel == "D08Z01S01") // Bridge
            {
                Esdras.BossfightChanges.Apply();
            }
        }

        /// <summary>
        /// Kicks player out to main menu and displays message. Meant to
        /// interrupt gameplay and avoid undefined behaviour when a serious
        /// error happens.
        /// </summary>
        /// <param name="message">Error message to display</param>
        public void FatalError(string message)
        {
            if(_fatalErrorIssued)
            {
                ModLog.Error($"Fatal Error already issued, can't process error with message: {message}");
                return;
            }

            ModLog.Error($"Fatal Error issued: {message}");

            fatalErrorCoroutine = FatalErrorCoroutine(message);
            Main.Instance.StartCoroutine(fatalErrorCoroutine);
        }


        private bool _fatalErrorIssued = false;
        private IEnumerator fatalErrorCoroutine;

        /// <summary>
        /// Coroutine managing the handling of a fatal error. It will kick the player 
        /// back to the main menu, and display an error message.
        /// </summary>
        /// <param name="message">Error message to display</param>
        /// <returns>Coroutine IEnumerator</returns>
        private IEnumerator FatalErrorCoroutine(string message)
        {
            _fatalErrorIssued = true;

            yield return new WaitUntil(() => !Core.LevelManager.InsideChangeLevel);
            yield return new WaitForSecondsRealtime(0.05f);

            // Check if we're already in the MainMenu scene
            if(!Core.LevelManager.currentLevel.LevelName.Equals("MainMenu"))
            {
                Core.LevelManager.ChangeLevel("MainMenu");

                yield return new WaitUntil(() => !Core.LevelManager.InsideChangeLevel);
                yield return new WaitForSecondsRealtime(0.05f);
            }

            ModLog.Display(message);

            _fatalErrorIssued = false;
        }
    }
}
