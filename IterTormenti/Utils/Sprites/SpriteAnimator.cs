using Blasphemous.ModdingAPI;
using System;
using System.Collections;
using UnityEngine;
using IterTormenti.Utils.Sprites.Animations;
using System.Collections.Generic;
using IterTormenti.Utils.Audio;

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
        /// <summary>
        /// Make default constructor empty so it isn't used.
        /// Note: Seems to be necessary to implement this constructor for Unity.
        /// </summary>
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
            AudioPlayer = null;
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
            AudioPlayer = null;
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
            AudioPlayer = source.AudioPlayer;
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

        /// <summary>
        /// Map of SpriteAnimations registered. The animation name is used as the
        /// key value.
        /// TODO: Implement a way to read sprite and animation
        ///       configurations from json files, so they don't
        ///       need to be hard-coded
        /// </summary>
        public Dictionary<string, SpriteAnimation> Animations { get; set; }

        /// <summary>
        /// Currently active anumation. Will return null if no animation is active.
        /// </summary>
        public SpriteAnimation CurrentAnimation
        {
            get
            {               
                SpriteAnimation retVal = null;                
                Animations.TryGetValue(_activeAnimation, out retVal);
                return retVal;
            }
        }

        /// <summary>
        /// Current active frame of the currently active animation,
        /// will return null if no animation is active.
        /// </summary>
        public Frame CurrentFrame
        { 
            get
            {
                if(null == CurrentAnimation) return null;
                return CurrentAnimation.CurrentFrame;
            }
        }

        /// <summary>
        /// Name of the currently active animation.
        /// Will only be updated if the value matches an existing animation.
        /// </summary>
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

        /// <summary>
        /// Audio player that will manage animation audio events.
        /// </summary>
        public AudioPlayer AudioPlayer {get; set;}

        // -- Methods --


        /// <summary>
        /// Function used to managed delayed animation switching.
        /// </summary>
        private void ChangeAnimation()
        {
            ActiveAnimation = _nextAnimation;
            CurrentAnimation.Index = _nextAnimationIndex;

            _waitingForAnimationChange = false;
        }
        
        /// <summary>
        /// Start playing the animation.
        /// </summary>
        public void Play()
        {
            foreach(SpriteAnimation anim in Animations.Values)
            {
                anim.AnimationCompleted += OnAnimationEnded;
                anim.FrameAudio += OnFrameAudio;
            }

            UpdateFrame();

            _playing = true;
        }


        /// <summary>
        /// Stop playing the animation, and reset the animation index to the first frame.
        /// </summary>
        public void Stop()
        {
            foreach(SpriteAnimation anim in Animations.Values)
            {
                anim.AnimationCompleted -= OnAnimationEnded;
                anim.FrameAudio -= OnFrameAudio;
            }

            _playing = false;
        }

        /// <summary>
        /// Pause/Unpause the animation at the current frame.
        /// </summary>
        public void Pause()
        {
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

        /// <summary>
        /// Changes the active animation
        /// </summary>
        /// <param name="name">Animation to change to</param>
        /// <param name="index">Frame index to change to, default: 0</param>
        /// <param name="synched">If 'true', animation will wait until current frame ends
        /// before switching, if 'false' the animation will switch instantly, default: true</param>
        public void GoToAnimation(string name, int index = 0, bool synched = true)
        {
            if(!Animations.ContainsKey(name))
            {
                ModLog.Error($"SpriteAnimator::GoToAnimation: ERROR: '{name}' does not match an existing animation!");
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
        // TODO: Placeholder until a better solution with a FSM implementation?

        /// <summary>
        /// Map of transitions to do when a given animation ends.
        /// Key is the animation that ends, and the Value is the animation
        /// to transition to.
        /// </summary>
        public Dictionary<string, string> OnEndTransitions;

        /// <summary>
        /// AnimationEnded event handler. Will switch to the next animation
        /// if there is a transition defined on OnEndTransitions.
        /// </summary>
        /// <param name="sender">Event sender</param>
        /// <param name="args">Event arguments</param>
        private void OnAnimationEnded(object sender, AnimationEventArgs args)
        {
            if(!OnEndTransitions.ContainsKey(args.Name)) return;

            GoToAnimation(OnEndTransitions[args.Name], 0, false);
        }

        /// <summary>
        /// FrameAudio event handler. Will play the sound.
        /// TODO: Add extra audio features?
        /// </summary>
        /// <param name="sender">Event sender</param>
        /// <param name="args">Event arguments</param>
        private void OnFrameAudio(object sender, AudioEventArgs args)
        {
            AudioPlayer.Play(args.Name);
        }

#if DISABLED // TODO: Currently, all animations loop by default, as this doesn't seem to work
        /// <summary>
        /// Helper function that makes an animation loop by setting it as its
        /// own target for the OnEndTransition
        /// </summary>
        /// <param name="name">Animation to loop</param>
        public void MakeAnimationLoop(string name)
        {
            OnEndTransitions[name] = name;
        }
#endif

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
        /// Indicates if the animation is playing.
        /// </summary>
        private bool _playing;

        /// <summary>
        /// Time, in seconds, ellapsed since the last frame was updated.
        /// </summary>          
        private float _timeSinceLastUpdate;

        /// <summary>
        /// Indicates if a state change happened and we need to change
        /// the active animation in the next frame.
        /// </summary>
        private bool _waitingForAnimationChange;

        /// <summary>
        /// Name of the currently active animation.
        /// </summary>
        private string _activeAnimation;

        /// <summary>
        /// Name of the animation to switch to.
        /// </summary>                        
        private string _nextAnimation;

        /// <summary>
        /// Frame index to switch to.
        /// </summary>
        private int _nextAnimationIndex;
    }
}
