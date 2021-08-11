using System;
using System.Data;
using System.Text;
using Common;
using Microsoft.AspNetCore.Mvc;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using SurveyWebAPI.Utility;
using SurveyWebAPI.Models;
using Microsoft.AspNetCore.Authorization;
using System.Data.SqlClient;

namespace SurveyWebAPI.Controllers
{
    [Authorize]
    [Route("api/Survey/Question/Blank")]
    [ApiController]
    public class SurveyQuestionBlankController : ControllerBase
    {
        private readonly DBHelper _db;
        public SurveyQuestionBlankController()
        {
            _db = new DBHelper(AppSettingsHelper.DefaultConnectionString);
        }
        [Route("Query")]
        [HttpGet]
        public object Query(string surveyId, string questionId)
        {
            Log.Debug("單選題-單題查詢(編輯用)");
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

                //-------sql para----start
                SqlParameter[] sqlParams = new SqlParameter[] {
                    new SqlParameter("@surveyId", SqlDbType.UniqueIdentifier),


                };
                sqlParams[0].Value = surveyId.ValidGuid();


                //-------sql para----end

                string maxQuestionSeq = _db.GetSingle(sqlS, sqlParams);
                if (string.IsNullOrEmpty(maxQuestionSeq))
                    maxQuestionSeq = "1";
                else
                    maxQuestionSeq = (int.Parse(maxQuestionSeq) + 1).ToString();
                //只新增QUE002,並且返回一個data實體到前端
                sqlS = string.Format(@"INSERT INTO QUE002_QuestionnaireDetail(SurveyId, QuestionId, QuestionSeq, QuestionType, QuestionSubject, SubjectStyle, QuestionNote, PageNo,
                                IsRequired, HasOther, OtherIsShowText, OtherVerify, OtherTextMandatory, OtherCheckMessage, IsSetShowNum,PCRowNum, MobileRowNum, IsRamdomOption, 
                                ExcludeOther, BaseDataValidType, BlankDefaultWords,BlankValidType, MatrixItems, UpdUserId, UpdDateTime,BlankMaxLimit,BlankMinLimit,QuestionImage,QuestionVideo,MultiOptionLimit)
                                VALUES(@surveyId,@questionId,@maxQuestionSeq,@questionType,'','','',@pageNo,'0','0','0',0,'0','','0',0,0,'0','0',0,'',1,'',@userId,SYSDATETIME(),'','','','','')");


                //-------sql para----start
                SqlParameter[] sql1Params = new SqlParameter[] {
                    new SqlParameter("@surveyId", SqlDbType.UniqueIdentifier),
                    new SqlParameter("@questionId", SqlDbType.UniqueIdentifier),
                    new SqlParameter("@maxQuestionSeq", SqlDbType.Int),
                    new SqlParameter("@questionType", SqlDbType.Int),
                    new SqlParameter("@pageNo", SqlDbType.Int),
                    new SqlParameter("@userId", SqlDbType.UniqueIdentifier),


                };
                sql1Params[0].Value = surveyId.ValidGuid();
                sql1Params[1].Value = questionId.ValidGuid();
                sql1Params[2].Value = maxQuestionSeq.ValidInt();
                sql1Params[3].Value = questionType.ValidInt();
                sql1Params[4].Value = pageNo.ValidInt();
                sql1Params[5].Value = userId.ValidGuid();

                //-------sql para----end

                //sqlS = string.Format("insert into QUE002_QuestionnaireDetail(SurveyId,QuestionId,QuestionSeq,QuestionType,UpdUserId,UpdDateTime) values('{0}','{1}',{2},{3},NEWID(),'{4}') ",
                //                             surveyId, questionId, maxQuestionSeq, questionType, DateTime.Now.ToString("yyyy/MM/dd HH:mm:ss"));
                Log.Debug("新增QUE002_QuestionnaireDetail:" + sqlS);
                int iR = _db.ExecuteSql(sqlS, sql1Params);

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
            Blank blank = JsonConvert.DeserializeObject<Blank>(value.ToString());
            string surveyId = blank.SurveyId;
            string questionId = blank.QuestionId;
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
                //,BaseDataValidType={19},BlankDefaultWords='{20}',BlankValidType='{21}',MatrixItems='{22}',UpdUserId='{22}',UpdDateTime='{23}'
                StringBuilder sqlSB = new StringBuilder();
                string sqlS = "";

                //更新QUE002,Check傳入數據中如果有null值，則不去更新相關欄位
                sqlSB.Append(string.Format("Update QUE002_QuestionnaireDetail set QuestionSeq=@QuestionSeq,QuestionType=@QuestionType, PageNo=@PageNo,IsRequired=@IsRequired,UpdUserId=@userId,UpdDateTime=SYSDATETIME() "));
                if (blank.main != null)
                {
                    sqlSB.Append(string.Format(",QuestionSubject=@QuestionSubject,SubjectStyle=@SubjectStyle,QuestionNote=@QuestionNote,QuestionImage=@QuestionImage,QuestionVideo=@QuestionVideo,BlankDefaultWords=@BlankDefaultWords,BlankValidType=@BlankValidType,BlankMaxLimit=@BlankMaxLimit,BlankMinLimit=@BlankMinLimit "));
                }

                sqlSB.Append(string.Format("where SurveyId=@SurveyId and QuestionId=@QuestionId "));
                sqlS = sqlSB.ToString();

                //-------sql para----start
                SqlParameter[] sqlParams = new SqlParameter[] {
                    new SqlParameter("@QuestionSeq", SqlDbType.Int),
                    new SqlParameter("@QuestionType", SqlDbType.Int),
                    new SqlParameter("@PageNo", SqlDbType.Int),
                    new SqlParameter("@IsRequired", SqlDbType.Bit),
                    new SqlParameter("@userId", SqlDbType.UniqueIdentifier),
                    new SqlParameter("@QuestionSubject", SqlDbType.NVarChar),
                    new SqlParameter("@SubjectStyle", SqlDbType.NVarChar),
                    new SqlParameter("@QuestionNote", SqlDbType.NVarChar),
                    new SqlParameter("@QuestionImage", SqlDbType.NVarChar),
                    new SqlParameter("@QuestionVideo", SqlDbType.NVarChar),
                    new SqlParameter("@BlankDefaultWords", SqlDbType.NVarChar),
                    new SqlParameter("@BlankValidType", SqlDbType.Int),
                    new SqlParameter("@BlankMaxLimit", SqlDbType.Int),
                    new SqlParameter("@BlankMinLimit", SqlDbType.Int),
                    new SqlParameter("@SurveyId", SqlDbType.UniqueIdentifier),
                    new SqlParameter("@QuestionId", SqlDbType.UniqueIdentifier),


                };
                sqlParams[0].Value = blank.QuestionSeq.ValidInt();
                sqlParams[1].Value = blank.QuestionType.ValidInt();
                sqlParams[2].Value = blank.PageNo.ValidInt();
                Log.Debug("blank.IsRequired:" + blank.IsRequired);
                sqlParams[3].Value = blank.IsRequired.ValidBit();
                sqlParams[4].Value = userId.ValidGuid();
                sqlParams[5].Value = blank.main.QuestionSubject.Valid();
                sqlParams[6].Value = blank.main.SubjectStyle.Valid();
                sqlParams[7].Value = blank.main.QuestionNote.Valid();
                sqlParams[8].Value = blank.main.QuestionImage.Valid();
                sqlParams[9].Value = blank.main.QuestionVideo.Valid();
                sqlParams[10].Value = blank.main.BlankDefaultWords.Valid();
                sqlParams[11].Value = blank.main.BlankValidType.ValidInt();
                sqlParams[12].Value = blank.main.BlankMaxLimit.ValidInt();
                sqlParams[13].Value = blank.main.BlankMinLimit.ValidInt();
                sqlParams[14].Value = blank.SurveyId.ValidGuid();
                sqlParams[15].Value = blank.QuestionId.ValidGuid();


                //-------sql para----end

                Log.Debug("Update QUE002_QuestionnaireDetail:" + sqlS);
                int iR = _db.ExecuteSql(sqlS, sqlParams);

                replyData.code = "200";
                replyData.message = $"更新資料成功!";
                replyData.data = QueryData(2, surveyId, questionId);
            }
            catch (Exception ex)
            {
                replyData.code = "-1";
                replyData.message = $"更新資料失敗!{ex.Message}.";
                replyData.data = null;
                Log.Error("api/Survey/Question/Blank/Update 更新資料失敗!" + ex.Message);
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
                string sSql = string.Format("DELETE FROM QUE002_QuestionnaireDetail WHERE SurveyId=@surveyId and QuestionId=@questionId ");

                //-------sql para----start
                SqlParameter[] sqlParams = new SqlParameter[] {
                    new SqlParameter("@surveyId", SqlDbType.UniqueIdentifier),
                    new SqlParameter("@questionId", SqlDbType.UniqueIdentifier),

                };
                sqlParams[0].Value = surveyId.ValidGuid();
                sqlParams[1].Value = questionId.ValidGuid();


                //-------sql para----end


                Log.Debug("刪除QUE002_QuestionnaireDetail:" + sSql);
                int iR = _db.ExecuteSql(sSql,sqlParams);

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
        protected Blank QueryData(int opType, string surveyId, string questionId)
        {
            //data實體
            Blank blank = new Blank();
            MainB main = new MainB();
            try
            {
                string sqlS = string.Format("select * from QUE002_QuestionnaireDetail where SurveyId=@surveyId and QuestionId=@questionId order by QuestionSeq");

                //-------sql para----start
                SqlParameter[] sqlParams = new SqlParameter[] {
                    new SqlParameter("@surveyId", SqlDbType.UniqueIdentifier),
                    new SqlParameter("@questionId", SqlDbType.UniqueIdentifier),

                };
                sqlParams[0].Value = surveyId.ValidGuid();
                sqlParams[1].Value = questionId.ValidGuid();


                //-------sql para----end


                DataTable dtR = _db.GetQueryData(sqlS,sqlParams);

                if (dtR != null && dtR.Rows.Count == 1)
                {
                    //Fill data
                    blank.SurveyId = dtR.Rows[0]["SurveyId"].ToString().Trim();
                    blank.QuestionId = dtR.Rows[0]["QuestionId"].ToString().Trim();
                    if (dtR.Rows[0]["QuestionSeq"] == null || string.IsNullOrEmpty(dtR.Rows[0]["QuestionSeq"].ToString().Trim()))
                        blank.QuestionSeq = 1;
                    else
                        blank.QuestionSeq = int.Parse(dtR.Rows[0]["QuestionSeq"].ToString().Trim());
                    if (dtR.Rows[0]["QuestionType"] == null || string.IsNullOrEmpty(dtR.Rows[0]["QuestionType"].ToString().Trim()))
                        blank.QuestionType = 1;
                    else
                        blank.QuestionType = int.Parse(dtR.Rows[0]["QuestionType"].ToString().Trim());
                    blank.IsRequired = dtR.Rows[0]["IsRequired"].ToString().Trim();
                    if (dtR.Rows[0]["PageNo"] == null || string.IsNullOrEmpty(dtR.Rows[0]["PageNo"].ToString().Trim()))
                        blank.PageNo = 1;
                    else
                        blank.PageNo = int.Parse(dtR.Rows[0]["PageNo"].ToString().Trim());

                    main.QuestionSubject = dtR.Rows[0]["QuestionSubject"].ToString().Trim();
                    main.SubjectStyle = dtR.Rows[0]["SubjectStyle"].ToString().Trim();
                    main.QuestionNote = dtR.Rows[0]["QuestionNote"].ToString().Trim();
                    main.QuestionImage = dtR.Rows[0]["QuestionImage"].ToString().Trim();
                    main.QuestionVideo = dtR.Rows[0]["QuestionVideo"].ToString().Trim();
                    main.BlankDefaultWords = dtR.Rows[0]["BlankDefaultWords"].ToString().Trim();
                    main.BlankMaxLimit = dtR.Rows[0]["BlankMaxLimit"].ToString().Trim();
                    main.BlankMinLimit = dtR.Rows[0]["BlankMinLimit"].ToString().Trim();
                    if (dtR.Rows[0]["BlankValidType"] == null || string.IsNullOrEmpty(dtR.Rows[0]["BlankValidType"].ToString().Trim()))
                        main.BlankValidType = 1;
                    else
                        main.BlankValidType = int.Parse(dtR.Rows[0]["BlankValidType"].ToString().Trim());
                    blank.main = main;
                    return blank;
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
