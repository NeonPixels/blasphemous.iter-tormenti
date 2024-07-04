using HarmonyLib;
using Framework.Managers;

using Gameplay.GameControllers.Bosses.EcclesiaBros.Esdras;
using System.Collections;
using System.Threading;
using Gameplay.GameControllers.Enemies.Framework.IA;
using UnityEngine;
using NodeCanvas.BehaviourTrees;

using System;

#if DISABLED //TODO: No longer needed
namespace IterTormenti.Esdras
{
    using Gameplay.GameControllers.Bosses.EcclesiaBros.Esdras;

    namespace AnimationInjector
    {
        using Gameplay.GameControllers.Bosses.EcclesiaBros.Esdras.Animator;

        [HarmonyPatch]
        public class EsdrasAnimationInyector_Patch
        {
            [HarmonyPatch(typeof(EsdrasAnimatorInyector), "Death")]
            public static bool Prefix(ref EsdrasAnimatorInyector __instance)
            {
                Main.IterTormenti.Log("EsdrasAnimationInyector_Patch Death: Start");

                // Call behaviour that moves animator to proper position
                // Disable sprite


                if( !Core.InventoryManager.IsQuestItemOwned("QI203") )
                {
                    Main.IterTormenti.Log("EsdrasAnimationInyector_Patch Death: Scapular not owned! Continue as normal");
                    return true;
                }

                Main.IterTormenti.Log("EsdrasAnimationInyector_Patch Death: Scapular owned! Activate hurt frame");

                if (!(__instance.EntityAnimator == null))
                {
                    if (__instance.EntityAnimator.GetCurrentAnimatorStateInfo(0).IsName("HURT"))
                    {
                        //Debug.Log("PLAY TRICK HURT");
                        //__instance.EntityAnimator.Play("HURT", 0, 0f); // TODO: Make animation last until change to FSM
                        __instance.EntityAnimator.Play("HURT", 0, 50000f); // TODO: Make animation last until change to FSM
                    }
                    else
                    {
                        __instance.EntityAnimator.SetTrigger("HURT");
                    }
                }
                
                return false;
            }
        }
    }
}
#endif


namespace IterTormenti.Playmaker
{
    namespace Conditions
    {
        using Tools.Playmaker2.Condition;

        [HarmonyPatch] // Tools.Playmaker2.Condition.FlagExists
        public class FlagExists_Patch
        {
            [HarmonyPatch(typeof(FlagExists), "OnEnter")]
            public static bool Prefix(ref FlagExists __instance)
            {
                //Main.IterTormenti.Log("Tools.Playmaker2.Condition.FlagExists.OnEnter Prefix");

                // Get data about this flag check
                string flagName   = __instance.flagName.Value.ToUpper().Replace(' ', '_');
                string ownerName = __instance.Owner.name;
                string sceneName  = Core.LevelManager.currentLevel.LevelName;

                bool flag = Core.Events.GetFlag(flagName);

                Main.IterTormenti.Log($"FlagExists::OnEnter Prefix: {{ scene: \"{sceneName}\", owner: \"{ownerName}\", flag: \"{flagName}\", value: {flag} }}");
                
                // switch(sceneName)
                // {
                //     case "D08Z01S01": // Bridge
                //     {
                //         switch(ownerName)
                //         {
                //             case "EsdrasFightActivator":
                //             {
                //                 switch(flagName)
                //                 {
                //                     case "D01Z06S01_BOSSDEAD":
                //                     case "D02Z05S01_BOSSDEAD":
                //                     case "D03Z04S01_BOSSDEAD":
                //                     {
                //                         flag = true;
                //                         Main.IterTormenti.Log($"Overriding flag: \"{flagName}\"  => value: {flag}");
                //                         break;
                //                     }
                //                     case "D08Z01S01_BOSSDEAD":  // Esdras Dead
                //                     {
                //                         flag = false;
                //                         Main.IterTormenti.Log($"Overriding flag: \"{flagName}\"  => value: {flag}");
                //                         break;
                //                     }
                //                     default: break;
                //                 }
                //                 break;
                //             }
                //             case "EsdrasNPC":
                //             {
                //                 switch(flagName)
                //                 {
                //                     case "ESDRAS_CHAPEL":  // Esdras already in chapel
                //                     default: break;
                //                 }
                //                 break;
                //             }
                //             case "BossItemChecker":
                //             {
                //                 switch(flagName)
                //                 {
                //                     case "D08Z01S01_BOSSDEAD": // Esdras Dead
                //                     default: break;
                //                 }
                //                 break;
                //             }
                //             default: break;
                //         }
                        
                //         break;
                //     }
                //     default: break;
                // }

                // Finish action
                if (__instance.outValue != null)
                    __instance.outValue.Value = flag;
                if (flag)
                    __instance.Fsm.Event(__instance.flagAvailable);
                else
                    __instance.Fsm.Event(__instance.flagUnavailable);
                __instance.Finish();
                
                return false;
            }
        }
        
        // [HarmonyPatch] // Tools.Playmaker2.Condition.ItemIsOwned
        // public class ItemIsOwned_Patch
        // {
        //     [HarmonyPatch(typeof(ItemIsOwned), "executeAction")]
        //     public static bool Prefix(  ref ItemIsOwned __instance,
        //                                 string objectIdStting,
        //                                 InventoryManager.ItemType objType,
        //                                 int slot,
        //                                 ref bool __result )
        //     {
        //         string ownerName = __instance.Owner.name;
        //         string sceneName = Core.LevelManager.currentLevel.LevelName;

        //         string itemName = objectIdStting.ToUpper().Replace(' ', '_');
        //         string itemType = nameof(objType);
                                
        //         Main.IterTormenti.Log($"ItemIsOwned::executeAction Prefix: {{ scene: \"{sceneName}\", owner: \"{ownerName}\", item: \"{itemName}\", type: \"{itemType}\", slot: {slot} }}");

        //         switch(sceneName)
        //         {
        //             case "D08Z01S01": // Bridge
        //             {
        //                 switch(ownerName)
        //                 {
        //                     case "EsdrasFightActivator":
        //                     {
        //                         switch(itemName)
        //                         {
        //                             case "QI203":                                    
        //                             {
        //                                 __result = true;
        //                                 Main.IterTormenti.Log($"Overriding possession of item: \"{itemName}\"  => value: {__result}");
        //                                 return false;
        //                             }                                    
        //                             default: break;
        //                         }
        //                         break;
        //                     }
        //                     default: break;
        //                 }
                        
        //                 break;
        //             }
        //             default: break;
        //         }

        //         return true;
        //     }
        // }

        // [HarmonyPatch] // Tools.Playmaker2.Condition.EntityIsPenitent
        // public class EntityIsPenitent_Patch
        // {
        //     [HarmonyPatch(typeof(EntityIsPenitent), "OnEnter")]
        //     public static bool Prefix(  ref EntityIsPenitent __instance )
        //     {
        //         string ownerName = __instance.Owner.name;
        //         string sceneName = Core.LevelManager.currentLevel.LevelName;

        //         //base.Fsm.Event((!entity.Value.CompareTag("Penitent")) ? onFailure : onSuccess);
        //         string entityName = __instance.entity.Value.name;
                                
        //         Main.IterTormenti.Log($"EntityIsPenitent::OnEnter Prefix: {{ scene: \"{sceneName}\", owner: \"{ownerName}\", entity: \"{entityName}\" }}");

        //         return true;
        //     }
        // }        
    }

    namespace Actions
    {
        using HutongGames.PlayMaker;
        using HutongGames.PlayMaker.Actions;
        using Tools.Playmaker2.Action;

        [HarmonyPatch] // HutongGames.PlayMaker.Actions.CallMethod
        public class CallMethod_Patch
        {
            // [HarmonyPatch(typeof(CallMethod), "DoMethodCall")]
            // public static void Postfix(ref CallMethod __instance)
            // {
            //     string ownerName = __instance.Owner.name;
            //     string sceneName  = Core.LevelManager.currentLevel.LevelName;
            //     string methodName = __instance.methodName == null? "NULL" : __instance.methodName.ToString();
            //     string parameterList = "";
                
            //     if (__instance.parameters == null || __instance.parameters.Length == 0)
            //     {
            //         parameterList = "[]";
            //     }
            //     else
            //     {
            //         parameterList += "[";
                    
            //         for(int idx = 0; idx < __instance.parameters.Length; idx++)
            //         {
            //             string paramType = __instance.parameters[idx].Type.ToString();
                        
            //             parameterList += "{" + paramType + "},";
            //             //TODO: get values
            //         }
                    
            //         parameterList += "]";
            //     }

            //     Main.IterTormenti.Log($"CallMethod::DoMethodCall Postfix: {{ scene: \"{sceneName}\", owner: \"{ownerName}\", method: \"{methodName}\", parameters: {parameterList} }}");
            // }
        }

        [HarmonyPatch] // HutongGames.PlayMaker.Actions.Wait
        public class Wait_Patch
        {
            [HarmonyPatch(typeof(Wait), "OnEnter")]
            public static void Postfix(ref Wait __instance)
            {
                string ownerName = __instance.Owner.name;
                string sceneName  = Core.LevelManager.currentLevel.LevelName;
                float waitTime = __instance.time.Value;
                
                Main.IterTormenti.Log($"Wait::OnEnter Postfix: {{ scene: \"{sceneName}\", owner: \"{ownerName}\", waitTime: {waitTime} }}");

                
            }
        }

        [HarmonyPatch] // Tools.PlayMaker2.Action.DialogStart
        public class DialogStart_Patch
        {
            [HarmonyPatch(typeof(DialogStart), "OnEnter")]
            public static void Postfix(ref DialogStart __instance)
            {
                string ownerName = __instance.Owner.name;
                string sceneName  = Core.LevelManager.currentLevel.LevelName;
                

                string title = (__instance.conversation == null) ? string.Empty : __instance.conversation.Value;
                bool isModal = __instance.modal == null || __instance.modal.Value;
                bool useOnlyLast = __instance.useOnlyLast != null && __instance.useOnlyLast.Value;
                bool dontCloseWideAtEnd = __instance.dontCloseWidetAtEnd != null && __instance.dontCloseWidetAtEnd.Value;
                bool useBackground = __instance.useFullScreenBackgound != null && __instance.useFullScreenBackgound.Value;
                
                
                // if (string.IsNullOrEmpty(text))
                // {
                // 	base.LogWarning("PlayMaker Action Start Conversation - conversation title is blank");
                // }
                // else if (Core.Dialog.StartConversation(text, flag, flag2, !flag3, (int)this.purge.Value, useBackground))
                // {
                // 	Core.Dialog.OnDialogFinished += new DialogManager.DialogEvent(this.DialogEnded);
                // }
                // Core.Logic.Penitent.Animator.SetBool("IS_DIALOGUE_MODE", this.enablePlayerDialogueMode.Value);
                // if (!this.enablePlayerDialogueMode.Value)
                // {
                // 	this.remainDialogueMode.Value = false;
                // }

                Main.IterTormenti.Log($"DialogStart::OnEnter Postfix: {{ scene: \"{sceneName}\", owner: \"{ownerName}\", dialog: {{ title: \"{title}\", isModal: {isModal}, useOnlyLast: {useOnlyLast}, dontCloseWideAtEnd: {dontCloseWideAtEnd}, useBackground: {useBackground} }} }}");
            }

        }


    // Tools.Playmaker.Action.AudioOneShot
	// Tools.Playmaker2.Action.FlagModification
	// Tools.Playmaker2.Action.Teleport
	// Tools.Playmaker2.Action.CutscenePlay
	// Tools.PlayMaker.Action.InputBlock
	// Tools.Playmaker2.Action.CameraFade
	// Tools.Playmaker.Action.AudioSetGlobalParameter
    // Tools.PlayMaker.Action.SaveOnlyManually
    // Tools.Playmaker2.Action.ItemAddition
	// Tools.Playmaker2.Action.ItemAdditionMessage
	// Tools.Playmaker2.Action.GrantAchievement
	// Tools.Playmaker2.Action.ShowFullMessage
	// Tools.PlayMaker.Action.CompletitionPercentageAdd
	// Tools.Playmaker2.Action.GuiltReset
	// Tools.Playmaker2.Action.ShowMessage
	// Tools.Playmaker2.Action.GetGuilt
    // Tools.Playmaker2.Action.CheckGameModeActive
	// Tools.Playmaker2.Action.CheckLastDLC
	// Tools.Playmaker2.Action.ThunderScreenEffect

    }

    namespace Events
    {
        using Tools.Playmaker2.Events;

        [HarmonyPatch] // Tools.Playmaker2.Events.EntityDead
        public class EntityDead_Patch
        {
            [HarmonyPatch(typeof(EntityDead), "Dead")]
            public static void Postfix(ref EntityDead __instance)
            {
                string ownerName = __instance.Owner.name;
                string sceneName  = Core.LevelManager.currentLevel.LevelName;
                string entityName = __instance.entity.Value.name;

                //Main.IterTormenti.Log($"EntityDead::Dead Postfix: {{ scene: \"{sceneName}\", owner: \"{ownerName}\", entity: \"{entityName}\" }}");

                // Check if Esdras has died
                if(sceneName.Equals("D08Z01S01") && ownerName.Equals("BossFight") && entityName.Equals("Esdras"))
                {
                    Main.IterTormenti.Log($"EntityDead::Dead Postfix: {{ scene: \"{sceneName}\", owner: \"{ownerName}\", entity: \"{entityName}\" }}");
                }

            }


            // [HarmonyPatch(typeof(Tools.Playmaker2.Events.EntityDead), "OnEnter")]
            // public static void Postfix(ref EntityDead __instance)
            // {
            //     string ownerName = __instance.Owner.name;
            //     string sceneName  = Core.LevelManager.currentLevel.LevelName;
            //     string entityName = __instance.entity.Value.name;

            //     Main.IterTormenti.Log($"EntityDead Postfix: Scene: {sceneName} - '{ownerName}' is killing {entityName}");
            // }
        }

        [HarmonyPatch] // Tools.Playmaker2.Events.InteractableInteractionStarted
        public class InteractableInteractionStarted_Patch
        {
            // [HarmonyPatch(typeof(InteractableInteractionStarted), "OnEnter")]
            // public static void Postfix(ref InteractableInteractionStarted __instance)
            // {
            //     string ownerName = __instance.Owner.name;
            //     string sceneName  = Core.LevelManager.currentLevel.LevelName;

            //     bool listenOnlySelf = __instance.listenOnlySelf.Value;

            //     Main.IterTormenti.Log($"InteractableInteractionStarted::OnEnter Postfix: {{ scene: \"{sceneName}\", owner: \"{ownerName}\", listenOnlySelf: \"{listenOnlySelf}\" }}");
            // }
        }
    }

    namespace FSM
    {
        using HutongGames.PlayMaker;

        [HarmonyPatch]
        public class SwitchState_Patch
        {
            [HarmonyPatch(typeof(HutongGames.PlayMaker.Fsm), "SwitchState")]
            public static bool Prefix(  ref Fsm __instance )
            {
                if(    __instance.GameObjectName.Equals("EsdrasFightActivator")
                    || __instance.GameObjectName.Equals("BossFight")
                    || __instance.GameObjectName.Equals("EsdrasNPC") )
                {
                    Main.IterTormenti.Log($"{__instance.GameObjectName}::{__instance.Name} SwitchState Prefix: ActiveState: {__instance.ActiveStateName} ==>");
                }
                
                return true;
            }

            [HarmonyPatch(typeof(HutongGames.PlayMaker.Fsm), "SwitchState")]
            public static void Postfix(ref Fsm __instance )
            {
                if(    __instance.GameObjectName.Equals("EsdrasFightActivator")
                    || __instance.GameObjectName.Equals("BossFight")
                    || __instance.GameObjectName.Equals("EsdrasNPC") )
                {
                    Main.IterTormenti.Log($"{__instance.GameObjectName}::{__instance.Name} SwitchState Postfix: ActiveState: {__instance.ActiveStateName} <==");
                }
            }
        }

    }
	
}

    
   


/*    
    [HarmonyPatch]
    public class EnemyBehaviour_ReversePatch
    {
        [HarmonyReversePatch]
        [HarmonyPatch(typeof(EnemyBehaviour), "BehaviourTree", MethodType.Getter)]
        public static BehaviourTreeOwner BehaviourTree(object instance)
        {
            // Reverse Patch to allow using a private method
            // This is a stub, should never be called
            return null;
        }
    } 

    [HarmonyPatch]
    public class EsdrasBehaviour_ReversePatch
    {
        [HarmonyReversePatch]
        [HarmonyPatch(typeof(EsdrasBehaviour), "ChangePhase")]
        public static void ChangePhase(object instance, EsdrasBehaviour.ESDRAS_PHASES phase)
        {
            // Reverse Patch to allow using a private method
            // This is a stub, should never be called
        }

        [HarmonyReversePatch]
        [HarmonyPatch(typeof(EsdrasBehaviour), "ActivateCollisions")]
        public static void ActivateCollisions(object instance, bool activate)
        {
            // Reverse Patch to allow using a private method
            // This is a stub, should never be called
        }

        [HarmonyReversePatch]
        [HarmonyPatch(typeof(EsdrasBehaviour), "StartWaitingPeriod")]
        public static void StartWaitingPeriod(object instance, float seconds)
        {
            // Reverse Patch to allow using a private method
            // This is a stub, should never be called
        }
    } 


    
    [HarmonyPatch(typeof(EsdrasBehaviour), "IntroSequenceCoroutine")]
    public class EsdrasIntroSequenceCoroutine_PassthroughPatch
    {
        public static IEnumerator Postfix( IEnumerator values, ref EsdrasBehaviour __instance)
        {
            Main.IterTormenti.Log("EsdrasBehaviour::IntroSequenceCoroutine: passthrough");

            //__instance.ChangePhase(EsdrasBehaviour.ESDRAS_PHASES.FIRST);
            EsdrasBehaviour_ReversePatch.ChangePhase(__instance, EsdrasBehaviour.ESDRAS_PHASES.FIRST);
			__instance.Esdras.AnimatorInyector.SpinAttack();//.Taunt();
			yield return new WaitForSeconds(1.5f);
			__instance.LookAtPenitent();
			yield return new WaitForSeconds(1.5f);
			//__instance.base.BehaviourTree.StartBehaviour();
            EnemyBehaviour_ReversePatch.BehaviourTree(__instance).StartBehaviour();
			//__instance.ActivateCollisions(true);
            EsdrasBehaviour_ReversePatch.ActivateCollisions(__instance, true);
			//__instance.StartWaitingPeriod(0.1f);
            EsdrasBehaviour_ReversePatch.StartWaitingPeriod(__instance, 0.1f);
        }


        // public void StartIntroSequence()
		// {
		// 	this._fsm.ChangeState(this.stIntro);
		// 	this.ActivateCollisions(false);
		// 	this.StartAttackAction();
		// 	this.SetCurrentCoroutine(base.StartCoroutine(this.IntroSequenceCoroutine()));
		// }

		// [DebuggerHidden]
		// private IEnumerator IntroSequenceCoroutine()
		// {
		// 	this.ChangePhase(EsdrasBehaviour.ESDRAS_PHASES.FIRST);
		// 	this.Esdras.AnimatorInyector.Taunt();
		// 	yield return new WaitForSeconds(1.5f);
		// 	this.LookAtPenitent();
		// 	yield return new WaitForSeconds(1.5f);
		// 	base.BehaviourTree.StartBehaviour();
		// 	this.ActivateCollisions(true);
		// 	this.StartWaitingPeriod(0.1f);
		// }
    }

*/






