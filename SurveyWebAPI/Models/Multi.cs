using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace SurveyWebAPI.Models
{
    public class Multi
    { 
        public string SurveyId { get; set; }
        public string QuestionId { get; set; }
        public int QuestionSeq { get; set; }
        public int QuestionType { get; set; }
        public string IsRequired { get; set; }
        public string HasOther { get; set; }
        public int PageNo { get; set; }
        public Main main { get; set; }
        public AdvanceM advance { get; set; }
    }
    public class AdvanceM
    {
        public ShowWay showWay { get; set; }
        public RandomI random { get; set; }
        public string MultiOptionLimit { get; set; }
    }
}
