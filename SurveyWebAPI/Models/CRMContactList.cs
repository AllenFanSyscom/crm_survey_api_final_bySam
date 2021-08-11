using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace SurveyWebApi.Models
{
    public class CRMContactList
    {
        public String code;
        public String message;
        public List<ContactList> data;
    }

    public class ContactList
    {
        public String ActivityId;

        public String ListName;
    }
    
}
