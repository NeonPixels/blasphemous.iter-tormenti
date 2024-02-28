using HarmonyLib;
using Framework.Managers;
using Framework.Penitences;
using Gameplay.UI.Widgets;
using Gameplay.UI.Others.MenuLogic;
using Gameplay.UI.Others.UIGameLogic;
using Tools.Items;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;


namespace IterTormenti
{
    // Disable penitence completion medals for combo penitences
    [HarmonyPatch(typeof(PenitenceSlot), "UpdateFromSavegameData")]
    class PenitenceSlotUpdateFromSavegameData_Patch
    {
        public static void Postfix( PenitenceManager.PenitencePersistenceData data,
                                    ref GameObject ___childElement )
        {
            // Retrieve all combo penitence medals
            List<GameObject> penitenceMedalsList = new List<GameObject>();
            foreach (Transform item in ___childElement.transform.parent)
            {
                // Is the item a Penitence medal?
                if( item.gameObject.name.StartsWith("Penitence_") )
                {
                    // Is it a Combo Penitence medal?
                    if(null != Main.IterTormenti.ComboPenitenceList.Find((ComboPenitence x) => item.gameObject.name == "Penitence_" + x.id ))
                    {
                        penitenceMedalsList.Add(item.gameObject);
                    }
                }
            }

            // Now let's check the persistence data and see what the penitence status is
            foreach (GameObject medal in penitenceMedalsList)
            {
                IPenitence current = data.allPenitences.Find( (IPenitence x) => medal.name == "Penitence_" + x.Id );
                
                if(null == current)
                {
                    continue;
                }

                // If the penitence is completed, disable the game object
                if(current.Completed)
                {
                    medal.SetActive(value: false);
                }
            }
        }
    }


    // --- Enable PE02 Health Fix ---
    //
    // Sadly, the behaviour enabling the PE02 health management is 
    // part of the GameplayWidget code, which means it can't be normally
    // triggered by custom penitences.
    //
    // We need to override this behaviour so a penitence activating the 
    // "health stocks" mode results in a UI update.

    [HarmonyPatch(typeof(GameplayWidget), "OnPenitenceChanged")]
    public class GameplayWidgetOnPenitenceChanged_Patch
    {
        public static bool Prefix(  IPenitence current,
                                    List<IPenitence> completed,
                                    ref List<GameObject> ___normalHealthGameObjects,
                                    ref List<GameObject> ___pe02HealthGameObjects,
                                    ref PlayerHealth ___normalPlayerHealth,
                                    ref PlayerHealthPE02 ___pe02PlayerHealth,
                                    ref Image ___CurrentPenitence,
                                    ref List<SelectSaveSlots.PenitenceData> ___PenitencesConfig )
        {
            // Use the 'UseStocksOfHealth' flag to determine if health orbs are active
            bool useHealthOrbs = Core.PenitenceManager.UseStocksOfHealth;

            ___normalHealthGameObjects.ForEach(delegate(GameObject x)
            {
                x.SetActive(!useHealthOrbs);
            });

            ___pe02HealthGameObjects.ForEach(delegate(GameObject x)
            {
                x.SetActive(useHealthOrbs);
            });

            ___normalPlayerHealth.enabled = !useHealthOrbs;
            ___pe02PlayerHealth.enabled = useHealthOrbs;
            
            if (useHealthOrbs)
            {
                ___pe02PlayerHealth.ForceUpdate();
            }
            
            Sprite sprite = null;
            if (current != null)
            {
                foreach (SelectSaveSlots.PenitenceData current2 in ___PenitencesConfig)
                {
                    if (current2.id.ToUpper() == current.Id.ToUpper())
                    {
                        sprite = current2.InProgress;
                    }
                }
            }

            ___CurrentPenitence.enabled = null != sprite;
            ___CurrentPenitence.sprite  = sprite;

            return false;
        }
    }

    // --- Fervour Flask Fix ---
    //
    // When removing the RB103 bead, it will set the 'UseFervourFlasks'
    // to false unless the active penitence is of the type 'PenitencePE03'.
    // This means that other penitences using the flag will have the
    // bead disabling the feature when unequipped.
    // This fix checks the combo penitences for PE03.

    [HarmonyPatch(typeof(GuiltPenitenceBeadEffect), "OnRemoveEffect")]
    public class GuiltPenitenceBeadEffectOnRemoveEffect_Patch
    {
        public static void Postfix()
        {
            ComboPenitence penitence = Main.IterTormenti.ComboPenitenceList.Find((ComboPenitence x) => x.id == Core.PenitenceManager.GetCurrentPenitence().Id);

            if(null == penitence)
            {
                return;
            }

            // TODO: The UseFervourFlasks feature could be managed in a more flexible manner

            bool containsPE03 = null != penitence.Penitences.Find((IPenitence x) => x is PenitencePE03 );

            Core.PenitenceManager.UseFervourFlasks = containsPE03;
        }
    }
}
