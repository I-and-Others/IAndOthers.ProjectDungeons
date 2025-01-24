using System;
using System.Collections.Generic;
using Scripts.Core;

public class EventManager : Singleton<EventManager>
{
    private Dictionary<string, EventHandler> _eventHandlers = new Dictionary<string, EventHandler>();
    private Dictionary<string, EventHandler<EventArgs>> _genericEventHandlers = new Dictionary<string, EventHandler<EventArgs>>();

    public void AddListener(string eventName, EventHandler listener)
    {
        if (!_eventHandlers.ContainsKey(eventName))
        {
            _eventHandlers[eventName] = null;
        }

        _eventHandlers[eventName] += listener;
    }

    public void AddListener<T>(string eventName, EventHandler<T> listener) where T : EventArgs
    {
        if (!_genericEventHandlers.ContainsKey(eventName))
        {
            _genericEventHandlers[eventName] = null;
        }

        EventHandler<EventArgs> wrapper = (sender, args) => listener(sender, (T)args);
        _genericEventHandlers[eventName] += wrapper;
    }

    public void RemoveListener(string eventName, EventHandler listener)
    {
        if (_eventHandlers.ContainsKey(eventName))
        {
            _eventHandlers[eventName] -= listener;
        }
    }

    public void RemoveListener<T>(string eventName, EventHandler<T> listener) where T : EventArgs
    {
        if (_genericEventHandlers.ContainsKey(eventName))
        {
            EventHandler<EventArgs> wrapper = (sender, args) => listener(sender, (T)args);
            _genericEventHandlers[eventName] -= wrapper;
        }
    }

    public void Trigger(string eventName, object sender, EventArgs args)
    {
        if (_eventHandlers.ContainsKey(eventName))
        {
            _eventHandlers[eventName]?.Invoke(sender, args);
        }
    }

    public void Trigger<T>(string eventName, object sender, T args) where T : EventArgs
    {
        if (_genericEventHandlers.ContainsKey(eventName))
        {
            _genericEventHandlers[eventName]?.Invoke(sender, args);
        }
    }

    void OnDestroy()
    {
        _eventHandlers.Clear();
        _genericEventHandlers.Clear();
    }
}