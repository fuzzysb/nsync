using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;

namespace nsync
{
    /// <summary>
    /// ExcludeData Class
    /// </summary>
    class ExcludeData
    {
        #region Class Variables
        private List<string> excludeFolders;
        private List<string> excludeFileNames;
        private List<string> excludeFileTypes;
        #endregion

        #region Constructor
        /// <summary>
        /// Constructor of ExcludeData class
        /// </summary>
        public ExcludeData(List<string> excludeFolders, List<string> excludeFileNames, List<string> excludeFiletypes)
        {
            this.excludeFolders = excludeFolders;
            this.excludeFileNames = excludeFileNames;
            this.excludeFileTypes = excludeFileTypes;
        }
        #endregion

        #region Public Methods
        /// <summary>
        /// Function to return list of File Types in Exclude Box
        /// </summary>
        /// <returns>List of File Types to be excluded in sync</returns>
        public List<string> GetFileTypeList()
        {
            return excludeFileTypes;
        }

        /// <summary>
        /// Function to return list of File Names in Exclude Box
        /// </summary>
        /// <returns>List of File Names to be excluded in sync</returns>
        public List<string> GetFileNameList()
        {
            return excludeFileNames;
        }

        /// <summary>
        /// Function to return list of Folders in Exclude Box
        /// </summary>
        /// <returns>List of Folders to be excluded in sync</returns>
        public List<string> GetFolderList()
        {
            return excludeFolders;
        }
        #endregion
    }
}
