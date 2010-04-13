﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;
using System.Windows;
using System.Management;

namespace nsync
{
    /// <summary>
    /// DebugLogger Class
    /// </summary>
    public sealed class DebugLogger
    {
        #region Class Variables
        private string nsyncFolderPath = Environment.GetEnvironmentVariable("APPDATA") + "\\nsync\\";
        private string debugFolderPath = Environment.GetEnvironmentVariable("APPDATA") + "\\nsync\\debug\\";
        private string debugFilePath;
        private string currentTime = System.DateTime.Now.ToString("dd-MMM-yyyy HH'h'mm'm'ss's'");
        private Window mainWindow = Application.Current.MainWindow;
        private ExcludeWindow excludeWindow;
        private VisualPreviewWindow visualPreviewWindow;
        private HomePage homePage;
        private StreamWriter log;
        #endregion

        #region Singleton Setup
        /// <summary>
        /// Create an instance of DebugLogger class
        /// </summary>
        private static readonly DebugLogger instance = new DebugLogger();

        /// <summary>
        /// Constructor of DebugLogger class
        /// </summary>
        private DebugLogger()
        {
            string debugFileCreationTime = currentTime;
            if (SetupDebugFolders())
            {
                debugFilePath = Environment.GetEnvironmentVariable("APPDATA") + "\\nsync\\debug\\" + debugFileCreationTime + ".txt";
                try
                {
                    log = new StreamWriter(debugFilePath, true);
                    WriteHeaderMessage(debugFileCreationTime);
                }
                catch
                {
                    MessageBox.Show("=DEBUG LOGGER=" + "\n" + "Error creating/writing debugFile");
                }


            }

        }

        /// <summary>
        /// Gets the instance of the Settings object
        /// </summary>
        public static DebugLogger Instance
        {
            get { return instance; }
        }
        #endregion

        #region Public Methods
        /// <summary>
        /// References Homepage (Overloaded Method) 
        /// </summary>
        /// <param name="homePage"></param>
        public void SetOwnerWindow(HomePage homePage)
        {
            this.homePage = homePage;
        }

        /// <summary>
        /// References ExcludeWindow (Overloaded Method) 
        /// </summary>
        /// <param name="excludeWindow"></param>
        public void SetOwnerWindow(ExcludeWindow excludeWindow)
        {
            this.excludeWindow = excludeWindow;
        }

        /// <summary>
        /// References VisualPreviewWindow (Overloaded Method)
        /// </summary>
        /// <param name="visualPreviewWindow"></param>
        public void SetOwnerWindow(VisualPreviewWindow visualPreviewWindow)
        {
            this.visualPreviewWindow = visualPreviewWindow;
        }

        /// <summary>
        /// Write the last log message to log file and close the file
        /// </summary>
        /// <param name="leftPath"></param>
        /// <param name="rightPath"></param>
        /// <param name="message"></param>
        public void ClosingMessage(string leftPath, string rightPath, string message)
        {
            WriteLogMessage(leftPath, rightPath, "DebugLogger.ClosingMessage()", "NSYNC CLOSING");
            log.Close();
        }

        /// <summary>
        /// Write the log message to the log file
        /// </summary>
        /// <param name="leftPath"></param>
        /// <param name="rightPath"></param>
        /// <param name="callingMethodName"></param>
        /// <param name="message"></param>
        public void LogMessage(string leftPath, string rightPath, string callingMethodName, string message)
        {
            WriteLogMessage(leftPath, rightPath, callingMethodName, message);
        }

        #endregion

        #region Private Methods
        /// <summary>
        /// Check whether debug folder exists. If not create new.
        /// </summary>
        private bool CreateDebugFolder()
        {
            if (!(Directory.Exists(debugFolderPath)))
            {
                try
                {
                    Directory.CreateDirectory(debugFolderPath);
                }
                catch (UnauthorizedAccessException)
                {
                    // if the debug folder is locked, do nothing
                    return false;
                }
                catch (Exception e)
                {
                    MessageBox.Show("=DEBUG LOGGER=" + "\n" + e.Message);
                    return false;
                }
                return true;
            }
            return true;
        }

        /// <summary>
        /// Check whether nsync folder exists. If not create new.
        /// </summary>
        private bool CreateNsyncFolder()
        {
            if (!(Directory.Exists(nsyncFolderPath)))
            {
                try
                {
                    Directory.CreateDirectory(nsyncFolderPath);
                }
                catch (UnauthorizedAccessException)
                {
                    // if the debug folder is locked, do nothing
                    return false;
                }
                catch (Exception e)
                {
                    MessageBox.Show("=DEBUG LOGGER=" + "\n" + e.Message);
                    return false;
                }
                return true;
            }
            return true;
        }

        /// <summary>
        /// Setup the debug logger folders ready for use later.
        /// </summary>
        private bool SetupDebugFolders()
        {
            if (!CreateNsyncFolder() || !CreateDebugFolder())
                return false;
            else
                return true;
        }

        private void WriteHeaderMessage(string debugFileCreationTime)
        {
            try
            {
                log.WriteLine("======================");
                log.WriteLine(" nsync DEBUG LOG");
                log.WriteLine("======================");
                log.WriteLine("");
                log.WriteLine("Start time: " + debugFileCreationTime);
                log.WriteLine("");
                log.WriteLine("User's System Configuration");
                log.WriteLine("--------------------------------------");
            }
            catch
            {
                MessageBox.Show("=DEBUG LOGGER=" + "\n" + "Error creating/writing debugFile");
                return;
            }
        }

        private void WriteLogMessage(string leftPath, string rightPath, string callingMethodName, string message)
        {
            try
            {
                log.WriteLine("");
                log.WriteLine("--------------------------------------");
                log.WriteLine("[" + System.DateTime.Now.ToString("dd-MMM-yyyy HH'h'mm'm'ss's'") + "]");
                log.WriteLine("Left Path: " + leftPath);
                log.WriteLine("Right Path: " + rightPath);
                log.WriteLine("Calling Method: " + callingMethodName);
                log.WriteLine("Message: " + message);
                log.WriteLine("--------------------------------------");
                log.WriteLine("");
            }
            catch
            {
                MessageBox.Show("=aaa");
                return;
            }
        }
        #endregion
    }
}