using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace SurveyWebApi.Models
{
    public class CRMCampActEBDateTime
    {
        public String code;
        public String message;
        public List<CampActEBDateTime> data;
    }

    public class CampActEBDateTime
    {
        public String SysDateTime;

        public DateTime New_effectivestart;

        public DateTime New_effectiveend;

        public int New_statuscode;

        public int statuscode;
    }
}
