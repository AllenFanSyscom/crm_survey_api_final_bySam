using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace SurveyWebAPI.Models
{
    /// <summary>
    /// 問卷主表
    /// </summary>
    public class QUE001_QuestionnaireBase
    {
        /// <summary>
        /// 問卷ID  uniqueidentifier
        /// </summary>
        public Object SurveyId { get; set; }
        /// <summary>
        /// 問卷名稱 nvarchar
        /// </summary>
        public Object Title { get; set; }
        /// <summary>
        /// 正式網址 nvarchar
        /// </summary>
        public Object FinalUrl { get; set; }
        /// <summary>
        /// 感謝詞 nvarchar
        /// </summary>
        public Object ThankWords { get; set; }
        /// <summary>
        /// 結束動作 int 
        /// </summary>
        public Object DueAction { get; set; }
        /// <summary>
        /// 刪除註記 bit
        /// </summary>
        public Object DelFlag { get; set; }
        /// <summary>
        /// 是否記名 bit
        /// </summary>
        public Object Audit { get; set; }
        /// <summary>
        /// 建立人員 uniqueidentifier
        /// </summary>
        public Object CreateUserId { get; set; }
        /// <summary>
        /// 建立時間 datetime2
        /// </summary>
        public Object CreateDateTime { get; set; }
        /// <summary>
        /// 更新人員 uniqueidentifier
        /// </summary>
        public Object UpdUserId { get; set; }
        /// <summary>
        /// 更新日期時間 datetime2
        /// </summary>
        public Object UpdDateTime { get; set; }
        /// <summary>
        /// 外觀類型 int
        /// </summary>
        public Object StyleType { get; set; }
        /// <summary>
        /// 問卷底色 varchar
        /// </summary>
        public Object DefBackgroudColor { get; set; }
        /// <summary>
        /// 問卷表頭圖片 varchar
        /// </summary>
        public Object DefHeaderPic { get; set; }
        /// <summary>
        /// 手機版表頭圖片 varchar
        /// </summary>
        public Object DefHeaderPhonePic { get; set; }
    }
}
