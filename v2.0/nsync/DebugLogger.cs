using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;
using System.Windows;
using System.Collections;
using System.Management;

namespace nsync
{
    /// <summary>
    /// DebugLogger Class
    /// </summary>
    public sealed class DebugLogger
    {
        #region Class Variables
        private string nsyncFolderPath = Environment.GetEnvironmentVariable("APPDATA") + nsync.Properties.Resources.nsyncFolderPath;
        private string debugFolderPath = Environment.GetEnvironmentVariable("APPDATA") + nsync.Properties.Resources.debugFolderPath;
        private string debugFilePath;
        private string currentTime = System.DateTime.Now.ToString(nsync.Properties.Resources.timeStampFormat);
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
                debugFilePath = Environment.GetEnvironmentVariable("APPDATA") + nsync.Properties.Resources.debugFolderPath + debugFileCreationTime + ".txt";
                try
                {
                    log = new StreamWriter(debugFilePath, true);
                    WriteHeaderMessage(debugFileCreationTime);
                }
                catch
                {
                    //MessageBox.Show("=DEBUG LOGGER=" + "\n" + "Constructor: Error creating/writing debugFile");
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
        /// Write the last log message to log file and close the file
        /// </summary>
        /// <param name="message"></param>
        public void ClosingMessage(string message)
        {
            WriteLogMessage(nsync.Properties.Resources.notApplicable, nsync.Properties.Resources.notApplicable, "DebugLogger.ClosingMessage()", nsync.Properties.Resources.debugLoggerClosingMessage);
            try
            {
                log.Close();
            }
            catch
            {
                //MessageBox.Show("=DEBUG LOGGER=" + "\n" + "Error closing debugFile");
            }
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

        /// <summary>
        /// Write the log message to the log file
        /// <para>Overloaded method to print the exclude filters</para>
        /// </summary>
        /// <param name="leftPath"></param>
        /// <param name="rightPath"></param>
        /// <param name="callingMethodName"></param>
        /// <param name="message"></param>
        /// <param name="excludeFileNames"></param>
        /// <param name="excludeFileTypes"></param>
        /// <param name="excludeFolders"></param>
        public void LogMessage(string leftPath, string rightPath, string callingMethodName, string message, 
            List<string> excludeFileNames, List<string> excludeFileTypes, List<string> excludeFolders)
        {
            WriteLogMessage(leftPath, rightPath, callingMethodName, message, excludeFileNames, excludeFileTypes, excludeFolders);
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
                catch
                {
                    //MessageBox.Show("=DEBUG LOGGER=" + "\n" + e.Message);
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
                catch
                {
                    //MessageBox.Show("=DEBUG LOGGER=" + "\n" + e.Message);
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

        /// <summary>
        /// Creates the header of the debug log file
        /// </summary>
        /// <param name="debugFileCreationTime">This string holds the timestamp of the file creation</param>
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
                log.WriteLine("--------------------------------------");
                log.WriteLine("User's System Configuration");
                log.WriteLine("OS: " + GetSystemInfo("Win32_OperatingSystem", "Caption"));
                log.WriteLine("OS Architecture: " + GetSystemInfo("Win32_OperatingSystem", "OSArchitecture"));
                log.WriteLine("System Type: " + GetSystemInfo("Win32_ComputerSystem", "SystemType"));
                log.WriteLine("Description: " + GetSystemInfo("Win32_ComputerSystem", "Description"));
                log.WriteLine("Manufacturer: " + GetSystemInfo("Win32_ComputerSystem", "Manufacturer"));
                log.WriteLine("Model: " + GetSystemInfo("Win32_ComputerSystem", "Model"));
                log.WriteLine("Machine Name: " + System.Environment.MachineName.ToString());
                log.WriteLine("Username: " + System.Environment.UserName.ToString());
                log.WriteLine("--------------------------------------");
                log.WriteLine("");
                log.Flush();
            }
            catch
            {
                //MessageBox.Show("=DEBUG LOGGER=" + "\n" + "Error creating/writing debugFile");
            }
        }

        /// <summary>
        /// Append log message to the existing log file
        /// </summary>
        /// <param name="leftPath"></param>
        /// <param name="rightPath"></param>
        /// <param name="callingMethodName"></param>
        /// <param name="message"></param>
        private void WriteLogMessage(string leftPath, string rightPath, string callingMethodName, string message)
        {
            try
            {
                log.WriteLine("");
                log.WriteLine("--------------------------------------");
                log.WriteLine("[" + System.DateTime.Now.ToString(nsync.Properties.Resources.timeStampFormat) + "]");
                log.WriteLine("Left Path: " + leftPath);
                log.WriteLine("Right Path: " + rightPath);
                log.WriteLine("Calling Method: " + callingMethodName);
                log.WriteLine("Message: " + message);
                log.WriteLine("--------------------------------------");
                log.WriteLine("");
                log.Flush();
            }
            catch
            {
                //MessageBox.Show("=DEBUG LOGGER=" + "\n" + "Error creating/writing debugFile");
            }
        }

        /// <summary>
        /// Append log message to the existing log file
        /// <para>Overloaded function for printing exclude filters</para>
        /// </summary>
        /// <param name="leftPath"></param>
        /// <param name="rightPath"></param>
        /// <param name="callingMethodName"></param>
        /// <param name="message"></param>
        /// <param name="excludeFileNames"></param>
        /// <param name="excludeFileTypes"></param>
        /// <param name="excludeFolders"></param>
        private void WriteLogMessage(string leftPath, string rightPath, string callingMethodName, string message, List<string> excludeFileNames, List<string> excludeFileTypes, List<string> excludeFolders)
        {
            string listOfFileNames = BuildFilterString(excludeFileNames);
            string listOfFileTypes = BuildFilterString(excludeFileTypes);
            string listOfFolderNames = BuildFilterString(excludeFolders);

            try
            {
                log.WriteLine("");
                log.WriteLine("--------------------------------------");
                log.WriteLine("[" + System.DateTime.Now.ToString(nsync.Properties.Resources.timeStampFormat) + "]");
                log.WriteLine("Left Path: " + leftPath);
                log.WriteLine("Right Path: " + rightPath);
                log.WriteLine("Calling Method: " + callingMethodName);
                log.WriteLine("Message: " + message);
                log.WriteLine("Exclude Filenames: ");
                log.WriteLine(listOfFileNames);
                log.WriteLine("Exclude File Types: ");
                log.WriteLine(listOfFileTypes);
                log.WriteLine("Exclude Folders: ");
                log.WriteLine(listOfFolderNames);
                log.WriteLine("--------------------------------------");
                log.WriteLine("");
                log.Flush();
            }
            catch
            {
                //MessageBox.Show("=DEBUG LOGGER=" + "\n" + "Error creating/writing debugFile");
            }
        }

        /// <summary>
        /// Builds a list of string into a single string
        /// </summary>
        /// <param name="filter">This parameter contains the list of string to be concat</param>
        /// <returns></returns>
        private string BuildFilterString(List<string> filter)
        {
            if (filter.Count == 0)
                return "Empty";

            StringBuilder sb = new StringBuilder();
            try
            {
                foreach (string message in filter)
                    sb.AppendLine(message + " ");
            }
            catch
            {
                sb.AppendLine("Error printing filters");
            }
            return sb.ToString();
        }

        /// <summary>
        /// Gets the system information as stated by the parameters
        /// </summary>
        /// <param name="strTable"></param>
        /// <param name="properties"></param>
        /// <returns>Returns a string which contains the requested system information</returns>
        private string GetSystemInfo(string strTable, string properties)
        {
            try
            {
                ManagementObjectSearcher mos = new ManagementObjectSearcher();
                mos.Query.QueryString = "SELECT " + properties + " FROM " + strTable;
                ManagementObjectCollection moc = mos.Get();
                string strInfo = string.Empty;
                foreach (ManagementObject mo in moc)
                    foreach (PropertyData pd in mo.Properties)
                        strInfo += pd.Value + ",";
                return strInfo.Substring(0, strInfo.Length - 1);
            }
            catch { return nsync.Properties.Resources.getSystemInfoErrorMessage; }
        }
        #endregion
    }
}