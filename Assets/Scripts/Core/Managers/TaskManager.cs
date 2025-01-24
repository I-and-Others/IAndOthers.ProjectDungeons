using System;
using System.Collections;
using System.Collections.Generic;
using Scripts.Core;
using UnityEditor;
using UnityEngine;

public class TaskManager : Singleton<TaskManager>
{
    private readonly Dictionary<string, Action> _fireAndForgetTasks = new();
    private readonly Dictionary<string, Action> _delayedTasks = new ();
    private readonly Dictionary<string, Action> _recurringTasks = new ();
    private readonly Dictionary<string, Func<bool>> _continuationTasks = new ();

    // Method to schedule a Fire-and-Forget Job
    public void ScheduleFireAndForgetTask(string taskName, Action taskAction)
    {
        _fireAndForgetTasks[taskName] = taskAction;
        ExecuteFireAndForgetTask(taskName);
    }

    public void ScheduleDelayedTask(string taskName, Action taskAction, float delaySeconds)
    {
        IEnumerator DelayedExecution(float delay)
        {
            yield return new WaitForSeconds(delay);
            taskAction();
            // Remove the task from the delayed tasks dictionary after it has executed
            _delayedTasks.Remove(taskName);
        }

        IEnumeratorWrapper(DelayedExecution(delaySeconds));
        // Add the delayed task to the dictionary when scheduled
        _delayedTasks[taskName] = taskAction;
    }

    public void ScheduleRecurringTask(string taskName, Action taskAction, float intervalSeconds)
    {
        IEnumerator RecurringExecution(float interval)
        {
            while (true)
            {
                yield return new WaitForSeconds(interval);
                taskAction();
            }
        }

        IEnumeratorWrapper(RecurringExecution(intervalSeconds));
        // Add the recurring task to the dictionary when scheduled
        _recurringTasks[taskName] = taskAction;
    }


    // Method to schedule a Continuation
    public void ScheduleContinuationTask(string taskName, Func<bool> taskFunc)
    {
        _continuationTasks[taskName] = taskFunc;
        ExecuteContinuationTask(taskName);
    }

    // Method to execute a Fire-and-Forget Job immediately
    private void ExecuteFireAndForgetTask(string taskName)
    {
        if (_fireAndForgetTasks.TryGetValue(taskName, out var taskAction))
        {
            taskAction?.Invoke();
            _fireAndForgetTasks.Remove(taskName);
        }
    }

    // Method to execute a Continuation Task
    private void ExecuteContinuationTask(string taskName)
    {
        if (_continuationTasks.TryGetValue(taskName, out var taskFunc))
        {
            if (!taskFunc())
            {
                // Task is not completed, re-schedule it for the next frame
                StartCoroutine(ContinuationExecution(taskName));
            }
            else
            {
                // Task is completed, remove it from the dictionary
                _continuationTasks.Remove(taskName);
            }
        }
    }

    // Helper coroutine to execute Continuation tasks on the next frame
    private IEnumerator ContinuationExecution(string taskName)
    {
        yield return null; // Wait for the next frame
        ExecuteContinuationTask(taskName);
    }

    // Helper method to execute an IEnumerator as a coroutine
    private void IEnumeratorWrapper(IEnumerator enumerator)
    {
        StartCoroutine(enumerator);
    }
}