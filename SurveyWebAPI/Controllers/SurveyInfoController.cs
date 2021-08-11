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
    [Route("api/Survey/Info")]
    [ApiController]
    public class SurveyInfoController : ControllerBase
    {
        /// <summary>
        /// 問卷系統後臺之主畫面操作--Get問卷清單列表、新增一個問卷
        /// </summary>
        /// <designer>Allen/Gem</designer>
        private DBHelper _db;
        private DBHelper _crmDB;
        private DataTable _dtCode0102;
        public SurveyInfoController()
        {
            _db = new DBHelper(AppSettingsHelper.DefaultConnectionString);
            _crmDB = new DBHelper(AppSettingsHelper.CRMConnectionString);
        }

        #region 問卷清單列表
        /// <summary>
        /// GET Get問卷清單列表
        /// </summary>
        /// <returns></returns>
        [Route("queryByPage")]
        [HttpGet]
        public String queryByPage()
        {
            Log.Debug("主畫面操作-Get問卷清單列表...");

            List<SurveyList> lstStatus = new List<SurveyList>();
            ReplyData replyData = new ReplyData();

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


            string sOrder = " ORDER BY A.UpdDateTime DESC ";// " ORDER BY A.CreateDateTime DESC ";
            string sSql = $"SELECT A.SurveyId, A.Title, A.Audit, " +
                        " NULL AS StartDate, NULL AS EndDate, NULL AS[Status], SYSDATETIME() AS SysDate, " +  //這三個參數從其他DB來，暫寫空白
                        // " COUNT(B.ReplyId) AS ReplyNum, " + // 修改問卷清單回覆數量有誤_20201019
                        " (Select COUNT(B.ReplyId) From QUE021_AnwserCollection B Where B.SurveyId = A.SurveyId and B.Env = '2' and (B.DelFlag IS NULL OR B.DelFlag <> 1)) AS ReplyNum," + // 修改問卷清單回覆數量有誤_20201019
                        " COUNT(D.SurveyId) AS SurveyNumInQUE002," +
                        " A.CreateUserId, C.UserName AS CreateUserName, A.CreateDateTime, A.UpdDateTime " +
                        " FROM QUE001_QuestionnaireBase A " +
                        " LEFT JOIN QUE021_AnwserCollection B ON B.SurveyId = A.SurveyId AND (B.DelFlag IS NULL OR B.DelFlag <> 1) " +
                        " LEFT JOIN SSEC001_UserInfo C ON C.UserId=A.CreateUserId " +
                        " LEFT JOIN QUE002_QuestionnaireDetail D ON D.SurveyId=A.SurveyId ";

            string sGroup= " GROUP BY A.SurveyId, A.Title, A.Audit,  A.CreateUserId, C.UserName, A.CreateDateTime, A.UpdDateTime ";

            string sWhere = " WHERE 1=1 AND A.DelFlag <> 1 ";// (B.DelFlag IS NULL OR B.DelFlag <> 1) "; // 修改問卷清單回覆數量有誤_20201019
            if (info.RoleId==2) //角色ID = 2為行銷人員，只回傳他自己創建的問卷
            {
                sWhere += $" AND A.CreateUserId=@UpdUserId ";
            }
            sSql += sWhere;
            sSql += sGroup;
            sSql += sOrder;

            //-------sql para----start
            SqlParameter[] sqlParams = new SqlParameter[] {
                new SqlParameter("@UpdUserId", SqlDbType.UniqueIdentifier),
            };
            sqlParams[0].Value = UpdUserId.ValidGuid();
            //-------sql para----end

            try
            {
                Log.Debug($"queryByPage:sql='{sSql}'");
                _dtCode0102= LoadAllCode0102("0102");

                DataTable dtR = _db.GetQueryData(sSql, sqlParams);
                var CurrentDate = DateTime.Now.ToString("yyyy/MM/dd HH:mm:ss.fffffff");
                foreach (DataRow dr in dtR.Rows)
                {
                    //SurveyPageInfo spi = new SurveyPageInfo();
                    SurveyList suvstatus = new SurveyList();
                    suvstatus.SurveyId = dr["SurveyId"];
                    suvstatus.Title = dr["Title"];
                    //新增Audit(QUE001.Audit) + isEmpty(QUE002 by SurveyID無值給true, 否則false)
                    suvstatus.Audit = dr["Audit"];
                    suvstatus.IsEmpty = Convert.ToInt32(dr["SurveyNumInQUE002"]) > 0 ? false : true;
                    //以下三個欄位來自CRM
                    var crm = GetCrmStatusBy(suvstatus.SurveyId.ToString());
                    //suvstatus.StartDate = dr["StartDate"] == DBNull.Value ? dr["SysDate"] : dr["StartDate"];
                    //suvstatus.EndDate = dr["EndDate"] == DBNull.Value ? dr["SysDate"] : dr["EndDate"];
                    ////Status 需要處理一下：依據Config, 產生1-7內的隨機數字，或者1
                    //suvstatus.Status = dr["Status"] == DBNull.Value ? GetStatusBy() : dr["Status"];
                    suvstatus.StartDate = crm.New_effectivestart ;
                    suvstatus.EndDate = crm.New_effectiveend;
                    //Status 需要處理一下：依據Config, 產生1-7內的隨機數字，或者1
                    //狀態要再去串GEN004_AllCode(0102)把中文字取得再傳給前端
                    suvstatus.Status = crm.New_statuscode.ToString();
                    suvstatus.StatusDesc = GetCodeSubNameBy(crm.New_statuscode);
                    //以上三個欄位來自CRM，需要處理
                    suvstatus.ReplyNum = dr["ReplyNum"];
                    suvstatus.CreateUserId = dr["CreateUserId"];
                    suvstatus.CreateUserName = dr["CreateUserName"];
                    suvstatus.UpdDateTime = dr["UpdDateTime"];
                    //spi.list = suvstatus;
                    lstStatus.Add(suvstatus);
                }

                replyData.code = "200";
                replyData.message = $"查詢紀錄完成。共{lstStatus.Count}筆。";
                Log.Debug($"查詢紀錄完成。共{lstStatus.Count}筆。");
                //先不要SerializeObject list 應該也可以
                replyData.data = lstStatus;// JsonConvert.SerializeObject(lstStatus);
            }
            catch (Exception ex)
            {
                replyData.code = "-1";
                replyData.message = $"查詢紀錄失敗！{ex.Message}.";
                replyData.data = "";
                Log.Error("查詢紀錄失敗!" + ex.Message);
            }

            //返回結果
            return JsonConvert.SerializeObject(replyData);
            //return lstUserInfo.ToArray();
        }
        private Object GetCodeSubNameBy(Object codeSubCode)
        {
            if (codeSubCode == null || codeSubCode == DBNull.Value)
                return null;
            else if (codeSubCode.ToString().Trim().Length == 0)
                return "";
            var drs = _dtCode0102.Select($" CodeSubCode='{codeSubCode.ToString().Trim()}'");
            if (drs.Length > 0)
                return drs[0]["CodeSubName"];
            else
                return "";
        }
        /// <summary>
        /// 依據CodeCode取得GEN004_AllCode資料
        /// </summary>
        /// <param name="codeCode"></param>
        /// <returns></returns>
        private DataTable LoadAllCode0102(string codeCode)
        {
            try
            {
                //-------sql para----start
                SqlParameter[] sqlParams = new SqlParameter[] {
                new SqlParameter("@codeCode", SqlDbType.VarChar),
                };
                sqlParams[0].Value = codeCode.Valid();
                //-------sql para----end
                DataTable dt = _db.GetQueryData($" SELECT * FROM GEN004_AllCode WHERE CodeCode=@codeCode AND UsedMark='1' ORDER BY CAST(CodeSubCode as int) ",sqlParams);
                return dt;
            }
            catch(Exception ex)
            {
                Log.Error("LoadAllCode0102：" + ex.StackTrace);
                Log.Error("LoadAllCode0102：" + ex.Message);
                Log.Error($"取參數CodeCode='{codeCode}'失敗！");
                throw ex;
            }
        }
        /// <summary>
        /// Status空白時，依據config參數，看回傳1還是1-7之間的隨機數
        /// </summary>
        /// <returns></returns>
        private Object GetStatusBy()
        {
            var Status = 1;
            try
            {
                // Status空白時，判斷是否需要隨機產生Status (要設定config，決定是否random)
                if (Common.AppSettingsHelper.NeedRandomStatus)
                {
                    //Math.random()是0-1之間的隨機數，乘長度就是0到長度之間的隨機數
                    Status = 1;
                     //seed 可以隨便寫?
                    Random rd = new Random();// Random(seed);
                    Status = rd.Next(1, 7) ;
                }
                else
                {
                    Status = 1;    //不需要產生隨機數，回傳1
                }

                return Status.ToString();
            }
            catch (Exception ex)
            {
                Log.Error("問卷清單列表：" + ex.StackTrace);
                Log.Error("問卷清單列表：" + ex.Message);
                Log.Error("問卷清單列表：產生Status失敗，將其設定為1傳回。" );
                return "1";
            }
        }
        /// <summary>
        /// 依據問卷ID取得來自CRM的有效時間起訖及狀態欄位
        /// </summary>
        /// <param name="surveyId"></param>
        /// <returns></returns>
        private CRMFields GetCrmStatusBy(String surveyId)
        {
            CRMFields crm = new CRMFields();
            var sSql = " SELECT A.New_effectivestart, A.New_effectiveend, A.New_statuscode " +
                        " FROM CampaignActivityExtensionBase A " +
                        $" WHERE A.ActivityId = @surveyId ";
            //-------sql para----start
            SqlParameter[] sqlParams = new SqlParameter[] {
                new SqlParameter("@surveyId", SqlDbType.UniqueIdentifier),
                };
            sqlParams[0].Value = surveyId.ValidGuid();
            //-------sql para----end
            try
            {
                DataTable dt;
                if(AppSettingsHelper.EnvSwitchToCRM.SwitchToCRM)
                {
                    dt = CRMDbApiHelper.SurveyInfoController_GetCrmStatus(surveyId);
                }
                else
                {
                    dt = _crmDB.GetQueryData(sSql,sqlParams);
                }

                if(dt.Rows.Count>0)
                {
                    DataRow dr = dt.Rows[0];  //只有一筆
                    crm.New_effectivestart = dr["New_effectivestart"];
                    crm.New_effectiveend = dr["New_effectiveend"];
                    crm.New_statuscode = dr["New_statuscode"];
                }
                if(crm.New_statuscode==DBNull.Value||crm.New_statuscode==null)
                {
                    Log.Debug("CRM New_statuscode 空白，依據config產生Status");
                    crm.New_statuscode = GetStatusBy();
                }
                return crm;
            }
            catch (Exception ex)
            {
                Log.Error("GetCrmStatusBy：" + ex.StackTrace);
                Log.Error("GetCrmStatusBy：" + ex.Message);
                throw ex;
            }
        }

        #endregion

        #region 新增一個問卷
        /// <summary>
        /// 新增一個問卷
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
            //if(value.GetType().Name=="JArray"){}

            JObject jo = (JObject)JsonConvert.DeserializeObject(value.ToString());
            var replyData = new ReplyData();

            //UserId 必須有?
            if (jo["Uid"] == null||String.IsNullOrWhiteSpace(jo["Uid"].ToString()))
            {
                //報告錯誤
                replyData.code = "-1";
                replyData.message = $"新增一個問卷失敗！參數Uid不能為空！";
                replyData.data = "";
                Log.Error("新增問卷失敗!" + "參數Uid不能為空！");
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
            //var UpdUserId = "00000000-0000-0000-0000-000000000000";
            //UpdDateTime datetime2 yyyy-MM-dd HH:mm:ss.fffffff"
            //var UpdDateTime = DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss.fffffff");
            //Check UserId是否存在不存在需要新增
            var UserId = jo["Uid"];
            var insSql = "";
            var insRole = "";
            try
            {

                var SurveyId = jo["SurveyId"];
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
                    SurveyId = Guid.NewGuid().ToString();

                    ////報告錯誤
                    //replyData.code = "-1";
                    //replyData.message = $"新增大量選項失敗！參數SurveyId不能為空！";
                    //replyData.data = "";
                    //Log.Error("新增大量選項失敗!" + "參數SurveyId不能為空！");
                    //return JsonConvert.SerializeObject(replyData);
                }
                var Title = jo["Title"] == null ? "" : jo["Title"].ToString();
                string sSql = $"INSERT INTO QUE001_QuestionnaireBase (" +
                " SurveyId, Title, FinalUrl, ThankWords, DueAction," +
                " DelFlag,  Audit, CreateUserId, CreateDateTime, UpdUserId," +
                " UpdDateTime ) VALUES (  " +
                $" @SurveyId, @Title, '','',0," +
                $" '0', '0',@UserId, SYSDATETIME(), @UpdUserId," +
                $" SYSDATETIME())";  //UpdDateTime 為datetime2: yyyy-MM-dd HH:mm:ss.ffffffff

                //-------sql para----start
                SqlParameter[] sqlParams = new SqlParameter[] {
                new SqlParameter("@SurveyId", SqlDbType.UniqueIdentifier),
                new SqlParameter("@Title", SqlDbType.NVarChar),
                new SqlParameter("@UserId", SqlDbType.UniqueIdentifier),
                new SqlParameter("@UpdUserId", SqlDbType.UniqueIdentifier),
                };
                sqlParams[0].Value = new System.Data.SqlTypes.SqlGuid(SurveyId.ToString());
                sqlParams[1].Value = Title.Valid();
                sqlParams[2].Value = new System.Data.SqlTypes.SqlGuid(UserId.ToString());
                sqlParams[3].Value = new System.Data.SqlTypes.SqlGuid(UpdUserId.ToString());
                //-------sql para----end

                Log.Debug("新增一個問卷 " + sSql);

                var   list = new List<KeyValuePair<string, SqlParameter[]>> ();
                var obj = new KeyValuePair<string, SqlParameter[]>(sSql, sqlParams);

                list.Add(obj);
                int iR = _db.ExecuteSqlTran(list);


                //*******新增問卷時候，要一併新增結束頁資訊(QUE007) by Allen.20201005
                var EndPagePic = AppSettingsHelper.EndPageDefault.EndPagePic.ToString();
                var EndPageStyle = AppSettingsHelper.EndPageDefault.EndPageStyle.ToString();
                var ButtonSentence = AppSettingsHelper.EndPageDefault.ButtonSentence.ToString();
                var EnableRedirect = AppSettingsHelper.EndPageDefault.EnableRedirect.ToString();
                var RedirectUrl = AppSettingsHelper.EndPageDefault.RedirectUrl.ToString();
                //開始新增

                //-------sql para----start
                SqlParameter[] sqlSParams = new SqlParameter[] {
                new SqlParameter("@SurveyId", SqlDbType.UniqueIdentifier),
                new SqlParameter("@EndPagePic", SqlDbType.NVarChar),
                new SqlParameter("@EndPageStyle", SqlDbType.NVarChar),
                new SqlParameter("@ButtonSentence", SqlDbType.NVarChar),
                new SqlParameter("@EnableRedirect", SqlDbType.Bit),
                new SqlParameter("@RedirectUrl", SqlDbType.NVarChar),
                new SqlParameter("@UpdUserId", SqlDbType.UniqueIdentifier),
                };
                sqlSParams[0].Value = new System.Data.SqlTypes.SqlGuid(SurveyId.ToString());
                sqlSParams[1].Value = EndPagePic.Valid();
                sqlSParams[2].Value = EndPageStyle.Valid();
                sqlSParams[3].Value = ButtonSentence.Valid();
                sqlSParams[4].Value = EnableRedirect.ValidBit();
                sqlSParams[5].Value = RedirectUrl.Valid();
                sqlSParams[6].Value = new System.Data.SqlTypes.SqlGuid(UpdUserId.ToString());
                //-------sql para----end

                sSql = " INSERT INTO QUE007_QuestionnaireEndPage " +
                    " (SurveyId, EndPagePic, EndPageStyle, ButtonSentence, EnableRedirect, " +
                    " RedirectUrl, UpdUserId, UpdDateTime ) ";

                var vSql = $" VALUES(@SurveyId,@EndPagePic, @EndPageStyle,@ButtonSentence,@EnableRedirect," +
                        $"@RedirectUrl, @UpdUserId, SYSDATETIME())";
                sSql = string.Concat(sSql, vSql);
                Log.Debug($"設計問卷 - 結束頁 - 新增,sql={sSql}");
                iR = _db.ExecuteSql(sSql, sqlSParams);
                //**********End

                replyData.code = "200";
                replyData.message = $"新增記錄完成。";
                replyData.data = SurveyId;// ExecuteQuery($"SELECT * FROM QUE001_QuestionnaireBase WHERE SurveyId='{SurveyId}' ");

            }
            catch (Exception ex)
            {
                replyData.code = "-1";
                replyData.message = $"新增問卷失敗！{ex.Message}.";
                replyData.data = "";
                Log.Error("新增問卷失敗!" + ex.Message);
            }
            //返回
            return JsonConvert.SerializeObject(replyData);

        }
        /// <summary>
        /// 傳入UserId是否已存在
        /// </summary>
        /// <param name="UserId"></param>
        /// <returns></returns>
        private bool IsUserIdExists(object UserId)
        {
            string sSql = $"SELECT COUNT(1) FROM SSEC001_UserInfo WHERE UserId=@UserId";
            //-------sql para----start
            SqlParameter[] sqlParams = new SqlParameter[] {
                new SqlParameter("@UserId", SqlDbType.UniqueIdentifier),

                };
            sqlParams[0].Value = new System.Data.SqlTypes.SqlGuid(UserId.ToString());

            //-------sql para----end
            try
            {
                var result = _db.GetSingle(sSql, sqlParams);
                if (string.IsNullOrEmpty(result) || result == "0")
                    return false;
                else
                    return true;
            }
            catch (Exception)
            {
                throw;
            }
        }

        [Route("Delete")]
        [HttpDelete]
        public object Delete(string surveyId)
        {

            var replyData = new ReplyData();
            try
            {
                string sSql = string.Format("UPDATE QUE001_QuestionnaireBase SET DelFlag ='1' WHERE SurveyId=@surveyId AND Audit != '1'  ");

                //-------sql para----start
                SqlParameter[] sqlParams = new SqlParameter[] {
                new SqlParameter("@surveyId", SqlDbType.UniqueIdentifier),

                };
                sqlParams[0].Value = surveyId.ValidGuid();

                //-------sql para----end

                Log.Debug("更新QUE001_QuestionnaireBase: DelFlag=true" + sSql);
                int iR = _db.ExecuteSql(sSql,sqlParams);

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

        #endregion
    }

    /// <summary>
    /// 問卷清單列表, 分頁先不用 2020/8/11 by Allen/Gem
    /// </summary>
    public class SurveyPageInfo
    {
        //public Object Total { get; set; }
        //public Object CurrentPage { get; set; }
        //public Object TotalPage { get; set; }
        //public Object PerPage { get; set; }
        //public SurveyList list { get; set; }
    }
    public class SurveyList
    {
        private Object _startDate;
        private Object _endDate;
        private Object _status;
        public Object SurveyId { get; set; }
        public Object Title { get; set; }
        public Object Audit { get; set; }
        public Object IsEmpty { get; set; }
        public Object StartDate
        {
            get {
                return _startDate ?? "-/-/-";
            }
            set {
                _startDate = value;
            }
        }

        public Object EndDate
        {
            get {
                return _endDate ?? "-/-/-";
            }
            set {
                _endDate = value;
            }
        }
        public Object Status
        {
            get { return _status ?? 1; }
            set { _status = value; }
        }
        public Object StatusDesc { get; set; }

        public Object ReplyNum { get; set; }
        public Object CreateUserId { get; set; }
        public Object CreateUserName { get; set; }
        public Object UpdDateTime { get; set; }
    }
    /// <summary>
    /// 來自CRM的有效時間起迄與狀態
    /// </summary>
    public class CRMFields
    {
        /// <summary>
        /// 有效時間起
        /// </summary>
        public Object New_effectivestart { get; set; }
        /// <summary>
        /// 有效時間迄
        /// </summary>
        public Object New_effectiveend { get; set; }
        /// <summary>
        /// 狀態
        /// </summary>
        public Object New_statuscode { get; set; }

    }
}
