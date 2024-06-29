using System;
using System.Collections;
using UnityEngine;
using IterTormenti.Utils.Sprites.Animations;
using IterTormenti.Utils.Sprites.Animations.Fsm;
using System.Collections.Generic;
using System.Linq;

namespace IterTormenti.Utils.Sprites
{
    public abstract class ANIMATION
    {
        public const string NO_ANIMATION = "";
    }

    /// <summary>
    /// Sprite animation manager. It is a simplified emulation of the Unity Animator, 
    /// but focused exclusively on sprite animations.
    /// Allows defining multiple animations that will use a single Sprite collection,
    /// as well as a single SpriteRenderer.
    /// Animations can be selected directly via accessor methods.
    /// Simple state machine workflows can be implemented to allow automatically
    /// switching animations.
    /// </summary>
    [Serializable]
    class SpriteAnimator : MonoBehaviour
    {
        private SpriteAnimator()
        {
            Name = "SpriteAnimator";
            sprites = new Sprite[0];
            _animations = new Dictionary<string, SpriteAnimation>(0);
            Fsm = new AnimationFsm(Name + "Fsm");
            Fsm.FsmEvent += OnFsmEvent;

            _playing = false;
            _timeSinceLastUpdate = 0.0f;
            _waitingForAnimationChange = false;
            _activeAnimation = "";
        }

        public SpriteAnimator(string name = "SpriteAnimator")
        {
            Name = name;
            sprites = new Sprite[0];
            _animations = new Dictionary<string, SpriteAnimation>(0);
            Fsm = new AnimationFsm(Name + "Fsm");
            Fsm.FsmEvent += OnFsmEvent;

            _playing = false;
            _timeSinceLastUpdate = 0.0f;
            _waitingForAnimationChange = false;
            _activeAnimation = "";
        }

        public SpriteAnimator(ref SpriteAnimator source)
        {
            Clone(ref source);
        }
        
        /// <summary>
        /// Creates a depp copy of the Animator.
        /// In an attempt to keep names unique, the name of the copy will be 
        /// modified.
        /// </summary>
        /// <param name="source">SpriteAnimator to clone</param>
        public void Clone(ref SpriteAnimator source)
        {
            this.Name = source.Name + "_copy";
            
            sprites = new Sprite[source.sprites.Length];
            Array.Copy( source.sprites, sprites, source.sprites.Length );
            
            _animations = new Dictionary<string, SpriteAnimation>(0);
            foreach(KeyValuePair<string, SpriteAnimation> kvp in source._animations)
            {
                SpriteAnimation anim = kvp.Value;
                _animations.Add(kvp.Key, new SpriteAnimation(ref anim));
            }

            Fsm = new AnimationFsm(source.Fsm);
            Fsm.FsmEvent += OnFsmEvent;

            _playing = false;
            _timeSinceLastUpdate = 0.0f;
            _waitingForAnimationChange = false;
            _activeAnimation = "";
        }

        // -- Properties and Attribtues --

        /// <summary>
        /// Animator name
        /// </summary>
        public string Name {get; set;}

        /// <summary>
        /// Colection of sprites to use as animation frames.
        /// Animations will reference the indices of this collection.
        /// </summary>
        public Sprite[] sprites;

        /// <summary>
        /// SpriteRenderer used to display the animation sprites.
        /// </summary>
        public SpriteRenderer Renderer {get; set;}

        public SpriteAnimation[] Animations
        { 
            get{ return _animations.Values.ToArray(); }            
        }

        public SpriteAnimation AddAnimation(ref SpriteAnimation value)
        {
            if(null == value)
            {
                Main.IterTormenti.LogError("SpriteAnimator::AddAnimation: ERROR: Invalid animation!");
                return null;
            }

            if(_animations.ContainsKey(value.Name))
            {
                Main.IterTormenti.LogError($"SpriteAnimator::AddAnimation: ERROR: Animation with name '{value.Name}' already exists");
                return null;
            }

            _animations.Add(value.Name, new SpriteAnimation(ref value));

            SpriteAnimation retVal = null;                
            _animations.TryGetValue(value.Name, out retVal);
            return retVal;
        }
       
        public SpriteAnimation CurrentAnimation
        {
            get
            {               
                SpriteAnimation retVal = null;                
                _animations.TryGetValue(_activeAnimation, out retVal);
                return retVal;
            }
        }

        public Frame CurrentFrame
        { 
            get
            {
                if(null == CurrentAnimation) return null;
                return CurrentAnimation.CurrentFrame;
            }
        }

        public string ActiveAnimation
        {
            get { return _activeAnimation; }
            set
            {
                if(_animations.ContainsKey(value))
                {
                    _activeAnimation = value;
                }                
            }            
        }

        // -- FSM --

        public AnimationFsm Fsm { get; private set; }

        // -- Methods --

        private void ChangeAnimation()
        {
            CurrentAnimation.AnimationCompleted -= Fsm.OnAnimationEvent;
            
            ActiveAnimation = Fsm.ActiveState.AnimationName;
            CurrentAnimation.Index = 0;

            CurrentAnimation.AnimationCompleted += Fsm.OnAnimationEvent;

            _waitingForAnimationChange = false;
        }
        
        /// <summary>
        /// Start playing the animation.
        /// </summary>
        public void Play()
        {
            if(null == CurrentAnimation) return; // No animations to play!

            // Register callbacks
            // foreach(SpriteAnimation animation in animations)
            // {
            //     animation.AnimationCompleted += OnAnimationEvent;
            // }

            CurrentAnimation.AnimationCompleted += Fsm.OnAnimationEvent;
            AnimatorEvent += Fsm.OnAnimationEvent;
            
            
            _playing = true;
        }


        /// <summary>
        /// Stop playing the animation, and reset the animation index to the first frame.
        /// </summary>
        public void Stop()
        {
            // Deregister callbacks
            // foreach(SpriteAnimation animation in animations)
            // {
            //     animation.AnimationCompleted -= OnAnimationEvent;
            // }

            CurrentAnimation.AnimationCompleted -= Fsm.OnAnimationEvent;
            AnimatorEvent -= Fsm.OnAnimationEvent;

            _playing = false;            
        }

        /// <summary>
        /// Pause the animation at the current frame.
        /// </summary>
        public void Pause()
        {
            _playing = false;
        }

        override public string ToString()
        {
            string text = $"{{ Sprites: {sprites.Length}, Animations: {_animations.Count} [ ";
            foreach(SpriteAnimation anim in _animations.Values)
            {
                if(null == anim) continue;
                text += anim.ToString() + ", ";
            }
            text += " ] }";
            return text;
        }

        // --- Event handling ---

        public event EventHandler<AnimationEventArgs> AnimatorEvent;

        public void CallEvent(AnimationEventArgs eventArgs)
        {
            AnimatorEvent?.Invoke(this, eventArgs);   
        }

        protected void OnAnimationEvent(object animation, AnimationEventArgs eventArgs)
        {
            if(!_playing)
            {
                Main.IterTormenti.LogError($"ERROR: Animation  '{eventArgs.Name}' has somehow completed while not playing!");
            }

            Main.IterTormenti.Log($"Animation '{eventArgs.Name}' is complete");
        }

        protected void OnFsmEvent(object source, FsmEventArgs eventArgs) //TODO: Register to events
        {
            Main.IterTormenti.Log($"SpriteAnimator:OnFsmEvent: '{eventArgs.Name}'");

            if(eventArgs.Name.Equals(AnimationFsm.EVENT.STATE_CHANGED))
            {
                if(eventArgs.Synched)
                {
                    _waitingForAnimationChange = true;
                }
                else
                {
                    ChangeAnimation();
                    UpdateFrame();
                }
            }
            
            if(eventArgs.Name.Equals(AnimationFsm.EVENT.END_REACHED))
            {
                Stop();
                enabled = false;
                // TODO: Is this enough to hide everything?
            }
        }



        // --- MonoBehaviour methods --


        /// <summary>
        /// Awake is called when the script instance is being loaded.
        /// https://docs.unity3d.com/2017.4/Documentation/ScriptReference/MonoBehaviour.Awake.html
        /// </summary>
        void Awake()
        {
            _timeSinceLastUpdate = 0.0f;
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
            if(!_playing) return;
            if(null == CurrentFrame) return;
            
            _timeSinceLastUpdate += Time.deltaTime;

            // Check if we've waited enough
            if (_timeSinceLastUpdate < CurrentAnimation.Delay) return;

            if(_waitingForAnimationChange)
            {
                ChangeAnimation();
            }
            else
            {
                CurrentAnimation.NextFrame();
            }

            UpdateFrame();
        }

        private void UpdateFrame()
        {
            _timeSinceLastUpdate = 0.0f;

            Renderer.sprite = sprites[CurrentFrame.Index];
        }


        // -- Internal attributes --

        /// <summary>
        /// Indicates if the animation is playing
        /// </summary>
        private bool _playing;

        /// <summary>
        /// Time, in seconds, ellapsed since the last frame was updated
        /// </summary>          
        private float _timeSinceLastUpdate;

        /// <summary>
        /// Indicates if a state change happened and we need to change
        /// the active animation in the next frame.
        /// </summary>
        private bool _waitingForAnimationChange;

        private Dictionary<string, SpriteAnimation> _animations;

        /// <summary>
        /// Name of the currently active animation
        /// </summary>
        private string _activeAnimation;
    }
}