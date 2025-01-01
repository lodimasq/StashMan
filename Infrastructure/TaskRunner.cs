using System;
using System.Collections.Concurrent;
using System.Threading;
using System.Threading.Tasks;

namespace StashMan.Infrastructure
{
    /// <summary>
    /// A simple concurrency or "task runner" approach.
    /// Spawns or tracks background tasks, can stop them by name, etc.
    /// </summary>
    public static class TaskRunner
    {
        public static readonly ConcurrentDictionary<string, CancellationTokenSource> ActiveTasks 
            = new ConcurrentDictionary<string, CancellationTokenSource>();

        /// <summary>
        /// Runs a task by name and tracks it for cancellation.
        /// The job must accept a CancellationToken to allow graceful stopping.
        /// </summary>
        public static void Run(Func<CancellationToken, Task> job, string name)
        {
            if (ActiveTasks.ContainsKey(name))
            {
                // Task with the same name is already running
                Console.WriteLine($"Task '{name}' is already running.");
                return;
            }

            var cts = new CancellationTokenSource();
            ActiveTasks[name] = cts;

            Task.Run(async () =>
            {
                try
                {
                    // Console.WriteLine($"Task '{name}' started.");
                    await job(cts.Token); // Pass the token to the job
                }
                catch (OperationCanceledException)
                {
                    // Expected when the task is canceled
                    // Console.WriteLine($"Task '{name}' was canceled.");
                }
                catch (Exception ex)
                {
                    // Log or handle unexpected exceptions
                    // Console.WriteLine($"Task '{name}' encountered an error: {ex.Message}");
                }
                finally
                {
                    // Console.WriteLine($"Task '{name}' has exited.");
                    ActiveTasks.TryRemove(name, out _); // Remove from active tasks
                }
            }, cts.Token);
        }

        /// <summary>
        /// Stops a running task by name by requesting cancellation.
        /// </summary>
        public static void Stop(string name)
        {
            if (ActiveTasks.TryGetValue(name, out var cts))
            {
                Console.WriteLine($"Stopping task: {name}");
                cts.Cancel(); // Request cancellation
                ActiveTasks.TryRemove(name, out _); // Remove from the dictionary
            }
            else
            {
                Console.WriteLine($"No task with name '{name}' is currently running.");
            }
        }

        /// <summary>
        /// Checks if a task is currently running.
        /// </summary>
        public static bool IsRunning(string name)
        {
            return ActiveTasks.ContainsKey(name);
        }
    }
}
