using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.Events;
using Random = UnityEngine.Random;

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