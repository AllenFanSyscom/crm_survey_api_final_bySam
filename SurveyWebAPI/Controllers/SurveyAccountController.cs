using System;
using System.Reflection;
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
using SurveyWebApi.Utility;
using System.Data.SqlClient;

namespace SurveyWebAPI.Controllers
{
    [Authorize]
    [Route("api/Survey/Account/")]
    [ApiController]
    public class SurveyAccountController : Controller
    {
        /// <summary>
        /// 問卷系統後臺之主畫面操作--帳號列表API
        /// </summary>
        /// <designer>Allen Fan/Gem He</design>
        /// <History> 1. 2020/08/06 Gem Create
        /// </History>


        //private readonly ILogger<QUE004_QuestionnaireSettingController> _logger;  //可實現log在console輸出
        private DBHelper _db;
        private DBHelper _crmDB;
        //publicQUE004_QuestionnaireSettingController(ILogger<QUE004_QuestionnaireSettingController> logger)
        public SurveyAccountController()
        {
            //_logger = logger;
            //_db = new DBHelper();
            //_logger = logger;
            //_connectionOptions = options.Value;
            _db = new DBHelper(AppSettingsHelper.DefaultConnectionString);
            _crmDB = new DBHelper(AppSettingsHelper.CRMConnectionString);
        }
        [Route("queryByPage")]
        [HttpGet]
        public String QueryByPage()
        {
            Log.Debug("主畫面操作-帳號列表...");
            /*
             * 用SSEC001_UserInfo去left join SSEC004_RoleId取得角色
             * SYS001_SystemToken.UpdDate 可以抓到最後登入時間
             * CreateDateTime 抓SSEC001_UserInfo.UpdDateTime
             * 返回：   "UserId": "",
             *          "RoleName": "管理員",
             *          "CreateDateTime": "",
             *          "LastLogInDateTime": ""
             */
            List<AccountInfo> lstacntInfo = new List<AccountInfo>();
            ReplyData replyData = new ReplyData();
            string sSql = " SELECT A.UserId, A.UserCode, A.UserName, C.RoleId, C.RoleName, A.UpdDateTime as CreateDateTime, D.UpdDate AS LastLogInDateTime " +
                          " FROM SSEC001_UserInfo A "+
                          " LEFT JOIN SSEC005_UserRole B ON B.UserId = A.UserId "+
                          " LEFT JOIN SSEC004_RoleId C ON C.RoleId = B.RoleId "+
                          " LEFT JOIN " +
                          " (" +
                          "   SELECT UserId, UpdDate, ROW_NUMBER() OVER (PARTITION BY UserId ORDER BY UpdDate DESC) rn "+
                          "   FROM SYS001_SystemToken " +
                          " ) D ON D.UserId = A.UserId  AND D.rn=1 order by D.UpdDate desc, A.UpdDateTime desc";
            try
            {
                DataTable dtR = _db.GetQueryData(sSql);
                foreach (DataRow dr in dtR.Rows)
                {
                    AccountInfo acntInfo = new AccountInfo();
                    acntInfo.UserId = dr["UserId"];//.ToString();
                    acntInfo.UserCode = dr["UserCode"];//.ToString();
                    acntInfo.UserName = dr["UserName"];//.ToString();
                    acntInfo.RoleId = dr["RoleId"];//.ToString();
                    acntInfo.RoleName = dr["RoleName"];//.ToString();
                    acntInfo.CreateDateTime = dr["CreateDateTime"];//.ToString();
                    acntInfo.LastLogInDateTime = dr["LastLogInDateTime"];//.ToString();
                    //User的資訊CRM DB中是最新的，所以，改由CRM取
                    DataTable dtCRM;
                    if(AppSettingsHelper.EnvSwitchToCRM.SwitchToCRM)
                    {
                        dtCRM = CRMDbApiHelper.SurveyAccountController_GetCRMUserInfo(dr["UserId"].ToString().Trim());
                    }
                    else
                    {
                        dtCRM = GetCRMUserInfoBy(dr["UserId"].ToString().Trim());
                    }
                    
                    if (dtCRM.Rows.Count > 0)
                    {
                        acntInfo.UserCode = dtCRM.Rows[0]["UserCode"];//.ToString();
                        acntInfo.UserName = dtCRM.Rows[0]["UserName"];//.ToString();
                    }
                    lstacntInfo.Add(acntInfo);
                }
                replyData.code = "200";
                replyData.message = $"資料取得成功。共{lstacntInfo.Count}筆。";
                Log.Debug($"資料取得成功。共{lstacntInfo.Count}筆。");
                //先不要SerializeObject list 應該也可以
                replyData.data = lstacntInfo;  // JsonConvert.SerializeObject(lstBaseicSetting);
            }
            catch (Exception ex)
            {
                replyData.code = "-1";
                replyData.message = $"資料取得失敗！{ex.Message}.";
                replyData.data = "";
                Log.Error("查詢帳號列表失敗!" + ex.Message);
            }
            //返回結果
            return JsonConvert.SerializeObject(replyData);
            //return lstUserInfo.ToArray();
        }
        private DataTable GetCRMUserInfoBy(String userId)
        {
            string sSql = $" SELECT SystemUserId AS UserId, FullName AS UserName, EmployeeId AS UserCode, MobilePhone AS Telephone " +
                " FROM SystemUserBase " +
                $" WHERE SystemUserId=@userId ";

            //-------sql para----start
            SqlParameter[] sqlParams = new SqlParameter[] {
                    new SqlParameter("@userId", SqlDbType.UniqueIdentifier),
            };
            sqlParams[0].Value = userId.ValidGuid(); 

            //-------sql para----end

            try
            {
                Log.Debug("api/Survey/Account/queryByPage, GetCRMUserInfoBy sql:" + sSql);
                return _crmDB.GetQueryData(sSql,sqlParams);
            }
            catch (Exception ex)
            {
                Log.Error("api/Survey/Account/queryByPage: GetUserInfoBy fail！" + ex.Message);
                Log.Error(ex.StackTrace);
                throw ex;
            }
        }
    }
    public class AccountInfo
    {
        public Object UserId { get; set; }
        public Object UserCode { get; set; }
        public Object UserName { get; set; }
        public Object RoleId { get; set; }
        public Object RoleName { get; set; }
        public Object CreateDateTime { get; set; }
        public Object LastLogInDateTime { get; set; }
    }
}
