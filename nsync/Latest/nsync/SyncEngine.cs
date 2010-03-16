using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;
using System.Management;
using System.ComponentModel;
using System.Windows.Forms;
using Microsoft.Synchronization;
using Microsoft.Synchronization.Files;

namespace nsync
{
    class SyncEngine
    {
        public System.ComponentModel.BackgroundWorker backgroundWorkerForSync;
        public System.ComponentModel.BackgroundWorker backgroundWorkerForPreSync;
        private ulong freeDiskSpaceForLeft = 0;
        private ulong freeDiskSpaceForRight = 0;
        private ulong diskSpaceNeededForLeft = 0;
        private ulong diskSpaceNeededForRight = 0;
        private bool isCheckForLeftDone = false;
        private string leftPath;
        private string rightPath;
        private static int countDoneChanges = 0;
        private static int countChanges = 0;
        private Intelligence intelligentManager;

        /// <summary>
        /// Constructor for SyncEngine
        /// </summary>
        public SyncEngine()
        {
            // Set up the BackgroundWorker object by 
            // attaching event handlers.
            backgroundWorkerForSync = new System.ComponentModel.BackgroundWorker();
            backgroundWorkerForSync.DoWork += new DoWorkEventHandler(backgroundWorker_DoWork);
            backgroundWorkerForSync.WorkerReportsProgress = true;

            backgroundWorkerForPreSync = new System.ComponentModel.BackgroundWorker();
            backgroundWorkerForPreSync.DoWork += new DoWorkEventHandler(backgroundWorker2_DoWork);
            backgroundWorkerForPreSync.WorkerReportsProgress = true;

            // Create the intelligence manager
            intelligentManager = new Intelligence();
        }

        /// <summary>
        /// Computes the amount of free disk space
        /// <para>Units is in bytes</para>
        /// </summary>
        /// <param name="drive">This parameter is the drive volume to be checked</param>
        /// <returns></returns>
        private ulong GetFreeDiskSpaceInBytes(string drive)
        {
            ManagementObject disk = new ManagementObject(
            "win32_logicaldisk.deviceid=\"" + drive + ":\"");
            disk.Get();
            return (ulong)disk["FreeSpace"];
        }

        /// <summary>
        /// Converts bytes to megabytes
        /// </summary>
        /// <param name="amount">This parameter is the value to be converted</param>
        /// <returns>Returns a string which contains the converted value</returns>
        private string ConvertBytesToMegabytes(ulong amount)
        {
            return (Convert.ToUInt64(amount.ToString()) / 1000000).ToString() + " MB";
        }

        /// <summary>
        /// Detect the changes done to the folder
        /// <para>Updates the metadata</para>
        /// </summary>
        /// <param name="replicaRootPath">This parameter is the folder path to be checked</param>
        /// <param name="filter">This parameter is the filter which will be used during synchronization</param>
        /// <param name="options">This parameter holds the synchronization options</param>
        private static void DetectChangesonFileSystemReplica(
            string replicaRootPath, FileSyncScopeFilter filter, FileSyncOptions options)
        {
            FileSyncProvider provider = null;

            try
            {
                provider = new FileSyncProvider(replicaRootPath, filter, options);
                provider.DetectChanges();
            }
            finally
            {
                // Release resources or memory
                if (provider != null)
                    provider.Dispose();
            }
        }

        /// <summary>
        /// Start the synchronization in one direction
        /// </summary>
        /// <param name="sourcePath">This parameter holds the source folder path</param>
        /// <param name="destPath">This parameter holds the destination folder path</param>
        /// <param name="filter">This parameter is the filter which will be used during synchronization</param>
        /// <param name="options">This parameter holds the synchronization options</param>
        /// <param name="isPreview">This parameter is a boolean which indicates if this method should be run in preview mode</param>
        /// <returns>Returns a boolean to indicate if the the synchronization was successful</returns>
        private bool SyncFileSystemReplicasOneWay(string sourcePath, string destPath,
            FileSyncScopeFilter filter, FileSyncOptions options, bool isPreview)
        {
            FileSyncProvider sourceProvider = null;
            FileSyncProvider destProvider = null;

            try
            {
                sourceProvider = new FileSyncProvider(sourcePath, filter, options);
                destProvider = new FileSyncProvider(destPath, filter, options);

                // When it's in preview mode, no actual changes are done.
                // This mode is used to check the number of changes that will be carried out later
                if (isPreview)
                {
                    sourceProvider.PreviewMode = true;
                    destProvider.PreviewMode = true;
                }
                else
                {
                    sourceProvider.PreviewMode = false;
                    destProvider.PreviewMode = false;
                }

                if (isPreview)
                {
                    if (!isCheckForLeftDone)
                    {
                        freeDiskSpaceForLeft = GetFreeDiskSpaceInBytes(sourcePath.Substring(0, 1));
                        freeDiskSpaceForRight = GetFreeDiskSpaceInBytes(destPath.Substring(0, 1));
                    }
                }

                destProvider.Configuration.ConflictResolutionPolicy = ConflictResolutionPolicy.ApplicationDefined;
                SyncCallbacks destinationCallBacks = destProvider.DestinationCallbacks;
                destinationCallBacks.ItemConflicting += new EventHandler<ItemConflictingEventArgs>(OnItemConflicting);
                destinationCallBacks.ItemConstraint += new EventHandler<ItemConstraintEventArgs>(OnItemConstraint);

                if(isPreview)
                    destProvider.ApplyingChange += new EventHandler<ApplyingChangeEventArgs>(OnApplyingChange);
                else
                    destProvider.AppliedChange += new EventHandler<AppliedChangeEventArgs>(OnAppliedChange);

                SyncOrchestrator agent = new SyncOrchestrator();
                agent.LocalProvider = sourceProvider;
                agent.RemoteProvider = destProvider;
                agent.Direction = SyncDirectionOrder.Upload;

                agent.Synchronize();

                if (isPreview)
                    return CheckSpace();

                return true;
            }
            finally
            {
                if (sourceProvider != null) sourceProvider.Dispose();
                if (destProvider != null) destProvider.Dispose();
            }
        }

        /// <summary>
        /// Checks if there is sufficient disk space for synchronization to be done
        /// </summary>
        /// <returns>Returns a boolean of the result</returns>
        private bool CheckSpace()
        {
            if (!isCheckForLeftDone)
            {
                isCheckForLeftDone = !isCheckForLeftDone;
                return diskSpaceNeededForLeft < freeDiskSpaceForRight;
            }
            return diskSpaceNeededForLeft < freeDiskSpaceForRight &&
                   diskSpaceNeededForRight < freeDiskSpaceForLeft;
        }

        /// <summary>
        /// This method is called when there are conflicting items during synchronization
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="args"></param>
        private static void OnItemConflicting(object sender, ItemConflictingEventArgs args)
        {
            // Currently, latest change wins
            args.SetResolutionAction(ConflictResolutionAction.SourceWins);
        }

        /// <summary>
        /// This method is called when there are constraint items during synchronization
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="args"></param>
        private static void OnItemConstraint(object sender, ItemConstraintEventArgs args)
        {
            args.SetResolutionAction(ConstraintConflictResolutionAction.SourceWins);
        }

        /// <summary>
        /// This method is called when changes are done to a file
        /// <para>Counts the number of changes already done by the sync framework</para>
        /// <para>Reports the progress percentage to the backgroundWorker</para>
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="args"></param>
        private void OnAppliedChange(object sender, AppliedChangeEventArgs args)
        {
            
            countDoneChanges++;
            // This method will raise an event to the backgroundWorker via backgroundWorker_ProgressChanged
            backgroundWorkerForSync.ReportProgress((int)((double)countDoneChanges / countChanges * 100));

            /*
            switch (args.ChangeType)
            {
                case ChangeType.Create:
                    _summary.InsertMsg("-- Applied CREATE for file " + args.NewFilePath);
                    break;
                case ChangeType.Delete:
                    _summary.InsertMsg("-- Applied DELETE for file " + args.OldFilePath);
                    break;
                case ChangeType.Update:
                    _summary.InsertMsg("-- Applied OVERWRITE for file " + args.OldFilePath);
                    break;
                case ChangeType.Rename:
                    _summary.InsertMsg("-- Applied RENAME for file " + args.OldFilePath +
                                      " as " + args.NewFilePath);
                    break;
            }
            */ 
        }

        /// <summary>
        /// This method is called when changes are going to be done to a file
        /// <para>Counts the number of changes to be made later during synchronization</para>
        /// <para>Counts the amount of disk space needed later during synchronization</para>
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="args"></param>
        private void OnApplyingChange(object sender, ApplyingChangeEventArgs args)
        {
            countChanges++;
            if (!isCheckForLeftDone)
                diskSpaceNeededForLeft += (ulong) args.NewFileData.Size;
            else
                diskSpaceNeededForRight += (ulong)args.NewFileData.Size;
        }

        /// <summary>
        /// This method is called when backgroundWorker is called to start working
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void backgroundWorker_DoWork(object sender, DoWorkEventArgs e)
        {
            System.ComponentModel.BackgroundWorker worker = sender as BackgroundWorker;
            // e.Result will be available to RunWorkerCompletedEventArgs later
            // when the work assigned is completed.
            e.Result = InternalStartSync();
        }

        /// <summary>
        /// This method is called when backgroundWorker2 is called to start working
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void backgroundWorker2_DoWork(object sender, DoWorkEventArgs e)
        {
            e.Result = InternalPreSync();
        }

        /// <summary>
        /// Starts the synchronization job
        /// </summary>
        /// <returns>Returns a boolean to indicate if the synchronization was successful</returns>
        private bool InternalStartSync()
        {
            try
            {
                // Configure sync options
                FileSyncOptions options = FileSyncOptions.ExplicitDetectChanges |
                    FileSyncOptions.RecycleConflictLoserFiles |
                    FileSyncOptions.RecycleDeletedFiles |
                    FileSyncOptions.RecyclePreviousFileOnUpdates;

                // Configure sync filters
                // This example will exclude shortcut links during sync
                FileSyncScopeFilter filter = new FileSyncScopeFilter();
                filter.FileNameExcludes.Add("*.lnk");

                // Update metadata of the folders before sync to
                // check for any changes or modifications
                DetectChangesonFileSystemReplica(leftPath, filter, options);
                DetectChangesonFileSystemReplica(rightPath, filter, options);

                // Start the 2-way sync
                SyncFileSystemReplicasOneWay(leftPath, rightPath, null, options, false);
                SyncFileSystemReplicasOneWay(rightPath, leftPath, null, options, false);

                return true;
            }
            catch
            {
                MessageBox.Show("Error"); // should call helper to show the message instead
                return false;
            }
        }


        /// <summary>
        /// Gets the stored folder paths in SyncEngine
        /// </summary>
        /// <returns>Returns an array of string which contains 2 folder paths</returns>
        public string[] GetPath()
        {
            string[] listOfPaths = new string[2];
            listOfPaths[0] = leftPath;
            listOfPaths[1] = rightPath;
            return listOfPaths;
        }

        /// <summary>
        /// Setter and Getter method for left folder path
        /// </summary>
        public string LeftPath
        {
            get { return leftPath; }
            set { leftPath = value; }
        }

        /// <summary>
        /// Setter and Getter method for right folder path
        /// </summary>
        public string RightPath
        {
            get { return rightPath; }
            set { rightPath = value; }
        }

        /// <summary>
        /// Gets backgroundWorker2 to do presync preparations
        /// </summary>
        public void PreSync()
        {
            backgroundWorkerForPreSync.RunWorkerAsync();
        }
        
        /// <summary>
        /// Does actual presync preparations
        /// </summary>
        /// <returns></returns>
        private bool InternalPreSync()
        {
            try
            {
                // Reset all counters before every synchronization
                countChanges = 0;
                countDoneChanges = 0;
                freeDiskSpaceForLeft = 0;
                freeDiskSpaceForRight = 0;
                diskSpaceNeededForLeft = 0;
                diskSpaceNeededForRight = 0;
                isCheckForLeftDone = false;

                // Configure sync options
                FileSyncOptions options = FileSyncOptions.ExplicitDetectChanges |
                    FileSyncOptions.RecycleConflictLoserFiles |
                    FileSyncOptions.RecycleDeletedFiles |
                    FileSyncOptions.RecyclePreviousFileOnUpdates;

                // Configure sync filters
                // Currently, this will exclude shortcut links during sync
                FileSyncScopeFilter filter = new FileSyncScopeFilter();
                filter.FileNameExcludes.Add("*.lnk");

                // Update metadata of the folders before sync to
                // check for any changes or modifications
                DetectChangesonFileSystemReplica(leftPath, filter, options);
                DetectChangesonFileSystemReplica(rightPath, filter, options);

                // Start the 2-way sync
                if (!SyncFileSystemReplicasOneWay(leftPath, rightPath, null, options, true))
                    return false;
                if (!SyncFileSystemReplicasOneWay(rightPath, leftPath, null, options, true))
                    return false;

                return true;
            }
            catch
            {
                MessageBox.Show("Error!"); // should ask helper to show error message instead
                return false;
            }
        }

        /// <summary>
        /// Get the real synchronization process to start
        /// </summary>
        public void StartSync()
        {
            // Start the asynchronous operation.
            backgroundWorkerForSync.RunWorkerAsync();
        }

        /// <summary>
        /// Asks IntelligentManager to check if a folder is subfolder of another folder
        /// </summary>
        /// <returns></returns>
        public bool CheckSubFolder()
        {
            return !intelligentManager.IsFolderSubFolder(leftPath, rightPath);
        }

        /// <summary>
        /// Checks if folder paths are already synchronized
        /// </summary>
        /// <returns>Return the result which indicates if folder paths are already synchronized</returns>
        public bool AreFoldersSync()
        {
            if (countChanges == 0)
                return true;
            else
                return false;
        }

        /// <summary>
        /// Asks IntelligentManager to check if the left or right folder path exists
        /// </summary>
        /// <param name="leftOrRight">This parameter indicates the left or right folder to be checked</param>
        /// <returns>Returns the result of the check in a boolean</returns>
        public bool CheckFolderExists(string leftOrRight)
        {
            if (leftOrRight == "left" || leftOrRight == "Left")
            {
                return intelligentManager.IsFolderExists(leftPath);
            }
            else if(leftOrRight == "right" || leftOrRight == "Right")
            {
                return intelligentManager.IsFolderExists(rightPath);
            }
            return false;
        }

        /// <summary>
        /// Asks IntelligentManager to check if the two folder paths are similar
        /// </summary>
        /// <returns>Returns the result of the check in a boolean</returns>
        public bool CheckSimilarFolder()
        {
            return intelligentManager.IsFoldersSimilar(leftPath, rightPath);
        }

        /// <summary>
        /// Ask IntelligentManager to check if a folder path is a removable drive
        /// </summary>
        /// <param name="path">This parameters indicates the folder path to be checked</param>
        /// <returns>Returns the result of the check in a boolean</returns>
        public bool CheckRemovableDrive(string path)
        {
            return intelligentManager.IsRemovableDrive(path);
        }
    }
}
