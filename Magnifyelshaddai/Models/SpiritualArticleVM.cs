using Magnifyelshaddai.Models.EDMXModel;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace Magnifyelshaddai.Models
{
    public class SpiritualArticleVM
    {
        public SpiritualArticle spiritualArticle { get; set; }
        public List<SpiritualArticleDownloadDetail> lstSpiritualArticleDownloadDetails { get; set; }
    }
}