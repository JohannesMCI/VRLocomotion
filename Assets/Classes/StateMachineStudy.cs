using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class StateMachineStudy
{
    public enum ProcessState
    {
        Walk,
        Orientation,
        ShowLM,
        Maze,
        PointLM,
        Empty,
        Exit
    }

    public enum Command
    {
        Init,
        Begin,
        SeenLM,
        TouchedLM,
        PointedTowardsLM,
        Exit
    }

    public class Process
    {
        public class StateTransition
        {
            internal readonly ProcessState CurrentState;
            internal readonly Command Command;

            public StateTransition(ProcessState currentState, Command command)
            {
                CurrentState = currentState;
                Command = command;
            }

            public override int GetHashCode()
            {
                return 17 + 31 * CurrentState.GetHashCode() + 31 * Command.GetHashCode();
            }

            public override bool Equals(object obj)
            {
                StateTransition other = obj as StateTransition;
                return other != null && this.CurrentState == other.CurrentState && this.Command == other.Command;
            }
        }

        Dictionary<StateTransition, ProcessState> transitions;
        public ProcessState CurrentState { get; private set; }

        public Process()
        {
            CurrentState = ProcessState.Empty;
            transitions = new Dictionary<StateTransition, ProcessState>
            {
                { new StateTransition(ProcessState.Empty, Command.Init), ProcessState.Orientation },
                { new StateTransition(ProcessState.Orientation, Command.Begin), ProcessState.Walk },
                { new StateTransition(ProcessState.Walk, Command.Begin), ProcessState.ShowLM },
                { new StateTransition(ProcessState.ShowLM, Command.SeenLM), ProcessState.Maze },
                { new StateTransition(ProcessState.Maze, Command.TouchedLM), ProcessState.PointLM },
                { new StateTransition(ProcessState.PointLM, Command.PointedTowardsLM), ProcessState.ShowLM },
                 { new StateTransition(ProcessState.PointLM, Command.Exit), ProcessState.Exit }
            };
        }

        private ProcessState GetNext(Command command)
        {
            StateTransition transition = new StateTransition(CurrentState, command);
            ProcessState nextState;
            if (!transitions.TryGetValue(transition, out nextState))
            {
                Debug.Log("Invalid transition: " + CurrentState + " -> " + command);
                return CurrentState;
            }
            return nextState;
        }

        public ProcessState MoveNext(Command command)
        {
            CurrentState = GetNext(command);
            Debug.Log("Current State:" + CurrentState.ToString());
            return CurrentState;
        }
    }
}
