using Magnifyelshaddai.Models.EDMXModel;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace Magnifyelshaddai.Models
{
    public class BibleDictionaryVM
    {
        public BibleDictionary bibleDictionary { get; set; }
        public List<BibleDictionaryDownloadDetail> lstBibleDictionaryDownloadDetails { get; set; }
    }
}