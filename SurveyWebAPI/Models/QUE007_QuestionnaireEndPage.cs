using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace SurveyWebAPI.Models
{
    public class QUE007_QuestionnaireEndPage
    {
		/// <summary>
		/// 問卷ID  --uniqueidentifier
		/// </summary>
		public Object SurveyId { get; set; }
		/// <summary>
		/// 結束頁圖片 --nvarchar
		/// </summary>
		public Object EndPagePic { get; set; }
		/// <summary>
		/// 結束頁樣式   --nvarchar
		/// </summary>
		public Object EndPageStyle { get; set; }
		/// <summary>
		/// 按鈕文字  --nvarchar
		/// </summary>
		public Object ButtonSentence { get; set; }
		/// <summary>
		/// 轉導連結 --bit
		/// </summary>
		public Object EnableRedirect { get; set; }
		/// <summary>
		/// 轉導URL
		/// </summary>
		public Object RedirectUrl { get; set; }

        ///// <summary>
        ///// 建立人員
        ///// </summary>
        //public Object CreateUserId { get; set; }
        ///// <summary>
        ///// 建立時間
        ///// </summary>
        //public Object CreateDateTime { get; set; }
        /// <summary>
        /// 更改人員
        /// </summary>
        public Object UpdUserId { get; set; }
        /// <summary>
        /// 更改時間
        /// </summary>
        public Object UpdDateTime { get; set; }

    }
}
