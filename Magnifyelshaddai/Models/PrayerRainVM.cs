using Magnifyelshaddai.Models.EDMXModel;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace Magnifyelshaddai.Models
{
    public class PrayerRainVM
    {
        public PrayerRain prayerRain { get; set; }
        public List<PrayerRainDownloadDetail> lstPrayerRainDownloadDetails { get; set; }
    }
}