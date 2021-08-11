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
using SurveyWebApi.Utility;
using SurveyWebAPI.Models;
using SurveyWebAPI.Utility;

namespace SurveyWebAPI.Controllers
{
    [Authorize]
    [Route("api/Survey/Question/CollectionWay")]
    [ApiController]
    public class SurveyQuestionCollectionWayController : ControllerBase
    {
        /// <summary>
        /// 問卷系統後臺之收集回覆
        /// </summary>
        /// <designer>Allen/Gem</designer>
        private DBHelper _db;
        private DBHelper _crmDB;
        public SurveyQuestionCollectionWayController()
        {
            _db = new DBHelper(AppSettingsHelper.DefaultConnectionString);
            _crmDB = new DBHelper(AppSettingsHelper.CRMConnectionString);
        }
        /// <summary>
        /// 收集回覆-陳核
        /// </summary>
        /// <param name="value"></param>
        /// <returns></returns>
        [Route("Proposal")]
        [HttpPut]
        public String Proposal([FromBody] Object value)
        {
            //依據SurveyId修改QUE001.Audit
            ////"Newtonsoft.Json.Linq.JArray"
            ////"Newtonsoft.Json.Linq.JObject"
            ////多筆資料的話，此處需要處理，暫不管
            ////if(value.GetType().Name=="JArray")
            ////{
            ////    foreach (var val in value as JArray)
            ////    {
            ////        InsertOne(val);
            ////    }
            ////}
            JObject jo = (JObject)JsonConvert.DeserializeObject(value.ToString());
            var replyData = new ReplyData();

            //SurveyId 必須有?
            if (jo["SurveyId"] == null || String.IsNullOrWhiteSpace(jo["SurveyId"].ToString()))
            {
                /* 註解一下GUID：GUID，Globally Unique Identifier ,全局唯一標識，
                 * C#產生時，有下列4種格式：
                 * 格式 xxxxxxxx-xxxx-xxxx-xxxx-xxxxxxxxxxxx 每個x表0-9或者a-f的十六進制
                 * string guid1 = Guid.NewGuid().ToString("N"); d468954e22a145f8806ae41fb938e79e
                 * string guid2 = Guid.NewGuid().ToString("D"); c05d1709-0361-4304-8b2c-58fadcc4ae08
                 * string guid3 = Guid.NewGuid().ToString("P"); (d3a300a7-144d-4587-9e22-3a7699013f01)
                 * string guid4 = Guid.NewGuid().ToString("B"); {3351ca09-5302-400a-aea8-2a8be6c12b06}
                 * SQL Server 的 NEWID()產生的格式 c05d1709-0361-4304-8b2c-58fadcc4ae08 和C# D參數產生的一致。
                 */
                // var uuid = Guid.NewGuid().ToString();

                //報告錯誤
                replyData.code = "-1";
                replyData.message = $"陳核失敗！參數SurveyId不能為空！";
                replyData.data = "";
                Log.Error("陳核失敗!" + "參數SurveyId不能為空！");
                return JsonConvert.SerializeObject(replyData);
            }
            var SurveyId = jo["SurveyId"].ToString();

            //獲取操作員資訊
            var key = User.Identity.Name;
            if (key == null)
            {
                //報告錯誤
                replyData.code = "-1";
                replyData.message = $"用戶不存在！";
                replyData.data = "";
                Log.Error("發送OTP失敗!" + "用戶不存在！");
                return JsonConvert.SerializeObject(replyData);
            }
            var info = Utility.Common.GetConnectionInfo(key);
            if (info == null)
            {
                //報告錯誤
                replyData.code = "-1";
                replyData.message = $"用戶不存在！";
                replyData.data = "";
                Log.Error("發送OTP失敗!" + "用戶不存在！");
                return JsonConvert.SerializeObject(replyData);
            }
            var UpdUserId = info.UserId;

            //UpdUserId 會有程式依據Token取得，所以,目前暫時寫成00000000-0000-0000-0000-000000000000
            //var UpdUserId = jo["UpdUserId"] == null ? "00000000-0000-0000-0000-000000000000" : jo["UpdUserId"].ToString();
            //var UpdUserId = "00000000-0000-0000-0000-000000000000";
            //UpdDateTime 為datetime2: yyyy-MM-dd HH:mm:ss.ffffffff
            //var UpdDateTime = DateTime.Now.ToString("yyyy/MM/dd HH:mm:ss.fffffff");
            string sSql = $"UPDATE QUE001_QuestionnaireBase " +
                $" SET Audit=1 , UpdUserId=@UpdUserId, UpdDateTime=SYSDATETIME() " +
                $" WHERE SurveyId=@SurveyId";

            //-------sql para----start
            SqlParameter[] sqlParams = new SqlParameter[] {
                    new SqlParameter("@UpdUserId", SqlDbType.UniqueIdentifier),
                    new SqlParameter("@SurveyId", SqlDbType.UniqueIdentifier),

                };
            sqlParams[0].Value = UpdUserId.ValidGuid();
            sqlParams[1].Value = SurveyId.ValidGuid();


            //-------sql para----end

            Log.Debug("收集回覆-陳核:" + sSql);
            try
            {
                int iR = _db.ExecuteSql(sSql,sqlParams);

                replyData.code = "200";
                replyData.message = $"陳核完成。";

                replyData.data = iR;
                //return Ok(iR);
            }
            catch (Exception ex)
            {
                replyData.code = "-1";
                replyData.message = $"陳核失敗！{ex.Message}.";
                replyData.data = "";
                Log.Error("陳核失敗!" + ex.Message);
            }
            //返回
            return JsonConvert.SerializeObject(replyData);

        }
        /// <summary>
        /// 收集回覆-儲存
        /// </summary>
        /// <param name="value"></param>
        /// <returns></returns>
        [Route("Update")]
        [HttpPut]
        public String Update([FromBody] Object value)
        {
            /* 輸入格式：
             * {
             *    "SurveyId":"",
             *    "ProvideType":0,
             *    "FinalUrl":"",
             *    "ReplyMaxNum":10000,
             *    "MultiProvideType":0,
             *    "ValidRegister":0
             * }
             */
            JObject jo = (JObject)JsonConvert.DeserializeObject(value.ToString());
            var replyData = new ReplyData();

            //獲取操作員資訊
            var key = User.Identity.Name;
            if (key == null)
            {
                //報告錯誤
                replyData.code = "-1";
                replyData.message = $"用戶不存在！";
                replyData.data = "";
                Log.Error("發送OTP失敗!" + "用戶不存在！");
                return JsonConvert.SerializeObject(replyData);
            }
            var info = Utility.Common.GetConnectionInfo(key);
            if (info == null)
            {
                //報告錯誤
                replyData.code = "-1";
                replyData.message = $"用戶不存在！";
                replyData.data = "";
                Log.Error("發送OTP失敗!" + "用戶不存在！");
                return JsonConvert.SerializeObject(replyData);
            }
            var UpdUserId = info.UserId;

            //SurveyId 必須有?
            if (jo["SurveyId"] == null)
            {
                //報告錯誤
                replyData.code = "-1";
                replyData.message = $"儲存失敗！參數SurveyId不能為空！";
                replyData.data = "";
                Log.Error("儲存失敗!" + "參數SurveyId不能為空！");
                return JsonConvert.SerializeObject(replyData);
            }
            var SurveyId = jo["SurveyId"].ToString();
            //SurveyId 必須有?
            if (jo["ProvideType"] == null)
            {
                //報告錯誤
                replyData.code = "-1";
                replyData.message = $"儲存失敗！參數ProvideType不能為空！";
                replyData.data = "";
                Log.Error("儲存失敗!" + "參數ProvideType不能為空！");
                return JsonConvert.SerializeObject(replyData);
            }
            var ProvideType = jo["ProvideType"].ToString();

            string sWhereCondition = $" WHERE SurveyId=@SurveyId AND ProvideType=@ProvideType ";
            string sSql = " UPDATE QUE009_QuestionnaireProvideType SET ";

            //-------sql para----start
            List<SqlParameter> sqlParams = new List<SqlParameter>();
            //-------sql para----end


            var obj1 = new SqlParameter("@SurveyId", SqlDbType.UniqueIdentifier);
            obj1.Value = SurveyId.ValidGuid();
            sqlParams.Add(obj1);


            obj1 = new SqlParameter("@ProvideType", SqlDbType.Int);
            obj1.Value = ProvideType.ValidInt();
            sqlParams.Add(obj1);

            if (jo["ReplyMaxNum"] != null)
            {
                var ReplyMaxNum = Convert.ToInt32(jo["ReplyMaxNum"]);
                sSql += $" ReplyMaxNum=@ReplyMaxNum,";

                var obj = new SqlParameter("@ReplyMaxNum", SqlDbType.Int);
                obj.Value = ReplyMaxNum.ValidInt();
                sqlParams.Add(obj);
            }
            if (jo["ValidRegister"] != null)
            {
                var ValidRegister = Convert.ToInt32(jo["ValidRegister"]);
                sSql += $" ValidRegister=@ValidRegister,";

                var obj = new SqlParameter("@ValidRegister", SqlDbType.Int);
                obj.Value = ValidRegister.ValidInt();
                sqlParams.Add(obj);
            }
            if (jo["MultiProvideType"] != null)
            {
                var MultiProvideType = Convert.ToInt32(jo["MultiProvideType"]);
                sSql += $" MultiProvideType=@MultiProvideType,";

                var obj = new SqlParameter("@MultiProvideType", SqlDbType.Int);
                obj.Value = MultiProvideType.ValidInt();
                sqlParams.Add(obj);

            }
            if (jo["FinalUrl"] != null)
            {
                var FinalUrl = jo["FinalUrl"].ToString();
                sSql += $" FinalUrl=@FinalUrl,";

                var obj = new SqlParameter("@FinalUrl", SqlDbType.VarChar);
                obj.Value = FinalUrl.Valid();
                sqlParams.Add(obj);

            }
            if (jo["FullEndFlag"] != null)
            {
                var FullEndFlag = jo["FullEndFlag"].ToString();
                sSql += $" FullEndFlag=@FullEndFlag,";

                var obj = new SqlParameter("@FullEndFlag", SqlDbType.Bit);
                obj.Value = FullEndFlag.ValidInt16Bit();
                sqlParams.Add(obj);
            }
            if (jo["TestUrl"] != null)
            {
                var TestUrl = jo["TestUrl"].ToString();
                sSql += $" TestUrl=@TestUrl,";

                var obj = new SqlParameter("@TestUrl", SqlDbType.VarChar);
                obj.Value = TestUrl.Valid();
                sqlParams.Add(obj);
            }
            if (jo["ValidField"] != null)
            {
                var ValidField = jo["ValidField"].ToString();
                sSql += $" ValidField=@ValidField,";

                var obj = new SqlParameter("@ValidField", SqlDbType.Int);
                obj.Value = ValidField.ValidInt();
                sqlParams.Add(obj);
            }

            //UpdUserId 會有程式依據Token取得，所以,目前暫時寫成00000000-0000-0000-0000-000000000000
            //var UpdUserId = "00000000-0000-0000-0000-000000000000";
            sSql += $" UpdUserId=@UpdUserId,";

            var Sobj = new SqlParameter("@UpdUserId", SqlDbType.UniqueIdentifier);
            Sobj.Value = UpdUserId.ValidGuid();
            sqlParams.Add(Sobj);

            //if (jo["UpdUserId"] != null)
            //{
            //    var UpdUserId = jo["UpdUserId"].ToString();
            //    sSql += $" UpdUserId=NEWID(),";
            //}
            //updatetime 為 datetime2  yyyy-MM-dd HH:mm:ss.fffffff
            //var UpdDateTime = DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss.fffffff");
            sSql += $" UpdDateTime=SYSDATETIME() ";

            sSql += sWhereCondition;

            Log.Debug("收集回覆-儲存:" + sSql);
            try
            {
                int iR = _db.ExecuteSql(sSql,sqlParams.ToArray());

                replyData.code = "200";
                replyData.message = $"儲存記錄完成。";
                try
                {
                    //執行成功後,需要將本筆資料帶回前端

                    //-------sql para----start
                    SqlParameter[] sql1Params = new SqlParameter[] {
                    new SqlParameter("@SurveyId", SqlDbType.UniqueIdentifier),
                    new SqlParameter("@ProvideType", SqlDbType.Int),

                    };
                    sql1Params[0].Value = SurveyId.ValidGuid();
                    sql1Params[1].Value = ProvideType.ValidInt();


                    //-------sql para----end

                    var result = ExecuteQuery($"SELECT * FROM QUE009_QuestionnaireProvideType WHERE SurveyId=@SurveyId AND ProvideType=@ProvideType",sql1Params);
                    if (result.Count == 1)
                        replyData.data = result[0];  //應該只有一筆
                    else
                        replyData.data = result;
                }
                catch (Exception ex)
                {
                    //新增成功，查詢失敗
                    Log.Debug("儲存記錄完成，再查詢時失敗," + ex.Message);
                    replyData.data = "";
                }
            }
            catch (Exception ex)
            {
                replyData.code = "-1";
                replyData.message = $"儲存記錄失敗！{ex.Message}.";
                replyData.data = "";
                Log.Error("儲存記錄失敗!" + ex.Message);
            }
            //返回
            return JsonConvert.SerializeObject(replyData);

        }
        /// <summary>
        /// 收集回覆-查詢
        /// </summary>
        /// <param name="value"></param>
        /// <returns></returns>
        [Route("Query")]
        [HttpGet]
        public string QueryBy(string SurveyId)
        {
            ReplyData replyData = new ReplyData();
            List<QUE009_QuestionnaireProvideType> lstDataInfo = new List<QUE009_QuestionnaireProvideType>();
            string sSql = "SELECT * FROM QUE009_QuestionnaireProvideType ";
            string sWhereCondition = " WHERE 1=1 ";

            //-------sql para----start
            List<SqlParameter> sqlParams = new List<SqlParameter>();
            //-------sql para----end

            if (!String.IsNullOrWhiteSpace(SurveyId))
            {
                sWhereCondition += $" AND SurveyId=@SurveyId ";

                var obj = new SqlParameter("@SurveyId", SqlDbType.UniqueIdentifier);
                obj.Value = SurveyId.ValidGuid();
                sqlParams.Add(obj);

            }

            sSql += sWhereCondition;
            Log.Debug("收集回覆-查詢:" + sSql);
            try
            {
                lstDataInfo = ExecuteQuery(sSql, sqlParams.ToArray());
                replyData.code = "200";
                replyData.message = $"查詢記錄完成。共{lstDataInfo.Count}筆。";
                replyData.data = lstDataInfo;// JsonConvert.SerializeObject(lstUserInfo);
            }
            catch (Exception ex)
            {
                replyData.code = "-1";
                replyData.message = $"查詢記錄失敗！{ex.Message}.";
                replyData.data = "";
                Log.Error("收集回覆-查詢失敗!" + ex.Message);
            }
            //返回
            return JsonConvert.SerializeObject(replyData);
        }
        public string Query(string value)
        {
            JObject jo = (JObject)JsonConvert.DeserializeObject(value.ToString());
            ReplyData replyData = new ReplyData();
            List<QUE009_QuestionnaireProvideType> lstDataInfo = new List<QUE009_QuestionnaireProvideType>();
            string sSql = "SELECT * FROM QUE009_QuestionnaireProvideType ";
            string sWhereCondition = " WHERE 1=1 ";

            //-------sql para----start
            List<SqlParameter> sqlParams = new List<SqlParameter>();
            //-------sql para----end

            if (jo["SurveyId"] != null)
            {
                var SurveyId = jo["SurveyId"].ToString();
                sWhereCondition += $" AND SurveyId=@SurveyId ";

                var obj = new SqlParameter("@SurveyId", SqlDbType.UniqueIdentifier);
                obj.Value = SurveyId.ValidGuid();
                sqlParams.Add(obj);

            }
            if (jo["ProvideType"] != null)
            {
                var ProvideType = jo["ProvideType"].ToString();
                sWhereCondition += $" AND ProvideType=@ProvideType ";

                var obj = new SqlParameter("@ProvideType", SqlDbType.Int);
                obj.Value = ProvideType.ValidInt();
                sqlParams.Add(obj);
            }
            if (jo["ReplyMaxNum"] != null)
            {
                var ReplyMaxNum = jo["ReplyMaxNum"].ToString();
                sWhereCondition += $" AND ReplyMaxNum=@ReplyMaxNum ";

                var obj = new SqlParameter("@ReplyMaxNum", SqlDbType.Int);
                obj.Value = ReplyMaxNum.ValidInt();
                sqlParams.Add(obj);

            }
            if (jo["ValidRegister"] != null)
            {
                var ValidRegister = jo["ValidRegister"].ToString();
                sWhereCondition += $" AND ValidRegister =@ValidRegister ";

                var obj = new SqlParameter("@ValidRegister", SqlDbType.VarChar);
                obj.Value = ValidRegister.Valid();
                sqlParams.Add(obj);

            }
            if (jo["MultiProvideType"] != null)
            {
                var MultiProvideType = jo["MultiProvideType"].ToString();
                sWhereCondition += $" AND MultiProvideType=@MultiProvideType ";

                var obj = new SqlParameter("@MultiProvideType", SqlDbType.Int);
                obj.Value = MultiProvideType.ValidInt();
                sqlParams.Add(obj);
            }
            if (jo["FinalUrl"] != null)
            {
                var FinalUrl = jo["FinalUrl"].ToString();
                sWhereCondition += $" AND FinalUrl=@FinalUrl ";

                var obj = new SqlParameter("@FinalUrl", SqlDbType.VarChar);
                obj.Value = FinalUrl.Valid();
                sqlParams.Add(obj);

            }
            if (jo["FullEndFlag"] != null)
            {
                var FullEndFlag = jo["FullEndFlag"].ToString();
                sWhereCondition += $" AND FullEndFlag=@FullEndFlag ";

                var obj = new SqlParameter("@FullEndFlag", SqlDbType.Bit);
                obj.Value = FullEndFlag.ValidInt16Bit();
                sqlParams.Add(obj);
            }
            if (jo["CreateUserId"] != null)
            {
                var CreateUserId = jo["CreateUserId"].ToString();
                sWhereCondition += $" AND CreateUserId=@CreateUserId ";

                var obj = new SqlParameter("@CreateUserId", SqlDbType.UniqueIdentifier);
                obj.Value = CreateUserId.ValidGuid();
                sqlParams.Add(obj);
            }
            if (jo["CreateDateTime"] != null)
            {
                var CreateDateTime = jo["CreateDateTime"].ToString();
                sWhereCondition += $" AND CreateDateTime=@CreateDateTime ";

                var obj = new SqlParameter("@CreateDateTime", SqlDbType.VarChar);
                obj.Value = CreateDateTime.Valid();
                sqlParams.Add(obj);
            }

            if (jo["UpdUserId"] != null)
            {
                var UpdUserId = jo["UpdUserId"].ToString();
                sWhereCondition += $" AND UpdUserId=@UpdUserId ";

                var obj = new SqlParameter("@UpdUserId", SqlDbType.UniqueIdentifier);
                obj.Value = UpdUserId.ValidGuid();
                sqlParams.Add(obj);
            }

            if (jo["UpdDateTime"] != null)
            {
                var UpdDateTime = jo["UpdDateTime"].ToString();
                sWhereCondition += $" AND UpdDateTime=@UpdDateTime ";

                var obj = new SqlParameter("@UpdDateTime", SqlDbType.VarChar);
                obj.Value = UpdDateTime.Valid();
                sqlParams.Add(obj);

            }
            sSql += sWhereCondition;
            Log.Debug("收集回覆-查詢:" + sSql);
            try
            {
                lstDataInfo = ExecuteQuery(sSql,sqlParams.ToArray());
                replyData.code = "200";
                replyData.message = $"查詢記錄完成。共{lstDataInfo.Count}筆。";
                replyData.data = lstDataInfo;// JsonConvert.SerializeObject(lstUserInfo);
            }
            catch (Exception ex)
            {
                replyData.code = "-1";
                replyData.message = $"查詢記錄失敗！{ex.Message}.";
                replyData.data = "";
                Log.Error("收集回覆-查詢失敗!" + ex.Message);
            }
            //返回
            return JsonConvert.SerializeObject(replyData);
        }
        /// <summary>
        /// 收集回覆-新增
        /// </summary>
        /// <param name="value"></param>
        /// <returns></returns>
        [Route("Insert")]
        [HttpPost]
        public String Insert([FromBody] Object value)
        {
            JObject jo = (JObject)JsonConvert.DeserializeObject(value.ToString());
            var replyData = new ReplyData();

            //獲取操作員資訊
            var key = User.Identity.Name;
            if (key == null)
            {
                //報告錯誤
                replyData.code = "-1";
                replyData.message = $"用戶不存在！";
                replyData.data = "";
                Log.Error("發送OTP失敗!" + "用戶不存在！");
                return JsonConvert.SerializeObject(replyData);
            }
            var info = Utility.Common.GetConnectionInfo(key);
            if (info == null)
            {
                //報告錯誤
                replyData.code = "-1";
                replyData.message = $"用戶不存在！";
                replyData.data = "";
                Log.Error("發送OTP失敗!" + "用戶不存在！");
                return JsonConvert.SerializeObject(replyData);
            }
            var UpdUserId = info.UserId;


            //PK 必須有?
            if (jo["SurveyId"]==null)
            {
                //報告錯誤
                replyData.code = "-1";
                replyData.message = $"收集回覆-新增失敗！參數SurveyId不能空白！";
                replyData.data = "";
                Log.Error("收集回覆-新增失敗！" + "參數SurveyId不能空白！");
                return JsonConvert.SerializeObject(replyData);
            }
            if (jo["ProvideType"] == null)
            {
                //報告錯誤
                replyData.code = "-1";
                replyData.message = $"收集回覆-新增失敗！參數ProvideType不能空白！";
                replyData.data = "";
                Log.Error("收集回覆-新增失敗！" + "參數ProvideType不能空白！");
                return JsonConvert.SerializeObject(replyData);
            }
            var SurveyId = jo["SurveyId"] == null ? "" : jo["SurveyId"].ToString();
            var ProvideType = jo["ProvideType"] == null ? "" : jo["ProvideType"].ToString();
            var ReplyMaxNum = jo["ReplyMaxNum"] == null ? 0 : jo["ReplyMaxNum"];
            var ValidRegister = jo["ValidRegister"] == null ? 0: jo["ValidRegister"];
            var MultiProvideType = jo["MultiProvideType"] == null ? 0 : jo["MultiProvideType"];
            var FinalUrl = jo["FinalUrl"] == null ? 0 : jo["FinalUrl"];
            var FullEndFlag = jo["FullEndFlag"] == null ? "0" : jo["FullEndFlag"].ToString();
            var TestUrl = jo["TestUrl"] == null ? "" : jo["TestUrl"].ToString();
            var ValidField = jo["ValidField"] == null ? 0 : jo["ValidField"];
            var CreateUserId = jo["CreateUserId"] == null ? "" : jo["CreateUserId"].ToString();
            var CreateDateTime = jo["CreateDateTime"] == null ? "" : jo["CreateDateTime"].ToString();
            //var UpdUserId = jo["UpdUserId"] == null ? "" : jo["UpdUserId"].ToString();
            var UpdDateTime = jo["UpdDateTime"] == null ? "" : jo["UpdDateTime"].ToString();
            //UpdUserId 會有程式依據Token取得，所以,目前暫時寫成00000000-0000-0000-0000-000000000000
            //var UpdUserId = "00000000-0000-0000-0000-000000000000";
            //var UpdUserId = " NEWID() ";// jo["UpdUserId"] == null ? "" : jo["UpdUserId"].ToString();
            //var UpdDateTime = "SYSDATETIME()";// DateTime.Now.ToString("yyyy /MM/dd HH:mm:ss");
            string sSql = $"INSERT INTO QUE009_QuestionnaireProvideType (" +
                " SurveyId, ProvideType, ReplyMaxNum, ValidRegister, MultiProvideType, "+
                " FinalUrl,  FullEndFlag, TestUrl, ValidField, " +
                " CreateUserId, CreateDateTime, UpdUserId, UpdDateTime ) VALUES (  " +
                $" @SurveyId, @ProvideType, @ReplyMaxNum, @ValidRegister, @MultiProvideType," +
                $" @FinalUrl,@FullEndFlag, @TestUrl, @ValidField, " +
                $" @UpdUserId,SYSDATETIME(),@UpdUserId,SYSDATETIME())";

            //-------sql para----start
            SqlParameter[] sql1Params = new SqlParameter[] {
                    new SqlParameter("@SurveyId", SqlDbType.UniqueIdentifier),
                    new SqlParameter("@ProvideType", SqlDbType.Int),
                    new SqlParameter("@ReplyMaxNum", SqlDbType.Int),
                    new SqlParameter("@ValidRegister", SqlDbType.Int),
                    new SqlParameter("@MultiProvideType", SqlDbType.Int),
                    new SqlParameter("@FinalUrl", SqlDbType.VarChar),
                    new SqlParameter("@FullEndFlag", SqlDbType.Bit),
                    new SqlParameter("@TestUrl", SqlDbType.VarChar),
                    new SqlParameter("@ValidField", SqlDbType.Int),
                    new SqlParameter("@UpdUserId", SqlDbType.UniqueIdentifier),

                    };
            sql1Params[0].Value = SurveyId.ValidGuid();
            sql1Params[1].Value = ProvideType.ValidInt();
            sql1Params[2].Value = ReplyMaxNum.ValidInt();
            sql1Params[3].Value = ValidRegister.ValidInt();
            sql1Params[4].Value = MultiProvideType.ValidInt();
            sql1Params[5].Value = FinalUrl.ValidStrOrDBNull();
            sql1Params[6].Value = FullEndFlag.ValidInt16Bit();
            sql1Params[7].Value = TestUrl.Valid();
            sql1Params[8].Value = ValidField.ValidInt();
            sql1Params[9].Value = UpdUserId.ValidGuid();





            //-------sql para----end

            Log.Debug("收集回覆-新增:" + sSql);
            try
            {
                int iR = _db.ExecuteSql(sSql,sql1Params);

                replyData.code = "200";
                replyData.message = $"新增記錄完成。";
                try
                {
                    //執行成功後,需要將本筆資料帶回前端

                    //-------sql para----start
                    SqlParameter[] sqlParams = new SqlParameter[] {
                    new SqlParameter("@SurveyId", SqlDbType.VarChar),
                    new SqlParameter("@ProvideType", SqlDbType.Int),


                    };
                    sqlParams[0].Value = SurveyId.ValidGuid();
                    sqlParams[1].Value = ProvideType.ValidInt();
                    //-------sql para----end

                    var result = ExecuteQuery($"SELECT * FROM QUE009_QuestionnaireProvideType WHERE SurveyId=@SurveyId AND ProvideType=@ProvideType",sqlParams);
                    if (result.Count == 1)
                        replyData.data = result[0];  //應該只有一筆
                    else
                        replyData.data = result;
                }
                catch (Exception ex)
                {
                    //新增成功，查詢失敗
                    Log.Debug("新增記錄完成，再查詢時失敗," + ex.Message);
                    replyData.data = "";
                }
            }
            catch (Exception ex)
            {
                replyData.code = "-1";
                replyData.message = $"新增記錄失敗！{ ex.Message}";
                replyData.data = "";
                Log.Error("新增記錄失敗!" + ex.Message);
            }
            //返回
            return JsonConvert.SerializeObject(replyData);

        }
        /// <summary>
        /// 收集回覆-刪除
        /// </summary>
        /// <param name="value"></param>
        /// <returns></returns>
        [Route("Delete")]
        [HttpDelete]
        public String Delete(String SurveyId, String ProvideType)
        {
            //JObject jo = (JObject)JsonConvert.DeserializeObject(value.ToString());
            var replyData = new ReplyData();
            if (SurveyId == null || ProvideType == null)
            {
                //報告錯誤
                replyData.code = "-1";
                replyData.message = $"刪除失敗！參數SurveyId、ProvideType未傳入！";
                replyData.data = "";
                Log.Error("收集回覆-刪除失敗!" + "參數SurveyId、ProvideType未傳入！");
                return JsonConvert.SerializeObject(replyData);
            }

            string sSql = string.Format("DELETE FROM QUE009_QuestionnaireProvideType WHERE SurveyId=@SurveyId AND ProvideType=@ProvideType ");
            //-------sql para----start
            SqlParameter[] sqlParams = new SqlParameter[] {
                    new SqlParameter("@SurveyId", SqlDbType.UniqueIdentifier),
                    new SqlParameter("@ProvideType", SqlDbType.Int),


                    };

            sqlParams[0].Value = SurveyId.ValidGuid();
            sqlParams[1].Value = ProvideType.ValidInt();

            //-------sql para----end
            try
            {
                int iR = _db.ExecuteSql(sSql,sqlParams);
                replyData.code = "000";
                replyData.message = $"刪除記錄完成。";
                replyData.data = iR;
                Log.Debug("收集回覆-刪除記錄完成。共刪除{iR}筆。");
                //return Ok(iR);
            }
            catch (Exception ex)
            {
                replyData.code = "-1";
                replyData.message = $"刪除記錄失敗！{ex.Message}.";
                replyData.data = "";
                Log.Error("收集回覆-刪除失敗!" + ex.Message);
            }
            //return Ok(replyData);
            return JsonConvert.SerializeObject(replyData);
        }
        /// <summary>
        /// 行銷名單
        /// </summary>
        /// <param name="SurveyId"></param>
        /// <returns></returns>
        [Route("ContactList")]
        [HttpGet]
        public string ContactList(string SurveyId)
        {
            ReplyData replyData = new ReplyData();
            if (String.IsNullOrWhiteSpace(SurveyId))
            {
                //報告錯誤
                replyData.code = "-1";
                replyData.message = $"參數SurveyId不能為空！";
                Log.Error("收集回覆-行銷名單，" + "參數SurveyId不能為空！");
                return JsonConvert.SerializeObject(replyData);
            }

            ContactListResult contactList = new ContactListResult();
            string sSql = " SELECT A.ActivityId, C.ListName  " +
                " FROM "+
                " CampaignActivityBase A, "+
                " CampaignActivityItemBase B, "+
                " ListBase C " +
                $" WHERE  A.ActivityId = @SurveyId " +
                " AND A.ActivityId = B.CampaignActivityId "+
                " AND B.ItemId = C.ListId ";

            //-------sql para----start
            SqlParameter[] sqlParams = new SqlParameter[] {
                    new SqlParameter("@SurveyId", SqlDbType.UniqueIdentifier),



                    };
            sqlParams[0].Value = SurveyId.ValidGuid();

            //-------sql para----end

            Log.Debug("收集回覆-行銷名單:" + sSql);
            try
            {
                //來源為CRM，回傳資料先寫死讓前端測試
                //lstDataInfo = ExecuteQuery(sSql);
                List<string> lstName = new List<string>();
                //lstName.Add("名單1");
                //lstName.Add("名單2");
                //contactList.List = lstName;
                //contactList.Count = lstName.Count();

                DataTable dtR;
                if(AppSettingsHelper.EnvSwitchToCRM.SwitchToCRM)
                {
                    dtR = CRMDbApiHelper.SurveyQuesionCollectionWay_ContactList(SurveyId);
                }
                else
                {
                    dtR = _crmDB.GetQueryData(sSql, sqlParams);
                }

                foreach (DataRow dr in dtR.Rows)
                {
                    lstName.Add(dr["ListName"].ToString().Trim());
                }
                contactList.List = lstName;
                contactList.Count = lstName.Count();

                replyData.code = "200";
                replyData.message = $"查詢記錄完成。共{contactList.Count}筆。";
                replyData.data = contactList;   // JsonConvert.SerializeObject(lstUserInfo);
            }
            catch (Exception ex)
            {
                replyData.code = "-1";
                replyData.message = $"查詢記錄失敗！{ex.Message}.";
                replyData.data = "";
                Log.Error("收集回覆-行銷名單-查詢失敗!" + ex.Message);
            }
            //返回
            return JsonConvert.SerializeObject(replyData);
        }

        private List<QUE009_QuestionnaireProvideType> ExecuteQuery(String sSql)
        {
            List<QUE009_QuestionnaireProvideType> lstDataInfo = new List<QUE009_QuestionnaireProvideType>();
            try
            {
                DataTable dtR = _db.GetQueryData(sSql);
                foreach (DataRow dr in dtR.Rows)
                {
                    QUE009_QuestionnaireProvideType datainfo = new QUE009_QuestionnaireProvideType();
                    datainfo.SurveyId = dr["SurveyId"];
                    datainfo.ProvideType = dr["ProvideType"];
                    datainfo.FinalUrl = dr["FinalUrl"];
                    datainfo.ReplyMaxNum = dr["ReplyMaxNum"];
                    datainfo.MultiProvideType = dr["MultiProvideType"];
                    datainfo.ValidRegister = dr["ValidRegister"];
                    datainfo.FullEndFlag = dr["FullEndFlag"];
                    datainfo.CreateUserId = dr["CreateUserId"];
                    datainfo.CreateDateTime = dr["CreateDateTime"];
                    datainfo.UpdUserId = dr["UpdUserId"];
                    datainfo.UpdDateTime = dr["UpdDateTime"];
                    datainfo.TestUrl = dr["TestUrl"];
                    datainfo.ValidField = dr["ValidField"];
                    //填寫次數ReplyNum：依據SurveyId+ProvideType count（1）from QUE021
                    datainfo.ReplyNum = GetReplyNumBy(datainfo.SurveyId.ToString(), Convert.ToInt32(datainfo.ProvideType));
                    lstDataInfo.Add(datainfo);
                }
            }
            catch (Exception)
            {
                throw;
            }
            return lstDataInfo;
        }


        private List<QUE009_QuestionnaireProvideType> ExecuteQuery(String sSql, SqlParameter[] cmdParams)
        {
            List<QUE009_QuestionnaireProvideType> lstDataInfo = new List<QUE009_QuestionnaireProvideType>();
            try
            {
                DataTable dtR = _db.GetQueryData(sSql, cmdParams);
                foreach (DataRow dr in dtR.Rows)
                {
                    QUE009_QuestionnaireProvideType datainfo = new QUE009_QuestionnaireProvideType();
                    datainfo.SurveyId = dr["SurveyId"];
                    datainfo.ProvideType = dr["ProvideType"];
                    datainfo.FinalUrl = dr["FinalUrl"];
                    datainfo.ReplyMaxNum = dr["ReplyMaxNum"];
                    datainfo.MultiProvideType = dr["MultiProvideType"];
                    datainfo.ValidRegister = dr["ValidRegister"];
                    datainfo.FullEndFlag = dr["FullEndFlag"];
                    datainfo.CreateUserId = dr["CreateUserId"];
                    datainfo.CreateDateTime = dr["CreateDateTime"];
                    datainfo.UpdUserId = dr["UpdUserId"];
                    datainfo.UpdDateTime = dr["UpdDateTime"];
                    datainfo.TestUrl = dr["TestUrl"];
                    datainfo.ValidField = dr["ValidField"];
                    //填寫次數ReplyNum：依據SurveyId+ProvideType count（1）from QUE021
                    datainfo.ReplyNum = GetReplyNumBy(datainfo.SurveyId.ToString(), Convert.ToInt32(datainfo.ProvideType));
                    lstDataInfo.Add(datainfo);
                }
            }
            catch (Exception)
            {
                throw;
            }
            return lstDataInfo;
        }


        private int GetReplyNumBy(String SurveyId, int ProvideType)
        {
            var sSql = $" SELECT COUNT(1) FROM QUE021_AnwserCollection WHERE SurveyId=@SurveyId AND ProvideType=@ProvideType " +
                       $" AND Env = 2 AND (DelFlag IS NULL OR DelFlag<>1) "; // 20201021_填寫次數只計算正式環境Env=2 & 在QUE021取得資料時，都要加上判斷DelFlag的條件。



            //-------sql para----start
            SqlParameter[] sqlParams = new SqlParameter[] {
                    new SqlParameter("@SurveyId", SqlDbType.UniqueIdentifier),
                    new SqlParameter("@ProvideType", SqlDbType.Int),


                    };

            sqlParams[0].Value = SurveyId.ValidGuid();
            sqlParams[1].Value = ProvideType.ValidInt();

            //-------sql para----end



            try
            {
                var result = _db.GetSingle(sSql, sqlParams);
                if (String.IsNullOrWhiteSpace(result))
                    return  0;
                else
                    return  Convert.ToInt32(result);
            }
            catch (Exception ex)
            {
                Log.Error(ex.Message);
                Log.Error(ex.StackTrace);
                throw;
            }
        }

        [Route("Reject")]
        [HttpPut]
        public string Reject([FromBody] Object value)
        {
            JObject jo = (JObject)JsonConvert.DeserializeObject(value.ToString());
            var replyData = new ReplyData();

            //SurveyId 必須有?
            if (jo["SurveyId"] == null)
            {
                //報告錯誤
                replyData.code = "-1";
                replyData.message = $"參數SurveyId不能為空！";
                replyData.data = "";
                Log.Error("參數SurveyId不能為空！");
                return JsonConvert.SerializeObject(replyData);
            }
            var SurveyId = jo["SurveyId"].ToString();

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
            string sSql = string.Format("UPDATE QUE001_QuestionnaireBase SET Audit='0',UpdUserId=@userId,UpdDateTime=SYSDATETIME() where SurveyId=@SurveyId" );

            //-------sql para----start
            SqlParameter[] sqlParams = new SqlParameter[] {
                    new SqlParameter("@SurveyId", SqlDbType.UniqueIdentifier),
                    new SqlParameter("@userId", SqlDbType.UniqueIdentifier),



                    };
            sqlParams[0].Value = SurveyId.ValidGuid();
            sqlParams[1].Value = userId.ValidGuid();

            //-------sql para----end

            Log.Debug("收集回覆-退回:" + sSql);
            try
            {
                int iR = _db.ExecuteSql(sSql, sqlParams);

                replyData.code = "200";
                replyData.message = $"退回記錄完成。";
                replyData.data = iR;
            }
            catch (Exception ex)
            {
                replyData.code = "-1";
                replyData.message = $"退回記錄失敗！{ex.Message}.";
                replyData.data = "";
                Log.Error("退回記錄失敗!" + ex.Message);
            }
            //返回
            return JsonConvert.SerializeObject(replyData);
        }

    }
    public class ContactListResult
    {
        /// <summary>
        /// 名單數量
        /// </summary>
        public Object Count { get; set; }
        /// <summary>
        /// 名單列表
        /// </summary>
        public List<String> List { get; set; }
    }
}
