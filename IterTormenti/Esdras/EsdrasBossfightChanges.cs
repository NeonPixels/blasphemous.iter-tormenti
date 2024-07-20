namespace IterTormenti.Esdras
{
    /// <summary>
    /// TODO
    /// </summary>
    public abstract class BossfightChanges
    {
        public static void Apply()
        {
            // We need to override FSM behaviour ONLY when the Esdras fight is to be skipped by the Incomplete Scapular,
            // as such, check first if the conditions match, and exit if not:

#if DISABLED_FOR_TESTING // TODO: Re-enable
            // Esdras already down?
            if( Core.Events.GetFlag("D08Z01S01_BOSSDEAD") )
            {
                Main.IterTormenti.Log("IterTormenti.Esdras.FSMChanges.Apply: Esdras has already been defeated! Ending execution");
                return;
            }

            // Is Esdras already in the chapel?
            if( Core.Events.GetFlag("ESDRAS_CHAPEL") )
            {
                Main.IterTormenti.Log("IterTormenti.Esdras.FSMChanges.Apply: Esdras is already in the Chapel! Ending execution");
                return;
            }

            // Do we have the scapular?
            if( !Core.InventoryManager.IsQuestItemOwned("QI203") )
            {
                Main.IterTormenti.Log("IterTormenti.Esdras.FSMChanges.Apply: Scapular not owned! Ending execution");
                return;
            }

            // Have the three wounds been acquired?
            if(    !Core.Events.GetFlag("D01Z06S01_BOSSDEAD")
                || !Core.Events.GetFlag("D02Z05S01_BOSSDEAD")
                || !Core.Events.GetFlag("D03Z04S01_BOSSDEAD") )
            {
                Main.IterTormenti.Log("IterTormenti.Esdras.FSMChanges.Apply: Esdras can't be fought yet! Ending execution");
                return;
            }
#endif // DISABLED_FOR_TESTING

            // Create the animator for the transition between the Boss and the NPC
            // This animation will play upon boss defeat, replacing the boss sprite
            // It will remain in an idle state during the Requiem Aeternam animation,
            // and will be updated to the "WeaponPickup" state.
            // Once the "WeaponPickup" state is done, the animator will be disabled, and replaced
            // with the NPC
            if(!DefeatAnimation.Create())
            {
                return;
            }

            // At this point, we are ready to start the modified encounter, so we modify the FSMs to directly go to the state we need.
            // We can skip any decision states that do not apply.

            // OBJECTIVE:
            // When the Penitent One reaches Esdras, he should speak his introductory dialog, and perform his taunt (slamming his weapon).
            // Once done, a dialog choice should pop up asking if the player wants to show the Incomplete Scapular or not.
            // If the player assents, the fight is skipped.
            // If the player dissents, the fight starts, and upon Esdras being defeated, he remains stunned, and the Requiem Aeternam message is displayed.
            // Afterwards, regardless of choice, the animation where Esdras reacts to Perpetva plays normally, and items are awarded.

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
            //  -EsdrasFightActivator: Directly activate the BossFight gameobject and associated elements (Arena boundaries and such)
            //  -BossFight: After the taunt is performed, display dialog choice. If player assents, disable BossFight and enable EsdrasNPC.
            //              If player dissents, continue fight normally. Upon Esdras death, display Requien message, move EsdrasNPC to the same position as
            //              the boss, and move the Scapular Shine object to the same position as the Penitent, then disable BossFight and enable EsdrasNPC.
            //  -EsdrasNPC: Jump directly to Perpetvua animation, perform normally, grant rewards.
            
            // Note: Make sure to do proper setup/cleanup of camera position, fight boundaries, input blockers, and completion flags.

            // Why not activate EsdrasNPC first, and only activate the bossfight if needed? Mostly convenience. If we do that, we would need to track
            // when Esdras is defeated so upon reactivating EsdrasNPC we know which state to jump to.
            // I think it is easier to only activate each GameObject once, and then forget about it.
            

            if(!FightActivatorFsmPatch.Apply())
            {
                return;
            }

            if(!BossfightFsmPatch.Apply())
            {
                return;
            }

            if(!NpcFsmPatch.Apply())
            {
                return;
            }

            // TODO: If there's an error, crash to the main menu or something? If any of these fail, and the others don't, we might get unstable behaviour
        }
    }
}