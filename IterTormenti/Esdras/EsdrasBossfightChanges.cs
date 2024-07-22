using Framework.Managers;

namespace IterTormenti.Esdras
{
    /// <summary>
    /// Asbtract class that manages all the changes to the Esdras Bossfight.
    /// </summary>
    public abstract class BossfightChanges
    {
        public static void Apply()
        {
            // We need to override FSM behaviour ONLY when the Esdras fight is to be skipped by the Incomplete Scapular,
            // as such, check first if the conditions match, and exit if not:

            // Has Esdras already been defeated?
            if( Core.Events.GetFlag("D08Z01S01_BOSSDEAD") )
            {
                return;
            }

            // Is Esdras already in the chapel?
            if( Core.Events.GetFlag("ESDRAS_CHAPEL") )
            {
                return;
            }

            // Do we have the scapular?
            if( !Core.InventoryManager.IsQuestItemOwned("QI203") )
            {
                return;
            }

            // Have the three wounds been acquired?
            if(    !Core.Events.GetFlag("D01Z06S01_BOSSDEAD")
                || !Core.Events.GetFlag("D02Z05S01_BOSSDEAD")
                || !Core.Events.GetFlag("D03Z04S01_BOSSDEAD") )
            {
                return;
            }

            // Create the animator for the transition between the Boss and the NPC
            // This animation will play upon boss defeat, replacing the boss sprite
            // It will remain in an idle state during the Requiem Aeternam animation,
            // and will be updated to the "WeaponPickup" state.
            // Once the "WeaponPickup" state is done, the animator will move to the
            // target position, where it will be disabled, and replaced with the NPC
            if(!DefeatAnimation.Create())
            {
                //HandleError();
                return;
            }

            // At this point, we are ready to start the modified encounter, so we modify the FSMs to directly go to the state we need.
            // We can skip any decision states that do not apply.

            // OBJECTIVE:
            // When the Penitent One reaches Esdras while in possesion of the Incomplete Scapular, the fight should proceed normally,
            // but after defeat, Esdras should transition into the animation where Perpetva reveals hersef, and the animation should
            // proceed normally.

            // GameObjects:
            // These are the GameObjects containing the FSMs that we need to modify, all of them contained in the 'D08Z01S01_LOGIC' scene:
            //  -LOGIC/SCRIPTS/EsdrasFightActivator:
            //      Active when entering the scene, its FSM will decide whether to enable/disable the other two GameObjects depending on conditions.
            //  -CHARACTERS/NPCs/EsdrasNPC:
            //      This manages all Esdras NPC interactions across the entire game. The FSM determines which animations/actions to perform based on active scene
            //      and conditions.
            //      This contains the animation frames used for Esdras reacting to Perpetvua (dropping his weapon, kneeling), so we can't just use the BossFight
            //      below, and need to use this.
            //  -CHARACTERS/BOSS_FIGHT_STUFF/BossFight:
            //      This manages the BossFight itself, playing the intro animations, activating the fight, and doing cleanup aftwerwards.


            // Modifications:
            //  -EsdrasFightActivator: Directly activate the BossFight and NPC gameobjects and associated elements (Arena boundaries and such)
            //  -BossFight: Initiate fight normally, but upon Esdras death, replace the boss sprite with the Animator and transition to the NPC.
            //  -EsdrasNPC: Hide the sprite intially, Reposition based on final Penitent position, jump directly to Perpetvua animation, perform normally, grant rewards.
            
            // Note: Make sure to do proper setup/cleanup of camera position, fight boundaries, input blockers, and completion flags.

            // Why are both the Bossfight and NPC activated at the start? So the FSMs are active, as we need the NPC to manage some features,
            // since once the Bossfight is disabled it won't be able to do so.
            

            if(!FightActivatorFsmPatch.Apply())
            {
                // HandleError();
                return;
            }

            if(!BossfightFsmPatch.Apply())
            {
                // HandleError();
                return;
            }

            if(!NpcFsmPatch.Apply())
            {
                //HandleError();
                return;
            }

            
        }

        // private static void HandleError()
        // {
        //      TODO: We need some manner of error handling to avoid corrupting saves
        //            if this setup fails
        // }
    }
}