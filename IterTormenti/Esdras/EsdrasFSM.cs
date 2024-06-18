
using UnityEngine;
using HutongGames.PlayMaker;
using HutongGames.PlayMaker.Actions;
using Tools.Playmaker2.Condition;
using Tools.Playmaker2.Action;

using Framework.Managers;
using Tools.PlayMaker.Action;
using System;
using Epic.OnlineServices.P2P;

namespace IterTormenti.Esdras
{
    public abstract class FSMChanges
    {
        public static void Apply()
        {

            DEVSTUFF(); // TODO: Remove when done

            // We need to override FSM behaviour ONLY when the Esdras fight is to be skipped by the Incomplete Scapular,
            // as such, check first if the conditions match, and exit if not:

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

            // Have the three wounds been adquired?
            if(    !Core.Events.GetFlag("D01Z06S01_BOSSDEAD")
                || !Core.Events.GetFlag("D02Z05S01_BOSSDEAD")
                || !Core.Events.GetFlag("D03Z04S01_BOSSDEAD") )
            {
                Main.IterTormenti.Log("IterTormenti.Esdras.FSMChanges.Apply: Esdras can't be fought yet! Ending execution");
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
            // These are the GameObjects that we need to modify:
            //  -EsdrasFightActivator:  Active when entering the scene, its FSM will decide whether to enable/disable the other two GameObjects depending on conditions.
            //  -EsdrasNPC: This manages all Esdras NPC interactions across the entire game. The FSM determines which animations/actions to perform based on active scene
            //              and conditions.
            //              This contains the animation frames used for Esdras reacting to Perpetvua (dropping his weapon, kneeling), so we can't just use the BossFight
            //              below, and need to use this.
            //  -BossFight: This manages the BossFight itself, playing the intro animations, activating the fight, and doing cleanup aftwerwards.


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

            // TODO: If there's an error, crash to the main menu or something? If any of these fail, and the others don't, we might get unstable behaviour
        }

        private static bool UpdateEsdrasFightActivator()
        {
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

            // GameObject esdrasNPC = GameObject.Find("EsdrasNPC");
            // if(null == esdrasNPC)
            // {
            //     Main.IterTormenti.LogError("Failed to patch 'EsdrasFightActivator': 'EsdrasNPC' object not found!");
            //     return false;
            // }

            // GameObject bossFightStuff = GameObject.Find("BOSS_FIGHT_STUFF");
            // if(null == bossFightStuff)
            // {
            //     Main.IterTormenti.LogError("Failed to patch 'EsdrasFightActivator': 'BOSS_FIGHT_STUFF' object not found!");
            //     return false;
            // }

                
            Main.IterTormenti.Log("Patching '" + gameObject.name + ":" + fsm.name + "' FSM...");


            
            // FsmState activateALL = new FsmState(fsm.Fsm);
            // {
            //     activateALL.Name = "TEST ACTIVATE ALL";
                
            //     ActivateGameObject activateNPC = new ActivateGameObject();
            //     {
            //         FsmOwnerDefault target = new FsmOwnerDefault()
            //         {
            //             GameObject = new FsmGameObject(esdrasNPC),
            //             OwnerOption = OwnerDefaultOption.SpecifyGameObject
            //         };

            //         activateNPC.gameObject = target;
            //         activateNPC.activate = true;
            //     }

            //     ActivateGameObject activateBossStuff = new ActivateGameObject();
            //     {
            //         FsmOwnerDefault ownerDef = new FsmOwnerDefault()
            //         {
            //             GameObject = new FsmGameObject(bossFightStuff),
            //             OwnerOption = OwnerDefaultOption.SpecifyGameObject
            //         };
                    
            //         activateBossStuff.gameObject = ownerDef;
            //         activateBossStuff.activate = true;
            //     }
                
            //     activateALL.AddAction(activateNPC);
            //     activateALL.AddAction(activateBossStuff);
            // }


            // fsm.AddState(activateALL);

            // Activate the bossfight directly
            fsm.ChangeGlobalTransition("ON LEVEL READY", "ActivateEsdrasFight");
            //activateALL.AddTransition("FINISHED", "");
            
            return true;
        }

        private static bool UpdateBossFight()
        {
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

            GameObject scapularShine = GameObject.Find("Scapular Shine");
            if(null == scapularShine)
            {
                Main.IterTormenti.LogError("Failed to patch 'BossFight': 'Scapular Shine' object not found!");
                return false;
            }

            GameObject bossFightStuff = GameObject.Find("BOSS_FIGHT_STUFF");
            if(null == bossFightStuff)
            {
                Main.IterTormenti.LogError("Failed to patch 'BossFight': 'BOSS_FIGHT_STUFF' object not found!");
                return false;
            }


            // Objects found! Start patching...
            Main.IterTormenti.Log("Patching '" + gameObject.name + ":" + fsm.name + "' FSM...");
            
            // Send Boss Triggered event
            FsmState bossTriggeredEvent = new FsmState(fsm.Fsm);
            {
                bossTriggeredEvent.Name = "BOSS TRIGGERED";
                
                FsmEvent bossStartEvent = new FsmEvent("ON ESDRAS BOSSFIGHT START");
                bossStartEvent.IsGlobal = true;

                FsmEventTarget eventTarget = new FsmEventTarget()
                {
                    target = FsmEventTarget.EventTarget.BroadcastAll
                };

                SendEvent sendEvent = new SendEvent()
                {
                    sendEvent = bossStartEvent,
                    eventTarget = eventTarget
                };
                
                bossTriggeredEvent.AddAction(sendEvent);
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
            FsmState cleanUp = new FsmState(fsm.GetState("Give control back")); // Duplicate existing state
            {
                cleanUp.Name = "Clean up";
                
                // Remove camera repositioning action, which is of type 'CallMethod'
                cleanUp.RemoveAction( cleanUp.GetIndexOfFirstActionOfType<CallMethod>() );

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

                cleanUp.AddAction(removeBossDiedInputBlock);
                cleanUp.AddAction(removeIntroInputBlock);                
            }

            // Move EsdrasNPC to position of Boss
            FsmState moveEsdrasNPC = new FsmState(fsm.Fsm); // PLACEHOLDER
            {
                moveEsdrasNPC.Name = "Move Esdras NPC";
                // SetPosition setPosition = new SetPosition();
                // {
                //     FsmOwnerDefault target = new FsmOwnerDefault();
                //     target.GameObject = new FsmGameObject(esdrasNPC);
                //     target.OwnerOption = OwnerDefaultOption.SpecifyGameObject;

                //     setPosition.gameObject = target;
                //     setPosition.everyFrame = false;
                //     setPosition.vector = null; // TODO
                // }

                // moveEsdrasNPC.AddAction(setPosition);
            }

            // Move Scapular Shine to position of Penitent (Needed?)
            FsmState moveScapularShine = new FsmState(fsm.Fsm); // PLACEHOLDER
            {
                moveScapularShine.Name = "Move Scapular Shine";
                // SetPosition setPosition = new SetPosition();
                // {
                //     FsmOwnerDefault target = new FsmOwnerDefault();
                //     target.GameObject = new FsmGameObject(scapularShine);
                //     target.OwnerOption = OwnerDefaultOption.SpecifyGameObject;

                //     setPosition.gameObject = target;
                //     setPosition.everyFrame = false;
                //     setPosition.vector = null; // TODO
                // }

                // moveScapularShine.AddAction(setPosition);
            }

            // Switch to NPC
            FsmState switchToNPC = new FsmState(fsm.Fsm);
            {
                switchToNPC.Name = "SWITCH TO NPC";
                
                ActivateGameObject activateNPC = new ActivateGameObject();
                {
                    activateNPC.activate = true;
                    FsmOwnerDefault target = new FsmOwnerDefault();
                    target.GameObject = new FsmGameObject(esdrasNPC);
                    target.OwnerOption = OwnerDefaultOption.SpecifyGameObject;

                    activateNPC.gameObject = target;
                }

                // CallMethod callMethod = new CallMethod();
                // {
                //     FsmString value = "BlockPlayerInput";
                //     FsmVar param = new FsmVar(value);

                //     callMethod.behaviour = new FsmObject(esdrasNPC);
                //     callMethod.methodName = "SetState";
                //     callMethod.parameters = new FsmVar[] { param };
                // }

                SendEvent sendBossDoneEvent = new SendEvent();
                {
                    FsmEvent bossDoneEvent = new FsmEvent("ON ESDRAS BOSSFIGHT DONE")
                    {
                        IsGlobal = true
                    };

                    FsmEventTarget eventTarget = new FsmEventTarget()
                    {
                        target = FsmEventTarget.EventTarget.BroadcastAll
                    };

                    sendBossDoneEvent.sendEvent = bossDoneEvent;
                    sendBossDoneEvent.eventTarget = eventTarget;
                }

                ActivateGameObject deactivateBossStuff = new ActivateGameObject();
                {
                    FsmOwnerDefault ownerDef = new FsmOwnerDefault()
                    {
                        GameObject = new FsmGameObject(bossFightStuff),
                        OwnerOption = OwnerDefaultOption.SpecifyGameObject
                    };
                    
                    deactivateBossStuff.gameObject = ownerDef;
                    deactivateBossStuff.activate = false;
                }
                
                switchToNPC.AddAction(activateNPC);
                // switchToNPC.AddAction(callMethod);
                switchToNPC.AddAction(sendBossDoneEvent);
                switchToNPC.AddAction(deactivateBossStuff);
            }


            // --- Add FSM States ---


            fsm.AddState(bossTriggeredEvent);
            fsm.AddState(choiceDialog);
            fsm.AddState(switchToNPC);
            fsm.AddState(cleanUp);
            fsm.AddState(moveEsdrasNPC);
            fsm.AddState(moveScapularShine);

            
            // --- UPDATE FSM FLOW ---


            // Insert Boss Triggered Event
            {
                FsmState blockPlayerInput = fsm.GetState("BlockPlayerInput");
                blockPlayerInput.ChangeTransition(FsmEvent.Finished.Name, bossTriggeredEvent.Name);

                bossTriggeredEvent.AddTransition(FsmEvent.Finished.Name, "Wait 3");
            }

            // Insert dialog into flow
            {
                FsmState introWait = fsm.GetState("IntroWait");
                introWait.ChangeTransition(FsmEvent.Finished.Name, cleanUp.Name);//choiceDialog.Name);
                
                //choiceDialog.AddTransition(FsmEvent.Finished.Name, cleanUp.Name); // PLACEHOLDER
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

            // Insert cleanup into flow
            {
                FsmState guiltReset = fsm.GetState("Guilt reset");
                guiltReset.ChangeTransition(FsmEvent.Finished.Name, cleanUp.Name);
            }

            // Insert moveEsdrasNPC into flow
            {
                cleanUp.AddTransition(FsmEvent.Finished.Name, moveEsdrasNPC.Name);
            }

            // Insert moveScapularShine into flow
            {
                moveEsdrasNPC.AddTransition(FsmEvent.Finished.Name, moveScapularShine.Name);
            }

            // Insert switchToNPC into flow
            {
                moveEsdrasNPC.AddTransition(FsmEvent.Finished.Name, switchToNPC.Name);
            }           

            return true;
        }

        private static bool UpdateEsdrasNPC()
        {
            // Find required objects first

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


            Main.IterTormenti.Log("Patching '" + gameObject.name + ":" + fsm.name + "' FSM...");


            FsmState waitForBossfightStart = new FsmState(fsm.Fsm);
            {
                waitForBossfightStart.Name = "Wait for Bossfight trigger";
            }


            FsmState waitForBossfightEnd = new FsmState(fsm.Fsm);
            {
                waitForBossfightEnd.Name = "Wait for Bossfight end";
            }


            // --- Add States to FSM ---


            fsm.AddState(waitForBossfightStart);
            fsm.AddState(waitForBossfightEnd);


            // --- UPDATE FSM FLOW ---

            
            fsm.ChangeGlobalTransition("ON LEVEL READY", waitForBossfightStart.Name);

            // Setup camera, it will be handled by this FSM, then wait for bossfight to end
            {
                FsmState setCamera = fsm.GetState("SetCamera");

                waitForBossfightStart.AddTransition("ON ESDRAS BOSSFIGHT START", setCamera.Name);

                setCamera.ChangeTransition(FsmEvent.Finished.Name, waitForBossfightEnd.Name);
            }

            // We need to block input here to avoid having to manage input blocks across different FSMs, each
            // one should clear their own input blockers before ending.
            {
                waitForBossfightEnd.AddTransition("ON ESDRAS BOSSFIGHT DONE", "BlockPlayerInput");
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


            return true;
        }

        private static void DEVSTUFF()
        {

            Main.IterTormenti.Log("Flag 'D08Z01S01_BOSSDEAD': " + (Core.Events.GetFlag("D08Z01S01_BOSSDEAD")?"TRUE":"FALSE"));
            Main.IterTormenti.Log("Flag 'ESDRAS_CHAPEL':      " + (Core.Events.GetFlag("ESDRAS_CHAPEL")?"TRUE":"FALSE"));
            Main.IterTormenti.Log("Item 'QI203' owned?:       " + (Core.InventoryManager.IsQuestItemOwned("QI203")?"YES":"NO"));
            Main.IterTormenti.Log("Flag 'D08Z01S01_BOSSDEAD': " + (Core.Events.GetFlag("D01Z06S01_BOSSDEAD")?"TRUE":"FALSE"));
            Main.IterTormenti.Log("Flag 'D08Z01S01_BOSSDEAD': " + (Core.Events.GetFlag("D02Z05S01_BOSSDEAD")?"TRUE":"FALSE"));
            Main.IterTormenti.Log("Flag 'D08Z01S01_BOSSDEAD': " + (Core.Events.GetFlag("D03Z04S01_BOSSDEAD")?"TRUE":"FALSE"));

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
    }



}