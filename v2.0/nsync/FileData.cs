using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;

namespace nsync
{
    /// <summary>
    /// FileData Class
    /// </summary>
    public class FileData
    {
        #region Class Variables
        private string fileName;
        private string fileType;
        private string rootPath;
        private string relativePath;
        private bool isFolder;
        private Changes changeType;
        #endregion

        #region Constructor
        /// <summary>
        /// Constructor of FileData class
        /// </summary>
        public FileData(string rootPath, string fileName, string relativePath, Changes changeType, bool isFolder)
        {
            this.rootPath = rootPath;
            this.fileName = fileName;
            this.relativePath = relativePath;
            this.fileType = Path.GetExtension(fileName);
            this.changeType = changeType;
            this.isFolder = isFolder;
        }
        #endregion

        #region Properties
        /// <summary>
        /// Gets the FileName of FileData object
        /// </summary>
        public String FileName
        {
            get
            {
                return fileName;
            }
        }

        /// <summary>
        /// Gets the FileType of FileData object
        /// </summary>
        public String FileType
        {
            get
            {
                return fileType;
            }
        }

        /// <summary>
        /// Gets the ChangeType of FileData object
        /// </summary>
        public Changes ChangeType
        {
            get
            {
                return changeType;
            }
        }

        /// <summary>
        /// Gets the RootPath of FileData object
        /// </summary>
        public string RootPath
        {
            get
            {
                return rootPath;
            }
        }

        /// <summary>
        /// Gets the RelativePath of FileData object
        /// </summary>
        public String RelativePath
        {
            get
            {
                return relativePath;
            }
        }

        /// <summary>
        /// Checks whether fileData is a folder or a file
        /// </summary>
        public bool IsFolder
        {
            get
            {
                return isFolder;
            }
        }
        #endregion
    }

    #region Enum Variables
    /// <summary>
    /// Types Of Changes In FileData Object
    /// </summary>
    public enum Changes
    {
        /// <summary>
        /// Creation
        /// </summary>
        Create,
        /// <summary>
        /// Deletion
        /// </summary>
        Delete,
        /// <summary>
        /// Updating
        /// </summary>
        Update,
        /// <summary>
        /// Renaming
        /// </summary>
        Rename
    }
    #endregion
}
