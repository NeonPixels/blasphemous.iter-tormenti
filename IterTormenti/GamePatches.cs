using HarmonyLib;
using Gameplay.UI.Others.MenuLogic;
using Gameplay.UI.Widgets;
using System.Collections.Generic;
using Rewired;
using UnityEngine;
using UnityEngine.UI;
using I2.Loc;
using Blasphemous.ModdingAPI.Input;

// --- Ascend Empty Save Slot ---
//
// Rather than tinker with the new game callbacks, we just allow triggering the standard NG+ Ascend 
// confirmation dialog on empty save slots, which will handle setting the game into NG+ for us.
// We also update the description text to differentiate it from the regular ascension confirmation.
// To do so, we access the child of 'SelectSaveSlots.ConfirmationUpgradeRoot' containing the description.
//
// Here we have the structure of the object:
//
//    name: Upgrade
//    [0]: BorderUp
//    [1]: BorderDown
//    [2]: Header
//    [3]: DesciptionTextPlus
//    [4]: Confirmation
//    [4][0]: ConfirmationTextPlus
//    [4][1]: SelectionUpgrade
//
// We need to update the text of the 'DescriptionTextPlus' item found at index '3'.

namespace IterTormenti
{
    [HarmonyPatch(typeof(SaveSlot), "SetData")]
    public class SaveSlotsSetData_Patch
    {
        public static bool Prefix(string zoneName, ref bool canConvert)
        {
            if(zoneName == string.Empty) // Note: Comment this line out to force ALL slots to be upgradeable... ALWAYS (useful for testing)
            {
                // If the slot is empty, we force the 'canConvert' flag so the Ascend button prompt is shown
                canConvert = true;

                //Main.IterTormenti.Log("SaveSlot.SetData Prefix: Empty Slot updated to be ascendable");
            }
            
            return true;
        }
    }

    [HarmonyPatch]
    public class SelectSaveSlotsSetConfirming_ReversePatch
    {
        [HarmonyReversePatch]
        [HarmonyPatch(typeof(SelectSaveSlots), "SetConfirming")]
        public static void SetConfirming(object instance, bool IsUpgrade, int NumberOfNewGamePlusUpgrades)
        {
            // Reverse Patch to allow using a private method
            // This is a stub, should never be called
        }
    } 

    [HarmonyPatch(typeof(SelectSaveSlots), "SetConfirming")]
    public class SelectSaveSlotsSetConfirming_Patch
    {
        public static void Postfix( bool IsUpgrade,
                                    ref GameObject ___ConfirmationUpgradeRoot )
        {
            if( IsUpgrade )
            {
                // A regular upgrade action has been called, restore the dialog text
                Text descriptionText = ___ConfirmationUpgradeRoot.transform.GetChild(3).GetComponent<Text>();
                descriptionText.text = ScriptLocalization.Get("UI_Slot/LABEL_GAME+_DESCRIPTION");

                // Note: We could add a way to detect if the text needs to be updated, but it would be more
                // convoluted and/or costly than just updating the text.

                //Main.IterTormenti.Log("SelectSaveSlots.SetConfirming Postfix: Displaying standard Slot Ascension confirmation");
            }

            // Note: This patch will only affect calls to the original 'SetConfirming' method.
            // Our Reverse Patch above will NOT trigger this patch. That's why we assume that
            // calls to this patch can only happen with regular Upgrade actions, as we use
            // the ReversePatch call for the new Empty Save Upgrade.
        }
    }

    [HarmonyPatch(typeof(SelectSaveSlots), "Update")]
    public class SelectSaveSlotsUpdate_Patch
    {
        public static void Postfix( ref SelectSaveSlots __instance,
                                    List<SaveSlot> ___slots,
                                    SelectSaveSlots.SlotsModes ___CurrentSlotsMode,
                                    ref GameObject ___ConfirmationUpgradeRoot )
        {
            if (  !__instance.IsShowing
                || __instance.IsConfirming
                || __instance.SelectedSlot < 0
                || ReInput.players.playerCount <= 0
                || ((bool)__instance.corruptedSaveMessage && __instance.corruptedSaveMessage.IsShowing)
                || ___CurrentSlotsMode == SelectSaveSlots.SlotsModes.BossRush
                 )
            {
                // Skip this patch under the following conditions:
                // - If the SelectSaveSlots menu is not displaying
                // - If there is already a confirmation action in progress (upgrade or delete)
                // - If the slected slot is not valid
                // - If no players are active
                // - If a corrupted save message is displaying
                // - If we are in boss rush mode
                return;
            }

            if ( Main.IterTormenti.InputHandler.GetButtonDown(ButtonCode.UIOptions)
                 && ___slots[__instance.SelectedSlot].IsEmpty
                 && ___slots[__instance.SelectedSlot].CanConvertToNewGamePlus )
            {
                // If the player presses the UIOptions button when an Empty slot that can be upgraded is selected,
                // we manually call the 'SetConfirming' method so the normal process to Ascend a slot takes place...
                SelectSaveSlotsSetConfirming_ReversePatch.SetConfirming(__instance, true, 0);
               
                // ...but we also update the dialog text to the Empty Slot Ascend version
                Text descriptionText = ___ConfirmationUpgradeRoot.transform.GetChild(3).GetComponent<Text>();
                descriptionText.text = Main.IterTormenti.LocalizationHandler.Localize("UI_Slot/LABEL_NEW_GAME_DESCRIPTION");                

                //Main.IterTormenti.Log( "SelectSaveSlots.Update Postfix: Displaying Empty Slot Ascension confirmation" );
            }
        }
    }
}