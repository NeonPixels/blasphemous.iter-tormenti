using UnityEngine;
using HutongGames.PlayMaker;
using HutongGames.PlayMaker.Actions;
using IterTormenti.FSMUtils;
using System.Collections.Generic;

namespace IterTormenti.Esdras
{
    public abstract class NpcFsmPatch
    {
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
        public static bool Apply()
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

            // Hide sprite and shadow directly, we will re-enable them when needed
            gameObject.transform.Find("#Constitution/Body").gameObject.GetComponent<SpriteRenderer>().enabled = false;
            gameObject.transform.Find("#Constitution/Body/BlobShadow").gameObject.GetComponent<SpriteRenderer>().enabled = false;
            
            // Disable the NPC dialog prompt, won't be used here
            gameObject.transform.Find("ACT_Interaction").gameObject.SetActive(false);

            
            GameObject esdrasDefeatAnimator = GameObject.Find("EsdrasDefeatAnimator");
            if(null == esdrasDefeatAnimator)
            {
                Main.IterTormenti.LogError("Failed to patch 'EsdrasNPC': 'EsdrasDefeatAnimator' object not found!");
                return false;
            }

            AnimatorBehaviour esdrasBehaviour = esdrasDefeatAnimator.GetComponent<AnimatorBehaviour>();
            if(null == esdrasBehaviour)
            {
                Main.IterTormenti.LogError("Failed to patch 'EsdrasNPC': 'esdrasBehaviour' object not found!");
                return false;
            }

        #endregion Find Required Objects
        #region Build FSM States


            Main.IterTormenti.Log("Patching '" + gameObject.name + ":" + fsm.name + "' FSM...");


            // Wait until the Penitent object is in the scene
            FsmState waitForPenitent = new(fsm.Fsm);
            {
                waitForPenitent.Name = "Wait for Penitent";

                fsm.AddVariable<FsmGameObject>("Penitent");

                WaitForGameObject waitForObject = new()
                {
                    withTag = "Penitent",
                    store = fsm.GetVariable<FsmGameObject>("Penitent")
                };

                waitForPenitent.AddAction(waitForObject);
            }
            
            FsmState waitForBossfightStart = new(fsm.Fsm);
            {
                waitForBossfightStart.Name = "Wait for Bossfight start";
            }

            FsmState waitForBossfightEnd = new(fsm.Fsm);
            {
                waitForBossfightEnd.Name = "Wait for Bossfight end";
            }

            FsmState waitForAnimatorEnd = new(fsm.Fsm);
            {
                waitForAnimatorEnd.Name = "Wait for Animator end";
            }

            // Reset Esdras facing so final animation plays correctly
            FsmState resetEsdrasFacing = new(fsm.Fsm);
            {
                resetEsdrasFacing.Name = "Reset Esdras Facing";

                CallMethod callMethod = new();
                {
                    var methodParams = new List<FsmVar>();
                    
                    FsmObject target = new()
                    {
                        Value = esdrasBehaviour,
                        ObjectType = esdrasBehaviour.GetType()
                    };
                    
                    callMethod.behaviour = target;
                    callMethod.methodName = "ResetEsdrasFacing";
                    callMethod.parameters = methodParams.ToArray();
                }

                resetEsdrasFacing.AddAction(callMethod);
            }


        #endregion Build FSM States
        #region Add States to FSM


            fsm.AddState(waitForBossfightStart);
            fsm.AddState(waitForBossfightEnd);
            fsm.AddState(waitForAnimatorEnd);
            fsm.AddState(resetEsdrasFacing);


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

            // Wait until Animator changes state
            {
                FsmState getReward = fsm.GetState("Get reward");
                getReward.ChangeTransition("reward given", waitForAnimatorEnd.Name);
            }

            // Insert Esdras facing reset before he leaves
            {
                FsmState wait16 = fsm.GetState("Wait 16");
                wait16.ChangeTransition(FsmEvent.Finished.Name, "Reset Esdras Facing");
                resetEsdrasFacing.AddTransition(FsmEvent.Finished.Name, "Move back 3");
            }


        #endregion Update FSM Workflow

            return true;
        }      
    }
}