using System;
using System.Collections.Concurrent;
using System.Threading;
using System.Threading.Tasks;

namespace StashMan.Infrastructure
{
    /// <summary>
    /// A simple concurrency or "task runner" approach.
    /// Spawns or tracks background tasks, can stop them by name, etc.
    /// This is an example scaffold.
    /// </summary>
    public static class TaskRunner
    {
        public static readonly ConcurrentDictionary<string, CancellationTokenSource> ActiveTasks 
            = new ConcurrentDictionary<string, CancellationTokenSource>();

        public static void Run(Func<Task> job, string name)
        {
            if (ActiveTasks.ContainsKey(name))
            {
                // Already running or handle differently
                return;
            }

            var cts = new CancellationTokenSource();
            ActiveTasks[name] = cts;

            Task.Run(async () =>
            {
                try
                {
                    await job();
                }
                catch (Exception e)
                {
                    // Log or handle exception
                }
                finally
                {
                    // remove from dictionary
                    ActiveTasks.TryRemove(name, out _);
                }
            }, cts.Token);
        }

        public static void Stop(string name)
        {
            if (ActiveTasks.TryGetValue(name, out var cts))
            {
                cts.Cancel();
                ActiveTasks.TryRemove(name, out _);
            }
        }

        public static bool IsRunning(string name)
        {
            return ActiveTasks.ContainsKey(name);
        }
    }
}