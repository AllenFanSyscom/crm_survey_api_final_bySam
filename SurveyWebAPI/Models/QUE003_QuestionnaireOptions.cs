using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace SurveyWebAPI.Models
{
    /// <summary>
    /// 問卷選項
    /// </summary>
    public class QUE003_QuestionnaireOptions
    {
        /// <summary>
        /// 題目ID uniqueidentifier
        /// </summary>
        public Object QuestionId { get; set; }
        /// <summary>
        /// 選項ID uniqueidentifier
        /// </summary>
        public Object OptionId { get; set; }
        /// <summary>
        /// 選項順序 int
        /// </summary>
        public Object OptionSeq { get; set; }
        /// <summary>
        /// 選項類型 int
        /// </summary>
        public Object OptionType { get; set; }
        /// <summary>
        /// 選項內容 nvarchar
        /// </summary>
        public Object OptionContent { get; set; }
        /// <summary>
        /// 子題題目ID uniqueidentifier
        /// </summary>
        public Object ChildQuestionId { get; set; }
        /// <summary>
        /// 更新人員 uniqueidentifier
        /// </summary>
        public Object UpdUserId { get; set; }
        /// <summary>
        /// 更新日期時間 datetime2
        /// </summary>
        public Object UpdDateTime { get; set; }
        /// <summary>
        /// 選項圖片
        /// </summary>
        public Object OptionImage { get; set; }
        /// <summary>
        /// 選項影片
        /// </summary>
        public Object OptionVideo { get; set; }
        /// <summary>
        /// 是否其他選項
        /// </summary>
        public Object OtherFlag { get; set; }
    }
}
