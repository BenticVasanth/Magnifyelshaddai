using Magnifyelshaddai.Models.EDMXModel;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace Magnifyelshaddai.Models
{
    public class PrayerNoteVM
    {
        public PrayerNote prayerNote { get; set; }
        public List<PrayerNoteDownloadDetail> lstPrayerNoteDownloadDetails { get; set; }
    }
}