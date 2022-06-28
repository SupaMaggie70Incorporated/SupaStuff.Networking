using System;
using System.Collections.Generic;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Timers;
using Timer = System.Timers.Timer;

namespace SupaStuff.Net.Shared
{
    [Obsolete]
    internal class UpdateThread : IDisposable
    {
        public static UpdateThread Instance;
        private bool running;
        public int updateInterval;
        public Timer timer;
        public static UpdateThread MakeUpdateThread(int updateInterval)
        {
            if (Instance != null)
            {
                Instance.Stop();
            }
            Instance = new UpdateThread(updateInterval);
            return Instance;
        }
        private UpdateThread(int updateInterval)
        {
            Instance = this;
            this.updateInterval = updateInterval;
            timer = new Timer(updateInterval);
            timer.Elapsed += Update;
            timer.AutoReset = true;
            Start();
        }
        public void Start()
        {
            timer.Enabled = true;
            running = true;
        }
        public void Stop()
        {
            timer.Enabled = false;
            running = false;
        }
        public void Update(Object src,ElapsedEventArgs args)
        {
            Main.Update();
        }
        public void Dispose()
        {
            Stop();
            try
            {
                timer.Close();
                timer.Dispose();
            }
            catch
            {

            }
        }
    }
}
