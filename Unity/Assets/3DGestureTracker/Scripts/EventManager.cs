using UnityEngine;
using System;
using System.Collections;
using System.Collections.Generic;

public class EventManager
{

    private Dictionary<string, Action<string>> eventDictionary;

    private static EventManager eventManager;

    private EventManager()
    {
        eventDictionary = new Dictionary<string, Action<string>>();
    }

    public static EventManager instance
    {
        get
        {
            if (eventManager == null)
            {
                eventManager = new EventManager();
            }

            return eventManager;
        }
    }

    public static void StartListening(string eventName, Action<string> listener)
    {
        Action<string> thisEvent = null;
        if (instance.eventDictionary.TryGetValue(eventName, out thisEvent))
        {
            thisEvent += listener;
            //thisEvent.AddListener(listener);

        }
        else
        {
            thisEvent = listener;
            instance.eventDictionary.Add(eventName, thisEvent);
        }
    }

    public static void StopListening(string eventName, Action<string> listener)
    {
        if (eventManager == null) return;
        Action<string> thisEvent = null;
        if (instance.eventDictionary.TryGetValue(eventName, out thisEvent))
        {
            thisEvent -= listener;
        }
    }

    public static void TriggerEvent(string eventName, string args = "")
    {
        Action<string> thisEvent = null;
        if (instance.eventDictionary.TryGetValue(eventName, out thisEvent))
        {
            thisEvent.Invoke(args);
        }
    }

    public static void TriggerGestureEvent(string eventName, string gesture = "", double confidence = 0)
    {
        Action<string> thisEvent = null;
        if (instance.eventDictionary.TryGetValue(eventName, out thisEvent))
        {
            thisEvent.Invoke(gesture);
        }
    }
}