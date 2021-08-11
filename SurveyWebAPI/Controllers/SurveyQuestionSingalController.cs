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
using System.Collections.Generic;

namespace SurveyWebAPI.Controllers
{
    [Authorize]
    [Route("api/Survey/Question/Singal")]
    [ApiController]
    public class SurveyQuestionSingalController : ControllerBase
    {
        private readonly DBHelper _db;
        public SurveyQuestionSingalController()
        {
            _db = new DBHelper(AppSettingsHelper.DefaultConnectionString);
        }
        [Route("Query")]
        [HttpGet]
        public object Query(string surveyId, string questionId, bool otherFlag)
        {
            Log.Debug("單選題-單題查詢(編輯用)");

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
                replyData.data = QueryData(0, surveyId, questionId, otherFlag);
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
                SqlParameter[] sqlParamsMax = new SqlParameter[] {
                    new SqlParameter("@surveyId", SqlDbType.UniqueIdentifier)
                };
                sqlParamsMax[0].Value = surveyId.ValidGuid();
                //-------sql para----end
                string maxQuestionSeq = _db.GetSingle(sqlS, sqlParamsMax);
                if (string.IsNullOrEmpty(maxQuestionSeq))
                    maxQuestionSeq = "1";
                else
                    maxQuestionSeq = (int.Parse(maxQuestionSeq) + 1).ToString();
                //只新增QUE002,並且返回一個data實體到前端
                sqlS = string.Format(@"INSERT INTO QUE002_QuestionnaireDetail(SurveyId, QuestionId, QuestionSeq, QuestionType, QuestionSubject, SubjectStyle, QuestionNote, PageNo,
                                IsRequired, HasOther, OtherIsShowText, OtherVerify, OtherTextMandatory, OtherCheckMessage, IsSetShowNum,PCRowNum, MobileRowNum, IsRamdomOption, 
                                ExcludeOther, BaseDataValidType, BlankDefaultWords,BlankValidType, MatrixItems, UpdUserId, UpdDateTime,BlankMaxLimit,BlankMinLimit,QuestionImage,QuestionVideo,MultiOptionLimit,OtherMinLimit,OtherMaxLimit)
                                VALUES(@surveyId,@questionId,@maxQuestionSeq,@questionType,'','','',@pageNo,'0','0','0',1,'0','','0',0,0,'0','0',0,'',0,'',@userId,SYSDATETIME(),'','','','','',null,null)");
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

                int iR = _db.ExecuteSql(sqlS, sqlParams);

                replyData.code = "200";
                replyData.message = "新增記錄完成。";
                replyData.data = QueryData(1, surveyId, questionId, false);
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
            Singal singal = JsonConvert.DeserializeObject<Singal>(value.ToString());
            string surveyId = singal.SurveyId;
            string questionId = singal.QuestionId;
            var replyData = new ReplyData();
            try
            {
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

                //-------sql para----start
                List<SqlParameter> sqlParams = new List<SqlParameter>();
                //-------sql para----end

                //更新QUE002,Check傳入數據中如果有null值，則不去更新相關欄位
                sqlSB.Append("Update QUE002_QuestionnaireDetail set QuestionSeq=@QuestionSeq,QuestionType=@QuestionType, PageNo=@PageNo,IsRequired=@IsRequired,HasOther=@HasOther,UpdUserId=@userId,UpdDateTime=SYSDATETIME() ");
                //-------sql para----start
                var obj = new SqlParameter("@QuestionSeq", SqlDbType.Int);
                obj.Value = singal.QuestionSeq.ValidInt();
                sqlParams.Add(obj);
                 obj = new SqlParameter("@QuestionType", SqlDbType.Int);
                obj.Value = singal.QuestionType.ValidInt();
                sqlParams.Add(obj);
                 obj = new SqlParameter("@PageNo", SqlDbType.Int);
                obj.Value = singal.PageNo.ValidInt();
                sqlParams.Add(obj);
                 obj = new SqlParameter("@IsRequired", SqlDbType.Bit);
                Log.Debug("IsRequired:" + singal.IsRequired);
                obj.Value = singal.IsRequired.ValidBit();
                sqlParams.Add(obj);
                 obj = new SqlParameter("@HasOther", SqlDbType.Bit);
                Log.Debug("IsRequired:" + singal.HasOther);
                obj.Value = singal.HasOther.ValidBit();
                sqlParams.Add(obj);
                 obj = new SqlParameter("@userId", SqlDbType.UniqueIdentifier);
                obj.Value = userId.ValidGuid();
                sqlParams.Add(obj);
                //-------sql para----end


                if (singal.main != null)
                {
                    sqlSB.Append(",QuestionSubject=@QuestionSubject,SubjectStyle=@SubjectStyle,QuestionNote=@QuestionNote,QuestionImage=@QuestionImage,QuestionVideo=@QuestionVideo ");
                    //-------sql para----start
                    var obj1 = new SqlParameter("@QuestionSubject", SqlDbType.NVarChar);
                    obj1.Value = singal.main.QuestionSubject.Valid();
                    sqlParams.Add(obj1);
                    obj1 = new SqlParameter("@SubjectStyle", SqlDbType.NVarChar);
                    obj1.Value = singal.main.SubjectStyle.Valid();
                    sqlParams.Add(obj1);
                    obj1 = new SqlParameter("@QuestionNote", SqlDbType.NVarChar);
                    obj1.Value = singal.main.QuestionNote.Valid();
                    sqlParams.Add(obj1);
                    obj1 = new SqlParameter("@QuestionImage", SqlDbType.NVarChar);
                    obj1.Value = singal.main.QuestionImage.Valid();
                    sqlParams.Add(obj1);
                    obj1 = new SqlParameter("@QuestionVideo", SqlDbType.NVarChar);
                    obj1.Value = singal.main.QuestionVideo.Valid();
                    sqlParams.Add(obj1);
                    //-------sql para----end

                    if (singal.main.other != null)
                    {
                        sqlSB.Append(",OtherIsShowText=@OtherIsShowText,OtherVerify=@OtherVerify,OtherTextMandatory=@OtherMandatory,OtherCheckMessage=@OtherCheckMessage,OtherMinLimit=@OtherMinLimit,OtherMaxLimit=@OtherMaxLimit ");

                        //-------sql para----start
                        var obj2 = new SqlParameter("@OtherIsShowText", SqlDbType.Bit);
                        Log.Debug("OtherIsShowText:" + singal.main.other.OtherIsShowText);
                        obj2.Value = singal.main.other.OtherIsShowText.ValidBit();
                        sqlParams.Add(obj2);
                        obj2 = new SqlParameter("@OtherVerify", SqlDbType.Int);
                        obj2.Value = singal.main.other.OtherVerify.ValidInt();
                        sqlParams.Add(obj2);
                        obj2 = new SqlParameter("@OtherMandatory", SqlDbType.Bit);
                        Log.Debug("OtherIsShowText:" + singal.main.other.OtherMandatory);
                        obj2.Value = singal.main.other.OtherMandatory.ValidBit();
                        sqlParams.Add(obj2);
                        obj2 = new SqlParameter("@OtherCheckMessage", SqlDbType.NVarChar);
                        obj2.Value = singal.main.other.OtherCheckMessage.Valid();
                        sqlParams.Add(obj2);
                        obj2 = new SqlParameter("@OtherMinLimit", SqlDbType.Int);
                        obj2.Value = singal.main.other.OtherMinLimit.ValidInt();
                        sqlParams.Add(obj2);
                        obj2 = new SqlParameter("@OtherMaxLimit", SqlDbType.Int);
                        obj2.Value = singal.main.other.OtherMaxLimit.ValidInt();
                        sqlParams.Add(obj2);
                        //-------sql para----end


                        if (!String.IsNullOrEmpty( singal.main.other.OtherChildQuestionId ))
                        {
                            sqlSB.Append(" ,OtherChildQuestionId=@OtherChildQuestionId ");
                            //-------sql para----start
                            var obj3 = new SqlParameter("@OtherChildQuestionId", SqlDbType.UniqueIdentifier);
                            obj3.Value = singal.main.other.OtherChildQuestionId.ValidGuid();
                            sqlParams.Add(obj3);
                            //-------sql para----end
                        }
                    }
                }
                if (singal.advance.showWay != null)
                {
                    sqlSB.Append(",IsSetShowNum=@IsSetShowNum,PCRowNum=@PCRowNum,MobileRowNum=@MobileRowNum ");
                    //-------sql para----start
                    var obj4 = new SqlParameter("@IsSetShowNum", SqlDbType.Bit);
                    Log.Debug("IsSetShowNum:" + singal.advance.showWay.IsSetShowNum);
                    obj4.Value = singal.advance.showWay.IsSetShowNum.ValidBit();
                    sqlParams.Add(obj4);
                    obj4 = new SqlParameter("@PCRowNum", SqlDbType.Int);
                    obj4.Value = singal.advance.showWay.PCRowNum.ValidInt();
                    sqlParams.Add(obj4);
                    obj4 = new SqlParameter("@MobileRowNum", SqlDbType.Int);
                    obj4.Value = singal.advance.showWay.MobileRowNum.ValidInt();
                    sqlParams.Add(obj4);
                    //-------sql para----end
                }
                if (singal.advance.random != null)
                {
                    sqlSB.Append(",IsRamdomOption=@IsRamdomOption,ExcludeOther=@ExcludeOther ");
                    //-------sql para----start
                    var obj5 = new SqlParameter("@IsRamdomOption", SqlDbType.Bit);
                    Log.Debug("IsRamdomOption:" + singal.advance.random.IsRamdomOption);
                    obj5.Value = singal.advance.random.IsRamdomOption.ValidBit();
                    sqlParams.Add(obj5);
                    obj5 = new SqlParameter("@ExcludeOther", SqlDbType.Bit);
                    Log.Debug("ExcludeOther:" + singal.advance.random.ExcludeOther);
                    obj5.Value = singal.advance.random.ExcludeOther.ValidBit();
                    sqlParams.Add(obj5);
                    //-------sql para----end
                }
                sqlSB.Append("where SurveyId=@SurveyId and QuestionId=@QuestionId ");
                //-------sql para----start
                var obj6 = new SqlParameter("@SurveyId", SqlDbType.UniqueIdentifier);
                obj6.Value = singal.SurveyId.ValidGuid();
                sqlParams.Add(obj6);
                obj6 = new SqlParameter("@QuestionId", SqlDbType.UniqueIdentifier);
                obj6.Value = singal.QuestionId.ValidGuid();
                sqlParams.Add(obj6);
                //-------sql para----end

                sqlS = sqlSB.ToString();
                Log.Debug("Update QUE002_QuestionnaireDetail:" + sqlS);
                int iR = _db.ExecuteSql(sqlS, sqlParams.ToArray());



                //更新QUE003,先刪除再新增
                int optionCount = 0;
                sqlS = "delete from QUE003_QuestionnaireOptions where QuestionId=@QuestionId";
                //-------sql para----start
                SqlParameter[] sqlParamsA = new SqlParameter[] {
                    new SqlParameter("@QuestionId", SqlDbType.UniqueIdentifier)
                };
                sqlParamsA[0].Value = singal.QuestionId.ValidGuid();
                //-------sql para----end
                Log.Debug("delete QUE003_QuestionnaireOptions:" + sqlS);
                int dR = _db.ExecuteSql(sqlS, sqlParamsA);

                if (singal.main.option != null)
                {
                    foreach (Option opt in singal.main.option)
                    {
                        if (string.IsNullOrEmpty(opt.OptionId))
                        {
                            opt.OptionId = Guid.NewGuid().ToString();

                        }
                        if (opt.ChildQuestionId == null || string.IsNullOrEmpty(opt.ChildQuestionId))
                        {
                            sqlS = "Insert Into QUE003_QuestionnaireOptions(QuestionId,OptionId,OptionSeq,OptionType,OptionContent,ChildQuestionId,UpdUserId,UpdDateTime,OptionImage,OptionVideo) values(@QuestionId,@OptionId,@OptionSeq,@OptionType,@OptionContent,null,@userId,SYSDATETIME(),@OptionImage,@OptionVideo) ";

                            //-------sql para----start
                            SqlParameter[] sqlParamsB = new SqlParameter[] {
                                new SqlParameter("@QuestionId", SqlDbType.UniqueIdentifier),
                                new SqlParameter("@OptionId", SqlDbType.UniqueIdentifier),
                                new SqlParameter("@OptionSeq", SqlDbType.Int),
                                new SqlParameter("@OptionType", SqlDbType.Int),
                                new SqlParameter("@OptionContent", SqlDbType.NVarChar),
                                new SqlParameter("@userId", SqlDbType.UniqueIdentifier),
                                new SqlParameter("@OptionImage", SqlDbType.NVarChar),
                                new SqlParameter("@OptionVideo", SqlDbType.NVarChar)
                            };
                            sqlParamsB[0].Value = singal.QuestionId.ValidGuid();
                            sqlParamsB[1].Value = opt.OptionId.ValidGuid();
                            sqlParamsB[2].Value = opt.OptionSeq.ValidInt();
                            sqlParamsB[3].Value = opt.OptionType.ValidInt();
                            sqlParamsB[4].Value = opt.OptionContent.Valid();
                            sqlParamsB[5].Value = userId.ValidGuid();
                            sqlParamsB[6].Value = opt.OptionImage.Valid();
                            sqlParamsB[7].Value = opt.OptionVideo.Valid();
                            //-------sql para----end
                            iR += _db.ExecuteSql(sqlS, sqlParamsB);
                        }
                        else
                        {
                            sqlS = "Insert Into QUE003_QuestionnaireOptions(QuestionId,OptionId,OptionSeq,OptionType,OptionContent,ChildQuestionId,UpdUserId,UpdDateTime,OptionImage,OptionVideo) values(@QuestionId,@OptionId,@OptionSeq,@OptionType,@OptionContent,@ChildQuestionId,@userId,SYSDATETIME(),@OptionImage,@OptionVideo) ";

                            //-------sql para----start
                            SqlParameter[] sqlParamsB = new SqlParameter[] {
                                new SqlParameter("@QuestionId", SqlDbType.UniqueIdentifier),
                                new SqlParameter("@OptionId", SqlDbType.UniqueIdentifier),
                                new SqlParameter("@OptionSeq", SqlDbType.Int),
                                new SqlParameter("@OptionType", SqlDbType.Int),
                                new SqlParameter("@OptionContent", SqlDbType.NVarChar),
                                new SqlParameter("@ChildQuestionId", SqlDbType.UniqueIdentifier),
                                new SqlParameter("@userId", SqlDbType.UniqueIdentifier),
                                new SqlParameter("@OptionImage", SqlDbType.NVarChar),
                                new SqlParameter("@OptionVideo", SqlDbType.NVarChar)
                            };
                            sqlParamsB[0].Value = singal.QuestionId.ValidGuid();
                            sqlParamsB[1].Value = opt.OptionId.ValidGuid();
                            sqlParamsB[2].Value = opt.OptionSeq.ValidInt();
                            sqlParamsB[3].Value = opt.OptionType.ValidInt();
                            sqlParamsB[4].Value = opt.OptionContent.Valid();
                            sqlParamsB[5].Value = opt.ChildQuestionId.ValidGuid();
                            sqlParamsB[6].Value = userId.ValidGuid();
                            sqlParamsB[7].Value = opt.OptionImage.Valid();
                            sqlParamsB[8].Value = opt.OptionVideo.Valid();
                            //-------sql para----end
                            iR += _db.ExecuteSql(sqlS, sqlParamsB);
                        }
                        Log.Debug("Update QUE003_QuestionnaireOptions:" + sqlS);

                        optionCount++;
                    }
                }
                //單選 / 多選的編輯 QUE002.HasOther = True，就要新增一筆QUE003(已存在不新增)，而且QUE003.otherflag = true，要新增欄位註記是其他選項
                if (Convert.ToBoolean(singal.HasOther))
                {
                    optionCount++;
                    if (singal.main.other.OtherChildQuestionId == null || string.IsNullOrEmpty(singal.main.other.OtherChildQuestionId))
                    {
                        sqlS = @"
Insert Into QUE003_QuestionnaireOptions
(QuestionId,OptionId,OptionSeq,OptionType
,OptionContent,ChildQuestionId,UpdUserId,UpdDateTime,OptionImage
,OptionVideo,OtherFlag) 
values(@QuestionId,@Guid,@optionCount, 0,'其它',null,@userId,SYSDATETIME(),'','','1') ";
                        //-------sql para----start
                        SqlParameter[] sqlParamsC = new SqlParameter[] {
                            new SqlParameter("@QuestionId", SqlDbType.UniqueIdentifier),
                            new SqlParameter("@Guid", SqlDbType.UniqueIdentifier),
                            new SqlParameter("@optionCount", SqlDbType.Int),
                            new SqlParameter("@userId", SqlDbType.UniqueIdentifier)
                        };
                        sqlParamsC[0].Value = singal.QuestionId.ValidGuid();
                        sqlParamsC[1].Value = new System.Data.SqlTypes.SqlGuid(Guid.NewGuid().ToString());
                        sqlParamsC[2].Value = optionCount.ValidInt();
                        sqlParamsC[3].Value = userId.ValidGuid();
                        //-------sql para----end
                        iR += _db.ExecuteSql(sqlS, sqlParamsC);
                    }
                    else
                    {
                        sqlS = @"
Insert Into QUE003_QuestionnaireOptions(QuestionId,OptionId,OptionSeq,OptionType,OptionContent,ChildQuestionId,UpdUserId,UpdDateTime,OptionImage,OptionVideo,OtherFlag) 
values(@QuestionId,@Guid,@optionCount, 0,'其它',@OtherChildQuestionId,@userId,SYSDATETIME(),'','','1') ";

                        //-------sql para----start
                        SqlParameter[] sqlParamsC = new SqlParameter[] {
                            new SqlParameter("@QuestionId", SqlDbType.UniqueIdentifier),
                            new SqlParameter("@Guid", SqlDbType.UniqueIdentifier),
                            new SqlParameter("@optionCount", SqlDbType.Int),
                            new SqlParameter("@userId", SqlDbType.UniqueIdentifier),
                            new SqlParameter("@OtherChildQuestionId", SqlDbType.UniqueIdentifier)
                        };
                        sqlParamsC[0].Value = singal.QuestionId.ValidGuid();
                        sqlParamsC[1].Value = new System.Data.SqlTypes.SqlGuid(Guid.NewGuid().ToString());
                        sqlParamsC[2].Value = optionCount.ValidInt();
                        sqlParamsC[3].Value = userId.ValidGuid();
                        sqlParamsC[4].Value = singal.main.other.OtherChildQuestionId.ValidGuid();
                        //-------sql para----end
                        iR += _db.ExecuteSql(sqlS, sqlParamsC);
                    }

                    Log.Debug("Insert QUE003_QuestionnaireOptions:" + sqlS);

                }
                replyData.code = "200";
                replyData.message = $"更新資料成功!";
                replyData.data = QueryData(2, surveyId, questionId, false);
            }
            catch (Exception ex)
            {
                replyData.code = "-1";
                replyData.message = $"更新資料失敗!{ex.Message}.";
                replyData.data = null;
                Log.Error("api/Survey/Question/Singal/Update 更新資料失敗!" + ex.Message);
            }
            return JsonConvert.SerializeObject(replyData);
        }

        [Route("Delete")]
        [HttpDelete]
        public object Delete(string surveyId, string questionId)
        {
            var replyData = new ReplyData();
            try
            {
                string sSql = "DELETE FROM QUE002_QuestionnaireDetail WHERE SurveyId=@surveyId and QuestionId=@questionId ";
                Log.Debug("刪除QUE002_QuestionnaireDetail:" + sSql);
                //-------sql para----start
                SqlParameter[] sqlParams = new SqlParameter[] {
                    new SqlParameter("@surveyId", SqlDbType.UniqueIdentifier),
                    new SqlParameter("@questionId", SqlDbType.UniqueIdentifier)
                };
                sqlParams[0].Value = surveyId.ValidGuid();
                sqlParams[1].Value = questionId.ValidGuid();
                //-------sql para----end
                int iR = _db.ExecuteSql(sSql, sqlParams);

                sSql = "DELETE FROM QUE003_QuestionnaireOptions WHERE QuestionId=@questionId ";
                //-------sql para----start
                SqlParameter[] sqlParamsA = new SqlParameter[] {
                    new SqlParameter("@questionId", SqlDbType.UniqueIdentifier)
                };
                sqlParamsA[0].Value = questionId.ValidGuid();
                //-------sql para----end
                Log.Debug("刪除QUE003_QuestionnaireOptions:" + sSql);
                iR += _db.ExecuteSql(sSql, sqlParamsA);

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

        protected Singal QueryData(int opType, string surveyId, string questionId, bool otherFlag)
        {
            //data實體
            Singal singal = new Singal();
            Main main = new Main();
            try
            {
                string sqlS = "select * from QUE002_QuestionnaireDetail where SurveyId=@surveyId and QuestionId=@questionId order by QuestionSeq";
                //-------sql para----start
                SqlParameter[] sqlParams = new SqlParameter[] {
                    new SqlParameter("@surveyId", SqlDbType.UniqueIdentifier),
                    new SqlParameter("@questionId", SqlDbType.UniqueIdentifier)
                };
                sqlParams[0].Value = surveyId.ValidGuid();
                sqlParams[1].Value = questionId.ValidGuid();
                //-------sql para----end
                DataTable dtR = _db.GetQueryData(sqlS, sqlParams);

                if (dtR != null && dtR.Rows.Count == 1)
                {
                    //Fill data
                    singal.SurveyId = dtR.Rows[0]["SurveyId"].ToString().Trim();
                    singal.QuestionId = dtR.Rows[0]["QuestionId"].ToString().Trim();
                    if (dtR.Rows[0]["QuestionSeq"] == null || string.IsNullOrEmpty(dtR.Rows[0]["QuestionSeq"].ToString().Trim()))
                        singal.QuestionSeq = 1;
                    else
                        singal.QuestionSeq = int.Parse(dtR.Rows[0]["QuestionSeq"].ToString().Trim());
                    if (dtR.Rows[0]["QuestionType"] == null || string.IsNullOrEmpty(dtR.Rows[0]["QuestionType"].ToString().Trim()))
                        singal.QuestionType = 1;
                    else
                        singal.QuestionType = int.Parse(dtR.Rows[0]["QuestionType"].ToString().Trim());
                    singal.IsRequired = dtR.Rows[0]["IsRequired"].ToString().Trim();
                    singal.HasOther = dtR.Rows[0]["HasOther"].ToString().Trim();
                    if (dtR.Rows[0]["PageNo"] == null || string.IsNullOrEmpty(dtR.Rows[0]["PageNo"].ToString().Trim()))
                        singal.PageNo = 1;
                    else
                        singal.PageNo = int.Parse(dtR.Rows[0]["PageNo"].ToString().Trim());
                    main.QuestionSubject = dtR.Rows[0]["QuestionSubject"].ToString().Trim();
                    main.SubjectStyle = dtR.Rows[0]["SubjectStyle"].ToString().Trim();
                    main.QuestionNote = dtR.Rows[0]["QuestionNote"].ToString().Trim();
                    main.QuestionImage = dtR.Rows[0]["QuestionImage"].ToString().Trim();
                    main.QuestionVideo = dtR.Rows[0]["QuestionVideo"].ToString().Trim();

                    if (opType == 1)
                    {
                        main.other = null;
                        Advance advance = new Advance();
                        advance.showWay = null;
                        advance.random = null;
                        singal.advance = advance;
                    }
                    else
                    {
                        Other other = new Other();
                        other.OtherIsShowText = dtR.Rows[0]["OtherIsShowText"].ToString().Trim();
                        if (dtR.Rows[0]["OtherVerify"] == null || string.IsNullOrEmpty(dtR.Rows[0]["OtherVerify"].ToString().Trim()))
                            other.OtherVerify = 1;
                        else
                            other.OtherVerify = int.Parse(dtR.Rows[0]["OtherVerify"].ToString().Trim());
                        other.OtherMandatory = dtR.Rows[0]["OtherTextMandatory"].ToString().Trim();
                        other.OtherCheckMessage = dtR.Rows[0]["OtherCheckMessage"].ToString().Trim();
                        other.OtherMinLimit = int.Parse(dtR.Rows[0]["OtherMinLimit"].ToString().Trim());
                        other.OtherMaxLimit = int.Parse(dtR.Rows[0]["OtherMaxLimit"].ToString().Trim());
                        other.OtherChildQuestionId = dtR.Rows[0]["OtherChildQuestionId"].ToString().Trim();
                        main.other = other;
                        ShowWay showWay = new ShowWay();
                        showWay.IsSetShowNum = dtR.Rows[0]["IsSetShowNum"].ToString().Trim();
                        if (dtR.Rows[0]["PCRowNum"] == null || string.IsNullOrEmpty(dtR.Rows[0]["PCRowNum"].ToString().Trim()))
                            showWay.PCRowNum = 1;
                        else
                            showWay.PCRowNum = int.Parse(dtR.Rows[0]["PCRowNum"].ToString().Trim());
                        if (dtR.Rows[0]["MobileRowNum"] == null || string.IsNullOrEmpty(dtR.Rows[0]["MobileRowNum"].ToString().Trim()))
                            showWay.MobileRowNum = 1;
                        else
                            showWay.MobileRowNum = int.Parse(dtR.Rows[0]["MobileRowNum"].ToString().Trim());
                        RandomI random = new RandomI();
                        random.IsRamdomOption = dtR.Rows[0]["IsRamdomOption"].ToString().Trim();
                        random.ExcludeOther = dtR.Rows[0]["ExcludeOther"].ToString().Trim();
                        Advance advance = new Advance();
                        advance.showWay = showWay;
                        advance.random = random;
                        singal.advance = advance;
                    }

                    sqlS = "select * from QUE003_QuestionnaireOptions where QuestionId=@questionId ";
                    //-------sql para----start
                    SqlParameter[] sqlParamsA = new SqlParameter[] {
                        new SqlParameter("@questionId", SqlDbType.UniqueIdentifier)
                    };
                    sqlParamsA[0].Value = questionId.ValidGuid();
                    //-------sql para----end
                    if (!otherFlag)//單選 / 多選的查詢【其他選項】不要回傳到前端
                    {
                        sqlS +=" and (OtherFlag='0' or OtherFlag is null)";
                    }
                    sqlS += " order by OptionSeq";
                    dtR = _db.GetQueryData(sqlS, sqlParamsA);
                    if (dtR == null || dtR.Rows.Count == 0)
                    {
                        main.option = null;
                        singal.main = main;
                    }
                    else
                    {
                        Option[] options = new Option[dtR.Rows.Count];
                        foreach (DataRow dr in dtR.Rows)
                        {
                            Option option = new Option();
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
                            option.OptionImage = dr["OptionImage"].ToString().Trim();
                            option.OptionVideo = dr["OptionVideo"].ToString().Trim();
                            options[dtR.Rows.IndexOf(dr)] = option;
                        }
                        main.option = options;
                        singal.main = main;
                    }
                    return singal;
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
