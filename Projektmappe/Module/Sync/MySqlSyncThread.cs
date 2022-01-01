using System;
using System.Collections.Concurrent;
using System.Threading;
using System.Threading.Tasks;
using MySql.Data.MySqlClient;
using GVRP.Handler;
using GVRP.Module.Configurations;
using GVRP.Module.Logging;

namespace GVRP.Module.Sync
{
    public class MySqlSyncThread
    {
        public static MySqlSyncThread Instance { get; } = new MySqlSyncThread();

        private readonly ConcurrentQueue<string> queue = new ConcurrentQueue<string>();
        private readonly ConcurrentQueue<string> queue2 = new ConcurrentQueue<string>();
        private readonly ConcurrentQueue<string> queue3 = new ConcurrentQueue<string>();
        private readonly ConcurrentQueue<string> queue4 = new ConcurrentQueue<string>();
        private int index = 1;

        private MySqlSyncThread()
        {
            Task.Factory.StartNew(async () =>
            {
                while (true)
                {
                    while (queue.IsEmpty)
                    {
                        Thread.Sleep(1500);
                    }
                    using (var conn = new MySqlConnection(Configuration.Instance.GetMySqlConnection()))
                    {
                        try
                        {
                            conn.Open();
                            while (!queue.IsEmpty)
                            {
                                try
                                {
                                    if (!queue.TryDequeue(out var query)) continue;
                                    using (var cmd = conn.CreateCommand())
                                    {
                                        cmd.CommandText = @query;
                                        await cmd.ExecuteNonQueryAsync();
                                        Logger.Debug(@"async task: " + query);
                                    }
                                }
                                catch (Exception e)
                                {
                                    Logger.Crash(e);
                                }
                            }
                        }
                        catch (Exception e)
                        {
                            Logger.Crash(e);

                            if (e is NullReferenceException)
                            {
                                //DiscordHandler l_Handler = new DiscordHandler("Eine kritische Exception ist aufgetreten!", e.ToString());
                                //l_Handler.Send();
                            }
                        }
                        finally
                        {
                            conn.Close();
                        }
                    }
                }
            }, TaskCreationOptions.LongRunning);

            Task.Factory.StartNew(async () =>
            {
                while (true)
                {
                    while (queue2.IsEmpty)
                    {
                        Thread.Sleep(1500);
                    }
                    using (var conn = new MySqlConnection(Configuration.Instance.GetMySqlConnection()))
                    {
                        try
                        {
                            conn.Open();
                            while (!queue2.IsEmpty)
                            {
                                try
                                {
                                    if (!queue2.TryDequeue(out var query)) continue;
                                    using (var cmd = conn.CreateCommand())
                                    {
                                        cmd.CommandText = @query;
                                        await cmd.ExecuteNonQueryAsync();
                                        Logger.Debug(@"async task: " + query);
                                    }
                                }
                                catch (Exception e)
                                {
                                    Logger.Crash(e);
                                }
                            }
                        }
                        catch (Exception e)
                        {
                            Logger.Crash(e);

                            if (e is NullReferenceException)
                            {
                                //DiscordHandler l_Handler = new DiscordHandler("Eine kritische Exception ist aufgetreten!", e.ToString());
                                //l_Handler.Send();
                            }
                        }
                        finally
                        {
                            conn.Close();
                        }
                    }
                }
            }, TaskCreationOptions.LongRunning);

            Task.Factory.StartNew(async () =>
            {
                while (true)
                {
                    while (queue3.IsEmpty)
                    {
                        Thread.Sleep(1500);
                    }
                    using (var conn = new MySqlConnection(Configuration.Instance.GetMySqlConnection()))
                    {
                        try
                        {
                            conn.Open();
                            while (!queue3.IsEmpty)
                            {
                                try
                                {
                                    if (!queue3.TryDequeue(out var query)) continue;
                                    using (var cmd = conn.CreateCommand())
                                    {
                                        cmd.CommandText = @query;
                                        await cmd.ExecuteNonQueryAsync();
                                        Logger.Debug(@"async task: " + query);
                                    }
                                }
                                catch (Exception e)
                                {
                                    Logger.Crash(e);
                                }
                            }
                        }
                        catch (Exception e)
                        {
                            Logger.Crash(e);

                            if (e is NullReferenceException)
                            {
                                //DiscordHandler l_Handler = new DiscordHandler("Eine kritische Exception ist aufgetreten!", e.ToString());
                                //l_Handler.Send();
                            }
                        }
                        finally
                        {
                            conn.Close();
                        }
                    }
                }
            }, TaskCreationOptions.LongRunning);

            Task.Factory.StartNew(async () =>
            {
                while (true)
                {
                    while (queue4.IsEmpty)
                    {
                        Thread.Sleep(1500);
                    }
                    using (var conn = new MySqlConnection(Configuration.Instance.GetMySqlConnection()))
                    {
                        try
                        {
                            conn.Open();
                            while (!queue4.IsEmpty)
                            {
                                try
                                {
                                    if (!queue4.TryDequeue(out var query)) continue;
                                    using (var cmd = conn.CreateCommand())
                                    {
                                        cmd.CommandText = @query;
                                        await cmd.ExecuteNonQueryAsync();
                                        Logger.Debug(@"async task: " + query);
                                    }
                                }
                                catch (Exception e)
                                {
                                    Logger.Crash(e);
                                }
                            }
                        }
                        catch (Exception e)
                        {
                            Logger.Crash(e);

                            if (e is NullReferenceException)
                            {
                                DiscordHandler l_Handler = new DiscordHandler();
                                l_Handler.SendMessage("Eine kritische Exception ist aufgetreten!", e.ToString());
                            }
                        }
                        finally
                        {
                            conn.Close();
                        }
                    }
                }
            }, TaskCreationOptions.LongRunning);
        }

        public async void Add(string query, bool inventoryquery)
        {
            if (inventoryquery)
            {
                queue4.Enqueue(query);
                return;
            }

            if (index > 3) index = 1;

            if (index == 1) queue.Enqueue(query);
            else if (index == 2) queue2.Enqueue(query);
            else queue3.Enqueue(query);

            index++;
        }
    }
}