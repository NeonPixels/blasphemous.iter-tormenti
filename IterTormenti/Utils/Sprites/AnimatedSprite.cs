using System;
using System.Collections;
using UnityEngine;


namespace IterTormenti.Utils.Sprites
{
    /// <summary>
    /// Simple animated sprite. It will update the sprite displayed by the assigned SpriteRenderer
    /// from a collection of sprites, waiting the defined delay between each frame.
    /// </summary>
    [Serializable]
    class AnimatedSprite : MonoBehaviour
    {
        public AnimatedSprite()
        {
            frames = new Sprite[0];
        }

        public AnimatedSprite(AnimatedSprite source)
        {
            Clone(source);
        }
        
        /// <summary>
        /// Creates a deep copy of the AnimatedSprite.
        /// In an attempt to keep names unique, the name of the copy will be 
        /// modified.
        /// </summary>
        /// <param name="source">SpriteAnimator to clone</param>
        public void Clone(AnimatedSprite source)
        {
            this.Name = source.Name + "_copy";
            frames = new Sprite[source.frames.Length];
            Array.Copy( source.frames, frames, source.frames.Length );
        }

        // -- Properties and Attribtues --

        /// <summary>
        /// Sprite name
        /// </summary>
        public string Name {get; set;}

        /// <summary>
        /// Colection of sprites to use as animation frames.
        /// </summary>
        public Sprite[] frames;

        /// <summary>
        /// SpriteRenderer used to display the animation sprites.
        /// </summary>
        public SpriteRenderer Renderer {get; set;}

        /// <summary>
        /// Delay, in seconds, between every frame.
        /// Minimum value is 0.0
        /// </summary>
        public float Delay
        {
            get { return _delay; }
            set { _delay = value < 0.0f ? 0.0f : value; }
        }

        /// <summary>
        /// Determines if the animation will loop continuously, or will stop
        /// when reaching the last frame.
        /// </summary>
        public bool Loop { get; set; }


        // -- Methods --

        /// <summary>
        /// Start playing the animation.
        /// </summary>
        public void Play()
        {
            if(frames.Length == 0) return; // No frames to play!

            _playing = true;
        }

        /// <summary>
        /// Stop playing the animation, and reset the animation index to the first frame.
        /// </summary>
        public void Stop()
        {
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
            return $"{{ Frames: {frames.Length}, Delay: {Delay}s, Loop: {Loop} }}";            
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

            if (_timeSinceLastUpdate < _delay) return;

            _timeSinceLastUpdate = 0.0f;

            _index++;

            if(_index >= frames.Length)
            {
                if(!Loop)
                {
                    Stop();
                    _index = frames.Length - 1;                    
                }
                else
                {
                    _index = 0;
                }
            }

            Renderer.sprite = frames[_index];
        }


        // -- Internal attributes --


        /// <summary>
        /// Index of the currently displayed sprite
        /// </summary>
        private int _index = 0;

        /// <summary>
        /// Indicates if the animation is playing
        /// </summary>
        private bool _playing = false;

        /// <summary>
        /// Contains the delay, in seconds, between frames
        /// </summary>
        private float _delay = 1.0f;

        /// <summary>
        /// Time, in seconds, ellapsed since the last frame was updated
        /// </summary>          
        private float _timeSinceLastUpdate = 0.0f;        
    }
}