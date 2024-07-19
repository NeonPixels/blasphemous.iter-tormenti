using UnityEngine;
using HutongGames.PlayMaker;
using HutongGames.PlayMaker.Actions;
using Tools.PlayMaker.Action;
using System.Collections.Generic;

namespace IterTormenti.Esdras
{
    public abstract class BossfightFsmPatch
    {
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
        public static bool Apply()
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

            GameObject esdrasBoss = GameObject.Find("Esdras");
            if(null == esdrasBoss)
            {
                Main.IterTormenti.LogError("Failed to patch 'BossFight': 'Esdras' object not found!");
                return false;
            }

            GameObject esdrasDefeatAnimator = GameObject.Find("EsdrasDefeatAnimator");
            if(null == esdrasDefeatAnimator)
            {
                Main.IterTormenti.LogError("Failed to patch 'BossFight': 'EsdrasDefeatAnimator' object not found!");
                return false;
            }

            AnimatorBehaviour esdrasBehaviour = esdrasDefeatAnimator.GetComponent<AnimatorBehaviour>();
            if(null == esdrasBehaviour)
            {
                Main.IterTormenti.LogError("Failed to patch 'BossFight': 'esdrasBehaviour' object not found!");
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
            FsmState deferSetCamera = new(fsm.Fsm);
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

                CallMethod callMethod = new();
                {
                    var methodParams = new List<FsmVar>();
                    {
                        FsmString value = "SetCamera";
                        FsmVar param = new(value);

                        methodParams.Add(param);
                    }
                    
                    FsmObject target = new()
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

            
            FsmState choiceDialog = new(fsm.Fsm); // PLACEHOLDER
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
            FsmState cleanUp = new(fsm.Fsm);
            {
                cleanUp.Name = "Clean up";
                
                InputBlock removeBossDiedInputBlock = new()
                {
                    inputBlockName = "BossDiedBlock",
                    active = false
                };

                InputBlock removeIntroInputBlock = new()
                {
                    inputBlockName = "INTRO",
                    active = false
                };

                cleanUp.AddAction(removeBossDiedInputBlock);
                cleanUp.AddAction(removeIntroInputBlock);                
            }

            // Move the different characters to their expected positions
            //  - EsdrasNPC: Move to last position of Esdras boss, face TPO
            //  - Perpetvua Apparition: Move to TPO position, face Esdras
            //  - TPO: Face Esdras
            FsmState moveCharacters = new(fsm.Fsm); // PLACEHOLDER
            {
                moveCharacters.Name = "Move Characters";

                CallMethod callReplaceBossWithAnimator = new();
                {
                    var methodParams = new List<FsmVar>();
                    
                    FsmObject target = new()
                    {
                        Value = esdrasBehaviour,
                        ObjectType = esdrasBehaviour.GetType()
                    };
                    
                    callReplaceBossWithAnimator.behaviour = target;
                    callReplaceBossWithAnimator.methodName = "ReplaceBossWithAnimator";
                    callReplaceBossWithAnimator.parameters = methodParams.ToArray();
                }

                moveCharacters.AddAction(callReplaceBossWithAnimator);
            }


            // Start NPC workflow
            FsmState startNPC = new(fsm.Fsm);
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

                CallMethod callMethod = new();
                {
                    var methodParams = new List<FsmVar>();
                    {
                        FsmString value = "BlockPlayerInput";
                        FsmVar param = new(value);

                        methodParams.Add(param);
                    }
                    
                    FsmObject target = new()
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
            FsmState switchToNPC = new(fsm.Fsm);
            {
                switchToNPC.Name = "SWITCH TO NPC";
                
                CallMethod callMethod = new();
                {
                    FsmObject target = new()
                    {
                        Value = esdrasBehaviour,
                        ObjectType = esdrasBehaviour.GetType()
                    };
                    
                    callMethod.behaviour = target;
                    callMethod.methodName = "SetAnimatorToStandUp";
                    callMethod.parameters = new FsmVar[0];
                }

                switchToNPC.AddAction(callMethod);


                ActivateGameObject deactivateBossStuff = new();
                {
                    FsmOwnerDefault target = new();
                    {                        
                        FsmGameObject fsmGameObject = new()
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
            fsm.AddState(switchToNPC);            
            fsm.AddState(cleanUp);
            fsm.AddState(moveCharacters);            

            
        #endregion Add States to FSM
        #region Update FSM Workflow

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

            // Insert moveCharacters into flow // TODO
            {
                FsmState isBossDead = fsm.GetState("Is Boss?");
                isBossDead.ChangeTransition("Yes", moveCharacters.Name);
            }

            // Get back to normal flow // TODO: Call behaviour
            {                
                moveCharacters.AddTransition(FsmEvent.Finished.Name, "Add completion %");
            }

            // Skip granting the item reward, it will be handled by the NPC portion
            {
                FsmState grantAchievement = fsm.GetState("Grant AC05");
                grantAchievement.ChangeTransition(FsmEvent.Finished.Name, "Combat is Over (SET FLAG), block player");
            }

            // Insert moveCharacters into flow  // TODO: Replace boss sprite
            {
                //FsmState combatOver = fsm.GetState("Combat is Over (SET FLAG), block player");

                // TODO: WTF? if we don't jump to the "Wait" state, the boss doesn't enter the death animation
                //       and keeps attacking until disabled later on. WHY?!
                //combatOver.ChangeTransition(FsmEvent.Finished.Name, moveCharacters.Name);
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

            // Insert switchToNPC into flow
            {
                cleanUp.AddTransition(FsmEvent.Finished.Name, switchToNPC.Name);
            }


        #endregion Update FSM Workflow

            return true;
        }

    }
}