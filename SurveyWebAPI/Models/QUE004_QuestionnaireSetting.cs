using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace SurveyWebAPI.Models
{
	public class QUE004_QuestionnaireSetting
	{
		public Object SurveyId { get; set; }
		public Object IsShowPageNo { get; set; }
		public Object IsShowQuestionNo { get; set; }
		public Object IsShowRequiredStar { get; set; }
		public Object IsShowProgress { get; set; }
		public Object PorgressPosition { get; set; }
		public Object ProgressStyle { get; set; }
		public Object UseVirifyCode { get; set; }
		public Object IsOneQuestionPerPage { get; set; }
		public Object IsPublishResult { get; set; }
		public Object IsShowEndPage { get; set; }
		/// <summary>
		/// 是否顯示選項編號
		/// </summary>
		public Object IsShowOptionNo { get; set; }
		//public Object UpdUserId { get; set; }
		//public Object UpdDateTime { get; set; }
	}

}
