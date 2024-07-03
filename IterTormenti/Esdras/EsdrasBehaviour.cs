

using System.Collections;
using IterTormenti.Utils.Sprites;
using IterTormenti.Utils.Sprites.Animations;
using UnityEngine;

namespace IterTormenti.Esdras
{
    class EsdrasBehaviour : MonoBehaviour
    {
        private EsdrasBehaviour()
        {
            Name = "EsdrasBehaviour";            
        }

        public EsdrasBehaviour(string name = "EsdrasBehaviour")
        {
            Name = name;            
        }

        public EsdrasBehaviour(ref EsdrasBehaviour source)
        {
            Clone(ref source);
        }
        
        /// <summary>
        /// Creates a deep copy of the EsdrasBehaviour.
        /// In an attempt to keep names unique, the name of the copy will be 
        /// modified.
        /// </summary>
        /// <param name="source">EsdrasBehaviour to clone</param>
        public void Clone(ref EsdrasBehaviour source)
        {
            this.Name = source.Name + "_copy";
        }

        // -- Properties and Attribtues --

        /// <summary>
        /// EsdrasBehaviour name
        /// </summary>
        public string Name {get; set;}

        public GameObject EsdrasBoss {get; set;}

        public GameObject EsdrasNPC {get; set;}

        public PlayMakerFSM EsdrasNpcFSM {get; set;}

        public GameObject EsdrasAnimatorGO {get; set;}

        public SpriteAnimator EsdrasAnimator
        {
            get
            {
                if(null == EsdrasAnimatorGO) return null;
                return EsdrasAnimatorGO.GetComponent<SpriteAnimator>();
            }
        }

        public GameObject Penitent {get; set;}

        public GameObject PerpetvaVFX {get; set;}


        // -- Methods --

        public void MoveToFinalPosition()
        {
            Vector3 bossPosition = EsdrasBoss.transform.position;
            
            EsdrasAnimatorGO.transform.position = bossPosition;
            EsdrasNPC.transform.position = bossPosition;

            // TODO: Move Perpetva? She should be a couple steps behind the Penitent
        }

        public void ReplaceBossWithAnimator()
        {
            Main.IterTormenti.Log("EsdrasBehaviour::ReplaceBossWithAnimator");
            EsdrasBoss.transform.Find("#Constitution/Sprite").gameObject.GetComponent<SpriteRenderer>().enabled = false;
            EsdrasAnimator.Play();
        }

        public void SetAnimatorToStandUp()
        {
            Main.IterTormenti.Log("EsdrasBehaviour::SetAnimatorToStandUp");
            EsdrasAnimator.Animations["EsdrasPickUpWeapon"].AnimationCompleted += ReplaceAnimatorWithNPC;
            EsdrasAnimator.GoToAnimation("EsdrasPickUpWeapon");
        }

        public void ReplaceAnimatorWithNPC(object item, AnimationEventArgs args)
        {
            Main.IterTormenti.Log("EsdrasBehaviour::ReplaceAnimatorWithNPC");
            EsdrasAnimatorGO.SetActive(false);
            EsdrasNPC.transform.Find("#Constitution/Body").gameObject.GetComponent<SpriteRenderer>().enabled = true;
            EsdrasNpcFSM.SetState("BlockPlayerInput");
        }

        // TODO: Move objects to final Boss and Penitent position
        // TODO: Make Esdras and Penintent face each other
        // TODO: Replace Boss with Non-lethal defeat animator
        // TODO: Activate Esdras standup animation
        // TODO: Replace Non-lethal defeat animator with NPC
        
        // --- MonoBehaviour methods --


        /// <summary>
        /// Awake is called when the script instance is being loaded.
        /// https://docs.unity3d.com/2017.4/Documentation/ScriptReference/MonoBehaviour.Awake.html
        /// </summary>
        void Awake()
        {
            
        }

        /// <summary>
        /// Start is called on the frame when a script is enabled just before any of the
        /// Update methods is called the first time.
        /// https://docs.unity3d.com/2017.4/Documentation/ScriptReference/MonoBehaviour.Start.html
        /// </summary>
        /// <returns></returns>
        IEnumerator Start()
        {
            yield return new WaitForSeconds(0.0f);            
        }

        /// <summary>
        /// Update is called every frame, if the MonoBehaviour is enabled.
        /// https://docs.unity3d.com/2017.4/Documentation/ScriptReference/MonoBehaviour.Update.html
        /// </summary>
        public void Update()
        {
            
        }

        // -- Internal attributes --

    }
}
