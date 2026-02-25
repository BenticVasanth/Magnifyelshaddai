using Magnifyelshaddai.Models.EDMXModel;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace Magnifyelshaddai.Models
{
    public class BiblicalTheologyVM
    {
        public BiblicalTheology biblicalTheology { get; set; }
        public List<BiblicalTheologyDownloadDetail> lstBiblicalTheologyDownloadDetails { get; set; }
    }
}