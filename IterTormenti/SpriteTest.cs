using System;
using System.Collections;
using Blasphemous.ModdingAPI;
using Blasphemous.ModdingAPI.Files;
using HarmonyLib;
using UnityEngine;


namespace IterTormenti
{
    static class Test
    {
        public static void Load()//out GameObject spriteTestGO)
        {
            GameObject spriteTestGO = new GameObject("SpriteTest");
            {
                SpriteRenderer renderer = spriteTestGO.AddComponent<SpriteRenderer>();
                renderer.enabled = true;
                renderer.drawMode = SpriteDrawMode.Simple;
                renderer.sortingLayerName = "After Player";

            }

            Animator animator = spriteTestGO.AddComponent<Animator>();
            {
                SpriteImportOptions importOptions = new SpriteImportOptions()
                {
                    Pivot = new Vector2(0.5f, 0.0f)
                };

                animator.LoadFrames("MjAnimSpritesheet.png", new Vector2(56f,66f), importOptions);

                Animation dance = new Animation("dance");
                Animation jump = new Animation("jump");
                dance.Delay = jump.Delay = 0.07f;
                dance.Loop = true;
                jump.Loop = true;

                dance.Frames = new Frame[animator.Frames.Length - 7];
                jump.Frames  = new Frame[7];

                int spriteIndex = 0;
                for(int frameIndex = 0; frameIndex < dance.Frames.Length; frameIndex++)
                {
                    dance.Frames[frameIndex] = new Frame(spriteIndex);
                    spriteIndex++;
                }

                for(int frameIndex = 0; frameIndex < jump.Frames.Length; frameIndex++)
                {
                    jump.Frames[frameIndex] = new Frame(spriteIndex);
                    spriteIndex++;
                }

                animator.Animations = new Animation[]{ dance, jump };
                animator.Renderer = spriteTestGO.GetComponent<SpriteRenderer>();
            }

            animator.ActiveName = "dance";
            spriteTestGO.transform.position = new Vector3(-90.0f,8.1f,0.0f);
            spriteTestGO.SetActive(true);


            Main.IterTormenti.Log("animator: " + animator.ToString());

            animator.Play();


            GameObject spriteTestGO2 = new GameObject("SpriteTest2");
            {
                SpriteRenderer renderer2 = spriteTestGO2.AddComponent<SpriteRenderer>();
                renderer2.enabled = true;
                renderer2.drawMode = SpriteDrawMode.Simple;
                renderer2.sortingLayerName = "Before Player";
            }
            
            Animator animator2 = spriteTestGO2.AddComponent<Animator>();
            {
                animator2.Clone(animator);
                animator2.Renderer = spriteTestGO2.GetComponent<SpriteRenderer>();

                animator2.GetByName("dance").Next = "jump";
                animator2.GetByName("dance").Loop = false;

                animator2.GetByName("jump").Next = "dance";
                animator2.GetByName("jump").Loop = false;
            }
            
            spriteTestGO2.transform.position = new Vector3(-93.0f,7.6f,0.0f);
            animator2.ActiveName = "dance";
            spriteTestGO2.SetActive(true);
           
            Main.IterTormenti.Log("animator2: " + animator2.ToString());

            animator2.Play();
        }
    }

    [Serializable]
    class Animator : MonoBehaviour
    {
        public Animator()
        {
            _animations = new Animation[0];
            _frames = new Sprite[0];
        }

        public Animator(Animator original)
        {
            _animations = new Animation[original._animations.Length];
            for(int idx = 0; idx < _animations.Length; idx++)
            {
                _animations[idx] = new Animation(original._animations[idx]);
            }

            _frames = new Sprite[original._frames.Length];
            Array.Copy( original._frames, _frames, original._frames.Length );

            _activeIndex = original._activeIndex;
        }

        public void Clone(Animator original)
        {
            _animations = new Animation[original._animations.Length];
            for(int idx = 0; idx < _animations.Length; idx++)
            {
                _animations[idx] = new Animation(original._animations[idx]);
            }

            _frames = new Sprite[original._frames.Length];
            Array.Copy( original._frames, _frames, original._frames.Length );

            _activeIndex = original._activeIndex;
        }

        private Animation[] _animations;

        public Animation[] Animations
        {
            get { return _animations; }
            set
            {
                _animations = value;
                _activeIndex = 0;
            }
        }

        private Sprite[] _frames;
        
        public Sprite[] Frames { get{ return _frames;} set{ _frames = value; } }

        public SpriteRenderer Renderer {get; set;}

        private int _activeIndex = 0;

        public void LoadFrames(string path, Vector2 size, SpriteImportOptions spriteImportOptions = null)
        {
            if(null == spriteImportOptions) spriteImportOptions = new SpriteImportOptions();

            Main.IterTormenti.FileHandler.LoadDataAsFixedSpritesheet(path, size, out _frames, spriteImportOptions);
        }

        public int ActiveIndex 
        {
            get { return _activeIndex; }
            set
            { 
                if (_animations.Length == 0) _activeIndex = 0;
                else if (value < 0) _activeIndex = 0;
                else if (value >= _animations.Length) _activeIndex = _animations.Length - 1;
                else _activeIndex = value;
                Main.IterTormenti.Log("ActiveIndex: '" + ActiveIndex + "'");
            }                                          
        }

        public string ActiveName
        {
            get { return _animations.Length == 0? "" : _animations[_activeIndex].Name; }            
            set 
            {
                _activeIndex = GetAnimationIndex(value);
                Main.IterTormenti.Log("ActiveName: '" + (null==Active? "NULL" : Active.Name) + "'");
            }
        }

        public Animation GetByName(string name)
        {
            foreach(Animation anim in _animations)
            {
                if(anim.Name.Equals(name)) return anim;
            }

            return null;
        }

        public Animation GetByIndex(int index)
        {
            if(index < 0 || index >= _animations.Length)
                return null;

            return _animations[index];
        }

        public Animation Active { get{ return _animations.Length == 0? null: _animations[_activeIndex];} }

        private bool _playing = false;
        private bool _reverse = false;

        private float _timeSinceLastUpdate = 0f;

        public void Play()
        {
            _playing = true;
            _timeSinceLastUpdate = 0.0f;
            Main.IterTormenti.Log("Anim: '" + Active.Name + "' Play");
        }

        public void Stop()
        {
            _playing = false;
        }

        public void Reverse()
        {
            _reverse |= true;
        }

        public int GetAnimationIndex(string name)
        {
            for(int idx = 0; idx < _animations.Length; idx++)
            {
                if(_animations[idx].Name.Equals(name))
                {
                    return idx;
                }
            }

            return 0;
        }

        public string GetAnimationName(int index)
        {
            if(_animations.Length == 0 || index >= _animations.Length) return "";

            return _animations[index].Name;
        }

        private string _go = "";
        private bool _cut = false;        
        private bool _now = false;


        public void CutTo(string target, bool now = false)
        {
            _go = target;
            _cut = true;
            _now = now;
        }

        public void Cut(bool now = false)
        {
            CutTo(Active.Next, now);
        }

        public void GoTo(string target, bool now = false)
        {
            _go = target;
            _cut = false;
            _now = now;
        }

        public void Go(bool now = false)
        {
            GoTo(Active.Next, now);
        }

        public void Event(string name)
        {
            // TODO: Use animation event to change animations
        }



        void Awake()
        {
            _timeSinceLastUpdate = 0.0f;
        }

        IEnumerator Start()
        {
            yield return new WaitForSeconds(0.0f);            
        }

        public void Update()
        {
            if(!_playing) return;
            
            Animation anim = Active;

            if(null == anim) return;

            _timeSinceLastUpdate += Time.deltaTime;

            float delay = anim.CurrentFrame.Delay >= 0.0f? anim.CurrentFrame.Delay : anim.Delay;

            if (_timeSinceLastUpdate < delay) return;

            _timeSinceLastUpdate = 0.0f;

            //TODO: Cut, Go, FFW, RWD

            if(!anim.Next.Equals("") && anim.IsKeyFrame(_reverse))
            {
                ActiveName = anim.Next;
                anim.Index = 0;
            }
            else
            {
                if(!_reverse) anim.NextFrame();
                else anim.PrevFrame();
            }

            if(null == anim.CurrentFrame) return;

            Renderer.sprite = _frames[anim.CurrentFrame.Index];
        }

        override public string ToString()
        {
            string text = "{ Frames: " + Frames.Length + ", Animations: [ ";
            foreach(Animation anim in Animations)
            {
                text += (null == anim ? "{NULL}" : anim.ToString()) + ", ";
            }
            text += " ], Active: '" + (Active == null? "NULL" : Active.Name) + "' }";
            return text;
        }

    }

    [Serializable]
    class Frame
    {
        public Frame(int index, float delay = -1.0f)
        {
            Index = index;
            Delay = delay;
        }

        public int Index {get; private set;}
        public float Delay {get; private set;}

        override public string ToString()
        {
            return Delay>=0.0f ? "{" + Index + ", " + Delay + "s}" : "{" + Index + "}";
        }
    }

    [Serializable]
    class Animation
    {
        public Animation(string name)
        {
            Name = name;
            Frames = new Frame[0];
            Index = 0;
            Loop = false;
            Next = "";
            Delay = 1.0f;
        }

        public Animation(Animation original)
        {
            Name = original.Name;
            Frames = new Frame[original.Frames.Length];
            Array.Copy( original.Frames, Frames, original.Frames.Length );
            Index = original.Index;
            Loop = original.Loop;
            Next = original.Next;
            Delay = original.Delay;
        }

        public string Name {get; private set;}

        public Frame[] Frames;// {get; set;}
        
        public float Delay {get; set;}

        public bool Loop {get; set;}

        public string Next {get; set;}

        public AnimationEvent[] Events {get; set;}

        private int _index = 0;
        public int Index 
        {
            get { return _index; }
            set
            {
                if(Frames.Length == 0) _index = 0;
                else if(value < 0)  _index = 0;
                else if(value >= Frames.Length) _index = Frames.Length - 1;
                else _index = value;
            }
        }

        public void NextFrame()
        {
            if(Frames.Length == 0)
            {
                _index = 0;
                return;
            }

            _index++;

            if(_index >= Frames.Length) _index = Loop? 0 : Frames.Length - 1;
        }

        public void PrevFrame()
        {
            if(Frames.Length == 0)
            {
                _index = 0;
                return;
            }
            
            _index--;

            if(_index < 0) _index = Loop? Frames.Length - 1 : 0;
        }

        public Frame CurrentFrame
        {
            get
            {
                if(Frames.Length == 0) return null;
                else return Frames[_index];
            }
        }

        public bool IsKeyFrame(bool reversed)
        {
            // TODO: Event functionality
            // By default, the last frame is the only keyframe (first frame if reversed)

            if(reversed) return _index == 0;
            else return _index == (Frames.Length - 1);
        }

        override public string ToString()
        {
            string text = "{ " + Name + ", " + Delay + "s, " + (Loop? "Looping" : "Once") + ", [ ";
            foreach(Frame frame in Frames)
            {
                text += frame.ToString() + ", ";
            }
            text += " ] }";
            return text;
        }
    }

    class AnimationEvent
    {
        public AnimationEvent(string name)
        {
            this.name = name;
        }

        public string name {get; private set;}

        public int[] keyframes {get; set;}

        public string next {get; set;}
    }

    

}