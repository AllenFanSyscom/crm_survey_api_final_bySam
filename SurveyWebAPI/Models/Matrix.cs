using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace SurveyWebAPI.Models
{
    public class Matrix
    { 
        public string SurveyId { get; set; }
        public string QuestionId { get; set; }
        public int QuestionSeq { get; set; }
        public int QuestionType { get; set; }
        public string IsRequired { get; set; }
        public int PageNo { get; set; }
        public MainM main { get; set; }
    }
    public class MainM
    {
        public string QuestionSubject { get; set; }
        public string SubjectStyle { get; set; }
        public string QuestionNote { get; set; }
        public string QuestionImage { get; set; }
        public string QuestionVideo { get; set; }
        public string MatrixItems { get; set; }
        public OptionM[] option { get; set; }
    }
    public class OptionM
    {
        public string OptionId { get; set; }
        public int OptionSeq { get; set; }
        public int OptionType { get; set; }
        public string OptionContent { get; set; }
        public string ChildQuestionId { get; set; }

    }
}
