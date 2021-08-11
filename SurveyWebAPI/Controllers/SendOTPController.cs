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
using System.Text;

namespace SurveyWebAPI.Controllers
{
    [Route("api")]
    [ApiController]
    public class SendOTPController : ControllerBase
    {
        /// <summary>
        /// 問卷系統後臺--登入相關--發送OTP
        /// </summary>
        /// <designer>Allen Fan/Gem He</design>
        /// <History> 1. 2020/08/21 Gem Create
        /// </History>
        private DBHelper _db;
        private DBHelper _crmDB;
        private string _otpCode;
        public SendOTPController()
        {
            _db = new DBHelper(AppSettingsHelper.DefaultConnectionString);
            _crmDB = new DBHelper(AppSettingsHelper.CRMConnectionString);
        }
        [Route("SendOTP")]
        [HttpPost]
        public String SendOTP([FromBody] Object value)
        {
            /* 輸入格式：
             * {
             *   "CellPhone":"0912123456",  //手機號碼
             *   "UserCode":"ABC"           //用戶帳號
             *}
             */
            JObject jo = (JObject)JsonConvert.DeserializeObject(value.ToString());
            var replyData = new ReplyData();
            //CellPhone,UserCode 必須有?
            if (jo["CellPhone"] == null)
            {
                //報告錯誤
                replyData.code = "-1";
                replyData.message = $"未傳入參數CellPhone！";
                replyData.data = "";
                Log.Error("發送OTP失敗!" + "未傳入參數CellPhone！");
                return JsonConvert.SerializeObject(replyData);
            }
            var CellPhone = jo["CellPhone"].ToString();
            var Cellphone = Base64Decrypt(CellPhone);
            if (jo["UserCode"] == null)
            {
                //報告錯誤
                replyData.code = "-1";
                replyData.message = $"未傳入參數UserCode！";
                replyData.data = "";
                Log.Error("發送OTP失敗!" + "未傳入參數UserCode！");
                return JsonConvert.SerializeObject(replyData);
            }
            var UserCode = jo["UserCode"].ToString();
            //這個API是登入的API，HEADER不會傳TOKEN，所以抓不到USER DATA,而是透過傳入參數進行用戶資料檢核 by Allen

            try
            {
                //檢查用戶帳號是否存在(SSEC001_UserInfo.UserCode)
                var ErrCode = IsUserCodeExists(UserCode, CellPhone);
                if (ErrCode != "0")
                {
                    //報告錯誤
                    ErrorCode.Code = ErrCode;
                    replyData.code = ErrorCode.Code;
                    replyData.message = ErrorCode.Message;
                    replyData.data = null;
                    Log.Error("發送OTP失敗!" + $"用戶帳號【{UserCode}】【{CellPhone}】不存在！");
                    return JsonConvert.SerializeObject(replyData);
                }

                //使用(securityToken=手機號碼+用戶帳號)產生OTP驗證碼(使用Utility\Totp.cs\GenerateTotp)
                byte[] securityToken = System.Text.Encoding.Default.GetBytes(String.Concat(CellPhone, UserCode));
                //舊用法換掉 by sam 20210720
                //_otpCode = Totp.GenerateTotp(securityToken).ToString().PadLeft(6,'0');
                Random rnd = new Random();
                _otpCode = rnd.Next(100000, 999999).ToString();
                // Allen說：需要發送成功後，才可更新DB
                // 發送簡訊 (要設定config，決定是否要發簡訊)
                if (Common.AppSettingsHelper.NeedSendOtpMessage && !CellPhone.Equals(AppSettingsHelper.SpecificCellPhone.ToString()))
                {
                    //簡訊文案: 您的OTP驗證碼為123456
                    var OTPMessage = $"您的OTP驗證碼為{_otpCode}";
                    Log.Debug($"發送OTP簡訊： UserCode='{UserCode}',CellPhone='{CellPhone}',OTP='{_otpCode}'");
                    //call 發送程式,預留
                   if(!SMSSender.SendSMS(CellPhone, OTPMessage).ReturnCode.Equals("200"))
                    {
                        //報告錯誤
                        ErrorCode.Code = "104";
                        replyData.code = ErrorCode.Code;
                        replyData.message = ErrorCode.Message;
                        //replyData.code = "-1";
                        //replyData.message = $"發送OTP簡訊失敗！";// + messageMsg;
                        replyData.data = "";
                        Log.Error("發送OTP失敗!" + replyData.message);
                        return JsonConvert.SerializeObject(replyData);
                    }
                   Log.Debug($"向[{CellPhone}]發送OTP簡訊[{OTPMessage}]成功!");
                }
                else
                {
                    Log.Debug("Config設定為不需要發送OTP簡訊。");
                    //直接更新DB
                }

                // 更新OTP驗證碼到(SSEC001_UserInfo.OTP)
                //UpdUserId 會有程式依據Token取得，所以,目前暫時寫成00000000-0000-0000-0000-000000000000
                //var UpdUserId = "00000000-0000-0000-0000-000000000000";
                // 這個API是登入的API，HEADER不會傳TOKEN，所以抓不到USER DATA
                //  而是透過傳入參數進行用戶資料檢核
                var UpdUserId = GetUserIdBy(UserCode, CellPhone);
                int i = WriteOTPCode2DB(CellPhone, UserCode, UpdUserId);
                if(i<1)
                {
                    //udpate DB 失敗
                }
                // 回傳結果
                replyData.code = "200";
                replyData.message = $"發送OTP成功。";
                replyData.data = "";
                Log.Debug($"發送OTP成功！CellPhone='{CellPhone}', UserCode='{UserCode}',OTP={_otpCode}");
                return JsonConvert.SerializeObject(replyData);
            }
            catch(Exception ex)
            {
                //報告錯誤
                replyData.code = "-1";
                replyData.message = ex.Message;
                replyData.data = "";
                Log.Error("發送OTP失敗!" + ex.Message);
                return JsonConvert.SerializeObject(replyData);
            }
        }
        private String GetUserIdBy(String userCode, String cellPhone)
        {
            string sSql = $"SELECT UserId FROM SSEC001_UserInfo WHERE UserCode=@userCode AND  Telephone=@cellPhone ";


            //-------sql para----start
            SqlParameter[] sqlParams = new SqlParameter[] {
                new SqlParameter("@userCode", SqlDbType.VarChar),
                new SqlParameter("@cellPhone", SqlDbType.VarChar),

                };
            sqlParams[0].Value = userCode.Valid();
            sqlParams[1].Value = cellPhone.Valid();
            //-------sql para----end

            try
            {
                DataTable dtR = _db.GetQueryData(sSql, sqlParams);
                if (dtR.Rows.Count < 1)
                {
                    Log.Debug($"依據UserCode:[{userCode}],Telephone:[{cellPhone}]找不到UserId,置UserId=[00000000-0000-0000-0000-000000000000]");
                    return "00000000-0000-0000-0000-000000000000";
                }
                else if (dtR.Rows[0]["UserId"] == DBNull.Value)
                {
                    Log.Debug($"依據UserCode:[{userCode}],Telephone:[{cellPhone}]找不到UserId,置UserId=[00000000-0000-0000-0000-000000000000]");
                    return "00000000-0000-0000-0000-000000000000";
                }
                else
                {
                    Log.Debug($"SendOTP:傳入UserCode='{userCode}',cellPhone='{cellPhone}';DB UserId={dtR.Rows[0]["UserId"].ToString()}.");
                    return dtR.Rows[0]["UserId"].ToString();
                }

            }
            catch (Exception ex)
            {
                Log.Error("OTP: Query SEC001 by UserCode fail！" + ex.Message);
                Log.Error(ex.StackTrace);
                throw ex;
            }
        }
        /// <summary>
        /// 傳入UserCode是否已存在
        /// </summary>
        /// <param name="UserId"></param>
        /// <returns></returns>
        private string IsUserCodeExists(String userCode, String cellPhone)
        {
            string sSql = $"SELECT UserId,Telephone,PwdErrorTime,LoginDate, SYSDATETIME() as NowTime FROM SSEC001_UserInfo WHERE UserCode=@userCode ";

            //-------sql para----start
            SqlParameter[] sqlParams = new SqlParameter[] {
                new SqlParameter("@userCode", SqlDbType.VarChar),

                };
            sqlParams[0].Value = userCode.Valid();
            //-------sql para----end
            try
            {
                DataTable dtR = _db.GetQueryData(sSql, sqlParams);


                if (dtR.Rows.Count < 1)
                {
                    return "101";  //賬號錯誤
                }
                else
                {
                    int errorTimes = Convert.ToInt32(String.IsNullOrEmpty(dtR.Rows[0]["PwdErrorTime"].ToString()) ? "0" : dtR.Rows[0]["PwdErrorTime"].ToString());
                    DateTime loginDate = Convert.ToDateTime(String.IsNullOrEmpty(dtR.Rows[0]["LoginDate"].ToString()) ? DateTime.Now.ToString() : dtR.Rows[0]["LoginDate"].ToString());
                    DateTime nowDate = Convert.ToDateTime(String.IsNullOrEmpty(dtR.Rows[0]["NowTime"].ToString()) ? DateTime.Now.ToString() : dtR.Rows[0]["NowTime"].ToString());

                    TimeSpan ts = nowDate - loginDate;
                    //帳號鎖定
                    if (errorTimes >= 3 && ts.TotalMinutes <= AppSettingsHelper.ErrorLockTime) //輸入錯誤三次，鎖定帳號30分鐘
                    {
                        Log.Debug($"輸入電話超過三次!" + $"UserName:{userCode}，Telephone:{cellPhone}，錯誤次數:{errorTimes}，鎖定時間:{ts.TotalMinutes}");
                        return "105";
                    }
                    else //帳號沒有鎖定
                    {
                        //檢核電話正確性
                        return IsPhoneNumberValid(userCode, cellPhone, dtR.Rows[0]["UserId"].ToString(), dtR.Rows[0]["Telephone"].ToString(), dtR.Rows[0]["PwdErrorTime"] == DBNull.Value ? 0 : Convert.ToInt32(dtR.Rows[0]["PwdErrorTime"].ToString()));
                    }
                }
            }
            catch (Exception ex)
            {
                Log.Error("OTP: Query SEC001 by UserCode fail！" + ex.Message);
                Log.Error(ex.StackTrace);
                throw ex;
            }
        }

        /// <summary>
        /// 檢核手機號碼的正確性
        /// </summary>
        /// <param name="userCode">帳號</param>
        /// <param name="inputPhone">輸入手機號碼</param>
        /// <param name="userId">uid</param>
        /// <param name="surveyPhone">問卷DB儲存的手機號碼</param>
        /// <param name="pwdErrorTime">錯誤次數</param>
        /// <returns></returns>
        private string IsPhoneNumberValid(string userCode,string inputPhone,string userId,string surveyPhone, int pwdErrorTime)
        {
            string sSql = "";
            //帳號沒鎖定，進行以下測試。
            if (String.IsNullOrEmpty(surveyPhone)) //問卷DB取得的手機號碼為空
            {
                //取不到手機號碼，要檢查是否為特殊用戶，特殊用戶要用特殊密碼
                List<string> specificUsers = AppSettingsHelper.SpecificUsers.ToString().Split(';').ToList();
                if (specificUsers.Contains(userCode.ToLower()))  //屬於特殊帳號
                {
                    if (inputPhone.Equals(AppSettingsHelper.SpecificCellPhone.ToString()))
                    {
                        try
                        {
                            sSql = $" Update SSEC001_UserInfo " +
                                $" SET PwdErrorTime=0,LoginDate=SYSDATETIME() " +
                                $" WHERE UserCode=@userCode ";

                            //-------sql para----start
                            SqlParameter[] sqlParams = new SqlParameter[] {
                                new SqlParameter("@userCode", SqlDbType.VarChar),

                            };
                            sqlParams[0].Value = userCode.Valid();
                            //-------sql para----end
                            _db.ExecuteSql(sSql, sqlParams);

                            Log.Debug($"特殊用戶登入驗證成功!" + $"userId:{userId}，inputPhone:{inputPhone}");
                        }
                        catch (Exception ex)
                        {
                            Log.Debug($"特殊用戶登入驗證異常!" + $"userId:{userId}，inputPhone:{inputPhone}，Message:{ex.Message}，SQL:{sSql}");
                        }
                        return "0";  //符合特殊用戶與特殊手機號碼，可通過驗證
                    }
                    else
                    {

                        UpdateErrorTimes(userCode, pwdErrorTime);

                        Log.Debug($"特殊用戶手機輸入錯誤!" + $"userId:{userId}，inputPhone:{inputPhone}");
                        return "102";  //手機錯誤
                    }

                }
                else
                {
                    //非特殊用戶，CRM數據庫用戶手機不可為空
                    try
                    {

                        UpdateErrorTimes(userCode, pwdErrorTime);

                        Log.Debug($"CRM數據庫用戶手機為空!" + $"UserId:{userId}，用戶輸入Telephone:{inputPhone}");

                    }
                    catch (Exception ex)
                    {
                        Log.Debug($"CRM數據庫用戶手機為空，更新錯誤次數異常!" + $"UserId:{userId}，用戶輸入Telephone:{inputPhone}，Exception:{ex.Message}，SQL:{sSql}");

                    }
                    return "102";
                }
            }
            else //手機號碼不為空
            {
                DataTable dtCRMUser;
                //從CRM取得資料。
                if (AppSettingsHelper.EnvSwitchToCRM.SwitchToCRM)
                {
                    dtCRMUser = CRMDbApiHelper.SystemController_GetCRMUserInfo(userId);
                }
                else
                {
                    dtCRMUser = GetCRMUserInfoBy(userId);
                }
                //檢查CRM是否存在用戶訊息。
                if (dtCRMUser.Rows.Count < 1)
                {
                    Log.Debug($"CRM平台不存在用戶資訊!" + $"userId:{userId}，Telephone:{inputPhone}");
                    return "101";
                }
                DataRow drCRM = dtCRMUser.Rows[0];
                string crmTel = drCRM["Telephone"].ToString();
                string crmUserCode = drCRM["UserCode"].ToString();
                if (!crmTel.Trim().Equals(inputPhone.Trim())) //手機錯誤
                {
                    try
                    {

                        UpdateErrorTimes(crmUserCode, pwdErrorTime);

                        Log.Debug($"手機號碼驗證不正確!" + $"UserId:{userId}，用戶輸入Telephone:{inputPhone}");

                    }
                    catch (Exception ex)
                    {
                        Log.Debug($"手機號碼驗證不正確，更新錯誤次數異常!" + $"UserId:{userId}，用戶輸入Telephone:{inputPhone}，Exception:{ex.Message}，SQL:{sSql}");

                    }
                    return "102";
                }
                else  //手機正確
                {
                    try
                    {
                        sSql = $" Update SSEC001_UserInfo " +
                            $" SET PwdErrorTime=0, LoginDate=SYSDATETIME() " +
                            $" WHERE UserCode=@userCode ";

                        //-------sql para----start
                        SqlParameter[] sqlParams = new SqlParameter[] {
                                new SqlParameter("@userCode", SqlDbType.VarChar),

                            };
                        sqlParams[0].Value = crmUserCode.Valid();
                        //-------sql para----end


                        _db.ExecuteSql(sSql, sqlParams);
                        Log.Debug($"手機號碼驗證正確!" + $"UserId:{userId}，用戶輸入Telephone:{inputPhone}");

                    }
                    catch (Exception ex)
                    {
                        Log.Debug($"手機號碼驗證正確，更新錯誤次數異常!" + $"UserId:{userId}，用戶輸入Telephone:{inputPhone}，Exception:{ex.Message}，SQL:{sSql}");

                    }
                    return "0";
                }
            }

        }

        private string UpdateErrorTimes(string userCode, int pwdErrorTime)
        {
            string sSql = "";
            pwdErrorTime++;
            if (pwdErrorTime > 3)
            {
                //鎖定解除才會進到這個條件，鎖定解除後又失敗，錯誤次數設定為1
                sSql = $" Update SSEC001_UserInfo " +
                $" SET PwdErrorTime=1, LoginDate=SYSDATETIME() " +
                $" WHERE UserCode=@userCode ";

                //-------sql para----start
                SqlParameter[] sqlParams = new SqlParameter[] {
                    new SqlParameter("@userCode", SqlDbType.VarChar),

                };
                sqlParams[0].Value = userCode.Valid();
                //-------sql para----end
                _db.ExecuteSql(sSql,sqlParams);
            }
            else
            {
                sSql = $" Update SSEC001_UserInfo " +
                $" SET PwdErrorTime=@pwdErrorTime, LoginDate=SYSDATETIME() " +
                $" WHERE UserCode=@userCode ";

                //-------sql para----start
                SqlParameter[] sqlParams = new SqlParameter[] {
                    new SqlParameter("@userCode", SqlDbType.VarChar),
                    new SqlParameter("@pwdErrorTime", SqlDbType.Int),

                };
                sqlParams[0].Value = userCode.Valid();
                sqlParams[1].Value = pwdErrorTime.ValidInt();
                //-------sql para----end
                _db.ExecuteSql(sSql, sqlParams);

            }



            Log.Debug($"UpdateErrorTimes:{sSql}");
            return sSql;

        }


        private DataTable GetCRMUserInfoBy(String userId)
        {
            string sSql = $" SELECT SystemUserId AS UserId, FullName AS UserName, SUBSTRING(DomainName,CHARINDEX('\\',DomainName)+1,LEN(DomainName)-CHARINDEX('\\',DomainName)) AS UserCode, MobilePhone AS Telephone " +
                " FROM SystemUserBase " +
                $" WHERE SystemUserId=@userId  ";

            //-------sql para----start
            SqlParameter[] sqlParams = new SqlParameter[] {
                    new SqlParameter("@userId", SqlDbType.UniqueIdentifier),

                };
            sqlParams[0].Value = userId.ValidGuid();
            //-------sql para----end

            try
            {
                Log.Debug("SSO登入取得token, GetCRMUserInfoBy sql:" + sSql);
                return _crmDB.GetQueryData(sSql,sqlParams);
            }
            catch (Exception ex)
            {
                Log.Error("SSO登入取得token: GetUserInfoBy fail！" + ex.Message);
                Log.Error(ex.StackTrace);
                throw ex;
            }
        }

        /// <summary>
        /// 將生成的OTP碼寫入SEC001.OTP
        /// </summary>
        /// <param name="cellPhone"></param>
        /// <param name="userCode"></param>
        /// <returns></returns>
        private int WriteOTPCode2DB(string cellPhone, string userCode, string updUserId)
        {
            int i = -1;
            try
            {
                //datetime2 SYSDATETIME()
                //var UpdDateTime = DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss.fffffff");
                var sSql = $" Update SSEC001_UserInfo "+
                    $" SET OTP=@_otpCode, OTPTime=SYSDATETIME(), UpdUserId=@updUserId " +
                    $" WHERE Telephone=@cellPhone AND UserCode=@userCode ";

                //-------sql para----start
                SqlParameter[] sqlParams = new SqlParameter[] {
                    new SqlParameter("@_otpCode", SqlDbType.NChar),
                    new SqlParameter("@updUserId", SqlDbType.UniqueIdentifier),
                    new SqlParameter("@cellPhone", SqlDbType.VarChar),
                    new SqlParameter("@userCode", SqlDbType.VarChar),

                };
                sqlParams[0].Value = _otpCode.Valid();
                sqlParams[1].Value = updUserId.ValidGuid();
                sqlParams[2].Value = cellPhone.Valid();
                sqlParams[3].Value = userCode.Valid();
                //-------sql para----end

                i = _db.ExecuteSql(sSql,sqlParams);
                return i;
            }
            catch(Exception ex)
            {
                Log.Error("Write OTP 2 Database fail！" + ex.Message);
                Log.Error(ex.StackTrace);
                throw ex;
            }
        }

        /// <summary>
        /// Base64解密
        /// </summary>
        /// <param name="input">需要解密的字符串</param>
        /// <returns></returns>
        public static string Base64Decrypt(string input)
        {
            return Base64Decrypt(input, new UTF8Encoding());
        }

        /// <summary>
        /// Base64解密
        /// </summary>
        /// <param name="input">需要解密的字符串</param>
        /// <param name="encode">字符的編碼</param>
        /// <returns></returns>
        public static string Base64Decrypt(string input, Encoding encode)
        {
            return encode.GetString(Convert.FromBase64String(input));
        }

    }
}
