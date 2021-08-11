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
using Microsoft.AspNetCore.Authorization;
using System.Data.SqlClient;

namespace SurveyWebAPI.Controllers
{
    [Authorize]
    [Route("api/Survey/BaseConfig")]
    [ApiController]
    public class SurveyBaseConfigController : Controller
    {
        /// <summary>
        /// 問卷系統後臺之基本設定API
        /// </summary>
        /// <designer>Allen Fan/Gem He</design>
        /// <History> 1. 2020/08/04 Gem Create
        /// </History>


        //private readonly ILogger<QUE004_QuestionnaireSettingController> _logger;  //可實現log在console輸出
        private DBHelper _db;
        //publicQUE004_QuestionnaireSettingController(ILogger<QUE004_QuestionnaireSettingController> logger)
        public SurveyBaseConfigController()
        {
            //_logger = logger;
            //_db = new DBHelper();
            //_logger = logger;
            //_connectionOptions = options.Value;
            _db = new DBHelper(AppSettingsHelper.DefaultConnectionString);
        }
        /// <summary>
        /// 查詢所有基本設定資料
        /// </summary>
        /// <returns></returns>
        //[Route("query")]
        //[HttpGet]
        //public IEnumerable<QUE004_QuestionnaireSetting> QueryAll()
        private String QueryAll()
        {
            Log.Debug("查詢基本設定...");

            List<QUE004_QuestionnaireSetting> lstBasicSetting = new List<QUE004_QuestionnaireSetting>();
            ReplyData replyData = new ReplyData();
            string sSql = "SELECT * FROM QUE004_QuestionnaireSetting ";
            try
            {
                lstBasicSetting = ExecuteQuery(sSql);
                replyData.code = "200";
                replyData.message = $"查詢記錄完成。共{lstBasicSetting.Count}筆。";
                Log.Debug($"查詢記錄完成。共{lstBasicSetting.Count}筆。");
                //先不要SerializeObject list 應該也可以
                replyData.data = lstBasicSetting;  // JsonConvert.SerializeObject(lstBaseicSetting);
            }
            catch (Exception ex)
            {
                replyData.code = "-1";
                replyData.message = $"查詢基本設定記錄失敗！{ex.Message}.";
                replyData.data = "";
                Log.Error("查詢基本設定記錄失敗!" + ex.Message);
            }

            //返回結果
            return JsonConvert.SerializeObject(replyData);
            //return lstUserInfo.ToArray();
        }
        [Route("query")]
        [HttpGet]
        public string QueryBy(string SurveyId)
        {
            if (String.IsNullOrWhiteSpace(SurveyId))
            {
                return QueryAll();
            }

            ReplyData replyData = new ReplyData();
            List<QUE004_QuestionnaireSetting> lstBasicSetting = new List<QUE004_QuestionnaireSetting>();
            string sSql = "SELECT * FROM QUE004_QuestionnaireSetting ";
            string sWhereCondition = " WHERE 1=1 ";

            sWhereCondition += $" AND SurveyId=@SurveyId ";


            sSql += sWhereCondition;


            //-------sql para----start
            SqlParameter[] sqlParams = new SqlParameter[] {
                    new SqlParameter("@SurveyId", SqlDbType.UniqueIdentifier),

            };
            sqlParams[0].Value = SurveyId.ValidGuid();

            //-------sql para----end

            Log.Debug("Query QUE004_QuestionnaireSetting  by condition:" + sSql);
            try
            {
                lstBasicSetting = ExecuteQuery(sSql,sqlParams);

                replyData.code = "200";
                replyData.message = $"查詢記錄完成。共{lstBasicSetting.Count}筆。";
                //data 直接給list 先不要序列化成string
                //replyData.data = JsonConvert.SerializeObject(lstBaseicSetting);
                if (lstBasicSetting.Count == 1)
                    replyData.data = lstBasicSetting[0];
                else
                    replyData.data = lstBasicSetting;
            }
            catch (Exception ex)
            {
                replyData.code = "-1";
                replyData.message = $"查詢記錄失敗！{ex.Message}.";
                replyData.data = "";
                Log.Error("查詢基本設定記錄失敗!" + ex.Message);
            }
            //返回
            return JsonConvert.SerializeObject(replyData);
        }

        /// <summary>
        /// 依帶入參數條件查詢資料(預留看後續是否能用到)
        /// </summary>
        /// <param name="value"></param>
        /// <returns></returns>
        [Route("queryBy")]
        [HttpGet]
        public string QueryByCondition([FromBody] Object value)
        {
            JObject jo = (JObject)JsonConvert.DeserializeObject(value.ToString());
            ReplyData replyData = new ReplyData();
            List<QUE004_QuestionnaireSetting> lstBasicSetting = new List<QUE004_QuestionnaireSetting>();
            string sSql = "SELECT * FROM QUE004_QuestionnaireSetting ";
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

            if (jo["IsShowPageNo"] != null)
            {
                var IsShowPageNo = jo["IsShowPageNo"].ToString();
                sWhereCondition += $" AND IsShowPageNo =@IsShowPageNo ";

                var obj = new SqlParameter("@IsShowPageNo", SqlDbType.Char);
                obj.Value = IsShowPageNo.Valid();
                sqlParams.Add(obj);


            }
            if (jo["IsShowQuestionNo"] != null)
            {
                var IsShowQuestionNo = jo["IsShowQuestionNo"].ToString();
                sWhereCondition += $" AND IsShowQuestionNo = @IsShowQuestionNo ";

                var obj = new SqlParameter("@IsShowQuestionNo", SqlDbType.Char);
                obj.Value = IsShowQuestionNo.Valid();
                sqlParams.Add(obj);
            }
            if (jo["IsShowRequiredStar"] != null)
            {
                var IsShowRequiredStar = jo["IsShowRequiredStar"].ToString();
                sWhereCondition += $" AND IsShowRequiredStar=@IsShowRequiredStar ";

                var obj = new SqlParameter("@IsShowRequiredStar", SqlDbType.Char);
                obj.Value = IsShowRequiredStar.Valid();
                sqlParams.Add(obj);
            }
            if (jo["IsShowProgress"] != null)
            {
                var IsShowProgress = jo["IsShowProgress"].ToString();
                sWhereCondition += $" AND IsShowProgress=@IsShowProgress ";

                var obj = new SqlParameter("@IsShowProgress", SqlDbType.Char);
                obj.Value = IsShowProgress.Valid();
                sqlParams.Add(obj);
            }
            if (jo["PorgressPosition"] != null)
            {
                var PorgressPosition = jo["PorgressPosition"].ToString();
                sWhereCondition += $" AND PorgressPosition=@PorgressPosition ";

                var obj = new SqlParameter("@PorgressPosition", SqlDbType.Int);
                obj.Value = PorgressPosition.ValidInt();
                sqlParams.Add(obj);

            }
            if (jo["ProgressStyle"] != null)
            {
                var ProgressStyle = jo["ProgressStyle"].ToString();
                sWhereCondition += $" AND StarProgressStyle=@ProgressStyle ";

                var obj = new SqlParameter("@ProgressStyle", SqlDbType.Int);
                obj.Value = ProgressStyle.ValidInt();
                sqlParams.Add(obj);

            }
            if (jo["UseVirifyCode"] != null)
            {
                var UseVirifyCode = jo["UseVirifyCode"].ToString();
                sWhereCondition += $" AND UseVirifyCode=@UseVirifyCode ";

                var obj = new SqlParameter("@UseVirifyCode", SqlDbType.Char);
                obj.Value = UseVirifyCode.Valid();
                sqlParams.Add(obj);

            }
            if (jo["IsOneQuestionPerPage"] != null)
            {
                var IsOneQuestionPerPage = jo["IsOneQuestionPerPage"].ToString();
                sWhereCondition += $" AND IsOneQuestionPerPage=@IsOneQuestionPerPage ";

                var obj = new SqlParameter("@IsOneQuestionPerPage", SqlDbType.Char);
                obj.Value = IsOneQuestionPerPage.Valid();
                sqlParams.Add(obj);
            }
            if (jo["IsPublishResult"] != null)
            {
                var IsPublishResult = jo["IsPublishResult"].ToString();
                sWhereCondition += $" AND IsPublishResult=@IsPublishResult ";

                var obj = new SqlParameter("@IsPublishResult", SqlDbType.Char);
                obj.Value = IsPublishResult.Valid();
                sqlParams.Add(obj);

            }
            if (jo["IsShowEndPage"] != null)
            {
                var IsShowEndPage = jo["IsShowEndPage"].ToString();
                sWhereCondition += $" AND IsShowEndPage=@IsShowEndPage ";

                var obj = new SqlParameter("@IsShowEndPage", SqlDbType.Char);
                obj.Value = IsShowEndPage.Valid();
                sqlParams.Add(obj);

            }
            if (jo["IsShowOptionNo"] != null)
            {
                var IsShowOptionNo = jo["IsShowOptionNo"].ToString();
                sWhereCondition += $" AND IsShowOptionNo=@IsShowOptionNo ";

                var obj = new SqlParameter("@IsShowOptionNo", SqlDbType.Char);
                obj.Value = IsShowOptionNo.Valid();
                sqlParams.Add(obj);
            }



            //-------sql para----start



            // if (jo["UpdUserId"] != null)
            //{
            //    var UpdUserId = jo["UpdUserId"].ToString();
            //    sWhereCondition += $" AND UpdUserId='{UpdUserId}' ";
            //}

            //if (jo["UpdDateTime"] != null)
            //{
            //    var UpdDateTime = jo["UpdDateTime"].ToString();
            //    sWhereCondition += $" AND UpdDateTime='{UpdDateTime}' ";
            //}
            sSql += sWhereCondition;
            Log.Debug("Query QUE004_QuestionnaireSetting  by condition:" + sSql);
            try
            {
                lstBasicSetting = ExecuteQuery(sSql, sqlParams);

                replyData.code = "200";
                replyData.message = $"查詢記錄完成。共{lstBasicSetting.Count}筆。";
                //data 直接給list 先不要序列化成string
                //replyData.data = JsonConvert.SerializeObject(lstBaseicSetting);
                replyData.data = lstBasicSetting;
            }
            catch (Exception ex)
            {
                replyData.code = "-1";
                replyData.message = $"查詢記錄失敗！{ex.Message}.";
                replyData.data = "";
                Log.Error("查詢基本設定記錄失敗!" + ex.Message);
            }
            //返回
            return JsonConvert.SerializeObject(replyData);
        }
        /// <summary>
        /// 基本設定--新增
        /// </summary>
        /// <param name="value"></param>
        /// <returns></returns>
        [Route("Insert")]
        [HttpPost]
        public String Insert([FromBody] Object value)
        {
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
            JObject jo = (JObject)JsonConvert.DeserializeObject(value.ToString());
            var replyData = new ReplyData();
            //UserId 必須有?
            if (jo["SurveyId"] == null || String.IsNullOrWhiteSpace(jo["SurveyId"].ToString()))
            {
                //據Allen講, DB中所有Id欄位,都是GUID產生, 那這是client 產好傳過來還是後台產生????????
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
                replyData.message = $"新增資料失敗！參數SurveyId不能為空！";
                replyData.data = "";
                Log.Error("新增基本設定記錄失敗!" + "參數SurveyId不能為空！");
                return JsonConvert.SerializeObject(replyData);
            }
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

            var SurveyId = jo["SurveyId"].ToString();
            var IsShowPageNo = jo["IsShowPageNo"] == null ? "0" : jo["IsShowPageNo"].ToString();
            var IsShowQuestionNo = jo["IsShowQuestionNo"] == null ? "0" : jo["IsShowQuestionNo"].ToString();
            var IsShowRequiredStar = jo["IsShowRequiredStar"] == null ? "1" : jo["IsShowRequiredStar"].ToString();
            var IsShowProgress = jo["IsShowProgress"] == null ? "0" : jo["IsShowProgress"].ToString();
            var PorgressPosition = jo["PorgressPosition"] == null ? 0 : Convert.ToInt32(jo["PorgressPosition"]);
            var ProgressStyle = jo["ProgressStyle"] == null ? 0 : Convert.ToInt32(jo["ProgressStyle"]);
            var UseVirifyCode = jo["UseVirifyCode"] == null ? "0" : jo["UseVirifyCode"].ToString();
            var IsOneQuestionPerPage = jo["IsOneQuestionPerPage"] == null ? "0" : jo["IsOneQuestionPerPage"].ToString();
            var IsPublishResult = jo["IsPublishResult"] == null ? "0" : jo["IsPublishResult"].ToString();
            var IsShowEndPage = jo["IsShowEndPage"] == null ? "1" : jo["IsShowEndPage"].ToString();
            var IsShowOptionNo = jo["IsShowOptionNo"] == null ? "0" : jo["IsShowOptionNo"].ToString();
            //UpdUserId 會有程式依據Token取得，所以,目前暫時寫成00000000-0000-0000-0000-000000000000
            //var UpdUserId = jo["UpdUserId"] == null ? "00000000-0000-0000-0000-000000000000" : jo["UpdUserId"].ToString();
            //var UpdUserId = "00000000-0000-0000-0000-000000000000";
            //var UpdDateTime = DateTime.Now.ToString("yyyy/MM/dd HH:mm:ss.fffffff");
            string sSql = $"INSERT INTO QUE004_QuestionnaireSetting (" +
            " SurveyId, IsShowPageNo, IsShowQuestionNo, IsShowRequiredStar, IsShowProgress," +
            " PorgressPosition,  ProgressStyle, UseVirifyCode, IsOneQuestionPerPage, IsPublishResult," +
            " IsShowEndPage,  IsShowOptionNo, UpdUserId, UpdDateTime ) VALUES (  " +
            $" @SurveyId, @IsShowPageNo, @IsShowQuestionNo,@IsShowRequiredStar,@IsShowProgress," +
            $" @PorgressPosition, @ProgressStyle,@UseVirifyCode, @IsOneQuestionPerPage, @IsPublishResult," +
            $" @IsShowEndPage,@IsShowOptionNo, @UpdUserId, " + " SYSDATETIME())";  //UpdDateTime 為datetime2: yyyy-MM-dd HH:mm:ss.ffffffff


            //-------sql para----start
            SqlParameter[] sqlParams = new SqlParameter[] {
                    new SqlParameter("@SurveyId", SqlDbType.UniqueIdentifier),
                    new SqlParameter("@IsShowPageNo", SqlDbType.Char),
                    new SqlParameter("@IsShowQuestionNo", SqlDbType.Char),
                    new SqlParameter("@IsShowRequiredStar", SqlDbType.Char),
                    new SqlParameter("@IsShowProgress", SqlDbType.Char),
                    new SqlParameter("@PorgressPosition", SqlDbType.Int),
                    new SqlParameter("@ProgressStyle", SqlDbType.Int),
                    new SqlParameter("@UseVirifyCode", SqlDbType.Char),
                    new SqlParameter("@IsOneQuestionPerPage", SqlDbType.Char),
                    new SqlParameter("@IsPublishResult", SqlDbType.Char),
                    new SqlParameter("@IsShowEndPage", SqlDbType.Char),
                    new SqlParameter("@IsShowOptionNo", SqlDbType.Char),
                    new SqlParameter("@UpdUserId", SqlDbType.UniqueIdentifier),

            };
            sqlParams[0].Value = SurveyId.ValidGuid();
            sqlParams[1].Value = IsShowPageNo.Valid();
            sqlParams[2].Value = IsShowQuestionNo.Valid();
            sqlParams[3].Value = IsShowRequiredStar.Valid();
            sqlParams[4].Value = IsShowProgress.Valid();
            sqlParams[5].Value = PorgressPosition.ValidInt();
            sqlParams[6].Value = ProgressStyle.ValidInt();
            sqlParams[7].Value = UseVirifyCode.Valid();
            sqlParams[8].Value = IsOneQuestionPerPage.Valid();
            sqlParams[9].Value = IsPublishResult.Valid();
            sqlParams[10].Value = IsShowEndPage.Valid();
            sqlParams[11].Value = IsShowOptionNo.Valid();
            sqlParams[12].Value = UpdUserId.ValidGuid();

            //-------sql para----end

            Log.Debug("新增QUE004_QuestionnaireSetting:" + sSql);
            try
            {
                int iR = _db.ExecuteSql(sSql,sqlParams);

                replyData.code = "200";
                replyData.message = $"新增記錄完成。";
                try
                {
                    //執行成功後,需要將本筆資料帶回前端

                    //-------sql para----start
                    SqlParameter[] sqlSParams = new SqlParameter[] {
                    new SqlParameter("@SurveyId", SqlDbType.UniqueIdentifier),

                    };
                    sqlParams[0].Value = SurveyId.ValidGuid();
                    //-------sql para----end


                    var result = ExecuteQuery($"SELECT * FROM QUE004_QuestionnaireSetting WHERE SurveyId=@SurveyId ",sqlSParams);
                    if (result.Count == 1)
                        replyData.data = result[0];  //應該只有一筆
                    else
                        replyData.data = result;
                }
                catch (Exception)
                {
                    //新增成功，查詢失敗
                    replyData.data = "";
                }
                //replyData.data = ExecuteQuery($"SELECT * FROM QUE004_QuestionnaireSetting WHERE SurveyId='{SurveyId}' ");
                //return Ok(iR);
            }
            catch (Exception ex)
            {
                replyData.code = "-1";
                replyData.message = $"新增資料失敗！{ex.Message}.";
                replyData.data = "";
                Log.Error("新增基本設定記錄失敗!" + ex.Message);
            }
            //返回
            return JsonConvert.SerializeObject(replyData);

        }
        /// <summary>
        /// 預留
        /// </summary>
        /// <param name="value"></param>
        /// <returns></returns>
        private List<QUE004_QuestionnaireSetting>InsertOne(object value)
        {
            JObject jo = (JObject)JsonConvert.DeserializeObject(value.ToString());
            //var replyData = new ReplyData();
            //UserId 必須有?
            if (jo["SurveyId"] == null)
            {
                //據Allen講, DB中所有Id欄位,都是GUID產生, 那這是client 產好傳過來還是後台產生????????
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
                //replyData.code = "-1";
                //replyData.message = $"新增資料失敗！參數SurveyId不能為空！";
                //replyData.data = "";
                //Log.Error("新增基本設定記錄失敗!" + "參數SurveyId不能為空！");
                throw(new Exception("新增資料失敗！參數SurveyId不能為空！"));
            }
            var SurveyId = jo["SurveyId"].ToString();
            var IsShowPageNo = jo["IsShowPageNo"] == null ? "0" : jo["IsShowPageNo"].ToString();
            var IsShowQuestionNo = jo["IsShowQuestionNo"] == null ? "0" : jo["IsShowQuestionNo"].ToString();
            var IsShowRequiredStar = jo["IsShowRequiredStar"] == null ? "1" : jo["IsShowRequiredStar"].ToString();
            var IsShowProgress = jo["IsShowProgress"] == null ? "0" : jo["IsShowProgress"].ToString();
            var PorgressPosition = jo["PorgressPosition"] == null ? 0 : Convert.ToInt32(jo["PorgressPosition"]);
            var ProgressStyle = jo["ProgressStyle"] == null ? 0 : Convert.ToInt32(jo["ProgressStyle"]);
            var UseVirifyCode = jo["UseVirifyCode"] == null ? "0" : jo["UseVirifyCode"].ToString();
            var IsOneQuestionPerPage = jo["IsOneQuestionPerPage"] == null ? 0 : Convert.ToInt32(jo["IsOneQuestionPerPage"]);
            var IsPublishResult = jo["IsPublishResult"] == null ? "0" : jo["IsPublishResult"].ToString();
            var IsShowEndPage = jo["IsShowEndPage"] == null ? "1" : jo["IsShowEndPage"].ToString();
            var IsShowOptionNo = jo["IsShowOptionNo"] == null ? "0" : jo["IsShowOptionNo"].ToString();
            //UpdUserId需要NEWID自己產生
            //var UpdUserId = jo["UpdUserId"] == null ? "API" : jo["UpdUserId"].ToString();
            //var UpdDateTime = DateTime.Now.ToString("yyyy/MM/dd HH:mm:ss.fffffff");
            string sSql = $"INSERT INTO QUE004_QuestionnaireSetting (" +
            " SurveyId, IsShowPageNo, IsShowQuestionNo, IsShowRequiredStar, IsShowProgress," +
            " PorgressPosition,  ProgressStyle, UseVirifyCode, IsOneQuestionPerPage, IsPublishResult," +
            " IsShowEndPage, IsShowOptionNo, UpdUserId, UpdDateTime ) VALUES (  " +
            $" @SurveyId, @IsShowPageNo, @IsShowQuestionNo ,@IsShowRequiredStar,@IsShowProgress," +
            $" @PorgressPosition, @ProgressStyle,@UseVirifyCode, @IsOneQuestionPerPage, @IsPublishResult," +
            $" @IsShowEndPage, @IsShowOptionNo, " + " NEWID()," + " SYSDATETIME())";  //UpdDateTime 為datetime2: yyyy-MM-dd HH:mm:ss.ffffffff


            //-------sql para----start
            SqlParameter[] sqlParams = new SqlParameter[] {
                    new SqlParameter("@SurveyId", SqlDbType.UniqueIdentifier),
                    new SqlParameter("@IsShowPageNo", SqlDbType.Char),
                    new SqlParameter("@IsShowQuestionNo", SqlDbType.Char),
                    new SqlParameter("@IsShowRequiredStar", SqlDbType.Char),
                    new SqlParameter("@IsShowProgress", SqlDbType.Char),
                    new SqlParameter("@PorgressPosition", SqlDbType.Int),
                    new SqlParameter("@ProgressStyle", SqlDbType.Int),
                    new SqlParameter("@UseVirifyCode", SqlDbType.Char),
                    new SqlParameter("@IsOneQuestionPerPage", SqlDbType.Char),
                    new SqlParameter("@IsPublishResult", SqlDbType.Char),
                    new SqlParameter("@IsShowEndPage", SqlDbType.Char),
                    new SqlParameter("@IsShowOptionNo", SqlDbType.Char),


            };
            sqlParams[0].Value = SurveyId.ValidGuid();
            sqlParams[1].Value = IsShowPageNo.Valid();
            sqlParams[2].Value = IsShowQuestionNo.Valid();
            sqlParams[3].Value = IsShowRequiredStar.Valid();
            sqlParams[4].Value = IsShowProgress.Valid();
            sqlParams[5].Value = PorgressPosition.ValidInt();
            sqlParams[6].Value = ProgressStyle.ValidInt();
            sqlParams[7].Value = UseVirifyCode.Valid();
            sqlParams[8].Value = IsOneQuestionPerPage.ValidInt();
            sqlParams[9].Value = IsPublishResult.Valid();
            sqlParams[10].Value = IsShowEndPage.Valid();
            sqlParams[11].Value = IsShowOptionNo.Valid();


            //-------sql para----end


            Log.Debug("新增QUE004_QuestionnaireSetting:" + sSql);
            try
            {
                int iR = _db.ExecuteSql(sSql,sqlParams);

                //replyData.code = "200";
                //replyData.message = $"新增記錄完成。";
                try
                {
                    //執行成功後,需要將本筆資料帶回前端
                    //-------sql para----start
                    SqlParameter[] sqlSParams = new SqlParameter[] {
                    new SqlParameter("@SurveyId", SqlDbType.UniqueIdentifier),

                    };
                    sqlParams[0].Value = SurveyId.ValidGuid();
                    //-------sql para----end

                    return ExecuteQuery($"SELECT * FROM QUE004_QuestionnaireSetting WHERE SurveyId=@SurveyId ",sqlParams);
                }
                catch (Exception)
                {
                    //新增成功，查詢失敗
                    //replyData.data = "";

                    //throw;
                }
                //replyData.data = ExecuteQuery($"SELECT * FROM QUE004_QuestionnaireSetting WHERE SurveyId='{SurveyId}' ");
                //return Ok(iR);
            }
            catch (Exception ex)
            {
                Log.Error("新增基本設定記錄失敗!" + ex.Message);
                throw (ex);
                //replyData.code = "-1";
                //replyData.message = $"新增資料失敗！{ex.Message}.";
                //replyData.data = "";

            }
            //返回
            //return JsonConvert.SerializeObject(replyData);
            return null;
        }
        /// <summary>
        /// 基本設定之儲存(修改)
        /// </summary>
        /// <param name="value"></param>
        /// <returns></returns>
        [Route("Update")]
        [HttpPut]
        //public IActionResult Update([FromBody] Object value)
        public String Update([FromBody] Object value)
        {
            JObject jo = (JObject)JsonConvert.DeserializeObject(value.ToString());
            var replyData = new ReplyData();
            //必須依UserId update?
            if (jo["SurveyId"] == null)
            {
                //報告錯誤
                replyData.code = "-1";
                replyData.message = $"儲存失敗！參數SurveyId不能為空！";
                replyData.data = "";
                Log.Error("儲存失敗!" + "參數SurveyId不能為空！");
                return JsonConvert.SerializeObject(replyData);
            }

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

            var SurveyId = jo["SurveyId"].ToString();
            string sWhereCondition = $" WHERE SurveyId=@SurveyId ";
            string sSql = " UPDATE QUE004_QuestionnaireSetting SET ";

            //-------sql para----start
            List<SqlParameter> sqlParams = new List<SqlParameter>();
            //-------sql para----end

            var obj1 = new SqlParameter("@SurveyId", SqlDbType.UniqueIdentifier);
            obj1.Value = SurveyId.ValidGuid();
            sqlParams.Add(obj1);

            if (jo["IsShowPageNo"] != null)
            {
                var IsShowPageNo = jo["IsShowPageNo"].ToString();
                sSql += $" IsShowPageNo=@IsShowPageNo,";

                var obj = new SqlParameter("@IsShowPageNo", SqlDbType.VarChar);
                obj.Value = IsShowPageNo.Valid();
                sqlParams.Add(obj);
            }
            if (jo["IsShowQuestionNo"] != null)
            {
                var IsShowQuestionNo = jo["IsShowQuestionNo"].ToString();
                sSql += $" IsShowQuestionNo=@IsShowQuestionNo,";

                var obj = new SqlParameter("@IsShowQuestionNo", SqlDbType.VarChar);
                obj.Value = IsShowQuestionNo.Valid();
                sqlParams.Add(obj);
            }
            if (jo["IsShowRequiredStar"] != null)
            {
                var IsShowRequiredStar = jo["IsShowRequiredStar"].ToString();
                sSql += $" IsShowRequiredStar=@IsShowRequiredStar,";

                var obj = new SqlParameter("@IsShowRequiredStar", SqlDbType.VarChar);
                obj.Value = IsShowRequiredStar.Valid();
                sqlParams.Add(obj);
            }
            if (jo["IsShowProgress"] != null)
            {
                var IsShowProgress = jo["IsShowProgress"].ToString();
                sSql += $" IsShowProgress=@IsShowProgress,";

                var obj = new SqlParameter("@IsShowProgress", SqlDbType.VarChar);
                obj.Value = IsShowProgress.Valid();
                sqlParams.Add(obj);

            }
            if (jo["PorgressPosition"] != null)
            {
                var PorgressPosition = jo["PorgressPosition"].ToString();
                sSql += $" PorgressPosition=@PorgressPosition,";

                var obj = new SqlParameter("@PorgressPosition", SqlDbType.Int);
                obj.Value = PorgressPosition.ValidInt();
                sqlParams.Add(obj);
            }
            if (jo["ProgressStyle"] != null)
            {
                var ProgressStyle = jo["ProgressStyle"].ToString();
                sSql += $" ProgressStyle=@ProgressStyle,";

                var obj = new SqlParameter("@ProgressStyle", SqlDbType.Int);
                obj.Value = ProgressStyle.ValidInt();
                sqlParams.Add(obj);

            }
            if (jo["UseVirifyCode"] != null)
            {
                var UseVirifyCode = jo["UseVirifyCode"].ToString();
                sSql += $" UseVirifyCode=@UseVirifyCode,";

                var obj = new SqlParameter("@UseVirifyCode", SqlDbType.VarChar);
                obj.Value = UseVirifyCode.Valid();
                sqlParams.Add(obj);
            }
            if (jo["IsOneQuestionPerPage"] != null)
            {
                var IsOneQuestionPerPage = jo["IsOneQuestionPerPage"];
                sSql += $" IsOneQuestionPerPage=@IsOneQuestionPerPage,";

                var obj = new SqlParameter("@IsOneQuestionPerPage", SqlDbType.VarChar);
                obj.Value = IsOneQuestionPerPage.ValidStrOrDBNull();
                sqlParams.Add(obj);

            }
            if (jo["IsPublishResult"] != null)
            {
                var IsPublishResult = jo["IsPublishResult"].ToString();
                sSql += $" IsPublishResult=@IsPublishResult,";

                var obj = new SqlParameter("@IsPublishResult", SqlDbType.VarChar);
                obj.Value = IsPublishResult.Valid();
                sqlParams.Add(obj);

            }
            if (jo["IsShowEndPage"] != null)
            {
                var IsShowEndPage = jo["IsShowEndPage"].ToString();
                sSql += $" IsShowEndPage=@IsShowEndPage,";

                var obj = new SqlParameter("@IsShowEndPage", SqlDbType.VarChar);
                obj.Value = IsShowEndPage.Valid();
                sqlParams.Add(obj);
            }
            if (jo["IsShowOptionNo"] != null)
            {
                var IsShowOptionNo = jo["IsShowOptionNo"].ToString();
                sSql += $" IsShowOptionNo=@IsShowOptionNo,";

                var obj = new SqlParameter("@IsShowOptionNo", SqlDbType.VarChar);
                obj.Value = IsShowOptionNo.Valid();
                sqlParams.Add(obj);
            }

            //UpdUserId 會有程式依據Token取得，所以,目前暫時寫成00000000-0000-0000-0000-000000000000
            //var UpdUserId = "00000000-0000-0000-0000-000000000000";
            sSql += $" UpdUserId=@UpdUserId,";

            var Sobj = new SqlParameter("@UpdUserId", SqlDbType.UniqueIdentifier);
            Sobj.Value = UpdUserId.ValidGuid();
            sqlParams.Add(Sobj);

            //var UpdUserId = jo["UpdUserId"] != null ? jo["UpdUserId"].ToString() : "API";
            //sSql += $" UpdUserId='{UpdUserId}' ";
            // if (jo["UpdUserId"] != null)
            //{
            //    UpdUserId = jo["UpdUserId"].ToString();
            //    sSql += $" UpdUserId='{UpdUserId}'";
            //}
            //var UpdDateTime = DateTime.Now.ToString("yyyy/MM/dd HH:mm:ss");
            //sSql += $" UpdDateTime='{UpdDateTime}' ";
            sSql += "  UpdDateTime=SYSDATETIME() ";
            sSql += sWhereCondition;
            Log.Debug("修改QUE004_QuestionnaireSetting:" + sSql);
            try
            {
                int iR = _db.ExecuteSql(sSql, sqlParams.ToArray());

                replyData.code = "200";
                replyData.message = $"儲存記錄完成。";
                try
                {
                    //執行成功後,需要將本筆資料帶回前端
                    //-------sql para----start
                    SqlParameter[] sqlSParams = new SqlParameter[] {
                    new SqlParameter("@SurveyId", SqlDbType.UniqueIdentifier),

                    };
                    sqlParams[0].Value = SurveyId.ValidGuid();
                    //-------sql para----end
                    var result = ExecuteQuery($"SELECT * FROM QUE004_QuestionnaireSetting WHERE SurveyId=@SurveyId ",sqlSParams);
                    if (result.Count == 1)
                        replyData.data = result[0];  //應該只有一筆
                    else
                        replyData.data = result;
                }
                catch (Exception)
                {
                    //修改成功 查詢失敗
                    replyData.data = "";
                }
                //replyData.data = ExecuteQuery($"SELECT * FROM QUE004_QuestionnaireSetting WHERE SurveyId='{SurveyId}' ");
                Log.Debug("儲存基本設定記錄完成。共修改{iR}筆。");
                //return Ok(iR);
            }
            catch (Exception ex)
            {
                replyData.code = "-1";
                replyData.message = $"儲存記錄失敗！{ex.Message}.";
                replyData.data = "";
                Log.Error("修改基本設定記錄失敗!" + ex.Message);
            }
            //return Ok(replyData);
            return JsonConvert.SerializeObject(replyData);
        }
        /// <summary>
        /// 基本設定之刪除
        /// </summary>
        /// <param name="value"></param>
        /// <returns></returns>
        [Route("Delete")]
        [HttpDelete]
        //public IActionResult Delete([FromBody] Object value)
        public String Delete([FromBody] Object value)
        {
            JObject jo = (JObject)JsonConvert.DeserializeObject(value.ToString());
            var replyData = new ReplyData();
            string SurveyId = jo["SurveyId"].ToString();

            //-------sql para----start
            SqlParameter[] sqlParams = new SqlParameter[] {
                new SqlParameter("@SurveyId", SqlDbType.UniqueIdentifier),
            };
            sqlParams[0].Value = SurveyId.ValidGuid();
            //-------sql para----end

            string sSql = string.Format("DELETE FROM QUE004_QuestionnaireSetting WHERE SurveyId=@SurveyId ");
            Log.Debug("刪除QUE004_QuestionnaireSetting:" + sSql);
            try
            {
                int iR = _db.ExecuteSql(sSql, sqlParams);
                replyData.code = "000";
                replyData.message = $"刪除記錄完成。";
                replyData.data = "";
                Log.Debug("刪除基本設定記錄完成。共刪除{iR}筆。");
                //return Ok(iR);
            }
            catch (Exception ex)
            {
                replyData.code = "-1";
                replyData.message = $"刪除記錄失敗！{ex.Message}.";
                replyData.data = "";
                Log.Error("刪除QUE004_QuestionnaireSetting資料失敗!" + ex.Message);
            }
            //return Ok(replyData);
            return JsonConvert.SerializeObject(replyData);
        }


        /// <summary>
        /// 依據傳入sql command 執行查詢
        /// </summary>
        /// <param name="sSql"></param>
        /// <returns></returns>
        private List<QUE004_QuestionnaireSetting> ExecuteQuery(String sSql)
        {
            List<QUE004_QuestionnaireSetting> lstBaseicSetting = new List<QUE004_QuestionnaireSetting>();
            try
            {
                DataTable dtR = _db.GetQueryData(sSql);
                foreach (DataRow dr in dtR.Rows)
                {
                    QUE004_QuestionnaireSetting baseicSetting = new QUE004_QuestionnaireSetting();
                    baseicSetting.SurveyId = dr["SurveyId"];
                    baseicSetting.IsShowPageNo = dr["IsShowPageNo"];
                    baseicSetting.IsShowQuestionNo = dr["IsShowQuestionNo"];
                    baseicSetting.IsShowRequiredStar = dr["IsShowRequiredStar"];
                    baseicSetting.IsShowProgress = dr["IsShowProgress"];
                    baseicSetting.PorgressPosition = dr["PorgressPosition"];
                    baseicSetting.ProgressStyle = dr["ProgressStyle"];
                    baseicSetting.UseVirifyCode = dr["UseVirifyCode"];
                    baseicSetting.IsOneQuestionPerPage = dr["IsOneQuestionPerPage"];
                    baseicSetting.IsPublishResult = dr["IsPublishResult"];
                    baseicSetting.IsShowEndPage = dr["IsShowEndPage"];
                    baseicSetting.IsShowOptionNo = dr["IsShowOptionNo"] == DBNull.Value ? "0" : dr["IsShowOptionNo"];
                    //baseicSetting.UpdUserId = dr["UpdUserId"];
                    //baseicSetting.UpdDateTime = dr["UpdDateTime"];
                    lstBaseicSetting.Add(baseicSetting);
                }
            }
            catch (Exception)
            {
                throw;
            }
            return lstBaseicSetting;
        }

        /// <summary>
        /// 依據傳入sql command 執行查詢
        /// </summary>
        /// <param name="sSql"></param>
        /// <returns></returns>
        private List<QUE004_QuestionnaireSetting> ExecuteQuery(String sSql, SqlParameter[] cmdParams)
        {
            List<QUE004_QuestionnaireSetting> lstBaseicSetting = new List<QUE004_QuestionnaireSetting>();
            try
            {
                DataTable dtR = _db.GetQueryData(sSql, cmdParams);
                foreach (DataRow dr in dtR.Rows)
                {
                    QUE004_QuestionnaireSetting baseicSetting = new QUE004_QuestionnaireSetting();
                    baseicSetting.SurveyId = dr["SurveyId"];
                    baseicSetting.IsShowPageNo = dr["IsShowPageNo"];
                    baseicSetting.IsShowQuestionNo = dr["IsShowQuestionNo"];
                    baseicSetting.IsShowRequiredStar = dr["IsShowRequiredStar"];
                    baseicSetting.IsShowProgress = dr["IsShowProgress"];
                    baseicSetting.PorgressPosition = dr["PorgressPosition"];
                    baseicSetting.ProgressStyle = dr["ProgressStyle"];
                    baseicSetting.UseVirifyCode = dr["UseVirifyCode"];
                    baseicSetting.IsOneQuestionPerPage = dr["IsOneQuestionPerPage"];
                    baseicSetting.IsPublishResult = dr["IsPublishResult"];
                    baseicSetting.IsShowEndPage = dr["IsShowEndPage"];
                    baseicSetting.IsShowOptionNo = dr["IsShowOptionNo"] == DBNull.Value ? "0" : dr["IsShowOptionNo"];
                    //baseicSetting.UpdUserId = dr["UpdUserId"];
                    //baseicSetting.UpdDateTime = dr["UpdDateTime"];
                    lstBaseicSetting.Add(baseicSetting);
                }
            }
            catch (Exception)
            {
                throw;
            }
            return lstBaseicSetting;
        }

        /// <summary>
        /// 依據傳入sql command 執行查詢
        /// </summary>
        /// <param name="sSql"></param>
        /// <returns></returns>
        private List<QUE004_QuestionnaireSetting> ExecuteQuery(String sSql, List<SqlParameter> cmdParams)
        {
            List<QUE004_QuestionnaireSetting> lstBaseicSetting = new List<QUE004_QuestionnaireSetting>();
            try
            {
                DataTable dtR = _db.GetQueryData(sSql, cmdParams.ToArray());
                foreach (DataRow dr in dtR.Rows)
                {
                    QUE004_QuestionnaireSetting baseicSetting = new QUE004_QuestionnaireSetting();
                    baseicSetting.SurveyId = dr["SurveyId"];
                    baseicSetting.IsShowPageNo = dr["IsShowPageNo"];
                    baseicSetting.IsShowQuestionNo = dr["IsShowQuestionNo"];
                    baseicSetting.IsShowRequiredStar = dr["IsShowRequiredStar"];
                    baseicSetting.IsShowProgress = dr["IsShowProgress"];
                    baseicSetting.PorgressPosition = dr["PorgressPosition"];
                    baseicSetting.ProgressStyle = dr["ProgressStyle"];
                    baseicSetting.UseVirifyCode = dr["UseVirifyCode"];
                    baseicSetting.IsOneQuestionPerPage = dr["IsOneQuestionPerPage"];
                    baseicSetting.IsPublishResult = dr["IsPublishResult"];
                    baseicSetting.IsShowEndPage = dr["IsShowEndPage"];
                    baseicSetting.IsShowOptionNo = dr["IsShowOptionNo"] == DBNull.Value ? "0" : dr["IsShowOptionNo"];
                    //baseicSetting.UpdUserId = dr["UpdUserId"];
                    //baseicSetting.UpdDateTime = dr["UpdDateTime"];
                    lstBaseicSetting.Add(baseicSetting);
                }
            }
            catch (Exception)
            {
                throw;
            }
            return lstBaseicSetting;
        }

    }
}
