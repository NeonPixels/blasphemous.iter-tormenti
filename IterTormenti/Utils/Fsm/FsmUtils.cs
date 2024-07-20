using HutongGames.PlayMaker;

namespace IterTormenti.Utils.Fsm
{
    /// <summary>
    /// Utilities to work with FSM.
    /// Based on code from https://github.com/PrashantMohta/Satchel,
    /// adapted to Unity 2017.4 and trimmed for use in this mod.
    /// </summary>
    public static partial class FsmUtils
    {
        public static FsmState AddState(this PlayMakerFSM fsm, FsmState state)
        {   
            var currStates = fsm.Fsm.States;
            var states = new FsmState[currStates.Length+1];
            var i = 0;
            for(; i < currStates.Length; i++)
            {
                states[i] = currStates[i];
            }
            states[i] = state;
            fsm.Fsm.States = states;
            return states[i];
        }

        public static FsmState GetState(this PlayMakerFSM fsm, string stateName)
        {
            return fsm.Fsm.GetState(stateName);
        }

        public static void AddTransition(this FsmState state, string onEventName, string toStateName)
        {
            var currTransitions = state.Transitions;
            var transitions = new FsmTransition[currTransitions.Length + 1];

            var newTransiton = new FsmTransition
            {
                ToState = toStateName,
                FsmEvent = FsmEvent.GetFsmEvent(onEventName)
            };

            var i = 0;
            for(; i < currTransitions.Length; i++)
            {
                transitions[i] = currTransitions[i];
            }
            transitions[i] = newTransiton;
            state.Transitions = transitions;
        }

        public static FsmTransition GetTransition(this FsmState state, string onEventName)
        {
            var transitions = state.Transitions;
            for(int i = 0; i < transitions.Length; i++)
            {
                if(transitions[i].EventName == onEventName)
                {
                    return transitions[i];
                }
            }
            return null;
        }
        
        public static void ChangeTransition(this FsmState state, string onEventName, string toStateName)
        {
            var transition = state.GetTransition(onEventName);
            transition.ToState = toStateName;
        }
        
        public static FsmTransition GetGlobalTransition(this PlayMakerFSM fsm, string onGlobalEventName)
        {
            var transitions = fsm.FsmGlobalTransitions;
            for(int i = 0; i < transitions.Length; i++)
            {
                if(transitions[i].EventName == onGlobalEventName)
                {
                    return transitions[i];
                }
            }
            return null;
        }

        public static void ChangeGlobalTransition(this PlayMakerFSM fsm, string onGlobalEventName, string toStateName)
        {
            fsm.GetGlobalTransition(onGlobalEventName).ToState = toStateName;
        }
    
        public static void InsertAction(this FsmState state, FsmStateAction action, int index)
        {
            var currActions = state.Actions;
            var actions = new FsmStateAction[currActions.Length + 1];
            
            for(int i=0 , oldPos = 0;i<actions.Length;i++ , oldPos++)
            {
                if(i == index)
                {
                    actions[i] = action;
                    i++;
                }

                if(oldPos < currActions.Length)
                {
                    actions[i] = currActions[oldPos];
                }
            }
            state.Actions = actions;
            action.Init(state);
        }
        
        public static void AddAction(this FsmState state, FsmStateAction action)
        {
            state.InsertAction(action,state.Actions.Length);
        }
    }
}
