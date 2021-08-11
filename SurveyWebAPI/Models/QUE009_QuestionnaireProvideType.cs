using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace SurveyWebAPI.Models
{
    public class QUE009_QuestionnaireProvideType
    {
		/// <summary>
		/// 問卷ID  --uniqueidentifier
		/// </summary>
		public Object SurveyId { get; set; }
		/// <summary>
		/// 收集管道 --int
		/// </summary>
		public Object ProvideType { get; set; }
		/// <summary>
		/// 正式網址   --nvarchar
		/// </summary>
		public Object FinalUrl { get; set; }
		/// <summary>
		/// 問卷填寫上限  --int
		/// </summary>
		public Object ReplyMaxNum { get; set; }
		/// <summary>
		/// 重複填寫機制 --int
		/// </summary>
		public Object MultiProvideType { get; set; }
		/// <summary>
		/// 記名驗證方式  --int
		/// </summary>
		public Object ValidRegister { get; set; }
		/// <summary>
		/// 滿額註記   --bit
		/// </summary>
		public Object FullEndFlag { get; set; }
		/// <summary>
		/// 建立人員
		/// </summary>
		public Object CreateUserId { get; set; }
		/// <summary>
		/// 建立時間
		/// </summary>
		public Object CreateDateTime { get; set; }
		/// <summary>
		/// 更改人員
		/// </summary>
		public Object UpdUserId { get; set; }
		/// <summary>
		/// 更改時間
		/// </summary>
		public Object UpdDateTime { get; set; }
		/// <summary>
		/// 測試網址   --nvarchar
		/// </summary>
		public Object TestUrl { get; set; }
		/// <summary>
		/// 驗證欄位
		/// </summary>
		public Object ValidField { get; set; }
		/// <summary>
		/// 填寫次數
		/// </summary>
		public Object ReplyNum { get; set; }
	}
}
