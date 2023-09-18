using BepInEx;
using UnityEngine;

namespace IterTormenti
{
    [BepInPlugin(PluginInfo.PLUGIN_GUID, PluginInfo.PLUGIN_NAME, PluginInfo.PLUGIN_VERSION)]
    [BepInDependency("com.damocles.blasphemous.modding-api", "1.5.0")]
    public class Main : BaseUnityPlugin
    {
        public static IterTormenti IterTormenti { get; private set; }

        private void Start()
        {
            IterTormenti = new IterTormenti();
        }
    }

    public static class Utils
    {
        // Utility function that prints the names of the GameObjects contained
        // inside a GameObject, with their indices.
        // Useful when needing to manipulate specific elements.
        // TODO: Make it into a proper JSON parser
        public static string LogGameObjectStructure(GameObject parent)
        {
            string result = "";
            
            result += parent.name + " structure:\n";

            for(int idxA = 0; idxA < parent.transform.childCount; idxA++)
            {
                GameObject childA = parent.transform.GetChild(idxA).gameObject;

                result += "  Child[" + idxA +"]: " + childA.name + "\n";
            
                for(int idxB = 0; idxB < childA.transform.childCount; idxB++)
                {
                    GameObject childB = childA.transform.GetChild(idxB).gameObject;

                    result += "     SubChild[" + idxB +"]: " + childB.name + "\n";
                }
            }

            return result;
        }        
    }
}
