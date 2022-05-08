using System;
using UnityEngine;

namespace Events
{
    /// <summary>
    /// Highest priority will execute first and Lowest will execute last.
    /// </summary>
    public enum Priority
    {
        Highest = 100,
        High    = 200,
        Normal  = 300,
        Low     = 400,
        Lowest  = 500,

        Final = 1000
    }

    /// <summary>
    /// Internal superclass of all events.
    /// 
    /// Variables that listeners should modify should be accompanied by a readonly
    /// 'original' value to keep calculations consistent. However, due to overhead,
    /// they should be used sparingly. They are used to poll for updates such as
    /// calculating modified damage or movement.
    /// 
    /// For events with read only data, they can still be interacted with by being cancelled.
    /// Note that an event will reach all listeners, whether it was cancelled or not.
    /// </summary>
    [Serializable]
    public abstract class Event
    {
        public bool isCancelled;
        [SerializeField] private bool __hasBeenDispatched;

        public bool HasBeenDispatched {
            get => __hasBeenDispatched;
            internal set => __hasBeenDispatched = value;
        }

        protected Event()
        {
            HasBeenDispatched = false;
            isCancelled = false;
        }
    }
}