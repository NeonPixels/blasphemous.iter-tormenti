using Blasphemous.ModdingAPI;
using Blasphemous.ModdingAPI.Files;
using Blasphemous.Framework.Levels;
using Blasphemous.Framework.Levels.Modifiers;
using System;
using UnityEngine;
using Framework.Inventory;
using Framework.Util;

using IterTormenti.Utils.Sprites;
using Blasphemous.ModdingAPI.Helpers;

namespace IterTormenti
{
    /// <summary>
    /// Allows adding collectable items with custom sprites.
    /// JSON format:
    ///     {
    ///         "type": "item-ground-custom",
    ///         "id": "<Collectable item ID>",
    ///         "position": {
    ///             "x": <x position>,
    ///             "y": <y position>
    ///         },
    ///         "condition": "<spawn condition (optional)>",
    ///         "properties": [
    ///             "<spritesheet filename>",
    ///             "<frame width>",
    ///             "<frame height>",
    ///             "<frame delay>"
    ///         ]
    ///     }
    ///  
    ///  Collectable item ID <string>: Identifier of the item to be awarded, must be a valid item id.
    ///  X/Y position <int>: Position of the item on the map
    ///  Spawn condition <string>: Special spawn conditions for the item (optional)
    ///  Properties:
    ///     - Spritesheet filename <string>: Name of the image containing the sprites
    ///     - Frame width <int>:  Width of the individual sprite frame
    ///     - Frame height <int>: Height of the individual sprite frame
    ///     - Frame delay <float>: Time, in seconds, between each frame.
    ///
    /// If there is a single sprite, the 'frame delay' value is not used.
    /// If the 'frame delay' value is set to 0.0, only the first frame of the spritesheet
    /// will be used, with no animation.
    /// If the 'frame delay' is negative, the animation will be reversed.
    /// 
    /// Note: The sprite pivot is set to the middle horizontally, and to the bottom vertically.
    /// </summary>
    public class CustomGroundItemModifier : IModifier
    {
        public void Apply(GameObject obj, ObjectData data)
        {
            obj.name = $"Ground item {data.id}";

            UniqueId idComp = obj.GetComponent<UniqueId>();
            idComp.uniqueId = "ITEM-GROUND-" + data.id;

            InteractableInvAdd addComp = obj.GetComponent<InteractableInvAdd>();
            addComp.item = data.id;
            addComp.itemType =  ItemHelper.GetItemTypeFromId(data.id);

            if(data.properties.Length < 4)
            {
                return; // Nothing else, just a regular item
            }

            string spriteSheetPath = data.properties[0];
            int spriteWidth  = int.Parse(data.properties[1]);
            int spriteHeight = int.Parse(data.properties[2]);
            float frameDelay = float.Parse(data.properties[3]);

            GameObject interactableAnimation = obj.transform.Find("Interactable Animation").gameObject;
            if(null == interactableAnimation)
            {
                ModLog.Error("CustomGroundItemModifier::Apply: ERROR - Failed to find Interactable Animator object");
                return;
            }
            
            Sprite[] sprites;

            // Load sprites
            {
                Vector2 frameSize = new(spriteWidth, spriteHeight);
                
                SpriteImportOptions importOptions = new()
                {
                    Pivot = new Vector2(0.5f,0.0f)
                };

                Main.IterTormenti.FileHandler.LoadDataAsFixedSpritesheet( spriteSheetPath,
                                                                          frameSize,
                                                                          out sprites,
                                                                          importOptions );
            }

            if(sprites.Length <= 0)
            {
                ModLog.Error($"CustomGroundItemModifier::Apply: ERROR - Failed to load sprites from: {spriteSheetPath}");
                return;
            }

            // Disable existing animator
            Animator animator = interactableAnimation.GetComponent<Animator>();
            if(null == animator)
            {
                ModLog.Error("CustomGroundItemModifier::Apply: ERROR - Failed to find Animator component");
                return;
            }

            animator.enabled = false;

            SpriteRenderer renderer = interactableAnimation.GetComponent<SpriteRenderer>();
            if(null == renderer)
            {
                ModLog.Error("CustomGroundItemModifier::Apply: ERROR - Failed to find SpriteRenderer component");
                return;
            }

            if(1 == sprites.Length || 0.0f == frameDelay) // Single sprite
            {
                renderer.sprite = sprites[0];
            }
            else
            {
                AnimatedSprite itemAnim = obj.AddComponent<AnimatedSprite>();
                {
                    itemAnim.Renderer = renderer;
                    itemAnim.frames = new Sprite[sprites.Length];
                    Array.Copy( sprites, 0, itemAnim.frames, 0, sprites.Length );
                    itemAnim.Delay = frameDelay;
                    itemAnim.Loop = true;

                    itemAnim.Play();
                }
            }
        }
    }
}
