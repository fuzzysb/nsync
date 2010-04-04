using System;
using System.Threading;
using System.Windows;

namespace nsync
{
    // Class Credited to 
    // IliyaTretyakov
    // http://wpfsingleinstance.codeplex.com/
    /// <summary>
    /// SingleInstance class adds support for a having Single Instance Applications in WPF
    /// </summary>
    public static class SingleInstance
    {
        internal static void Make(String name, Application app)
        {

            EventWaitHandle eventWaitHandle = null;
            String eventName = Environment.MachineName + "-" + Environment.CurrentDirectory.Replace('\\', '-') + "-" + name;

            bool isFirstInstance = false;

            try
            {
                eventWaitHandle = EventWaitHandle.OpenExisting(eventName);
            }
            catch
            {
                // it's first instance
                isFirstInstance = true;
            }

            if (isFirstInstance)
            {
                eventWaitHandle = new EventWaitHandle(
                    false,
                    EventResetMode.AutoReset,
                    eventName);

                ThreadPool.RegisterWaitForSingleObject(eventWaitHandle, waitOrTimerCallback, app, Timeout.Infinite, false);

                // not need more
                eventWaitHandle.Close();
            }
            else
            {
                eventWaitHandle.Set();

                // For that exit no interceptions
                Environment.Exit(0);
            }
        }


        private static void waitOrTimerCallback(Object state, Boolean timedOut)
        {
            Application app = (Application)state;
            app.Dispatcher.BeginInvoke(new activate(delegate()
            {
                Application.Current.MainWindow.Activate();
            }), null);
        }

        private delegate void activate();
    }
}
