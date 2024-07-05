using System.Collections;
using Gameplay.GameControllers.Bosses.EcclesiaBros.Esdras.Audio;
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

        public GameObject BossfightCenter {get; set;}


        // -- Methods --

        private void MoveElementsToFinalPosition()
        {
            EsdrasAnimatorGO.transform.position = EsdrasBoss.transform.position;
            EsdrasNPC.transform.position = CalculateEsdrasNpcPosition();

            // TODO: Flip Animator to match boss facing

            // TODO: Move Perpetva? She should be a couple steps behind the Penitent
        }

        private Vector3 CalculateEsdrasNpcPosition()
        {
            Vector3 startPosition    = EsdrasAnimatorGO.transform.position;
            //Vector3 penitentPosition = Penitent.transform.position;

            Vector3 arenaLeftBorder = new Vector3(0f,0f,0f);
            Vector3 arenaRightBorder = new Vector3(0f,0f,0f);

            
//TODO
            // Final position of the EsdrasNPC, constraints:
            //  - Distance to Penitent must be >= MinDistanceToPenitent
            //  - Distance to either arena boundary muust be >= MinDistanceToBoundary
            //  - Esdras should only move through the Penitent if too close to a Boundary
            Vector3 npcTarget = new Vector3(0f,0f,0f);



            // TODO: PLACEHOLDER
            return EsdrasBoss.transform.position;
        }

        private void PenitentFaceTarget(Vector3 target)
        {
            // TODO
        }

        private void AnimatorFaceTarget(Vector3 target)
        {
            // TODO
        }

        private void NpcFaceTarget(Vector3 target)
        {
            // TODO: Sprite has a different default facing?
        }

// TODO: Add sounds to falling weapon, and to Esdras swooshing the weapon back to his shoulder

        public void ReplaceBossWithAnimator()
        {
            Main.IterTormenti.Log("EsdrasBehaviour::ReplaceBossWithAnimator");
            
            MoveElementsToFinalPosition();
            
            
            EsdrasBoss.transform.Find("#Constitution/Sprite").gameObject.GetComponent<SpriteRenderer>().enabled = false;
            // TODO: Disable boss sounds too?            

            EsdrasAnimator.Play();
        }

        public void SetAnimatorToStandUp()
        {
            Main.IterTormenti.Log("EsdrasBehaviour::SetAnimatorToStandUp");
            EsdrasAnimator.Animations["EsdrasPickUpWeapon"].AnimationCompleted += ReplaceAnimatorWithNPC;
            EsdrasAnimator.GoToAnimation("EsdrasPickUpWeapon");

            // EsdrasAudio audio = EsdrasBoss.GetComponentInChildren<EsdrasAudio>();
            // if(audio != null)
            // {
            //     audio.PlayCallSister_AUDIO();
            // }            
        }

        public void MoveAnimatorToTarget()
        {
            Main.IterTormenti.Log("EsdrasBehaviour::MoveAnimatorToTarget");
            // TODO: How to notify that movement is done?
            EsdrasAnimator.GoToAnimation("EsdrasLimp");
            // TODO: Tweening to EsdrasNPC position
            
            //TODO: Make Penitent face Esdras as he moves

            //TODO: Once done, flip facing to look at penitent
        }

        public void ReplaceAnimatorWithNPC(object item, AnimationEventArgs args)
        {
            Main.IterTormenti.Log("EsdrasBehaviour::ReplaceAnimatorWithNPC");
            EsdrasAnimatorGO.SetActive(false);
            EsdrasNPC.transform.Find("#Constitution/Body").gameObject.GetComponent<SpriteRenderer>().enabled = true;
            EsdrasNpcFSM.SetState("BlockPlayerInput");
        }


        
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
