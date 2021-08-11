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
    [Route("api/Survey/Style")]
    [ApiController]
    public class SurveyStyleController : ControllerBase
    {
        /// <summary>
        /// 問卷系統後臺--設計問卷--外觀
        /// </summary>
        /// <designer>Allen Fan/Gem He</design>
        /// <History> 1. 2020/08/20 Gem Create
        /// </History>


        //private readonly ILogger<SurveyQuestionTextController> _logger;  //可實現log在console輸出
        private DBHelper _db;
        //public SurveyQuestionTextController(ILogger<SurveyQuestionTextController> logger)
        public SurveyStyleController()
        {
            //_logger = logger;
            //_db = new DBHelper();
            //_logger = logger;
            //_connectionOptions = options.Value;
            _db = new DBHelper(AppSettingsHelper.DefaultConnectionString);
        }
        /// <summary>
        ///  設計問卷-外觀設定查詢
        /// </summary>
        /// <returns></returns>
        [Route("Query")]
        [HttpGet]
        public String Query(String SurveyId)
        {
            Log.Debug("設計問卷-外觀設定查詢...");

            List<SurveyStyle> lstQStyle = new List<SurveyStyle>();
            ReplyData replyData = new ReplyData();

            //-------sql para----start
            List<SqlParameter> sqlParams = new List<SqlParameter>();
            //-------sql para----end

            string sWhere = " WHERE 1=1 ";
            if (SurveyId != null)
            {
                sWhere += $" AND SurveyId=@SurveyId ";

                var obj = new SqlParameter("@SurveyId", SqlDbType.UniqueIdentifier);
                obj.Value = SurveyId.ValidGuid();
                sqlParams.Add(obj);
            }
            else
            {
                //報告錯誤
                replyData.code = "-1";
                replyData.message = $"外觀設定查詢失敗！參數SurveyId不能為空！";
                replyData.data = "";
                Log.Error("外觀設定查詢!" + "參數SurveyId不能為空！");
                return JsonConvert.SerializeObject(replyData);
            }
             string sSql = "SELECT * FROM QUE001_QuestionnaireBase " + sWhere;
            try
            {
                lstQStyle = ExecuteQuery(sSql, sqlParams.ToArray());
                replyData.code = "200";
                replyData.message = $"查詢記錄完成。";
                Log.Debug($"外觀設定查詢記錄完成。共{lstQStyle.Count}筆。");
                //先不要SerializeObject list 應該也可以
                if (lstQStyle.Count == 1)  //SurveyId+QuestionId 應該只有一筆
                    replyData.data = lstQStyle[0];  // JsonConvert.SerializeObject(lstBaseicSetting);
                else
                    replyData.data = lstQStyle;
            }
            catch (Exception ex)
            {
                replyData.code = "-1";
                replyData.message = $"基本資料-單題查詢失敗！{ex.Message}.";
                replyData.data = "";
                Log.Error("基本資料-單題查詢記錄失敗!" + ex.Message);
            }

            //返回結果
            return JsonConvert.SerializeObject(replyData);
            //return lstUserInfo.ToArray();
        }

        /// <summary>
        /// 設計問卷-外觀編輯
        /// </summary>
        /// <param name="value"></param>
        /// <returns></returns>
        [Route("Update")]
        [HttpPut]
        public String Update([FromBody] Object value)
        {
            /* 輸入格式：
             * {
             *    "SurveyId": "",
             *    "StyleType": 0,
             *    "DefHeaderPic": "",
             *    "DefBackgroudColor": "",
             *    "DefPhoneHeaderPic": ""
             *}
             */
            JObject jo = (JObject)JsonConvert.DeserializeObject(value.ToString());
            var replyData = new ReplyData();

            //SurveyId 必須有?
            if (jo["SurveyId"] == null)
            {
                //報告錯誤
                replyData.code = "-1";
                replyData.message = $"編輯失敗！參數SurveyId不能為空！";
                replyData.data = "";
                Log.Error("圖片題型-編輯失敗!" + "參數SurveyId不能為空！");
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

            //-------sql para----start
            List<SqlParameter> sqlParams = new List<SqlParameter>();
            //-------sql para----end

            string sWhereCondition = $" WHERE SurveyId=@SurveyId ";
            var obj_SurveyId = new SqlParameter("@SurveyId", SqlDbType.UniqueIdentifier);
            obj_SurveyId.Value = SurveyId.ValidGuid();
            sqlParams.Add(obj_SurveyId);
            string sSql = " UPDATE QUE001_QuestionnaireBase SET ";

            if (jo["StyleType"] != null)
            {
                var StyleType = Convert.ToInt32(jo["StyleType"]);
                sSql += $" StyleType=@StyleType,";

                var obj = new SqlParameter("@StyleType", SqlDbType.Int);
                obj.Value = StyleType.ValidInt();;
                sqlParams.Add(obj);
            }
            if (jo["DefHeaderPic"] != null)
            {
                var DefHeaderPic = jo["DefHeaderPic"];
                sSql += $" DefHeaderPic=@DefHeaderPic,";

                var obj = new SqlParameter("@DefHeaderPic", SqlDbType.VarChar);
                obj.Value = DefHeaderPic.ValidStrOrDBNull();;
                sqlParams.Add(obj);
            }
            if (jo["DefBackgroudColor"] != null)
            {
                var DefBackgroudColor = (jo["DefBackgroudColor"]);
                sSql += $" DefBackgroudColor=@DefBackgroudColor,";

                var obj = new SqlParameter("@DefBackgroudColor", SqlDbType.NVarChar);
                obj.Value = DefBackgroudColor.ValidStrOrDBNull();;
                sqlParams.Add(obj);
            }
            if (jo["DefHeaderPhonePic"] != null)
            {
                var DefHeaderPhonePic = jo["DefHeaderPhonePic"].ToString();
                sSql += $" DefHeaderPhonePic=@DefHeaderPhonePic,";

                var obj = new SqlParameter("@DefHeaderPhonePic", SqlDbType.NVarChar);
                obj.Value = DefHeaderPhonePic.Valid();;
                sqlParams.Add(obj);
            }
            //UpdUserId 會有程式依據Token取得，所以,目前暫時寫成00000000-0000-0000-0000-000000000000
            //var UpdUserId = "00000000-0000-0000-0000-000000000000";
            sSql += $" UpdUserId=@UpdUserId,";
            var obj1 = new SqlParameter("@UpdUserId", SqlDbType.UniqueIdentifier);
            obj1.Value = UpdUserId.ValidGuid();
            sqlParams.Add(obj1);
            //if (jo["UpdUserId"] != null)
            //{
            //    var UpdUserId = jo["UpdUserId"].ToString();
            //    sSql += $" UpdUserId=NEWID(),";
            //}
            //updatetime 為 datetime2  yyyy-MM-dd HH:mm:ss.fffffff
            var UpdDateTime = DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss.fffffff");
            sSql += $" UpdDateTime=SYSDATETIME() ";

            sSql += sWhereCondition;

            Log.Debug("設計問卷-外觀編輯:" + sSql);
            try
            {
                int iR = _db.ExecuteSql(sSql, sqlParams.ToArray());

                replyData.code = "200";
                replyData.message = $"編輯記錄完成。";

                try
                {
                    //-------sql para----start
                    SqlParameter[] sqlParamsA = new SqlParameter[] {
                        new SqlParameter("@SurveyId", SqlDbType.UniqueIdentifier)
                    };
                    sqlParamsA[0].Value = SurveyId.ValidGuid();
                    //-------sql para----end
                    //執行成功後,需要將本筆資料帶回前端
                    var result = ExecuteQuery($"SELECT * FROM QUE001_QuestionnaireDetail WHERE SurveyId=@SurveyId ", sqlParamsA);
                    if (result.Count == 1)
                        replyData.data = result[0];  //應該只有一筆
                    else
                        replyData.data = result;
                }
                catch (Exception ex)
                {
                    //新增成功，查詢失敗
                    replyData.data = "";
                    Log.Debug("設計問卷-外觀編輯成功,查詢返回結果失敗!" + ex.Message);
                }
            }
            catch (Exception ex)
            {
                replyData.code = "-1";
                replyData.message = $"編輯記錄失敗！{ex.Message}.";
                replyData.data = "";
                Log.Error("設計問卷-外觀編輯記錄失敗!" + ex.Message);
            }
            //返回
            return JsonConvert.SerializeObject(replyData);

        }

        /// <summary>
        /// 依據傳入sql command 執行查詢
        /// </summary>
        /// <param name="sSql"></param>
        /// <returns></returns>
        private List<SurveyStyle> ExecuteQuery(String sSql)
        {
            List<SurveyStyle> lstQStyle = new List<SurveyStyle>();
            try
            {
                DataTable dtR = _db.GetQueryData(sSql);
                foreach (DataRow dr in dtR.Rows)
                {
                    SurveyStyle Qstyle = new SurveyStyle();
                    Qstyle.SurveyId = dr["SurveyId"];
                    Qstyle.StyleType = dr["StyleType"];
                    Qstyle.DefHeaderPic = dr["DefHeaderPic"];
                    Qstyle.DefBackgroudColor = dr["DefBackgroudColor"];
                    Qstyle.DefHeaderPhonePic = dr["DefHeaderPhonePic"];

                    lstQStyle.Add(Qstyle);
                }
            }
            catch (Exception)
            {
                throw;
            }
            return lstQStyle;
        }

        /// <summary>
        /// 依據傳入sql command 執行查詢
        /// </summary>
        /// <param name="sSql"></param>
        /// <returns></returns>
        private List<SurveyStyle> ExecuteQuery(String sSql, SqlParameter[] cmdParams)
        {
            List<SurveyStyle> lstQStyle = new List<SurveyStyle>();
            try
            {
                DataTable dtR = _db.GetQueryData(sSql, cmdParams);
                foreach (DataRow dr in dtR.Rows)
                {
                    SurveyStyle Qstyle = new SurveyStyle();
                    Qstyle.SurveyId = dr["SurveyId"];
                    Qstyle.StyleType = dr["StyleType"];
                    Qstyle.DefHeaderPic = dr["DefHeaderPic"];
                    Qstyle.DefBackgroudColor = dr["DefBackgroudColor"];
                    Qstyle.DefHeaderPhonePic = dr["DefHeaderPhonePic"];

                    lstQStyle.Add(Qstyle);
                }
            }
            catch (Exception)
            {
                throw;
            }
            return lstQStyle;
        }
    }
    public class SurveyStyle
    {
        //以後如果有補充欄位從QUE001 copy
        public Object SurveyId { get; set; }
        public Object StyleType { get; set; }
        public Object DefHeaderPic { get; set; }
        public Object DefBackgroudColor { get; set; }
        public Object DefHeaderPhonePic { get; set; }
    }
}
