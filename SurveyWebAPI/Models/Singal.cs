using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace SurveyWebAPI.Models
{
    public class Singal
    { 
        public string SurveyId { get; set; }
        public string QuestionId { get; set; }
        public int QuestionSeq { get; set; }
        public int QuestionType { get; set; }
        public string IsRequired { get; set; }
        public string HasOther { get; set; }
        public int PageNo { get; set; }
        public Main main { get; set; }
        public Advance advance { get; set; }
    }
    public class Main
    {
        public string QuestionSubject { get; set; }
        public string SubjectStyle { get; set; }
        public string QuestionNote { get; set; }
        public string QuestionImage { get; set; }
        public string QuestionVideo { get; set; }
        public Option[] option { get; set; }
        public Other other { get; set; }
    }
    public class Option
    {
        public string OptionId { get; set; }
        public int OptionSeq { get; set; }
        public int OptionType { get; set; }
        public string OptionContent { get; set; }
        public string ChildQuestionId { get; set; }
        public string OptionImage { get; set; }
        public string OptionVideo { get; set; }
        
    }
    public class Other
    {
        public string OtherIsShowText { get; set; }
        public int OtherVerify { get; set; }
        public string OtherMandatory { get; set; }
        public string OtherCheckMessage { get; set; }
        public int OtherMinLimit { get; set; }
        public int OtherMaxLimit { get; set; }
        public string OtherChildQuestionId { get; set; }
    }
    public class Advance
    {
        public ShowWay showWay { get; set; }
        public RandomI random { get; set; }
    }
    public class ShowWay
    {
        public string IsSetShowNum { get; set; }
        public int PCRowNum { get; set; }
        public int MobileRowNum { get; set; }
    }
    public class RandomI
    {
        public string IsRamdomOption { get; set; }
        public string ExcludeOther { get; set; }
    }
}
