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
        private Changes changeType;
        #endregion

        #region Public Methods
        /// <summary>
        /// Constructor of FileData class
        /// </summary>
        public FileData(string rootPath, string fileName, Changes changeType)
        {
            this.rootPath = rootPath;
            this.fileName = fileName;
            this.fileType = Path.GetExtension(fileName);
            this.changeType = changeType;
        }

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
        #endregion
    }

    /// <summary>
    /// Types Of Changes In FileData Object
    /// </summary>
    public enum Changes
    {
        Create,
        Delete,
        Update,
        Rename
    }
}
