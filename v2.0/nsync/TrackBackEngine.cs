using System;
using System.Collections.Generic;
using System.IO;
using System.Xml;
using System.Management;
using System.ComponentModel;

namespace nsync
{
    class TrackBackEngine
    {
        #region Class Variables
        ////////////////////
        // CLASS VARIABLES
        ////////////////////

        public System.ComponentModel.BackgroundWorker backgroundWorkerForTrackBackBackup;
        public System.ComponentModel.BackgroundWorker backgroundWorkerForTrackBackRestore;
        private TrackBackFolder leftFolder, rightFolder;
        private string leftFolderPath, rightFolderPath;
        private string timeStamp;

        private readonly string DATE_FORMAT = "yyyy-MM-dd";
        private readonly string TIME_FORMAT = "hh.mm.ss tt";
        #endregion

        #region Properties
        ////////////////////
        // PROPERTIES
        ////////////////////

        /// <summary>
        /// Getter method for left folder path
        /// </summary>
        public string LeftFolderPath
        {
            get { return leftFolderPath; }
            set 
            { 
                leftFolderPath = value;
                leftFolder = new TrackBackFolder(leftFolderPath);
            }
        }

        /// <summary>
        /// Getter method for right folder path
        /// </summary>
        public string RightFolderPath
        {
            get { return rightFolderPath; }
            set 
            { 
                rightFolderPath = value;
                rightFolder = new TrackBackFolder(rightFolderPath);
            }
        }

        /// <summary>
        /// Getter method for time stamp
        /// </summary>
        public string TimeStamp
        {
            get { return timeStamp; }
        }
        #endregion

        #region Constructor
        ////////////////////
        // CONSTRUCTOR
        ////////////////////

        /// <summary>
        /// Creates TrackBackEngine object
        /// </summary>
        public TrackBackEngine()
        {
            backgroundWorkerForTrackBackBackup = new System.ComponentModel.BackgroundWorker();
            backgroundWorkerForTrackBackBackup.DoWork += new DoWorkEventHandler(backgroundWorkerForTrackBackBackup_DoWork);
            backgroundWorkerForTrackBackBackup.WorkerReportsProgress = true;

            backgroundWorkerForTrackBackRestore = new System.ComponentModel.BackgroundWorker();
            backgroundWorkerForTrackBackRestore.DoWork += new DoWorkEventHandler(backgroundWorkerForTrackBackRestore_DoWork);
            backgroundWorkerForTrackBackRestore.WorkerReportsProgress = true;
        }
        #endregion

        #region Public Methods
        ////////////////////
        // PUBLIC METHODS
        ////////////////////

        /// <summary>
        /// Starts the backing up of folders
        /// </summary>
        public void StartBackup()
        {
            backgroundWorkerForTrackBackBackup.RunWorkerAsync();
        }

        /// <summary>
        /// Starts the restoration of folders
        /// </summary>
        /// <param name="folderPath">The path of the original folder</param>
        /// <param name="dateTime">The version selected, indicated by the date and time</param>
        public void StartRestore(string folderPath, string dateTime)
        {
            TrackBackData data = new TrackBackData();
            data.DateTime = dateTime;
            data.FolderPath = folderPath;

            backgroundWorkerForTrackBackRestore.RunWorkerAsync(data);
        }
        
        /// <summary>
        /// Retrieves the folder names of the different folder versions stored in TrackBack.
        /// </summary>
        /// <param name="folderPath">The folder path string</param>
        /// <returns>A string array of the folder names</returns>
        public string[] GetFolderVersions(string folderPath)
        {
            if (folderPath == leftFolderPath)
                return leftFolder.GetFolderVersions();
            else
                return rightFolder.GetFolderVersions();
        }

        /// <summary>
        /// Retrieves the different folder destinations which the folder was synced to.
        /// </summary>
        /// <param name="folderPath">The folder path string</param>
        /// <returns>A string array of the folder destination paths</returns>
        public string[] GetFolderDestinations(string folderPath)
        {
            if (folderPath == leftFolderPath)
                return leftFolder.GetFolderDestinations();
            else
                return rightFolder.GetFolderDestinations();
        }

        /// <summary>
        /// Retrieves the dates and times of when the sync took place.
        /// </summary>
        /// <param name="folderPath">The folder path string</param>
        /// <returns>A string array of the dates and times of the sync sessions</returns>
        public string[] GetFolderTimeStamps(string folderPath)
        {
            if (folderPath == leftFolderPath)
                return leftFolder.GetFolderTimeStamp();
            else
                return rightFolder.GetFolderTimeStamp();
        }

        /// <summary>
        /// Checks that there is enough disk space for TrackBack to run smoothly
        /// </summary>
        /// <returns>If there is enough space for the folder to be copied, return true, false otherwise.</returns>
        public bool hasEnoughDiskSpace()
        {
            return leftFolder.hasEnoughDiskSpace() && rightFolder.hasEnoughDiskSpace();
        }

        /// <summary>
        /// Checks that the folder has any previous stored versions in TrackBack.
        /// </summary>
        /// <param name="folderPath">The folder path string</param>
        /// <returns>If the folder has stored previous folder versions, return true, false otherwise.</returns>
        public bool hasTrackBackData(string folderPath)
        {
            if (folderPath == leftFolderPath)
                return leftFolder.hasTrackBackData();
            else
                return rightFolder.hasTrackBackData();
        }

        #endregion

        #region Private Methods
        ////////////////////
        // PRIVATE METHODS
        ////////////////////

        /// <summary>
        /// Gets the background worker to start working and start backing up of folders
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void backgroundWorkerForTrackBackBackup_DoWork(object sender, DoWorkEventArgs e)
        {
            timeStamp = DateTime.Now.ToString(DATE_FORMAT) + " " + DateTime.Now.ToString(TIME_FORMAT);

            leftFolder = new TrackBackFolder(leftFolderPath, rightFolderPath, timeStamp);
            rightFolder = new TrackBackFolder(rightFolderPath, leftFolderPath, timeStamp);

            e.Result = BackupFolders();
        }

        private void backgroundWorkerForTrackBackRestore_DoWork(object sender, DoWorkEventArgs e)
        {
            TrackBackData data = e.Argument as TrackBackData;
            e.Result = RestoreFolder(data.FolderPath, data.DateTime);
        }

        /// <summary>
        /// Stores a copy of the sync folder pair in a subfolder located inside the "_nsync_trackback" folder
        /// </summary>
        private bool BackupFolders()
        {
            try
            {
                leftFolder.BackupFolder();
                rightFolder.BackupFolder();
            }
            catch (Exception e)
            {
                return false;
            }
            return true;
        }

        /// <summary>
        /// Restores the folder back to its selected version
        /// </summary>
        private bool RestoreFolder(string folderPath, string dateTime)
		{
		    try 
	        {	        
		        if (folderPath == leftFolderPath && leftFolder.isTrackBackXMLValid())
				    leftFolder.RestoreFolder(dateTime);
			    else if (folderPath == rightFolderPath && rightFolder.isTrackBackXMLValid())
			        rightFolder.RestoreFolder(dateTime);
			    else
			        return false;
	        }
	        catch (Exception e)
		    {
                return false;
            }
            return true;
		}
        #endregion
    }

    class TrackBackData
    {
        private string folderPath;
        private string dateTime;

        public string FolderPath
        {
            get { return folderPath; }
            set { folderPath = value; }
        }

        public string DateTime
        {
            get { return dateTime; }
            set { dateTime = value; }
        }
    }
}
