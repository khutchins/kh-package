using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using static KH.TaskQueue;

namespace KH {
    /// <summary>
    /// A simple implementation of a handler for the TaskQueue.
    /// Each task handler should register itself with the manager,
    /// which will automatically process all tasks once one is
    /// added to the queue.
    /// 
    /// Designed to be a per scene singleton. It'll clear the queue
    /// on awake and register itself as the singleton (hence the 
    /// earlier execution order), so it should be safe to register
    /// handlers on the instance in their own awake, as long as they
    /// don't also have an earlier execution order. If it comes
    /// across a task that it cannot handle, it will log a warning
    /// and drop it.
    /// </summary>
    [DefaultExecutionOrder(-1)]
    public class TaskQueueManager : MonoBehaviour {
        [SerializeField] TaskQueue Queue;

        private readonly Dictionary<System.Type, System.Func<ITask, IEnumerator>> _taskHandlers = 
            new Dictionary<System.Type, System.Func<ITask, IEnumerator>>();

        public static TaskQueueManager INSTANCE;

        private void Awake() {
            Queue.Clear();
            INSTANCE = this;
        }

        private void OnEnable() {
            Queue.OnQueueNoLongerEmpty += QueueNotEmpty;
            if (!Queue.Empty) QueueNotEmpty(Queue);
        }

        private void OnDisable() {
            Queue.OnQueueNoLongerEmpty -= QueueNotEmpty;
        }

        public void AddHandler(System.Type taskType, System.Func<ITask, IEnumerator> handler) {
            if (_taskHandlers.ContainsKey(taskType)) {
                Debug.LogWarning($"Replacing handler for {taskType}.");
            }
            _taskHandlers[taskType] = handler;
        }

        public void RemoveHandler(System.Type taskType, System.Func<ITask, IEnumerator> handler) {
            if (_taskHandlers.ContainsKey(taskType) || _taskHandlers[taskType] != handler) {
                Debug.LogWarning($"Removing handler for {taskType} that wasn't registered or was replaced.");
                return;
            }
            _taskHandlers.Remove(taskType);
        }

        void QueueNotEmpty(TaskQueue queue) {
            if (!queue.TaskInProgress) StartCoroutine(ClearAllTasks(queue));
        }

        IEnumerator TaskHandler(ITask task) {
            if (_taskHandlers.ContainsKey(task.GetType())) {
                yield return _taskHandlers[task.GetType()](task);
            } else {
                Debug.LogWarning($"No task handler for {task}.");
                yield return null;
            }
        }

        IEnumerator ClearAllTasks(TaskQueue queue) {
            yield return queue.HandleAllTasks(TaskHandler);
        }
    }
}