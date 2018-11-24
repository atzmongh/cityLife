using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace cityLife4
{
    public partial class ApartmentPhoto
    {
        /// <summary>
        /// If a thumbnail exists - returns its file path. Otherwise - returns the file path of the main photo. (that may also be used as a thumbnail)
        /// </summary>
        public string thumbnailOrFilePath
        {
            get
            {
                if(this.thumbnailPath != "")
                {
                    return this.thumbnailPath;
                }
                else
                {
                    return this.filePath;
                }
            }
        }
    }
}