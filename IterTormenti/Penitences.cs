using Blasphemous.ModdingAPI;
using Blasphemous.Framework.Penitence;
using Framework.Managers;
using Framework.Penitences;
using Framework.Inventory;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Gameplay.UI;


namespace IterTormenti
{
    /// <summary>
    /// Custom penitences that combine the effects of several other penitences
    /// </summary>
    public abstract class ComboPenitence : ModPenitence
    {
        protected override string Name => Main.IterTormenti.LocalizationHandler.Localize(Id + ".name");

        protected override string Description => Main.IterTormenti.LocalizationHandler.Localize(Id + ".desc");
        // TODO: Long descriptions overflow the "Abandon Penitence" text box.

        public List<IPenitence> Penitences = new();
        public List<RewardItem> Rewards = new();
        public List<string> Skins = new();

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
        /// Structure with the definition of reward items.
        /// Needed because we don't have access to the items themselves
        /// at initialization.
        /// </summary>
        public struct RewardItem
        {
            public RewardItem(string itemId, InventoryManager.ItemType itemType)
            {
                Id       = itemId;
                ItemType = itemType;
            }

            public string Id {get;}
            public InventoryManager.ItemType ItemType {get;}
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
                    ModLog.Error($"ComboPenitence::{Id}::CompletePenitences: Penitence with id: " + penitence.Id + " could not be found!");
                    continue;
                }

                penitenceToComplete.Completed = true;
            }
        }

        public override IEnumerator Complete()
        {            
            yield return base.Complete();

            CompletePenitences();

            foreach(RewardItem rewardDef in Rewards)
            {
                BaseInventoryObject reward = Core.InventoryManager.GetBaseObject( rewardDef.Id, rewardDef.ItemType );

                if(null == reward)
                {
                    ModLog.Error($"ComboPenitence::{Id}::GiveRewards: Invalid reward object found!");
                    continue;
                }

                // Attempt to add items to inventory. Will fail if item is already owned.
                if( !Core.InventoryManager.AddBaseObject(reward) )
                {
                    continue;
                }

                // Display item popup
                UIController.instance.ShowObjectPopUp( UIController.PopupItemAction.GetObejct,
                                                       reward.caption,
                                                       reward.picture,
                                                       reward.GetItemType(),
                                                       3f,
                                                       true );

                yield return new WaitForSecondsRealtime(0.05f);
				yield return new WaitUntil(() => !UIController.instance.IsShowingPopUp());
                yield return new WaitForSecondsRealtime(1.0f);
            }

            foreach(string skin in Skins)
            {
                if(!Core.ColorPaletteManager.GetAllColorPalettesId().Contains(skin))
                {
                    ModLog.Error($"ComboPenitence::{Id}::GiveRewards: Invalid skin object found!");
                    continue;
                }

                Core.ColorPaletteManager.UnlockColorPalette(skin);

                yield return new WaitForSecondsRealtime(0.05f);
				yield return new WaitUntil(() => !UIController.instance.IsUnlockActive());
            }
        }

        protected abstract string Spritesheet { get; }

        protected static Rect[] penitenceSpriteAreas = { new Rect(  0,  0, 94, 110),
                                                         new Rect( 95,  1, 92, 108),
                                                         new Rect(190, 94, 16, 16),
                                                         new Rect(190, 78, 16, 16),
                                                         new Rect(190, 62, 16, 16),
                                                         new Rect(188,  0, 18, 18) };

        protected PenitenceImageCollection LoadImages()
        {
            PenitenceImageCollection imageCollection = new();

            if( Main.IterTormenti.FileHandler.LoadDataAsVariableSpritesheet(Spritesheet, penitenceSpriteAreas, out Sprite[] images) )
            {
                imageCollection.ChooseSelected   = images[0];
                imageCollection.ChooseUnselected = images[1];
                imageCollection.InProgress       = images[2];
                imageCollection.Completed        = images[3];
                imageCollection.Abandoned        = images[4];
                imageCollection.Gameplay         = images[5];
            }
            else
            {
                imageCollection.ChooseSelected   =
                imageCollection.ChooseUnselected =
                imageCollection.InProgress       =
                imageCollection.Completed        =
                imageCollection.Abandoned        =
                imageCollection.Gameplay         = null;

                ModLog.Error($"ComboPenitence::{Id}::LoadImages: ERROR: Failed to load sprites!");
            }

            return imageCollection;
        }

        protected override PenitenceImageCollection Images => LoadImages();
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
            Penitences = new(){
                new PenitencePE01(),
                new PenitencePE02()
            };

            Rewards = new(){
                new RewardItem( "RB101", InventoryManager.ItemType.Bead ), // PE01 Reward
                new RewardItem( "RB102", InventoryManager.ItemType.Bead )  // PE02 Reward
            };

            Skins = new(){
                "PENITENT_PE01",
                "PENITENT_PE02"
            };
        }

        protected override string Id => "PE_IT_AB";

        protected override string Spritesheet => "PenitenceAB.png";
    }

    /// <summary>
    /// Penitence combining the effects and rewards of:    
    ///     PE02: Penitence of the Unwavering Faith => RB102: Reliquary of the Fervent Heart
    ///     PE03: Penitence of the True Guilt       => RB103: Reliquary of the Sorrowful Heart
    /// </summary>
    public class PenitenceBC : ComboPenitence
    {
        public PenitenceBC()
        {
            Penitences = new List<IPenitence>{
                new PenitencePE02(),
                new PenitencePE03()
            };

            Rewards = new(){
                new RewardItem( "RB102", InventoryManager.ItemType.Bead ), // PE02 Reward
                new RewardItem( "RB103", InventoryManager.ItemType.Bead )  // PE03 Reward
            };

            Skins = new(){
                "PENITENT_PE02",
                "PENITENT_PE03"
            };
        }

        protected override string Id => "PE_IT_BC";

        protected override string Spritesheet => "PenitenceBC.png";
    }

    /// <summary>
    /// Penitence combining the effects and rewards of:
    ///     PE03: Penitence of the True Guilt       => RB103: Reliquary of the Sorrowful Heart
    ///     PE01: Penitence of a Bleeding Heart     => RB101: Reliquary of the Suffering Heart    
    /// </summary>
    public class PenitenceCA : ComboPenitence
    {
        public PenitenceCA()
        {
            Penitences = new List<IPenitence>{
                new PenitencePE03(),
                new PenitencePE01()
            };

            Rewards = new(){
                new RewardItem( "RB103", InventoryManager.ItemType.Bead ), // PE03 Reward
                new RewardItem( "RB101", InventoryManager.ItemType.Bead )  // PE01 Reward
            };

            Skins = new(){
                "PENITENT_PE03",
                "PENITENT_PE01"
            };
        }

        protected override string Id => "PE_IT_CA";

        protected override string Spritesheet => "PenitenceCA.png";
    }

    /// <summary>
    /// Penitence combining the effects and rewards of all the basic penitences:
    ///     PE01: Penitence of a Bleeding Heart     => RB101: Reliquary of the Suffering Heart
    ///     PE02: Penitence of the Unwavering Faith => RB102: Reliquary of the Fervent Heart
    ///     PE03: Penitence of the True Guilt       => RB103: Reliquary of the Sorrowful Heart
    /// </summary>
    public class PenitenceABC : ComboPenitence
    {
        public PenitenceABC()
        {
            Penitences = new List<IPenitence>{ new PenitencePE01(),
                                               new PenitencePE02(),
                                               new PenitencePE03() };

            Rewards = new(){
                new RewardItem( "RB101", InventoryManager.ItemType.Bead ), // PE01 Reward
                new RewardItem( "RB102", InventoryManager.ItemType.Bead ), // PE02 Reward
                new RewardItem( "RB103", InventoryManager.ItemType.Bead )  // PE03 Reward
            };

            Skins = new(){
                "PENITENT_PE01",
                "PENITENT_PE02",
                "PENITENT_PE03"
            };
        }

        protected override string Id => "PE_IT_ABC";

        protected override string Spritesheet => "PenitenceABC.png";
    }
}
