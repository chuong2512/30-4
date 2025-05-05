namespace HoleBox
{
    using System;
    using System.Collections.Generic;
    using System.Threading.Tasks;
    using UnityEngine;

    public class MovementThread : Singleton<MovementThread>
    {
        private ActionQueue _actionQueues = new ActionQueue();


        public void EnqueueAction(Func<Task> action) { _actionQueues.EnqueueAction(action); }
    }
}