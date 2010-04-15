///////////////////////////////////////////////////////
//                                        
//   Title:     SingleInstance.cs
//   Author:    Siow Boon Lin Eugene
//
///////////////////////////////////////////////////////

using System;
using System.Threading;
using System.Windows;

namespace nsync
{
    // Class Credited to 
    // IliyaTretyakov
    // http://wpfsingleinstance.codeplex.com/
    /// <summary>
    /// SingleInstance adds support for having a Single Instance Application in WPF
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
            app.Dispatcher.BeginInvoke(System.Windows.Threading.DispatcherPriority.Normal,new activate(delegate()
            {
                Application.Current.MainWindow.Activate();
            }));
        }

        private delegate void activate();
    }
}
