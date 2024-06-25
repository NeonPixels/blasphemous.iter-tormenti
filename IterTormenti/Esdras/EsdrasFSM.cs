
using UnityEngine;
using HutongGames.PlayMaker;
using HutongGames.PlayMaker.Actions;
using Tools.Playmaker2.Condition;
using Tools.Playmaker2.Action;

using Framework.Managers;
using Tools.PlayMaker.Action;
using System;
using Epic.OnlineServices.P2P;
using System.Collections.Generic;

using System.Diagnostics;
using UnityEngine.SceneManagement;
using IterTormenti.FSMUtils;
using Blasphemous.ModdingAPI.Files;
using IterTormenti.utils.sprite;

namespace IterTormenti.Esdras
{
    public abstract class FSMChanges
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
            

            if(!UpdateEsdrasFightActivator())
            {
                return;
            }

            if(!UpdateBossFight())
            {
                return;
            }

            if(!UpdateEsdrasNPC())
            {
                return;
            }

            CreateDefeatAnimation();

            // TODO: If there's an error, crash to the main menu or something? If any of these fail, and the others don't, we might get unstable behaviour
        }

        /// <summary>
        /// Update the EsdrasFightActivator FSM to perform the following actions:
        ///     - Make sure that both the Bossfight and EsdrasNPC game objects are activated immediately.
        /// </summary>
        /// <returns>'true' if no errors happen, 'false' otherwise</returns>
        private static bool UpdateEsdrasFightActivator()
        {

        #region Find Required Objects


            GameObject gameObject = GameObject.Find("EsdrasFightActivator");
            if(null == gameObject)
            {
                Main.IterTormenti.LogError("Failed to patch 'EsdrasFightActivator': Main object not found!");
                return false;
            }

            PlayMakerFSM fsm = PlayMakerFSM.FindFsmOnGameObject(gameObject, "EsdrasFightActivator");
            if(null == fsm)
            {
                Main.IterTormenti.LogError("Failed to patch 'EsdrasFightActivator': FSM object not found!");
                return false;
            }

            GameObject esdrasNPC = GameObject.Find("EsdrasNPC");
            if(null == esdrasNPC)
            {
                Main.IterTormenti.LogError("Failed to patch 'EsdrasFightActivator': 'EsdrasNPC' object not found!");
                return false;
            }

            GameObject bossFightStuff = GameObject.Find("BOSS_FIGHT_STUFF");
            if(null == bossFightStuff)
            {
                Main.IterTormenti.LogError("Failed to patch 'EsdrasFightActivator': 'BOSS_FIGHT_STUFF' object not found!");
                return false;
            }


        #endregion Find Required Objects
        #region Build FSM States


            Main.IterTormenti.Log("Patching '" + gameObject.name + ":" + fsm.name + "' FSM...");
            
            FsmState activateEverything = new FsmState(fsm.Fsm);
            {
                activateEverything.Name = "ActivateEverything";
                
                ActivateGameObject activateNPC = new ActivateGameObject();
                {
                    FsmOwnerDefault target = new FsmOwnerDefault();
                    {                        
                        FsmGameObject fsmGameObject = new FsmGameObject()
                        {
                            Value = esdrasNPC
                        };

                        target.OwnerOption = OwnerDefaultOption.SpecifyGameObject;
                        target.GameObject = fsmGameObject;
                    }

                    activateNPC.gameObject = target;
                    activateNPC.activate = true;
                    activateNPC.recursive = false;
                    activateNPC.resetOnExit = false;
                }

                ActivateGameObject activateBossStuff = new ActivateGameObject();
                {
                    FsmOwnerDefault target = new FsmOwnerDefault();
                    {                        
                        FsmGameObject fsmGameObject = new FsmGameObject()
                        {
                            Value = bossFightStuff
                        };

                        target.OwnerOption = OwnerDefaultOption.SpecifyGameObject;
                        target.GameObject = fsmGameObject;
                    }

                    activateBossStuff.gameObject = target;
                    activateBossStuff.activate = true;
                    activateBossStuff.recursive = false;
                    activateBossStuff.resetOnExit = false;
                }
                
                activateEverything.AddAction(activateNPC);
                activateEverything.AddAction(activateBossStuff);
            }


        #endregion Build FSM States
        #region Add States to FSM


            fsm.AddState(activateEverything);


        #endregion Add States to FSM
        #region Update FSM Workflow


            // Activate everything so they can operate in parallel
            fsm.ChangeGlobalTransition("ON LEVEL READY", activateEverything.Name);


        #endregion Update FSM Workflow

            return true;
        }

        /// <summary>
        /// Update the BossFight FSM to perform the following actions:
        ///     - Wait for player collision as normal.
        ///     - Set up bossfight as normal, EXCEPT the camera setup.
        ///     - Ask EsdrasNPC FSM to set the camera. This is done because, if the camera is managed
        ///       by BossFight FSM, once the GameObject is disabled, the camera resets, so camera management is
        ///       deferred to EsdrasNPC's FSM.
        ///     - Play the introduction to the fight normally, until Esdras performs his taunt
        ///       (slams weapon on ground)
        ///     - Initiate a dialog, allowing players to choose wether to fight Esdras or skip the fight.
        ///     - If players choose to skip the fight, immediately replace the boss with the NPC, do any cleanup
        ///       and switch over to the EsdrasNPC FSM.
        ///     - If players choose to fight, continue normally until the deathblow.
        ///     - Upon the boss dying, do the following:
        ///         - Instantly replace it with the NPC version, on the same position and with an appropriate animation // TODO
        ///           Note: The boss animations have to be overriden to avoid the death animation that makes Esdras explode.
        ///         - Display the 'Requiem Aeternam' message.
        ///     - Afterwards, do any cleanup and switch over to the EsdrasNPC FSM.        
        ///     - Cleanup involves removing any input blocks that might've been enabled. EsdrasNPC FSM will manage its
        ///       own input blockers.
        ///     - Switching involves disabling the BossFight related GameObjects (BOSS_FIGHT_STUFF container), and asking
        ///       EsdrasNPC FSM to start its workflow.
        /// </summary>
        /// <returns>'true' if no errors happen, 'false' otherwise</returns>
        private static bool UpdateBossFight()
        {

        #region Find Required Objects


            GameObject gameObject = GameObject.Find("BossFight");
            if(null == gameObject)
            {
                Main.IterTormenti.LogError("Failed to patch 'BossFight': Main object not found!");
                return false;
            }

            PlayMakerFSM fsm = PlayMakerFSM.FindFsmOnGameObject(gameObject, "FSM");
            if(null == fsm)
            {
                Main.IterTormenti.LogError("Failed to patch 'BossFight': FSM object not found!");
                return false;
            }

            GameObject esdrasNPC = GameObject.Find("EsdrasNPC");
            if(null == esdrasNPC)
            {
                Main.IterTormenti.LogError("Failed to patch 'BossFight': 'EsdrasNPC' object not found!");
                return false;
            }

            PlayMakerFSM esdrasNpcFSM = PlayMakerFSM.FindFsmOnGameObject(esdrasNPC, "FSM");
            if(null == esdrasNpcFSM)
            {
                Main.IterTormenti.LogError("Failed to patch 'BossFight': EsdrasNPC FSM object not found!");
                return false;
            }

            GameObject bossFightStuff = GameObject.Find("BOSS_FIGHT_STUFF");
            if(null == bossFightStuff)
            {
                Main.IterTormenti.LogError("Failed to patch 'BossFight': 'BOSS_FIGHT_STUFF' object not found!");
                return false;
            }


        #endregion Find Required Objects
        #region Build FSM States

            Main.IterTormenti.Log("Patching '" + gameObject.name + ":" + fsm.name + "' FSM...");
            

        #if DISABLED_CODE // TODO: This should work but doesn't, leaving as reference
            // Send Bossfight Start event
            FsmState bossTriggeredEvent = new FsmState(fsm.Fsm);
            {
                bossTriggeredEvent.Name = "BOSS TRIGGERED";

                SendEvent sendEventAction = new SendEvent();
                {
                    FsmEvent bossStartEvent = new FsmEvent("ON ESDRAS BOSSFIGHT START");
                    bossStartEvent.IsGlobal = true;

                    FsmEventTarget eventTarget = new FsmEventTarget();
                    {
                        FsmOwnerDefault target = new FsmOwnerDefault();
                        {                        
                            FsmGameObject fsmGameObject = new FsmGameObject()
                            {
                                Value = esdrasNPC
                            };

                            target.OwnerOption = OwnerDefaultOption.SpecifyGameObject;
                            target.GameObject = fsmGameObject;
                        }

                        eventTarget.target = FsmEventTarget.EventTarget.GameObjectFSM;
                        eventTarget.gameObject = target;
                    }

                    sendEventAction.sendEvent = bossStartEvent;
                    sendEventAction.eventTarget = eventTarget;
                }
                
                bossTriggeredEvent.AddAction(sendEventAction);
            }
        #endif //DISABLED_CODE

            // Tell EsdrasNPC FSM to set camera boundaries
            FsmState deferSetCamera = new FsmState(fsm.Fsm);
            {
                deferSetCamera.Name = "DeferSetCamera";

                // NOTE: Since using events to trigger state changes in
                //       other FSMs doesn't seem to work via code, we
                //       are resorting to directly telling the FSM to
                //       swith to an specific state.
                //       If a way to make it work is found, it would
                //       be preferable to issue an event signal and
                //       have the remote FSM process it independently
                //       instead of doing this.
                //       See disabled code above

                CallMethod callMethod = new CallMethod();
                {
                    var methodParams = new List<FsmVar>();
                    {
                        FsmString value = "SetCamera";
                        FsmVar param = new FsmVar(value);

                        methodParams.Add(param);
                    }
                    
                    FsmObject target = new FsmObject()
                    {
                        Value = esdrasNpcFSM,
                        ObjectType = esdrasNpcFSM.GetType()
                    };
                    
                    callMethod.behaviour = target;
                    callMethod.methodName = "SetState";
                    callMethod.parameters = methodParams.ToArray();
                }

                deferSetCamera.AddAction(callMethod);
            }

            
            FsmState choiceDialog = new FsmState(fsm.Fsm); // PLACEHOLDER
            {
                choiceDialog.Name = "Choice Dialog";

                // FsmEvent assent  = new FsmEvent("Assent");
                // FsmEvent dissent = new FsmEvent("Dissent");

                // DialogStart dialog = new DialogStart();
                // {
                //     dialog.conversation = "DLG_QUESTION_ASSENT";                                    
                //     dialog.answer1 = assent;
                //     dialog.answer2 = dissent;
                // }

                // choiceDialog.AddAction(dialog);
            }

            // Cleanup removes bosss stuff, but doesn't change camera, as the NPC portion will do that
            // It also disables the INTRO input blocker, in case the player chose to show the scapular,
            // and the blocker wasn't removed before the fight            
            FsmState cleanUp = new FsmState(fsm.Fsm);//fsm.GetState("Give control back")); // Duplicate existing state
            {
                cleanUp.Name = "Clean up";
                
                // Remove camera repositioning action, which is of type 'CallMethod'
                // cleanUp.RemoveAction( cleanUp.GetIndexOfFirstActionOfType<CallMethod>() );

                InputBlock removeBossDiedInputBlock = new InputBlock()
                {
                    inputBlockName = "BossDiedBlock",
                    active = false
                };

                InputBlock removeIntroInputBlock = new InputBlock()
                {
                    inputBlockName = "INTRO",
                    active = false
                };

                // SetProperty disableBoss = new SetProperty();
                // {
                //     FsmProperty target = new FsmProperty();
                //     {
                //         FsmObject targetObject = new FsmObject()
                //         {
                //             Value = bossFightStuff,
                //             ObjectType = bossFightStuff.GetType()
                //         };

                //         target.TargetObject = targetObject;
                //         target.TargetType   = targetObject.GetType();
                //         target.PropertyName = "active";
                //         target.PropertyName = "System.Boolean";
                //     }
                // }

                cleanUp.AddAction(removeBossDiedInputBlock);
                cleanUp.AddAction(removeIntroInputBlock);                
            }

            // Move the different characters to their expected positions
            //  - EsdrasNPC: Move to last position of Esdras boss, face TPO
            //  - Perpetvua Apparition: Move to TPO position, face Esdras
            //  - TPO: Face Esdras
            FsmState moveCharacters = new FsmState(fsm.Fsm); // PLACEHOLDER
            {
                moveCharacters.Name = "Move Characters";
                // SetPosition setPosition = new SetPosition();
                // {
                //     FsmOwnerDefault target = new FsmOwnerDefault();
                //     target.GameObject = new FsmGameObject(esdrasNPC);
                //     target.OwnerOption = OwnerDefaultOption.SpecifyGameObject;

                //     setPosition.gameObject = target;
                //     setPosition.everyFrame = false;
                //     setPosition.vector = null; // TODO
                // }

                // moveCharacters.AddAction(setPosition);
            }


            // Start NPC workflow
            FsmState startNPC = new FsmState(fsm.Fsm);
            {
                startNPC.Name = "START NPC";


                // NOTE: Since using events to trigger state changes in
                //       other FSMs doesn't seem to work via code, we
                //       are resorting to directly telling the FSM to
                //       swith to an specific state.
                //       If a way to make it work is found, it would
                //       be preferable to issue an event signal and
                //       have the remote FSM process it independently
                //       instead of doing this.                

                CallMethod callMethod = new CallMethod();
                {
                    var methodParams = new List<FsmVar>();
                    {
                        FsmString value = "BlockPlayerInput";
                        FsmVar param = new FsmVar(value);

                        methodParams.Add(param);
                    }
                    
                    FsmObject target = new FsmObject()
                    {
                        Value = esdrasNpcFSM,
                        ObjectType = esdrasNpcFSM.GetType()
                    };
                    
                    callMethod.behaviour = target;
                    callMethod.methodName = "SetState";
                    callMethod.parameters = methodParams.ToArray();
                }

                startNPC.AddAction(callMethod);
            }

            // Switch to NPC
            FsmState switchToNPC = new FsmState(fsm.Fsm);
            {
                switchToNPC.Name = "SWITCH TO NPC";
                
                ActivateGameObject deactivateBossStuff = new ActivateGameObject();
                {
                    FsmOwnerDefault target = new FsmOwnerDefault();
                    {                        
                        FsmGameObject fsmGameObject = new FsmGameObject()
                        {
                            Value = bossFightStuff
                        };

                        target.OwnerOption = OwnerDefaultOption.SpecifyGameObject;
                        target.GameObject = fsmGameObject;
                    }

                    deactivateBossStuff.gameObject = target;
                    deactivateBossStuff.activate = false;
                    deactivateBossStuff.recursive = true;
                    deactivateBossStuff.resetOnExit = false;
                }
                
                switchToNPC.AddAction(deactivateBossStuff);
            }


        #endregion Build FSM States
        #region Add States to FSM


            //fsm.AddState(bossTriggeredEvent);
            fsm.AddState(deferSetCamera);
            fsm.AddState(choiceDialog);
            fsm.AddState(startNPC);
            fsm.AddState(switchToNPC);            
            fsm.AddState(cleanUp);
            fsm.AddState(moveCharacters);            

            
        #endregion Add States to FSM
        #region Update FSM Workflow


            // // Insert Boss Triggered Event
            // {
            //     FsmState blockPlayerInput = fsm.GetState("BlockPlayerInput");
            //     blockPlayerInput.ChangeTransition(FsmEvent.Finished.Name, bossTriggeredEvent.Name);

            //     bossTriggeredEvent.AddTransition(FsmEvent.Finished.Name, "Wait 3");
            // }

            // Insert Defer Set Camera
            {
                FsmState blockPlayerInput = fsm.GetState("BlockPlayerInput");
                blockPlayerInput.ChangeTransition(FsmEvent.Finished.Name, deferSetCamera.Name);

                deferSetCamera.AddTransition(FsmEvent.Finished.Name, "Wait 3");
            }

            // Insert dialog into flow
            {
                FsmState introWait = fsm.GetState("IntroWait");
                introWait.ChangeTransition(FsmEvent.Finished.Name, choiceDialog.Name);
                
                choiceDialog.AddTransition(FsmEvent.Finished.Name, "StartBossfight");//cleanUp.Name); // PLACEHOLDER
                // choiceDialog.AddTransition("Assent", "Clean up");
                // choiceDialog.AddTransition("Dissent", "StartBossfight");
            }

            // Skip granting the item reward, it will be handled by the NPC portion
            {
                FsmState grantAchievement = fsm.GetState("Grant AC05");
                grantAchievement.ChangeTransition(FsmEvent.Finished.Name, "Combat is Over (SET FLAG), block player");
            }

            // Skip Esdras final dialog, he will be rudely interrupted
            {
                FsmState clearPerpetva = fsm.GetState("Set Perpetva flag to avoid fight");
                clearPerpetva.ChangeTransition(FsmEvent.Finished.Name, "SHOW MESSAGE");
            }

            // Skip displaying the reward message, it will be handled by the NPC portion
            {
                FsmState showMessage = fsm.GetState("SHOW MESSAGE");
                showMessage.ChangeTransition(FsmEvent.Finished.Name, "Is guilt >0?");
            }

            // Update guitl check 'No' transition
            {
                FsmState guiltCheck = fsm.GetState("Is guilt >0?");
                guiltCheck.ChangeTransition("No", cleanUp.Name);
            }

            // Insert cleanup into flow
            {
                FsmState guiltReset = fsm.GetState("Guilt reset");
                guiltReset.ChangeTransition(FsmEvent.Finished.Name, cleanUp.Name);
            }

            // Insert moveCharacters into flow
            {
                cleanUp.AddTransition(FsmEvent.Finished.Name, moveCharacters.Name);
            }

            // Insert startNPC into flow
            {
                moveCharacters.AddTransition(FsmEvent.Finished.Name, startNPC.Name);
            }  

            // Insert switchToNPC into flow
            {
                startNPC.AddTransition(FsmEvent.Finished.Name, switchToNPC.Name);
            }           


        #endregion Update FSM Workflow

            return true;
        }

        /// <summary>
        /// Update the EsdrasNPC FSM to perform the following actions:
        ///     - On level load:
        ///         - Make the Esdras sprite invisible. // TODO
        ///         - Attach the position of the PerpetvaAppears sprite to the Penitent. // TODO
        ///         - Attach the position of the Esdras NPC sprite to the Boss position. // TODO
        ///         - just wait.
        ///     - When requested, do the camera setup, and then wait.
        ///     - When requested, make Esdras sprite visible at a given position, with an appropriate animation. // TODO: Attach NPC sprite to boss?
        ///     - When requested, start the Bridge workflow, with the following changes:
        ///         - Apply input blockers.
        ///         - Skip camera setup.
        ///         - Make TPO face Esdras.  // TODO
        ///         - Make Esdras face TPO. // TODO
        ///         - Skip directly to the point where Perpetva reveals herself.
        ///     - Play the whole animation normally, including the item rewards and flag updates.
        /// </summary>
        /// <returns>'true' if no errors happen, 'false' otherwise</returns>
        private static bool UpdateEsdrasNPC()
        {

        #region Find Required Objects


            GameObject gameObject = GameObject.Find("EsdrasNPC");
            if(null == gameObject)
            {
                Main.IterTormenti.LogError("Failed to patch 'EsdrasNPC': Main object not found!");
                return false;
            }
        
            PlayMakerFSM fsm = PlayMakerFSM.FindFsmOnGameObject(gameObject, "FSM");
            if(null == fsm)
            {
                Main.IterTormenti.LogError("Failed to patch 'EsdrasNPC': FSM object not found!");
                return false;
            }

            GameObject perpetvaAppears = null;
            foreach ( var go in Resources.FindObjectsOfTypeAll(typeof(GameObject)) )
            {
                if(null == go) continue;
                if(typeof(GameObject) != go.GetType()) continue;
                if(!go.name.Equals("PerpetvaAppears_SimpleVFX")) continue;

                perpetvaAppears = go as GameObject;
                break;
            }
            if(null == perpetvaAppears)
            {
                Main.IterTormenti.LogError("Failed to patch 'BossFight': 'PerpetvaAppears_SimpleVFX' object not found!");
                return false;
            }


        #endregion Find Required Objects
        #region Build FSM States


            Main.IterTormenti.Log("Patching '" + gameObject.name + ":" + fsm.name + "' FSM...");


            // Wait until the Penitent object is in the scene
            FsmState waitForPenitent = new FsmState(fsm.Fsm);
            {
                waitForPenitent.Name = "Wait for Penitent";

                fsm.AddVariable<FsmGameObject>("Penitent");

                WaitForGameObject waitForObject = new WaitForGameObject()
                {
                    withTag = "Penitent",
                    store = fsm.GetVariable<FsmGameObject>("Penitent")
                };

                waitForPenitent.AddAction(waitForObject);
            }

            // Attach Perpetva position to Penitent position
            FsmState attachPerpetvaToPenitent = new FsmState(fsm.Fsm);
            {
                attachPerpetvaToPenitent.Name = "Attach Perpetva to Penitent";
                //TODO
            }

            // Attach NPC to Boss
            FsmState attachNpcToBoss = new FsmState(fsm.Fsm);
            {
                attachNpcToBoss.Name = "Attach NPC to Boss";
                //TODO
            }

            // Dettach NPC from Boss
            FsmState dettachNpcFromBoss = new FsmState(fsm.Fsm);
            {
                attachNpcToBoss.Name = "Dettach NPC from Boss";
                //TODO
            }

            FsmState waitForBossfightStart = new FsmState(fsm.Fsm);
            {
                waitForBossfightStart.Name = "Wait for Bossfight start";
            }

            FsmState waitForBossfightEnd = new FsmState(fsm.Fsm);
            {
                waitForBossfightEnd.Name = "Wait for Bossfight end";
            }

            // Updates the facing of Esdras and the Penitent
            // Note: Perpetva is already facing the same direction as the Penitent
            FsmState updateFacing = new FsmState(fsm.Fsm);
            {
                updateFacing.Name = "Update Facing";
                //TODO
            }


        #endregion Build FSM States
        #region Add States to FSM


            fsm.AddState(waitForBossfightStart);
            fsm.AddState(waitForBossfightEnd);


        #endregion Add States to FSM
        #region Update FSM Workflow


            // Initial state, the FSM will wait until activated
            {                
                fsm.ChangeGlobalTransition("ON LEVEL READY", waitForBossfightStart.Name);
             
                // TODO: This doesn't seem to work. Why?
                // waitForBossfightStart.AddTransition("ON ESDRAS BOSSFIGHT START", "SetCamera");
            }

            // Move camera to position used for bossfight, then wait again until activated
            {
                FsmState setCamera = fsm.GetState("SetCamera");
                setCamera.ChangeTransition(FsmEvent.Finished.Name, waitForBossfightEnd.Name);                
            }

            // We need to block input here to avoid having to manage input blocks across different FSMs, each
            // one should clear their own input blockers before ending.
            {
                // TODO: This doesn't seem to work. Why?
                // waitForBossfightEnd.AddTransition("ON ESDRAS BOSSFIGHT DONE", "BlockPlayerInput");
            }

            // Skip camera setup, it has already been done
            {
                FsmState blockPlayerInput = fsm.GetState("BlockPlayerInput");
                blockPlayerInput.ChangeTransition(FsmEvent.Finished.Name, "Raise chapel flag");
            }

            // Skip Esdras intro and taunt, it has already been done by the BossFight object
            {
                FsmState wait9 = fsm.GetState("Wait 9");
                wait9.ChangeTransition(FsmEvent.Finished.Name, "Play IdleDazzle animation");
            }


        #endregion Update FSM Workflow

            return true;
        }


        private static bool CreateDefeatAnimation()
        {
            GameObject esdrasDefeatAnimations = new GameObject("EsdrasDefeatAnimations");
            {
                SpriteRenderer renderer = esdrasDefeatAnimations.AddComponent<SpriteRenderer>();
                renderer.enabled = true;
                renderer.drawMode = SpriteDrawMode.Simple;
                renderer.sortingLayerName = "Player";

                SpriteImportOptions importOptions = new SpriteImportOptions()
                {
                    Pivot = new Vector2(0.5f, 0.0f)
                };

                Vector2 frameSize = new Vector2(256.0f,128.0f);
                const float animationDelay = 0.1f; // 100fps?

                AnimatedSprite defeat = esdrasDefeatAnimations.AddComponent<AnimatedSprite>();
                {
                    defeat.Renderer = renderer;
                    defeat.Delay = animationDelay;
                    defeat.Loop = true;//false;

                    Sprite[] sprites;
                    Main.IterTormenti.FileHandler.LoadDataAsFixedSpritesheet("EsdrasNonLethalDefeat.png", frameSize, out sprites, importOptions);

                    
                    if(sprites.Length < 26)
                    {
                        Main.IterTormenti.LogError($"Failed loading 'EsdrasNonLethalDefeat.png', received {sprites.Length} frames!");
                        return false;
                    }

                    // Spritesheet only has 26 frames, ignore extra frames
                    Array.Copy( sprites, defeat.frames = new Sprite[26], defeat.frames.Length);

                    defeat.enabled = true;
                    defeat.Play();

                    Main.IterTormenti.Log("Defeat AnimatedSprite: " + defeat.ToString());
                }

                AnimatedSprite hunchedOver = esdrasDefeatAnimations.AddComponent<AnimatedSprite>();
                {
                    hunchedOver.Renderer = renderer;
                    hunchedOver.Delay = animationDelay;
                    hunchedOver.Loop = true;

                    Sprite[] sprites;
                    Main.IterTormenti.FileHandler.LoadDataAsFixedSpritesheet("EsdrasDefeated.png", frameSize, out sprites, importOptions);

                    
                    if(sprites.Length < 3)
                    {
                        Main.IterTormenti.LogError($"Failed loading 'EsdrasDefeated.png', received {sprites.Length} frames!");
                        return false;
                    }

                    // Spritesheet only has 3 frames, ignore extra frames
                    Array.Copy( sprites, hunchedOver.frames = new Sprite[3], hunchedOver.frames.Length);

                    hunchedOver.enabled = false;
                    //hunchedOver.Play();

                    Main.IterTormenti.Log("HunchedOver AnimatedSprite: " + hunchedOver.ToString());
                }

                AnimatedSprite pickUpWeapon = esdrasDefeatAnimations.AddComponent<AnimatedSprite>();
                {
                    pickUpWeapon.Renderer = renderer;
                    pickUpWeapon.Delay = animationDelay;
                    pickUpWeapon.Loop = true;//false;

                    Sprite[] sprites;
                    Main.IterTormenti.FileHandler.LoadDataAsFixedSpritesheet("EsdrasPickupWeapon.png", frameSize, out sprites, importOptions);

                    
                    if(sprites.Length < 15)
                    {
                        Main.IterTormenti.LogError($"Failed loading 'EsdrasPickupWeapon.png', received {sprites.Length} frames!");
                        return false;
                    }

                    // Spritesheet only has 15 frames, ignore extra frames
                    Array.Copy( sprites, pickUpWeapon.frames = new Sprite[15], pickUpWeapon.frames.Length);

                    pickUpWeapon.enabled = false;
                    //hunchedOver.Play();

                    Main.IterTormenti.Log("PickUpWeapon AnimatedSprite: " + pickUpWeapon.ToString());
                }
            }

            esdrasDefeatAnimations.transform.position = new Vector3(-90.0f,8.0f,0.0f);
            esdrasDefeatAnimations.SetActive(true);


#region TEST

            GameObject animTest2 = new GameObject("AnimTest2");
            {
                SpriteRenderer renderer = animTest2.AddComponent<SpriteRenderer>();
                renderer.enabled = true;
                renderer.drawMode = SpriteDrawMode.Simple;
                renderer.sortingLayerName = "Player";

                SpriteImportOptions importOptions = new SpriteImportOptions()
                {
                    Pivot = new Vector2(0.5f, 0.0f)
                };

                Vector2 frameSize = new Vector2(256.0f,128.0f);
                const float animationDelay = 0.1f; // 100fps?

                AnimatedSprite hunchedOver = animTest2.AddComponent<AnimatedSprite>();
                {
                    hunchedOver.Renderer = renderer;
                    hunchedOver.Delay = animationDelay;
                    hunchedOver.Loop = true;

                    Sprite[] sprites;
                    Main.IterTormenti.FileHandler.LoadDataAsFixedSpritesheet("EsdrasDefeated.png", frameSize, out sprites, importOptions);

                    
                    if(sprites.Length < 3)
                    {
                        Main.IterTormenti.LogError($"Failed loading 'EsdrasDefeated.png', received {sprites.Length} frames!");
                        return false;
                    }

                    // Spritesheet only has 3 frames, ignore extra frames
                    //Array.Copy( sprites, hunchedOver.frames = new Sprite[3], hunchedOver.frames.Length);

                    // Assign frames
                    hunchedOver.frames = new Sprite[6];
                    hunchedOver.frames[0] = sprites[0];
                    hunchedOver.frames[1] = sprites[0];
                    hunchedOver.frames[2] = sprites[1];
                    hunchedOver.frames[3] = sprites[2];
                    hunchedOver.frames[4] = sprites[2];
                    hunchedOver.frames[5] = sprites[1];


                    hunchedOver.enabled = true;
                    hunchedOver.Play();
                }
            }

            animTest2.transform.position = new Vector3(-88.0f,8.0f,0.0f);
            animTest2.SetActive(true);

            GameObject animTest3 = new GameObject("AnimTest3");
            {
                SpriteRenderer renderer = animTest3.AddComponent<SpriteRenderer>();
                renderer.enabled = true;
                renderer.drawMode = SpriteDrawMode.Simple;
                renderer.sortingLayerName = "Player";

                SpriteImportOptions importOptions = new SpriteImportOptions()
                {
                    Pivot = new Vector2(0.5f, 0.0f)
                };

                Vector2 frameSize = new Vector2(256.0f,128.0f);
                const float animationDelay = 0.1f; // 100fps?

                AnimatedSprite pickUpWeapon = animTest3.AddComponent<AnimatedSprite>();
                {
                    pickUpWeapon.Renderer = renderer;
                    pickUpWeapon.Delay = animationDelay;
                    pickUpWeapon.Loop = true;//false;

                    Sprite[] sprites;
                    Main.IterTormenti.FileHandler.LoadDataAsFixedSpritesheet("EsdrasPickupWeapon.png", frameSize, out sprites, importOptions);

                    
                    if(sprites.Length < 15)
                    {
                        Main.IterTormenti.LogError($"Failed loading 'EsdrasPickupWeapon.png', received {sprites.Length} frames!");
                        return false;
                    }

                    // Spritesheet only has 15 frames, ignore extra frames
                    Array.Copy( sprites, pickUpWeapon.frames = new Sprite[15], pickUpWeapon.frames.Length);

                    pickUpWeapon.enabled = true;
                    pickUpWeapon.Play();
                }
            }

            animTest3.transform.position = new Vector3(-86.0f,8.0f,0.0f);
            animTest3.SetActive(true);

#endregion



            return true;
        }


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