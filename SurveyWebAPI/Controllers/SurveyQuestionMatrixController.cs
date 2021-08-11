using System;
using System.Data;
using System.Text;
using Common;
using Microsoft.AspNetCore.Mvc;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using SurveyWebAPI.Utility;
using SurveyWebAPI.Models;
using System.Linq;
using Microsoft.AspNetCore.Authorization;
using System.Data.SqlClient;
using System.Collections.Generic;

namespace SurveyWebAPI.Controllers
{
    [Authorize]
    [Route("api/Survey/Question/Matrix")]
    [ApiController]
    public class SurveyQuestionMatrixController : ControllerBase
    {
        private readonly DBHelper _db;
        public SurveyQuestionMatrixController()
        {
            _db = new DBHelper(AppSettingsHelper.DefaultConnectionString);
        }
        [Route("Query")]
        [HttpGet]
        public object Query(string surveyId, string questionId)
        {
            Log.Debug("矩陣題-單題查詢(編輯用)");

            //JObject jo = (JObject)JsonConvert.DeserializeObject(value.ToString());
            //string surveyId = jo["surveyId"].ToString();
            //string questionId = jo["questionId"].ToString();
            //返回數據
            ReplyData replyData = new ReplyData();
            try
            {
                //data實體
                replyData.code = "200";
                replyData.message = $"查詢記錄完成。";
                replyData.data = QueryData(0, surveyId, questionId);
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
            JObject jo = (JObject)JsonConvert.DeserializeObject(value.ToString());
            string surveyId = jo["SurveyId"].ToString();
            string questionType = jo["QuestionType"].ToString();
            string pageNo = jo["PageNo"].ToString();
            string questionId  = Guid.NewGuid().ToString();

            var replyData = new ReplyData();
            try
            {
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
                //取得當前最大題數號+1
                string sqlS = string.Format("select max(QuestionSeq) MaxQuestionSeq from QUE002_QuestionnaireDetail where SurveyId=@surveyId");
                SqlParameter[] sqlSParams = new SqlParameter[] {
                new SqlParameter("@surveyId", SqlDbType.UniqueIdentifier),

                };
                sqlSParams[0].Value = surveyId.ValidGuid();
                string maxQuestionSeq = _db.GetSingle(sqlS, sqlSParams);
                if (string.IsNullOrEmpty(maxQuestionSeq))
                    maxQuestionSeq = "1";
                else
                    maxQuestionSeq = (int.Parse(maxQuestionSeq) + 1).ToString();
                //只新增QUE002,並且返回一個data實體到前端
                sqlS = @"INSERT INTO QUE002_QuestionnaireDetail(SurveyId, QuestionId, QuestionSeq, QuestionType, QuestionSubject, SubjectStyle, QuestionNote, PageNo,
                                IsRequired, HasOther, OtherIsShowText, OtherVerify, OtherTextMandatory, OtherCheckMessage, IsSetShowNum,PCRowNum, MobileRowNum, IsRamdomOption, 
                                ExcludeOther, BaseDataValidType, BlankDefaultWords,BlankValidType, MatrixItems, UpdUserId, UpdDateTime,BlankMaxLimit,BlankMinLimit,QuestionImage,QuestionVideo,MultiOptionLimit,OtherMinLimit,OtherMaxLimit)
                                VALUES(@surveyId,@questionId,@maxQuestionSeq,@questionType,'','','',@pageNo,'0','0','0',1,'0','','0',0,0,'0','0',0,'',0,'',@userId,SYSDATETIME(),'','','','','',null,null)";
                Log.Debug("新增QUE002_QuestionnaireDetail:" + sqlS);
                //-------sql para----start
                SqlParameter[] sqlParams = new SqlParameter[] {
                    new SqlParameter("@surveyId", SqlDbType.UniqueIdentifier),
                    new SqlParameter("@questionId", SqlDbType.UniqueIdentifier),
                    new SqlParameter("@maxQuestionSeq", SqlDbType.Int),
                    new SqlParameter("@questionType", SqlDbType.Int),
                    new SqlParameter("@pageNo", SqlDbType.Int),
                    new SqlParameter("@userId", SqlDbType.UniqueIdentifier)
                };
                sqlParams[0].Value = surveyId.ValidGuid();
                sqlParams[1].Value = questionId.ValidGuid();
                sqlParams[2].Value = maxQuestionSeq.ValidInt();
                sqlParams[3].Value = questionType.ValidInt();
                sqlParams[4].Value = pageNo.ValidInt();
                sqlParams[5].Value = userId.ValidGuid();
                //-------sql para----end

                int iR = _db.ExecuteSql(sqlS,sqlParams);

                replyData.code = "200";
                replyData.message = "新增記錄完成。";
                replyData.data = QueryData(1, surveyId, questionId);
            }
            catch (Exception ex)
            {
                replyData.code = "-1";
                replyData.message = $"新增記錄失敗!{ex.Message}.";
                replyData.data = null;
                Log.Error("新增記錄失敗!" + ex.Message);
            }
            return JsonConvert.SerializeObject(replyData);
        }

        [Route("Update")]
        [HttpPut]
        public object Update([FromBody] Object value)
        {
            Matrix matrix = JsonConvert.DeserializeObject<Matrix>(value.ToString());
            string surveyId = matrix.SurveyId;
            string questionId = matrix.QuestionId;
            var replyData = new ReplyData();
            try
            {
                string MatrixItems = matrix.main.MatrixItems.Trim();
                if (string.IsNullOrEmpty(MatrixItems))
                {
                    replyData.code = "-1";
                    replyData.message = "更新資料失敗!MatrixItems不可為空白!";
                    replyData.data = null;
                    Log.Error("更新資料失敗!MatrixItems不可為空白!");
                    return JsonConvert.SerializeObject(replyData);
                }
                else
                {
                    string s = string.Join(";", MatrixItems.Split(';').Distinct().ToArray());
                    if (s.Length != MatrixItems.Length)
                    {
                        ErrorCode.Code = "201";
                        replyData.code = ErrorCode.Code;
                        replyData.message = ErrorCode.Message;
                        replyData.data = null;
                        Log.Error("更新資料失敗!MatrixItems欄位不可重複!");
                        return JsonConvert.SerializeObject(replyData);
                    }
                }

                //,BaseDataValidType={19},BlankDefaultWords='{20}',BlankValidType='{21}',MatrixItems='{22}',UpdUserId='{22}',UpdDateTime='{23}'
                StringBuilder sqlSB = new StringBuilder();
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
                //更新QUE002,Check傳入數據中如果有null值，則不去更新相關欄位
                sqlSB.Append("Update QUE002_QuestionnaireDetail set QuestionSeq=@QuestionSeq,QuestionType=@QuestionType, PageNo=@PageNo,IsRequired=@IsRequired,UpdUserId=@userId,UpdDateTime=SYSDATETIME() ");
                //-------sql para----start
                List<SqlParameter> sqlParams = new List<SqlParameter>();
                //-------sql para----end

                var obj = new SqlParameter("@QuestionSeq", SqlDbType.Int);
                obj.Value = matrix.QuestionSeq.ValidInt();
                sqlParams.Add(obj);
                 obj = new SqlParameter("@QuestionType", SqlDbType.Int);
                obj.Value = matrix.QuestionType.ValidInt();
                sqlParams.Add(obj);
                 obj = new SqlParameter("@PageNo", SqlDbType.Int);
                obj.Value = matrix.PageNo.ValidInt();
                sqlParams.Add(obj);
                 obj = new SqlParameter("@IsRequired", SqlDbType.Bit);
                obj.Value = matrix.IsRequired.ValidBit();
                sqlParams.Add(obj);
                 obj = new SqlParameter("@userId", SqlDbType.UniqueIdentifier);
                obj.Value = userId.ValidGuid();
                sqlParams.Add(obj);
                //-------sql para----end

                if (matrix.main != null)
                {
                    if (!string.IsNullOrEmpty(matrix.main.MatrixItems.Trim()) && matrix.main.MatrixItems.Trim().Substring(0, 1).Equals(";"))
                    {
                        matrix.main.MatrixItems = matrix.main.MatrixItems.Trim().Substring(1);
                    }
                    sqlSB.Append(",QuestionSubject=@QuestionSubject ,SubjectStyle=@SubjectStyle ,QuestionNote=@QuestionNote ,QuestionImage=@QuestionImage ,QuestionVideo=@QuestionVideo ,MatrixItems=@MatrixItems ");
                    //-------sql para----start
                    var obj1 = new SqlParameter("@QuestionSubject", SqlDbType.NVarChar);
                    obj1.Value = matrix.main.QuestionSubject.Valid();
                    sqlParams.Add(obj1);
                    obj1 = new SqlParameter("@SubjectStyle", SqlDbType.NVarChar);
                    obj1.Value = matrix.main.SubjectStyle.Valid();
                    sqlParams.Add(obj1);
                    Log.Debug("Update SubjectStyle:" + matrix.main.SubjectStyle);

                    obj1 = new SqlParameter("@QuestionNote", SqlDbType.NVarChar);
                    obj1.Value = matrix.main.QuestionNote.Valid();
                    sqlParams.Add(obj1);
                    obj1 = new SqlParameter("@QuestionImage", SqlDbType.NVarChar);
                    obj1.Value = matrix.main.QuestionImage.Valid();
                    sqlParams.Add(obj1);
                    obj1 = new SqlParameter("@QuestionVideo", SqlDbType.NVarChar);
                    obj1.Value = matrix.main.QuestionVideo.Valid();
                    sqlParams.Add(obj1);
                    obj1 = new SqlParameter("@MatrixItems", SqlDbType.NVarChar);
                    obj1.Value = matrix.main.MatrixItems.Valid();
                    sqlParams.Add(obj1);
                    //-------sql para----end


                }
                sqlSB.Append("where SurveyId=@SurveyId and QuestionId=@QuestionId ");
                //-------sql para----start
                var obj2 = new SqlParameter("@SurveyId", SqlDbType.UniqueIdentifier);
                obj2.Value = surveyId.ValidGuid();
                sqlParams.Add(obj2);
                Log.Debug("Update QUE002_QuestionnaireDetail SurveyId:" + surveyId);
                obj2 = new SqlParameter("@QuestionId", SqlDbType.UniqueIdentifier);
                obj2.Value = matrix.QuestionId.ValidGuid();
                sqlParams.Add(obj2);
                //-------sql para----end

                sqlS = sqlSB.ToString();
                Log.Debug("Update QUE002_QuestionnaireDetail:" + sqlS);
                int iR = _db.ExecuteSql(sqlS, sqlParams.ToArray());

                //更新QUE003,先刪除再新增
                sqlS = "delete from QUE003_QuestionnaireOptions where QuestionId=@QuestionId";
                //-------sql para----start
                SqlParameter[] sqlParamsA = new SqlParameter[] {
                    new SqlParameter("@QuestionId", SqlDbType.UniqueIdentifier)
                };
                sqlParamsA[0].Value = matrix.QuestionId.ValidGuid();
                //-------sql para----end
                Log.Debug("delete QUE003_QuestionnaireOptions:" + sqlS);
                int dR = _db.ExecuteSql(sqlS, sqlParamsA);

                if (matrix.main.option != null)
                {
                    foreach (OptionM opt in matrix.main.option)
                    {
                        if (string.IsNullOrEmpty(opt.OptionId))
                        {
                            opt.OptionId = Guid.NewGuid().ToString();
                        }
                        if (opt.ChildQuestionId == null || string.IsNullOrEmpty(opt.ChildQuestionId))
                        {
                            sqlS = "Insert Into QUE003_QuestionnaireOptions(QuestionId,OptionId,OptionSeq,OptionType,OptionContent,ChildQuestionId,UpdUserId,UpdDateTime) values(@QuestionId,@OptionId,@OptionSeq,@OptionType,@OptionContent,null,@userId,SYSDATETIME()) ";
                            //-------sql para----start
                            SqlParameter[] sqlParamsD = new SqlParameter[] {
                                new SqlParameter("@QuestionId", SqlDbType.UniqueIdentifier),
                                new SqlParameter("@OptionId", SqlDbType.UniqueIdentifier),
                                new SqlParameter("@OptionSeq", SqlDbType.Int),
                                new SqlParameter("@OptionType", SqlDbType.Int),
                                new SqlParameter("@OptionContent", SqlDbType.NVarChar),
                                new SqlParameter("@userId", SqlDbType.UniqueIdentifier)
                            };
                            sqlParamsD[0].Value = matrix.QuestionId.ValidGuid();
                            sqlParamsD[1].Value = opt.OptionId.ValidGuid();
                            sqlParamsD[2].Value = opt.OptionSeq.ValidInt();
                            sqlParamsD[3].Value = opt.OptionType.ValidInt();
                            sqlParamsD[4].Value = opt.OptionContent.Valid();
                            sqlParamsD[5].Value = userId.ValidGuid();
                            //-------sql para----end
                            iR += _db.ExecuteSql(sqlS, sqlParamsD);
                        }
                        else
                        {
                            sqlS = "Insert Into QUE003_QuestionnaireOptions(QuestionId,OptionId,OptionSeq,OptionType,OptionContent,ChildQuestionId,UpdUserId,UpdDateTime) values(@QuestionId,@OptionId,@OptionSeq,@OptionType,@OptionContent,@ChildQuestionId,@userId,SYSDATETIME()) ";
                            //-------sql para----start
                            SqlParameter[] sqlParamsD = new SqlParameter[] {
                                new SqlParameter("@QuestionId", SqlDbType.UniqueIdentifier),
                                new SqlParameter("@OptionId", SqlDbType.UniqueIdentifier),
                                new SqlParameter("@OptionSeq", SqlDbType.Int),
                                new SqlParameter("@OptionType", SqlDbType.Int),
                                new SqlParameter("@OptionContent", SqlDbType.NVarChar),
                                new SqlParameter("@ChildQuestionId", SqlDbType.UniqueIdentifier),
                                new SqlParameter("@userId", SqlDbType.UniqueIdentifier)
                            };
                            sqlParamsD[0].Value = matrix.QuestionId.ValidGuid();
                            sqlParamsD[1].Value = opt.OptionId.ValidGuid();
                            sqlParamsD[2].Value = opt.OptionSeq.ValidInt();
                            sqlParamsD[3].Value = opt.OptionType.ValidInt();
                            sqlParamsD[4].Value = opt.OptionContent.Valid();
                            sqlParamsD[5].Value = opt.ChildQuestionId.ValidGuid();
                            sqlParamsD[6].Value = userId.ValidGuid();
                            //-------sql para----end
                            iR += _db.ExecuteSql(sqlS, sqlParamsD);
                        }
                        Log.Debug("Update QUE003_QuestionnaireOptions:" + sqlS);

                    }
                }
                replyData.code = "200";
                replyData.message = $"更新資料成功!";
                replyData.data = QueryData(2, surveyId, questionId);
            }
            catch (Exception ex)
            {
                replyData.code = "-1";
                replyData.message = $"更新資料失敗!{ex.Message}.";
                replyData.data = null;
                Log.Error("api/Survey/Question/Matrix/Update 更新資料失敗!" + ex.Message);
            }
            return JsonConvert.SerializeObject(replyData);
        }
        [Route("Delete")]
        [HttpDelete]
        public object Delete(string surveyId, string questionId)
        {
            //JObject jo = (JObject)JsonConvert.DeserializeObject(value.ToString());

            //string surveyId = jo["surveyId"].ToString();
            //string questionId = jo["questionId"].ToString();

            var replyData = new ReplyData();
            try
            {
                string sSql = "DELETE FROM QUE002_QuestionnaireDetail WHERE SurveyId=@SurveyId and QuestionId=@QuestionId ";
                Log.Debug("刪除QUE002_QuestionnaireDetail:" + sSql);
                //-------sql para----start
                SqlParameter[] sqlParamsE = new SqlParameter[] {
                    new SqlParameter("@SurveyId", SqlDbType.UniqueIdentifier),
                    new SqlParameter("@QuestionId", SqlDbType.UniqueIdentifier)
                };
                sqlParamsE[0].Value = surveyId.ValidGuid();
                sqlParamsE[1].Value = questionId.ValidGuid();
                //-------sql para----end
                int iR = _db.ExecuteSql(sSql, sqlParamsE);

                sSql = "DELETE FROM QUE003_QuestionnaireOptions WHERE QuestionId=@QuestionId ";
                Log.Debug("刪除QUE003_QuestionnaireOptions:" + sSql);
                //-------sql para----start
                SqlParameter[] sqlParamsF = new SqlParameter[] {
                    new SqlParameter("@QuestionId", SqlDbType.UniqueIdentifier)
                };
                sqlParamsF[0].Value = questionId.ValidGuid();
                //-------sql para----end
                iR += _db.ExecuteSql(sSql, sqlParamsF);

                replyData.code = "200";
                replyData.message = $"刪除記錄完成。";
                replyData.data = iR;
                //2021/01/22 題目刪除，要把相關邏輯規則和子母題資料也清掉。
                Utility.Common.CleanQuestionId(questionId, surveyId);
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
        protected Matrix QueryData(int opType, string surveyId, string questionId)
        {
            //data實體
            Matrix matrix = new Matrix();
            MainM main = new MainM();
            try
            {
                string sqlS = "select * from QUE002_QuestionnaireDetail where SurveyId=@SurveyId and QuestionId=@QuestionId order by QuestionSeq";
                //-------sql para----start
                SqlParameter[] sqlParams = new SqlParameter[] {
                    new SqlParameter("@SurveyId", SqlDbType.UniqueIdentifier),
                    new SqlParameter("@QuestionId", SqlDbType.UniqueIdentifier)
                };
                sqlParams[0].Value = surveyId.ValidGuid();
                sqlParams[1].Value = questionId.ValidGuid();
                //-------sql para----end
                DataTable dtR = _db.GetQueryData(sqlS, sqlParams);

                if (dtR != null && dtR.Rows.Count == 1)
                {
                    //Fill data
                    matrix.SurveyId = dtR.Rows[0]["SurveyId"].ToString().Trim();
                    matrix.QuestionId = dtR.Rows[0]["QuestionId"].ToString().Trim();
                    if (dtR.Rows[0]["QuestionSeq"] == null || string.IsNullOrEmpty(dtR.Rows[0]["QuestionSeq"].ToString().Trim()))
                        matrix.QuestionSeq = 1;
                    else
                        matrix.QuestionSeq = int.Parse(dtR.Rows[0]["QuestionSeq"].ToString().Trim());
                    if (dtR.Rows[0]["QuestionType"] == null || string.IsNullOrEmpty(dtR.Rows[0]["QuestionType"].ToString().Trim()))
                        matrix.QuestionType = 1;
                    else
                        matrix.QuestionType = int.Parse(dtR.Rows[0]["QuestionType"].ToString().Trim());
                    matrix.IsRequired = dtR.Rows[0]["IsRequired"].ToString().Trim();
                    if (dtR.Rows[0]["PageNo"] == null || string.IsNullOrEmpty(dtR.Rows[0]["PageNo"].ToString().Trim()))
                        matrix.PageNo = 1;
                    else
                        matrix.PageNo = int.Parse(dtR.Rows[0]["PageNo"].ToString().Trim());

                    main.QuestionSubject = dtR.Rows[0]["QuestionSubject"].ToString().Trim();
                    main.SubjectStyle = dtR.Rows[0]["SubjectStyle"].ToString().Trim();
                    main.QuestionNote = dtR.Rows[0]["QuestionNote"].ToString().Trim();
                    main.QuestionImage = dtR.Rows[0]["QuestionImage"].ToString().Trim();
                    main.QuestionVideo = dtR.Rows[0]["QuestionVideo"].ToString().Trim();
                    main.MatrixItems = dtR.Rows[0]["MatrixItems"].ToString().Trim();

                    sqlS = "select * from QUE003_QuestionnaireOptions where QuestionId=@QuestionId order by OptionSeq";
                    //-------sql para----start
                    SqlParameter[] sqlParamsA = new SqlParameter[] {
                        new SqlParameter("@QuestionId", SqlDbType.UniqueIdentifier)
                    };
                    sqlParamsA[0].Value = questionId.ValidGuid();
                    //-------sql para----end
                    dtR = _db.GetQueryData(sqlS, sqlParamsA);
                    if (dtR == null || dtR.Rows.Count == 0)
                    {
                        main.option = null;
                        matrix.main = main;
                    }
                    else
                    {
                        OptionM[] options = new OptionM[dtR.Rows.Count];
                        foreach (DataRow dr in dtR.Rows)
                        {
                            OptionM option = new OptionM();
                            option.OptionId = dr["OptionId"].ToString().Trim();
                            if (dr["OptionSeq"] == null || string.IsNullOrEmpty(dr["OptionSeq"].ToString().Trim()))
                                option.OptionSeq = 0;
                            else
                                option.OptionSeq = int.Parse(dr["OptionSeq"].ToString().Trim());
                            if (dr["OptionType"] == null || string.IsNullOrEmpty(dr["OptionType"].ToString().Trim()))
                                option.OptionType = 0;
                            else
                                option.OptionType = int.Parse(dr["OptionType"].ToString().Trim());
                            option.OptionContent = dr["OptionContent"].ToString().Trim();
                            option.ChildQuestionId = dr["ChildQuestionId"].ToString().Trim();
                            options[dtR.Rows.IndexOf(dr)] = option;
                        }
                        main.option = options;
                        matrix.main = main;
                    }
                    return matrix;
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
    }
}
