using HarmonyLib;

using Framework.Managers;
using Framework.Penitences;
using Gameplay.UI.Widgets;
using Gameplay.UI.Others.MenuLogic;
using Gameplay.UI.Others.UIGameLogic;
using Tools.Items;

using System.Collections.Generic;
using System.Linq;

using UnityEngine;
using UnityEngine.UI;


namespace IterTormenti
{

    // Prevent displaying completion medal for combo penitences
    [HarmonyPatch(typeof(PenitenceSlot), "CreateElement")]
    class PenitenceSlotCreateElement_Patch
    {
        public static bool Prefix(string name)
        {             
            // Check if it is one of our combo penitences
            if(null != Main.IterTormenti.ComboPenitenceList.Find((ComboPenitence x) => x.id == name))
            {
                IPenitence penitence = Core.PenitenceManager.GetAllPenitences().Find((IPenitence x) => x.Id == name);

                // If the penitence is completed, we want to
                // skip creation of the medal icon
                if(penitence.Completed)
                {
                    // Skip original method
                    return false;
                }
            }

            // Go back to original method
            return true;
        }
    }


    // --- Enable PE02 Health ---
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
        public static bool Prefix( IPenitence current, List<IPenitence> completed,
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

            // TODO: The UseFervourFlasks feature should be managed in a more flexible manner

            bool containsPE03 = null != penitence.Penitences.Find((IPenitence x) => x is PenitencePE03 );

            Core.PenitenceManager.UseFervourFlasks = containsPE03;
        }
    }

}