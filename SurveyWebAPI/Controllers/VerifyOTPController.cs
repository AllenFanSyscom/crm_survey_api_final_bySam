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
using Microsoft.AspNetCore.Routing;
using Microsoft.VisualBasic.CompilerServices;
using SurveyWebApi.Utility;
using System.Data.SqlClient;

namespace SurveyWebAPI.Controllers
{
    //[Route("api/[controller]")]
    [Route("api")]
    [ApiController]
    public class VerifyOTPController : ControllerBase
    {
        /// <summary>
        /// 問卷系統後臺--登入相關--驗證OTP
        /// </summary>
        /// <designer>Allen Fan/Gem He</design>
        /// <History> 1. 2020/08/21 Gem Create
        /// </History>
        private DBHelper _db;
        private DBHelper _crmDB;
        private bool _isValid;
        private readonly JwtHelpers jwt;
        private readonly int OTPEffectPeriod = AppSettingsHelper.OTPEffectPeriod;
        public VerifyOTPController(JwtHelpers jwt)
        {
            _db = new DBHelper(AppSettingsHelper.DefaultConnectionString);
            _crmDB = new DBHelper(AppSettingsHelper.CRMConnectionString);
            this.jwt = jwt;
        }
        [Route("VerifyOTP")]
        [HttpPost]
        public String VerifyOTP([FromBody] Object value)
        {
            /* 輸入格式：
             *{
             *    "VertifyCode": "123456",  //OTP 驗證碼
             *    "UserCode": "123",        //用戶帳號
             *    "CellPhone": "0912458976" //手機號碼
             *    "Ip":"127.0.0.1"
             *}
             */
            JObject jo = (JObject)JsonConvert.DeserializeObject(value.ToString());
            var replyData = new ReplyData();
            //VertifyCode,CellPhone,UserCode 必須有?
            if (jo["VertifyCode"] == null)
            {
                //報告錯誤
                replyData.code = "-1";
                replyData.message = $"未傳入參數VertifyCode！";
                replyData.data = "";
                Log.Error("驗證OTP失敗!" + "未傳入參數VertifyCode！");
                return JsonConvert.SerializeObject(replyData);
            }
            var VertifyCode = jo["VertifyCode"].ToString();
            if (jo["UserCode"] == null)
            {
                //報告錯誤
                replyData.code = "-1";
                replyData.message = $"未傳入參數UserCode！";
                replyData.data = "";
                Log.Error("驗證OTP失敗!" + "未傳入參數UserCode！");
                return JsonConvert.SerializeObject(replyData);
            }
            var UserCode = jo["UserCode"].ToString();
            if (jo["CellPhone"] == null)
            {
                //報告錯誤
                replyData.code = "-1";
                replyData.message = $"未傳入參數CellPhone！";
                replyData.data = "";
                Log.Error("驗證OTP失敗!" + "未傳入參數CellPhone！");
                return JsonConvert.SerializeObject(replyData);
            }
            var CellPhone = jo["CellPhone"].ToString();
            //IP 由前端傳來
            var Ip = jo["Ip"] == null ? "" : jo["Ip"].ToString();

            try
            {
                bool dummy = false;
                //20210718 SAM 拿掉111111認證
                //參數設定，OTPTest，若為true,而且傳入值為111111，則返回登入成功
                //if (AppSettingsHelper.OTPTest && AppSettingsHelper.OTPTestValue.ToString() == VertifyCode)
                //{
                //    dummy = true;
                //    Log.Debug($"驗證OTP:參數OTPTest為true，且輸入VerifyCode={AppSettingsHelper.OTPTestValue}, 視為驗證成功!");
                //}
                //else
                //{
                    //真正開始驗證

                    //檢查用戶帳號是否存在(SSEC001_UserInfo.UserCode)
                    if (!IsUserCodeExists(UserCode, CellPhone))
                    {
                        //報告錯誤
                        ErrorCode.Code = "101"; //帳號錯誤
                        replyData.code = ErrorCode.Code;
                        replyData.message = ErrorCode.Message;
                        //replyData.code = "-1";
                        //replyData.message = $"用戶帳號'{UserCode}'不存在！";
                        replyData.data = "";
                        Log.Error("驗證OTP失敗!" + $"用戶帳號'{UserCode}'不存在！");
                        return JsonConvert.SerializeObject(replyData);
                    }
                    //驗證otp (使用Utility\Totp.cs\ValidateTotp)
                    //securityToken=手機號碼+用戶帳號
                    byte[] securityToken = System.Text.Encoding.Default.GetBytes(String.Concat(CellPhone, UserCode));
                    //_isValid = Totp.ValidateTotp(securityToken, Convert.ToInt32(VertifyCode));
                    //改版檢核OTP方式=>直接查詢db.userinfo
                    _isValid = CheckIsVaildOTP(CellPhone, UserCode , VertifyCode);
                    if (!_isValid)
                    {
                        //驗證不通過
                        // 回傳結果
                        ErrorCode.Code = "103";
                        replyData.code = ErrorCode.Code;
                        replyData.message = ErrorCode.Message;
                        //replyData.code = "-1";
                        //replyData.message = $"驗證OTP失敗。";
                        replyData.data = "";
                        Log.Error($"驗證OTP失敗！CellPhone='{CellPhone}', UserCode='{UserCode}',result={_isValid}");
                        return JsonConvert.SerializeObject(replyData);
                    }
                //}

                //Token改取JWT by Allen 20201006
                //產生TOKEN(GUID)
                //var Token = Guid.NewGuid().ToString("D");
                //寫入DB(System_Token)
                //int i = WriteToken2DB(CellPhone, UserCode, Ip, Token);
                //回傳執行結果
                TokenInfo ti = new TokenInfo();
                ti.Token = dummy ? jwt.GenerateToken(UserCode) : jwt.GenerateToken(UserCode); //Token;

                //還是得寫入DB(System_Token)，因為要只能從SYS001中取到User的LastLoginDateTime
                int i = WriteToken2DB(CellPhone, UserCode, Ip);

                DataTable dtR = GetUserInfoBy(UserCode, CellPhone);
                if (dtR.Rows.Count > 0)
                {
                    DataRow row = dtR.Rows[0];
                    ti.UserId = row["UserId"];
                    ti.UserName = row["UserName"];
                    ti.RoleId = row["RoleId"];
                    ti.RoleName = row["RoleName"];
                    ti.UserCode = row["UserCode"];
                }
                //從SSEC001_UserInfo取得UserName/UserCode/Telephone請改取CHT_MSCRM的SystemUserBase裡面的FullName/EmployeeId/MobilePhone
                DataTable dtUserFromCRM;
                if (AppSettingsHelper.EnvSwitchToCRM.SwitchToCRM)
                {
                    dtUserFromCRM = CRMDbApiHelper.VerifyOTPController_GetCRMUserInfo(UserCode, CellPhone);
                }
                else
                {
                    dtUserFromCRM = GetCRMUserInfoBy(UserCode, CellPhone);
                }
                if (dtUserFromCRM.Rows.Count > 0)
                {
                    //CRM DB裡面的user資訊最新，所以要從CRM取
                    DataRow row = dtUserFromCRM.Rows[0];
                    ti.UserId = row["UserId"];
                    ti.UserName = row["UserName"];
                }
                // 回傳結果
                replyData.code = "200";
                replyData.message = $"驗證OTP成功。";
                replyData.data = ti; //new TokenInfo() { Token = Token };
                Log.Debug($"驗證OTP成功！CellPhone='{CellPhone}', UserCode='{UserCode}',TOKEN='{ti.Token}'");
                return JsonConvert.SerializeObject(replyData);
            }
            catch (Exception ex)
            {
                //報告錯誤
                replyData.code = "-1";
                replyData.message = ex.Message;
                replyData.data = "";
                Log.Error("驗證OTP失敗!" + ex.Message);
                return JsonConvert.SerializeObject(replyData);
            }
        }
        /// <summary>
        /// 傳入UserCode是否已存在
        /// </summary>
        /// <param name="UserId"></param>
        /// <param name="cellPhone"></param>
        /// <returns></returns>
        private bool IsUserCodeExists(String userCode, String cellPhone)
        {

            string sSql = $"SELECT COUNT(1) FROM SSEC001_UserInfo WHERE UserCode=@userCode AND Telephone=@cellPhone ";


            //-------sql para----start
            SqlParameter[] sqlParams = new SqlParameter[] {
                    new SqlParameter("@userCode", SqlDbType.VarChar),
                    new SqlParameter("@cellPhone", SqlDbType.VarChar)
                };
            sqlParams[0].Value = userCode.Valid();
            sqlParams[1].Value = cellPhone.Valid();
            //-------sql para----end

            //特殊用戶不會有手機號碼
            List<string> specificUsers = AppSettingsHelper.SpecificUsers.ToString().Split(';').ToList();
            if (specificUsers.Contains(userCode.ToLower())) //使用特殊帳號，是沒有手機號碼的。
            {
                sSql = $"SELECT COUNT(1) FROM SSEC001_UserInfo WHERE UserCode=@userCode ";


            }

            try
            {
                Log.Debug("VerifyOTP, IsUserCodeExists sql:" + sSql);
                var result = _db.GetSingle(sSql, sqlParams);
                if (string.IsNullOrEmpty(result) || result == "0")
                    return false;
                else
                    return true;
            }
            catch (Exception ex)
            {
                Log.Error("OTP: Query SEC001 by UserCode fail！" + ex.Message);
                Log.Error(ex.StackTrace);
                throw ex;
            }
        }
        private DataTable GetCRMUserInfoBy(String userCode, String cellPhone)
        {
            string sSql = $" SELECT TOP 1  SystemUserId AS UserId, FullName AS UserName " +
                " FROM SystemUserBase " +
                $" WHERE EmployeeId=@userCode AND MobilePhone=@cellPhone ";

            //-------sql para----start
            SqlParameter[] sqlParams = new SqlParameter[] {
                    new SqlParameter("@userCode", SqlDbType.VarChar),
                    new SqlParameter("@cellPhone", SqlDbType.VarChar)
                };
            sqlParams[0].Value = userCode.Valid();
            sqlParams[1].Value = cellPhone.Valid();
            //-------sql para----end

            try
            {
                Log.Debug("VerifyOTP, GetCRMUserInfoBy sql:" + sSql);
                return _crmDB.GetQueryData(sSql, sqlParams);
            }
            catch (Exception ex)
            {
                Log.Error("VerifyOTP: GetUserInfoBy fail！" + ex.Message);
                Log.Error(ex.StackTrace);
                throw ex;
            }
        }
        private DataTable GetUserInfoBy(String userCode, String cellPhone)
        {
            //除UserId外， UserCode，UserName等在SEC001中可能不是最新，要去最新User資訊，需要去CRM.SystemUserBase
            string sSql = $" SELECT TOP 1  A.UserId, A.UserName, B.RoleId, C.RoleName,A.UserCode "+
                " FROM SSEC001_UserInfo A " +
                " LEFT JOIN SSEC005_UserRole B ON B.UserId = A.UserId AND B.UsedMark = '1' " +
                " LEFT JOIN SSEC004_RoleId C ON C.RoleId = B.RoleId AND C.UsedMark = '1' " +
                $" WHERE A.UserCode=@userCode AND A.Telephone=@cellPhone AND A.UsedMark = '1' ";


            //-------sql para----start
            SqlParameter[] sqlParams = new SqlParameter[] {
                    new SqlParameter("@userCode", SqlDbType.VarChar),
                    new SqlParameter("@cellPhone", SqlDbType.VarChar)
                };
            sqlParams[0].Value = userCode.Valid();
            sqlParams[1].Value = cellPhone.Valid();
            //-------sql para----end

            //特殊用戶不會有手機號碼
            List<string> specificUsers = AppSettingsHelper.SpecificUsers.ToString().Split(';').ToList();
            if (specificUsers.Contains(userCode.ToLower())) //使用特殊帳號，是沒有手機號碼的。
            {
                sSql = $" SELECT TOP 1  A.UserId, A.UserName, B.RoleId, C.RoleName ,A.UserCode" +
                " FROM SSEC001_UserInfo A " +
                " LEFT JOIN SSEC005_UserRole B ON B.UserId = A.UserId AND B.UsedMark = '1' " +
                " LEFT JOIN SSEC004_RoleId C ON C.RoleId = B.RoleId AND C.UsedMark = '1' " +
                $" WHERE A.UserCode=@userCode AND A.UsedMark = '1' ";
            }

            try
            {
                Log.Debug("VerifyOTP, VerifyOTP sql:" + sSql);
                return _db.GetQueryData(sSql,sqlParams);
            }
            catch (Exception ex)
            {
                Log.Error("VerifyOTP: GetUserInfoBy fail！" + ex.Message);
                Log.Error(ex.StackTrace);
                throw ex;
            }
        }

        /// <summary>
        /// 將生成的Token寫入SYS001
        /// </summary>
        /// <param name="cellPhone"></param>
        /// <param name="userCode"></param>
        /// <returns></returns>
        private int WriteToken2DB(string cellPhone, string userCode, string ip)
        {
            int i = -1;
            try
            {
                //傳入的這個token格式不對，寫不到db，所以用GUID即可，SYS001僅僅為了去LastLoginDateTime，所以，其他欄位沒關係
                string token = Guid.NewGuid().ToString("D");

                //特殊帳號如crmop1/administrator沒有手機號碼，沒法進行資料比對，直接先取問卷平台
                var sSql = $"SELECT TOP 1 UserId, UserName FROM SSEC001_UserInfo " +
                $" WHERE UserCode=@userCode AND Telephone=@cellPhone";


                //-------sql para----start
                SqlParameter[] sqlParams = new SqlParameter[] {
                    new SqlParameter("@userCode", SqlDbType.VarChar),
                    new SqlParameter("@cellPhone", SqlDbType.VarChar)
                };
                sqlParams[0].Value = userCode.Valid();
                sqlParams[1].Value = cellPhone.Valid();
                //-------sql para----end

                DataTable dtR;

                dtR = _db.GetQueryData(sSql, sqlParams) ;

                if (dtR.Rows.Count > 0)
                {
                    DataRow dr = dtR.Rows[0];
                    var UserId = dr["UserId"].ToString();
                    var UserName = dr["UserName"].ToString();
                    sSql = " INSERT INTO SYS001_SystemToken (Token, UserId, UserName, Ip,ExpiredDate, UpdDate) VALUES " +
                        $"(@token, @UserId,@UserName,@ip,SYSDATETIME(),SYSDATETIME())";

                    //-------sql para----start
                    SqlParameter[] sql1Params = new SqlParameter[] {
                    new SqlParameter("@token", SqlDbType.VarChar),
                    new SqlParameter("@UserId", SqlDbType.UniqueIdentifier),
                    new SqlParameter("@UserName", SqlDbType.VarChar),
                    new SqlParameter("@ip", SqlDbType.VarChar),
                };
                    sql1Params[0].Value = token.Valid();
                    sql1Params[1].Value = UserId.ValidGuid();
                    sql1Params[2].Value = UserName.Valid();
                    sql1Params[3].Value = ip.Valid();
                    //-------sql para----end

                    i = _db.ExecuteSql(sSql, sql1Params);
                    return i;
                }
                return 0;
            }
            catch (Exception ex)
            {
                Log.Error("Write Token 2 Database fail！" + ex.Message);
                Log.Error(ex.StackTrace);
                throw ex;
            }
        }

        //20210718 sam 新增改版OTP檢核方式
        private bool CheckIsVaildOTP(string cellPhone, string userCode,string otpCode)
        {

            try
            {
                //datetime2 SYSDATETIME()
                var nowDateTime = DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss.fffffff");
                var sSql = $" SELECT * from  SSEC001_UserInfo " +
                $" WHERE Telephone=@cellPhone AND UserCode=@userCode AND  OTP=@otpCode and  @nowDateTime BETWEEN  OTPTime and DATEADD(second, {OTPEffectPeriod}, OTPTime)";


                //-------sql para----start
                SqlParameter[] sqlParams = new SqlParameter[] {
                    new SqlParameter("@cellPhone", SqlDbType.VarChar),
                    new SqlParameter("@userCode", SqlDbType.VarChar),
                    new SqlParameter("@otpCode", SqlDbType.VarChar),
                    new SqlParameter("@nowDateTime", SqlDbType.DateTime),
                };
                sqlParams[0].Value = cellPhone.Valid();
                sqlParams[1].Value = userCode.Valid();
                sqlParams[2].Value = otpCode.Valid();
                sqlParams[3].Value = nowDateTime.Valid();
                //-------sql para----end

                DataTable dtR;
                dtR = _db.GetQueryData(sSql, sqlParams);
                if(dtR.Rows.Count>0)
                    return true;
                else
                    return false;
            }
            catch (Exception ex)
            {
                Log.Error("Query OTP 2 Database fail！" + ex.Message);
                Log.Error(ex.StackTrace);
                throw ex;
            }
        }

    }
    public class TokenInfo
    {
        /// <summary>
        /// Token
        /// </summary>
        public String Token { get; set; }
        /// <summary>
        /// User Id
        /// </summary>
        public Object UserId { get; set; }
        /// <summary>
        /// User Name
        /// </summary>
        public Object UserName { get; set; }
        /// <summary>
        /// User's Role Id
        /// </summary>
        public Object RoleId { get; set;}
        /// <summary>
        /// User's Role Name
        /// </summary>
        public Object RoleName { get; set; }


        public Object UserCode { get; set; }
    }
}
