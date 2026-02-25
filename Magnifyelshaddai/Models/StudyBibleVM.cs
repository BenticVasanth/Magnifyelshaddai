using Magnifyelshaddai.Models.EDMXModel;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace Magnifyelshaddai.Models
{
    public class StudyBibleVM
    {
        public StudyBible studyBible { get; set; }
        public List<StudyBibleDownloadDetail> lstStudyBibleDownloadDetails { get; set; }
    }
}