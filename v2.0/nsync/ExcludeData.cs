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
        private List<string> excludeFolderList;
        private List<string> excludeFileNameList;
        private List<string> excludeFileTypeList;
        #endregion

        #region Constructor
        /// <summary>
        /// Constructor of ExcludeData class
        /// </summary>
        public ExcludeData()
        {
            excludeFolderList = new List<string>();
            excludeFileNameList = new List<string>();
            excludeFileTypeList = new List<string>();
        }
        #endregion

        #region Public Methods
        /// <summary>
        /// Setter and Getter method exclude list which contains file types
        /// </summary>
        public List<string> ExcludeFileTypeList
        {
            get { return excludeFileTypeList; }
            set { excludeFileTypeList = value; }
        }

        /// <summary>
        /// Setter and Getter method exclude list which contains file names
        /// </summary>
        public List<string> ExcludeFileNameList
        {
            get { return excludeFileNameList; }
            set { excludeFileNameList = value; }
        }

        /// <summary>
        /// Setter and Getter method exclude list which contains folder
        /// </summary>
        public List<string> ExcludeFolderList
        {
            get { return excludeFolderList; }
            set { excludeFolderList = value; }
        }
        #endregion
    }
}
