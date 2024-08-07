using Blasphemous.ModdingAPI;
using UnityEngine;
using HutongGames.PlayMaker;
using HutongGames.PlayMaker.Actions;
using Tools.PlayMaker.Action;
using System.Collections.Generic;

using IterTormenti.Utils.Fsm;

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
        ///     - Play the fight normally until the deathblow.
        ///     - Upon the boss dying, do the following:
        ///         - Instantly replace it with the Animator to transition between Boss and NPC version
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
                ModLog.Error("Failed to patch 'BossFight': Main object not found!");
                return false;
            }

            PlayMakerFSM fsm = PlayMakerFSM.FindFsmOnGameObject(gameObject, "FSM");
            if(null == fsm)
            {
                ModLog.Error("Failed to patch 'BossFight': FSM object not found!");
                return false;
            }

            GameObject esdrasNPC = GameObject.Find("EsdrasNPC");
            if(null == esdrasNPC)
            {
                ModLog.Error("Failed to patch 'BossFight': 'EsdrasNPC' object not found!");
                return false;
            }

            PlayMakerFSM esdrasNpcFSM = PlayMakerFSM.FindFsmOnGameObject(esdrasNPC, "FSM");
            if(null == esdrasNpcFSM)
            {
                ModLog.Error("Failed to patch 'BossFight': EsdrasNPC FSM object not found!");
                return false;
            }

            GameObject bossFightStuff = GameObject.Find("BOSS_FIGHT_STUFF");
            if(null == bossFightStuff)
            {
                ModLog.Error("Failed to patch 'BossFight': 'BOSS_FIGHT_STUFF' object not found!");
                return false;
            }

            GameObject esdrasBoss = GameObject.Find("Esdras");
            if(null == esdrasBoss)
            {
                ModLog.Error("Failed to patch 'BossFight': 'Esdras' object not found!");
                return false;
            }

            GameObject esdrasDefeatAnimator = GameObject.Find("EsdrasDefeatAnimator");
            if(null == esdrasDefeatAnimator)
            {
                ModLog.Error("Failed to patch 'BossFight': 'EsdrasDefeatAnimator' object not found!");
                return false;
            }

            AnimatorBehaviour esdrasBehaviour = esdrasDefeatAnimator.GetComponent<AnimatorBehaviour>();
            if(null == esdrasBehaviour)
            {
                ModLog.Error("Failed to patch 'BossFight': 'esdrasBehaviour' object not found!");
                return false;
            }


        #endregion Find Required Objects
        #region Build FSM States


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

            // Cleanup removes boss stuff, but doesn't change camera, as the NPC portion will do that            
            FsmState cleanUp = new(fsm.Fsm);
            {
                cleanUp.Name = "Clean up";
                
                InputBlock removeBossDiedInputBlock = new()
                {
                    inputBlockName = "BossDiedBlock",
                    active = false
                };

                cleanUp.AddAction(removeBossDiedInputBlock);
            }

            // Replace the boss sprite with the transition animator
            FsmState replaceBossWithAnimator = new(fsm.Fsm);
            {
                replaceBossWithAnimator.Name = "Replace Boss With Animator";

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

                replaceBossWithAnimator.AddAction(callReplaceBossWithAnimator);
            }

            // At this point the boss is fully dead, so deactivate everything,
            // and tell the transition animator to continue
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


            fsm.AddState(deferSetCamera);
            fsm.AddState(switchToNPC);            
            fsm.AddState(cleanUp);
            fsm.AddState(replaceBossWithAnimator);            

            
        #endregion Add States to FSM
        #region Update FSM Workflow

            // Insert Defer Set Camera
            {
                FsmState blockPlayerInput = fsm.GetState("BlockPlayerInput");
                blockPlayerInput.ChangeTransition(FsmEvent.Finished.Name, deferSetCamera.Name);

                deferSetCamera.AddTransition(FsmEvent.Finished.Name, "Wait 3");
            }

            // Insert replaceBossWithAnimator into flow
            {
                FsmState isBossDead = fsm.GetState("Is Boss?");
                isBossDead.ChangeTransition("Yes", replaceBossWithAnimator.Name);
            }

            // Get back to normal flow
            {                
                replaceBossWithAnimator.AddTransition(FsmEvent.Finished.Name, "Add completion %");
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

            // Update guilt check 'No' transition
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
