using Magnifyelshaddai.Models.EDMXModel;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace Magnifyelshaddai.Models
{
    public class SabbathDayDocumentVM
    {
        public Document sabbathDayDocument { get; set; }
        public List<DownloadDetail> lstSabbathDayDocumentsDownloadDetails { get; set; }
    }
}