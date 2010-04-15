///////////////////////////////////////////////////////
//
//   Title:     SummaryReport.cs
//   Author:    Woon Tian Wei Norman
//
///////////////////////////////////////////////////////

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;

namespace nsync
{
    /// <summary>
    /// SummaryReport generates a report if there are any files/folders that are not successfully synchronized by nsync.
    /// </summary>
    public class SummaryReport
    {
        #region Class Variables
        private List<string> errorMessage = new List<string>();
        private bool noChanges = false;
        private string leftPath;
        private string rightPath;
        private string directoryPath = Environment.GetEnvironmentVariable("APPDATA") + nsync.Properties.Resources.logFolderPath;
        private string logPath = Environment.GetEnvironmentVariable("APPDATA") + nsync.Properties.Resources.logFolderPath + 
            System.DateTime.Now.ToString(nsync.Properties.Resources.timeStampFormat) + ".txt";
        private List<FileData> fileData;
        #endregion

        #region Constructors
        /// <summary>
        /// Constructor for SummaryReport when there are no changes
        /// </summary>
        public SummaryReport(bool zeroChange, List<string> errorMessage)
        {
            noChanges = zeroChange;
            this.errorMessage = errorMessage;
        }

        /// <summary>
        /// Constructor for SummaryReport when there are some changes
        /// </summary>
        public SummaryReport(List<FileData> information, List<string> errorMessage)
        {
            fileData = new List<FileData>();
            fileData = information;
            this.errorMessage = errorMessage;
        }
        #endregion

        #region Properties
        /// <summary>
        /// Setter and Getter method for left folder path
        /// </summary>
        public string LeftPath
        {
            set { leftPath = value; }
        }

        /// <summary>
        /// Setter and Getter method for right folder path
        /// </summary>
        public string RightPath
        {
            set { rightPath = value; }
        }

        /// <summary>
        /// Setter and Getter method for getting log path
        /// </summary>
        public string LogPath
        {
            get { return logPath; }
            set { logPath = value; }
        }
        #endregion

        #region Private Methods
        /// <summary>
        /// Check whether log folder exists. If not create new.
        /// </summary>
        private void CheckFolderExist()
        {
            if (!(Directory.Exists(directoryPath)))
            {
                Directory.CreateDirectory(directoryPath);
            }
        }

        /// <summary>
        /// Method to Parse rename errors into another format
        /// </summary>
        private void ParseErrorMessage()
        {
            if (errorMessage.Count < 2)
                return;

            List<string> actualErrorMessage = new List<string>();
            for (int i = 0; i < errorMessage.Count; i += 2)
                actualErrorMessage.Add(errorMessage[i] + " and " + errorMessage[i + 1]);

            errorMessage = actualErrorMessage;
        }
        #endregion

        #region Public Methods
        /// <summary>
        /// Generates log data and writes to log file.
        /// </summary>
        public void CreateLog()
        {
            //Edits rename errors into different format
            ParseErrorMessage();

            //Checks if folder exist, will create new log folder if not found
            CheckFolderExist();
            StreamWriter log = new StreamWriter(logPath, true);

            log.WriteLine("======================");
            log.WriteLine(" nsync SUMMARY REPORT");
            log.WriteLine("======================");
            log.WriteLine("");
            log.WriteLine("Sync Done at : " + System.DateTime.Now.ToString(nsync.Properties.Resources.timeStampFormat));
            log.WriteLine("Folders Involved In Synchronization : ");
            log.WriteLine(leftPath.ToString());
            log.WriteLine(rightPath.ToString());
            log.WriteLine("");
            log.WriteLine("--------------------------------------");
            log.WriteLine("");

            //If there are errors, display all files that are not propagated
            if (!noChanges)
            {
                log.WriteLine("File Sync Completed");
                log.WriteLine("Number of errors found : " + fileData.Count.ToString());
                log.WriteLine("");
                foreach (FileData file in fileData)
                {
                    switch (file.ChangeType)
                    {
                        case Changes.Delete:
                            log.WriteLine("File Failed To Be Deleted:");
                            if (file.RootPath == leftPath)
                            {
                                log.WriteLine(rightPath + "\\" + file.RelativePath);
                            }
                            else
                            {
                                log.WriteLine(leftPath + "\\" + file.RelativePath);
                            }
                            log.WriteLine("");
                            break;

                        case Changes.Create:
                            log.WriteLine("File Failed To Be Copied Over:");
                            if (file.RootPath == leftPath)
                            {
                                log.WriteLine(rightPath + "\\" + file.RelativePath);
                            }
                            else
                            {
                                log.WriteLine(leftPath + "\\" + file.RelativePath);
                            }
                            log.WriteLine("");
                            break;

                        case Changes.Update:
                            log.WriteLine("File Failed To Be Overwritten:");
                            if (file.RootPath == leftPath)
                            {
                                log.WriteLine(rightPath + "\\" + file.RelativePath);
                            }
                            else
                            {
                                log.WriteLine(leftPath + "\\" + file.RelativePath);
                            }
                            log.WriteLine("");
                            break;
                        case Changes.Rename:
                            log.WriteLine("File Failed To Be Renamed:");
                            if (file.RootPath == leftPath)
                            {
                                log.WriteLine(rightPath + "\\" + file.RelativePath);
                            }
                            else
                            {
                                log.WriteLine(leftPath + "\\" + file.RelativePath);
                            }
                            log.WriteLine("");
                            break;
                    }
                }
                //When there are renaming conflicts
                //Will display which files gets renamed.
                if (errorMessage.Count != 0)
                {
                    log.WriteLine("--------------------------------------");
                    log.WriteLine("");
                    log.WriteLine("File Renaming Conflicts:");
                    foreach (string message in errorMessage)
                    {
                        log.WriteLine(message);
                    }
                    log.WriteLine("");
                }

            }
            else
            {
                //If not error founds, display rename conflicts if any
                log.WriteLine("File Sync Successful. No Error Detected");
                if (errorMessage.Count != 0)
                {
                    log.WriteLine("--------------------------------------");
                    log.WriteLine("");
                    log.WriteLine("File Renaming Conflicts:");
                    foreach (string message in errorMessage)
                    {
                        log.WriteLine(message);
                    }
                    log.WriteLine("");
                }
            }

            log.Close();
        }
        #endregion

    }
}