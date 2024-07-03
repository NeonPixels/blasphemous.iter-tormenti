using System;

namespace IterTormenti.Utils.Sprites.Animations
{
    /// <summary>
    /// Structure that defines animations to be used by the SpriteAnimator.
    /// </summary>
    [Serializable]
    public class SpriteAnimation
    {
        private SpriteAnimation()
        {
            Name = "SpriteAnimation";
            frames = new Frame[0];
            _delay = 0.0f;
            _index = 0;
        }

        public SpriteAnimation(string name)
        {
            Name = name;
            frames = new Frame[0];
            _delay = 0.0f;
            _index = 0;
        }

        public SpriteAnimation(ref SpriteAnimation source)
        {
            Clone(ref source);
        }
        
        /// <summary>
        /// Creates a depp copy of the Animator.
        /// In an attempt to keep names unique, the name of the copy will be 
        /// modified.
        /// </summary>
        /// <param name="source">Animation to clone</param>
        public void Clone(ref SpriteAnimation source)
        {
            this.Name = source.Name;
            frames = new Frame[source.frames.Length];
            for(int idx = 0; idx < frames.Length; idx++)
            {
                frames[idx] = new Frame(ref source.frames[idx]);
            }
            _delay = source._delay;
            _index = source._index;
        }

        // -- Properties and Attribtues --

        /// <summary>
        /// Animator name
        /// </summary>
        public string Name {get; set;}

        /// <summary>
        /// Colection of sprites to use as animation frames.
        /// </summary>
        public Frame[] frames;

        /// <summary>
        /// Returns the current animation frame.
        /// Will return null if there are no frames in the animation.
        /// </summary>
        public Frame CurrentFrame
        {
            get
            {
                if(frames.Length == 0) return null;

                return frames[_index];
            }
        }

        /// <summary>
        /// Current delay, in seconds, to the next frame.
        /// It will be the delay defined by the current frame, or, if none is
        /// defined, then it will be the default delay value;
        /// Minimum value is 0.0
        /// </summary>
        public float Delay
        {
            get
            {
                if(null == CurrentFrame) return _delay;
                if(CurrentFrame.Delay < 0.0f) return _delay;
                
                return CurrentFrame.Delay;
            }
        }

        /// <summary>
        /// Default delay, in seconds, between every frame.
        /// Will be used on frames that do not define their own delay.
        /// Values under 0.0 will be ignored.
        /// </summary>
        public float DefaultDelay
        {
            get { return _delay; }
            set { if(value >= 0.0f) _delay = value; }
        }

        public int Index 
        {
            get { return _index; }
            set 
            {
                if(frames.Length == 0) _index = -1;
                else if(value < 0) _index = 0;
                else if(value >= frames.Length) _index = frames.Length - 1;
                else _index = value;
            }
        }


        // -- Event Notification --

        public event EventHandler<AnimationEventArgs> AnimationCompleted;

        protected virtual void OnAnimationCompleted()
        {
            AnimationCompleted?.Invoke(this, new AnimationEventArgs(AnimationEventArgs.ON_ANIMATION_END){Name = this.Name});
        }

        // -- Methods --

        

        /// <summary>
        /// Returns the animation frame at the specified index.
        /// Will return null if the index is not valid or if
        /// there are no frames in the animation.
        /// </summary>
        public Frame GetFrameAt(int value)
        {
            if( frames.Length == 0
                || value < 0
                || value >= frames.Length )
            {
                return null;
            }

            return frames[_index];
        }

        /// <summary>
        /// Advance frame index by one, and return the resulting Frame.
        /// If the end of the animation is reached an event will be raised.
        /// If there are no defined frames, it will have no effect.
        /// All animations loop, it is up to the event handler
        /// to decide what action to perform once the end of the animation is reached.
        /// </summary>
        public Frame NextFrame()
        {
            if(frames.Length == 0) return null;
            
            _index++;
            
            if(_index >= frames.Length)
            {
                OnAnimationCompleted();

                _index = 0;
                //_index = frames.Length - 1;
            }

            return CurrentFrame;
        }
        
        override public string ToString()
        {
            string text = $"{{ Name: '{Name}', Delay: {Delay}s, Frames: {frames.Length} [ ";
            foreach(Frame frame in frames)
            {
                text += (null == frame? "NULL" : frame.ToString()) + ", ";
            }
            text += " ] }";

            return text;
        }

        // -- Internal attributes --


        /// <summary>
        /// Index of the current frame
        /// </summary>
        private int _index;

        /// <summary>
        /// Contains the delay, in seconds, between frames
        /// </summary>
        private float _delay;
    }
}