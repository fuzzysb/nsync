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
        //Unused Variables Kept For Future Use If Required
        private int totalChanges;
        private int createChanges;
        private int updateChanges;      
        private int deleteChanges;
        private int renameChanges;

        //Actual Variables being used
        private bool noChanges = false;
        private string directoryPath = Path.GetDirectoryName(System.Reflection.Assembly.GetExecutingAssembly().Location) +
            "\\log";
        private string logPath = Path.GetDirectoryName(System.Reflection.Assembly.GetExecutingAssembly().Location) +
            "\\log\\" + System.DateTime.Now.ToString("dd-MMM-yyyy HH'h'mm'm'ss's'") + ".txt";
        private List<FileData> fileData;

        /// <summary>
        /// Constructor for SummaryReport when there are no changes
        /// </summary>
        public SummaryReport(bool zeroChange)
        {
            noChanges = zeroChange;
        }

        /// <summary>
        /// Constructor for SummaryReport when there are some changes
        /// </summary>
        public SummaryReport(List<FileData> information)
        {
            fileData = new List<FileData>();
            fileData = information;
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
        /// Generates log data and writes to log file.
        /// </summary>
        public void CreateLog()
        {
            CheckFolderExist();
            StreamWriter log = new StreamWriter(logPath, true);

            log.WriteLine("Sync Done at : " + System.DateTime.Now.ToString("dd-MMM-yyyy h:mm:ss tt"));

            if(!noChanges)
            {
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
            }
            else
            {
                log.WriteLine("File Sync Successful. No Error Detected");
            }

            log.WriteLine("");
            log.Close();
        }
    }

}
