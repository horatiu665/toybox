namespace ToyBox
{
    using System;
    using System.Collections;
    using System.Collections.Generic;
    using System.Linq;
    using UnityEngine;
    using UnityEngine.Events;
    using Random = UnityEngine.Random;

    /// <summary>
    /// Super useful quick n dirty way to do stuff in the scene by toggling objects.
    /// </summary>
    public class EventOnEnable : MonoBehaviour
    {
        public UnityEvent theEvent = new UnityEvent();

        private void OnEnable()
        {
            DoItNow();
        }

        [DebugButton]
        public void DoItNow()
        {
            theEvent.Invoke();
        }

    }
}