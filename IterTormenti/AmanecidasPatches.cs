using HarmonyLib;
using Framework.Managers;
using Tools.Playmaker2.Action;

namespace IterTormenti.Amanecidas
{
    [HarmonyPatch(typeof(CheckGameModeActive), nameof(CheckGameModeActive.OnEnter))]
    class CheckGameModeActiveOnEnter_Patch
    {
        protected static string LOG_HEADER = nameof(CheckGameModeActive) + "::" + nameof(CheckGameModeActive.OnEnter) + "::Patch: ";

        /// <summary>
        /// This prefix will deceive FSMs asking for the current gamemode, by stating we're on
        /// NEW_GAME_PLUS when we're actually in NEW_GAME.
        /// The purpose is to make sure that the FSMs managing the Amanecidas questline work 
        /// properly, as they will be deactivated when not in NEW_GAME_PLUS.
        /// Unfortunately, this can cause a lot of overhead for all other checks for the
        /// current game state, as well as unpredictable behaviour for checks that should
        /// NOT be in NG+, as such, we will attempt to "fail" and return to the original method
        /// as early as possible by checking the specific conditions where this modification makes sense.        
        /// </summary>
        /// <param name="__instance">We need instance specific data</param>
        /// <returns>'true' to return to the original method, 'false' to skip it</returns>
        public static bool Prefix(CheckGameModeActive __instance)
        {           
            // Only care if the FSM is asking for NEW_GAME_PLUS
            if ( __instance.mode.Value != "NEW_GAME_PLUS" )
                return true;

            // Only perform these operations when playing in NEW_GAME
            if( Core.GameModeManager.GetCurrentGameMode() != GameModeManager.GAME_MODES.NEW_GAME )
                return true;

            // Check the identity of the FSM asking for the gamemode, and act accordingly
            switch( __instance.Owner.name )
            {
                case "LaudesTomb": // Manages the glass tomb breaking, should be enabled just like Jibrael
                case "Santos": // FSM that manages Jibrael encounters (Apparently, he was originally named Santos)
                {
                    // If Petrous is closed, we don't want to bother Jibrael
                    if( !Core.Events.GetFlag("SANTOS_DOOR_OPENED") ) return true;

                    // Jibrael's bell bushes and tomb remain after completing the quest, so we need 
                    // to override his checks forever

                    break;
                }
                case "JibraelCaveTrigger":
                case "JibraelCaveTriggerStop":
                {
                    // If the Player doesn't have the Petrified Bell there is no point in waking up
                    // the FSMs that control the bell vibrating
                    if( !Core.InventoryManager.IsQuestItemOwned("QI106") ) return true;

                    break;
                }
                case "Deosgracias":
                case "DeograciasControl":
                {
                    // Deogracias only checks NG+ to give Amanecidas lore, so once Jibrael has been spoken
                    // to for the first time, we keep checking for his actions
                    if( !Core.Events.GetFlag("SANTOS_FIRSTCONVERSATION_DONE") ) return true;

                    // TODO: Minor optimization, stop checking for Deogracias requests once his last conversation is done

                    break;
                }
                default:
                {
                    return true; // Who art thou? Begone! To the default implementation with you!
                }
            }

            // The Amanecidas & Laudes boss fights don't care if we're on NG+ or not

            __instance.Fsm.Event(__instance.modeIsActive); // Tell falsehoods to the FSM
            __instance.Finish();
            return false;
        }
    }
}
