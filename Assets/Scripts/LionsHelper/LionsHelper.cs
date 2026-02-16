using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
using UnityEngine;
using UnityEngine.Windows;
using Random = UnityEngine.Random;

public class LionsHelper
{
    public class HelperFunctions
    {
        /// <summary>
        /// Selects a random element from <paramref name="values"/> using the corresponding
        /// relative <paramref name="weights"/>. Each weight represents the relative chance
        /// of the element at the same index being chosen (i.e. weights are treated as non-normalized
        /// probabilities and only their relative magnitudes matter).
        /// </summary>
        /// <typeparam name="T">Type of the values in the pool.</typeparam>
        /// <param name="values">Array of candidate values. Must be the same length as <paramref name="weights"/> and not <c>null</c>.</param>
        /// <param name="weights">
        /// Array of non-negative weights corresponding to <paramref name="values"/>. The probability
        /// of selecting <c>values[i]</c> is <c>weights[i] / Sum(weights)</c> (assuming at least one weight &gt; 0).
        /// </param>
        /// <returns>
        /// One element from <paramref name="values"/> chosen according to the relative weights.
        /// If floating point roundoff causes the loop not to return, the method falls back and returns the last element.
        /// </returns>
        public static T GetWeightedRandom<T>(T[] values, float[] weights = null)
        {
            if (values == null)
                throw new ArgumentException("Values or weights are null.");

            if (weights == null)
            {
                weights = new float[values.Length];
                for (int i = 0; i < weights.Length; i++)
                {
                    weights[i] = 1;
                }
            }

            if (values.Length != weights.Length)
                throw new ArgumentException("Values and weights must have the same length.");


            float totalWeight = weights.Sum();
            float roll = Random.value * totalWeight;

            for (int i = 0; i < values.Length; i++)
            {
                if (roll < weights[i])
                    return values[i];

                roll -= weights[i];
            }

            // Fallback, shouldn't happen unless there's floating-point error
            return values[values.Length - 1];
        }
        /// <summary>
        /// Loads a random object from the resources folder
        /// </summary>
        /// <typeparam name="T">object type</typeparam>
        /// <param name="resourcesPath">path INSIDE resources</param>
        /// <returns></returns>
        public static T ResourcesLoadRandom<T>(string resourcesPath) where T : UnityEngine.Object
        {
            var prefabs = Resources.LoadAll<T>(resourcesPath);
            if (prefabs == null || prefabs.Length == 0) return null;
            int index = Random.Range(0, prefabs.Length);
            return prefabs[index];
        }


        /// <summary>
        /// Checks whether the provided <paramref name="animator"/> contains a parameter with name
        /// <paramref name="paramName"/> that matches the expected type for generic type <typeparamref name="T"/>.
        /// </summary>
        /// <typeparam name="T">The expected managed type for the parameter (float/int/bool).</typeparam>
        /// <param name="animator">The <see cref="Animator"/> instance to examine. If <c>null</c>, returns <c>false</c>.</param>
        /// <param name="paramName">The name of the parameter to look for.</param>
        /// <returns>
        /// <c>true</c> when a parameter exists with the matching name and AnimatorControllerParameterType for <typeparamref name="T"/>;
        /// otherwise <c>false</c>.</returns>
        /// <remarks>
        /// Mapping between <typeparamref name="T"/> and <see cref="AnimatorControllerParameterType"/>:
        /// <list type="bullet">
        ///   <item><description><c>float/double</c> -> <see cref="AnimatorControllerParameterType.Float"/></description></item>
        ///   <item><description><c>int/short/long</c> -> <see cref="AnimatorControllerParameterType.Int"/></description></item>
        ///   <item><description><c>bool</c> -> <see cref="AnimatorControllerParameterType.Bool"/></description></item>
        /// </list>
        /// If you need to check for a Trigger parameter, write a small wrapper that checks for <see cref="AnimatorControllerParameterType.Trigger"/>.
        /// </remarks>
        /// <example>
        /// bool hasFloat = AnimatorUtils.AnimatorHasParameter<float>(animator, "UpperBody_Speed");
        /// bool hasTrigger = animator.parameters.Any(p => p.name == "Fire" && p.type == AnimatorControllerParameterType.Trigger);
        /// </example>
        public static bool AnimatorHasParameter<T>(Animator animator, string paramName)
        {
            if (animator == null) return false;

            AnimatorControllerParameterType expectedType;

            if (typeof(T) == typeof(float) || typeof(T) == typeof(double))
                expectedType = AnimatorControllerParameterType.Float;
            else if (typeof(T) == typeof(int) || typeof(T) == typeof(short) || typeof(T) == typeof(long))
                expectedType = AnimatorControllerParameterType.Int;
            else if (typeof(T) == typeof(bool))
                expectedType = AnimatorControllerParameterType.Bool;
            else
            {
                // If you want to treat string as triggers, you could special-case here.
                Debug.LogError($"{animator}: unsupported generic type {typeof(T).Name}.");
                return false;
            }

            foreach (var p in animator.parameters)
            {
                if (p.name == paramName && p.type == expectedType) return true;
            }
            return false;
        }

        /// <summary>
        /// Turns a general Vector3 to Vector3Int
        /// </summary>
        /// <param name="vector3">The vector to int</param>
        /// <returns>the int-ed vector</returns>
        public Vector3Int IntVec3(Vector3 vector3)
        {
            return new Vector3Int((int)vector3.x, (int)vector3.y, (int)vector3.z);
        }

        /// <summary>
        /// Lerps a vector 3. Same as mathf.lerp
        /// </summary>
        /// <param name="VecStart">Start vector</param>
        /// <param name="VecTarget">Target vector</param>
        /// <param name="t">how much to lerp</param>
        /// <returns>The lerped vector</returns>
        public Vector3 LerpVec3(Vector3 VecStart, Vector3 VecTarget, float t)
        {
            return (new Vector3(Mathf.Lerp(VecStart.x, VecTarget.x, t), Mathf.Lerp(VecStart.y, VecTarget.y, t), Mathf.Lerp(VecStart.z, VecTarget.z, t)));
        }

        /// <summary>
        /// Plays a random source from <paramref name="clips"/> at <paramref name="audioSource"/>
        /// </summary>
        /// <param name="audioSource">The audio source</param>
        /// <param name="clips">All available clips</param>
        /// <param name="volume">Volume</param>
        public static void PlayRandomSound(AudioSource audioSource, AudioClip[] clips, float volume = 1)
        {
            if (audioSource == null || clips.Length == 0)
            {
                Debug.LogError("Played random sound with null audio source or no clips");
                return;
            }
            audioSource.PlayOneShot(clips[UnityEngine.Random.Range(0, clips.Length)], volume);
        }

        public static void ResetAllTriggers(Animator animator)
        {
            foreach (AnimatorControllerParameter param in animator.parameters)
            {
                if (param.type == AnimatorControllerParameterType.Trigger)
                    animator.ResetTrigger(param.name);
            }
        }

        /// <summary>
        /// Immediately set <paramref name="subject"/> to face the same yaw (Y rotation)
        /// as <paramref name="reference"/>. Keeps X/Z rotation at 0.
        /// </summary>
        public static void MatchYawInstant(Transform subject, Transform reference)
        {
            if (subject == null || reference == null) return;
            Vector3 refEuler = reference.rotation.eulerAngles;
            subject.rotation = Quaternion.Euler(0f, refEuler.y, 0f);
        }

        /// <summary>
        /// Deeps copy a list
        /// </summary>
        /// <typeparam name="T">Type of list</typeparam>
        /// <param name="source">Original list</param>
        /// <param name="cloneFunc">Functions for deep copy (e.g, copy constructor)</param>
        /// <returns>A new deep copied list</returns>
        public static List<T> DeepCopy<T>(List<T> source, Func<T, T> cloneFunc)
        {
            if (source == null)
                return null;

            return source.Select(item => cloneFunc(item)).ToList();
        }
    }

    [Serializable]
    public class Timer
    {
        public float initialDuration { get; private set; }
        public float TimeRemaining { get; set; }

        /// <summary>
        /// Constructor for timer class.
        /// Initializes at full length
        /// </summary>
        /// <param name="DefaultValue">Timer length</param>
        public Timer(float DefaultValue)
        {
            initialDuration = DefaultValue;
            TimeRemaining = DefaultValue;
        }
        /// <summary>
        /// Constructor for timer class.
        /// </summary>
        /// <param name="DefaultValue">Timer total length</param>
        /// <param name="TimerCurrentValue">Current length of timer</param>
        public Timer(float DefaultValue, float TimerCurrentValue)
        {
            initialDuration = DefaultValue;
            TimeRemaining = TimerCurrentValue;
        }
        /// <summary>
        /// Resets timer
        /// </summary>
        /// <param name="offset">+-to the timer length</param>
        public void Reset(float offset = 0)
        {
            TimeRemaining = initialDuration + offset;
        }

        public bool Finished(float FinishValue = 0)
        {
            return (TimeRemaining <= FinishValue);
        }
        /// <summary>
        /// "Ticks" the timer by time.delta time
        /// </summary>
        /// <param name="multiplier">multipler for tick</param>
        public void Update(float multiplier = 1)
        {
            TimeRemaining -= Time.deltaTime * multiplier;
        }
        /// <summary>
        /// Is the timer full?
        /// </summary>
        /// <param name="percentage">Fullness percentage which is ok</param>
        /// <returns></returns>
        public bool Full(int percentage = 100)
        {
            return TimeRemaining >= percentage / 100f * initialDuration;
        }
    }

    public abstract class StateMachine<Tstate> where Tstate : Enum
    {
        // Add states to this enum

        public Tstate CurrentState { get; private set; }
        public Action ActionToDo { get; set; }

        private bool _started = false;
        protected List<EventRow> EventRows; //can be turned to dict for more efficieny
        protected Queue<Event> EventQueue;

        public abstract class Event { }


        public abstract class Action
        {
            public abstract void ActionTransition(StateMachine<Tstate> iStateMachine, Event iEvent);

            public abstract void ActionUpdate(StateMachine<Tstate> iStateMachine);
        }

        public class Guard
        {
            public virtual bool IsValid(StateMachine<Tstate> iStateMachine, Event iEvent)
            {
                return true;
            }
        }

        public class EventRow
        {
            public Tstate State;
            public Type Event;
            public Tstate NextState;
            public Action Action;
            public Guard Guard;
            public EventRow(Tstate iState, Type iEvent, Tstate iNextState, Action iAction)
            {
                State = iState;
                Event = iEvent;
                NextState = iNextState;
                Action = iAction;
                Guard = new Guard();
            }

            public EventRow(Tstate iState, Type iEvent, Tstate iNextState, Action iAction, Guard iGuard)
            {
                State = iState;
                Event = iEvent;
                NextState = iNextState;
                Action = iAction;
                Guard = iGuard;
            }
        }

        public StateMachine()
        {
            EventRows = new List<EventRow>();
            EventQueue = new Queue<Event>();
            RegisterRows();
        }

        public void StartMachine(Tstate iStartState)
        {
            _started = true;
            CurrentState = iStartState;
        }

        public bool HandleEvent(Event iEvent)
        {
            bool aHandled = false;
            if (!_started)
            {
                Debug.LogError("Using state machine that was not started");
                return false;
            }

            foreach (EventRow aRow in EventRows)
            {
                if (CurrentState.Equals(aRow.State) && aRow.Event == iEvent.GetType() && aRow.Guard.IsValid(this, iEvent))
                {
                    //Debug.Log("State:" + mCurrentState.ToString() + " Event:" + iEvent.ToString() + " To State:" + aRow.mNextState.ToString());
                    // set the state before handling the event since the state can change there
                    aRow.Action.ActionTransition(this, iEvent);
                    CurrentState = aRow.NextState;
                    ActionToDo = aRow.Action;
                    aHandled = true;
                    break;
                }
            }
            if (EventQueue.Count != 0)
            {
                Event aEvent = EventQueue.Dequeue();
                HandleEvent(aEvent);
            }

            return aHandled;
        }

        public void QueueEvent(Event iEvent)
        {
            EventQueue.Enqueue(iEvent);
        }

        public abstract void RegisterRows();
    }
}


