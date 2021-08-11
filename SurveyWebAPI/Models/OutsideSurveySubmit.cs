using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace SurveyWebAPI.Models
{
    public class OutsideSurveySubmit
    {
        public string SurveyId { get; set; }
        public string ReplyKey { get; set; }
        public int ProvideType { get; set; }
        public string ExtenField { get; set; }
        public string VerifyInfo { get; set; }
        public string ParameterInfo { get; set; }
        public string Device { get; set; }
        public string ForceEnd { get; set; }
        public string TimePeriod { get; set; }
        public string SubmitTime { get; set; }
        public int Env { get; set; }
        public Answer[] AnswerList { get; set; }
    }
    public class Answer
    {
        public int id { get; set; }
        public string ReplyKey { get; set; }
        public string QuestionId { get; set; }
        public string OptionId { get; set; }
        public string MatrixField { get; set; }
        public string BlankAnwer { get; set; }
    }
}
