using System.Collections.Generic;
using System.Linq;

namespace IterTormenti.Utils.Sprites.Animations.Fsm
{
    // TODO: Documentation
    public class State
    {
        public State(string name, string animationName = "")
        {
            Name = name;            
            AnimationName = animationName;
            _transitions = new Dictionary<string, Transition>();
        }

        public State(State source)
        {
            Clone(source);
        }

        public void Clone(State source)
        {
            Name = source.Name;
            AnimationName = source.AnimationName;
            _transitions = new Dictionary<string, Transition>();
            
            foreach(KeyValuePair<string, Transition> kv in source._transitions)
            {
                _transitions.Add(kv.Key, new Transition(kv.Value));
            }            
        }

        public string Name { get; private set;}
        
        public string AnimationName { get; set; }


        private Dictionary<string, Transition> _transitions; 
        
        public Transition[] Transitions { get{ return _transitions.Values.ToArray<Transition>(); } }

        public Transition AddTransition(Transition value)
        {
            if(null == value)
            {
                Main.IterTormenti.LogError("State::AddTransition: ERROR: Invalid transition provided");
                return null;
            }

            if(_transitions.Keys.Contains(value.Event))
            {
                Main.IterTormenti.LogError($"State::AddTransition: ERROR: Transition already defined: {value.Event}");
                return null;
            }
            
            _transitions.Add(value.Event, value);

            return GetTransition(value.Event);
        }

        public Transition RemoveTransition(string value)
        {
            if(null == value)
            {
                Main.IterTormenti.LogError("State::RemoveTransition: ERROR: Invalid transition value provided");
                return null;
            }

            Transition retVal = GetTransition(value);

            if(!_transitions.Remove(value)) return null;

            return retVal;
        }

        public Transition GetTransition(string value)
        {
            if(null == value)
            {
                Main.IterTormenti.LogError("State::GetTransition: ERROR: Invalid transition value provided");
                return null;
            }

            Transition retVal = null;
            _transitions.TryGetValue(value, out retVal);

            return retVal;
        }
    }
}