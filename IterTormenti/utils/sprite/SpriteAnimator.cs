using System;
using System.Collections;
using UnityEngine;
using static IterTormenti.utils.sprite.Animation;


namespace IterTormenti.utils.sprite
{
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
        public SpriteAnimator()
        {
            sprites = new Sprite[0];
            animations = new Animation[0];
        }

        public SpriteAnimator(SpriteAnimator source)
        {
            Clone(source);
        }
        
        /// <summary>
        /// Creates a depp copy of the Animator.
        /// In an attempt to keep names unique, the name of the copy will be 
        /// modified.
        /// </summary>
        /// <param name="source">SpriteAnimator to clone</param>
        public void Clone(SpriteAnimator source)
        {
            this.Name = source.Name + "_copy";
            
            sprites = new Sprite[source.sprites.Length];
            Array.Copy( source.sprites, sprites, source.sprites.Length );
            
            animations = new Animation[source.animations.Length];
            for(int idx = 0; idx < animations.Length; idx++)
            {
                animations[idx] = new Animation(source.animations[idx]);
            }
            
            //TODO: MOre
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


        public Animation[] animations;
       
        public Animation CurrentAnimation
        {
            get
            {
                if(animations.Length == 0) return null;
                if(_index < 0 || _index >= animations.Length) return null;
                return animations[_index];
            }
        }

        public Animation.Frame CurrentFrame
        { 
            get
            {
                if(null == CurrentAnimation) return null;
                return CurrentAnimation.CurrentFrame;
            }
        }

        public int AnimationIndex
        {
            get { return _index; }
            set
            {
                if(animations.Length == 0) _index = -1;
                else if(value < 0) _index = 0;
                else if(value >= animations.Length) _index = animations.Length - 1;
                else _index = value;
            }
        }

        public string AnimationName
        {
            get { return null == CurrentAnimation ? "" : CurrentAnimation.Name; }
            set
            {
                for(int idx = 0; idx < animations.Length; idx++)
                {
                    if(animations[idx].Name.Equals(value))
                    {
                        AnimationIndex = idx;
                        return;
                    }
                }
            }            
        }

        // -- Methods --

        /// <summary>
        /// Start playing the animation.
        /// </summary>
        public void Play()
        {
            if(null == CurrentAnimation) return; // No animations to play!

            // Register callbacks
            foreach(Animation animation in animations)
            {
                animation.AnimationCompleted += OnAnimationCompleted;
            }
            
            _playing = true;
        }


        /// <summary>
        /// Stop playing the animation, and reset the animation index to the first frame.
        /// </summary>
        public void Stop()
        {
            // Deregister callbacks
            foreach(Animation animation in animations)
            {
                animation.AnimationCompleted -= OnAnimationCompleted;
            }

            _playing = false;
            _index = 0;
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
            string text = $"{{ Sprites: {sprites.Length}, Animations: {animations.Length} [ ";
            foreach(Animation anim in animations)
            {
                text += anim.ToString() + ", ";
            }
            text += " ] }";
            return text;
        }

        // --- Event handling ---

        protected void OnAnimationCompleted(object animation, AnimationEventArgs eventArgs)
        {
            if(!_playing)
            {
                Main.IterTormenti.LogError($"ERROR: Animation  '{eventArgs.Name}' has somehow completed while not playing!");
            }

            Main.IterTormenti.Log($"Animation '{eventArgs.Name}' is complete");
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

            CurrentAnimation.NextFrame();

            _timeSinceLastUpdate = 0.0f;

            Renderer.sprite = sprites[CurrentFrame.Index];
        }


        // -- Internal attributes --


        /// <summary>
        /// Index of the currently active animation
        /// </summary>
        private int _index = 0;

        /// <summary>
        /// Indicates if the animation is playing
        /// </summary>
        private bool _playing = false;

        /// <summary>
        /// Time, in seconds, ellapsed since the last frame was updated
        /// </summary>          
        private float _timeSinceLastUpdate = 0.0f;        
    }
}