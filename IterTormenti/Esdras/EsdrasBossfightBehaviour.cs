using System;
using System.Collections;
using Framework.FrameworkCore;
using Framework.Managers;
using Gameplay.GameControllers.Penitent;
using IterTormenti.Utils.Sprites;
using IterTormenti.Utils.Sprites.Animations;
using UnityEngine;

namespace IterTormenti.Esdras
{
    class BossfightBehaviour : MonoBehaviour
    {
        private BossfightBehaviour()
        {
            Name = "BossfightBehaviour";
        }

        public BossfightBehaviour( string name = "BossfightBehaviour" )
        {
            Name = name;            
        }

        public BossfightBehaviour(ref BossfightBehaviour source)
        {
            Clone(ref source);
        }
        
        /// <summary>
        /// Creates a deep copy of the BossfightBehaviour.
        /// In an attempt to keep names unique, the name of the copy will be 
        /// modified.
        /// </summary>
        /// <param name="source">BossfightBehaviour to clone</param>
        public void Clone(ref BossfightBehaviour source)
        {
            this.Name = source.Name + "_copy";
            this.BossFightStuff   = source.BossFightStuff;
            this.EsdrasNPC        = source.EsdrasNPC;
            this.EsdrasAnimatorGO = source.EsdrasAnimatorGO;
            //this.Penitent         = source.Penitent;
            this.PerpetvaVFX      = source.PerpetvaVFX;
        }

        // -- Properties and Attribtues --

        /// <summary>
        /// EsdrasBehaviour name
        /// </summary>
        public string Name {get; set;}


        public GameObject BossFightStuff {get; set;}

        public GameObject EsdrasBoss
        {
            get
            {
                if(null == BossFightStuff) return null;
                return BossFightStuff.transform.Find("Esdras").gameObject;
            }
        }

        public GameObject EsdrasNPC {get; set;}

        public PlayMakerFSM EsdrasNpcFSM
        {
            get
            {
                if(null == EsdrasNPC) return null;
                return PlayMakerFSM.FindFsmOnGameObject(EsdrasNPC, "FSM");
            }
        }

        public GameObject EsdrasAnimatorGO {get; set;}

        public SpriteAnimator EsdrasAnimator
        {
            get
            {
                if(null == EsdrasAnimatorGO) return null;
                return EsdrasAnimatorGO.GetComponent<SpriteAnimator>();
            }
        }

        //public GameObject Penitent {get; set;}

        public GameObject PerpetvaVFX {get; set;}

        

        // -- Methods --

        private void MoveElementsToFinalPosition()
        {
            EsdrasAnimatorGO.transform.position = EsdrasBoss.transform.position;
            
            EsdrasNPC.transform.position = CalculateEsdrasNpcPosition();

            PerpetvaVFX.transform.position = CalculatePerpetvaPosition();

            // TODO: Flip Animator to match boss facing

            // TODO: Flip NPC sprites depending on positions

            // TODO: Make an alternate Esdras movement state, so it is shorter when on the left side
        }

        private Vector3 CalculateEsdrasNpcPosition()
        {
            Vector3 penitentPosition = Core.Logic.Penitent.gameObject.transform.position;

            // Matches original NPC position
            Vector3 targetRight = EsdrasNPC.transform.position;

            // Center of the arena, equalize non-x positions so they don't affect distance calculations
            Vector3 bossfightCenter = new Vector3( BossFightStuff.transform.Find("BossfightCenter").gameObject.transform.position.x,
                                                   targetRight.y,
                                                   targetRight.z );

            // Left target is at the same distance from the arena center
            Vector3 targetLeft = new Vector3( bossfightCenter.x - Vector3.Distance(bossfightCenter, targetRight),
                                              targetRight.y,
                                              targetRight.z );  

            Main.IterTormenti.Log($"PenitentXpos: {penitentPosition.x} ");
            Main.IterTormenti.Log($"LeftXpos: {targetLeft.x} CenterXpos: {bossfightCenter.x} RightXpos: {targetRight.x}");

            Main.IterTormenti.Log($"Distance to Right Target: {Vector3.Distance(penitentPosition, targetRight)}");
            Main.IterTormenti.Log($"Distance to Left Target:  {Vector3.Distance(penitentPosition, targetLeft)}");
            
            // Select the target furthest away from the Penitent
            if(Vector3.Distance(penitentPosition, targetRight) >= Vector3.Distance(penitentPosition, targetLeft))
            {
                return targetRight;
            }
            else
            {
                return targetLeft;
            }
            //TODO: Preserve facing

        }

        private Vector3 CalculatePerpetvaPosition()
        {
            Vector3 penitentPosition = Core.Logic.Penitent.gameObject.transform.position;
            
            // The left side position is the original position
            Vector3 positionLeft = PerpetvaVFX.transform.position;

            // Center of the arena, equalize non-x positions so they don't affect distance calculations
            Vector3 bossfightCenter = new Vector3( BossFightStuff.transform.Find("BossfightCenter").gameObject.transform.position.x,
                                                   positionLeft.y,
                                                   positionLeft.z );

            // Right side position is at the same distance from the arena center
            Vector3 positionRight = new Vector3( bossfightCenter.x + Vector3.Distance(bossfightCenter, positionLeft),
                                                 positionLeft.y,
                                                 positionLeft.z );  

            // Check if Penitent is to the left or to the right of the arena center, and move Perpetva accordingly
            if(penitentPosition.x < bossfightCenter.x)
            {
                return positionLeft;
            }
            else
            {
                return positionRight;
            }
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
            
            Vector3 penitentPosition =  Core.Logic.Penitent.gameObject.transform.position;

            // EsdrasAnimator sprite is facing right by default
            if(EsdrasAnimatorGO.transform.position.x > penitentPosition.x)
            {
                EsdrasAnimatorGO.transform.GetComponent<SpriteRenderer>().flipX = true;
            }
            else
            {
                EsdrasAnimatorGO.transform.GetComponent<SpriteRenderer>().flipX = false;
            }
            
            EsdrasBoss.transform.Find("#Constitution/Sprite").gameObject.GetComponent<SpriteRenderer>().enabled = false;
            // TODO: Disable boss sounds too?            

            EsdrasAnimator.Play();
        }

        public void SetAnimatorToStandUp()
        {
            Main.IterTormenti.Log("EsdrasBehaviour::SetAnimatorToStandUp");
            EsdrasAnimator.Animations["EsdrasPickUpWeapon"].AnimationCompleted += StopMovingAndFacePenitent;//ReplaceAnimatorWithNPC;//MoveAnimatorToTarget;
            EsdrasAnimator.GoToAnimation("EsdrasPickUpWeapon");
        }


        // ---- Movement -----

        public event EventHandler<EventArgs> MovementComplete;

        protected virtual void OnMovementComplete()
        {
            MovementComplete?.Invoke(this, new EventArgs());
        }

        public void MoveAnimatorToTarget()
        {
            Main.IterTormenti.Log("EsdrasBehaviour::MoveAnimatorToTarget");
            
            if(EsdrasNPC.transform.position.x > EsdrasAnimatorGO.transform.position.x)
            {
                EsdrasAnimatorGO.transform.GetComponent<SpriteRenderer>().flipX = false;
            }
            else
            {
                EsdrasAnimatorGO.transform.GetComponent<SpriteRenderer>().flipX = true;
            }

            // Make Penitent face Esdras as he moves
            lookAtEsdrasCoroutine = PenitentLookAtEsdrasCoroutine();
            StartCoroutine(lookAtEsdrasCoroutine);

            EsdrasAnimator.GoToAnimation("EsdrasDefeated");//"EsdrasLimp"); //TODO: Make animation
            
            // TODO: Tweening to EsdrasNPC position            
            moveToTargetCoroutine = MoveToTargetCoroutine(1.0f);
            StartCoroutine(moveToTargetCoroutine);
            

            //TODO: Once done, flip facing to look at penitent

            // MovementComplete += StopMovingAndFacePenitent;

            
        }

        private IEnumerator moveToTargetCoroutine;

        private const float movementSpeed = 0.1f;
        private const float minDistance = 0.01f; //TODO: Refine

        private IEnumerator MoveToTargetCoroutine(float speed)
        {
            float distance = Vector3.Distance(EsdrasNPC.transform.position, EsdrasAnimatorGO.transform.position);

            //float speedMs

            //1.0f / Time.deltaTime

            // TODO: Move NPC
            // TODO: FPS based movement speed?
            // TODO: If target reached, notify
            
            if(Vector3.Distance(EsdrasNPC.transform.position, EsdrasAnimatorGO.transform.position) <= minDistance)
            {
                OnMovementComplete(); //TODO: Better destination reached conditions
            }

            yield return new WaitForSeconds(0.1f);
        }

        private IEnumerator lookAtEsdrasCoroutine;

        private IEnumerator PenitentLookAtEsdrasCoroutine()
        {
            PenitentLookAtEsdras();

            yield return new WaitForSeconds(1.0f);
        }

        private void  PenitentLookAtEsdras()
        {
            Main.IterTormenti.Log("EsdrasBehaviour::PenitentLookAtEsdras");

            Penitent penitent = Core.Logic.Penitent.gameObject.GetComponent<Penitent>();
            Vector3 penitentPosition =  Core.Logic.Penitent.gameObject.transform.position;

            if(EsdrasNPC.transform.position.x < penitentPosition.x)
            {
                penitent.SetOrientation(EntityOrientation.Left);
            }

            if(EsdrasNPC.transform.position.x > penitentPosition.x)
            {
                penitent.SetOrientation(EntityOrientation.Right);
            }
        }



        //private void StopMovingAndFacePenitent(object item, EventArgs args)
        private void StopMovingAndFacePenitent(object item, AnimationEventArgs args)
        {
            Main.IterTormenti.Log("EsdrasBehaviour::StopMovingAndFacePenitent");

            //StopCoroutine(moveToTargetCoroutine);
            //StopCoroutine(lookAtEsdrasCoroutine);
            PenitentLookAtEsdras(); // Just in case the movement was too short

            Vector3 penitentPosition =  Core.Logic.Penitent.gameObject.transform.position;

            // EsdrasAnimator sprite is facing right by default
            // EsdrasNPC is facing left by default
            if(EsdrasNPC.transform.position.x > penitentPosition.x)
            {
                EsdrasNPC.transform.Find("#Constitution/Body").gameObject.GetComponent<SpriteRenderer>().flipX = false;
            }
            else
            {
                EsdrasNPC.transform.Find("#Constitution/Body").gameObject.GetComponent<SpriteRenderer>().flipX = true;
            }

            //TODO: Add a new action that forces the flipX variable to its default state (false) when Esdras is about to run away

            ReplaceAnimatorWithNPC();
        }





        //public void ReplaceAnimatorWithNPC(object item, AnimationEventArgs args)
        public void ReplaceAnimatorWithNPC()
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
        { }

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
        { }

        // -- Internal attributes --

    }
}
