using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using System.Linq;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using Common;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using SurveyWebApi.Utility;
using SurveyWebAPI.Models;
using SurveyWebAPI.Utility;

namespace SurveyWebAPI.Controllers
{
    [Authorize]
    [Route("api/OutsideSurvey")]
    [ApiController]
    public class OutsideSurveyController : ControllerBase
    {
        /// <summary>
        /// 問卷系統後臺之外部填寫平台--外部問卷題型明細
        /// </summary>
        /// <designer>Allen/Gem</designer>
        private DBHelper _db;
        private DBHelper _crmDB;
        private DBHelper _imptDB;
        private readonly JwtHelpers jwt;
        public OutsideSurveyController(JwtHelpers jwt)
        {
            _db = new DBHelper(AppSettingsHelper.DefaultConnectionString);
            _crmDB = new DBHelper(AppSettingsHelper.CRMConnectionString);
            _imptDB = new DBHelper(AppSettingsHelper.IMPTConnectionString);
            this.jwt = jwt;
        }
        #region 問卷預覽明細
        /// <summary>
        /// 外部問卷問卷預覽明細
        /// </summary>
        /// <param name="value"></param>
        /// <returns></returns>
        [Route("PreviewQuery")]
        [HttpGet]
        public String PreviewQuery(String SurveyId)
        {
            //輸入：
            /* "SurveyId":1231,
            */
            //輸出：
            //{
            // "code": "200",
            // "message": "查詢記錄完成。",
            // "data": {
            //    "SurveyId": "99999998-0000-0000-0000-000000000000",
            //     "QuestionList": [
            //       {
            //        "QuestionId":"99999998-0000-0000-0000-000000000000",
            //        "OptionList[
            //            {
            //               OptionId:"99999998-0000-0000-0000-000000000000"
            //            }
            //         ]
            //       }
            //     ],
            //     "LogicList":[
            //        {
            //        }
            //     ]
            //    "SurveySetting":{
            //    "IsShowPageNo":true
            //      }
            // }

            var replyData = new ReplyData();
            //SurveyId 必須有?
            if (SurveyId == null)
            {
                //報告錯誤
                replyData.code = "-1";
                replyData.message = $"參數SurveyId不能為空！";
                replyData.data = "";
                Log.Error("外部問卷題型明細參數SurveyId不能為空！");
                return JsonConvert.SerializeObject(replyData);
            }
            if(!SurveyWebAPI.Utility.Common.IsSurveyIdExist(SurveyId))
            {
                //報告錯誤
                replyData.code = "501";
                replyData.message = $"问卷不存在！";
                replyData.data = "";
                Log.Error($"外部問卷題型明細參數{SurveyId}不存在！");
                return JsonConvert.SerializeObject(replyData);
            }

            /* 測試錯誤訊息列表
            //var errlist = ErrorCode.ErrorCodes;
            //var msg = errlist.Where(t => t.Code.Equals("101")).FirstOrDefault().Message;
            //var ec = Utility.ErrorCode.errorCode;
            //var code = "200";
            //var ErrMsg = ec.Where(m => m.Code.Equals(code)).FirstOrDefault().Message;

            //var msg = ErrorCode.GetErrorMessageBy("101");
            ErrorCode.Code = "601";
            var a  = ErrorCode.Code;
            var b = ErrorCode.Message;
            */


            OutsideSurvey QS = new OutsideSurvey();
            try
            {
                //問卷基本資料
                QS = GetSurveyInfoBy(SurveyId);

                //問卷下的題目
                List<QuestionnairDetail> lstQD = new List<QuestionnairDetail>();   //QuestionId

                //取該問卷題目
                lstQD = GetQuestionInfo(SurveyId);
                if (lstQD.Count > 0)
                    QS.QuestionList = lstQD;
                //取該問卷邏輯
                List<RuleGroup> lstRuleGroup = new List<RuleGroup>();   //RuleGroup
                lstRuleGroup = GetLogicInfo(SurveyId);
                if (lstRuleGroup.Count > 0)
                    QS.LogicList = lstRuleGroup;
                //取該問卷基本設定from QUE004
                var SurveySetting = GetSurveySettingBy(SurveyId);
                QS.SurveySetting = SurveySetting;
                replyData.code = "200";
                replyData.message = $"查詢外部問卷題型明細完成。";
                replyData.data = QS;

            }
            catch (Exception ex)
            {
                replyData.code = "-1";
                replyData.message = $"查詢外部問卷題型明細失敗！{ex.Message}.";
                replyData.data = "";
                Log.Error("查詢外部問卷題型明細失敗!" + ex.Message);
            }
            //返回
            return JsonConvert.SerializeObject(replyData);

        }
        /// <summary>
        /// 依據SurveyId, 取得其下題目info
        /// </summary>
        /// <param name="SurveyId"></param>
        /// <returns></returns>
        private List<QuestionnairDetail> GetQuestionInfo(string SurveyId, bool ExcludeOther=false )
        {
            List<QuestionnairDetail> lstQD = new List<QuestionnairDetail>();   //QuestionId
            QuestionnairDetail QD = new QuestionnairDetail();

            List<QuestionnairOption> lstQO = new List<QuestionnairOption>();   //OptionId
            QuestionnairOption QO = new QuestionnairOption();
            //string sSql = "SELECT A.*, B.*, C.* " +
            //    " FROM QUE001_QuestionnaireBase A" +
            //    " LEFT JOIN QUE002_QuestionnaireDetail B ON B.SurveyId=A.SurveyId " +
            //    " LEFT JOIN QUE003_QuestionnaireOptions C ON C.QuestionId=B.QuestionId " +
            //    $" WHERE A.SurveyId='{SurveyId}' ORDER BY B.QuestionSeq, C.OptionSeq ";
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
                new SqlParameter("@SurveyId", SqlDbType.UniqueIdentifier)
            };
            sqlParams[0].Value = SurveyId.ValidGuid();
            //-------sql para----end

            try
            {
                DataTable dtR = _db.GetQueryData(sSql, sqlParams);
                foreach (DataRow dr in dtR.Rows)
                {
                    //問卷資料只有一筆
                    DataRow dr0 = dtR.Rows[0];
                    QO = new QuestionnairOption();
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
                        QD.OtherMinLimit = dr["OtherMinLimit"];
                        QD.OtherMaxLimit = dr["OtherMaxLimit"];
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
                return lstQD;

            }
            catch (Exception ex)
            {
                Log.Error("查詢外部問卷題型明細:" + ex.StackTrace);
                Log.Error("查詢外部問卷題型明細失敗!" + ex.Message);
                throw ex;
            }
        }
        /// <summary>
        /// 依據SurveyId, 取得其下邏輯規則
        /// </summary>
        /// <param name="SurveyId"></param>
        /// <returns></returns>
        private List<RuleGroup> GetLogicInfo(string SurveyId)
        {
            List<RuleGroup> lstRuleGroup = new List<RuleGroup>();   //RuleGroup
            RuleGroup ruleGroup = new RuleGroup();


            List<RuleCondition> lstRuleCondition = new List<RuleCondition>();   //RuleCondition
            RuleCondition ruleCondition = new RuleCondition();


            string sSql = " SELECT A.*, B.Id, B.ConditionRule, ISNULL(B.ConditionQuestionList,'') AS ConditionQuestionList,ISNULL(B.ConditionOptionList,'') AS ConditionOptionList,B.MatrixField " +
                "  FROM QUE005_QuestionnaireRuleGroup A " +
                "  LEFT JOIN QUE006_QuestionnaireRuleCondition B ON B.LogicId = A.LogicId " +
                $"  WHERE A.SurveyId = @SurveyId ";


            //-------sql para----start
            SqlParameter[] sqlParams = new SqlParameter[] {
                new SqlParameter("@SurveyId", SqlDbType.UniqueIdentifier)
            };
            sqlParams[0].Value = SurveyId.ValidGuid();
            //-------sql para----end

            try
            {
                DataTable dtR = _db.GetQueryData(sSql,sqlParams);
                foreach (DataRow dr in dtR.Rows)
                {
                    ruleCondition = new RuleCondition();
                    if (ruleGroup.LogicId == null || (ruleGroup.LogicId.ToString() != dr["LogicId"].ToString())) //同一個LogicId處理完
                    {
                        if (ruleGroup.LogicId != null) //不是第一筆資料, 把前面的加入
                        {
                            //當有rule condition 時，RuleGroup.ConditionList，否則，顯示null
                            if (lstRuleCondition.Count > 0)
                                ruleGroup.ConditionList = lstRuleCondition;  //Logic不同了, 則將ConditionList 給上一個logicId
                            //當LogicId有值才加
                            if (ruleGroup.LogicId != null && ruleGroup.LogicId != DBNull.Value)
                                lstRuleGroup.Add(ruleGroup);      //上一個LogicId完成,加入RuleGroup list裡
                        }
                        lstRuleCondition = new List<RuleCondition>();  //clear condition list
                        ruleGroup = new RuleGroup();           //init LogicId
                        //開始賦值給新的LogicId各欄位(因一個LogicId 可能對應多個Id, LogicId只在不同時set value一次即可
                        ruleGroup.LogicId = dr["LogicId"];
                        ruleGroup.LogicType = dr["LogicType"];
                        ruleGroup.LogicCondition = dr["LogicCondition"];
                        ruleGroup.TargetQuestionId = dr["TargetQuestionId"];
                        ruleGroup.BlockOptionList = dr["BlockOptionList"];

                        //RuleGroup
                    }
                    //每一筆的OptionId 都不同,紀錄下來加入 optionId list
                    ruleCondition.Id = dr["Id"];
                    ruleCondition.ConditionRule = dr["ConditionRule"];
                    ruleCondition.ConditionQuestionList = dr["ConditionQuestionList"];
                    ruleCondition.ConditionOptionList = dr["ConditionOptionList"];
                    ruleCondition.MatrixField = dr["MatrixField"];
                    if (ruleCondition.Id != null && ruleCondition.Id != DBNull.Value)  //如果沒有，不要加
                        lstRuleCondition.Add(ruleCondition);
                }
                if (dtR.Rows.Count > 0)
                {
                    //最後一筆加入
                    //當有Condition時，Logic.ConditionList才顯示，否則，顯示null
                    if (lstRuleCondition.Count > 0)
                        ruleGroup.ConditionList = lstRuleCondition;  //LogicId不同了, 則將Condition list 給上一個LogicId
                    //ruleGroup.ConditionList = lstRuleCondition;
                    //只加不為null的logic
                    if (ruleGroup.LogicId != null && ruleGroup.LogicId != DBNull.Value)
                        lstRuleGroup.Add(ruleGroup);
                }
                return lstRuleGroup;
            }
            catch (Exception ex)
            {
                Log.Error("查詢外部問卷題型明細-邏輯規則：" + ex.StackTrace);
                Log.Error("查詢外部問卷題型明細-邏輯規則：失敗!" + ex.Message);
                throw ex;
            }
        }
        /// <summary>
        /// 依據問卷Id，取得問卷基本資料
        /// </summary>
        /// <param name="SurveyId"></param>
        /// <returns></returns>
        private OutsideSurvey GetSurveyInfoBy(String SurveyId)
        {
            OutsideSurvey QS = new OutsideSurvey();
            //問卷基本資料
            var sSql = $" SELECT * FROM QUE001_QuestionnaireBase WHERE SurveyId=@SurveyId ";

            //-------sql para----start
            SqlParameter[] sqlParams = new SqlParameter[] {
                new SqlParameter("@SurveyId", SqlDbType.UniqueIdentifier)
            };
            sqlParams[0].Value = SurveyId.ValidGuid();
            //-------sql para----end
            try
            {
                DataTable dtSurvey = _db.GetQueryData(sSql,sqlParams);
                if (dtSurvey.Rows.Count < 1)
                {
                    Log.Error($"外部問卷題型明細{SurveyId}不存在！");
                    //報告錯誤
                    throw new Exception($"無此問卷{SurveyId}！");
                }
                //問卷資料只有一筆
                //QS只有一筆,所以lstQS最後添加一次就OK
                QS.SurveyId = SurveyId;
                DataRow dr0 = dtSurvey.Rows[0];
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

                return QS;
            }
            catch (Exception ex)
            {
                Log.Error(ex.Message);
                Log.Error(ex.StackTrace);
                throw ex;
            }
        }

        private QUE004_QuestionnaireSetting GetSurveySettingBy(String SurveyId)
        {
            QUE004_QuestionnaireSetting surveySetting = new QUE004_QuestionnaireSetting();
            //問卷基本資料
            var sSql = $" SELECT * FROM QUE004_QuestionnaireSetting WHERE SurveyId=@SurveyId ";

            //-------sql para----start
            SqlParameter[] sqlParams = new SqlParameter[] {
                new SqlParameter("@SurveyId", SqlDbType.UniqueIdentifier)
            };
            sqlParams[0].Value = SurveyId.ValidGuid();
            //-------sql para----end

            try
            {
                DataTable dtSurvey = _db.GetQueryData(sSql,sqlParams);
                if (dtSurvey.Rows.Count < 1)
                {
                    Log.Error("問卷{SurveyId}基本設定不存在！");
                    return null;
                }
                //每個問卷Id資料只有一筆
                surveySetting.SurveyId = SurveyId;
                DataRow dr0 = dtSurvey.Rows[0];
                surveySetting.SurveyId  = dr0["SurveyId"];
                surveySetting.IsShowPageNo   = dr0["IsShowPageNo"];
                surveySetting.IsShowQuestionNo  = dr0["IsShowQuestionNo"];
                surveySetting.IsShowRequiredStar   = dr0["IsShowRequiredStar"];
                surveySetting.IsShowProgress = dr0["IsShowProgress"];
                surveySetting.PorgressPosition   = dr0["PorgressPosition"];
                surveySetting.ProgressStyle   = dr0["ProgressStyle"];
                surveySetting.UseVirifyCode   = dr0["UseVirifyCode"];
                surveySetting.IsOneQuestionPerPage   = dr0["IsOneQuestionPerPage"];
                surveySetting.IsPublishResult   = dr0["IsPublishResult"];
                surveySetting.IsShowEndPage  = dr0["IsShowEndPage"];
                surveySetting.IsShowOptionNo = dr0["IsShowOptionNo"];

                return surveySetting;
            }
            catch (Exception ex)
            {
                Log.Error(ex.Message);
                Log.Error(ex.StackTrace);
                throw ex;
            }
        }
        #endregion

        #region 外部問卷提交
        [Route("Submit")]
        [HttpPost]
        public object Submit([FromBody] Object value)
        {
            string LogTimeID = Guid.NewGuid().ToString();
            Log.Debug("(" + LogTimeID + ")Submit Start:" + System.DateTime.Now.ToString("yyyy/MM/dd HH:mm:ss.fff"));
            OutsideSurveySubmit outsideSurvey = JsonConvert.DeserializeObject<OutsideSurveySubmit>(value.ToString());
            var replyData = new ReplyData();
            try
            {
                string surveyId = outsideSurvey.SurveyId;
                int provideType = outsideSurvey.ProvideType;
                string oldReplyKey = "";
                //查詢QUE009_QuestionnaireProvideType，判斷ValidRegister
                bool isInserDB = true;
                string sqlS = string.Format("select ValidRegister,MultiProvideType,ReplyMaxNum from QUE009_QuestionnaireProvideType where SurveyId=@surveyId and ProvideType=@provideType ");

                //-------sql para----start
                SqlParameter[] sqlParams = new SqlParameter[] {
                new SqlParameter("@surveyId", SqlDbType.UniqueIdentifier),
                new SqlParameter("@provideType", SqlDbType.Int)
                };
                sqlParams[0].Value = surveyId.ValidGuid();
                sqlParams[1].Value = provideType.ValidInt();
                //-------sql para----end

                Log.Debug("查詢QUE009_QuestionnaireProvideType:" + sqlS);
                DataRow drQUE009 = _db.GetQueryRecord(sqlS, sqlParams);
                if (drQUE009 == null)
                {
                    Log.Debug(string.Format("查無資料!from QUE009_QuestionnaireProvideType By SurveyId={0} and ProvideType={1}", surveyId, provideType));
                    throw new Exception(string.Format("查無資料!from QUE009_QuestionnaireProvideType By SurveyId={0} and ProvideType={1}", surveyId, provideType));
                }
                int validRegister = int.Parse(drQUE009["ValidRegister"].ToString());
                int multiProvideType = int.Parse(drQUE009["MultiProvideType"].ToString());
                int replyMaxNum = int.Parse(drQUE009["ReplyMaxNum"].ToString());
                ///
                ///新增檢核邏輯:
                ///1.若為正式環境，該問卷填寫數量(QUE021) > 額滿數量(QUE009.ReplyMaxNum)，回傳errorCode: 602，
                ///     Message: 問卷額滿，該筆資料不需寫入DB，data不需回傳資料，QUE009.ReplyMaxNum = 0表示無上限。
                ///2.若為測試環境，該問卷填寫數量 > 30筆，回傳errorCode: 603，
                ///     Message: 測試問卷額滿，該筆資料不需寫入DB，筆數要可以config
                ///
                sqlS = string.Format("select count(ReplyKey) from QUE021_AnwserCollection where SurveyId=@surveyId and ProvideType=@provideType and Env=@Env and DelFlag='0' ");

                //-------sql para----start
                SqlParameter[] sqlSParams = new SqlParameter[] {
                new SqlParameter("@surveyId", SqlDbType.UniqueIdentifier),
                new SqlParameter("@provideType", SqlDbType.Int),
                new SqlParameter("@Env", SqlDbType.Int)
                };
                sqlSParams[0].Value = surveyId.ValidGuid();
                sqlSParams[1].Value = provideType.ValidInt();
                sqlSParams[2].Value = outsideSurvey.Env.ValidInt();
                //-------sql para----end

                Log.Debug("該問卷填寫數量QUE021_AnwserCollection:" + sqlS);
                int replyNum = Convert.ToInt32(_db.GetSingle(sqlS, sqlSParams));
                if (outsideSurvey.Env == 2)
                {
                    if (replyNum >= replyMaxNum && replyMaxNum > 0)
                    {
                        ErrorCode.Code = "602";
                        replyData.code = ErrorCode.Code;
                        replyData.message = ErrorCode.Message;
                        replyData.data = null;
                        return JsonConvert.SerializeObject(replyData);
                    }
                }
                else if (outsideSurvey.Env == 1)
                {
                    if (replyNum >= AppSettingsHelper.ReplyLimit)
                    {
                        ErrorCode.Code = "603";
                        replyData.code = ErrorCode.Code;
                        replyData.message = ErrorCode.Message;
                        replyData.data = null;
                        return JsonConvert.SerializeObject(replyData);
                    }
                }

                //check 問卷是否存在
                if (!Utility.Common.IsSurveyIdExist(surveyId))
                {
                    Log.Error($"api/OutsideSurvey/Submit:問卷{surveyId}不存在！");
                    // 報告錯誤
                    ErrorCode.Code = "202";
                    replyData.code = ErrorCode.Code;
                    replyData.message = ErrorCode.Message;// "問卷不存在！";
                    replyData.data = "";
                    Log.Error($"api/OutsideSurvey/Submit:問卷{surveyId}不存在！");
                    return JsonConvert.SerializeObject(replyData);
                }
                int SurveyStatus = 1;
                //正式機,SurveyStatus 不為2，則返回-1以及帶回SurveyStatus
                if (outsideSurvey.Env == 2)  //正式機
                {
                    SurveyStatus = CheckNormalEnvStatus(surveyId, provideType.ToString(), "");
                    if (SurveyStatus != 2)
                    {

                        ErrorCode.Code = SurveyStatus.ToString();
                        replyData.code = "-1";
                        replyData.message = ErrorCode.Message;

                        OutsideSubmit submit = new OutsideSubmit();
                        submit.SurveyStatus = ErrorCode.Code;

                        replyData.data = submit;

                        Log.Error($"api/OutsideSurvey/Submit:"+ ErrorCode.Message);
                        return JsonConvert.SerializeObject(replyData);
                    }
                }

                //填寫資料，要走transaction，有寫入失敗則全部rollback
               var sqlStrList = new List<KeyValuePair<string, SqlParameter[]>>();
                //計算填寫時間，TimePeriod前端給開始填寫時間，後端要計算填寫時間在寫到DB
                TimeSpan ts = Convert.ToDateTime(outsideSurvey.SubmitTime) - Convert.ToDateTime(outsideSurvey.TimePeriod);
                outsideSurvey.TimePeriod = Math.Floor(ts.TotalHours).ToString().PadLeft(2, '0') + ":" + ts.Minutes.ToString().PadLeft(2, '0') + ":" + ts.Seconds.ToString().PadLeft(2, '0');

                //1:【匿名】 => 直接新增QUE021/022
                if (validRegister == 1)
                {
                    isInserDB = true;
                }
                //2:【驗證格式】(VerifyInfo)=> 進行MultiProvideType判斷
                else if (validRegister == 2)
                {
                    //1: 覆蓋舊資料 => 更新舊資料QUE021.DelFlag = true，然後新增QUE021/022
                    if (multiProvideType == 1)
                    {
                        sqlS = string.Format("Update QUE021_AnwserCollection set DelFlag='1' where SurveyId=@surveyId and VerifyInfo=@VerifyInfo ");

                        //-------sql para----start
                        SqlParameter[] sql1Params = new SqlParameter[] {
                        new SqlParameter("@surveyId", SqlDbType.UniqueIdentifier),
                        new SqlParameter("@VerifyInfo", SqlDbType.NChar)
                        };
                        sql1Params[0].Value = surveyId.ValidGuid();
                        sql1Params[1].Value = outsideSurvey.VerifyInfo.Valid();
                        //-------sql para----end


                        var obj = new KeyValuePair<string, SqlParameter[]>(sqlS, sql1Params);

                        Log.Debug("更新舊資料QUE021.DelFlag = true:" + sqlS);
                        //int uR = _db.ExecuteSql(sqlS);
                        sqlStrList.Add(obj);
                        isInserDB = true;
                    }
                    //2: 僅寫入第一筆 => 如果資料存在不處理，若不存在新增QUE021/022
                    else if (multiProvideType == 2)
                    {
                        sqlS = string.Format("select ReplyKey from QUE021_AnwserCollection where SurveyId=@surveyId and VerifyInfo=@VerifyInfo and DelFlag='1' ");

                        //-------sql para----start
                        SqlParameter[] sql1Params = new SqlParameter[] {
                        new SqlParameter("@surveyId", SqlDbType.UniqueIdentifier),
                        new SqlParameter("@VerifyInfo", SqlDbType.NChar)
                        };
                        sql1Params[0].Value = surveyId.ValidGuid();
                        sql1Params[1].Value = outsideSurvey.VerifyInfo.Valid();
                        //-------sql para----end


                        var obj = new KeyValuePair<string, SqlParameter[]>(sqlS, sql1Params);

                        Log.Debug("查詢QUE021_AnwserCollection是否有資料 by SurveyId+VerifyInfo" + sqlS);
                        oldReplyKey = _db.GetSingle(sqlS, sql1Params);
                        if (!string.IsNullOrEmpty(oldReplyKey))
                            isInserDB = false;
                        else
                            isInserDB = true;
                    }
                    //3: 重複紀錄 => 直接新增QUE021/022
                    else if (multiProvideType == 3)
                    {
                        isInserDB = true;
                    }
                }
                //3:【參數傳遞】(ParameterInfo)=> 進行MultiProvideType判斷
                else if (validRegister == 3)
                {
                    //1: 覆蓋舊資料 => 更新舊資料QUE021.DelFlag = true，然後新增QUE021/022
                    if (multiProvideType == 1)
                    {
                        sqlS = string.Format("Update QUE021_AnwserCollection set DelFlag='1' where SurveyId=@surveyId and ParameterInfo=@ParameterInfo ");

                        //-------sql para----start
                        SqlParameter[] sql1Params = new SqlParameter[] {
                        new SqlParameter("@surveyId", SqlDbType.UniqueIdentifier),
                        new SqlParameter("@ParameterInfo", SqlDbType.NChar)
                        };
                        sql1Params[0].Value = surveyId.ValidGuid();
                        sql1Params[1].Value = outsideSurvey.ParameterInfo.Valid();
                        //-------sql para----end


                        var obj = new KeyValuePair<string, SqlParameter[]>(sqlS, sql1Params);


                        Log.Debug("更新舊資料QUE021.DelFlag = true:" + sqlS);
                        //int uR = _db.ExecuteSql(sqlS);
                        sqlStrList.Add(obj);
                        isInserDB = true;
                    }
                    //2: 僅寫入第一筆 => 如果資料存在不處理，若不存在新增QUE021/022
                    else if (multiProvideType == 2)
                    {
                        sqlS = string.Format("select ReplyKey from QUE021_AnwserCollection where SurveyId=@surveyId and ParameterInfo=@ParameterInfo and DelFlag='1' ");

                        //-------sql para----start
                        SqlParameter[] sql1Params = new SqlParameter[] {
                        new SqlParameter("@surveyId", SqlDbType.UniqueIdentifier),
                        new SqlParameter("@ParameterInfo", SqlDbType.NChar)
                        };
                        sql1Params[0].Value = surveyId.ValidGuid();
                        sql1Params[1].Value = outsideSurvey.ParameterInfo.Valid();
                        //-------sql para----end


                        var obj = new KeyValuePair<string, SqlParameter[]>(sqlS, sql1Params);

                        Log.Debug("查詢QUE021_AnwserCollection是否有資料 by SurveyId+ParameterInfo" + sqlS);
                        oldReplyKey = _db.GetSingle(sqlS, sql1Params);
                        if (!string.IsNullOrEmpty(oldReplyKey))
                            isInserDB = false;
                        else
                            isInserDB = true;
                    }
                    //3: 重複紀錄 => 直接新增QUE021/022
                    else if (multiProvideType == 3)
                    {
                        isInserDB = true;
                    }
                }

                //確定新增
                if (isInserDB)
                {
                    string replyKey = Guid.NewGuid().ToString();
                    //新增QUE021_AnwserCollection
                    sqlS = string.Format("insert into QUE021_AnwserCollection (SurveyId,ProvideType,ExtenField,VerifyInfo,ParameterInfo,Device,ForceEnd,TimePeriod,SubmitTime,Env,DelFlag,ReplyKey)" +
                        "values(@surveyId,@ProvideType,@ExtenField,@VerifyInfo,@ParameterInfo,@Device,@ForceEnd,@TimePeriod,@SubmitTime,@Env,'0',@replyKey) ");

                    //-------sql para----start
                    SqlParameter[] sql1Params = new SqlParameter[] {
                        new SqlParameter("@surveyId", SqlDbType.UniqueIdentifier),
                        new SqlParameter("@ProvideType", SqlDbType.Int),
                        new SqlParameter("@ExtenField", SqlDbType.NVarChar),
                        new SqlParameter("@VerifyInfo", SqlDbType.NChar),
                        new SqlParameter("@ParameterInfo", SqlDbType.NChar),
                        new SqlParameter("@Device", SqlDbType.NChar),
                        new SqlParameter("@ForceEnd", SqlDbType.Bit),
                        new SqlParameter("@TimePeriod", SqlDbType.NChar),
                        new SqlParameter("@SubmitTime", SqlDbType.DateTime),
                        new SqlParameter("@Env", SqlDbType.Int),
                        new SqlParameter("@replyKey", SqlDbType.UniqueIdentifier),
                        };
                    sql1Params[0].Value = surveyId.ValidGuid();
                    sql1Params[1].Value = outsideSurvey.ProvideType.ValidInt();
                    sql1Params[2].Value = outsideSurvey.ExtenField.Valid();
                    sql1Params[3].Value = outsideSurvey.VerifyInfo.Valid();
                    sql1Params[4].Value = outsideSurvey.ParameterInfo.Valid();
                    sql1Params[5].Value = outsideSurvey.Device.Valid();
                    sql1Params[6].Value = outsideSurvey.ForceEnd.ValidBit();
                    sql1Params[7].Value = outsideSurvey.TimePeriod.Valid();
                    sql1Params[8].Value = outsideSurvey.SubmitTime.ValidDateTime();
                    sql1Params[9].Value = outsideSurvey.Env.ValidInt();
                    sql1Params[10].Value = replyKey.ValidGuid();
                    //-------sql para----end


                    var obj = new KeyValuePair<string, SqlParameter[]>(sqlS, sql1Params);

                    Log.Debug("新增QUE021_AnwserCollection:" + sqlS);
                    //int iR = _db.ExecuteSql(sqlS);
                    sqlStrList.Add(obj);
                    //查詢新增的ReplyId for QUE022
                    //OutsideSurveySubmit newQue021 = QueryOutsideSurvey(outsideSurvey.SurveyId, replyKey, 0);
                    //int replyId = newQue021.ReplyId;
                    //int replyId = 999;
                    //新增QUE022_AnwserCollectionDetail
                    if (outsideSurvey.AnswerList != null)
                    {
                        foreach (Answer answer in outsideSurvey.AnswerList)
                        {
                            if (string.IsNullOrEmpty(answer.OptionId))
                            {
                                //如果是身分證號，字母要大寫
                                sqlS = string.Format("insert into QUE022_AnwserCollectionDetail (ReplyKey,QuestionId,OptionId,MatrixField,BlankAnwer) values(@replyKey,@QuestionId,null,@MatrixField,@BlankAnwer) ");
                                // ,replyKey, answer.QuestionId, answer.MatrixField, Regex.IsMatch(answer.BlankAnwer, @"^[A-Z,a-z]{1}[0-9]{9}$") ? answer.BlankAnwer.ToUpper(): answer.BlankAnwer);
                                //-------sql para----start
                                SqlParameter[] sql2Params = new SqlParameter[] {
                                new SqlParameter("@replyKey", SqlDbType.UniqueIdentifier),
                                new SqlParameter("@QuestionId", SqlDbType.UniqueIdentifier),
                                new SqlParameter("@MatrixField", SqlDbType.NVarChar),
                                new SqlParameter("@BlankAnwer", SqlDbType.NVarChar),
                                };
                                sql2Params[0].Value = replyKey.ValidGuid();
                                sql2Params[1].Value = answer.QuestionId.ValidGuid();
                                sql2Params[2].Value = answer.MatrixField.Valid();
                                sql2Params[3].Value = ( Regex.IsMatch( answer.BlankAnwer, @"^[A-Z,a-z]{1}[0-9]{9}$" ) ? answer.BlankAnwer.ToUpper() : answer.BlankAnwer ).Valid();
                                //-------sql para----end

                                var obj2 = new KeyValuePair<string, SqlParameter[]>(sqlS, sql2Params);
                                sqlStrList.Add(obj2);
                            }
                            else
                            {
                                sqlS = string.Format("insert into QUE022_AnwserCollectionDetail (ReplyKey,QuestionId,OptionId,MatrixField,BlankAnwer) values(@replyKey,@QuestionId,@OptionId,@MatrixField,@BlankAnwer) ");
                                //, replyKey, answer.QuestionId, answer.OptionId, answer.MatrixField, answer.BlankAnwer);

                                //-------sql para----start
                                SqlParameter[] sql2Params = new SqlParameter[] {
                                new SqlParameter("@replyKey", SqlDbType.UniqueIdentifier),
                                new SqlParameter("@QuestionId", SqlDbType.UniqueIdentifier),
                                new SqlParameter("@OptionId", SqlDbType.UniqueIdentifier),
                                new SqlParameter("@MatrixField", SqlDbType.NVarChar),
                                new SqlParameter("@BlankAnwer", SqlDbType.NVarChar),
                                };
                                sql2Params[0].Value = replyKey.ValidGuid();
                                sql2Params[1].Value = answer.QuestionId.ValidGuid();
                                sql2Params[2].Value = answer.OptionId.ValidGuid();
                                sql2Params[3].Value = answer.MatrixField.Valid();
                                sql2Params[4].Value = answer.BlankAnwer.Valid();
                                //-------sql para----end

                                var obj2 = new KeyValuePair<string, SqlParameter[]>(sqlS, sql2Params);
                                sqlStrList.Add(obj2);


                            }
                            Log.Debug("新增QUE022_AnwserCollectionDetail:" + sqlS);
                            //iR += _db.ExecuteSql(sqlS);
                            //sqlStrList.Add(sqlS);
                        }
                    }
                    _db.ExecuteSqlTran(sqlStrList);
                    replyData.code = "200";
                    replyData.message = "外部問卷提交完成。";
                    replyData.data = QueryOutsideSurvey(surveyId, replyKey, 1);
                }
                else
                {
                    _db.ExecuteSqlTran(sqlStrList);
                    replyData.code = "200";
                    replyData.message = "外部問卷提交完成。";
                    replyData.data = QueryOutsideSurvey(surveyId, oldReplyKey, 1);
                }
            }
            catch (Exception ex)
            {
                replyData.code = "-1";
                replyData.message = $"外部問卷提交失敗!{ex.Message}.";
                replyData.data = null;
                Log.Error("外部問卷提交失敗!填寫資料:"+ value.ToString()+"\n異常訊息:" + ex.Message);
            }
            Log.Debug("(" + LogTimeID + ")Submit End:" + System.DateTime.Now.ToString("yyyy/MM/dd HH:mm:ss.fff"));
            return JsonConvert.SerializeObject(replyData);
        }
        public OutsideSurveySubmit QueryOutsideSurvey(string surveyId, string replyKey, int qType)
        {
            //data實體
            OutsideSurveySubmit outsideSurvey = new OutsideSurveySubmit();
            try
            {
                string sqlS = string.Format("select * from QUE021_AnwserCollection where SurveyId=@surveyId and ReplyKey=@replyKey");

                //-------sql para----start
                SqlParameter[] sqlParams = new SqlParameter[] {
                new SqlParameter("@surveyId", SqlDbType.UniqueIdentifier),
                new SqlParameter("@replyKey", SqlDbType.UniqueIdentifier),

                };
                sqlParams[0].Value = surveyId.ValidGuid();
                sqlParams[1].Value = replyKey.ValidGuid();

                //-------sql para----end

                DataTable dtR = _db.GetQueryData(sqlS,sqlParams);
                if (dtR != null && dtR.Rows.Count == 1)
                {

                    //Fill data
                    //if (dtR.Rows[0]["ReplyId"] == null || string.IsNullOrEmpty(dtR.Rows[0]["ReplyId"].ToString().Trim()))
                    //    outsideSurvey.ReplyId = 1;
                    //else
                    //    outsideSurvey.ReplyId = int.Parse(dtR.Rows[0]["ReplyId"].ToString().Trim());

                    //更新ReplyId
                    //sqlS = string.Format("update QUE022_AnwserCollectionDetail set ReplyId ='{0}' where ReplyKey='{1}'", outsideSurvey.ReplyId,replyKey);
                    //int iR = _db.ExecuteSql(sqlS);

                    outsideSurvey.SurveyId = dtR.Rows[0]["SurveyId"].ToString().Trim();
                    if (dtR.Rows[0]["ProvideType"] == null || string.IsNullOrEmpty(dtR.Rows[0]["ProvideType"].ToString().Trim()))
                        outsideSurvey.ProvideType = 1;
                    else
                        outsideSurvey.ProvideType = int.Parse(dtR.Rows[0]["ProvideType"].ToString().Trim());
                    outsideSurvey.ExtenField = dtR.Rows[0]["ExtenField"].ToString().Trim();
                    outsideSurvey.VerifyInfo = dtR.Rows[0]["VerifyInfo"].ToString().Trim();
                    outsideSurvey.ParameterInfo = dtR.Rows[0]["ParameterInfo"].ToString().Trim();
                    outsideSurvey.Device = dtR.Rows[0]["Device"].ToString().Trim();
                    outsideSurvey.ForceEnd = dtR.Rows[0]["ForceEnd"].ToString().Trim();
                    outsideSurvey.TimePeriod = dtR.Rows[0]["TimePeriod"].ToString().Trim();
                    outsideSurvey.SubmitTime = dtR.Rows[0]["SubmitTime"].ToString();
                    if (dtR.Rows[0]["Env"] == null || string.IsNullOrEmpty(dtR.Rows[0]["Env"].ToString().Trim()))
                        outsideSurvey.Env = 1;
                    else
                        outsideSurvey.Env = int.Parse(dtR.Rows[0]["Env"].ToString().Trim());

                    if (qType == 0)
                        return outsideSurvey;

                    sqlS = string.Format("select * from QUE022_AnwserCollectionDetail where ReplyKey=@replyKey");
                    SqlParameter[] sqlSParams = new SqlParameter[] {
                        new SqlParameter("@replyKey", SqlDbType.VarChar),
                    };
                    sqlSParams[0].Value = new System.Data.SqlTypes.SqlChars(replyKey);
                    dtR = _db.GetQueryData(sqlS, sqlSParams);
                    if (dtR != null && dtR.Rows.Count > 0)
                    {
                        Answer[] answerArr = new Answer[dtR.Rows.Count];
                        int i = 0;
                        foreach (DataRow dr in dtR.Rows)
                        {
                            Answer answer = new Answer();
                            if (dr["id"] == null || string.IsNullOrEmpty(dr["id"].ToString().Trim()))
                                answer.id = 1;
                            else
                                answer.id = int.Parse(dr["id"].ToString().Trim());
                            //if (dr["ReplyId"] == null || string.IsNullOrEmpty(dr["ReplyId"].ToString().Trim()))
                            //    answer.ReplyId = 1;
                            //else
                            //    answer.ReplyId = int.Parse(dr["ReplyId"].ToString().Trim());
                            answer.QuestionId = dr["QuestionId"].ToString().Trim();
                            answer.OptionId = dr["OptionId"].ToString().Trim();
                            answer.MatrixField = dr["MatrixField"].ToString().Trim();
                            answer.BlankAnwer = dr["BlankAnwer"].ToString().Trim();

                            answerArr[i] = answer;
                            i++;
                        }

                        outsideSurvey.AnswerList = answerArr;
                    }
                    return outsideSurvey;
                }
                else
                {
                    return null;
                }
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }
        #endregion

        #region 資格驗證
        /// <summary>
        /// 資格驗證
        /// </summary>
        /// <param name="value"></param>
        /// <returns></returns>
        [Route("LoginVerify")]
        [HttpPost]
        public String LoginVerify([FromBody] Object value)
        {
            /* 輸入格式：
             *{
             *    "SurveyId": "10",             //問卷Id
             *    "ProvideType": "1",           //收集渠道
             *    "ValidField": "10",           //OTP 驗證碼
             *    "ValidData": "0987654321",    //用戶帳號
             *}
             */
            string LogTimeID = Guid.NewGuid().ToString();
            Log.Debug("(" + LogTimeID + ")LoginVerify Start:" + System.DateTime.Now.ToString("yyyy/MM/dd HH:mm:ss.fff"));
            JObject jo = (JObject)JsonConvert.DeserializeObject(value.ToString());
            var replyData = new ReplyData();
            if (jo["SurveyId"] == null)
            {
                //報告錯誤
                replyData.code = "-1";
                replyData.message = $"未傳入參數SurveyId！";
                replyData.data = "";
                Log.Error("資格驗證失敗!" + "未傳入參數SurveyId！");
                return JsonConvert.SerializeObject(replyData);
            }
            var SurveyId = jo["SurveyId"].ToString();
            if (jo["ProvideType"] == null)
            {
                //報告錯誤
                replyData.code = "-1";
                replyData.message = $"未傳入參數ProvideType！";
                replyData.data = "";
                Log.Error("資格驗證失敗!" + "未傳入參數ProvideType！");
                return JsonConvert.SerializeObject(replyData);
            }
            var ProvideType = jo["ProvideType"].ToString();
            //ValidField, ValidData 必須有?
            if (jo["ValidField"] == null)
            {
                //報告錯誤
                replyData.code = "-1";
                replyData.message = $"未傳入參數ValidField！";
                replyData.data = "";
                Log.Error("資格驗證失敗!" + "未傳入參數ValidField！");
                return JsonConvert.SerializeObject(replyData);
            }
            //ValidField在DB中是數值
            int ValidField = 0;
            if(!int.TryParse(jo["ValidField"].ToString(),out ValidField))
            {
                //報告錯誤
                replyData.code = "-1";
                replyData.message = $"參數ValidField不正確！";
                replyData.data = "";
                Log.Error("資格驗證失敗!" + "參數ValidField不正確！");
                return JsonConvert.SerializeObject(replyData);
            }

            //var ValidField = Convert.ToInt32(jo["ValidField"]);//.ToString(); ValidField在DB中是數值
            if (jo["ValidData"] == null)
            {
                //報告錯誤
                replyData.code = "-1";
                replyData.message = $"未傳入參數ValidData！";
                replyData.data = "";
                Log.Error("資格驗證失敗!" + "未傳入參數ValidData！");
                return JsonConvert.SerializeObject(replyData);
            }
            var ValidData = jo["ValidData"].ToString();

            try
            {
                //開始驗證
                //1. 傳入ProvideType和原設定是否相同
                if(!IsProvideTypeValid(SurveyId,ProvideType))
                {
                    //驗證不通過
                    // 回傳結果
                    ErrorCode.Code = "605";    //驗證欄位不正確
                    replyData.code = ErrorCode.Code;
                    replyData.message = ErrorCode.Message;
                    replyData.data = "";
                    Log.Error($"資格驗證失敗！驗證欄位不正確！");
                    return JsonConvert.SerializeObject(replyData);
                }
                bool result = VerifyBy(SurveyId, ValidField, ValidData);
                if (!result)
                {
                    //驗證不通過
                    // 回傳結果
                    ErrorCode.Code = "601";
                    replyData.code = ErrorCode.Code;
                    replyData.message = ErrorCode.Message;
                    replyData.data = "";
                    Log.Error($"資格驗證失敗！ValidField={ValidField}, ValidData='{ValidData}'");
                    return JsonConvert.SerializeObject(replyData);
                }
                // 回傳結果
                replyData.code = "200";
                replyData.message = $"驗證成功。";
                replyData.data = null;
                Log.Debug($"驗證成功！ValidField={ValidField}, ValidData='{ValidData}'");

                Log.Debug("(" + LogTimeID + ")LoginVerify End:" + System.DateTime.Now.ToString("yyyy/MM/dd HH:mm:ss.fff"));
                return JsonConvert.SerializeObject(replyData);
            }
            catch (Exception ex)
            {
                //報告錯誤
                replyData.code = "-1";
                replyData.message = ex.Message;
                replyData.data = null;
                Log.Error("驗證失敗！" + ex.Message);
                return JsonConvert.SerializeObject(replyData);
            }
        }
        private bool IsProvideTypeValid(String surveyId, String provideType)
        {
            var sSql = " SELECT COUNT(1) FROM QUE009_QuestionnaireProvideType " +
                $" WHERE SurveyId=@surveyId AND ProvideType=@provideType ";

            //-------sql para----start
            SqlParameter[] sqlParams = new SqlParameter[] {
                new SqlParameter("@surveyId", SqlDbType.UniqueIdentifier),
                new SqlParameter("@provideType", SqlDbType.Int),

            };
            sqlParams[0].Value = surveyId.ValidGuid();
            sqlParams[1].Value = provideType.ValidInt();


            //-------sql para----end

            try
            {
                var result = _db.GetSingle(sSql,sqlParams);
                if (string.IsNullOrEmpty(result) || result == "0")
                    return false;
                else
                    return true;
            }
            catch (Exception ex)
            {
                Log.Error("驗證失敗！" + ex.StackTrace);
                Log.Error("驗證失敗！" + ex.Message);
                throw ex;
            }
        }
        private bool VerifyBy(String surveyId, int validField, String validData)
        {
            //CHT_IMPORT
            var sSql = " SELECT * FROM Contact ";
            var ColumnName = "";
            try
            {
                DataTable dtR;
                if (AppSettingsHelper.EnvSwitchToCRM.SwitchToCRM)
                {
                    dtR = CRMDbApiHelper.OutsideSurveyController_VerifyBy(surveyId,validField, validData);
                    if (dtR.Rows[0]["ContactCount"].ToString() != "0")
                    {
                        return true;
                    }
                    else
                    {
                        return false;
                    }

                }
                else
                {
                    dtR = _imptDB.GetQueryData(sSql);
                    switch (validField)
                    {
                        case 101:  //CHT會員編碼 SN
                            ColumnName = "SN";
                            break;
                        case 102:  //HN號碼 HN
                            ColumnName = "HN";
                            break;
                        case 103:  //MD號碼 MD
                            ColumnName = "MD";
                            break;
                        case 104:   //市話號碼 Telephone
                            ColumnName = "Telephone";
                            break;
                        case 105:    //電話號碼 CircuitNumber
                            ColumnName = "CircuitNumber";
                            break;
                        case 106:    //手機號碼 MobileNumber
                            ColumnName = "MobileNumber";
                            break;
                        case 107:    //主要聯絡信箱 EmailAddress1
                            ColumnName = "EMailAddress1";
                            break;
                        case 108:    //次要聯絡信箱 EmailAddress2
                            ColumnName = "EMailAddress2";
                            break;
                        case 109:     //Facebook使用者識別碼 FacebookUID
                            ColumnName = "FacebookUID";
                            break;
                        case 110:     //序號 SerialNumber
                            ColumnName = "SerialNumber";
                            break;
                        case 111:     //設備號碼 DeviceNumber
                            ColumnName = "DeviceNumber";
                            break;
                        case 202:     //備用欄位1 Spare_1
                            ColumnName = "Spare_1";
                            break;
                        case 203:     //備用欄位2 Spare_2
                            ColumnName = "Spare_2";
                            break;
                        default:
                            Log.Error($"validField[{validField}] Error!");
                            return false;
                    }
                    DataRow[] drs = dtR.Select($"{ColumnName}='{validData}'");
                    if (drs.Length == 0)
                    {
                        Log.Debug($"沒有找到{ColumnName}='{validData}'的資料！");
                        return false;
                    }
                    return true;
                }

            }

            catch (Exception ex)
            {
                Log.Error("驗證失敗！" + ex.StackTrace);
                Log.Error("驗證失敗！" + ex.Message);
                throw ex;
            }
            //if (validField.Equals("10") && validData.Equals("0987654321"))
            //{
            //    return true;
            //}
            //else
            //{
            //    return false;
            //}
        }
        #endregion

        #region 外部問卷生成
        /// <summary>
        /// 外部問卷生成
        /// </summary>
        /// <param name="value"></param>
        /// <returns></returns>
        [Route("RenderSurvey")]
        [HttpGet]
        public String RenderSurvey(String SurveyId, String OtherFlag)
        {
            //輸入：
            /* "SurveyId":1231,
            */
            //輸出：
            //{
            // "code": "200",
            // "message": "查詢記錄完成。",
            // "data": {
            //    "SurveyId": "99999998-0000-0000-0000-000000000000",
            //     "QuestionList": [
            //       {
            //        "QuestionId":"99999998-0000-0000-0000-000000000000",
            //        "OptionList[
            //            {
            //               OptionId:"99999998-0000-0000-0000-000000000000"
            //            }
            //         ]
            //       }
            //     ],
            //     "LogicList":[
            //        {
            //        }
            //     ],
            //    "SurveySetting":{
            //       "IsShowPageNo":true
            //      },
            //    "EndPage":{
            //        "EndPagePic": "iVBO"
            //       }
            // }
            string LogTimeID = Guid.NewGuid().ToString();
            Log.Debug("("+LogTimeID + ")RenderSurvey Start:"+System.DateTime.Now.ToString("yyyy/MM/dd HH:mm:ss.fff"));
            var replyData = new ReplyData();
            //SurveyId 必須有?
            if (SurveyId == null)
            {
                //報告錯誤
                replyData.code = "-1";
                replyData.message = $"參數SurveyId不能為空！";
                replyData.data = "";
                Log.Error("外部問卷生成參數SurveyId不能為空！");
                return JsonConvert.SerializeObject(replyData);
            }
            else if (!SurveyId.ToUpper().Equals(User.Identity.Name.ToUpper()))
            {
                //報告錯誤
                replyData.code = "-1";
                replyData.message = $"Token與問卷ID不符合！";
                replyData.data = "";
                Log.Error("Token與問卷ID不符合！:"+ SurveyId.ToUpper()+"，"+ User.Identity.Name.ToUpper());
                return JsonConvert.SerializeObject(replyData);
            }

            if (!SurveyWebAPI.Utility.Common.IsSurveyIdExist(SurveyId))
            {
                //報告錯誤
                replyData.code = "501";
                replyData.message = $"问卷不存在！";
                replyData.data = "";
                Log.Error($"外部問卷生成{SurveyId}不存在！");
                return JsonConvert.SerializeObject(replyData);
            }

            //OtherFlag： 有傳入且value=false時，不查詢“其他”，即只取QUE003中OtherFlag=false的資料；沒有傳入或True,全取（不過濾）
            bool ExcludeOther = false;      //是否排除“其他”選項
            if (OtherFlag != null && Convert.ToBoolean(OtherFlag) == false)
            {
                //OtherFlag有輸入，並且輸入了false，意思是排除“其他”，也就是需要排除QUE003.OtherFlag='true'的資料
                ExcludeOther = true;
                Log.Debug($"api/OutsideSurvey/RenderSurvey:{SurveyId}排除[其他]選項");
            }
            else
            {
                ExcludeOther = false;
                Log.Debug($"api/OutsideSurvey/RenderSurvey:{SurveyId}包含[其他]選項");
            }
            /* 測試錯誤訊息列表
            //var errlist = ErrorCode.ErrorCodes;
            //var msg = errlist.Where(t => t.Code.Equals("101")).FirstOrDefault().Message;
            //var ec = Utility.ErrorCode.errorCode;
            //var code = "200";
            //var ErrMsg = ec.Where(m => m.Code.Equals(code)).FirstOrDefault().Message;

            //var msg = ErrorCode.GetErrorMessageBy("101");
            ErrorCode.Code = "601";
            var a  = ErrorCode.Code;
            var b = ErrorCode.Message;
            */


            OutsideSurvey QS = new OutsideSurvey();
            try
            {
                //問卷基本資料
                QS = GetSurveyInfoBy(SurveyId);

                //問卷下的題目
                List<QuestionnairDetail> lstQD = new List<QuestionnairDetail>();   //QuestionId

                //取該問卷題目
                lstQD = GetQuestionInfo(SurveyId, ExcludeOther);
                if (lstQD.Count > 0)
                    QS.QuestionList = lstQD;
                //取該問卷邏輯
                List<RuleGroup> lstRuleGroup = new List<RuleGroup>();   //RuleGroup
                lstRuleGroup = GetLogicInfo(SurveyId);
                if (lstRuleGroup.Count > 0)
                    QS.LogicList = lstRuleGroup;
                //取該問卷基本設定from QUE004
                var SurveySetting = GetSurveySettingBy(SurveyId);
                QS.SurveySetting = SurveySetting;
                //取該問卷結束頁
                var EndPage = GetEndPageBy(SurveyId);
                QS.EndPage = EndPage;

                replyData.code = "200";
                replyData.message = $"外部問卷生成完成。";
                replyData.data = QS;

            }
            catch (Exception ex)
            {
                replyData.code = "-1";
                replyData.message = $"外部問卷生成失敗！{ex.Message}.";
                replyData.data = "";
                Log.Error("外部問卷生成失敗!" + ex.Message);
            }
            Log.Debug("(" + LogTimeID + ")RenderSurvey End:" + System.DateTime.Now.ToString("yyyy/MM/dd HH:mm:ss.fff"));
            //返回
            return JsonConvert.SerializeObject(replyData);

        }


        private EndPageSetting GetEndPageBy(String SurveyId)
        {
            EndPageSetting endPage = new EndPageSetting();
            //問卷基本資料
            var sSql = $" SELECT * FROM QUE007_QuestionnaireEndPage WHERE SurveyId=@SurveyId";

            //-------sql para----start
            SqlParameter[] sqlParams = new SqlParameter[] {
                new SqlParameter("@SurveyId", SqlDbType.UniqueIdentifier),


            };
            sqlParams[0].Value = SurveyId.ValidGuid();


            //-------sql para----end

            try
            {
                DataTable dtEndPage = _db.GetQueryData(sSql, sqlParams);
                if (dtEndPage.Rows.Count < 1)
                {
                    Log.Error($"問卷{SurveyId}結束頁設定不存在！");
                    return null;
                }
                //每個問卷Id資料只有一筆
                //endPage.SurveyId = SurveyId;
                DataRow dr0 = dtEndPage.Rows[0];
                //endPage.SurveyId = dr0["SurveyId"];
                endPage.EndPagePic = dr0["EndPagePic"];
                endPage.EndPageStyle = dr0["EndPageStyle"];
                endPage.ButtonSentence = dr0["ButtonSentence"];
                endPage.EnableRedirect = dr0["EnableRedirect"];
                endPage.RedirectUrl = dr0["RedirectUrl"];
                //endPage.UpdUserId = dr0["UpdUserId"];
                //endPage.UpdDateTime = dr0["UpdDateTime"];

                return endPage;
            }
            catch (Exception ex)
            {
                Log.Error(ex.Message);
                Log.Error(ex.StackTrace);
                throw ex;
            }
        }
        #endregion

        #region 外部問卷登入取得Token
        /// <summary>
        /// 外部問卷登入取得Token
        /// </summary>
        /// <param name="SurveyId">問卷Id</param>
        /// <param name="Env">1-正式環境；2-測試環境</param>
        /// <param name="ProvideType">收集方式</param>
        /// <param name="P">參數傳遞(若無則不須傳送)</param>
        /// <returns></returns>
        [AllowAnonymous]
        [Route("Login")]
        [HttpGet]
        public String Login(String SurveyId, String Env, String ProvideType, String P)
        {
            string LogTimeID = Guid.NewGuid().ToString();
            Log.Debug("(" + LogTimeID + ")Login Start:" + System.DateTime.Now.ToString("yyyy/MM/dd HH:mm:ss.fff"));
            var replyData = new ReplyData();
            //SurveyId 必須有?
            if (SurveyId == null)
            {
                //報告錯誤
                replyData.code = "-1";
                replyData.message = $"參數SurveyId不能為空！";
                replyData.data = "";
                Log.Error("外部問卷登入取得Token 參數SurveyId不能為空！");
                return JsonConvert.SerializeObject(replyData);
            }

            //Env 必須有?
            if (Env == null)
            {
                //報告錯誤
                replyData.code = "-1";
                replyData.message = $"參數Env不能為空！";
                replyData.data = "";
                Log.Error("外部問卷登入取得Token 參數Env不能為空！");
                return JsonConvert.SerializeObject(replyData);
            }
            else if (Env != "1" && Env != "2")
            {
                //報告錯誤
                replyData.code = "-1";
                replyData.message = $"參數Env只能為1（測試環境）或2（正式環境）！";
                replyData.data = "";
                Log.Error("外部問卷登入取得Token 參數Env只能為1（測試環境）或2（正式環境）！");
                return JsonConvert.SerializeObject(replyData);
            }
            //ProvideType 必須有?
            if (ProvideType == null)
            {
                //報告錯誤
                replyData.code = "-1";
                replyData.message = $"參數ProvideType不能為空！";
                replyData.data = "";
                Log.Error("外部問卷登入取得Token 參數ProvideType不能為空！");
                return JsonConvert.SerializeObject(replyData);
            }
            //P 可沒有
            //if (P == null)
            //{
            //    //報告錯誤
            //    replyData.code = "-1";
            //    replyData.message = $"參數P不能為空！";
            //    replyData.data = "";
            //    Log.Error("外部問卷登入取得Token 參數P不能為空！");
            //    return JsonConvert.SerializeObject(replyData);
            //}

            OutsideLogin login = new OutsideLogin();
            try
            {
                //check 問卷是否存在
                if(!Utility.Common.IsSurveyIdExist(SurveyId))
                {
                    Log.Error($"api/OutsideSurvey/Login:問卷{SurveyId}不存在！");
                    // 報告錯誤
                    ErrorCode.Code = "202";
                    replyData.code = ErrorCode.Code;
                    replyData.message = ErrorCode.Message;// "問卷不存在！";
                    replyData.data = "";
                    Log.Error($"api/OutsideSurvey/Login:問卷{SurveyId}不存在！");
                    return JsonConvert.SerializeObject(replyData);
                }
                int SurveyStatus = 1;
                //依據環境（正式機，測試機）分別check
                if(Env==null || Env=="1" || Env=="")  //測試機
                {
                    SurveyStatus = CheckTestEnvStatus(SurveyId, ProvideType);
                }
                else if(Env=="2")  //正式機
                {
                    SurveyStatus = CheckNormalEnvStatus(SurveyId, ProvideType, P);
                }
                //產生Token，不使用GUID改用JWT by Allen 20201005
                //var Token = Guid.NewGuid().ToString("D");
                login.Token = jwt.GenerateToken(SurveyId);
                login.SurveyStatus = SurveyStatus;
                //取得ValidRegister、ValidField
                var sSql = " SELECT ValidRegister, ValidField FROM QUE009_QuestionnaireProvideType " +
                    $" WHERE SurveyId=@SurveyId AND ProvideType=@ProvideType ";

                //-------sql para----start
                SqlParameter[] sqlParams = new SqlParameter[] {
                new SqlParameter("@SurveyId", SqlDbType.UniqueIdentifier),
                new SqlParameter("@ProvideType", SqlDbType.Int),


                };
                Log.Debug($"ProvideType:"+ ProvideType);
                sqlParams[0].Value = SurveyId.ValidGuid();
                sqlParams[1].Value = ProvideType.ValidInt();


                //-------sql para----end

                DataTable dtR = _db.GetQueryData(sSql, sqlParams);
                if (dtR.Rows.Count > 0)
                {
                    login.ValidRegister = dtR.Rows[0]["ValidRegister"];
                    login.ValidField = dtR.Rows[0]["ValidField"];
                }
                replyData.code = "200";
                replyData.message = $"外部問卷登入取得Token完成。";
                replyData.data = login;

            }
            catch (Exception ex)
            {
                replyData.code = "-1";
                replyData.message = $"外部問卷登入取得Token失敗！{ex.Message}.";
                replyData.data = "";
                Log.Error("外部問卷登入取得Token失敗!" + ex.Message);
            }
            Log.Debug("(" + LogTimeID + ")Login End:" + System.DateTime.Now.ToString("yyyy/MM/dd HH:mm:ss.fff"));
            //返回
            return JsonConvert.SerializeObject(replyData);

        }

        /// <summary>
        /// 外部填寫TOKEN驗證
        /// </summary>
        /// <returns></returns>
        [Authorize]
        [Route("tokenAuth")]
        [HttpGet]
        public String tokenAuth()
        {
            //JWT沒有擋的資料，則直接回傳合法。
             ReplyData replyData = new ReplyData();

            var key = User.Identity.Name; //抓出藏在token裡面的key，外部填寫是放SurveyId

            replyData.code = "200";
            replyData.message = $"token合法！";
            replyData.data = key;

            // 返回結果
            return JsonConvert.SerializeObject(replyData);
        }

        /// <summary>
        /// 測試環境下SurveyStatus
        /// </summary>
        /// <param name="surveyId">問卷ID</param>
        /// <param name="provideType">收集管道</param>
        /// <returns>SurveyStatus "6"-答卷數已超過30筆 "2"-正常</returns>
        private int CheckTestEnvStatus(String surveyId, String provideType)
        {
            //測試環境下，無時效檢查，也無參數檢驗
            int SurveyStatus = 1;
            var ReplyNum = 0;
            try
            {
                //檢查QUE021的筆數，Env: 1-測試， 2-正式
                var sSql = " SELECT COUNT(1) FROM QUE021_AnwserCollection " +
                    $" WHERE SurveyId=@surveyId AND ProvideType=@provideType " +
                    " AND (Env IS NOT NULL AND Env=1) AND (DelFlag='0' OR DelFlag IS NULL) ";

                //-------sql para----start
                SqlParameter[] sqlParams = new SqlParameter[] {
                new SqlParameter("@surveyId", SqlDbType.UniqueIdentifier),
                new SqlParameter("@provideType", SqlDbType.Int),


                };
                sqlParams[0].Value = surveyId.ValidGuid();
                sqlParams[1].Value = provideType.ValidInt();


                //-------sql para----end

                var result = _db.GetSingle(sSql, sqlParams);
                if (string.IsNullOrEmpty(result))
                    ReplyNum = 0;
                else
                    ReplyNum = Convert.ToInt32(result);
                if (ReplyNum >= 30)
                    SurveyStatus = 6;  //是否超過30筆(By SurveyId/ProvideType/Env)，如果是的話，SurveyStatus=6
                else
                    SurveyStatus = 2;  //通過以上檢查，則SurveyStatus = 2;

                return SurveyStatus;
            }
            catch (Exception ex)
            {
                Log.Error("api/OutsideSurvey/Login:" + ex.Message);
                Log.Error("api/OutsideSurvey/Login:" + ex.StackTrace);
                throw ex;
            }
        }
        /// <summary>
        /// 正式環境 SurveyStatus
        /// </summary>
        /// <param name="surveyId">問卷ID</param>
        /// <param name="provideType">收集管道</param>
        /// <param name="P">參數</param>
        /// <returns>SurveyStatus:"1"-時效小於開始時間 "3"-時效大於結束時間 "4"-有答卷上限且額滿 "5"-p值驗證失敗 "2"-正常 </returns>
        private int CheckNormalEnvStatus(String surveyId, String provideType, String P)
        {
            int SurveyStatus = 1;
            var ReplyMaxNum = 0;   //問卷答數上限，0表無限
            bool audited = false;

            //以下檢查步驟，任意一個滿足就return
            try
            {
                //1. 檢查SurveyId的時效，小於開始時間SurveyStatus=1，大於結束時間SurveyStatus=3
                //   因為時效從CRM來，目前暫預留
                SurveyStatus = CheckValidDateTimeFromCRM(surveyId);
                if (SurveyStatus != 0)
                    return SurveyStatus;

                //2. 檢查問卷填寫是否額滿，QUE009.ReplyMaxNum = 0 表示無上限，不需檢查，
                //   反之，QUE009.FullEndFlag是否為TRUE(By SurveyId/ProvideType)，為TRUE則SurveyStatus=4
                var sSql = String.Format("SELECT QUE009.ReplyMaxNum, QUE009.FullEndFlag, QUE009.ValidRegister, QUE009.ValidField, count(QUE021.ReplyKey) as Amount, QUE001.Audit FROM QUE009_QuestionnaireProvideType  QUE009 "
                                          + "LEFT JOIN QUE001_QuestionnaireBase QUE001 "
                                          + "ON QUE009.SurveyId = QUE001.SurveyId and(QUE001.DelFlag = '0' OR QUE001.DelFlag IS NULL) "
                                          + "LEFT JOIN QUE021_AnwserCollection QUE021 "
                                          + "ON QUE009.SurveyId = QUE021.SurveyId and QUE009.ProvideType = QUE021.ProvideType and QUE021.Env = 2 and(QUE021.DelFlag = '0' OR QUE021.DelFlag IS NULL) "
                                          + "WHERE QUE009.SurveyId = @surveyId AND QUE009.ProvideType =@provideType "
                                          + "group by QUE009.ReplyMaxNum, QUE009.FullEndFlag, QUE009.ValidRegister, QUE009.ValidField ,QUE001.Audit ");
                //-------sql para----start
                SqlParameter[] sqlParams = new SqlParameter[] {
                new SqlParameter("@surveyId", SqlDbType.UniqueIdentifier),
                new SqlParameter("@provideType", SqlDbType.Int),


                };
                sqlParams[0].Value = surveyId.ValidGuid();
                sqlParams[1].Value = provideType.ValidInt();


                //-------sql para----end

                DataTable dtR = _db.GetQueryData(sSql, sqlParams);
                if(dtR.Rows.Count>0)
                {
                    //上限
                    ReplyMaxNum = dtR.Rows[0]["ReplyMaxNum"] == DBNull.Value ? 0 : Convert.ToInt32(dtR.Rows[0]["ReplyMaxNum"]);
                    audited = dtR.Rows[0]["Audit"] == null ? false : Convert.ToBoolean(Convert.ToInt32(dtR.Rows[0]["Audit"]));
                }
                if (ReplyMaxNum != 0)
                {
                    //額滿？
                    var FullEndFlag = dtR.Rows[0]["FullEndFlag"] == DBNull.Value ? false : Convert.ToBoolean(dtR.Rows[0]["FullEndFlag"]);
                    var Amount = dtR.Rows[0]["Amount"] == DBNull.Value ? 0 : Convert.ToInt32(dtR.Rows[0]["Amount"]);
                    //有上限且額滿，返回SurveyStatus=4
                    if (FullEndFlag || Amount> ReplyMaxNum)
                    {
                        SurveyStatus = 4;
                        return SurveyStatus;
                    }
                }
                //問卷平台未呈核，不能填寫正式問卷。
                if (!audited)
                {
                    SurveyStatus = 1;
                    return SurveyStatus;
                }
                //3. 若p有值，則用問卷ID到CRM檢查p值是否驗證成功，失敗的話SurveyStatus=5
                if(P!=null && P.Trim()!="")
                {
                    Log.Debug("有P參數，需要用問卷ID到CRM檢查p值是否驗證成功,預留代開發...");
                    //暫時預留
                    //SurveyStatus = VerifyPFromCRM(P);
                    //if(SurveyStatus!="")
                    //    return "5";
                }
                //4. 通過以上檢查，則SurveyStatus = 2;
                SurveyStatus = 2;  //通過以上檢查，則SurveyStatus = 2;
                return SurveyStatus;
            }
            catch (Exception ex)
            {
                Log.Error("api/OutsideSurvey/Login:" + ex.Message);
                Log.Error("api/OutsideSurvey/Login:" + ex.StackTrace);
                throw ex;
            }
        }
        /// <summary>
        /// 檢查SurveyId的時效，小於開始時間SurveyStatus=1，大於結束時間SurveyStatus=3
        /// </summary>
        /// <param name="surveyId"></param>
        /// <returns></returns>
        private int CheckValidDateTimeFromCRM(String surveyId)
        {
            var sSql = $" SELECT  CONVERT(varchar(20),GETDATE() ,120) AS SysDateTime, New_effectivestart, New_effectiveend, New_statuscode, statuscode " +
                " FROM CampaignActivity " +
                $" WHERE ActivityId=@surveyId " ;

            //-------sql para----start
            SqlParameter[] sqlParams = new SqlParameter[] {
                new SqlParameter("@surveyId", SqlDbType.UniqueIdentifier),

                };
            sqlParams[0].Value = surveyId.ValidGuid();
            //-------sql para----end

            //allen說，要拿系統當前日期時間和start,end去比較
            try
            {
                DataTable dt;
                if(AppSettingsHelper.EnvSwitchToCRM.SwitchToCRM)
                {
                    Log.Debug("OutsideSurveyController_CheckValidDateTimeFromCRM");
                    dt = CRMDbApiHelper.OutsideSurveyController_CheckValidDateTimeFromCRM(surveyId);
                }
                else
                {
                    dt = _crmDB.GetQueryData(sSql,sqlParams);
                }

                if (dt.Rows.Count > 0)
                {
                    DataRow dr = dt.Rows[0];
                    var New_effectivestart = dr["New_effectivestart"] == DBNull.Value ? "" : ((DateTime)dr["New_effectivestart"]).ToString("yyyy-MM-dd HH:mm:ss");
                    var New_effectiveend = dr["New_effectiveend"] == DBNull.Value ? "" : ((DateTime)dr["New_effectiveend"]).ToString("yyyy-MM-dd HH:mm:ss");
                    var SysDateTime = dr["SysDateTime"].ToString();
                    var New_statuscode = dr["New_statuscode"];
                    var statuscode = dr["statuscode"];
                    //檢查SurveyId的時效，小於開始時間SurveyStatus = 1，大於結束時間SurveyStatus = 3
                    /*
                     *  1	建立中
                        2	主管審核中
                        3	執行單位審核中
                        4	修改中
                        5	已決行
                        6	執行中
                        7	已完成
                        8	已取消
                        9	個資窗口審核中
                     */

                    //在有效時間內，只有行銷活動方式狀態=已決行(New_statuscode=5)和執行狀態=進行中(statuscode=0)，才可以填寫問卷
                    if (String.Compare(SysDateTime, New_effectivestart) < 0 || Convert.ToInt32(New_statuscode) < 5 )
                    {
                        //問卷未開放填寫
                        Log.Debug($"current datetime:{SysDateTime},New_effectivestart:{New_effectivestart}");
                        return 1;
                    }
                    else if (String.Compare(SysDateTime, New_effectiveend) > 0 || Convert.ToInt32(New_statuscode) > 6)
                    {
                        //問卷已逾期
                        Log.Debug($"current datetime:{SysDateTime},New_effectiveend:{New_effectiveend}");
                        return 3;
                    }

                    //新增判斷執行狀態要為"進行中"
                    if (Convert.ToInt32(statuscode) != 0)
                    {
                        //問卷未開放填寫
                        Log.Debug($"執行狀態不為進行中，statuscode:{statuscode}");
                        return 1;
                    }
                }
                else
                {
                    Log.Debug($"CRM not exist SurveyId");
                    return 7;
                }
                return 0;
            }
            catch(Exception ex)
            {
                Log.Error("CheckValidDateTimeFromCRM:" + ex.Message);
                Log.Error("CheckValidDateTimeFromCRM:" + ex.StackTrace);
                throw ex;
            }
        }
        #endregion
    }
    public class OutsideSurvey
    {
        /// <summary>
        /// 問卷ID --uniqueidentifier
        /// </summary>
        public Object SurveyId { get; set; }
        /// <summary>
        /// 問卷名稱 --nvarchar
        /// </summary>
        public Object Title { get; set; }
        /// <summary>
        /// 正式網址 --nvarchar
        /// </summary>
        public Object FinalUrl { get; set; }
        /// <summary>
        /// 感謝詞  --nvarchar
        /// </summary>
        public Object ThankWords { get; set; }
        /// <summary>
        /// 結束動作  --int
        /// </summary>
        public Object DueAction { get; set; }
        /// <summary>
        /// 刪除註記  --bit
        /// </summary>
        public Object DelFlag { get; set; }
        /// <summary>
        /// 是否記名  --bit
        /// </summary>
        public Object Audit { get; set; }
        /// <summary>
        /// 建立人員  --uniqueidentifier
        /// </summary>
        public Object CreateUserId { get; set; }
        /// <summary>
        /// 建立時間  --datetime2
        /// </summary>
        public Object CreateDateTime { get; set; }
        /// <summary>
        /// 更新人員  --uniqueidentifier
        /// </summary>
        public Object UpdUserId { get; set; }
        /// <summary>
        /// 更新日期時間  --datetime2
        /// </summary>
        public Object UpdDateTime { get; set; }
        /// <summary>
        /// 外觀類型  --int
        /// </summary>
        public Object StyleType { get; set; }
        /// <summary>
        /// 問卷底色 --varchar
        /// </summary>
        public Object DefBackgroudColor { get; set; }
        /// <summary>
        /// 問卷表頭圖片 --varchar
        /// </summary>
        public Object DefHeaderPic { get; set; }
        /// <summary>
        /// 手機版表頭圖片 --varchar
        /// </summary>
        public Object DefHeaderPhonePic { get; set; }
        /// <summary>
        /// 題目   --在SurveyDetailController.cs中定義
        /// </summary>
        public List<QuestionnairDetail> QuestionList { get; set; }
        /// <summary>
        /// 邏輯   --在 Model-->Logic.cs中定義
        /// </summary>
        public List<RuleGroup> LogicList { get; set; }
        /// <summary>
        /// 問卷基本設定
        /// </summary>
        public QUE004_QuestionnaireSetting SurveySetting { get; set; }
        /// <summary>
        /// 結束頁
        /// </summary>
        public EndPageSetting EndPage { get; set; }
    }
    /// <summary>
    /// 邏輯規則主表
    /// </summary>
    public class RuleGroup
    {
        /// <summary>
        /// 邏輯ID --uniqueidentifier
        /// </summary>
        public Object LogicId { get; set; }
        /// <summary>
        /// 邏輯規則類型   --int
        /// </summary>
        public Object LogicType { get; set; }
        /// <summary>
        /// 邏輯條件  --int
        /// </summary>
        public Object LogicCondition { get; set; }
        /// <summary>
        /// 跳題題目  --uniqueidentifier
        /// </summary>
        public Object TargetQuestionId { get; set; }
        /// <summary>
        /// 選項不可選  varchar
        /// </summary>
        public Object BlockOptionList { get; set; }
        /// <summary>
        /// 邏輯規則條件
        /// </summary>
        public List<RuleCondition> ConditionList { get; set; }
    }
    /// <summary>
    /// 邏輯規則條件
    /// </summary>
    public class RuleCondition
    {
        /// <summary>
        /// 條件ID  --uniqueidentifier
        /// </summary>
        public Object Id { get; set; }
        /// <summary>
        /// 條件規則  --int
        /// </summary>
        public Object ConditionRule { get; set; }
        /// <summary>
        /// 條件題目  --varchar
        /// </summary>
        public Object ConditionQuestionList { get; set; }
        /// <summary>
        /// 條件選項  --varchar
        /// </summary>
        public Object ConditionOptionList { get; set; }
        public Object MatrixField { get; set; }
    }

    public class EndPageSetting
    {
  //      /// <summary>
		///// 問卷ID  --uniqueidentifier
		///// </summary>
		//public Object SurveyId { get; set; }
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
        ///// <summary>
        ///// 更改人員
        ///// </summary>
        //public Object UpdUserId { get; set; }
        ///// <summary>
        ///// 更改時間
        ///// </summary>
        //public Object UpdDateTime { get; set; }
    }
    public class OutsideLogin
    {
        /// <summary>
        /// Token Value
        /// </summary>
        public Object Token { get; set; }
        /// <summary>
        /// 外部問卷狀態判斷(1:未開放，2. 開放填寫中，3. 已截止， 4. 已額滿，5.參數傳遞驗證失敗)
        /// </summary>
        public Object SurveyStatus { get; set; }
        /// <summary>
        /// 驗證欄位(登入方式為2時，才有值)
        /// </summary>
        public Object ValidRegister { get; set; }
        /// <summary>
        /// 登入方式
        /// </summary>
        public Object ValidField { get; set; }
    }
    public class OutsideSubmit
    {
        /// <summary>
        /// 外部問卷狀態判斷(1:未開放，2. 開放填寫中，3. 已截止， 4. 已額滿，5.參數傳遞驗證失敗)
        /// </summary>
        public Object SurveyStatus { get; set; }
    }
}
