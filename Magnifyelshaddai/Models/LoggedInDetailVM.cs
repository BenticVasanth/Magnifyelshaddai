using Magnifyelshaddai.Models.EDMXModel;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace Magnifyelshaddai.Models
{
    public class LoggedInDetailVM
    {
        public LoggedInDetail loggedInDetail { get; set; }
        public List<LoggedInDetail> lstLoggedInDetails { get; set; }
    }
}