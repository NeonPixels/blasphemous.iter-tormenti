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
    class AnimatorBehaviour : MonoBehaviour
    {
        private AnimatorBehaviour()
        {
            Name = "AnimatorBehaviour";
        }

        public AnimatorBehaviour( string name = "AnimatorBehaviour" )
        {
            Name = name;
        }

        public AnimatorBehaviour(ref AnimatorBehaviour source)
        {
            Clone(ref source);
        }
        
        /// <summary>
        /// Creates a deep copy of the AnimatorBehaviour.
        /// In an attempt to keep names unique, the name of the copy will be 
        /// modified.
        /// </summary>
        /// <param name="source">AnimatorBehaviour to clone</param>
        public void Clone(ref AnimatorBehaviour source)
        {
            this.Name = source.Name + "_copy";
            this.BossFightStuff   = source.BossFightStuff;
            this.EsdrasNPC        = source.EsdrasNPC;
            this.EsdrasAnimatorGO = source.EsdrasAnimatorGO;            
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

        public GameObject PerpetvaVFX {get; set;}


        public GameObject EsdrasTarget {get; set;}


        // -- Methods --

        private void MoveElementsToFinalPosition()
        {
            EsdrasAnimatorGO.transform.position = EsdrasBoss.transform.position;
            
            Vector3 esdrasNpcFinalPosition = CalculateEsdrasNpcPosition();
            EsdrasTarget.transform.position = CalculateEsdrasTargetPosition(esdrasNpcFinalPosition);
            EsdrasNPC.transform.position = esdrasNpcFinalPosition;

            PerpetvaVFX.transform.position = CalculatePerpetvaPosition();           
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

            // Main.IterTormenti.Log($"PenitentXpos: {penitentPosition.x} ");
            // Main.IterTormenti.Log($"LeftXpos: {targetLeft.x} CenterXpos: {bossfightCenter.x} RightXpos: {targetRight.x}");

            // Main.IterTormenti.Log($"Distance to Right Target: {Vector3.Distance(penitentPosition, targetRight)}");
            // Main.IterTormenti.Log($"Distance to Left Target:  {Vector3.Distance(penitentPosition, targetLeft)}");
            
            // Select the target depending on which side of the arena the penitent is at
            if(penitentPosition.x <=  bossfightCenter.x)//if(Vector3.Distance(penitentPosition, targetRight) >= Vector3.Distance(penitentPosition, targetLeft))
            {
                return targetRight;
            }
            else
            {
                return targetLeft;
            }
        }

        // We need to move the offscreen target that Esdras moves out to after his dialog,
        // otherwise, if he is closer to the target, the animation tweening will make him
        // move slowly
        private Vector3 CalculateEsdrasTargetPosition(Vector3 esdrasNpcFinalPosition)
        {
            Vector3 targetPosition = EsdrasTarget.transform.position;

            if(targetPosition == EsdrasNPC.transform.position)
            {
                // Original position, do nothing
                return targetPosition;
            }

            // Move target the same distance between the new position and the old position
            Vector3 newTarget = new Vector3( targetPosition.x - Vector3.Distance( esdrasNpcFinalPosition,  EsdrasNPC.transform.position),
                                             targetPosition.y,
                                             targetPosition.z );

            return newTarget;
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
            if(penitentPosition.x <= bossfightCenter.x)
            {
                return positionLeft;
            }
            else
            {
                return positionRight;
            }
        }

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
            EsdrasBoss.transform.Find("#Constitution/Sprite/BlobShadow").gameObject.GetComponent<SpriteRenderer>().enabled = false;

            // Make Penitent seath weapon like a boss when Esdras is down
            EsdrasAnimator.Animations["EsdrasNonLethalDefeat"].AnimationCompleted += PenitentSeathWeapon;

            EsdrasAnimator.Play();

            EsdrasNpcFSM.SetState("BlockPlayerInput");
        }

        private readonly int _sheathedAnim = Animator.StringToHash("IdleToSheathed");
        public void PenitentSeathWeapon(object item, AnimationEventArgs args)
        {
            Core.Logic.Penitent.Animator.Play(_sheathedAnim);
		    Core.Logic.Penitent.Animator.SetBool("IS_DIALOGUE_MODE", true);
        }

        public void SetAnimatorToStandUp()
        {
            Main.IterTormenti.Log("EsdrasBehaviour::SetAnimatorToStandUp");
            EsdrasAnimator.Animations["EsdrasPickUpWeapon"].AnimationCompleted += MoveAnimatorToTarget;
            EsdrasAnimator.GoToAnimation("EsdrasPickUpWeapon");
        }


        // ---- Movement -----

        public event EventHandler<EventArgs> MovementComplete;

        protected virtual void OnMovementComplete()
        {
            Main.IterTormenti.Log("EsdrasBehaviour::OnMovementComplete");
            MovementComplete?.Invoke(this, new EventArgs());
        }

        public void MoveAnimatorToTarget(object item, AnimationEventArgs args)
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

            EsdrasAnimator.GoToAnimation("EsdrasRun");//"EsdrasLimp"); //TODO: Make animation
            
            moveToTargetCoroutine = MoveToTargetCoroutine(4.0f);
            StartCoroutine(moveToTargetCoroutine);
            
            MovementComplete += StopMovingAndFacePenitent;
        }

        private IEnumerator moveToTargetCoroutine;

        private IEnumerator MoveToTargetCoroutine(float speed)
        {
            Main.IterTormenti.Log("EsdrasBehaviour::MoveToTargetCoroutine");
            float distance = Vector3.Distance(EsdrasNPC.transform.position, EsdrasAnimatorGO.transform.position);

            //float speedMs

            //1.0f / Time.deltaTime
            
            // if(Vector3.Distance(EsdrasNPC.transform.position, EsdrasAnimatorGO.transform.position) <= minDistance)
            // {
            //     OnMovementComplete(); //TODO: Better destination reached conditions
            // }

            // public static IEnumerator Tweeng( this float duration,
            //                               System.Action<float> var,
            //                               float start,
            //                               float end )
        
            float duration = speed <= 0.0f? 0.0f : distance / speed;
            float startTime = Time.time;
            float endTime = startTime + duration;
            
            Vector3 start = EsdrasAnimatorGO.transform.position;
            Vector3 end   = EsdrasNPC.transform.position;
            
            Main.IterTormenti.Log($"EsdrasBehaviour::MoveToTargetCoroutine PreLoop: duration: {duration}, startTime: {startTime}, endTime: {endTime}");

            while (Time.time < endTime)
            {
                float currentTime = (Time.time - startTime) / duration;
                //Main.IterTormenti.Log($"EsdrasBehaviour::MoveToTargetCoroutine Loop: {currentTime}");
                EsdrasAnimatorGO.transform.position = Vector3.Lerp(start, end, Mathf.SmoothStep(0f, 1f, currentTime) );
                yield return null;
            }
            
            EsdrasAnimatorGO.transform.position = end;
            OnMovementComplete();        
        }

        private IEnumerator lookAtEsdrasCoroutine;

        private IEnumerator PenitentLookAtEsdrasCoroutine()
        {
            Main.IterTormenti.Log("EsdrasBehaviour::PenitentLookAtEsdrasCoroutine");
            
            while(true)
            {
                PenitentLookAtEsdras();
                yield return new WaitForSeconds(0.2f);
            }
        }

        private void  PenitentLookAtEsdras()
        {
            Main.IterTormenti.Log("EsdrasBehaviour::PenitentLookAtEsdras");

            Penitent penitent = Core.Logic.Penitent.gameObject.GetComponent<Penitent>();
            Vector3 penitentPosition = Core.Logic.Penitent.gameObject.transform.position;

            if(EsdrasAnimatorGO.transform.position.x < penitentPosition.x && penitent.GetOrientation() == EntityOrientation.Right)
            {
                penitent.SetOrientation(EntityOrientation.Left);
            }

            if(EsdrasAnimatorGO.transform.position.x > penitentPosition.x && penitent.GetOrientation() == EntityOrientation.Left)
            {
                penitent.SetOrientation(EntityOrientation.Right);
            }
        }

        private void StopMovingAndFacePenitent(object item, EventArgs args)        
        {
            Main.IterTormenti.Log("EsdrasBehaviour::StopMovingAndFacePenitent");

            StopCoroutine(moveToTargetCoroutine);
            StopCoroutine(lookAtEsdrasCoroutine);
            PenitentLookAtEsdras(); // Just in case the movement was too short

            Vector3 penitentPosition = Core.Logic.Penitent.gameObject.transform.position;

            // EsdrasAnimator sprite is facing right by default
            // EsdrasNPC is facing left by default
            // Esdras run animation is facing right by default
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

        
        public void ReplaceAnimatorWithNPC()
        {
            Main.IterTormenti.Log("EsdrasBehaviour::ReplaceAnimatorWithNPC");
            EsdrasAnimatorGO.SetActive(false);
            EsdrasNPC.transform.Find("#Constitution/Body").gameObject.GetComponent<SpriteRenderer>().enabled = true;
            EsdrasNPC.transform.Find("#Constitution/Body/BlobShadow").gameObject.GetComponent<SpriteRenderer>().enabled = true;
            EsdrasNpcFSM.SetState("Make taunt animation 2");//"Wait9");
        }

        public void ResetEsdrasFacing()
        {
            EsdrasNPC.transform.Find("#Constitution/Body").gameObject.GetComponent<SpriteRenderer>().flipX = false;
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
