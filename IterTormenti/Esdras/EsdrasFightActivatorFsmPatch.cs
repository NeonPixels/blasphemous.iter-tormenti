using UnityEngine;
using HutongGames.PlayMaker;
using HutongGames.PlayMaker.Actions;

namespace IterTormenti.Esdras
{
    public abstract class FightActivatorFsmPatch
    {
        /// <summary>
        /// Update the EsdrasFightActivator FSM to perform the following actions:
        ///     - Make sure that both the Bossfight and EsdrasNPC game objects are activated immediately.
        /// </summary>
        /// <returns>'true' if no errors happen, 'false' otherwise</returns>
        public static bool Apply()
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
            
            FsmState activateEverything = new(fsm.Fsm);
            {
                activateEverything.Name = "ActivateEverything";
                
                ActivateGameObject activateNPC = new();
                {
                    FsmOwnerDefault target = new();
                    {                        
                        FsmGameObject fsmGameObject = new()
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

                ActivateGameObject activateBossStuff = new();
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
    }
}