using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using System.Linq;
using System.Threading.Tasks;
using Common;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using SurveyWebAPI.Models;
using SurveyWebAPI.Utility;

namespace SurveyWebAPI.Controllers
{
    [Authorize]
    [Route("api/Survey/Detail")]
    [ApiController]
    public class SurveyDetailController : ControllerBase
    {
        /// <summary>
        /// 問卷系統後臺之主畫面操作--問卷題型明細
        /// </summary>
        /// <designer>Allen/Gem</designer>
        private DBHelper _db;
        public SurveyDetailController()
        {
            _db = new DBHelper(AppSettingsHelper.DefaultConnectionString);
        }

        #region 問卷題型明細
        /// <summary>
        /// 問卷題型明細
        /// </summary>
        /// <param name="value"></param>
        /// <returns></returns>
        [Route("QueryByPage")]
        [HttpGet]
        public String QueryByPage(String SurveyId, String OtherFlag)
        {
            //輸入：
            /* "SurveyId":1231,
             * ["OtherFlag]:false
            */
            //"Newtonsoft.Json.Linq.JArray"
            //"Newtonsoft.Json.Linq.JObject"
            //多筆資料的話，此處需要處理，暫不管
            //if(value.GetType().Name=="JArray")
            //{
            //    foreach (var val in value as JArray)
            //    {
            //        InsertOne(val);
            //    }
            //}

            //JObject jo = (JObject)JsonConvert.DeserializeObject(value.ToString());
            var replyData = new ReplyData();
            //SurveyId 必須有?
            if (SurveyId == null)
            {
                //報告錯誤
                replyData.code = "-1";
                replyData.message = $"參數SurveyId不能為空！";
                replyData.data = "";
                Log.Error("問卷題型明細參數SurveyId不能為空！");
                return JsonConvert.SerializeObject(replyData);
            }
            //OtherFlag： 有傳入且value=false時，不查詢“其他”，即只取QUE003中OtherFlag=false的資料；沒有傳入或True,全取（不過濾）
            bool ExcludeOther = false;      //是否排除“其他”選項
            if (OtherFlag != null && Convert.ToBoolean(OtherFlag) == false)
            {
                //OtherFlag有輸入，並且輸入了false，意思是排除“其他”，也就是需要排除QUE003.OtherFlag='true'的資料
                ExcludeOther = true;
                Log.Debug($"api/Survey/Detail/QueryByPage:{SurveyId}排除[其他]選項");
            }
            else
            {
                ExcludeOther = false;
                Log.Debug($"api/Survey/Detail/QueryByPage:{SurveyId}包含[其他]選項");
            }
            //SurveyId 必須有?
            //if (jo["SurveyId"] == null)
            //{
            //    //報告錯誤
            //    replyData.code = "-1";
            //    replyData.message = $"參數SurveyId不能為空！";
            //    replyData.data = "";
            //    Log.Error("問卷題型明細參數SurveyId不能為空！");
            //    return JsonConvert.SerializeObject(replyData);
            //}
            //var SurveyId = jo["SurveyId"].ToString();
            string ExcludeOtherSql = "";
            if (ExcludeOther)  //需要排除掉“其他”
                ExcludeOtherSql = " AND (C.OtherFlag!='true' OR C.OtherFlag IS NULL ) ";
            string sSql = "SELECT A.*, B.*, C.* " +
                    " FROM QUE001_QuestionnaireBase A" +
                    " LEFT JOIN QUE002_QuestionnaireDetail B ON B.SurveyId=A.SurveyId " +
                    " LEFT JOIN QUE003_QuestionnaireOptions C ON C.QuestionId=B.QuestionId " + ExcludeOtherSql +
                    $" WHERE A.SurveyId=@SurveyId  ";
            //if (ExcludeOther)  //需要排除掉“其他”
            //    sSql += " AND (C.OtherFlag!='true' OR C.OtherFlag IS NULL ) ";
            string orderBy = " ORDER BY B.QuestionSeq, C.OptionSeq ";
            sSql += orderBy;

            //-------sql para----start
            SqlParameter[] sqlParams = new SqlParameter[] {
                new SqlParameter("@SurveyId", SqlDbType.UniqueIdentifier),
            };
            sqlParams[0].Value = SurveyId.ValidGuid();
            //-------sql para----end
            try
            {
                DataTable dtR = _db.GetQueryData(sSql, sqlParams);
                
                QuestionnairBase QS = new QuestionnairBase(); //SurveyId

                List<QuestionnairDetail> lstQD = new List<QuestionnairDetail>();   //QuestionId
                QuestionnairDetail QD = new QuestionnairDetail();

                List<QuestionnairOption> lstQO = new List<QuestionnairOption>();   //OptionId
                QuestionnairOption QO = new QuestionnairOption();

                //QS只有一筆,所以lstQS最後添加一次就OK
                QS.SurveyId = SurveyId;

                foreach (DataRow dr in dtR.Rows)
                {
                    //問卷資料只有一筆
                    DataRow dr0 = dtR.Rows[0];
                    QS.Title = dr0["Title"];
                    QS.FinalUrl = dr0["FinalUrl"];
                    QS.ThankWords = dr0["ThankWords"];
                    QS.DueAction = dr0["DueAction"];
                    QS.DelFlag = dr0["DelFlag"];
                    QS.Audit = dr0["Audit"];
                    QS.CreateUserId = dr0["CreateUserId"];
                    QS.CreateDateTime = dr0["CreateDateTime"];
                    QS.UpdUserId = dr0["UpdUserId"];
                    QS.UpdDateTime = dr0["UpdDateTime"];
                    QS.StyleType = dr0["StyleType"];
                    QS.DefBackgroudColor = dr0["DefBackgroudColor"];
                    QS.DefHeaderPic = dr0["DefHeaderPic"];
                    QS.DefHeaderPhonePic = dr0["DefHeaderPhonePic"];
                    QO = new QuestionnairOption();
                    if (QD.QuestionId==null ||(QD.QuestionId.ToString() != dr["QuestionId"].ToString())) //同一個questionId處理完
                    {
                        if (QD.QuestionId != null) //不是第一筆資料, 把前面的加入
                        {
                            //當有option時，Question.OptionList才顯示，否則，顯示null
                            if(lstQO.Count>0)
                                QD.option = lstQO;  //Question不同了, 則將Option list 給上一個QuestionId
                            //當question有值才加
                            if (QD.QuestionId != null && QD.QuestionId != DBNull.Value)
                                lstQD.Add(QD);      //上一個QuestionId完成,加入QuestionId list裡
                        }
                        lstQO = new List<QuestionnairOption>();  //clear optionId list
                        QD = new QuestionnairDetail();           //init questionId
                        //開始賦值給新的QuestionId各欄位(因一個QuestionId 可能對應多個OptionId, QuestionId只在不同時set value一次即可
                        QD.QuestionId = dr["QuestionId"];
                        QD.QuestionSeq = dr["QuestionSeq"];
                        QD.QuestionType = dr["QuestionType"];
                        QD.QuestionSubject = dr["QuestionSubject"];
                        QD.SubjectStyle = dr["SubjectStyle"];
                        QD.QuestionNote = dr["QuestionNote"];
                        QD.PageNo = dr["PageNo"];
                        QD.IsRequired = dr["IsRequired"];
                        QD.HasOther = dr["HasOther"];
                        QD.OtherIsShowText = dr["OtherIsShowText"];
                        QD.OtherVerify = dr["OtherVerify"];
                        QD.OtherTextMandatory = dr["OtherTextMandatory"];
                        QD.OtherCheckMessage = dr["OtherCheckMessage"];
                        QD.IsSetShowNum = dr["IsSetShowNum"];
                        QD.PCRowNum = dr["PCRowNum"];
                        QD.MobileRowNum = dr["MobileRowNum"];
                        QD.IsRamdomOption = dr["IsRamdomOption"];
                        QD.ExcludeOther = dr["ExcludeOther"];
                        QD.BaseDataValidType = dr["BaseDataValidType"];
                        QD.BlankDefaultWords = dr["BlankDefaultWords"];
                        QD.BlankValidType = dr["BlankValidType"];
                        QD.MatrixItems = dr["MatrixItems"];
                        QD.BlankMaxLimit = dr["BlankMaxLimit"];
                        QD.BlankMinLimit = dr["BlankMinLimit"];
                        QD.QuestionImage = dr["QuestionImage"];
                        QD.QuestionVideo = dr["QuestionVideo"];
                        QD.MultiOptionLimit = dr["MultiOptionLimit"];
                        QD.OtherMaxLimit = dr["OtherMaxLimit"];
                        QD.OtherMinLimit = dr["OtherMinLimit"];
                        //QD
                    }
                    //每一筆的OptionId 都不同,紀錄下來加入 optionId list
                    QO.OptionId = dr["OptionId"];
                    QO.OptionSeq = dr["OptionSeq"];
                    QO.OptionType = dr["OptionType"];
                    QO.OptionContent = dr["OptionContent"];
                    QO.ChildQuestionId = dr["ChildQuestionId"];
                    QO.OptionImage = dr["OptionImage"];
                    QO.OptionVideo = dr["OptionVideo"];
                    QO.OtherFlag = dr["OtherFlag"];
                    if (QO.OptionId != null && QO.OptionId != DBNull.Value)  //如果沒有，不要加
                        lstQO.Add(QO);
                }
                if (dtR.Rows.Count > 0)
                {
                    //最後一筆加入 
                    //當有option時，Question.OptionList才顯示，否則，顯示null
                    if (lstQO.Count > 0)
                        QD.option = lstQO;  //Question不同了, 則將Option list 給上一個QuestionId
                    //QD.option = lstQO;
                    //只加不為null的question
                    if (QD.QuestionId != null && QD.QuestionId != DBNull.Value)
                        lstQD.Add(QD);
                }
                if (lstQD.Count > 0)
                    QS.QuestionList = lstQD;
                //QS.QuestionList = lstQD;
                replyData.code = "200";
                replyData.message = $"查詢問卷題型明細完成。";
                replyData.data = QS;
                 
            }
            catch (Exception ex)
            {
                replyData.code = "-1";
                replyData.message = $"查詢問卷題型明細失敗！{ex.Message}.";
                replyData.data = "";
                Log.Error("查詢問卷題型明細失敗!" + ex.Message);
            }
            //返回
            return JsonConvert.SerializeObject(replyData);

        }
        #endregion

        #region 題型明細No pic
        /// <summary>
        /// 問卷題型明細不回傳圖片
        /// </summary>
        /// <param name="SurveyId"></param>
        /// <param name="OtherFlag"></param>
        /// <returns></returns>
        [Route("QueryNoPic")]
        [HttpGet]
        public String QueryNoPic(String SurveyId, String OtherFlag)
        {
            //輸入：
            /* "SurveyId":1231,
             * ["OtherFlag]:false
            */
            //"Newtonsoft.Json.Linq.JArray"
            //"Newtonsoft.Json.Linq.JObject"
            //多筆資料的話，此處需要處理，暫不管
            //if(value.GetType().Name=="JArray")
            //{
            //    foreach (var val in value as JArray)
            //    {
            //        InsertOne(val);
            //    }
            //}

            //JObject jo = (JObject)JsonConvert.DeserializeObject(value.ToString());
            var replyData = new ReplyData();
            //SurveyId 必須有?
            if (SurveyId == null)
            {
                //報告錯誤
                replyData.code = "-1";
                replyData.message = $"參數SurveyId不能為空！";
                replyData.data = "";
                Log.Error("問卷題型明細不回傳圖片:參數SurveyId不能為空！");
                return JsonConvert.SerializeObject(replyData);
            }

            //OtherFlag： 有傳入且value=false時，不查詢“其他”，即只取QUE003中OtherFlag=false的資料；沒有傳入或True,全取（不過濾）
            bool ExcludeOther = false;      //是否排除“其他”選項
            if (OtherFlag != null && Convert.ToBoolean(OtherFlag) == false)
            {
                //OtherFlag有輸入，並且輸入了false，意思是排除“其他”，也就是需要排除QUE003.OtherFlag='true'的資料
                ExcludeOther = true;
                Log.Debug($"api/Survey/Detail/QueryNopic:{SurveyId}排除[其他]選項");
            }
            else
            {
                ExcludeOther = false;
                Log.Debug($"api/Survey/Detail/QueryNopic:{SurveyId}包含[其他]選項");
            }
            //SurveyId 必須有?
            //if (jo["SurveyId"] == null)
            //{
            //    //報告錯誤
            //    replyData.code = "-1";
            //    replyData.message = $"參數SurveyId不能為空！";
            //    replyData.data = "";
            //    Log.Error("問卷題型明細參數SurveyId不能為空！");
            //    return JsonConvert.SerializeObject(replyData);
            //}
            //var SurveyId = jo["SurveyId"].ToString();

            //string sSql = "SELECT A.*, B.*, C.* " +
            //        " FROM QUE001_QuestionnaireBase A" +
            //        " LEFT JOIN QUE002_QuestionnaireDetail B ON B.SurveyId=A.SurveyId " +
            //        " LEFT JOIN QUE003_QuestionnaireOptions C ON C.QuestionId=B.QuestionId " +
            //        $" WHERE A.SurveyId='{SurveyId}'  ";
            //if (ExcludeOther)  //需要排除掉“其他”
            //    sSql += " AND (C.OtherFlag!='true' OR C.OtherFlag IS NULL ) ";
            //string orderBy = " ORDER BY B.QuestionSeq, C.OptionSeq ";
            //sSql += orderBy;

            string ExcludeOtherSql = "";
            if (ExcludeOther)  //需要排除掉“其他”
                ExcludeOtherSql = " AND (C.OtherFlag!='true' OR C.OtherFlag IS NULL ) ";
            string sSql = "SELECT A.*, B.*, C.* " +
                    " FROM QUE001_QuestionnaireBase A" +
                    " LEFT JOIN QUE002_QuestionnaireDetail B ON B.SurveyId=A.SurveyId " +
                    " LEFT JOIN QUE003_QuestionnaireOptions C ON C.QuestionId=B.QuestionId " + ExcludeOtherSql +
                    $" WHERE A.SurveyId=@SurveyId  ";
            //if (ExcludeOther)  //需要排除掉“其他”
            //    sSql += " AND (C.OtherFlag!='true' OR C.OtherFlag IS NULL ) ";
            string orderBy = " ORDER BY B.QuestionSeq, C.OptionSeq ";
            sSql += orderBy;
            //-------sql para----start
            SqlParameter[] sqlParams = new SqlParameter[] {
                new SqlParameter("@SurveyId", SqlDbType.UniqueIdentifier),
            };
            sqlParams[0].Value = SurveyId.ValidGuid();
            //-------sql para----end

            try
            {
                DataTable dtR = _db.GetQueryData(sSql,sqlParams);

                QuestionnairBaseNopic QS = new QuestionnairBaseNopic(); //SurveyId

                List<QuestionnairDetailNopic> lstQD = new List<QuestionnairDetailNopic>();   //QuestionId
                QuestionnairDetailNopic QD = new QuestionnairDetailNopic();

                List<QuestionnairOptionNopic> lstQO = new List<QuestionnairOptionNopic>();   //OptionId
                QuestionnairOptionNopic QO = new QuestionnairOptionNopic();

                //QS只有一筆,所以lstQS最後添加一次就OK
                QS.SurveyId = SurveyId;

                foreach (DataRow dr in dtR.Rows)
                {
                    //問卷資料只有一筆
                    DataRow dr0 = dtR.Rows[0];
                    QS.Title = dr0["Title"];
                    QS.FinalUrl = dr0["FinalUrl"];
                    QS.ThankWords = dr0["ThankWords"];
                    QS.DueAction = dr0["DueAction"];
                    QS.DelFlag = dr0["DelFlag"];
                    QS.Audit = dr0["Audit"];
                    QS.CreateUserId = dr0["CreateUserId"];
                    QS.CreateDateTime = dr0["CreateDateTime"];
                    QS.UpdUserId = dr0["UpdUserId"];
                    QS.UpdDateTime = dr0["UpdDateTime"];
                    QS.StyleType = dr0["StyleType"];
                    QO = new QuestionnairOptionNopic();
                    if (QD.QuestionId == null || (QD.QuestionId.ToString() != dr["QuestionId"].ToString())) //同一個questionId處理完
                    {
                        if (QD.QuestionId != null) //不是第一筆資料, 把前面的加入
                        {
                            //當有option時，Question.OptionList才顯示，否則，顯示null
                            if (lstQO.Count > 0)
                                QD.option = lstQO;  //Question不同了, 則將Option list 給上一個QuestionId
                            //當question有值才加
                            if (QD.QuestionId != null && QD.QuestionId != DBNull.Value)
                                lstQD.Add(QD);      //上一個QuestionId完成,加入QuestionId list裡
                        }
                        lstQO = new List<QuestionnairOptionNopic>();  //clear optionId list
                        QD = new QuestionnairDetailNopic();           //init questionId
                        //開始賦值給新的QuestionId各欄位(因一個QuestionId 可能對應多個OptionId, QuestionId只在不同時set value一次即可
                        QD.QuestionId = dr["QuestionId"];
                        QD.QuestionSeq = dr["QuestionSeq"];
                        QD.QuestionType = dr["QuestionType"];
                        QD.QuestionSubject = dr["QuestionSubject"];
                        QD.SubjectStyle = dr["SubjectStyle"];
                        QD.QuestionNote = dr["QuestionNote"];
                        QD.PageNo = dr["PageNo"];
                        QD.IsRequired = dr["IsRequired"];
                        QD.HasOther = dr["HasOther"];
                        QD.OtherIsShowText = dr["OtherIsShowText"];
                        QD.OtherVerify = dr["OtherVerify"];
                        QD.OtherTextMandatory = dr["OtherTextMandatory"];
                        QD.OtherCheckMessage = dr["OtherCheckMessage"];
                        QD.IsSetShowNum = dr["IsSetShowNum"];
                        QD.PCRowNum = dr["PCRowNum"];
                        QD.MobileRowNum = dr["MobileRowNum"];
                        QD.IsRamdomOption = dr["IsRamdomOption"];
                        QD.ExcludeOther = dr["ExcludeOther"];
                        QD.BaseDataValidType = dr["BaseDataValidType"];
                        QD.BlankDefaultWords = dr["BlankDefaultWords"];
                        QD.BlankValidType = dr["BlankValidType"];
                        QD.MatrixItems = dr["MatrixItems"];
                        QD.BlankMaxLimit = dr["BlankMaxLimit"];
                        QD.BlankMinLimit = dr["BlankMinLimit"];
                        QD.MultiOptionLimit = dr["MultiOptionLimit"];
                        QD.OtherMaxLimit = dr["OtherMaxLimit"];
                        QD.OtherMinLimit = dr["OtherMinLimit"];
                        //QD
                    }
                    //每一筆的OptionId 都不同,紀錄下來加入 optionId list
                    QO.OptionId = dr["OptionId"];
                    QO.OptionSeq = dr["OptionSeq"];
                    QO.OptionType = dr["OptionType"];
                    QO.OptionContent = dr["OptionContent"];
                    QO.ChildQuestionId = dr["ChildQuestionId"];
                    QO.OtherFlag = dr["OtherFlag"];
                    if (QO.OptionId != null && QO.OptionId != DBNull.Value)  //如果沒有，不要加
                        lstQO.Add(QO);
                }
                if (dtR.Rows.Count > 0)
                {
                    //最後一筆加入 
                    //當有option時，Question.OptionList才顯示，否則，顯示null
                    if (lstQO.Count > 0)
                        QD.option = lstQO;  //Question不同了, 則將Option list 給上一個QuestionId
                    //QD.option = lstQO;
                    //只加不為null的question
                    if (QD.QuestionId != null && QD.QuestionId != DBNull.Value)
                        lstQD.Add(QD);
                }
                if (lstQD.Count > 0)
                    QS.QuestionList = lstQD;
                //QS.QuestionList = lstQD;
                replyData.code = "200";
                replyData.message = $"查詢問卷題型明細（不回傳圖片）完成。";
                replyData.data = QS;

            }
            catch (Exception ex)
            {
                replyData.code = "-1";
                replyData.message = $"查詢問卷題型明細（不回傳圖片）失敗！{ex.Message}.";
                replyData.data = "";
                Log.Error("查詢問卷題型明細（不回傳圖片）失敗!" + ex.Message);
            }
            //返回
            return JsonConvert.SerializeObject(replyData);

        }
        #endregion
    }
    public class QuestionnairBase
    {
        public Object SurveyId { get; set; }
        public Object Title { get; set; }
        public Object FinalUrl { get; set; }
        public Object ThankWords { get; set; }
        public Object DueAction { get; set; }
        public Object DelFlag { get; set; }
        public Object Audit { get; set; }
        public Object CreateUserId { get; set; }
        public Object CreateDateTime { get; set; }
        public Object UpdUserId { get; set; }
        public Object UpdDateTime { get; set; }
        public Object StyleType { get; set; }
        public Object DefBackgroudColor { get; set; }
        public Object DefHeaderPic { get; set; }
        public Object DefHeaderPhonePic { get; set; }
        public List<QuestionnairDetail> QuestionList { get; set; }

    }
    public class QuestionnairDetail
    {
        /// <summary>
        /// 題型ID
        /// </summary>
        public Object QuestionId { get; set; }
        /// <summary>
        /// 題型順序
        /// </summary>
        public Object QuestionSeq { get; set; }
        /// <summary>
        /// 題目類型
        /// </summary>
        public Object QuestionType { get; set; }
        /// <summary>
        /// 題目標題
        /// </summary>
        public Object QuestionSubject { get; set; }
        /// <summary>
        /// 標題樣式
        /// </summary>
        public Object SubjectStyle { get; set; }
        /// <summary>
        /// 題型備註
        /// </summary>
        public Object QuestionNote { get; set; }
        /// <summary>
        /// 頁碼
        /// </summary>
        public Object PageNo { get; set; }
        /// <summary>
        /// 是否必填
        /// </summary>
        public Object IsRequired { get; set; }
        /// <summary>
        /// 是否新增【其他】選項
        /// </summary>
        public Object HasOther { get; set; }
        /// <summary>
        /// 是否顯示【其他】選項的填寫框
        /// </summary>
        public Object OtherIsShowText { get; set; }
        /// <summary>
        /// 驗證【其他】填寫框的值
        /// </summary>
        public Object OtherVerify { get; set; }
        /// <summary>
        /// 是否驗證【其他】填寫框的內容
        /// </summary>
        public Object OtherTextMandatory { get; set; }
        /// <summary>
        /// 【其他】填寫框驗證提示訊息
        /// </summary>
        public Object OtherCheckMessage { get; set; }
        /// <summary>
        /// 是否設定選項排版數量
        /// </summary>
        public Object IsSetShowNum { get; set; }
        /// <summary>
        /// PC選項排版數量
        /// </summary>
        public Object PCRowNum { get; set; }
        /// <summary>
        /// 手機選項排版數量
        /// </summary>
        public Object MobileRowNum { get; set; }
        /// <summary>
        /// 是否隨機排序
        /// </summary>
        public Object IsRamdomOption { get; set; }
        /// <summary>
        /// 是否排除【其他】進行排序
        /// </summary>
        public Object ExcludeOther { get; set; }
        /// <summary>
        /// 基本資料驗證方式
        /// </summary>
        public Object BaseDataValidType { get; set; }
        /// <summary>
        /// 填空預設文字
        /// </summary>
        public Object BlankDefaultWords { get; set; }
        /// <summary>
        /// 填空驗證方式
        /// </summary>
        public Object BlankValidType { get; set; }
        /// <summary>
        /// 矩陣選項，用逗號隔開
        /// </summary>
        public Object MatrixItems { get; set; }
        /// <summary>
        /// 填空題上限
        /// </summary>
        public Object BlankMaxLimit { get; set; }
        /// <summary>
        /// 填空題下限
        /// </summary>
        public Object BlankMinLimit { get; set; }
        /// <summary>
        /// 題目圖片
        /// </summary>
        public Object QuestionImage { get; set; }
        /// <summary>
        /// 題目影片
        /// </summary>
        public Object QuestionVideo { get; set; }
        /// <summary>
        /// 複選選項數量限制
        /// </summary>
        public Object MultiOptionLimit { get; set; }
        /// <summary>
        /// 其他選項數量限制下限
        /// </summary>
        public Object OtherMinLimit { get; set; }
        /// <summary>
        /// 其他選項數量限制上限
        /// </summary>
        public Object OtherMaxLimit { get; set; }
        public List<QuestionnairOption> option { get; set; }
    }
    public class QuestionnairOption
    {
        /// <summary>
        /// 選項ID
        /// </summary>
        public Object OptionId { get; set; }
        /// <summary>
        /// 選項順序
        /// </summary>
        public Object OptionSeq { get; set; }
        /// <summary>
        /// 選項類型
        /// </summary>
        public Object OptionType { get; set; }
        /// <summary>
        /// 選項內容
        /// </summary>
        public Object OptionContent { get; set; }
        /// <summary>
        /// 子題題目ID
        /// </summary>
        public Object ChildQuestionId { get; set; }
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
    /// <summary>
    /// 沒有圖檔的部分---查詢問卷題型明細（不回傳圖片）
    /// </summary>
    public class QuestionnairBaseNopic
    {
        public Object SurveyId { get; set; }
        public Object Title { get; set; }
        public Object FinalUrl { get; set; }
        public Object ThankWords { get; set; }
        public Object DueAction { get; set; }
        public Object DelFlag { get; set; }
        public Object Audit { get; set; }
        public Object CreateUserId { get; set; }
        public Object CreateDateTime { get; set; }
        public Object UpdUserId { get; set; }
        public Object UpdDateTime { get; set; }
        public Object StyleType { get; set; }
        public List<QuestionnairDetailNopic> QuestionList { get; set; }

    }

    public class QuestionnairDetailNopic
    {
        /// <summary>
        /// 題型ID
        /// </summary>
        public Object QuestionId { get; set; }
        /// <summary>
        /// 題型順序
        /// </summary>
        public Object QuestionSeq { get; set; }
        /// <summary>
        /// 題目類型
        /// </summary>
        public Object QuestionType { get; set; }
        /// <summary>
        /// 題目標題
        /// </summary>
        public Object QuestionSubject { get; set; }
        /// <summary>
        /// 標題樣式
        /// </summary>
        public Object SubjectStyle { get; set; }
        /// <summary>
        /// 題型備註
        /// </summary>
        public Object QuestionNote { get; set; }
        /// <summary>
        /// 頁碼
        /// </summary>
        public Object PageNo { get; set; }
        /// <summary>
        /// 是否必填
        /// </summary>
        public Object IsRequired { get; set; }
        /// <summary>
        /// 是否新增【其他】選項
        /// </summary>
        public Object HasOther { get; set; }
        /// <summary>
        /// 是否顯示【其他】選項的填寫框
        /// </summary>
        public Object OtherIsShowText { get; set; }
        /// <summary>
        /// 驗證【其他】填寫框的值
        /// </summary>
        public Object OtherVerify { get; set; }
        /// <summary>
        /// 是否驗證【其他】填寫框的內容
        /// </summary>
        public Object OtherTextMandatory { get; set; }
        /// <summary>
        /// 【其他】填寫框驗證提示訊息
        /// </summary>
        public Object OtherCheckMessage { get; set; }
        /// <summary>
        /// 是否設定選項排版數量
        /// </summary>
        public Object IsSetShowNum { get; set; }
        /// <summary>
        /// PC選項排版數量
        /// </summary>
        public Object PCRowNum { get; set; }
        /// <summary>
        /// 手機選項排版數量
        /// </summary>
        public Object MobileRowNum { get; set; }
        /// <summary>
        /// 是否隨機排序
        /// </summary>
        public Object IsRamdomOption { get; set; }
        /// <summary>
        /// 是否排除【其他】進行排序
        /// </summary>
        public Object ExcludeOther { get; set; }
        /// <summary>
        /// 基本資料驗證方式
        /// </summary>
        public Object BaseDataValidType { get; set; }
        /// <summary>
        /// 填空預設文字
        /// </summary>
        public Object BlankDefaultWords { get; set; }
        /// <summary>
        /// 填空驗證方式
        /// </summary>
        public Object BlankValidType { get; set; }
        /// <summary>
        /// 矩陣選項，用逗號隔開
        /// </summary>
        public Object MatrixItems { get; set; }
        /// <summary>
        /// 填空題上限
        /// </summary>
        public Object BlankMaxLimit { get; set; }
        /// <summary>
        /// 填空題下限
        /// </summary>
        public Object BlankMinLimit { get; set; }
        /// <summary>
        /// 複選選項數量限制
        /// </summary>
        public Object MultiOptionLimit { get; set; }
        /// <summary>
        /// 其他選項數量限制下限
        /// </summary>
        public Object OtherMinLimit { get; set; }
        /// <summary>
        /// 其他選項數量限制上限
        /// </summary>
        public Object OtherMaxLimit { get; set; }
        public List<QuestionnairOptionNopic> option { get; set; }
    }
    public class QuestionnairOptionNopic
    {
        /// <summary>
        /// 選項ID
        /// </summary>
        public Object OptionId { get; set; }
        /// <summary>
        /// 選項順序
        /// </summary>
        public Object OptionSeq { get; set; }
        /// <summary>
        /// 選項類型
        /// </summary>
        public Object OptionType { get; set; }
        /// <summary>
        /// 選項內容
        /// </summary>
        public Object OptionContent { get; set; }
        /// <summary>
        /// 子題題目ID
        /// </summary>
        public Object ChildQuestionId { get; set; }
        /// <summary>
        /// 是否其他選項
        /// </summary>
        public Object OtherFlag { get; set; }
    }
}
