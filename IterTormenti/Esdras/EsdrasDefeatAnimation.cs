using Blasphemous.ModdingAPI;
using Blasphemous.ModdingAPI.Files;
using IterTormenti.Utils.Sprites;
using IterTormenti.Utils.Sprites.Animations;
using IterTormenti.Utils.Audio;
using UnityEngine;
using System;

namespace IterTormenti.Esdras
{
    public abstract class DefeatAnimation
    {
        /// <summary>
        /// This creates the objects managing the Defeat Animation that is used to
        /// transition between the Boss and NPC versions of Esdras.
        /// </summary>
        /// <returns></returns>
        public static bool Create()
        {
            // ---- Get needed GameObjects ----

            GameObject esdrasBoss = GameObject.Find("Esdras");
            if(null == esdrasBoss)
            {
                ModLog.Error("Failed to create Defeat Animation: 'Esdras' object not found!");
                return false;
            }
            
            GameObject esdrasNPC = GameObject.Find("EsdrasNPC");
            if(null == esdrasNPC)
            {
                ModLog.Error("Failed to patch Defeat Animation: 'EsdrasNPC' object not found!");
                return false;
            }

            GameObject bossFightStuff = GameObject.Find("BOSS_FIGHT_STUFF");
            if(null == bossFightStuff)
            {
                ModLog.Error("Failed to create Defeat Animation: 'BOSS_FIGHT_STUFF' object not found!");
                return false;
            }

            GameObject esdrasTarget = GameObject.Find("Esdras destination Point");
            if(null == esdrasTarget)
            {
                ModLog.Error("Failed to create Defeat Animation: 'Esdras destination Point' object not found!");
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
                ModLog.Error("Failed to create Defeat Animation: 'PerpetvaAppears_SimpleVFX' object not found!");
                return false;
            }


            // ---- Create Animator ----


            GameObject esdrasDefeatAnimator = new("EsdrasDefeatAnimator");
            {
                SpriteRenderer renderer = esdrasDefeatAnimator.AddComponent<SpriteRenderer>();
                {
                    renderer.enabled = true;
                    renderer.drawMode = SpriteDrawMode.Simple;
                    renderer.sortingLayerName = "Player";
                    
                    // Have the animation be in front of player so it is visible if both characters end up overlapping
                    renderer.sortingOrder = 2;
                    
                    // This material is needed for the special shader effect when boss is defeated
                    renderer.material = esdrasBoss.transform.Find("#Constitution/Sprite").gameObject.GetComponent<SpriteRenderer>().material;
                }

                SpriteAnimator animator = esdrasDefeatAnimator.AddComponent<SpriteAnimator>();
                {
                    animator.Renderer = renderer;

                    animator.AudioPlayer = new EsdrasAudioPlayer();
                    
                    // Load sprites into SpriteAnimator
                    {
                        Vector2 frameSize = new(256.0f,128.0f);

                        SpriteImportOptions importOptions = new()
                        {
                            Pivot = new Vector2(0.5f,0.1024f) // Sprite is 8 pixels higher than it should
                        };

                        Sprite[] spritesA;
                        Sprite[] spritesB;
                        Sprite[] spritesC;
                        Sprite[] spritesD;
                        Main.IterTormenti.FileHandler.LoadDataAsFixedSpritesheet("EsdrasNonLethalDefeat.png", frameSize, out spritesA, importOptions);
                        Main.IterTormenti.FileHandler.LoadDataAsFixedSpritesheet("EsdrasDefeated.png", frameSize, out spritesB, importOptions);
                        Main.IterTormenti.FileHandler.LoadDataAsFixedSpritesheet("EsdrasPickupWeapon.png", frameSize, out spritesC, importOptions);
                        Main.IterTormenti.FileHandler.LoadDataAsFixedSpritesheet("EsdrasRun.png", frameSize, out spritesD, importOptions);
                        
                        if(spritesA.Length < 26)
                        {
                            ModLog.Error($"Failed loading 'EsdrasNonLethalDefeat.png', received {spritesA.Length} frames!");
                            return false;
                        }
                        
                        if(spritesB.Length < 3)
                        {
                            ModLog.Error($"Failed loading 'EsdrasDefeated.png', received {spritesB.Length} frames!");
                            return false;
                        }

                        if(spritesC.Length < 15)
                        {
                            ModLog.Error($"Failed loading 'EsdrasPickupWeapon.png', received {spritesC.Length} frames!");
                            return false;
                        }

                        if(spritesC.Length < 14)
                        {
                            ModLog.Error($"Failed loading 'EsdrasRun.png', received {spritesD.Length} frames!");
                            return false;
                        }

                        animator.sprites = new Sprite[26 + 3 + 15 + 14];
                        
                        Array.Copy( spritesA, 0, animator.sprites, 0, 26 );
                        Array.Copy( spritesB, 0, animator.sprites, 26, 3 );
                        Array.Copy( spritesC, 0, animator.sprites, 29, 15 );
                        Array.Copy( spritesD, 0, animator.sprites, 44, 14 );
                    }

                    // Build animations
                    {
                        const float animationDelay = 0.1f; // TODO: Find exact delay used in Unity project. 100fps?

                        SpriteAnimation esdrasNonLethalDefeat = new("EsdrasNonLethalDefeat")
                        {
                            DefaultDelay = animationDelay,
                            frames = new Frame[]
                            {
                                new(0),  new(1),  new(2),  new(3),  new(4),  new(5),
                                new(6),  new(7),  new(8),  new(9),  new(10), new(11),
                                new(12), new(13), new(14), new(15), new(16), 
                                new(17){ Audio = new AudioEventArgs(EsdrasAudioPlayer.ESDRAS_GROUND_HIT) },
                                new(18){ Audio = new AudioEventArgs(EsdrasAudioPlayer.ESDRAS_DROP_WEAPON) }, // Playing audio early, as it has leading silence
                                new(19), new(20), new(21), new(22), new(23), new(24), new(25)
                            }
                        };
                        animator.Animations.Add(esdrasNonLethalDefeat.Name, esdrasNonLethalDefeat);

                        SpriteAnimation esdrasDefeated = new("EsdrasDefeated")
                        {
                            DefaultDelay = animationDelay,
                            frames = new Frame[]
                            {
                                new(26, animationDelay*2.0f),
                                new(27),
                                new(28, animationDelay*2.0f),
                                new(27)
                            }
                        };
                        animator.Animations.Add(esdrasDefeated.Name, esdrasDefeated);

                        SpriteAnimation esdrasPickUpWeapon = new("EsdrasPickUpWeapon")
                        {
                            DefaultDelay = animationDelay,
                            frames = new Frame[]
                            {
                                new(29), new(30), new(31),
                                new(32), new(33), new(34),
                                new(35){ Audio = new AudioEventArgs(EsdrasAudioPlayer.ESDRAS_NORMAL_ATTACK) },
                                new(36), new(37), new(38),
                                new(39), new(40), new(41),
                                new(42), new(43)
                            }
                        };                        
                        animator.Animations.Add(esdrasPickUpWeapon.Name, esdrasPickUpWeapon);

                        SpriteAnimation esdrasRun = new("EsdrasRun")
                        {
                            DefaultDelay = animationDelay/2.0f,
                            frames = new Frame[]
                            {
                                new(44), new(45), new(46), new(47), new(48), new(49),
                                new(50){ Audio = new AudioEventArgs(EsdrasAudioPlayer.ESDRAS_FOOTSTEP) },
                                new(51), new(52), new(53), new(54), new(55), new(56),
                                new(57){ Audio = new AudioEventArgs(EsdrasAudioPlayer.ESDRAS_FOOTSTEP) }
                            }
                        };                        
                        animator.Animations.Add(esdrasRun.Name, esdrasRun);
                    }

                    animator.OnEndTransitions["EsdrasNonLethalDefeat"] = "EsdrasDefeated";
                    animator.OnEndTransitions["EsdrasPickUpWeapon"] = "EsdrasRun";

                    animator.enabled = true;
                    animator.ActiveAnimation = "EsdrasNonLethalDefeat";
                }
            
                AnimatorBehaviour esdrasBehaviour = esdrasDefeatAnimator.AddComponent<AnimatorBehaviour>();
                {
                    esdrasBehaviour.EsdrasAnimatorGO = esdrasDefeatAnimator;            
                    esdrasBehaviour.EsdrasNPC        = esdrasNPC;
                    esdrasBehaviour.BossFightStuff   = bossFightStuff;
                    esdrasBehaviour.PerpetvaVFX      = perpetvaAppears;
                    esdrasBehaviour.EsdrasTarget     = esdrasTarget;
                }

                // Copy the NPC shadow and add it as child of the animator
                UnityEngine.Object.Instantiate( esdrasNPC.transform.Find("#Constitution/Body/BlobShadow").gameObject,
                                                esdrasDefeatAnimator.transform,
                                                false );
            }

            esdrasDefeatAnimator.transform.position = esdrasNPC.transform.position;
            esdrasDefeatAnimator.SetActive(true);

            return true;
        }
    }
}
