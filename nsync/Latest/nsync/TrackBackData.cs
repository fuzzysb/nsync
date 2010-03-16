using System;
using System.IO;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace nsync
{
    class TrackBackData
    {
        ////////////////////
        // CLASS VARIABLES
        ////////////////////

        private DirectoryInfo sourceDirectory, destinationDirectory;
        private string dateCreated, timeCreated;
        private string timeStamp;

        ////////////////////
        // CONSTRUCTOR
        ////////////////////

        public TrackBackData(string srcPath, string desPath)
        {
            sourceDirectory = new DirectoryInfo(srcPath);
            destinationDirectory = new DirectoryInfo(desPath);

            dateCreated = DateTime.Now.ToString("yyyy-MM-dd");
            timeCreated = DateTime.Now.ToString("hh.mm.ss tt");

            timeStamp = dateCreated + " " + timeCreated;
        }

        ////////////////////
        // PROPERTIES
        ////////////////////

        public string TimeStamp
        {
            get { return timeStamp; }
        }

        public string DateCreated
        {
            get { return dateCreated; }
        }

        public string TimeCreated
        {
            get { return timeCreated; }
        }

        public DirectoryInfo SourceDirectory
        {
            get { return sourceDirectory; }
        }

        public DirectoryInfo DestinationDirectory
        {
            get { return destinationDirectory; }
        }
    }
}
