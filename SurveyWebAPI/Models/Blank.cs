using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace SurveyWebAPI.Models
{
    public class Blank
    { 
        public string SurveyId { get; set; }
        public string QuestionId { get; set; }
        public int QuestionSeq { get; set; }
        public int QuestionType { get; set; }
        public string IsRequired { get; set; }
        public int PageNo { get; set; }
        public MainB main { get; set; }
    }
    public class MainB
    {
        public string QuestionSubject { get; set; }
        public string SubjectStyle { get; set; }
        public string QuestionNote { get; set; }
        public string QuestionImage { get; set; }
        public string QuestionVideo { get; set; }
        public string BlankDefaultWords { get; set; }
        public int BlankValidType { get; set; }
        public string BlankMaxLimit { get; set; }
        public string BlankMinLimit { get; set; }
        
    }
}
