using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace SurveyWebApi.Models
{
    public class CRMContactInfo
    {
        public String code;
        public String message;
        public List<ContactInfo> data;
    }

    public class ContactInfo
    {
        public int ContactCount;
    }
}
