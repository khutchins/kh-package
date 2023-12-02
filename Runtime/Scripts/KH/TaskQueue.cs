using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace KH {
    /// <summary>
    /// Object that handles the processing of tasks in a queue,
    /// allowing coroutines to yield on a specific task being
    /// completed as well as waiting for all tasks.
    /// 
    /// It's designed for a single handler that coordinates
    /// parsing out the tasks to classes that can handle them,
    /// but it should work as long as handlers behave well and
    /// only grab tasks intended for them. TaskQueueManager is
    /// an implementation of the former.
    /// </summary>
    [CreateAssetMenu(menuName = "KH/TaskQueue", fileName = "TaskQueue")]
    public class TaskQueue : ScriptableObject {
        public interface ITask {
        }

        public delegate void TaskAdded(TaskQueue queue);
        public TaskAdded OnTaskAdded;
        public delegate void QueueNoLongerEmpty(TaskQueue queue);
        public QueueNoLongerEmpty OnQueueNoLongerEmpty;

        private readonly Queue<ITask> _queue = new();
        private int _lastIndex = -1;
        private int _lastStartedTask = -1;
        private int _lastFinishedTask = -1;
        private bool _taskInProgress = false;

        public bool TaskInProgress { get => _taskInProgress; }
        public bool Empty { get => _queue.Count == 0; }

        /// <summary>
        /// Enqueues the interaction and waits until the task is 
        /// </summary>
        /// <param name="interact">Task to be enqueued.</param>
        /// <returns>ID for the enqueued task.</returns>
        public int Enqueue(ITask interact) {
            _lastIndex++;
            _queue.Enqueue(interact);
            if (_queue.Count == 1) OnQueueNoLongerEmpty?.Invoke(this);
            OnTaskAdded?.Invoke(this);
            return _lastIndex;
        }

        /// <summary>
        /// Removes all tasks from queue, clearing all the interactions and triggering
        /// an end to all coroutines yielding on methods in this class.
        /// </summary>
        public void Clear() {
            _taskInProgress = false;
            while (!Empty) {
                _queue.Dequeue();
                _lastStartedTask++;
                _lastFinishedTask++;
            }
            ResetTasksIndexes();
        }

        void ResetTasksIndexes() {
            _lastIndex = -1;
            _lastStartedTask = -1;
            _lastFinishedTask = -1;
        }

        /// <summary>
        /// Handles a task by taking in a task handler. Note that this does not
        /// behave well if multiple tasks are handled concurrently (it's subject
        /// to a race condition in task finished coroutines). There should only
        /// be one handler, but if there's more than one, you can gate calling this
        /// method based on the TaskInProgress property.
        /// </summary>
        /// <param name="taskHandler"></param>
        public IEnumerator HandleTask(System.Func<ITask, IEnumerator> taskHandler) {
            if (Empty) yield break;
            _taskInProgress = true;
            _lastStartedTask++;
            yield return taskHandler(_queue.Dequeue());
            _lastFinishedTask++;
            _taskInProgress = false;
        }

        /// <summary>
        /// Handles all tasks by taking in a task handler. Note that this does not
        /// behave well if multiple tasks are handled concurrently (it's subject
        /// to a race condition in task finished coroutines). There should only
        /// be one handler, but if there's more than one, you can gate calling this
        /// method based on the TaskInProgress property.
        /// </summary>
        /// <param name="taskHandler"></param>
        public IEnumerator HandleAllTasks(System.Func<ITask, IEnumerator> taskHandler) {
            while (!Empty) {
                yield return HandleTask(taskHandler);
            }
        }

        /// <summary>
        /// Enqueues a task and waits until that task has started.
        /// </summary>
        public IEnumerator EnqueueAndAwaitTaskStarted(ITask task) {
            int taskId = Enqueue(task);
            yield return WaitUntilTaskStarted(taskId);
        }

        /// <summary>
        /// Enqueues a task and waits until that task has finished.
        /// </summary>
        public IEnumerator EnqueueAndAwaitTaskFinished(ITask task) {
            int taskId = Enqueue(task);
            yield return WaitUntilTaskFinished(taskId);
        }

        public IEnumerator WaitUntilTaskStarted(int taskId) {
            while (taskId > _lastStartedTask) yield return null;
        }

        public IEnumerator WaitUntilTaskFinished(int taskId) {
            while (taskId > _lastFinishedTask) yield return null;
        }

        public IEnumerator WaitUntilEmpty() {
            while (_queue.Count > 0 || _taskInProgress) yield return null;
        }
    }
}