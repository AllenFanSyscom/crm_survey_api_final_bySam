using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Threading.Tasks;
using Common;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using SurveyWebAPI.Utility;
using SurveyWebAPI.Models;
using System.Text;
using Microsoft.AspNetCore.Authorization;
using System.Data.SqlClient;

namespace SurveyWebAPI.Controllers
{
    [Authorize]
    [Route("api/Survey/Logic")]
    [ApiController]
    public class SurveyLogicController : ControllerBase
    {
        private readonly DBHelper _db;
        public SurveyLogicController()
        {
            _db = new DBHelper(AppSettingsHelper.DefaultConnectionString);
        }
        [Route("Query")]
        [HttpGet]
        public object Query(string surveyId)
        {
            Log.Debug("邏輯規則-列表查詢");
            //返回數據
            ReplyData replyData = new ReplyData();
            try
            {
                //data實體
                replyData.code = "200";
                replyData.message = $"查詢記錄完成。";
                replyData.data = QueryData(surveyId, "");
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
        [Route("Insert")]
        [HttpPost]
        public object Insert([FromBody] Object value)
        {
            Log.Debug("邏輯規則-新增");

            Logic logic = JsonConvert.DeserializeObject<Logic>(value.ToString());
            string surveyId = logic.SurveyId;
            var replyData = new ReplyData();

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
            string strLogicId = "";
            try
            {
                string sqlS = "";

                //QUE005,Check傳入數據中如果有null值，則不去更新相關欄位
                if (logic.LogicList != null)
                {
                    foreach (LogicDetail detail in logic.LogicList)
                    {
                        string detailId = Guid.NewGuid().ToString();
                        strLogicId += detailId + ";";
                        Log.Debug(detailId);
                        if (string.IsNullOrEmpty(detail.TargetQuestionId))
                        {
                            sqlS = string.Format("Insert Into QUE005_QuestionnaireRuleGroup (SurveyId,LogicId,LogicType,LogicCondition,TargetQuestionId,BlockOptionList,UpdUserId,UpdDateTime,CreateUserID,CreateDateTime)" +
                                " values(@SurveyId,@detailId,@LogicType,@LogicCondition,null,@BlockOptionList,@userId,SYSDATETIME(),@userId,SYSDATETIME()) ");

                            //-------sql para----start
                            SqlParameter[] sqlParams = new SqlParameter[] {
                                new SqlParameter("@SurveyId", SqlDbType.UniqueIdentifier),
                                new SqlParameter("@detailId", SqlDbType.UniqueIdentifier),
                                new SqlParameter("@LogicType", SqlDbType.Int),
                                new SqlParameter("@LogicCondition", SqlDbType.Int),
                                new SqlParameter("@BlockOptionList", SqlDbType.NVarChar),
                                new SqlParameter("@userId", SqlDbType.UniqueIdentifier),
                            };
                            sqlParams[0].Value = logic.SurveyId.ValidGuid();
                            sqlParams[1].Value = detailId.ValidGuid();
                            sqlParams[2].Value = detail.LogicType.ValidInt();;
                            sqlParams[3].Value = detail.LogicCondition.ValidInt();;
                            sqlParams[4].Value = detail.BlockOptionList.Valid();;
                            sqlParams[5].Value = userId.ValidGuid();
                            //-------sql para----end
                            int iR = _db.ExecuteSql(sqlS,sqlParams);

                        }
                        else
                        {
                            sqlS = string.Format("Insert Into QUE005_QuestionnaireRuleGroup (SurveyId,LogicId,LogicType,LogicCondition,TargetQuestionId,BlockOptionList,UpdUserId,UpdDateTime,CreateUserID,CreateDateTime)" +
                                " values(@SurveyId,@detailId,@LogicType,@LogicCondition,@TargetQuestionId,@BlockOptionList,@userId,SYSDATETIME(),@userId,SYSDATETIME()) ");


                            //-------sql para----start
                            SqlParameter[] sqlParams = new SqlParameter[] {
                                new SqlParameter("@SurveyId", SqlDbType.UniqueIdentifier),
                                new SqlParameter("@detailId", SqlDbType.UniqueIdentifier),
                                new SqlParameter("@LogicType", SqlDbType.Int),
                                new SqlParameter("@LogicCondition", SqlDbType.Int),
                                new SqlParameter("@TargetQuestionId", SqlDbType.UniqueIdentifier),
                                new SqlParameter("@BlockOptionList", SqlDbType.NVarChar),
                                new SqlParameter("@userId", SqlDbType.UniqueIdentifier),
                            };
                            sqlParams[0].Value = logic.SurveyId.ValidGuid();
                            sqlParams[1].Value = detailId.ValidGuid();
                            sqlParams[2].Value = detail.LogicType.ValidInt();;
                            sqlParams[3].Value = detail.LogicCondition.ValidInt();;
                            sqlParams[4].Value = detail.TargetQuestionId.ValidGuid();
                            sqlParams[5].Value = detail.BlockOptionList.Valid();;
                            sqlParams[6].Value = userId.ValidGuid();
                            //-------sql para----end
                            int iR = _db.ExecuteSql(sqlS, sqlParams);

                        }
                        Log.Debug("Insert QUE005_QuestionnaireRuleGroup:" + sqlS);
                        //int iR = _db.ExecuteSql(sqlS);

                        if (detail.ConditionList != null)
                        {
                            foreach (LogicCondition condition in detail.ConditionList)
                            {
                                sqlS = string.Format("Insert Into QUE006_QuestionnaireRuleCondition (LogicId,ConditionRule,ConditionQuestionList,ConditionOptionList,UpdUserId,UpdDateTime,MatrixField,CreateDateTime,CreateUserId) " +
                                    "values(@detailId,@ConditionRule,@ConditionQuestionList,@ConditionOptionList,@userId, SYSDATETIME(),@MatrixField, SYSDATETIME(),@userId)");


                                //-------sql para----start
                                SqlParameter[] sqlParams = new SqlParameter[] {
                                new SqlParameter("@detailId", SqlDbType.UniqueIdentifier),
                                new SqlParameter("@ConditionRule", SqlDbType.Int),
                                new SqlParameter("@ConditionQuestionList", SqlDbType.NVarChar),
                                new SqlParameter("@ConditionOptionList", SqlDbType.NVarChar),
                                new SqlParameter("@userId", SqlDbType.UniqueIdentifier),
                                new SqlParameter("@MatrixField", SqlDbType.NVarChar),
                                };
                                sqlParams[0].Value = detailId.ValidGuid();
                                sqlParams[1].Value = condition.ConditionRule.ValidInt();;
                                sqlParams[2].Value = condition.ConditionQuestionList.Valid();;
                                sqlParams[3].Value = condition.ConditionOptionList.Valid();;
                                sqlParams[4].Value = userId.ValidGuid();
                                sqlParams[5].Value = condition.MatrixField.Valid();
                                //-------sql para----end

                                Log.Debug("Insert QUE006_QuestionnaireRuleCondition:" + sqlS);
                                int iR = _db.ExecuteSql(sqlS,sqlParams);
                            }
                        }
                    }
                }
                replyData.code = "200";
                replyData.message = $"新增資料成功!";
                replyData.data = QueryData(surveyId, strLogicId);
            }
            catch (Exception ex)
            {
                replyData.code = "-1";
                replyData.message = $"新增資料失敗!{ex.Message}.";
                replyData.data = null;
                Log.Error("新增資料失敗!" + ex.Message);
            }
            return JsonConvert.SerializeObject(replyData);
        }
        [Route("Update")]
        [HttpPut]
        public object Update([FromBody] Object value)
        {
            Log.Debug("邏輯規則-編輯");

            Logic logic = JsonConvert.DeserializeObject<Logic>(value.ToString());
            string surveyId = logic.SurveyId;
            var replyData = new ReplyData();

            try
            {
                string sqlS = "";

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
                string strLogicId = "";

                //QUE005,Check傳入數據中如果有null值，則不去更新相關欄位
                if (logic.LogicList != null)
                {
                    foreach (LogicDetail detail in logic.LogicList)
                    {
                        string detailId = detail.LogicId;
                        strLogicId += detailId + ";";
                        if (string.IsNullOrEmpty(detail.TargetQuestionId))
                        {
                            sqlS = string.Format("Update QUE005_QuestionnaireRuleGroup set LogicType=@LogicType,LogicCondition=@LogicCondition, TargetQuestionId=null,BlockOptionList=@BlockOptionList,UpdUserId=@userId,UpdDateTime=SYSDATETIME() where SurveyId=@SurveyId and LogicId=@LogicId ");

                            //-------sql para----start
                            SqlParameter[] sqlParams = new SqlParameter[] {
                                new SqlParameter("@LogicType", SqlDbType.Int),
                                new SqlParameter("@LogicCondition", SqlDbType.Int),
                                new SqlParameter("@BlockOptionList", SqlDbType.NVarChar),
                                new SqlParameter("@SurveyId", SqlDbType.UniqueIdentifier),
                                new SqlParameter("@LogicId", SqlDbType.UniqueIdentifier),
                                new SqlParameter("@userId", SqlDbType.UniqueIdentifier),
                            };
                            sqlParams[0].Value = detail.LogicType.ValidInt();;
                            sqlParams[1].Value = detail.LogicCondition.ValidInt();;
                            sqlParams[2].Value = detail.BlockOptionList.Valid();;
                            sqlParams[3].Value = logic.SurveyId.ValidGuid();
                            sqlParams[4].Value = detail.LogicId.ValidGuid();
                            sqlParams[5].Value = userId.ValidGuid();
                            //-------sql para----end
                            int iR = _db.ExecuteSql(sqlS,sqlParams);

                        }
                        else
                        {
                            sqlS = string.Format("Update QUE005_QuestionnaireRuleGroup set LogicType=@LogicType,LogicCondition=@LogicCondition, TargetQuestionId=@TargetQuestionId,BlockOptionList=@BlockOptionList,UpdUserId=@userId,UpdDateTime=SYSDATETIME() where SurveyId=@SurveyId and LogicId=@LogicId ");


                            //-------sql para----start
                            SqlParameter[] sqlParams = new SqlParameter[] {
                                new SqlParameter("@LogicType", SqlDbType.Int),
                                new SqlParameter("@LogicCondition", SqlDbType.Int),
                                new SqlParameter("@TargetQuestionId", SqlDbType.UniqueIdentifier),
                                new SqlParameter("@BlockOptionList", SqlDbType.NVarChar),
                                new SqlParameter("@SurveyId", SqlDbType.UniqueIdentifier),
                                new SqlParameter("@LogicId", SqlDbType.UniqueIdentifier),
                                new SqlParameter("@userId", SqlDbType.UniqueIdentifier),
                            };
                            sqlParams[0].Value = detail.LogicType.ValidInt();
                            sqlParams[1].Value = detail.LogicCondition.ValidInt();
                            sqlParams[2].Value = detail.TargetQuestionId.ValidGuid();
                            sqlParams[3].Value = detail.BlockOptionList.Valid();
                            sqlParams[4].Value = logic.SurveyId.ValidGuid();
                            sqlParams[5].Value = detail.LogicId.ValidGuid();
                            sqlParams[6].Value = userId.ValidGuid();
                            //-------sql para----end
                            int iR = _db.ExecuteSql(sqlS, sqlParams);


                        }
                        Log.Debug("Update QUE005_QuestionnaireRuleGroup:" + sqlS);
                        //int iR = _db.ExecuteSql(sqlS);

                        sqlS = string.Format("Delete from QUE006_QuestionnaireRuleCondition where LogicId=@detailId ");

                        //-------sql para----start
                        SqlParameter[] sql1Params = new SqlParameter[] {
                                new SqlParameter("@detailId", SqlDbType.UniqueIdentifier),

                            };
                        sql1Params[0].Value = detailId.ValidGuid();

                        //-------sql para----end


                        Log.Debug("Update QUE006_QuestionnaireRuleCondition:" + sqlS);
                        var IR_X = _db.ExecuteSql(sqlS, sql1Params);

                        if (detail.ConditionList != null)
                        {
                            foreach (LogicCondition condition in detail.ConditionList)
                            {
                                sqlS = string.Format("Insert Into QUE006_QuestionnaireRuleCondition (LogicId,ConditionRule,ConditionQuestionList,ConditionOptionList,UpdUserId,UpdDateTime,MatrixField,CreateDateTime,CreateUserId) " +
                                    "values(@detailId,@ConditionRule,@ConditionQuestionList,@ConditionOptionList,@userId, SYSDATETIME(), @MatrixField, SYSDATETIME(),@userId)");
                                Log.Debug("Update QUE006_QuestionnaireRuleCondition:" + sqlS);

                                //-------sql para----start
                                SqlParameter[] sq12Params = new SqlParameter[] {
                                new SqlParameter("@detailId", SqlDbType.UniqueIdentifier),
                                new SqlParameter("@ConditionRule", SqlDbType.Int),
                                new SqlParameter("@ConditionQuestionList", SqlDbType.NVarChar),
                                new SqlParameter("@ConditionOptionList", SqlDbType.NVarChar),
                                new SqlParameter("@userId", SqlDbType.UniqueIdentifier),
                                new SqlParameter("@MatrixField", SqlDbType.NVarChar),

                                };
                                sq12Params[0].Value = detailId.ValidGuid();
                                sq12Params[1].Value = condition.ConditionRule.ValidInt();
                                sq12Params[2].Value = condition.ConditionQuestionList.Valid();
                                sq12Params[3].Value = condition.ConditionOptionList.Valid();
                                sq12Params[4].Value = userId.ValidGuid();
                                sq12Params[5].Value = condition.MatrixField.Valid();

                                //-------sql para----end
                                int iR = _db.ExecuteSql(sqlS,sq12Params);

                            }
                        }
                    }
                }
                replyData.code = "200";
                replyData.message = $"更新資料成功!";
                replyData.data = QueryData(surveyId, strLogicId);
            }
            catch (Exception ex)
            {
                replyData.code = "-1";
                replyData.message = $"更新資料失敗!{ex.Message}.";
                replyData.data = null;
                Log.Error("更新資料失敗!" + ex.Message);
            }
            return JsonConvert.SerializeObject(replyData);
        }
        [Route("Delete")]
        [HttpDelete]
        public object Delete(string surveyId, string logicId)
        {
            var replyData = new ReplyData();
            try
            {
                string sSql = string.Format("DELETE FROM QUE005_QuestionnaireRuleGroup WHERE SurveyId=@surveyId and LogicId=@logicId ");

                //-------sql para----start
                SqlParameter[] sqlParams = new SqlParameter[] {
                    new SqlParameter("@surveyId", SqlDbType.UniqueIdentifier),
                    new SqlParameter("@logicId", SqlDbType.UniqueIdentifier),

                };
                sqlParams[0].Value = surveyId.ValidGuid();
                sqlParams[1].Value = logicId.ValidGuid();

                //-------sql para----end

                Log.Debug("刪除QUE005_QuestionnaireRuleGroup:" + sSql);
                int iR = _db.ExecuteSql(sSql, sqlParams);
                sSql = string.Format("DELETE FROM QUE006_QuestionnaireRuleCondition WHERE LogicId=@logicId");

                //-------sql para----start
                SqlParameter[] sql1Params = new SqlParameter[] {
                    new SqlParameter("@logicId", SqlDbType.UniqueIdentifier),


                };
                sql1Params[0].Value = logicId.ValidGuid();

                //-------sql para----end

                Log.Debug("刪除QUE006_QuestionnaireRuleCondition:" + sSql);
                iR += _db.ExecuteSql(sSql, sql1Params);

                replyData.code = "200";
                replyData.message = $"刪除記錄完成。";
                replyData.data = iR;
            }
            catch (Exception ex)
            {
                replyData.code = "-1";
                replyData.message = $"刪除資料失敗!{ex.Message}.";
                replyData.data = null;
                Log.Error("刪除資料失敗!" + ex.Message);
            }
            return JsonConvert.SerializeObject(replyData);
        }
        //For Query,after inser/update
        protected Logic QueryData(string surveyId, string logicId)
        {
            //data實體
            Logic logic = new Logic();
            try
            {
                DataTable dtR = new DataTable();
                string sqlS = "";
                if (!string.IsNullOrEmpty(logicId))
                {
                    string[] logicIdArr = logicId.Split(";");
                    foreach (string tmp in logicIdArr)
                    {
                        if (string.IsNullOrEmpty(tmp))
                            continue;
                        sqlS = string.Format("select * from QUE005_QuestionnaireRuleGroup where SurveyId=@surveyId and LogicId=@tmp");
                        //-------sql para----start
                        SqlParameter[] sqlParams = new SqlParameter[] {
                        new SqlParameter("@surveyId", SqlDbType.UniqueIdentifier),
                        new SqlParameter("@tmp", SqlDbType.UniqueIdentifier),

                        };
                        sqlParams[0].Value = surveyId.ValidGuid();
                        sqlParams[1].Value = tmp.ValidGuid();

                        //-------sql para----end
                        Log.Debug("QueryData:" + sqlS);
                        if (dtR == null || dtR.Rows.Count == 0)
                            dtR = _db.GetQueryData(sqlS, sqlParams);
                        else
                            dtR.Merge(_db.GetQueryData(sqlS, sqlParams));
                    }

                }
                else
                {
                    sqlS = string.Format("select * from QUE005_QuestionnaireRuleGroup where SurveyId=@surveyId");
                    //-------sql para----start
                    SqlParameter[] sqlParams = new SqlParameter[] {
                        new SqlParameter("@surveyId", SqlDbType.UniqueIdentifier),


                        };
                    sqlParams[0].Value = surveyId.ValidGuid();

                    //-------sql para----end

                    Log.Debug("QueryData:" + sqlS);
                    dtR = _db.GetQueryData(sqlS, sqlParams);
                }
                DataView dv = dtR.DefaultView;
                dv.Sort = " CreateDateTime asc";
                dtR = dv.ToTable();

                if (dtR == null || dtR.Rows.Count == 0)
                {
                    return null;
                }
                else
                {
                    //Fill data
                    logic.SurveyId = dtR.Rows[0]["SurveyId"].ToString().Trim().ToLower();
                    LogicDetail[] details = new LogicDetail[dtR.Rows.Count];
                    foreach (DataRow dr in dtR.Rows)
                    {
                        //Logic
                        LogicDetail detail = new LogicDetail();
                        if (dr["LogicType"] == null || string.IsNullOrEmpty(dr["LogicId"].ToString().Trim()))
                            detail.LogicId = "";
                        else
                            detail.LogicId = dr["LogicId"].ToString().Trim().ToLower();

                        if (dr["LogicType"] == null || string.IsNullOrEmpty(dr["LogicType"].ToString().Trim()))
                            detail.LogicType = 1;
                        else
                            detail.LogicType = int.Parse(dr["LogicType"].ToString().Trim());

                        if (dr["LogicCondition"] == null || string.IsNullOrEmpty(dr["LogicCondition"].ToString().Trim()))
                            detail.LogicCondition = 1;
                        else
                            detail.LogicCondition = int.Parse(dr["LogicCondition"].ToString().Trim());

                        detail.TargetQuestionId = dr["TargetQuestionId"].ToString().Trim().ToLower();
                        detail.BlockOptionList = dr["BlockOptionList"].ToString().Trim().ToLower();

                        details[dtR.Rows.IndexOf(dr)] = detail;
                        //Condition
                        string detailId = dr["LogicId"].ToString().Trim();
                        sqlS = string.Format("select * from QUE006_QuestionnaireRuleCondition where LogicId=@detailId order by CreateDateTime asc");

                        //-------sql para----start
                        SqlParameter[] sqlParams = new SqlParameter[] {
                        new SqlParameter("@detailId", SqlDbType.UniqueIdentifier),


                        };
                        sqlParams[0].Value = detailId.ValidGuid();

                        //-------sql para----end
                        Log.Debug("QueryData:" + sqlS);
                        DataTable dtSubR = _db.GetQueryData(sqlS, sqlParams);
                        if (dtSubR == null || dtSubR.Rows.Count == 0)
                        {
                            detail.ConditionList = null;
                        }
                        else
                        {
                            LogicCondition[] conditions = new LogicCondition[dtSubR.Rows.Count];
                            foreach (DataRow subDR in dtSubR.Rows)
                            {
                                LogicCondition conditon = new LogicCondition();
                                conditon.ConditionQuestionList = subDR["ConditionQuestionList"].ToString().Trim().ToLower();
                                conditon.ConditionOptionList = subDR["ConditionOptionList"].ToString().Trim().ToLower();
                                if (subDR["ConditionRule"] == null || string.IsNullOrEmpty(subDR["ConditionRule"].ToString().Trim()))
                                    conditon.ConditionRule = 1;
                                else
                                    conditon.ConditionRule = int.Parse(subDR["ConditionRule"].ToString().Trim());

                                conditon.MatrixField = subDR["MatrixField"].ToString().Trim();

                                conditions[dtSubR.Rows.IndexOf(subDR)] = conditon;
                            }
                            detail.ConditionList = conditions;
                        }
                    }
                    logic.LogicList = details;
                    return logic;
                }
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }
    }
}

