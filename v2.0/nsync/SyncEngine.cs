using System;
using System.IO;
using System.Collections.Generic;
using System.Management;
using System.ComponentModel;
using System.Windows.Forms;
using Microsoft.Synchronization;
using Microsoft.Synchronization.Files;
using Microsoft.Synchronization.MetadataStorage;

namespace nsync
{
    class SyncEngine
    {
        #region Class Variables
        public System.ComponentModel.BackgroundWorker backgroundWorkerForSync;
        public System.ComponentModel.BackgroundWorker backgroundWorkerForPreSync;
        private bool isCheckForLeftDone = false;
        private ulong freeDiskSpaceForLeft = 0;
        private ulong freeDiskSpaceForRight = 0;
        private ulong diskSpaceNeededForLeft = 0;
        private ulong diskSpaceNeededForRight = 0;
        private string leftPath;
        private string rightPath;
        private static int countDoneChanges = 0;
        private static int countChanges = 0;
        private Intelligence intelligentManager;
        private List<string> excludeTypeList = new List<string>();
        private List<string> errorMessageForSummaryReport = new List<string>();
        private readonly string TRACKBACK_FOLDER_NAME = "_nsync_trackback";
        #endregion

        #region Properties
        public List<string> ErrorMessageForSummaryReport
        {
            get { return errorMessageForSummaryReport; }
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
        /// Setter and Getter method exclude list which contains file types
        /// </summary>
        public List<string> ExcludeTypeList
        {
            get { return excludeTypeList; }
            set { excludeTypeList = value; }
        }
        #endregion

        #region Public Methods
        /// <summary>
        /// Constructor for SyncEngine
        /// </summary>
        public SyncEngine()
        {
            // Set up the BackgroundWorker object by attaching event handlers.
            backgroundWorkerForSync = new System.ComponentModel.BackgroundWorker();
            backgroundWorkerForSync.DoWork += new DoWorkEventHandler(backgroundWorkerForSync_DoWork);
            backgroundWorkerForSync.WorkerReportsProgress = true;

            backgroundWorkerForPreSync = new System.ComponentModel.BackgroundWorker();
            backgroundWorkerForPreSync.DoWork += new DoWorkEventHandler(backgroundWorkerForPreSync_DoWork);
            backgroundWorkerForPreSync.WorkerReportsProgress = true;

            // Create the Intelligence object
            intelligentManager = new Intelligence();
        }

        /// <summary>
        /// Try to provide 2 new folder path (appropriate folder hierarchy), if possible
        /// </summary>
        /// <param name="targetPath">This parameter holds a string which contains the target folder path to be checked</param>
        /// <param name="lastSyncFolderPaths">This parameter holds a list of strings which contains the last sync folder paths</param>
        /// <param name="leftOrRight">This parameter indicates if the target path is leftPath or rightPath</param>
        /// <returns></returns>
        public string[] SyncToTheSameFolderHierarchy(string targetPath, string[] lastSyncFolderPaths, string leftOrRight)
        {
            if (lastSyncFolderPaths.Length == 0) // no folder pair in MRU
                return null;

            string oldTargetPath, oldNonTargetPath;
            if (leftOrRight == "left" || leftOrRight == "Left")
            {
                oldTargetPath = lastSyncFolderPaths[0];
                oldNonTargetPath = lastSyncFolderPaths[1];
            }
            else if (leftOrRight == "right" || leftOrRight == "right")
            {
                oldTargetPath = lastSyncFolderPaths[1];
                oldNonTargetPath = lastSyncFolderPaths[0];
            }
            else
                return null;

            // new target path is not subfolder of old path
            if (targetPath == oldTargetPath)
                return null;

            if (!(intelligentManager.IsFolderSubFolder(targetPath, oldTargetPath)))
                return null;

            if (!(intelligentManager.IsFolderExists(oldNonTargetPath + targetPath.Substring(oldTargetPath.Length))))
                return null;

            string[] finalFolderPaths = new string[2];
            if (leftOrRight == "left" || leftOrRight == "Left")
            {
                finalFolderPaths[0] = targetPath;
                finalFolderPaths[1] = oldNonTargetPath + targetPath.Substring(oldTargetPath.Length);
            }
            else if (leftOrRight == "right" || leftOrRight == "right")
            {
                finalFolderPaths[0] = oldNonTargetPath + targetPath.Substring(oldTargetPath.Length);
                finalFolderPaths[1] = targetPath;
            }
            return finalFolderPaths;
        }

        /// <summary>
        /// Try to get the serial number of removable disk if path is a removeable disk
        /// </summary>
        /// <param name="path">This parameter indicates the path to be checked</param>
        /// <returns>Returns a string which contains the serial number of the removeable disk, if it exists
        /// <para>Otherwise, return null</para></returns>
        public string GetRemovableDiskSerialNumberWithChecks(string path)
        {
            // check if the path is removable disk first
            if (!intelligentManager.IsRemovableDrive(path))
                return null;

            // check if the path is root (e.g. D:\ or F:\) 
            // bcos if path is F:\testfolder, users probably want to really sync that folder
            // don't try to outsmart the user
            if (!intelligentManager.IsPathRoot(path))
                return null;

            // check if the root has any files,
            // if there are files, users probably want to sync that file
            // don't try to outsmart the user
            if (intelligentManager.IsThereFilesInRootPath(path))
                return null;

            // if passed all the previous 2 checks, return the serial number of removeable disk
            return GetRemovableDiskSerialNumber(path);
        }

        /// <summary>
        /// Gets the removeable disk serial number
        /// </summary>
        /// <param name="path">This parameter indicates the path to be checked</param>
        /// <returns>Returns a string which contains the serial number of the removeable disk
        /// <para>Returns a null if path is not removeable disk</para></returns>
        public string GetRemovableDiskSerialNumber(string path)
        {
            return intelligentManager.FindRemoveableDiskSerialNumber(path);
        }

        /// <summary>
        /// Checks if a path is a removeable disk
        /// </summary>
        /// <param name="path">This parameter indicates the path to be checked</param>
        /// <returns>Returns a boolean which indicates if the path is a removeable disk</returns>
        public bool IsPathRemovableDisk(string path)
        {
            return intelligentManager.IsRemovableDrive(path);
        }

        /// <summary>
        /// Creates a folder in the root path and updates leftpath/rightpath, if required
        /// </summary>
        /// <returns>Returns a boolean to indicate if a folder was created</returns>
        public bool CreateFolderForRootPath()
        {
            if (IsRequiredToCreateFolder())
            {
                int leftOrRight; // 0 for left, 1 for right

                // Find the directory path to create the new folder
                string rootPath, sourcePath;
                if (intelligentManager.IsPathRoot(leftPath))
                {
                    leftOrRight = 0;
                    rootPath = leftPath;
                    sourcePath = rightPath;
                }
                else
                {
                    leftOrRight = 1;
                    rootPath = rightPath;
                    sourcePath = leftPath;
                }

                // If there are already files on the rootpath,
                // we assume that user is okay with having files flooding their rootpath
                // so we won't be creating a folder for them
                if (intelligentManager.IsThereFilesInRootPath(rootPath))
                {
                    return false;
                }

                rootPath += sourcePath.Substring(sourcePath.LastIndexOf("\\") + 1);

                // Create the folder in the root path
                try
                {
                    Directory.CreateDirectory(rootPath);
                }
                catch
                {
                    // If there is problem creating the folder, then forget it
                    return false;
                }

                // update the folder path in the synchronizer
                if (leftOrRight == 0) // left
                    leftPath = rootPath;
                else // right
                    rightPath = rootPath;

                return true;
            }
            else
            {
                // do nothing
                return false;
            }
        }

        /// <summary>
        /// Gets backgroundWorkerForPreSync to do presync preparations
        /// </summary>
        public void PreSync()
        {
            errorMessageForSummaryReport.Clear();
            backgroundWorkerForPreSync.RunWorkerAsync();
        }

        /// <summary>
        /// Get the real synchronization process to start
        /// </summary>
        public void StartSync()
        {
            backgroundWorkerForSync.RunWorkerAsync();
        }

        /// <summary>
        /// Checks if folder paths are already synchronized
        /// </summary>
        /// <returns>Return the result which indicates if folder paths are already synchronized</returns>
        public bool IsFoldersSync()
        {
            if (countChanges == 0)
                return true;
            else
                return false;
        }

        /// <summary>
        /// Asks IntelligentManager to check if a folder is subfolder of another folder
        /// </summary>
        /// <returns></returns>
        public bool IsFolderSubfolder()
        {
            return intelligentManager.IsFolderSubFolder(leftPath, rightPath);
        }

        /// <summary>
        /// Asks IntelligentManager to check if the left or right folder path exists
        /// </summary>
        /// <param name="leftOrRight">This parameter indicates the left or right folder to be checked</param>
        /// <returns>Returns the result of the check in a boolean</returns>
        public bool IsFolderExists(string leftOrRight)
        {
            if (leftOrRight == "left" || leftOrRight == "Left")
            {
                return intelligentManager.IsFolderExists(leftPath);
            }
            else if (leftOrRight == "right" || leftOrRight == "Right")
            {
                return intelligentManager.IsFolderExists(rightPath);
            }
            return false;
        }

        /// <summary>
        /// Asks IntelligentManager to check if the two folder paths are similar
        /// </summary>
        /// <returns>Returns the result of the check in a boolean</returns>
        public bool IsFoldersSimilar()
        {
            return intelligentManager.IsFoldersSimilar(leftPath, rightPath);
        }
        #endregion

        #region Private Methods
        /// <summary>
        /// Checks if it is required to create a folder on any root path
        /// </summary>
        /// <returns>Returns a boolean with the result of the query</returns>
        private bool IsRequiredToCreateFolder()
        {
            if ((intelligentManager.IsPathRoot(leftPath) && !intelligentManager.IsPathRoot(rightPath)) ||
                (!intelligentManager.IsPathRoot(leftPath) && intelligentManager.IsPathRoot(rightPath)))
                return true;
            else
                return false;
        }

        /// <summary>
        /// Computes the amount of free disk space of a disk drive
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

                if (isPreview)
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
        /// This method is called when there are conflicting items during synchronization
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="args"></param>
        private void OnItemConflicting(object sender, ItemConflictingEventArgs args)
        {
            IFileDataRetriever localfileDataRetriever = ((IFileDataRetriever)args.SourceChangeData);
            IFileDataRetriever remotefileDataRetriever = ((IFileDataRetriever)args.DestinationChangeData);
            //MessageBox.Show(localfileDataRetriever.FileData.RelativePath + "\n" + remotefileDataRetriever.FileData.RelativePath);
            if(localfileDataRetriever.FileData.RelativePath != remotefileDataRetriever.FileData.RelativePath)
            {
                if (!errorMessageForSummaryReport.Contains(localfileDataRetriever.AbsoluteSourceFilePath) && !errorMessageForSummaryReport.Contains(remotefileDataRetriever.AbsoluteSourceFilePath))
                {
                    errorMessageForSummaryReport.Add(localfileDataRetriever.AbsoluteSourceFilePath);
                    errorMessageForSummaryReport.Add(remotefileDataRetriever.AbsoluteSourceFilePath);
                }
            }
            
            // Latest change wins policy
            args.SetResolutionAction(ConflictResolutionAction.Merge);
        }

        /// <summary>
        /// This method is called when changes are done to a file
        /// <para>Counts the number of changes already done by the sync framework</para>
        /// <para>Reports the progress percentage to the backgroundWorkerForSync</para>
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="args"></param>
        private void OnAppliedChange(object sender, AppliedChangeEventArgs args)
        {
            
            countDoneChanges++;
            // This method will raise an event to the backgroundWorkerForSync via backgroundWorkerForSync_ProgressChanged
            backgroundWorkerForSync.ReportProgress((int)((double)countDoneChanges / countChanges * 100));
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
        /// This method is called when backgroundWorkerForSync is called to start working
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void backgroundWorkerForSync_DoWork(object sender, DoWorkEventArgs e)
        {
            // e.Result will be available to RunWorkerCompletedEventArgs later
            // when the job is completed.
            e.Result = InternalStartSync();
        }

        /// <summary>
        /// This method is called when backgroundWorkerForPreSync is called to start working
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void backgroundWorkerForPreSync_DoWork(object sender, DoWorkEventArgs e)
        {
                e.Result = InternalPreSync();
        }

        /// <summary>
        /// Does actual presync preparations
        /// </summary>
        /// <returns></returns>
        private int InternalPreSync()
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
                FileSyncScopeFilter filter = new FileSyncScopeFilter();

                filter.SubdirectoryExcludes.Add(TRACKBACK_FOLDER_NAME);
                
                for (int i = 0; i < excludeTypeList.Count; i++)
                {
                    filter.FileNameExcludes.Add("*" + excludeTypeList[i]);
                }

                // Update metadata of the folders before sync to
                // check for any changes or modifications
                DetectChangesonFileSystemReplica(leftPath, filter, options);
                DetectChangesonFileSystemReplica(rightPath, filter, options);

                // Start the 2-way sync
                // this one when return false, it means not enough disk space
                if (!SyncFileSystemReplicasOneWay(leftPath, rightPath, null, options, true))
                    return 1;
                if (!SyncFileSystemReplicasOneWay(rightPath, leftPath, null, options, true))
                    return 2;

                return 0;
            }
            catch (System.UnauthorizedAccessException)
            {
                return 3;
            }
            catch // catch other remaining unknown errors
            {
                return 4;
            }
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
                FileSyncScopeFilter filter = new FileSyncScopeFilter();

                filter.SubdirectoryExcludes.Add(TRACKBACK_FOLDER_NAME);

                for (int i = 0; i < excludeTypeList.Count; i++)
                {
                    filter.FileNameExcludes.Add("*" + excludeTypeList[i]);
                }

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
                return false;
            }
        }
        #endregion
    }
}