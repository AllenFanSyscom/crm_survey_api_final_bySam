using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace SurveyWebApi.Models
{
    public class CRMStatusInfo
    {
        public String code;
        public String message;
        public List<StatusInfo> data;
    }

    public class StatusInfo
    {
        public DateTime New_effectivestart;

        public DateTime New_effectiveend;

        public string New_statuscode;
    }
}
