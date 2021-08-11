using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace SurveyWebAPI.Models
{
    public class Logic
    {
        public string SurveyId { get; set; }

        public LogicDetail[] LogicList { get; set; }
    }
    public class LogicDetail
    {
        public string LogicId { get; set; }
        public int LogicType { get; set; }
        public int LogicCondition { get; set; }
        public string TargetQuestionId { get; set; }
        public string BlockOptionList { get; set; }
        public LogicCondition[] ConditionList { get; set; }
    }
    public class LogicCondition
    {
        public string ConditionQuestionList { get; set; }
        public string ConditionOptionList { get; set; }
        public int ConditionRule { get; set; }
        public string MatrixField { get; set; }
    }
}
