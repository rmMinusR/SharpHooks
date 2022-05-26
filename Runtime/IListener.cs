using System;
using UnityEngine;

namespace rmMinusR.EventBus
{
    /// <summary>
    /// Empty marker for objects that interact with Queries and Messages
    /// </summary>
    public interface IListener
    {
        public string GetDebugName();
    }

}