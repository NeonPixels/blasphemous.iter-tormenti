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
    /// <summary>
    /// Behaviour for the Animator that transitions between the 
    /// Esdras Boss and NPC.
    /// Meant to be invoked from the FSM, so we can more easily
    /// implement functionality that is cumbersome to manually add
    /// to the FSM.
    /// </summary>
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


        /// <summary>
        /// Reference to the BOSS_FIGHT_STUFF GameObject.
        /// </summary>
        public GameObject BossFightStuff {get; set;}

        /// <summary>
        /// Reference to the Esdras Boss GameObject, contained in BossFightStuff.
        /// </summary>
        public GameObject EsdrasBoss
        {
            get
            {
                if(null == BossFightStuff) return null;
                return BossFightStuff.transform.Find("Esdras").gameObject;
            }
        }

        /// <summary>
        /// Reference to the Esdras NPC GameObject.
        /// </summary>
        public GameObject EsdrasNPC {get; set;}

        /// <summary>
        /// Reference to the Esdras NPC FSM object, contained in EsdrasNPC.
        /// </summary>
        public PlayMakerFSM EsdrasNpcFSM
        {
            get
            {
                if(null == EsdrasNPC) return null;
                return PlayMakerFSM.FindFsmOnGameObject(EsdrasNPC, "FSM");
            }
        }

        /// <summary>
        /// Reference to the Esdras Animator GameObject.
        /// </summary>
        public GameObject EsdrasAnimatorGO {get; set;}

        /// <summary>
        /// Reference to the Esdras Animator object, contained in EsdrasAnimatorGO.
        /// </summary>
        public SpriteAnimator EsdrasAnimator
        {
            get
            {
                if(null == EsdrasAnimatorGO) return null;
                return EsdrasAnimatorGO.GetComponent<SpriteAnimator>();
            }
        }

        /// <summary>
        /// Reference to the PerpetvaVFX object, which contains the
        /// visual effect for Perpetva's appearance.
        /// </summary>
        public GameObject PerpetvaVFX {get; set;}


        /// <summary>
        /// Reference to the object containing the position towards which
        /// Esdras will leave the screen.
        /// </summary>
        public GameObject EsdrasTarget {get; set;}


        // -- Methods --


        /// <summary>
        /// Updates the position of the different elements, based on the final position
        /// of Esdras and the Penitent at the end of the fight.
        /// </summary>
        private void MoveElementsToFinalPosition()
        {
            EsdrasAnimatorGO.transform.position = EsdrasBoss.transform.position;
            
            Vector3 esdrasNpcFinalPosition = CalculateEsdrasNpcPosition();
            EsdrasTarget.transform.position = CalculateEsdrasTargetPosition(esdrasNpcFinalPosition);
            EsdrasNPC.transform.position = esdrasNpcFinalPosition;

            PerpetvaVFX.transform.position = CalculatePerpetvaPosition();           
        }

        /// <summary>
        /// We determine the position the EsdrasNPC will be at after the fight,
        /// which is also the position towards which the Animator version of Esdras
        /// will run towards after being defeated.
        /// There are two possible positions: The original position of the NPC, or an alternate
        /// possition on the other side of the arena.
        /// The position will be chosen based on which side of the arena the Penitent is.
        /// </summary>
        /// <returns>New position for EsdrasNPC</returns>
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
           
            // Select the target depending on which side of the arena the penitent is at
            if(penitentPosition.x <=  bossfightCenter.x)
            {
                return targetRight;
            }
            else
            {
                return targetLeft;
            }
        }

        
        /// <summary>
        /// We need to move the offscreen target that Esdras moves out to after his dialog,
        /// otherwise, if he is closer to the target, the animation tweening will make him
        /// move slowly
        /// </summary>
        /// <param name="esdrasNpcFinalPosition">Final position for the NPC</param>
        /// <returns>New position for the target</returns>
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

        /// <summary>
        /// Perpetva starts at the left end of the arena, and she always faces the same direction as the Penitent.
        /// If Esdras moves to the left side of the arena, we update her position to be on the other end of the arena,
        /// so he is always looking towards her when she appears.
        /// </summary>
        /// <returns>New position for Perpetva</returns>
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

        /// <summary>
        /// Function to hide the Boss sprite, and replace it with the Animator used to
        /// transition to the NPC.
        /// The Animator will be made to face the Penitent.
        /// The Penitent will be made to seathe its weapon once the death animation ends.
        /// The EsdrasNPC FSM will be instructed to apply an Input Block on the player.
        /// This function is to be called from a FSM action.
        /// </summary>
        public void ReplaceBossWithAnimator()
        {
            MoveElementsToFinalPosition();
            
            Vector3 penitentPosition = Core.Logic.Penitent.gameObject.transform.position;

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
        private void PenitentSeathWeapon(object item, AnimationEventArgs args)
        {
            Core.Logic.Penitent.Animator.Play(_sheathedAnim);
		    Core.Logic.Penitent.Animator.SetBool("IS_DIALOGUE_MODE", true);
        }

        /// <summary>
        /// Function used to make Esdras stand up and move to his final position,
        /// to be called from a FSM action.
        /// </summary>
        public void SetAnimatorToStandUp()
        {
            EsdrasAnimator.Animations["EsdrasPickUpWeapon"].AnimationCompleted += MoveAnimatorToTarget;
            EsdrasAnimator.GoToAnimation("EsdrasPickUpWeapon");
        }


        // ---- Movement -----

        public event EventHandler<EventArgs> MovementComplete;

        /// <summary>
        /// Event issued when the movement is complete.
        /// </summary>
        protected virtual void OnMovementComplete()
        {
            MovementComplete?.Invoke(this, new EventArgs());
        }

        /// <summary>
        /// Method that causes the animator to move to the EsdrasNPC position.
        /// To be called by an animation event.
        /// </summary>
        /// <param name="item">Object issuing the event</param>
        /// <param name="args">Event arguments</param>
        private void MoveAnimatorToTarget(object item, AnimationEventArgs args)
        {
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

            EsdrasAnimator.GoToAnimation("EsdrasRun");
            
            moveToTargetCoroutine = MoveToTargetCoroutine(4.0f);
            StartCoroutine(moveToTargetCoroutine);
            
            MovementComplete += StopMovingAndFacePenitent;
        }

        private IEnumerator moveToTargetCoroutine;

        /// <summary>
        /// Coroutine managing the movement of the animator.
        /// </summary>
        /// <param name="speed">Speed to move towards the target</param>
        /// <returns>Coroutine IEnumerator</returns>
        private IEnumerator MoveToTargetCoroutine(float speed)
        {
            float distance = Vector3.Distance(EsdrasNPC.transform.position, EsdrasAnimatorGO.transform.position);

            float duration = speed <= 0.0f? 0.0f : distance / speed;
            float startTime = Time.time;
            float endTime = startTime + duration;
            
            Vector3 start = EsdrasAnimatorGO.transform.position;
            Vector3 end   = EsdrasNPC.transform.position;
            
            // Do some tweening
            while (Time.time < endTime)
            {
                float currentTime = (Time.time - startTime) / duration;
                EsdrasAnimatorGO.transform.position = Vector3.Lerp(start, end, Mathf.SmoothStep(0f, 1f, currentTime) );
                yield return null;
            }
            
            EsdrasAnimatorGO.transform.position = end;
            OnMovementComplete();        
        }

        private IEnumerator lookAtEsdrasCoroutine;

        /// <summary>
        /// Coroutine that keeps the Penitent looking at Esdras,
        /// so they turn to look at him as he moves by.
        /// </summary>
        /// <returns>Coroutine IEnumerator</returns>
        private IEnumerator PenitentLookAtEsdrasCoroutine()
        {
            while(true)
            {
                PenitentLookAtEsdras();
                yield return new WaitForSeconds(0.2f);
            }
        }

        /// <summary>
        /// Updates Penitent facing to look at Esdras animator.
        /// </summary>
        private void  PenitentLookAtEsdras()
        {
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

        /// <summary>
        /// Method that stops the coroutines, makes the EsdrasNPC look towards the
        /// Penitent.
        /// Called from an event.
        /// </summary>
        /// <param name="item">Object issuing event</param>
        /// <param name="args">Event arguments</param>
        private void StopMovingAndFacePenitent(object item, EventArgs args)        
        {
            StopCoroutine(moveToTargetCoroutine);
            StopCoroutine(lookAtEsdrasCoroutine);
            PenitentLookAtEsdras(); // Just in case the movement was too short

            Vector3 penitentPosition = Core.Logic.Penitent.gameObject.transform.position;

            // Flip EsdrasNPC sprite renderer to face Penitent
            if(EsdrasNPC.transform.position.x > penitentPosition.x)
            {
                EsdrasNPC.transform.Find("#Constitution/Body").gameObject.GetComponent<SpriteRenderer>().flipX = false;
            }
            else
            {
                EsdrasNPC.transform.Find("#Constitution/Body").gameObject.GetComponent<SpriteRenderer>().flipX = true;
            }

            ReplaceAnimatorWithNPC();
        }

        /// <summary>
        /// Disables the Animator, makes the NPC sprites visible,
        /// and sets the EsdrasNPC FSM state to start the final animation.
        /// </summary>          
        private void ReplaceAnimatorWithNPC()
        {
            EsdrasAnimatorGO.SetActive(false);
            EsdrasNPC.transform.Find("#Constitution/Body").gameObject.GetComponent<SpriteRenderer>().enabled = true;
            EsdrasNPC.transform.Find("#Constitution/Body/BlobShadow").gameObject.GetComponent<SpriteRenderer>().enabled = true;
            EsdrasNpcFSM.SetState("Make taunt animation 2");
        }

        /// <summary>
        /// Resets EsdrasNPC sprite renderer to its default value, so when Esdras runs offscreen he has the 
        /// correct facing.
        /// To be called from a FSM action.
        /// </summary>
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
