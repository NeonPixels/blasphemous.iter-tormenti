using UnityEngine;
using System;
using Blasphemous.ModdingAPI.Files;
using IterTormenti.Utils.Sprites;
using IterTormenti.Utils.Sprites.Animations;
using IterTormenti.Utils.Audio;

namespace IterTormenti.Esdras
{
    public abstract class DefeatAnimation
    {
        /// <summary>
        /// TODO
        /// </summary>
        /// <returns></returns>
        public static bool Create()
        {
            // ---- Get needed GameObjects ----

            GameObject esdrasBoss = GameObject.Find("Esdras");
            if(null == esdrasBoss)
            {
                Main.IterTormenti.LogError("Failed to create Defeat Animation: 'Esdras' object not found!");
                return false;
            }
            
            GameObject esdrasNPC = GameObject.Find("EsdrasNPC");
            if(null == esdrasNPC)
            {
                Main.IterTormenti.LogError("Failed to patch Defeat Animation: 'EsdrasNPC' object not found!");
                return false;
            }

            GameObject bossFightStuff = GameObject.Find("BOSS_FIGHT_STUFF");
            if(null == bossFightStuff)
            {
                Main.IterTormenti.LogError("Failed to create Defeat Animation: 'BOSS_FIGHT_STUFF' object not found!");
                return false;
            }

            GameObject esdrasTarget = GameObject.Find("Esdras destination Point");
            if(null == esdrasTarget)
            {
                Main.IterTormenti.LogError("Failed to create Defeat Animation: 'Esdras destination Point' object not found!");
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
                Main.IterTormenti.LogError("Failed to create Defeat Animation: 'PerpetvaAppears_SimpleVFX' object not found!");
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

                SpriteImportOptions importOptions = new()
                {
                    Pivot = new Vector2(0.5f,0.1024f) // Sprite is 8 pixels higher than it should
                };

                Vector2 frameSize = new(256.0f,128.0f);
                const float animationDelay = 0.1f; // TODO: Find exact delay. 100fps?

                SpriteAnimator animator = esdrasDefeatAnimator.AddComponent<SpriteAnimator>();
                {
                    animator.Renderer = renderer;

                    animator.AudioPlayer = new EsdrasAudioPlayer();
                    
                    // Load sprites into SpriteAnimator
                    {
                        Sprite[] spritesA;
                        Sprite[] spritesB;
                        Sprite[] spritesC;
                        Sprite[] spritesD;
                        Main.IterTormenti.FileHandler.LoadDataAsFixedSpritesheet("EsdrasNonLethalDefeat.png", frameSize, out spritesA, importOptions);
                        Main.IterTormenti.FileHandler.LoadDataAsFixedSpritesheet("EsdrasDefeated.png", frameSize, out spritesB, importOptions);
                        Main.IterTormenti.FileHandler.LoadDataAsFixedSpritesheet("EsdrasPickupWeapon.png", frameSize, out spritesC, importOptions);
                        // Load existing resources                        
                        {
                            spritesD = new Sprite[1];
                            spritesD[0] = Resources.Load("Esdras_run_anim_0", typeof(Sprite)) as Sprite;

                            Main.IterTormenti.Log("Loaded asset is: " + (spritesD[0] == null? "NULL" : "NOT NULL") );
                        }

                        if(spritesA.Length < 26)
                        {
                            Main.IterTormenti.LogError($"Failed loading 'EsdrasNonLethalDefeat.png', received {spritesA.Length} frames!");
                            return false;
                        }
                        
                        if(spritesB.Length < 3)
                        {
                            Main.IterTormenti.LogError($"Failed loading 'EsdrasDefeated.png', received {spritesB.Length} frames!");
                            return false;
                        }

                        if(spritesC.Length < 15)
                        {
                            Main.IterTormenti.LogError($"Failed loading 'EsdrasPickupWeapon.png', received {spritesC.Length} frames!");
                            return false;
                        }

                        animator.sprites = new Sprite[26 + 3 + 15 + 1];

                        
                        Array.Copy( spritesA, 0, animator.sprites, 0, 26 );
                        Array.Copy( spritesB, 0, animator.sprites, 26, 3 );
                        Array.Copy( spritesC, 0, animator.sprites, 29, 15 );
                        Array.Copy( spritesD, 0, animator.sprites, 44, 1 );
                    }

                    // Build animations
                    {
                        SpriteAnimation esdrasNonLethalDefeat = new("EsdrasNonLethalDefeat")
                        {
                            DefaultDelay = animationDelay,
                            frames = new Frame[]
                            {
                                new(0),  new(1),  new(2),  new(3),  new(4),  new(5),
                                new(6),  new(7),  new(8),  new(9),  new(10), new(11),
                                new(12), new(13), new(14), new(15), new(16), 
                                new(17){ Audio = new AudioEventArgs("EsdrasGroundHit") },
                                new(18), new(19), new(20), new(21), new(22), new(23),
                                new(24), new(25)
                            }
                        };
                        animator.Animations.Add(esdrasNonLethalDefeat.Name, esdrasNonLethalDefeat);

                        SpriteAnimation esdrasDefeated = new("EsdrasDefeated")
                        {
                            DefaultDelay = animationDelay,
                            frames = new Frame[]
                            {
                                new(26, animationDelay*2),
                                new(27),
                                new(28, animationDelay*2),
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
                                new(35){ Audio = new AudioEventArgs("EsdrasNormalAttack") },
                                new(36), new(37), new(38),
                                new(39), new(40), new(41),
                                new(42), new(43)
                            }
                        };                        
                        animator.Animations.Add(esdrasPickUpWeapon.Name, esdrasPickUpWeapon);

                        SpriteAnimation esdrasRun = new("EsdrasRun")
                        {
                            DefaultDelay = animationDelay,
                            frames = new Frame[]
                            {
                                new(44) //TODO: Step sounds
                            }
                        };                        
                        animator.Animations.Add(esdrasRun.Name, esdrasRun);
                    }

                    Main.IterTormenti.Log("Animator: " + animator.ToString());

                    animator.OnEndTransitions["EsdrasNonLethalDefeat"] = "EsdrasDefeated";
                    animator.MakeAnimationLoop("EsdrasDefeated");
                    animator.MakeAnimationLoop("EsdrasRun");

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

            float xVal = -180f + 10.5625f + 102.28f -0.02999878f; //TODO: Just set it to the bossfight center position?
            float yVal = 9f - 0.96875f;
            esdrasDefeatAnimator.transform.position = new Vector3(xVal,yVal,0.0f);
            esdrasDefeatAnimator.SetActive(true);

            return true;
        }
    }
}