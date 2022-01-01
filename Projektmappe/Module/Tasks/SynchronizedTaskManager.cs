using System;
using System.Collections.Generic;
using System.Threading;
using GVRP.Module.Logging;

namespace GVRP.Module.Tasks
{
    public sealed class SynchronizedTaskManager
    {
        public static SynchronizedTaskManager Instance { get; } = new SynchronizedTaskManager();

        private readonly Thread mainThread;
        private bool hasToStop;

        private readonly List<SynchronizedTask> tasks = new List<SynchronizedTask>();

        private SynchronizedTaskManager()
        {
            mainThread = new Thread(MainLoop) {IsBackground = true};
            mainThread.Start();
        }

        public void Add(SynchronizedTask synchronizedTask)
        {
            if (!synchronizedTask.CanExecute()) return;
            lock (tasks) tasks.Add(synchronizedTask);
        }

        public void Remove(SynchronizedTask synchronizedTask)
        {
            lock (tasks) tasks.Remove(synchronizedTask);
        }

        public void Shutdown()
        {
            hasToStop = true;
            mainThread.Abort();
        }

        private void MainLoop()
        {
            while (!hasToStop)
            {
                try
                {
                    Thread.Sleep(1500);
                    if (tasks.Count == 0) continue;
                    List<SynchronizedTask> localTasks;
                    lock (tasks)
                    {
                        localTasks = new List<SynchronizedTask>(tasks);//Todo: not needed
                    }
                    
                    for (var i = localTasks.Count - 1; i >= 0; i--)
                    {
                        if (localTasks[i] != null)
                        {
                            if (localTasks[i].CanExecute())
                            {
                                try
                                {
                                    localTasks[i].Execute();
                                }
                                catch (Exception ex)
                                {
                                    Logger.Print("TASK EXECUTION FAILURE");
                                    Logger.Print(ex.ToString());
                                }
                            }

                            tasks.RemoveAt(i);
                        }
                    }
                }
                catch (Exception ex)
                {
                    Logger.Print("SYNCHRONIZATION FAILURE");
                    Logger.Print(ex.ToString());
                }
            }
        }
    }
}