using UnityEngine;
using HutongGames.PlayMaker;
using HutongGames.PlayMaker.Actions;
using IterTormenti.FSMUtils;

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

            // Disable sprite directly, we will re-enable it when needed
            gameObject.transform.Find("#Constitution/Body").gameObject.GetComponent<SpriteRenderer>().enabled = false;

            // Disable the NPC dialog prompt, won't be used here
            gameObject.transform.Find("ACT_Interaction").gameObject.SetActive(false);

            // TODO: If Esdras is too close to Penitent, make him walk away, so the animations are visible
            // TODO: Turn animator around to face penitent!

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

            // Attach Perpetva position to Penitent position
            FsmState attachPerpetvaToPenitent = new(fsm.Fsm);
            {
                attachPerpetvaToPenitent.Name = "Attach Perpetva to Penitent";
                //TODO
            }

            FsmState waitForBossfightStart = new(fsm.Fsm);
            {
                waitForBossfightStart.Name = "Wait for Bossfight start";
            }

            FsmState waitForBossfightEnd = new(fsm.Fsm);
            {
                waitForBossfightEnd.Name = "Wait for Bossfight end";
            }

            // Updates the facing of Esdras and the Penitent
            // Note: Perpetva is already facing the same direction as the Penitent
            FsmState updateFacing = new(fsm.Fsm);
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

            // Skip Esdras intro, it has already been done by the BossFight object
            {
                FsmState wait9 = fsm.GetState("Wait 9");
                wait9.ChangeTransition(FsmEvent.Finished.Name, "Make taunt animation 2");//"Play IdleDazzle animation");
            }


        #endregion Update FSM Workflow

            return true;
        }      
    }
}