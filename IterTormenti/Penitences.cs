using Blasphemous.Framework.Penitence;
using Framework.Managers;
using UnityEngine;

using Framework.Penitences;
using Framework.Inventory;
using System.Collections.Generic;
using Tools.Playmaker2.Action;
using Gameplay.UI;
using Gameplay.UI.Others.MenuLogic;


namespace IterTormenti
{
    // TODO: Long penitence descriptions overflow the textbox when rejecting the penitence. Add scrollbar to that textbox?    
    // TODO: Using one of the beads to reset fervour flasks back to health is a custom penitence bug

    /// <summary>
    /// Custom penitences that combine the effects of several other penitences
    /// </summary>
    public abstract class ComboPenitence : ModPenitence
    {

        protected override string Name => Main.IterTormenti.LocalizationHandler.Localize(Id + ".name");

        protected override string Description => Main.IterTormenti.LocalizationHandler.Localize(Id + ".desc");

        protected override string ItemIdToGive => null; // Will be handled by GiveReward method

        protected override InventoryManager.ItemType ItemTypeToGive => InventoryManager.ItemType.Bead; // Will be handled by GiveReward method

        public List<IPenitence> Penitences = new();


        public string id { get { return Id; } } // TODO: Temporary fix until base class gets public Id method

        public static bool IsActive { get; private set; }

        /// <summary>
        /// Use the original penitences to activate functionality
        /// </summary>
        protected override void Activate()
        {
            IsActive = true;
            
            foreach(IPenitence penitence in Penitences)
            {
                penitence.Activate();
            }
        }

        /// <summary>
        /// Use the original penitences to deactivate functionality
        /// </summary>
        protected override void Deactivate()
        {
            IsActive = false;
            
            foreach(IPenitence penitence in Penitences)
            {
                penitence.Deactivate();
            }
        }

        /// <summary>
        /// Mark the base penitences as completed
        /// </summary>
        protected void CompletePenitences()
        {
            List<IPenitence> allPenitences = Core.PenitenceManager.GetAllPenitences();

            foreach (IPenitence penitence in Penitences)
            {
                IPenitence penitenceToComplete = allPenitences.Find((IPenitence x) => x.Id == penitence.Id);
                
                if(null == penitenceToComplete)
                {
                    Main.IterTormenti.LogError("ComboPenitence::" + Id +"::CompletePenitences: Penitence with id: " + penitence.Id + " could not be found!");
                    continue;
                }

                penitenceToComplete.Completed = true;
            }
        }

        /// <summary>
        /// Attempt to add the provided item collection to the player inventory.
        /// For items that succeed, cue a popup notification.
        /// Save game and exist FSM afterwards.
        /// </summary>
        /// <param name="fsmStateAction">FSM Action to complete</param>
        /// <param name="rewards">Collection of items to add</param>
        protected void GiveRewards( PenitenceCheckCurrent fsmStateAction,
                                   List<BaseInventoryObject> rewards )
        {
            // Queue of items sucessfully added to inventory
            Queue<BaseInventoryObject> addedItems = new();

            foreach(BaseInventoryObject reward in rewards)
            {
                if(null == reward)
                {
                    Main.IterTormenti.LogError("ComboPenitence::" + Id +"::GiveRewards: Invalid reward object found!");
                    continue;
                }

                // Attempt to add items to inventory. Will fail if item is already owned.
                if( !Core.InventoryManager.AddBaseObject(reward) )
                {
                    continue;
                }
                
                addedItems.Enqueue(reward);
            }

            // Reduce popup duration time based on total number of items, so the
            // popups dissapear before returning to the main menu
            float timePerPopUp = addedItems.Count > 0? 3f / addedItems.Count : 3f;
                      
            AwardPopUp();
            
            return;
            
            // --- Callback functions ---

            void AwardPopUp()
            {
                PopUpWidget.OnDialogClose -= AwardPopUp;

                if(addedItems.Count == 0)
                {
                    // No items have been added, so nothing left to do
                    SaveAndFinish();
                    return;
                }

                // Get next item we need to show a popup for
                BaseInventoryObject award = addedItems.Dequeue();

                // If we have item popups left to display, set the callback to cue another popup
                // If we're on the last item, set the callback to save and finish
                PopUpWidget.OnDialogClose += addedItems.Count > 0 ? AwardPopUp : SaveAndFinish;

                // if(addedItems.Count > 0)
                // {
                //     Main.IterTormenti.Log("ComboPenitence::" + Id +"::GiveRewards: AwardPopUp -> " + addedItems.Count + " items left!");
                //     PopUpWidget.OnDialogClose += AwardPopUp;
                // }
                // else
                // {
                //     Main.IterTormenti.Log("ComboPenitence::" + Id +"::GiveRewards: AwardPopUp -> Last Item!");
                //     PopUpWidget.OnDialogClose += SaveAndFinish;
                // }

                // Display item popup
                UIController.instance.ShowObjectPopUp( UIController.PopupItemAction.GetObejct,
                                                       award.caption,
                                                       award.picture,
                                                       award.GetItemType(),
                                                       timePerPopUp,
                                                       true );
            }

            // Save game and finish FSM Action
            void SaveAndFinish()
            {
                PopUpWidget.OnDialogClose -= SaveAndFinish;

                Core.Persistence.SaveGame(true);
                fsmStateAction.Fsm.Event(fsmStateAction.noPenitenceActive);
                fsmStateAction.Finish();    
            }
        }

        protected abstract string Spritesheet { get; }

        /// <summary>
        /// Penitence sprite areas
        /// </summary>
        protected Rect[] penitenceSpriteAreas = { new Rect(  0,  0, 94, 110),
                                                  new Rect( 95,  1, 92, 108),
                                                  new Rect(190, 94, 16, 16),
                                                  new Rect(190, 78, 16, 16),
                                                  new Rect(190, 62, 16, 16),
                                                  new Rect(188,  0, 18, 18) };

        protected override void LoadImages(out Sprite inProgress, out Sprite completed, out Sprite abandoned, out Sprite gameplay, out Sprite chooseSelected, out Sprite chooseUnselected)
        {
            if( Main.IterTormenti.FileHandler.LoadDataAsVariableSpritesheet(Spritesheet, penitenceSpriteAreas, out Sprite[] images) )
            {
                chooseSelected   = images[0];
                chooseUnselected = images[1];
                inProgress       = images[2];
                completed        = images[3];
                abandoned        = images[4];
                gameplay         = images[5];
            }
            else
            {
                chooseSelected   =
                chooseUnselected =
                inProgress       =
                completed        =
                abandoned        =
                gameplay         = null;

                Main.IterTormenti.Log("Failed to load sprites for " + Id);
            }
        }
    }

    /// <summary>
    /// Penitence combining the effects and rewards of:
    ///     PE01: Penitence of a Bleeding Heart     => RB101: Reliquary of the Suffering Heart
    ///     PE02: Penitence of the Unwavering Faith => RB102: Reliquary of the Fervent Heart
    /// </summary>
    public class PenitenceAB : ComboPenitence
    {
        public PenitenceAB()
        {
            Penitences = new List<IPenitence>{ new PenitencePE01(),
                                               new PenitencePE02() };
        }

        protected override string Id => "PE_IT_AB";

        protected override string Spritesheet => "PenitenceAB.png";

        public override bool Complete(PenitenceCheckCurrent fsmStateAction)
        {
            CompletePenitences();

            Core.PenitenceManager.MarkCurrentPenitenceAsCompleted();

            List<BaseInventoryObject> rewards = new List<BaseInventoryObject>
            {
                Core.InventoryManager.GetBaseObject( "RB101", InventoryManager.ItemType.Bead ), // PE01 Reward
                Core.InventoryManager.GetBaseObject( "RB102", InventoryManager.ItemType.Bead )  // PE02 Reward
            };

            GiveRewards(fsmStateAction, rewards);

            return true;
        }
    }

    /// <summary>
    /// Penitence combining the effects and rewards of:    
    ///     PE02: Penitence of the Unwavering Faith => RB102: Reliquary of the Fervent Heart
    ///     PE03: Penitence of the True Guilt       => RB103:  Reliquary of the Sorrowful Heart
    /// </summary>
    public class PenitenceBC : ComboPenitence
    {
        public PenitenceBC()
        {
            Penitences = new List<IPenitence>{ new PenitencePE02(),
                                               new PenitencePE03() };
        }

        protected override string Id => "PE_IT_BC";

        protected override string Spritesheet => "PenitenceBC.png";

        public override bool Complete(PenitenceCheckCurrent fsmStateAction)
        {
            CompletePenitences();

            Core.PenitenceManager.MarkCurrentPenitenceAsCompleted();

            List<BaseInventoryObject> rewards = new List<BaseInventoryObject>
            {
                Core.InventoryManager.GetBaseObject( "RB102", InventoryManager.ItemType.Bead ), // PE02 Reward
                Core.InventoryManager.GetBaseObject( "RB103", InventoryManager.ItemType.Bead )  // PE03 Reward
            };

            GiveRewards(fsmStateAction, rewards);

            return true;
        }
    }

    /// <summary>
    /// Penitence combining the effects and rewards of:
    ///     PE03: Penitence of the True Guilt       => RB103:  Reliquary of the Sorrowful Heart
    ///     PE01: Penitence of a Bleeding Heart     => RB101: Reliquary of the Suffering Heart    
    /// </summary>
    public class PenitenceCA : ComboPenitence
    {
        public PenitenceCA()
        {
            Penitences = new List<IPenitence>{ new PenitencePE03(),
                                               new PenitencePE01() };
        }

        protected override string Id => "PE_IT_CA";

        protected override string Spritesheet => "PenitenceCA.png";

        public override bool Complete(PenitenceCheckCurrent fsmStateAction)
        {
            CompletePenitences();

            Core.PenitenceManager.MarkCurrentPenitenceAsCompleted();

            List<BaseInventoryObject> rewards = new List<BaseInventoryObject>
            {
                Core.InventoryManager.GetBaseObject( "RB103", InventoryManager.ItemType.Bead ), // PE03 Reward
                Core.InventoryManager.GetBaseObject( "RB101", InventoryManager.ItemType.Bead )  // PE01 Reward
            };

            GiveRewards(fsmStateAction, rewards);

            return true;
        }
    }

    /// <summary>
    /// Penitence combining the effects and rewards of all the basic penitences:
    ///     PE01: Penitence of a Bleeding Heart     => RB101: Reliquary of the Suffering Heart
    ///     PE02: Penitence of the Unwavering Faith => RB102: Reliquary of the Fervent Heart
    ///     PE03: Penitence of the True Guilt       => RB103:  Reliquary of the Sorrowful Heart
    /// </summary>
    public class PenitenceABC : ComboPenitence
    {
        public PenitenceABC()
        {
            Penitences = new List<IPenitence>{ new PenitencePE01(),
                                               new PenitencePE02(),
                                               new PenitencePE03() };
        }

        protected override string Id => "PE_IT_ABC";

        protected override string Spritesheet => "PenitenceABC.png";
        
        public override bool Complete(PenitenceCheckCurrent fsmStateAction)
        {
            CompletePenitences();

            Core.PenitenceManager.MarkCurrentPenitenceAsCompleted();
            
            List<BaseInventoryObject> rewards = new List<BaseInventoryObject>
            {
                Core.InventoryManager.GetBaseObject( "RB101", InventoryManager.ItemType.Bead ), // PE01 Reward
                Core.InventoryManager.GetBaseObject( "RB102", InventoryManager.ItemType.Bead ), // PE02 Reward
                Core.InventoryManager.GetBaseObject( "RB103", InventoryManager.ItemType.Bead )  // PE02 Reward
            };

            GiveRewards(fsmStateAction, rewards);

            return true;
        }
    }
}
