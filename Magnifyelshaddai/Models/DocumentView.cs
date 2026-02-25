using Magnifyelshaddai.Models.EDMXModel;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace Magnifyelshaddai.Models
{
    public class DocumentView
    {
        public IEnumerable<Document> lstSabbathDayDocuments { get; set; }
        public IEnumerable<LibraryIndex> lstLibraryBooks { get; set; }
        public IEnumerable<EBooksIndex> lstEBooks { get; set; }
        public IEnumerable<SpiritualArticle> lstSpiritualArticles { get; set; }
        public IEnumerable<PrayerRain> lstPrayerRains { get; set; }
        public IEnumerable<SinaiLetter> lstSinaiLetters { get; set; }
        public IEnumerable<PrayerNote> lstPrayerNotes { get; set; }
        public IEnumerable<MarianBook> lstMarianBooks { get; set; }
        public IEnumerable<MarianStudy> lstMarianStudies { get; set; }
        public IEnumerable<StudyBible> lstStudyBibles { get; set; }
        public IEnumerable<BibleCommentary> lstBibleCommentaries { get; set; }
        public IEnumerable<BiblicalTheology> lstBiblicalTheologies { get; set; }
        public IEnumerable<BibleDictionary> lstBibleDictionaries { get; set; }
    }
}