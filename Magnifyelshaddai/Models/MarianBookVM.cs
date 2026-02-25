using Magnifyelshaddai.Models.EDMXModel;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace Magnifyelshaddai.Models
{
    public class MarianBookVM
    {
        public MarianBook marianBook { get; set; }
        public List<MarianBookDownloadDetail> lstMarianBookDownloadDetails { get; set; }
    }
}