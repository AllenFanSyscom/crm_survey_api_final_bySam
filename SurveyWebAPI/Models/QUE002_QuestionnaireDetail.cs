using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace SurveyWebAPI.Models
{
    public class QUE002_QuestionnaireDetail
    {
		/// <summary>
		/// 問卷ID uniqueidentifier
		/// </summary>
		public Object SurveyId { get; set; }
		/// <summary>
		/// 題目ID uniqueidentifier
		/// </summary>
		public Object QuestionId { get; set; }
		/// <summary>
		/// 題目順序 int
		/// </summary>
		public Object QuestionSeq { get; set; }
		/// <summary>
		/// 題目類型 int, 對應AllCode.0100
		/// </summary>
		public Object QuestionType { get; set; }
		/// <summary>
		/// 題目標題 nvarchar
		/// </summary>
		public Object QuestionSubject { get; set; }
		/// <summary>
		/// 標題樣式 nvarchar
		/// </summary>
		public Object SubjectStyle { get; set; }
		/// <summary>
		/// 題目備註 nvarchar
		/// </summary>
		public Object QuestionNote { get; set; }
		/// <summary>
		/// 頁碼 int
		/// </summary>
		public Object PageNo { get; set; }
		/// <summary>
		/// 是否必填 bit
		/// </summary>
		public Object IsRequired { get; set; }
		/// <summary>
		/// 是否新增【其他】選項 bit
		/// </summary>
		public Object HasOther { get; set; }
		/// <summary>
		/// 是否顯示【其他】選項的填寫框 bit
		/// </summary>
		public Object OtherIsShowText { get; set; }
		/// <summary>
		/// 驗證【其他】填寫框的值 int
		/// </summary>
		public Object OtherVerify { get; set; }
		/// <summary>
		/// 是否驗證【其他】填寫框的內容 bit
		/// </summary>
		public Object OtherTextMandatory { get; set; }
		/// <summary>
		/// 【其他】填寫框驗證提示訊息 nvarchar
		/// </summary>
		public Object OtherCheckMessage { get; set; }
		/// <summary>
		/// 是否設定選項排版數量 bit
		/// </summary>
		public Object IsSetShowNum { get; set; }
		/// <summary>
		/// PC選項排版數量 int
		/// </summary>
		public Object PCRowNum { get; set; }
		/// <summary>
		/// 手機選項排版數量 int
		/// </summary>
		public Object MobileRowNum { get; set; }
		/// <summary>
		/// 是否隨機排序 bit
		/// </summary>
		public Object IsRamdomOption { get; set; }
		/// <summary>
		/// 是否排除【其他】進行排序 bit
		/// </summary>
		public Object ExcludeOther { get; set; }
		/// <summary>
		/// 基本資料驗證方式 int
		/// </summary>
		public Object BaseDataValidType { get; set; }
		/// <summary>
		/// 填空預設文字 nvarchar
		/// </summary>
		public Object BlankDefaultWords { get; set; }
		/// <summary>
		/// 填空驗證方式 int
		/// </summary>
		public Object BlankValidType { get; set; }
		/// <summary>
		/// 矩陣選項，用逗號隔開 nvarchar
		/// </summary>
		public Object MatrixItems { get; set; }
		/// <summary>
		/// 填空題上限 nvarchar
		/// </summary>
		public Object BlankMaxLimit { get; set; }
		/// <summary>
		/// 填空題下限 nvarchar
		/// </summary>
		public Object BlankMinLimit { get; set; }
		/// <summary>
		/// 題目圖片 varchar
		/// </summary>
		public Object QuestionImage { get; set; }
		/// <summary>
		/// 題目影片 varchar
		/// </summary>
		public Object QuestionVideo { get; set; }
		/// <summary>
		/// 複選選項數量限制 varchar
		/// </summary>
		public Object MultiOptionLimit { get; set; }

		//public Object UpdUserId { get; set; }
		//public Object UpdDateTime { get; set; }
	}
}
