using System;
using System.Collections;
using UnityEngine;
using IterTormenti.Utils.Sprites.Animations;
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
            Animations = new Dictionary<string, SpriteAnimation>(0);
            _playing = false;
            _timeSinceLastUpdate = 0.0f;
            _waitingForAnimationChange = false;
            _activeAnimation = "";

            OnEndTransitions = new Dictionary<string, string>(0);
        }

        public SpriteAnimator(string name = "SpriteAnimator")
        {
            Name = name;
            sprites = new Sprite[0];
            Animations = new Dictionary<string, SpriteAnimation>(0);
            _playing = false;
            _timeSinceLastUpdate = 0.0f;
            _waitingForAnimationChange = false;
            _activeAnimation = "";

            OnEndTransitions = new Dictionary<string, string>(0);
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
            
            Animations = new Dictionary<string, SpriteAnimation>(0);
            foreach(KeyValuePair<string, SpriteAnimation> kvp in source.Animations)
            {
                SpriteAnimation anim = kvp.Value;
                Animations.Add(kvp.Key, new SpriteAnimation(ref anim));
            }

            _playing = false;
            _timeSinceLastUpdate = 0.0f;
            _waitingForAnimationChange = false;
            _activeAnimation = "";

            OnEndTransitions = new Dictionary<string, string>(0);
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

        public Dictionary<string, SpriteAnimation> Animations { get; set; }

        public SpriteAnimation CurrentAnimation
        {
            get
            {               
                SpriteAnimation retVal = null;                
                Animations.TryGetValue(_activeAnimation, out retVal);
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
                if(Animations.ContainsKey(value))
                {
                    _activeAnimation = value;
                }                
            }            
        }

        // -- Methods --

        private string _nextAnimation;
        private int _nextAnimationIndex;

        private void ChangeAnimation()
        {
            //Main.IterTormenti.Log($"SpriteAnimation::ChangeAnimation: {CurrentAnimation.Name} => {_nextAnimation}");
            
            ActiveAnimation = _nextAnimation;
            CurrentAnimation.Index = _nextAnimationIndex;

            _waitingForAnimationChange = false;
        }
        
        /// <summary>
        /// Start playing the animation.
        /// </summary>
        public void Play()
        {
            Main.IterTormenti.Log("SpriteAnimation::Play");

            foreach(SpriteAnimation anim in Animations.Values)
            {
                anim.AnimationCompleted += OnAnimationEnded;
            }

            _playing = true;
        }


        /// <summary>
        /// Stop playing the animation, and reset the animation index to the first frame.
        /// </summary>
        public void Stop()
        {
            Main.IterTormenti.Log("SpriteAnimation::Stop");

            foreach(SpriteAnimation anim in Animations.Values)
            {
                anim.AnimationCompleted -= OnAnimationEnded;
            }

            _playing = false;
        }

        /// <summary>
        /// Pause/Unpause the animation at the current frame.
        /// </summary>
        public void Pause()
        {
            Main.IterTormenti.Log("SpriteAnimation::Pause");
            _playing |= false;
        }

        override public string ToString()
        {
            string text = $"{{ Sprites: {sprites.Length}, Animations: {Animations.Count} [ ";
            foreach(SpriteAnimation anim in Animations.Values)
            {
                if(null == anim) continue;
                text += anim.ToString() + ", ";
            }
            text += " ] }";
            return text;
        }

        // --- Animation Handling ---

        public void GoToAnimation(string name, int index = 0, bool synched = true)
        {
            //Main.IterTormenti.Log($"SpriteAnimator::GoToAnimation: '{name}' synched? {synched}");
            
            if(!Animations.ContainsKey(name))
            {
                Main.IterTormenti.LogError($"SpriteAnimator::GoToAnimation: ERROR: '{name}' does not match an existing animation!");
                return;
            }

            _nextAnimation = name;
            _nextAnimationIndex = index;

            if(synched)
            {
                _waitingForAnimationChange = true;
            }
            else
            {
                ChangeAnimation();
                UpdateFrame();
            }            
        }

        
        // --- Animation Event Handling ---
        // TODO: Placeholder until a better solution with the FSM?


        public Dictionary<string, string> OnEndTransitions;

        public void OnAnimationEnded(object sender, AnimationEventArgs args)
        {
             //Main.IterTormenti.Log($"SpriteAnimationr::OnAnimationEnded({args.Name})");

            if(!OnEndTransitions.ContainsKey(args.Name)) return;

            GoToAnimation(OnEndTransitions[args.Name], 0, false);
        }

        public void MakeAnimationLoop(string name)
        {
            OnEndTransitions[name] = name;
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

            _timeSinceLastUpdate += Time.deltaTime;

            if(null == CurrentFrame) return;

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

        /// <summary>
        /// Name of the currently active animation
        /// </summary>
        private string _activeAnimation;
    }
}