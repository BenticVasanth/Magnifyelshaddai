using Magnifyelshaddai.Models.EDMXModel;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace Magnifyelshaddai.Models
{
    public class MarianStudyVM
    {
        public MarianStudy marianStudy { get; set; }
        public List<MarianStudyDownloadDetail> lstMarianStudyDownloadDetails { get; set; }
    }
}