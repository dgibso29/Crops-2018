using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Crops.Events
{
    public class TimeEventArgs
    {
        public TimeEventType type;

        public TimeEventArgs(TimeEventType timeEventType)
        {
            type = timeEventType;
        }

    }
}
