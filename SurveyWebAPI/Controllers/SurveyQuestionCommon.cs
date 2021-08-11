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
    [Route("api/Survey/Question/Common")]
    [ApiController]
    public class SurveyQuestionCommon : ControllerBase
    {
        /// <summary>
        /// 問卷系統後臺之共用--新增大量選項、儲存題目內容樣式
        /// </summary>
        /// <designer>Allen/Gem</designer>
        private DBHelper _db;
        public SurveyQuestionCommon()
        {
            _db = new DBHelper(AppSettingsHelper.DefaultConnectionString);
        }
        #region 新增大量選項
        /// <summary>
        /// 新增大量選項
        /// </summary>
        /// <param name="value"></param>
        /// <returns></returns>
        [Route("InsertBatchOption")]
        [HttpPost]
        public String InsertBatchOption([FromBody] Object value)
        {
            //輸入格式如下：
            /* "surveyId": "",
             * "questionId": "",
             * "option": [
             * "選項1",
             * "選項2",
             * "選項3"
             * ]
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
            JObject jo = (JObject)JsonConvert.DeserializeObject(value.ToString());
            var replyData = new ReplyData();

            // 和SurveyId無關
            //if (jo["SurveyId"] == null)
            //{
            //    //據Allen講, DB中所有Id欄位,都是GUID產生, 那這是client 產好傳過來還是後台產生????????
            //    /* 註解一下GUID：GUID，Globally Unique Identifier ,全局唯一標識，
            //     * C#產生時，有下列4種格式：
            //     * 格式 xxxxxxxx-xxxx-xxxx-xxxx-xxxxxxxxxxxx 每個x表0-9或者a-f的十六進制
            //     * string guid1 = Guid.NewGuid().ToString("N"); d468954e22a145f8806ae41fb938e79e
            //     * string guid2 = Guid.NewGuid().ToString("D"); c05d1709-0361-4304-8b2c-58fadcc4ae08
            //     * string guid3 = Guid.NewGuid().ToString("P"); (d3a300a7-144d-4587-9e22-3a7699013f01)
            //     * string guid4 = Guid.NewGuid().ToString("B"); {3351ca09-5302-400a-aea8-2a8be6c12b06}
            //     * SQL Server 的 NEWID()產生的格式 c05d1709-0361-4304-8b2c-58fadcc4ae08 和C# D參數產生的一致。
            //     */
            //    // var uuid = Guid.NewGuid().ToString();

            //    //報告錯誤
            //    replyData.code = "-1";
            //    replyData.message = $"新增大量選項失敗！參數SurveyId不能為空！";
            //    replyData.data = "";
            //    Log.Error("新增大量選項失敗!" + "參數SurveyId不能為空！");
            //    return JsonConvert.SerializeObject(replyData);
            //}
            //var SurveyId = jo["surveyId"].ToString();
            if (jo["QuestionId"] == null||String.IsNullOrWhiteSpace(jo["QuestionId"].ToString()))
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
                replyData.message = $"新增大量選項失敗！參數QuestionId不能為空！";
                replyData.data = "";
                Log.Error("新增大量選項失敗!" + "參數QuestionId不能為空！");
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

            //UpdUserId 會有程式依據Token取得，所以,目前暫時寫成00000000-0000-0000-0000-000000000000
            //var UpdUserId = jo["UpdUserId"] == null ? "00000000-0000-0000-0000-000000000000" : jo["UpdUserId"].ToString();
            //var UpdUserId = "00000000-0000-0000-0000-000000000000";
            var QuestionId = jo["QuestionId"].ToString();
            var arrOption = jo["Option"];

            if(arrOption.Count()<1)
            {
                Log.Error("參數選項為空白，不用新增啦！");
                ErrorCode.Code = "204";
                replyData.code = ErrorCode.Code;
                replyData.message = ErrorCode.Message;
                //replyData.code = "-1";
                //replyData.message = $"新增大量選項失敗！選項空白！";
                replyData.data = "";
                return JsonConvert.SerializeObject(replyData);
            }
            if (arrOption.Count() >30)
            {
                Log.Error("大量選項新增不可超過三十筆！");
                ErrorCode.Code = "203";
                replyData.code = ErrorCode.Code;
                replyData.message = ErrorCode.Message;
                //replyData.code = "-1";
                //replyData.message = $"大量選項新增不可超過三十筆！";
                replyData.data = "";
                return JsonConvert.SerializeObject(replyData);
            }
            //開始新增
            //List<string> sqls = new List<string>();
            var sqls = new List<KeyValuePair<string, SqlParameter[]>>();

            string sSql = " INSERT INTO QUE003_QuestionnaireOptions " +
                " (QuestionId, OptionId, OptionSeq, OptionType, OptionContent, ChildQuestionId, OtherFlag, UpdUserId, UpdDateTime ) ";
            foreach (var option in arrOption)
            {
                //選項寫入OptionContent, ChildQuestionId 置null, 相同的QuestionId, 序號遞增, optionType=0
                var vSql = $" VALUES(@QuestionId,NEWID(), " +
                    $"(select ISNULL(MAX(B.OptionSeq),0)+1 from QUE003_QuestionnaireOptions B where B.QuestionId=@QuestionId)," +
                    $" 0," +
                    $"@option,null,'0',@UpdUserId,SYSDATETIME())";

                //-------sql para----start
                SqlParameter[] sqlParams = new SqlParameter[] {
                    new SqlParameter("@QuestionId", SqlDbType.UniqueIdentifier),
                    new SqlParameter("@option", SqlDbType.UniqueIdentifier),
                    new SqlParameter("@UpdUserId", SqlDbType.UniqueIdentifier),



                    };
                sqlParams[0].Value = QuestionId.ValidGuid();
                sqlParams[1].Value = new System.Data.SqlTypes.SqlGuid(option.ToString());
                sqlParams[2].Value = UpdUserId.ValidGuid();

                //-------sql para----end

                var obj = new KeyValuePair<string, SqlParameter[]>(string.Concat(sSql, vSql), sqlParams);
                sqls.Add(obj);


            }

            try
            {
                int iR = _db.ExecuteSqlTran(sqls);

                replyData.code = "200";
                replyData.message = $"新增大量選項完成。";
                replyData.data = arrOption;

            }
            catch (Exception ex)
            {
                replyData.code = "-1";
                replyData.message = $"新增大量選項失敗！{ex.Message}.";
                replyData.data = "";
                Log.Error("新增大量選項記錄失敗!" + ex.Message);
            }
            //返回
            return JsonConvert.SerializeObject(replyData);

        }
        #endregion

        #region 儲存題目內容樣式
        /// <summary>
        /// 儲存題目內容樣式
        /// </summary>
        /// <param name="value"></param>
        /// <returns></returns>
        [Route("SaveStyle")]
        [HttpPut]
        public String SaveStyle([FromBody] Object value)
        {
            //輸入格式如下：
            /* "SurveyId":1231,
             * "QuestionId":21321,
             * "SubjectStyle":"<p>標題內容</p>"
             */
            //依據SuveyId+QuestionId update QUE002.SubjectStyle

            JObject jo = (JObject)JsonConvert.DeserializeObject(value.ToString());
            var replyData = new ReplyData();

            //UserId 必須有?
            if (jo["SurveyId"] == null)
            {
                //報告錯誤
                replyData.code = "-1";
                replyData.message = $"儲存題目內容樣式失敗！參數SurveyId不能為空！";
                replyData.data = "";
                Log.Error("儲存題目內容樣式失敗!" + "參數SurveyId不能為空！");
                return JsonConvert.SerializeObject(replyData);
            }
            var SurveyId = jo["SurveyId"].ToString();
            if (jo["QuestionId"] == null)
            {

                //報告錯誤
                replyData.code = "-1";
                replyData.message = $"儲存題目內容樣式失敗！參數QuestionId不能為空！";
                replyData.data = "";
                Log.Error("儲存題目內容樣式失敗!" + "參數QuestionId不能為空！");
                return JsonConvert.SerializeObject(replyData);
            }
            var QuestionId = jo["QuestionId"].ToString();
            var SubjectStyle = jo["SubjectStyle"];

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

            //var UpdDateTime = DateTime.Now.ToString("yyyy/MM/dd HH:mm:ss.fffffff");
            string sSql = " UPDATE QUE002_QuestionnaireDetail "+
                $" SET SubjectStyle=@SubjectStyle, UpdUserId=@UpdUserId,UpdDateTime=SYSDATETIME()" +
                $" WHERE SurveyId=@SurveyId AND QuestionId= @QuestionId";  //UpdDateTime 為datetime2: yyyy-MM-dd HH:mm:ss.ffffffff


            //-------sql para----start
            SqlParameter[] sqlParams = new SqlParameter[] {
                    new SqlParameter("@SubjectStyle", SqlDbType.NVarChar),
                    new SqlParameter("@UpdUserId", SqlDbType.UniqueIdentifier),
                    new SqlParameter("@SurveyId", SqlDbType.UniqueIdentifier),
                    new SqlParameter("@QuestionId", SqlDbType.UniqueIdentifier),



                    };
            sqlParams[0].Value = SubjectStyle.ValidStrOrDBNull();
            sqlParams[1].Value = UpdUserId.ValidGuid();
            sqlParams[2].Value = SurveyId.ValidGuid();
            sqlParams[3].Value = QuestionId.ValidGuid();

            //-------sql para----end

            Log.Debug("儲存題目內容樣式:" + sSql);
            try
            {
                int iR = _db.ExecuteSql(sSql,sqlParams);

                replyData.code = "200";
                replyData.message = $"儲存題目內容樣式完成。";
                replyData.data = iR;
            }
            catch (Exception ex)
            {
                replyData.code = "-1";
                replyData.message = $"儲存題目內容樣式失敗！{ex.Message}.";
                replyData.data = "";
                Log.Error("儲存題目內容樣式失敗!" + ex.Message);
            }
            //返回
            return JsonConvert.SerializeObject(replyData);

        }
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
                    baseicSetting.IsShowOptionNo = dr["IsShowOptionNo"];
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
        #endregion
    }
}
