using UnityEngine;
using Framework.Managers;
using System.Diagnostics;

namespace IterTormenti.Esdras
{
    /// <summary>
    /// TODO
    /// </summary>
    public abstract class BossfightChanges
    {
        public static void Apply()
        {

            DEVSTUFF(); // TODO: Remove when done

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

        
        // TODO: Remove these when done. See what can be reused for utils

        [Conditional("DEBUG")]
        private static void DEVSTUFF()
        {

            Main.IterTormenti.Log("Flag 'D08Z01S01_BOSSDEAD': " + (Core.Events.GetFlag("D08Z01S01_BOSSDEAD")?"TRUE":"FALSE"));
            Main.IterTormenti.Log("Flag 'ESDRAS_CHAPEL':      " + (Core.Events.GetFlag("ESDRAS_CHAPEL")?"TRUE":"FALSE"));
            Main.IterTormenti.Log("Item 'QI203' owned?:       " + (Core.InventoryManager.IsQuestItemOwned("QI203")?"YES":"NO"));
            Main.IterTormenti.Log("Flag 'D08Z01S01_BOSSDEAD': " + (Core.Events.GetFlag("D01Z06S01_BOSSDEAD")?"TRUE":"FALSE"));
            Main.IterTormenti.Log("Flag 'D08Z01S01_BOSSDEAD': " + (Core.Events.GetFlag("D02Z05S01_BOSSDEAD")?"TRUE":"FALSE"));
            Main.IterTormenti.Log("Flag 'D08Z01S01_BOSSDEAD': " + (Core.Events.GetFlag("D03Z04S01_BOSSDEAD")?"TRUE":"FALSE"));



            //ExportFsm( GameObject.Find("Scapular Shine") );
            


            // Log all FSMs
            // Scene loadedScene = SceneManager.GetSceneByName(newLevel + "_LOGIC");
            // {
            //     Main.IterTormenti.Log("====== " + loadedScene.name + " ======");
            //     GameObject[] rootGameObjects = loadedScene.GetRootGameObjects();
            //     foreach (GameObject gameObject in rootGameObjects)
            //     {
            //         //Main.IterTormenti.Log(gameObject.name);
            //         PrintGameObjects(gameObject, " ");
            //     }

                
            // }

            

            // Log FSM
            /*{
                // string gameObjectName = "EsdrasFightActivator";
                // string fsmName = "EsdrasFightActivator";

                // string gameObjectName = "BossFight";
                // string fsmName = "FSM";

                string gameObjectName = "EsdrasNPC";
                string fsmName = "EsdrasNPC";

                GameObject gameObject = GameObject.Find(gameObjectName);
                if(null != gameObject)
                {
                    Main.IterTormenti.Log(" => " + gameObject.name);
                    
                    Main.IterTormenti.Log( gameObject.ListAllFsms() );

                    PlayMakerFSM fsm = PlayMakerFSM.FindFsmOnGameObject(gameObject, fsmName);

                    if(null != fsm)
                    {
                        Main.IterTormenti.Log(" =FSM=> " + fsm.FsmName);

                        //Main.IterTormenti.Log ( fsm.ToJson() );

                        foreach(var state in fsm.FsmStates)
                        {
                            Main.IterTormenti.Log ( "   " + state.Name );
                        }

                        //Main.Instance.StartCoroutine( fsm.SerializeIntoJsonFileCoroutine() );
                        //fsm.SerializeIntoJsonFile();
                    }
                }
            }*/

            // loadedScene = SceneManager.GetSceneByName(newLevel + "_LAYOUT");
            // {
            //     Main.IterTormenti.Log("====== " + loadedScene.name + " ======");
            //     GameObject[] rootGameObjects = loadedScene.GetRootGameObjects();
            //     foreach (GameObject gameObject in rootGameObjects)
            //     {
            //         //Main.IterTormenti.Log(gameObject.name);
            //         PrintGameObjects(gameObject, " ");
            //     }
            // }

            // loadedScene = SceneManager.GetSceneByName(newLevel + "_DECO");
            // {
            //     Main.IterTormenti.Log("====== " + loadedScene.name + " ======");
            //     GameObject[] rootGameObjects = loadedScene.GetRootGameObjects();
            //     foreach (GameObject gameObject in rootGameObjects)
            //     {
            //         //Main.IterTormenti.Log(gameObject.name);
            //         PrintGameObjects(gameObject, " ");
            //     }
            // }

            // // Check for Scapular
            // {
            //     FsmEvent scapularIsOwned  = new FsmEvent("ScapularIsOwned");
            //     FsmEvent scapularNotOwned = new FsmEvent("ScapularNotOwned");


            //     FsmState checkForScapular = new FsmState(fsm.Fsm);
            //     {
            //         checkForScapular.Name = "Check for Scapular";
            //         ItemIsOwned scapularOwned = new ItemIsOwned();
            //         {
            //             scapularOwned.itemType = 3;
            //             scapularOwned.objectId = "QI203";
            //             scapularOwned.onFailure = scapularNotOwned;
            //             scapularOwned.onSuccess = scapularIsOwned;
            //         }

            //         checkForScapular.AddAction(scapularOwned);

            //         checkForScapular.AddTransition("ScapularIsOwned", "StartBossfight");//"DIALOG"); //TODO
            //         checkForScapular.AddTransition("ScapularNotOwned", "StartBossfight");
            //     }

            //     fsm.AddState(checkForScapular);
            // }
        }
        private static void PrintGameObjects(GameObject gameObject, string ident)
        {
            Main.IterTormenti.Log(ident + gameObject.name);

            PlayMakerFSM[] components = gameObject.GetComponents<PlayMakerFSM>();
            foreach(PlayMakerFSM playMakerFSM in components)
            {
                Main.IterTormenti.Log(ident + " FSM: " + playMakerFSM.FsmName);
            }

            for (int idx = 0; idx < gameObject.transform.childCount; idx++)
            {
                PrintGameObjects(gameObject.transform.GetChild(idx).gameObject, ident + " ");
            }
        }

        private static void ExportFsm(GameObject gameObject)
        {
            if(null == gameObject) return;
            
            foreach (PlayMakerFSM fsm in PlayMakerFSM.FsmList)
            {
                if (fsm == null || fsm.gameObject != gameObject)
                {
                    continue;
                }

                Main.IterTormenti.Log($"Exporting {gameObject.name}::{fsm.FsmName} as '{fsm.gameObject.name}-{fsm.FsmName}.json'");


                Main.Instance.StartCoroutine( fsm.SerializeIntoJsonFileCoroutine() );
                // fsm.SerializeIntoJsonFile();
            }
        }
    }



}