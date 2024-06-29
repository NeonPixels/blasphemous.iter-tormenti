using System;
using System.Collections.Generic;
using System.Linq;


namespace IterTormenti.Utils.Sprites.Animations.Fsm
{

    public class AnimationFsm
    {
        public abstract class STATE
        {
            public const string START = "START";
            public const string END   = "END";
        }

        public abstract class TRANSITION
        {
            public const string ON_ANIMATION_END = "OnAnimationEnd";
        }

        public abstract class EVENT
        {
            public const string END_REACHED = "EndStateReached";
            public const string STATE_CHANGED = "StateChanged";
        }


        public AnimationFsm(string name)
        {
            Name = name;
            _global = new State("");
            _states = new Dictionary<string, State>{ {STATE.START, new(STATE.START)}, {STATE.END, new(STATE.END)} };
            _activeStateName = STATE.START;
        }

        public AnimationFsm(AnimationFsm source)
        {
            Clone(source);            
        }

        public void Clone(AnimationFsm source)
        {
            Name = source.Name + "_copy";
            _global = new State(source._global);

            _states = new Dictionary<string, State>();            
            foreach(KeyValuePair<string, State> kv in source._states)
            {
                _states.Add(kv.Key, new State(kv.Value));
            }

            _activeStateName = source._activeStateName;
        }

        public string Name { get; set; }

        private string _activeStateName;

        private Dictionary<string, State> _states;

        public State[] States { get{ return _states.Values.ToArray<State>(); } }

        public State GetState(string value)
        {
            if(null == value)
            {
                Main.IterTormenti.LogError("Fsm::GetState: ERROR: Invalid state value provided");
                return null;
            }

            State retVal = null;
            _states.TryGetValue(value, out retVal);            
            return retVal;
        }

        public State ActiveState
        {
            get { return GetState(_activeStateName); }            
        }

        public State AddState(State value)
        {
            if(null == value)
            {
                Main.IterTormenti.LogError("Fsm::AddState: ERROR: Attempt to add invalid state");
                return null;
            }

            if(_states.Keys.Contains(value.Name))
            {
                Main.IterTormenti.LogError("Fsm::AddState: ERROR: Attempt to add existing state: " + value.Name);
                return null;                
            }

            _states.Add(value.Name, value);

            return GetState(value.Name);
        }

        public State RemoveState(string value)
        {
            if(null == value)
            {
                Main.IterTormenti.LogError("Fsm::RemoveState: ERROR: Attempt to remove invalid state");
                return null;
            }

            State retVal = GetState(value);

            if(!_states.Remove(value)) return null;

            return retVal;
        }

        public State SetActiveState(string value)
        {
            if(null == value)
            {
                Main.IterTormenti.LogError("Fsm::SetActiveState: ERROR: Attempt to set invalid state");
                return null;
            }

            if(!_states.Keys.Contains(value))
            {
                Main.IterTormenti.LogError("Fsm::SetActiveState: ERROR: Attempt to set an unknown state: " + value);
                return null;                
            }

            _activeStateName = value;
            
            OnStateChanged(ActiveState);

            return ActiveState;
        }


        private State _global;

        public Transition[] GlobalTransitions { get{ return _global.Transitions; } }


        public Transition AddGlobalTransition(Transition value)
        {
            return _global.AddTransition(value);
        }

        public Transition RemoveGlobalTransition(string value)
        {
            return _global.RemoveTransition(value);
        }

        public Transition GetGlobalTransition(string value)
        {
            return _global.GetTransition(value);
        }

        
        // -- Event Notification --


        public event EventHandler<FsmEventArgs> FsmEvent;

        protected virtual void OnEndStateReached()
        {
            FsmEvent?.Invoke(this, new FsmEventArgs(EVENT.END_REACHED));
        }

        protected virtual void OnStateChanged(State newState)
        {
            if(newState.Name.Equals(STATE.END))
            {
                OnEndStateReached();
                return;
            }

            FsmEvent?.Invoke(this, new FsmEventArgs(EVENT.STATE_CHANGED){ AnimationIndex = newState.AnimationIndex} );
        }

        // -- Event Handling --

        public void OnAnimationEvent(object source, AnimationEventArgs eventArgs)
        {
            foreach(Transition transition in GlobalTransitions)
            {
                if(eventArgs.Name.Equals(transition.Event))
                {
                    SetActiveState(transition.TargetState);
                    return;
                }
            }

            foreach(Transition transition in ActiveState.Transitions)
            {
                if(eventArgs.Name.Equals(transition.Event))
                {
                    SetActiveState(transition.TargetState);
                    return;
                }
            }
        }

    }

    public abstract class Testing
    {
        public static void Test()
        {

            // TODO: Add method to SpriteAnimator that triggers events?
            // TODO: Really use events? Why not use a simple method call?

            AnimationFsm fsm = new AnimationFsm("Test");
            {
                // START
                {
                    Transition transition = new Transition("BossDefeated", "EsdrasNonLethalDefeat");
                    fsm.GetState(AnimationFsm.STATE.START).AddTransition(transition);
                }
                
                State esdrasNonLethalDefeat = new State("EsdrasNonLethalDefeat");
                {
                    Transition onEnded = new Transition(AnimationFsm.TRANSITION.ON_ANIMATION_END, "EsdrasDefeated");
                    esdrasNonLethalDefeat.AddTransition(onEnded);
                }
                fsm.AddState(esdrasNonLethalDefeat);

                State esdrasDefeated = new State("EsdrasDefeated");
                {
                    Transition transition = new Transition("EsdrasRecover", "EsdrasWeaponPickup");
                    esdrasDefeated.AddTransition(transition);
                }
                fsm.AddState(esdrasDefeated);

                State esdrasWeaponPickup = new State("EsdrasWeaponPickup");
                {
                    Transition onEnded = new Transition(AnimationFsm.TRANSITION.ON_ANIMATION_END, AnimationFsm.STATE.END);
                    esdrasWeaponPickup.AddTransition(onEnded);
                }
                fsm.AddState(esdrasWeaponPickup);
            }
        }
    }
}