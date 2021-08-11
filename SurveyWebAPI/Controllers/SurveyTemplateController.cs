using System;
using System.Collections.Generic;
using System.Data;
using System.Diagnostics;
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
using System.Data.SqlClient;

namespace SurveyWebAPI.Controllers
{
    [Authorize]
    [Route("api/Survey/Template")]
    [ApiController]
    public class SurveyTemplateController : ControllerBase
    {
        private DBHelper _db;
        public SurveyTemplateController()
        {
            _db = new DBHelper(AppSettingsHelper.DefaultConnectionString);
        }

        [Route("List")]
        [HttpGet]
        public object TemplateList()
        {
            Log.Debug("我的範本-列表...");

            var replyData = new ReplyData();

            //if (string.IsNullOrEmpty(SurveyId))
            //{
            //    replyData.code = "-1";
            //    replyData.message = $"查詢記錄失敗!參數SurveyId不能為空!";
            //    replyData.data = "";
            //    Log.Error("查詢記錄失敗!" + "參數SurveyId不能為空！");
            //    return JsonConvert.SerializeObject(replyData);
            //}
            //題型計算QuestionType<10，表示可操作題型，QuestionType>=10，表示不可操作之題型，不列入可操作題數計算
            try
            {
                var key = User.Identity.Name;
                var info = Utility.Common.GetConnectionInfo(key);
                var userId = info.UserId;
                string sqlS = $"select a.TemplateId,a.Subject,a.TotalQuestionNum from"
                                        +" (select a.TemplateId,a.Title as Subject,a.UpdDateTime,count(b.QuestionId) as TotalQuestionNum from QUE011_TemplateBase a "
                                        +" left join QUE012_TemplateDetail b on a.TemplateId=b.TemplateId where a.DelFlag='false'  and b.QuestionType < 10 and CreateUserId =@userId "
                                        +" group by a.TemplateId,a.Title,a.UpdDateTime  "
                                        +" ) a order by a.UpdDateTime desc ";
                Log.Debug("我的範本-列表:" + sqlS);
                //-------sql para----start
                SqlParameter[] sqlParams = new SqlParameter[] {
                    new SqlParameter("@userId", SqlDbType.UniqueIdentifier)
                };
                sqlParams[0].Value = userId.ValidGuid();
                //-------sql para----end
                DataTable dtList = _db.GetQueryData(sqlS, sqlParams);

                if (dtList == null || dtList.Rows.Count == 0)
                {
                    replyData.code = "200";
                    replyData.message = $"查無資料!";
                    replyData.data = null;
                }
                else
                {
                    SurveyTemplate[] templates = new SurveyTemplate[dtList.Rows.Count];
                    foreach (DataRow dr in dtList.Rows)
                    {
                        SurveyTemplate template = new SurveyTemplate();
                        template.TemplateId = dr["TemplateId"].ToString().Trim();
                        template.Subject = dr["Subject"].ToString().Trim();
                        template.TotalQuestionNum = Convert.ToInt32(dr["TotalQuestionNum"]);
                        templates[dtList.Rows.IndexOf(dr)] = template;
                    }
                    replyData.code = "200";
                    replyData.message = $"查詢記錄完成。";
                    replyData.data = templates;
                }
            }
            catch (Exception ex)
            {
                replyData.code = "-1";
                replyData.message = $"查詢記錄失敗!{ex.Message}.";
                replyData.data = null;
                Log.Error("查詢記錄失敗!" + ex.Message);
            }
            return JsonConvert.SerializeObject(replyData);
        }

        [Route("Add")]
        [HttpPost]
        public object TemplateAdd([FromBody] Object value)
        {
            Log.Debug("我的範本-新增...");

            var replyData = new ReplyData();

            JObject jo = (JObject)JsonConvert.DeserializeObject(value.ToString());

            if (jo["SurveyId"] == null || String.IsNullOrWhiteSpace(jo["SurveyId"].ToString()))
            {
                replyData.code = "-1";
                replyData.message = $"我的範本-新增失敗!參數SurveyId不能為空!";
                replyData.data = "";
                Log.Error("我的範本-新增失敗!" + "參數SurveyId不能為空！");
                return JsonConvert.SerializeObject(replyData);
            }
            var SurveyId = jo["SurveyId"].ToString();
            if (jo["Title"] == null || String.IsNullOrWhiteSpace(jo["Title"].ToString()))
            {
                replyData.code = "-1";
                replyData.message = $"我的範本-新增失敗!參數Title不能為空!";
                replyData.data = "";
                Log.Error("我的範本-新增失敗!" + "參數Title不能為空！");
                return JsonConvert.SerializeObject(replyData);
            }
            var Title = jo["Title"].ToString();
            try
            {
                string sqlS = "";

                // 1.需要檢查，問卷是否為空白(QUE002沒有資料就算是空白)，空白問卷不能新增套範本
                sqlS = $"select count(1) as num from QUE002_QuestionnaireDetail where SurveyId=@SurveyId ";
                //-------sql para----start
                SqlParameter[] sqlParams = new SqlParameter[] {
                    new SqlParameter("@SurveyId", SqlDbType.UniqueIdentifier)
                };
                sqlParams[0].Value = SurveyId.ValidGuid();
                //-------sql para----end
                Log.Debug("我的範本-新增:" + sqlS);
                string rowCount = _db.GetSingle(sqlS, sqlParams);
                if (int.Parse(rowCount) == 0)
                {
                    ErrorCode.Code = "701";
                    replyData.code = ErrorCode.Code;
                    replyData.message = ErrorCode.Message;
                    replyData.data = "";
                    Log.Error("我的範本-新增失敗!" + "空白問卷不能新增範本！");
                    return JsonConvert.SerializeObject(replyData);
                }
                string templateId = Guid.NewGuid().ToString();
                var key = User.Identity.Name;
                var info = Utility.Common.GetConnectionInfo(key);
                if (info == null)
                {
                    //報告錯誤
                    replyData.code = "-1";
                    replyData.message = "用戶不存在!";
                    replyData.data = "";
                    return JsonConvert.SerializeObject(replyData);
                }
                var userId = info.UserId;
                //var UpdDateTime = DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss.fffffff");

                sqlS = @"insert into QUE011_TemplateBase
(TemplateId,Title,FinalUrl,ThankWords,DueAction,DelFlag,Audit,CreateUserId,CreateDateTime,UpdUserId,UpdDateTime,StyleType,DefBackgroudColor,SurveyId,DefHeaderPic,DefHeaderPhonePic) 
SELECT @templateId,@Title,FinalUrl,ThankWords,DueAction,DelFlag,Audit,@userId,SYSDATETIME(),@userId,SYSDATETIME(),StyleType,DefBackgroudColor,SurveyId,DefHeaderPic,DefHeaderPhonePic 
from QUE001_QuestionnaireBase where SurveyId=@SurveyId";
                //-------sql para----start
                SqlParameter[] sqlParamsA = new SqlParameter[] {
                    new SqlParameter("@templateId", SqlDbType.UniqueIdentifier),
                    new SqlParameter("@Title", SqlDbType.NVarChar),
                    new SqlParameter("@userId", SqlDbType.UniqueIdentifier),
                    new SqlParameter("@SurveyId", SqlDbType.UniqueIdentifier)
                };
                sqlParamsA[0].Value = templateId.ValidGuid();
                sqlParamsA[1].Value = Title.Valid();
                sqlParamsA[2].Value = userId.ValidGuid();
                sqlParamsA[3].Value = SurveyId.ValidGuid();
                //-------sql para----end

                Log.Debug("Insert QUE011_TemplateBase:" + sqlS);
                int iR = _db.ExecuteSql(sqlS, sqlParamsA);

                sqlS = @"SELECT SurveyId,QuestionId from QUE002_QuestionnaireDetail where SurveyId=@SurveyId  ORDER BY QuestionSeq,UpdDateTime";
                //-------sql para----start
                SqlParameter[] sql1Params = new SqlParameter[] {
                    new SqlParameter("@SurveyId", SqlDbType.UniqueIdentifier)
                };
                sql1Params[0].Value = SurveyId.ValidGuid();
                //-------sql para----end
                DataTable dtQue002 = _db.GetQueryData(sqlS, sql1Params);
                Dictionary<string, string> dicQuestions = new Dictionary<string, string>();
                //Dictionary<string, string> dicOptions = new Dictionary<string, string>();
                foreach (DataRow dr in dtQue002.Rows)
                {
                    string questionId = dr["QuestionId"].ToString();
                    string newQuestionId = Guid.NewGuid().ToString();

                    sqlS = @"insert into QUE012_TemplateDetail 
                        (TemplateId,QuestionId,QuestionSeq,QuestionType,QuestionSubject,SubjectStyle,QuestionNote,PageNo,IsRequired,HasOther,OtherIsShowText,OtherVerify,OtherTextMandatory,OtherCheckMessage,IsSetShowNum,
                        PCRowNum,MobileRowNum,IsRamdomOption,ExcludeOther,BaseDataValidType,BlankDefaultWords,BlankValidType,MatrixItems,UpdUserId,UpdDateTime,BlankMaxLimit,BlankMinLimit,QuestionImage,QuestionVideo,MultiOptionLimit,OtherMinLimit,OtherMaxLimit,OtherChildQuestionId,OrgQuestionId)
                        SELECT @templateId,@newQuestionId,QuestionSeq,QuestionType,QuestionSubject,SubjectStyle,QuestionNote,PageNo,IsRequired,HasOther,OtherIsShowText,OtherVerify,OtherTextMandatory,OtherCheckMessage,IsSetShowNum,
                        PCRowNum,MobileRowNum,IsRamdomOption,ExcludeOther,BaseDataValidType,BlankDefaultWords,BlankValidType,MatrixItems,@userId,SYSDATETIME(),BlankMaxLimit,BlankMinLimit,QuestionImage,QuestionVideo,MultiOptionLimit ,OtherMinLimit,OtherMaxLimit,OtherChildQuestionId, @questionId  
                        from QUE002_QuestionnaireDetail where SurveyId=@SurveyId and QuestionId=@questionId";
                    Log.Debug("Insert QUE012_TemplateDetail:" + sqlS);
                    //-------sql para----start
                    SqlParameter[] sqlParamsB = new SqlParameter[] {
                        new SqlParameter("@templateId", SqlDbType.UniqueIdentifier),
                        new SqlParameter("@newQuestionId", SqlDbType.UniqueIdentifier),
                        new SqlParameter("@userId", SqlDbType.UniqueIdentifier),
                        new SqlParameter("@SurveyId", SqlDbType.UniqueIdentifier),
                        new SqlParameter("@questionId", SqlDbType.UniqueIdentifier)
                    };
                    sqlParamsB[0].Value = templateId.ValidGuid();
                    sqlParamsB[1].Value = newQuestionId.ValidGuid();
                    sqlParamsB[2].Value = userId.ValidGuid();
                    sqlParamsB[3].Value = SurveyId.ValidGuid();
                    sqlParamsB[4].Value = questionId.ValidGuid();
                    //-------sql para----end
                    iR += _db.ExecuteSql(sqlS, sqlParamsB);
                    dicQuestions.Add(questionId, newQuestionId);


                }

                foreach (DataRow dr in dtQue002.Rows)
                {
                    string questionId = dr["QuestionId"].ToString();
                    string newQuestionId = dicQuestions[questionId].ToString();

                    sqlS = @"insert into QUE013_TemplateOptions 
                        (QuestionId,OptionId,OptionSeq,OptionType,OptionContent,ChildQuestionId,UpdUserId,UpdDateTime,OptionImage,OptionVideo,OtherFlag,OrgOptionId)
                        SELECT @newQuestionId,NEWID(),QUE003.OptionSeq,QUE003.OptionType,QUE003.OptionContent,QUE012.QuestionId,@userId,SYSDATETIME(),QUE003.OptionImage,QUE003.OptionVideo,QUE003.OtherFlag,QUE003.OptionId
                        from QUE003_QuestionnaireOptions QUE003 
                        LEFT JOIN QUE012_TemplateDetail QUE012 ON QUE003.ChildQuestionId = QUE012.OrgQuestionId AND QUE012.TemplateId = @templateId
                        where QUE003.QuestionId=@questionId
                        group by QUE003.OptionSeq,QUE003.OptionType,QUE003.OptionContent,QUE012.QuestionId,QUE003.OptionImage,QUE003.OptionVideo,QUE003.OtherFlag,QUE003.OptionId";
                    Log.Debug("Insert QUE013_TemplateOptions:" + sqlS);
                    //-------sql para----start
                    SqlParameter[] sqlParamsC = new SqlParameter[] {
                        new SqlParameter("@newQuestionId", SqlDbType.UniqueIdentifier),
                        new SqlParameter("@userId", SqlDbType.UniqueIdentifier),
                        new SqlParameter("@questionId", SqlDbType.UniqueIdentifier),
                        new SqlParameter("@templateId", SqlDbType.UniqueIdentifier)
                    };
                    sqlParamsC[0].Value = newQuestionId.ValidGuid();
                    sqlParamsC[1].Value = userId.ValidGuid();
                    sqlParamsC[2].Value = questionId.ValidGuid();
                    sqlParamsC[3].Value = templateId.ValidGuid();
                    //-------sql para----end
                    iR += _db.ExecuteSql(sqlS, sqlParamsC);
                }


                sqlS = @"insert into QUE014_TemplateSetting 
                    (TemplateId,IsShowPageNo,IsShowQuestionNo,IsShowRequiredStar,IsShowProgress,PorgressPosition,ProgressStyle,UseVirifyCode,
                    IsOneQuestionPerPage,IsPublishResult,IsShowEndPage,UpdUserId,UpdDateTime,IsShowOptionNo)
                    select @templateId,IsShowPageNo,IsShowQuestionNo,IsShowRequiredStar,IsShowProgress,PorgressPosition,ProgressStyle,UseVirifyCode,
                    IsOneQuestionPerPage,IsPublishResult,IsShowEndPage,@userId,SYSDATETIME(),IsShowOptionNo  
                    from QUE004_QuestionnaireSetting where SurveyId = @SurveyId";
                Log.Debug("Insert QUE014_TemplateSetting:" + sqlS);
                //-------sql para----start
                SqlParameter[] sqlParamsD = new SqlParameter[] {
                    new SqlParameter("@templateId", SqlDbType.UniqueIdentifier),
                    new SqlParameter("@userId", SqlDbType.UniqueIdentifier),
                    new SqlParameter("@SurveyId", SqlDbType.UniqueIdentifier)
                };
                sqlParamsD[0].Value = templateId.ValidGuid();
                sqlParamsD[1].Value = userId.ValidGuid();
                sqlParamsD[2].Value = SurveyId.ValidGuid();
                //-------sql para----end
                iR += _db.ExecuteSql(sqlS, sqlParamsD);

                //新增邏輯規則主表，TargetQuestionId，BlockOptionList要用新的，邏輯規則要注意資料排序
                sqlS = @"SELECT SurveyId,LogicId from QUE005_QuestionnaireRuleGroup where SurveyId=@SurveyId ORDER BY CreateDateTime";
                //-------sql para----start
                SqlParameter[] sqlParamsE = new SqlParameter[] {
                    new SqlParameter("@SurveyId", SqlDbType.UniqueIdentifier)
                };
                sqlParamsE[0].Value = SurveyId.ValidGuid();
                //-------sql para----end
                DataTable dtQue005 = _db.GetQueryData(sqlS, sqlParamsE);
                Dictionary<string, string> dicLogics = new Dictionary<string, string>();

                foreach (DataRow dr in dtQue005.Rows)
                {
                    string logicId = dr["LogicId"].ToString();
                    string newLogicId = Guid.NewGuid().ToString();
                    sqlS = string.Format(@"insert into QUE015_TemplateRuleGroup 
                        (TemplateId,LogicId,LogicType,LogicCondition,TargetQuestionId,BlockOptionList,CreateUserID,CreateDateTime,OrgLogicId)
                        SELECT @templateId,@newLogicId,QUE005.LogicType,QUE005.LogicCondition,QUE012.QuestionId,ISNULL(BOList.NewOption,''),@userId,SYSDATETIME(),@logicId
                        FROM QUE005_QuestionnaireRuleGroup QUE005
                        LEFT JOIN QUE012_TemplateDetail QUE012 ON QUE005.TargetQuestionId = QUE012.OrgQuestionId AND QUE012.TemplateId=@templateId 
                        LEFT JOIN GetQUE015NewBlockOptionList(@SurveyId,@templateId) BOList ON QUE005.LogicId = BOList.LogicId 
                        WHERE QUE005.SurveyId=@SurveyId and QUE005.LogicId=@logicId ");
                    Log.Debug("Insert QUE015_TemplateRuleGroup:" + sqlS);
                    //-------sql para----start
                    SqlParameter[] sqlParamsF = new SqlParameter[] {
                        new SqlParameter("@templateId", SqlDbType.UniqueIdentifier),
                        new SqlParameter("@newLogicId", SqlDbType.UniqueIdentifier),
                        new SqlParameter("@userId", SqlDbType.UniqueIdentifier),
                        new SqlParameter("@SurveyId", SqlDbType.UniqueIdentifier),
                        new SqlParameter("@logicId", SqlDbType.UniqueIdentifier)
                    };
                    sqlParamsF[0].Value = templateId.ValidGuid();
                    sqlParamsF[1].Value = newLogicId.ValidGuid();
                    sqlParamsF[2].Value = userId.ValidGuid();
                    sqlParamsF[3].Value = SurveyId.ValidGuid();
                    sqlParamsF[4].Value = logicId.ValidGuid();
                    //-------sql para----end
                    iR += _db.ExecuteSql(sqlS, sqlParamsF);
                    dicLogics.Add(logicId, newLogicId);

                    //新增邏輯規則明細，ConditionQuestionList，ConditionOptionList要用新的
                    sqlS = @"insert into QUE016_TemplateRuleCondition 
                        (LogicId,ConditionRule,ConditionQuestionList,ConditionOptionList,MatrixField,CreateDateTime,CreateUserId,OrgId)
                        SELECT @newLogicId,QUE006.ConditionRule,ISNULL(CQList.NewQuestion,''),ISNULL(COList.NewOption,''),QUE006.MatrixField,SYSDATETIME(),@userId ,QUE006.Id
                        from QUE006_QuestionnaireRuleCondition QUE006
                        LEFT JOIN [dbo].[GetQUE016NewConditionOptionList](@logicId,@templateId) COList ON QUE006.Id = COList.Id 
						LEFT JOIN [dbo].[GetQUE016NewConditionQuestionList](@logicId,@templateId) CQList ON   QUE006.Id = CQList.Id 
                        WHERE QUE006.LogicId=@logicId";
                    Log.Debug("Insert QUE016_TemplateRuleCondition:" + sqlS);
                    //-------sql para----start
                    SqlParameter[] sqlParamsG = new SqlParameter[] {
                        new SqlParameter("@newLogicId", SqlDbType.UniqueIdentifier),
                        new SqlParameter("@userId", SqlDbType.UniqueIdentifier),
                        new SqlParameter("@logicId", SqlDbType.UniqueIdentifier),
                        new SqlParameter("@templateId", SqlDbType.UniqueIdentifier)
                    };
                    sqlParamsG[0].Value = newLogicId.ValidGuid();
                    sqlParamsG[1].Value = userId.ValidGuid();
                    sqlParamsG[2].Value = logicId.ValidGuid();
                    sqlParamsG[3].Value = templateId.ValidGuid();
                    //-------sql para----end
                    iR += _db.ExecuteSql(sqlS, sqlParamsG);
                }

                SurveyTemplateAdd addData = new SurveyTemplateAdd();
                addData.TemplateId = templateId;
                addData.Title = Title;

                replyData.code = "200";
                replyData.message = $"新增範本完成。";
                replyData.data = addData;
            }
            catch (Exception ex)
            {
                replyData.code = "-1";
                replyData.message = $"新增範本失敗!{ex.Message}.";
                replyData.data = null;
                Log.Error("新增範本失敗!" + ex.Message);
            }
            return JsonConvert.SerializeObject(replyData);
        }

        [Route("Apply")]
        [HttpPost]
        public object TemplateApply([FromBody] Object value)
        {
            Log.Debug("我的範本-套用...");

            var replyData = new ReplyData();

            JObject jo = (JObject)JsonConvert.DeserializeObject(value.ToString());

            if (jo["SurveyId"] == null || String.IsNullOrWhiteSpace(jo["SurveyId"].ToString()))
            {
                replyData.code = "-1";
                replyData.message = $"我的範本-套用失敗!參數SurveyId不能為空!";
                replyData.data = "";
                Log.Error("我的範本-套用失敗!" + "參數SurveyId不能為空！");
                return JsonConvert.SerializeObject(replyData);
            }
            if (jo["TemplateId"] == null || String.IsNullOrWhiteSpace(jo["TemplateId"].ToString()))
            {
                replyData.code = "-1";
                replyData.message = $"我的範本-套用失敗!參數TemplateId不能為空!";
                replyData.data = "";
                Log.Error("我的範本-套用失敗!" + "參數TemplateId不能為空！");
                return JsonConvert.SerializeObject(replyData);
            }
            var SurveyId = jo["SurveyId"].ToString();
            var TemplateId = jo["TemplateId"].ToString();

            try
            {
                string sqlS = "";
                int iR = 0;
                int dR = 0;

                var key = User.Identity.Name;
                var info = Utility.Common.GetConnectionInfo(key);
                if (info == null)
                {
                    //報告錯誤
                    replyData.code = "-1";
                    replyData.message = "用戶不存在!";
                    replyData.data = "";
                    return JsonConvert.SerializeObject(replyData);
                }
                var userId = info.UserId;
                //var UpdDateTime = DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss.fffffff");
                //刪除QUE001，再copy QUE011-->QUE001，SurveryId不變
                //更新QUE001的資料(基本設定儲存在這邊
                sqlS = @"SELECT StyleType, DefBackgroudColor,DefHeaderPic,DefHeaderPhonePic from QUE011_TemplateBase where TemplateId=@TemplateId";
                //-------sql para----start
                SqlParameter[] sqlParams_TemplateId = new SqlParameter[] {
                    new SqlParameter("@TemplateId", SqlDbType.UniqueIdentifier)
                };
                sqlParams_TemplateId[0].Value = TemplateId.ValidGuid();
                //-------sql para----end
                Log.Debug("Select QUE011_TemplateBase:" + sqlS);

                DataTable dtQue001 = _db.GetQueryData(sqlS, sqlParams_TemplateId);

                sqlS = @"update QUE001_QuestionnaireBase set StyleType =@StyleType,DefBackgroudColor=@DefBackgroudColor, DefHeaderPic=@DefHeaderPic,
                DefHeaderPhonePic=@DefHeaderPhonePic,TemplateId=@TemplateId, UpdUserId=@userId,UpdDateTime=SYSDATETIME() where SurveyId=@SurveyId";
                Log.Debug("update QUE001_QuestionnaireBase:" + sqlS);
                //-------sql para----start
                SqlParameter[] sqlParamsA = new SqlParameter[] {
                    new SqlParameter("@StyleType", SqlDbType.Int),
                    new SqlParameter("@DefBackgroudColor", SqlDbType.NVarChar),
                    new SqlParameter("@DefHeaderPic", SqlDbType.NVarChar),
                    new SqlParameter("@DefHeaderPhonePic", SqlDbType.NVarChar),
                    new SqlParameter("@TemplateId", SqlDbType.UniqueIdentifier),
                    new SqlParameter("@userId", SqlDbType.UniqueIdentifier),
                    new SqlParameter("@SurveyId", SqlDbType.UniqueIdentifier)
                };

                sqlParamsA[0].Value = dtQue001.Rows[0]["StyleType"].ValidInt( "1" );
                sqlParamsA[1].Value = dtQue001.Rows[0]["DefBackgroudColor"].Valid();
                sqlParamsA[2].Value = dtQue001.Rows[0]["DefHeaderPic"].Valid();
                sqlParamsA[3].Value = dtQue001.Rows[0]["DefHeaderPhonePic"].Valid();
                sqlParamsA[4].Value = TemplateId.ValidGuid();
                sqlParamsA[5].Value = userId.ValidGuid();
                sqlParamsA[6].Value = SurveyId.ValidGuid();
                //-------sql para----end
                iR += _db.ExecuteSql(sqlS, sqlParamsA);

                //查詢QUE002，刪除QUE002 & QUE003
                sqlS = @"SELECT SurveyId,QuestionId from QUE002_QuestionnaireDetail where SurveyId=@SurveyId";
                //-------sql para----start
                SqlParameter[] sqlParams_SurveyId = new SqlParameter[] {
                    new SqlParameter("@SurveyId", SqlDbType.UniqueIdentifier)
                };
                sqlParams_SurveyId[0].Value = SurveyId.ValidGuid();
                //-------sql para----end
                Log.Debug("Select QUE002_QuestionnaireDetail:" + sqlS);
                DataTable dtQue002 = _db.GetQueryData(sqlS, sqlParams_SurveyId);

                sqlS = @"delete from QUE002_QuestionnaireDetail where SurveyId = @SurveyId";
                Log.Debug("Delete QUE002_QuestionnaireDetail:" + sqlS);
                dR += _db.ExecuteSql(sqlS, sqlParams_SurveyId);

                foreach (DataRow dr in dtQue002.Rows)
                {
                    string questionId = dr["QuestionId"].ToString();
                    sqlS = @"delete from QUE003_QuestionnaireOptions where QuestionId = @questionId";
                    Log.Debug("Delete QUE003_QuestionnaireOptions:" + sqlS);
                    //-------sql para----start
                    SqlParameter[] sqlParams = new SqlParameter[] {
                        new SqlParameter("@questionId", SqlDbType.UniqueIdentifier)
                    };
                    sqlParams[0].Value = questionId.ValidGuid();
                    //-------sql para----end
                    dR += _db.ExecuteSql(sqlS, sqlParams);
                }

                //要將QUE005/QUE006/QUE009(邏輯規則/收集方式) 也清空
                sqlS = @"SELECT SurveyId,LogicId from QUE005_QuestionnaireRuleGroup where SurveyId=@SurveyId";
                Log.Debug("Select QUE005_QuestionnaireRuleGroup:" + sqlS);
                DataTable dtQue005 = _db.GetQueryData(sqlS, sqlParams_SurveyId);

                sqlS = @"delete from QUE005_QuestionnaireRuleGroup where SurveyId = @SurveyId";
                Log.Debug("Delete QUE005_QuestionnaireRuleGroup:" + sqlS);
                dR += _db.ExecuteSql(sqlS, sqlParams_SurveyId);
                foreach (DataRow dr in dtQue005.Rows)
                {
                    string logicId = dr["LogicId"].ToString();
                    sqlS = @"delete from QUE006_QuestionnaireRuleCondition where LogicId = @logicId";
                    Log.Debug("Delete QUE006_QuestionnaireRuleCondition:" + sqlS);
                    //-------sql para----start
                    SqlParameter[] sqlParams = new SqlParameter[] {
                        new SqlParameter("@logicId", SqlDbType.UniqueIdentifier)
                    };
                    sqlParams[0].Value = logicId.ValidGuid();
                    //-------sql para----end
                    dR += _db.ExecuteSql(sqlS, sqlParams);
                }
                sqlS = @"delete from QUE009_QuestionnaireProvideType where SurveyId = @SurveyId";
                Log.Debug("Delete QUE009_QuestionnaireProvideType:" + sqlS);
                dR += _db.ExecuteSql(sqlS, sqlParams_SurveyId);

                //查詢QUE012，新增QUE002 & QUE003，SurveryId不變，QuestionId為New
                sqlS = @"SELECT TemplateId,QuestionId from QUE012_TemplateDetail where TemplateId=@TemplateId order by QuestionSeq ";
                Log.Debug("Select QUE012_TemplateDetail:" + sqlS);
                DataTable dtQue012 = _db.GetQueryData(sqlS, sqlParams_TemplateId);


                Dictionary<string, string> dicQuestions = new Dictionary<string, string>();
                foreach (DataRow dr in dtQue012.Rows)
                {
                    string questionId = dr["QuestionId"].ToString();
                    string newQuestionId = Guid.NewGuid().ToString();

                    sqlS = @"insert into QUE002_QuestionnaireDetail 
                    (SurveyId,QuestionId,QuestionSeq,QuestionType,QuestionSubject,SubjectStyle,QuestionNote,PageNo,IsRequired,HasOther,OtherIsShowText,OtherVerify,OtherTextMandatory,OtherCheckMessage,IsSetShowNum,  
                    PCRowNum,MobileRowNum,IsRamdomOption,ExcludeOther,BaseDataValidType,BlankDefaultWords,BlankValidType,MatrixItems,UpdUserId,UpdDateTime,BlankMaxLimit,BlankMinLimit,QuestionImage,QuestionVideo,MultiOptionLimit,OtherMinLimit,OtherMaxLimit,OtherChildQuestionId,TempQuestionId)
                    SELECT @SurveyId,@newQuestionId,QuestionSeq,QuestionType,QuestionSubject,SubjectStyle,QuestionNote,PageNo,IsRequired,HasOther,OtherIsShowText,OtherVerify,OtherTextMandatory,OtherCheckMessage,IsSetShowNum,
                    PCRowNum,MobileRowNum,IsRamdomOption,ExcludeOther,BaseDataValidType,BlankDefaultWords,BlankValidType,MatrixItems,@userId,SYSDATETIME(),BlankMaxLimit,BlankMinLimit,QuestionImage,QuestionVideo,MultiOptionLimit,OtherMinLimit,OtherMaxLimit,OtherChildQuestionId, QuestionId  
                    from QUE012_TemplateDetail where TemplateId=@TemplateId and QuestionId=@questionId";
                    Log.Debug("Insert QUE002_QuestionnaireDetail:" + sqlS);
                    //-------sql para----start
                    SqlParameter[] sqlParams = new SqlParameter[] {
                        new SqlParameter("@SurveyId", SqlDbType.UniqueIdentifier),
                        new SqlParameter("@newQuestionId", SqlDbType.UniqueIdentifier),
                        new SqlParameter("@userId", SqlDbType.UniqueIdentifier),
                        new SqlParameter("@TemplateId", SqlDbType.UniqueIdentifier),
                        new SqlParameter("@questionId", SqlDbType.UniqueIdentifier)
                    };
                    sqlParams[0].Value = SurveyId.ValidGuid();
                    sqlParams[1].Value = newQuestionId.ValidGuid();
                    sqlParams[2].Value = userId.ValidGuid();
                    sqlParams[3].Value = TemplateId.ValidGuid();
                    sqlParams[4].Value = questionId.ValidGuid();
                    //-------sql para----end
                    iR += _db.ExecuteSql(sqlS, sqlParams);
                    dicQuestions.Add(questionId, newQuestionId);
                }

                foreach (DataRow dr in dtQue012.Rows)
                {
                    string questionId = dr["QuestionId"].ToString();
                    string newQuestionId = dicQuestions[questionId].ToString();

                    sqlS = @"insert into QUE003_QuestionnaireOptions 
                    (QuestionId,OptionId,OptionSeq,OptionType,OptionContent,ChildQuestionId,UpdUserId,UpdDateTime,OptionImage,OptionVideo,OtherFlag,TempOptionId)
                    SELECT newQuestionId,NEWID(),QUE013.OptionSeq,QUE013.OptionType,QUE013.OptionContent,QUE002.QuestionId,@userId,SYSDATETIME(),QUE013.OptionImage,QUE013.OptionVideo,QUE013.OtherFlag ,QUE013.OptionId 
                    from QUE013_TemplateOptions QUE013 
                    LEFT JOIN QUE002_QuestionnaireDetail QUE002 ON QUE002.TempQuestionId = QUE013.ChildQuestionId AND QUE002.SurveyId=@SurveyId
                    where QUE013.QuestionId=@questionId
                    group by QUE013.OptionSeq,QUE013.OptionType,QUE013.OptionContent,QUE002.QuestionId,QUE013.OptionImage,QUE013.OptionVideo,QUE013.OtherFlag ,QUE013.OptionId";
                    Log.Debug("Insert QUE003_QuestionnaireOptions:" + sqlS);
                    //-------sql para----start
                    SqlParameter[] sqlParams = new SqlParameter[] {
                        new SqlParameter("@newQuestionId", SqlDbType.UniqueIdentifier),
                        new SqlParameter("@userId", SqlDbType.UniqueIdentifier),
                        new SqlParameter("@questionId", SqlDbType.UniqueIdentifier),
                        new SqlParameter("@SurveyId", SqlDbType.UniqueIdentifier)
                    };
                    sqlParams[0].Value = newQuestionId.ValidGuid();
                    sqlParams[1].Value = userId.ValidGuid();
                    sqlParams[2].Value = questionId.ValidGuid();
                    sqlParams[3].Value = SurveyId.ValidGuid();
                    //-------sql para----end
                    iR += _db.ExecuteSql(sqlS, sqlParams);

                }


                //刪除QUE004，再copy QUE014-->QUE004，SurveryId不變
                sqlS = @"delete from QUE004_QuestionnaireSetting where SurveyId = @SurveyId";
                Log.Debug("delete QUE004_QuestionnaireSetting:" + sqlS);
                dR += _db.ExecuteSql(sqlS, sqlParams_SurveyId);

                sqlS = @"insert into QUE004_QuestionnaireSetting 
                    (SurveyId,IsShowPageNo,IsShowQuestionNo,IsShowRequiredStar,IsShowProgress,PorgressPosition,ProgressStyle,UseVirifyCode,
                    IsOneQuestionPerPage,IsPublishResult,IsShowEndPage,UpdUserId,UpdDateTime,IsShowOptionNo)
                    select @SurveyId,IsShowPageNo,IsShowQuestionNo,IsShowRequiredStar,IsShowProgress,PorgressPosition,ProgressStyle,UseVirifyCode,
                    IsOneQuestionPerPage,IsPublishResult,IsShowEndPage,@userId,SYSDATETIME(),IsShowOptionNo
                    from QUE014_TemplateSetting where TemplateId = @TemplateId";
                Log.Debug("Insert QUE004_QuestionnaireSetting:" + sqlS);
                //-------sql para----start
                SqlParameter[] sqlParamsB = new SqlParameter[] {
                    new SqlParameter("@SurveyId", SqlDbType.UniqueIdentifier),
                    new SqlParameter("@userId", SqlDbType.UniqueIdentifier),
                    new SqlParameter("@TemplateId", SqlDbType.UniqueIdentifier)
                };
                sqlParamsB[0].Value = SurveyId.ValidGuid();
                sqlParamsB[1].Value = userId.ValidGuid();
                sqlParamsB[2].Value = TemplateId.ValidGuid();
                //-------sql para----end
                iR += _db.ExecuteSql(sqlS, sqlParamsB);

                //查詢QUE015，新增QUE005 & QUE006，SurveryId不變，LogicId為New
                sqlS = @"SELECT TemplateId,LogicId from QUE015_TemplateRuleGroup where TemplateId=@TemplateId order by CreateDateTime ";
                Log.Debug("Select QUE015_TemplateRuleGroup:" + sqlS);
                DataTable dtQue015 = _db.GetQueryData(sqlS, sqlParams_TemplateId);

                Dictionary<string, string> dicLogics = new Dictionary<string, string>();
                foreach (DataRow dr in dtQue015.Rows)
                {
                    string logicId = dr["LogicId"].ToString();
                    string newLogicId = Guid.NewGuid().ToString();

                    sqlS = @"insert into QUE005_QuestionnaireRuleGroup 
                        (SurveyId,LogicId,LogicType,LogicCondition,TargetQuestionId,BlockOptionList,CreateUserID,CreateDateTime,TempLogicId)
                        SELECT @SurveyId,@newLogicId,QUE015.LogicType,QUE015.LogicCondition,QUE002.QuestionId,ISNULL(BOList.NewOption,''),@userId,SYSDATETIME(),@logicId  
                        from QUE015_TemplateRuleGroup QUE015 
                        LEFT JOIN QUE002_QuestionnaireDetail QUE002 ON QUE015.TargetQuestionId = QUE002.TempQuestionId AND QUE002.SurveyId=@SurveyId 
                        LEFT JOIN GetQUE005NewBlockOptionList(@logicId,@SurveyId) BOList ON QUE015.LogicId = BOList.LogicId 
                        where QUE015.TemplateId=@TemplateId and QUE015.LogicId=@logicId";
                    Log.Debug("Insert QUE005_QuestionnaireRuleGroup:" + sqlS);
                    //-------sql para----start
                    SqlParameter[] sqlParamsC = new SqlParameter[] {
                        new SqlParameter("@SurveyId", SqlDbType.UniqueIdentifier),
                        new SqlParameter("@newLogicId", SqlDbType.UniqueIdentifier),
                        new SqlParameter("@userId", SqlDbType.UniqueIdentifier),
                        new SqlParameter("@TemplateId", SqlDbType.UniqueIdentifier),
                        new SqlParameter("@logicId", SqlDbType.UniqueIdentifier)
                    };
                    sqlParamsC[0].Value = SurveyId.ValidGuid();
                    sqlParamsC[1].Value = newLogicId.ValidGuid();
                    sqlParamsC[2].Value = userId.ValidGuid();
                    sqlParamsC[3].Value = TemplateId.ValidGuid();
                    sqlParamsC[4].Value = logicId.ValidGuid();
                    //-------sql para----end
                    iR += _db.ExecuteSql(sqlS, sqlParamsC);
                    dicLogics.Add(logicId, newLogicId);

                    sqlS = @"insert into QUE006_QuestionnaireRuleCondition 
                        (LogicId,ConditionRule,ConditionQuestionList,ConditionOptionList,MatrixField,CreateDateTime,CreateUserId,TempId)
                        SELECT @newLogicId,QUE016.ConditionRule,ISNULL(CQList.NewQuestion,''),ISNULL(COList.NewOption,''),QUE016.MatrixField,SYSDATETIME(),@userId ,QUE016.Id  
                        from QUE016_TemplateRuleCondition QUE016 
                        LEFT JOIN [dbo].[GetQUE006NewConditionOptionList](@logicId,@SurveyId) COList ON QUE016.Id = COList.Id 
						LEFT JOIN [dbo].[GetQUE006NewConditionQuestionList](@logicId,@SurveyId) CQList ON  QUE016.Id = CQList.Id 
                        where QUE016.LogicId=@logicId";
                    Log.Debug("Insert QUE006_QuestionnaireRuleCondition:" + sqlS);
                    //-------sql para----start
                    SqlParameter[] sqlParamsD = new SqlParameter[] {
                        new SqlParameter("@newLogicId", SqlDbType.UniqueIdentifier),
                        new SqlParameter("@userId", SqlDbType.UniqueIdentifier),
                        new SqlParameter("@logicId", SqlDbType.UniqueIdentifier),
                        new SqlParameter("@SurveyId", SqlDbType.UniqueIdentifier)
                    };
                    sqlParamsD[0].Value = newLogicId.ValidGuid();
                    sqlParamsD[1].Value = userId.ValidGuid();
                    sqlParamsD[2].Value = logicId.ValidGuid();
                    sqlParamsD[3].Value = SurveyId.ValidGuid();
                    //-------sql para----end
                    iR += _db.ExecuteSql(sqlS, sqlParamsD);

                }

                //範本套用後，填答資料要清除，不管正式問卷或測試問卷。
                sqlS = @"update QUE021_AnwserCollection set DelFlag = 1 where SurveyId = @SurveyId";
                Log.Debug("update QUE021_AnwserCollection:" + sqlS);
                iR += _db.ExecuteSql(sqlS, sqlParams_SurveyId);

                SurveyTemplateAdd applyData = new SurveyTemplateAdd();
                applyData.TemplateId = SurveyId;

                replyData.code = "200";
                replyData.message = $"套用範本完成。";
                replyData.data = applyData;
            }
            catch (Exception ex)
            {
                replyData.code = "-1";
                replyData.message = $"套用範本失敗!{ex.Message}.";
                replyData.data = null;
                Log.Error("套用範本失敗!" + ex.Message);
            }
            return JsonConvert.SerializeObject(replyData);
        }

        [Route("Share")]
        [HttpPost]
        public object TemplateShare([FromBody] Object value)
        {
            Log.Debug("我的範本-分享給他人...");

            var replyData = new ReplyData();

            JObject jo = (JObject)JsonConvert.DeserializeObject(value.ToString());

            if (jo["SurveyId"] == null || String.IsNullOrWhiteSpace(jo["SurveyId"].ToString()))
            {
                replyData.code = "-1";
                replyData.message = $"我的範本-分享失敗!參數SurveyId不能為空!";
                replyData.data = "";
                Log.Error("我的範本-分享失敗!" + "參數SurveyId不能為空！");
                return JsonConvert.SerializeObject(replyData);
            }
            if (jo["UserId"] == null || String.IsNullOrWhiteSpace(jo["UserId"].ToString()))
            {
                replyData.code = "-1";
                replyData.message = $"我的範本-分享失敗!參數UserId不能為空!";
                replyData.data = "";
                Log.Error("我的範本-分享失敗!" + "參數UserId不能為空！");
                return JsonConvert.SerializeObject(replyData);
            }
            var SurveyId = jo["SurveyId"].ToString();
            var UserId = jo["UserId"].ToString();
            //var UpdDateTime = DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss.fffffff");
            try
            {
                string sqlS = "";

                // 1.需要檢查，問卷是否為空白(QUE002沒有資料就算是空白)，空白問卷不能分享範本
                sqlS = "select count(1) as num from QUE002_QuestionnaireDetail where SurveyId=@SurveyId ";
                Log.Debug("我的範本-分享:" + sqlS);
                //-------sql para----start
                SqlParameter[] sqlParams_SurveyId = new SqlParameter[] {
                    new SqlParameter("@SurveyId", SqlDbType.UniqueIdentifier)
                };
                sqlParams_SurveyId[0].Value = SurveyId.ValidGuid();
                //-------sql para----end
                string rowCount = _db.GetSingle(sqlS, sqlParams_SurveyId);
                if (int.Parse(rowCount) == 0)
                {
                    ErrorCode.Code = "702";
                    replyData.code = ErrorCode.Code;
                    replyData.message = ErrorCode.Message;
                    //replyData.code = "-1";
                    //replyData.message = $"我的範本-分享失敗!空白問卷不能分享範本!";
                    replyData.data = "";
                    Log.Error("我的範本-分享失敗!" + "空白問卷不能分享範本！");
                    return JsonConvert.SerializeObject(replyData);
                }
                string templateId = Guid.NewGuid().ToString();
                sqlS = @"insert into QUE011_TemplateBase
                    (TemplateId,Title,FinalUrl,ThankWords,DueAction,DelFlag,Audit,CreateUserId,CreateDateTime,UpdUserId,UpdDateTime,StyleType,DefBackgroudColor,SurveyId,DefHeaderPic,DefHeaderPhonePic) 
                    SELECT @templateId,Title,FinalUrl,ThankWords,DueAction,DelFlag,Audit,@UserId,SYSDATETIME(),@UserId,SYSDATETIME(),StyleType,DefBackgroudColor,SurveyId,DefHeaderPic,DefHeaderPhonePic 
                    from QUE001_QuestionnaireBase where SurveyId=@SurveyId";
                Log.Debug("Insert QUE011_TemplateBase:" + sqlS);
                //-------sql para----start
                SqlParameter[] sqlParams = new SqlParameter[] {
                    new SqlParameter("@templateId", SqlDbType.UniqueIdentifier),
                    new SqlParameter("@UserId", SqlDbType.UniqueIdentifier),
                    new SqlParameter("@SurveyId", SqlDbType.UniqueIdentifier)
                };
                sqlParams[0].Value = templateId.ValidGuid();
                sqlParams[1].Value = UserId.ValidGuid();
                sqlParams[2].Value = SurveyId.ValidGuid();
                //-------sql para----end
                int iR = _db.ExecuteSql(sqlS, sqlParams);

                sqlS = @"SELECT SurveyId,QuestionId from QUE002_QuestionnaireDetail where SurveyId=@SurveyId ORDER BY QuestionSeq,UpdDateTime";
                DataTable dtQue002 = _db.GetQueryData(sqlS, sqlParams_SurveyId);
                Log.Debug("SELECT SurveyId,QuestionId:" + sqlS);
                Dictionary<string, string> dicQuestions = new Dictionary<string, string>();
                foreach (DataRow dr in dtQue002.Rows)
                {
                    string questionId = dr["QuestionId"].ToString();
                    string newQuestionId = Guid.NewGuid().ToString();
                    sqlS = @"insert into QUE012_TemplateDetail 
                        (TemplateId,QuestionId,QuestionSeq,QuestionType,QuestionSubject,SubjectStyle,QuestionNote,PageNo,IsRequired,HasOther,OtherIsShowText,OtherVerify,OtherTextMandatory,OtherCheckMessage,IsSetShowNum,
                        PCRowNum,MobileRowNum,IsRamdomOption,ExcludeOther,BaseDataValidType,BlankDefaultWords,BlankValidType,MatrixItems,UpdUserId,UpdDateTime,BlankMaxLimit,BlankMinLimit,QuestionImage,QuestionVideo,MultiOptionLimit,OtherMinLimit,OtherMaxLimit,OtherChildQuestionId,OrgQuestionId)
                        SELECT @templateId,@newQuestionId,QuestionSeq,QuestionType,QuestionSubject,SubjectStyle,QuestionNote,PageNo,IsRequired,HasOther,OtherIsShowText,OtherVerify,OtherTextMandatory,OtherCheckMessage,IsSetShowNum,
                        PCRowNum,MobileRowNum,IsRamdomOption,ExcludeOther,BaseDataValidType,BlankDefaultWords,BlankValidType,MatrixItems,@UserId,SYSDATETIME(),BlankMaxLimit,BlankMinLimit,QuestionImage,QuestionVideo,MultiOptionLimit ,OtherMinLimit,OtherMaxLimit,OtherChildQuestionId, @questionId  
                        from QUE002_QuestionnaireDetail where SurveyId=@SurveyId and QuestionId=@questionId";
                    Log.Debug("Insert QUE012_TemplateDetail:" + sqlS);
                    //-------sql para----start
                    SqlParameter[] sqlParamsA = new SqlParameter[] {
                        new SqlParameter("@templateId", SqlDbType.UniqueIdentifier),
                        new SqlParameter("@newQuestionId", SqlDbType.UniqueIdentifier),
                        new SqlParameter("@UserId", SqlDbType.UniqueIdentifier),
                        new SqlParameter("@SurveyId", SqlDbType.UniqueIdentifier),
                        new SqlParameter("@questionId", SqlDbType.UniqueIdentifier)
                    };
                    sqlParamsA[0].Value = templateId.ValidGuid();
                    sqlParamsA[1].Value = newQuestionId.ValidGuid();
                    sqlParamsA[2].Value = UserId.ValidGuid();
                    sqlParamsA[3].Value = SurveyId.ValidGuid();
                    sqlParamsA[4].Value = questionId.ValidGuid();
                    //-------sql para----end
                    iR += _db.ExecuteSql(sqlS, sqlParamsA);
                    dicQuestions.Add(questionId, newQuestionId);
                }

                foreach (DataRow dr in dtQue002.Rows)
                {
                    string questionId = dr["QuestionId"].ToString();
                    string newQuestionId = dicQuestions[questionId].ToString();

                    sqlS = @"insert into QUE013_TemplateOptions 
                        (QuestionId,OptionId,OptionSeq,OptionType,OptionContent,ChildQuestionId,UpdUserId,UpdDateTime,OptionImage,OptionVideo,OtherFlag,OrgOptionId)
                        SELECT @newQuestionId,NEWID(),QUE003.OptionSeq,QUE003.OptionType,QUE003.OptionContent,QUE012.QuestionId,@UserId,SYSDATETIME(),QUE003.OptionImage,QUE003.OptionVideo,QUE003.OtherFlag,QUE003.OptionId
                        from QUE003_QuestionnaireOptions QUE003 
                        LEFT JOIN QUE012_TemplateDetail QUE012 ON QUE003.ChildQuestionId = QUE012.OrgQuestionId AND QUE012.TemplateId = @templateId
                        where QUE003.QuestionId=@questionId
                        group by QUE003.OptionSeq,QUE003.OptionType,QUE003.OptionContent,QUE012.QuestionId,QUE003.OptionImage,QUE003.OptionVideo,QUE003.OtherFlag,QUE003.OptionId";
                    Log.Debug("Insert QUE013_TemplateOptions:" + sqlS);
                    //-------sql para----start
                    SqlParameter[] sqlParamsA = new SqlParameter[] {
                        new SqlParameter("@newQuestionId", SqlDbType.UniqueIdentifier),
                        new SqlParameter("@UserId", SqlDbType.UniqueIdentifier),
                        new SqlParameter("@questionId", SqlDbType.UniqueIdentifier),
                        new SqlParameter("@templateId", SqlDbType.UniqueIdentifier)
                    };
                    sqlParamsA[0].Value = newQuestionId.ValidGuid();
                    sqlParamsA[1].Value = UserId.ValidGuid();
                    sqlParamsA[2].Value = questionId.ValidGuid();
                    sqlParamsA[3].Value = templateId.ValidGuid();
                    //-------sql para----end
                    iR += _db.ExecuteSql(sqlS, sqlParamsA);
                }


                sqlS = @"insert into QUE014_TemplateSetting 
                    (TemplateId,IsShowPageNo,IsShowQuestionNo,IsShowRequiredStar,IsShowProgress,PorgressPosition,ProgressStyle,UseVirifyCode,
                    IsOneQuestionPerPage,IsPublishResult,IsShowEndPage,UpdUserId,UpdDateTime,IsShowOptionNo)
                    select @templateId,IsShowPageNo,IsShowQuestionNo,IsShowRequiredStar,IsShowProgress,PorgressPosition,ProgressStyle,UseVirifyCode,
                    IsOneQuestionPerPage,IsPublishResult,IsShowEndPage,@UserId,SYSDATETIME(),IsShowOptionNo  
                    from QUE004_QuestionnaireSetting where SurveyId = @SurveyId";
                Log.Debug("Insert QUE014_TemplateSetting:" + sqlS);
                //-------sql para----start
                SqlParameter[] sqlParamsB = new SqlParameter[] {
                    new SqlParameter("@templateId", SqlDbType.UniqueIdentifier),
                    new SqlParameter("@UserId", SqlDbType.UniqueIdentifier),
                    new SqlParameter("@SurveyId", SqlDbType.UniqueIdentifier)
                };
                sqlParamsB[0].Value = templateId.ValidGuid();
                sqlParamsB[1].Value = UserId.ValidGuid();
                sqlParamsB[2].Value = SurveyId.ValidGuid();
                //-------sql para----end
                iR += _db.ExecuteSql(sqlS, sqlParamsB);

                //新增邏輯規則主表
                sqlS = @"SELECT SurveyId,LogicId from QUE005_QuestionnaireRuleGroup where SurveyId='{0}' ORDER BY CreateDateTime";
                DataTable dtQue005 = _db.GetQueryData(sqlS, sqlParams_SurveyId);
                foreach (DataRow dr in dtQue005.Rows)
                {
                    string logicId = dr["LogicId"].ToString();
                    string newLogicId = Guid.NewGuid().ToString();
                    sqlS = @"insert into QUE015_TemplateRuleGroup 
                        (TemplateId,LogicId,LogicType,LogicCondition,TargetQuestionId,BlockOptionList,CreateUserID,CreateDateTime,OrgLogicId)
                        SELECT @templateId,@newLogicId,QUE005.LogicType,QUE005.LogicCondition,QUE012.QuestionId,ISNULL(BOList.NewOption,''),@UserId,SYSDATETIME(),@logicId
                        FROM QUE005_QuestionnaireRuleGroup QUE005
                        LEFT JOIN QUE012_TemplateDetail QUE012 ON QUE005.TargetQuestionId = QUE012.OrgQuestionId AND QUE012.TemplateId=@templateId 
                        LEFT JOIN GetQUE015NewBlockOptionList(@logicId,@templateId) BOList ON QUE005.LogicId = BOList.LogicId 
                        WHERE QUE005.SurveyId=@SurveyId and QUE005.LogicId=@logicId ";
                    Log.Debug("Insert QUE015_TemplateRuleGroup:" + sqlS);
                    //-------sql para----start
                    SqlParameter[] sqlParamsC = new SqlParameter[] {
                        new SqlParameter("@templateId", SqlDbType.UniqueIdentifier),
                        new SqlParameter("@newLogicId", SqlDbType.UniqueIdentifier),
                        new SqlParameter("@UserId", SqlDbType.UniqueIdentifier),
                        new SqlParameter("@SurveyId", SqlDbType.UniqueIdentifier),
                        new SqlParameter("@logicId", SqlDbType.UniqueIdentifier)
                    };
                    sqlParamsC[0].Value = templateId.ValidGuid();
                    sqlParamsC[1].Value = newLogicId.ValidGuid();
                    sqlParamsC[2].Value = UserId.ValidGuid();
                    sqlParamsC[3].Value = SurveyId.ValidGuid();
                    sqlParamsC[4].Value = logicId.ValidGuid();
                    //-------sql para----end
                    iR += _db.ExecuteSql(sqlS, sqlParamsC);

                    sqlS = @"insert into QUE016_TemplateRuleCondition 
                        (LogicId,ConditionRule,ConditionQuestionList,ConditionOptionList,MatrixField,CreateDateTime,CreateUserId,OrgId)
                        SELECT @newLogicId,QUE006.ConditionRule,ISNULL(CQList.NewQuestion,''),ISNULL(COList.NewOption,''),QUE006.MatrixField,SYSDATETIME(),@UserId,  QUE006.Id 
                        from QUE006_QuestionnaireRuleCondition QUE006
                        LEFT JOIN [dbo].[GetQUE016NewConditionOptionList](@logicId,@templateId) COList ON QUE006.Id = COList.Id 
						LEFT JOIN [dbo].[GetQUE016NewConditionQuestionList](@logicId,@templateId) CQList ON   QUE006.Id = CQList.Id 
                        WHERE QUE006.LogicId=@logicId ";
                    Log.Debug("Insert QUE016_TemplateRuleCondition:" + sqlS);
                    //-------sql para----start
                    SqlParameter[] sqlParamsD = new SqlParameter[] {
                        new SqlParameter("@newLogicId", SqlDbType.UniqueIdentifier),
                        new SqlParameter("@UserId", SqlDbType.UniqueIdentifier),
                        new SqlParameter("@logicId", SqlDbType.UniqueIdentifier),
                        new SqlParameter("@templateId", SqlDbType.UniqueIdentifier)
                    };
                    sqlParamsD[0].Value = newLogicId.ValidGuid();
                    sqlParamsD[1].Value = UserId.ValidGuid();
                    sqlParamsD[2].Value = logicId.ValidGuid();
                    sqlParamsD[3].Value = templateId.ValidGuid();
                    //-------sql para----end
                    iR += _db.ExecuteSql(sqlS, sqlParamsD);
                }

                SurveyTemplateAdd sharedData = new SurveyTemplateAdd();
                sharedData.TemplateId = templateId;

                replyData.code = "200";
                replyData.message = $"分享範本完成。";
                replyData.data = sharedData;
            }
            catch (Exception ex)
            {
                replyData.code = "-1";
                replyData.message = $"分享範本失敗!{ex.Message}.";
                replyData.data = null;
                Log.Error("分享範本失敗!" + ex.Message);
            }
            return JsonConvert.SerializeObject(replyData);
        }

        [Route("Delete")]
        [HttpDelete]
        public object TemplateDelete(string templateId)
        {
            Log.Debug("我的範本-刪除...");

            var replyData = new ReplyData();


            if (templateId == null || String.IsNullOrWhiteSpace(templateId))
            {
                replyData.code = "-1";
                replyData.message = $"我的範本-刪除失敗!參數TemplateId不能為空!";
                replyData.data = "";
                Log.Error("我的範本-刪除失敗!" + "參數TemplateId不能為空！");
                return JsonConvert.SerializeObject(replyData);
            }

            try
            {
                string sqlS = "";

                sqlS = "Update QUE011_TemplateBase set DelFlag ='true', UpdDateTime = SYSDATETIME() where TemplateId=@templateId ";
                //-------sql para----start
                SqlParameter[] sqlParams = new SqlParameter[] {
                    new SqlParameter("@templateId", SqlDbType.UniqueIdentifier)
                };
                sqlParams[0].Value = templateId.ValidGuid();
                //-------sql para----end
                Log.Debug("我的範本-刪除:" + sqlS);

                int iR = _db.ExecuteSql(sqlS, sqlParams);

                if (iR == 0)
                {
                    replyData.code = "-1";
                    replyData.message = $"我的範本-刪除失敗!不存在範本!"+ templateId;
                    replyData.data = "";
                    Log.Error("我的範本-刪除失敗!不存在範本!" + templateId);
                    return JsonConvert.SerializeObject(replyData);
                }

                replyData.code = "200";
                replyData.message = $"刪除範本完成。";
                replyData.data = "";
            }
            catch (Exception ex)
            {
                replyData.code = "-1";
                replyData.message = $"刪除範本失敗!{ex.Message}.";
                replyData.data = null;
                Log.Error("刪除範本失敗!" + ex.Message);
            }
            return JsonConvert.SerializeObject(replyData);
        }
    }
}
