using Magnifyelshaddai.Models.EDMXModel;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace Magnifyelshaddai.Models
{
    public class BibleCommentaryVM
    {
        public BibleCommentary bibleCommentary { get; set; }
        public List<BibleCommentaryDownloadDetail> lstBibleCommentaryDownloadDetails { get; set; }
    }
}