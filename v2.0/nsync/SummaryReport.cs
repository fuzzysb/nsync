using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;

namespace nsync
{
    //CLASS FOR SUMMARY REPORT
    class SummaryReport
    {
        #region Class Variables
        //Unused Variables Kept For Future Use If Required
        private int totalChanges;
        private int createChanges;
        private int updateChanges;      
        private int deleteChanges;
        private int renameChanges;

        //Actual Variables being used
        private List<string> errorMessage = new List<string>();
        private bool noChanges = false;
        private string directoryPath = Path.GetDirectoryName(System.Reflection.Assembly.GetExecutingAssembly().Location) +
            "\\log";
        private string logPath = Path.GetDirectoryName(System.Reflection.Assembly.GetExecutingAssembly().Location) +
            "\\log\\" + System.DateTime.Now.ToString("dd-MMM-yyyy HH'h'mm'm'ss's'") + ".txt";
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

        /// <summary>
        /// Constructor for SummaryReport when there are some changes
        /// Unused for now, future development might want to display all the types of changes
        /// For now, only erroneous is displayed
        /// </summary>
        public SummaryReport(int allChanges, int newFiles, int overwrittenFiles, int deletedFiles,
            int renamedFiles, List<FileData> information)
        {
            totalChanges = allChanges;
            createChanges = newFiles;
            updateChanges = overwrittenFiles;
            deleteChanges = deletedFiles;
            renameChanges = renamedFiles;
            fileData = new List<FileData>();
            fileData = information;
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

        private void ParseErrorMessage()
        {
            if(errorMessage.Count < 2)
                return;

            List<string> actualErrorMessage = new List<string>();
            for (int i = 0; i < errorMessage.Count; i += 2)
                actualErrorMessage.Add("There was a file renaming conflict in " + errorMessage[i] + " and " + errorMessage[i + 1]);

            errorMessage = actualErrorMessage;
        }
        #endregion

        #region Public Methods
        /// <summary>
        /// Generates log data and writes to log file.
        /// </summary>
        public void CreateLog()
        {
            ParseErrorMessage();

            CheckFolderExist();
            StreamWriter log = new StreamWriter(logPath, true);

            log.WriteLine("Sync Done at : " + System.DateTime.Now.ToString("dd-MMM-yyyy h:mm:ss tt"));
            log.WriteLine("--------------------------------------");

            if(!noChanges)
            {
                log.WriteLine("File Sync Completed");
                log.WriteLine("Number of errors found : " + fileData.Count.ToString());
                log.WriteLine("-----------------------------");
                foreach (FileData file in fileData)
                {
                    switch (file.ChangeType)
                    {
                        case Changes.Delete:
                            log.WriteLine("File Failed To Be Delete");
                            log.WriteLine("-----------------------------");
                            log.WriteLine(file.FileName);
                            break;

                        case Changes.Create:
                            log.WriteLine("File Failed To Be Copied Over");
                            log.WriteLine("-----------------------------");
                            log.WriteLine(file.FileName);
                            break;

                        case Changes.Update:
                            log.WriteLine("File Failed To Be Overwritten");
                            log.WriteLine("-----------------------------");
                            log.WriteLine(file.FileName);
                            break;
                        case Changes.Rename:
                            log.WriteLine("File Failed To Be Renamed");
                            log.WriteLine("-----------------------------");
                            log.WriteLine(file.FileName);
                            break;
                    }
                }
                if (errorMessage.Count != 0)
                {
                    foreach (string message in errorMessage)
                    {
                        log.WriteLine(" ");
                        log.WriteLine(message);
                    }
                }

            }
            else
            {
                log.WriteLine("File Sync Successful. No Error Detected");
                if (errorMessage.Count != 0)
                {
                    foreach (string message in errorMessage)
                    {
                        log.WriteLine(" ");
                        log.WriteLine(message);
                    }
                }
            }

            log.Close();
        }
        #endregion

    }
}
