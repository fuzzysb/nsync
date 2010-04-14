// Norman

using System;
using System.Collections.Generic;
using System.IO;
using System.Management;
using System.Linq;
using System.Text;
using System.ComponentModel;
using Microsoft.Synchronization;
using Microsoft.Synchronization.Files;
using Microsoft.Synchronization.MetadataStorage;
using System.Windows;

namespace nsync
{
    //CLASS FOR PREVIEW
    public class Preview
    {
        #region Class Variables
        public System.ComponentModel.BackgroundWorker backgroundWorkerForPreview;
        public System.ComponentModel.BackgroundWorker backgroundWorkerForSummary;
        private List<FileData> fileData;
        private ExcludeData excludeData;
        private string leftPath;
        private string rightPath;

        private string METADATA_FILE_NAME = nsync.Properties.Resources.metadataFileName;
        private readonly string TRACKBACK_FOLDER_NAME = nsync.Properties.Resources.trackBackFolderName;
        #endregion

        #region Constructor
        /// <summary>
        /// Constructor for Preview
        /// </summary>
        public Preview()
        {
            // Attaching event handlers for preview
            backgroundWorkerForPreview = new System.ComponentModel.BackgroundWorker();
            backgroundWorkerForPreview.DoWork += new DoWorkEventHandler(backgroundWorkerForPreview_DoWork);
            backgroundWorkerForPreview.WorkerReportsProgress = true;

            // Attaching event handlers for summary
            backgroundWorkerForSummary = new System.ComponentModel.BackgroundWorker();
            backgroundWorkerForSummary.DoWork += new DoWorkEventHandler(backgroundWorkerForSummary_DoWork);
            backgroundWorkerForSummary.WorkerReportsProgress = true;
        }
        #endregion

        #region Properties
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
        /// Setter method for filters in ExcludeData
        /// </summary>
        public ExcludeData ExcludeData
        {
            set
            {
                excludeData = new ExcludeData();
                excludeData = value;
            }
        }
        #endregion

        #region Public Methods
        /// <summary>
        /// Gets backgroundWorkerForPreview to do synchronization preparations
        /// </summary>
        public void PreviewSync()
        {
            // Start the asynchronous operation.
            backgroundWorkerForPreview.RunWorkerAsync();
        }

        /// <summary>
        /// Gets backgroundWorkerForSummary to do synchronization preparations
        /// </summary>
        public void SummarySync()
        {
            // Start the asynchronous operation.
            backgroundWorkerForSummary.RunWorkerAsync();
        }

        /// <summary>
        /// Does sync operation and returns list of file data
        /// </summary>
        /// <returns>Returns a list of file data objects</returns>
        public List<FileData> GetData()
        {
            return fileData;
        }
        #endregion

        #region Background Worker Methods
        /// <summary>
        /// This method is called when backgroundWorkerForPreview is called to start working
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void backgroundWorkerForPreview_DoWork(object sender, DoWorkEventArgs e)
        {
            e.Result = InternalPreviewSync();
        }

        /// <summary>
        /// This method is called when backgroundWorkerForSummary is called to start working
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void backgroundWorkerForSummary_DoWork(object sender, DoWorkEventArgs e)
        {
            e.Result = InternalPreviewSync();
        }
        #endregion

        #region Sync Engine Mechanics
        /// <summary>
        /// Does Sync operation to store change events into a list of FileData objects
        /// </summary>
        private int InternalPreviewSync()
        {
            try
            {
                fileData = new List<FileData>();

                // Configure sync options
                FileSyncOptions options = FileSyncOptions.ExplicitDetectChanges |
                    FileSyncOptions.RecycleConflictLoserFiles |
                    FileSyncOptions.RecycleDeletedFiles |
                    FileSyncOptions.RecyclePreviousFileOnUpdates;


                // Configure sync filters
                FileSyncScopeFilter filter = new FileSyncScopeFilter();
                filter.SubdirectoryExcludes.Add(TRACKBACK_FOLDER_NAME);
                filter.FileNameExcludes.Add(METADATA_FILE_NAME);

                // Add filters for file types
                for (int i = 0; i < excludeData.ExcludeFileTypeList.Count; i++)
                {
                    filter.FileNameExcludes.Add("*" + excludeData.ExcludeFileTypeList[i]);
                }

                // Add filters for file names
                for (int i = 0; i < excludeData.ExcludeFileNameList.Count; i++)
                {
                    filter.FileNameExcludes.Add(excludeData.ExcludeFileNameList[i]);
                }

                // Add filters for folders
                for (int i = 0; i < excludeData.ExcludeFolderList.Count; i++)
                {
                    filter.SubdirectoryExcludes.Add(excludeData.ExcludeFolderList[i]);
                }

                // Update metadata of the folders before sync to
                // check for any changes or modifications
                DetectChangesonFileSystemReplica(leftPath, filter, options);
                DetectChangesonFileSystemReplica(rightPath, filter, options);

                // Start the 2-way sync
                SyncFileSystemReplicasOneWay(leftPath, rightPath, null, options);
                SyncFileSystemReplicasOneWay(rightPath, leftPath, null, options);
                return 0;
            }
            catch (UnauthorizedAccessException)
            {
                return 1;
            }
            catch (MetadataStorageEngineException)
            {
                return 2;
            }
            catch (MetadataStoreException)
            {
                return 3;
            }
            catch (Exception)
            {
                return 4;
            }
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
        private void SyncFileSystemReplicasOneWay(string sourcePath, string destPath,
        FileSyncScopeFilter filter, FileSyncOptions options)
        {
            FileSyncProvider sourceProvider = null;
            FileSyncProvider destProvider = null;

            try
            {
                sourceProvider = new FileSyncProvider(sourcePath, filter, options);
                destProvider = new FileSyncProvider(destPath, filter, options);

                sourceProvider.PreviewMode = true;
                destProvider.PreviewMode = true;


                destProvider.Configuration.ConflictResolutionPolicy = ConflictResolutionPolicy.ApplicationDefined;
                SyncCallbacks destinationCallBacks = destProvider.DestinationCallbacks;
                destinationCallBacks.ItemConflicting += new EventHandler<ItemConflictingEventArgs>(OnItemConflicting);
                destinationCallBacks.ItemConstraint += new EventHandler<ItemConstraintEventArgs>(OnItemConstraint);


                destProvider.ApplyingChange += new EventHandler<ApplyingChangeEventArgs>(OnApplyingChange);


                SyncOrchestrator agent = new SyncOrchestrator();
                agent.LocalProvider = sourceProvider;
                agent.RemoteProvider = destProvider;
                agent.Direction = SyncDirectionOrder.Upload;

                agent.Synchronize();
            }
            finally
            {
                if (sourceProvider != null) sourceProvider.Dispose();
                if (destProvider != null) destProvider.Dispose();
            }
        }

        /// <summary>
        /// This method is called when changes are going to be done to a file
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="args"></param>
        private void OnApplyingChange(object sender, ApplyingChangeEventArgs args)
        {
            FileSyncProvider provider = null;
            string rootPath = null;
            try
            {
                provider = (FileSyncProvider)sender;
                rootPath = provider.RootDirectoryPath;
                switch (args.ChangeType)
                {
                    case ChangeType.Delete:
                        fileData.Add(new FileData(rootPath, args.CurrentFileData.Name, args.CurrentFileData.RelativePath, Changes.Delete,
                            args.CurrentFileData.IsDirectory));
                        break;

                    case ChangeType.Create:
                        fileData.Add(new FileData(rootPath, args.NewFileData.Name, args.NewFileData.RelativePath, Changes.Create,
                            args.NewFileData.IsDirectory));
                        break;

                    case ChangeType.Update:
                        fileData.Add(new FileData(rootPath, args.NewFileData.Name, args.NewFileData.RelativePath, Changes.Update,
                            args.NewFileData.IsDirectory));
                        break;
                    case ChangeType.Rename:
                        fileData.Add(new FileData(rootPath, args.NewFileData.Name, args.NewFileData.RelativePath, Changes.Rename,
                            args.NewFileData.IsDirectory));
                        break;
                }
            }
            finally
            {
                provider.Dispose();
            }
        }

        /// <summary>
        /// This method is called when there are conflicting items during synchronization
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="args"></param>
        private static void OnItemConflicting(object sender, ItemConflictingEventArgs args)
        {
            // Currently, latest change wins
            args.SetResolutionAction(ConflictResolutionAction.Merge);
        }

        /// <summary>
        /// This method is called when there are constraint items during synchronization
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="args"></param>
        private static void OnItemConstraint(object sender, ItemConstraintEventArgs args)
        {
            args.SetResolutionAction(ConstraintConflictResolutionAction.Merge);
        }
        #endregion
    }
}
