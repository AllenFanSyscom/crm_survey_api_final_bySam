using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using System.Linq;
using System.Text;
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
    [Route("api/system")]
    [ApiController]
    public class SystemController : ControllerBase
    {
        /// <summary>
        /// 問卷系統後臺之登入相關-SSO登入取得token
        /// </summary>
        /// <designer>Allen/Gem</designer>
        private DBHelper _db;
        private DBHelper _crmDB;
        private readonly JwtHelpers jwt;
        public SystemController(JwtHelpers jwt)
        {
            _db = new DBHelper(AppSettingsHelper.DefaultConnectionString);
            _crmDB = new DBHelper(AppSettingsHelper.CRMConnectionString);
            this.jwt = jwt;
        }
        [Route("auth")]
        [HttpPost]
        public String auth([FromBody] Object value)
        {
            /* 輸入格式：
             *{
             *    "UserId":"99999999-0000-0000-0000-000000000002"             //UserId
             *}
             */
            JObject jo = (JObject)JsonConvert.DeserializeObject(value.ToString());
            var replyData = new ReplyData();
            if (jo["UserId"] == null)
            {
                //報告錯誤
                replyData.code = "-1";
                replyData.message = $"未傳入參數UserId！";
                replyData.data = "";
                Log.Error("SSO登入取得token失敗!" + "未傳入參數UserId！");
                return JsonConvert.SerializeObject(replyData);
            }


            try
            {
                //string UserId = AppSettingsHelper.EnvSwitchToCRM.SwitchToCRM ?
                //Utility.Common.Base64Decrypt(jo["UserId"].ToString(), new UTF8Encoding()) : jo["UserId"].ToString();

                //SSO傳入的UID，統一要使用BASE64編碼。
                string UserId = Utility.Common.Base64Decrypt(jo["UserId"].ToString());

                Log.Debug($"SSO傳入的UID:{UserId}!");

                //1. 檢查UserId是否存在(SSEC001_UserInfo.UserId)
                if (!SurveyWebAPI.Utility.Common.IsUserIdExists(UserId))
                {
                    //從CRM站台跳轉進來的GUID，如果不存在的話，要新增資料到問卷平台
                    Guid crmUserId;
                    bool isUserIdGuid = Guid.TryParse(UserId, out crmUserId);
                    DataTable dtCrmUser;

                    if (AppSettingsHelper.EnvSwitchToCRM.SwitchToCRM)
                    {
                        dtCrmUser = CRMDbApiHelper.SystemController_GetCRMUserInfo(UserId);
                    }
                    else
                    {
                        dtCrmUser = GetCRMUserInfoBy(UserId);
                    }

                    if (isUserIdGuid && dtCrmUser.Rows.Count>0)
                    {
                        //Create User
                        DataRow crmRow = dtCrmUser.Rows[0];
                        var crmUserCode = crmRow["UserCode"] == DBNull.Value ? "" : crmRow["UserCode"].ToString().Trim();
                        var crmDeptNo = "";
                        var crmUserName = "";
                        var crmUserFullName = "";
                        var crmTelephone = crmRow["Telephone"] == DBNull.Value ? "" : crmRow["Telephone"].ToString().Trim();
                        var crmEMail = "";
                        var crmStartDate = "SYSDATETIME()";
                        var crmStopDate = " DATEADD(year, 100, SYSDATETIME())";
                        var crmStyleNo = 0;
                        var crmUsedMark = "1";
                        var crmRemark = "SSO跳轉問卷平台添加新User";
                        var crmTypeErrorTime = 0;
                        var insSql = $"INSERT INTO SSEC001_UserInfo (" +
                            " UserId, UserCode, DeptNo, UserName, UserFullName, Telephone, " +
                            " EMail,  StartDate, StopDate, StyleNo, UsedMark," +
                            " Remark, PwdErrorTime, UpdUserId, UpdDateTime ) VALUES (  " +
                            $" @crmUserId, @crmUserCode, @crmDeptNo, @crmUserName, @crmUserFullName,@crmTelephone," +
                            $" @crmEMail, " +
                            $" @crmStartDate," +
                            $" @crmStopDate," +
                            $" @crmStyleNo, @crmUsedMark," +
                            $" @crmRemark, @crmTypeErrorTime,@crmUserId,   SYSDATETIME() )";
                        //-------sql para----start
                        SqlParameter[] sqlParams = new SqlParameter[] {
                            new SqlParameter("@crmUserId", SqlDbType.UniqueIdentifier),
                            new SqlParameter("@crmUserCode", SqlDbType.VarChar),
                            new SqlParameter("@crmDeptNo", SqlDbType.VarChar),
                            new SqlParameter("@crmUserName", SqlDbType.NVarChar),
                            new SqlParameter("@crmUserFullName", SqlDbType.NVarChar),
                            new SqlParameter("@crmTelephone", SqlDbType.VarChar),
                            new SqlParameter("@crmEMail", SqlDbType.VarChar),
                            new SqlParameter("@crmStartDate", SqlDbType.DateTime2),
                            new SqlParameter("@crmStopDate", SqlDbType.DateTime2),
                            new SqlParameter("@crmStyleNo", SqlDbType.Int),
                            new SqlParameter("@crmUsedMark", SqlDbType.Bit),
                            new SqlParameter("@crmRemark", SqlDbType.VarChar),
                            new SqlParameter("@crmTypeErrorTime", SqlDbType.Int),
                            new SqlParameter("@crmUserId", SqlDbType.UniqueIdentifier)
                        };
                        sqlParams[0].Value = crmUserId.ToString().ValidGuid();
                        sqlParams[1].Value = crmUserCode.Valid();
                        sqlParams[2].Value = crmDeptNo.Valid();
                        sqlParams[3].Value = crmUserName.Valid();
                        sqlParams[4].Value = crmUserFullName.Valid();
                        sqlParams[5].Value = crmTelephone.Valid();
                        sqlParams[6].Value = crmEMail.Valid();
                        sqlParams[7].Value = crmStartDate.Valid();
                        sqlParams[8].Value = crmStopDate.Valid();
                        sqlParams[9].Value = crmStyleNo.ValidInt();
                        sqlParams[10].Value = crmUsedMark.Valid();
                        sqlParams[11].Value = crmRemark.Valid();
                        sqlParams[12].Value = crmTypeErrorTime.ValidInt();
                        sqlParams[13].Value = crmUserId.ToString().ValidGuid();
                        //-------sql para----end

                        var insertNum = _db.ExecuteSql(insSql, sqlParams);
                        Log.Debug($"問卷平台跳轉SSO傳入Uid{UserId}不存在,需要新增!");
                        Log.Debug($"問卷平台跳轉SSO新增Uid Sql {insSql}");


                        //2020/10/12--Allen/Gem: 用戶不存在，新增的時候，也要寫一筆資料到SSEC005_UserRole，RoleId 固定給2，
                        //                        UsedMark = True，StartDate = Date.Now, EndDate = Date.Now+100年
                        var insRole = $" INSERT INTO SSEC005_UserRole " +
                            $" (UserId, [RoleId], UsedMark, StartDate, EndDate, " +
                            "   Remark, UpdUserId, UpdDateTime) VALUES " +
                            $" (@UserId,2, '1',@crmStartDate, @crmStopDate," +
                            $"   '',@crmUserId,  SYSDATETIME() )";
                        //-------sql para----start
                        SqlParameter[] sqlParamsA = new SqlParameter[] {
                            new SqlParameter("@UserId", SqlDbType.UniqueIdentifier),
                            new SqlParameter("@crmStartDate", SqlDbType.DateTime2),
                            new SqlParameter("@crmStopDate", SqlDbType.DateTime2),
                            new SqlParameter("@crmUserId", SqlDbType.UniqueIdentifier)
                        };
                        sqlParamsA[0].Value = UserId.ValidGuid();
                        sqlParamsA[1].Value = crmStartDate.Valid();
                        sqlParamsA[2].Value = crmStopDate.Valid();
                        sqlParamsA[3].Value = crmUserId.ToString().ValidGuid();
                        //-------sql para----end

                        insertNum = _db.ExecuteSql(insRole, sqlParamsA);
                        Log.Debug($"問卷平台跳轉SSO傳入Uid{UserId}不存在,新增UserId後，需要新增SEC005!");
                        Log.Debug($"問卷平台跳轉SSO新增SEC005 Sql {insRole}");
                    }
                    else
                    {
                        //不存在
                        // 回傳結果
                        ErrorCode.Code = "101";    //帳號錯誤
                        replyData.code = ErrorCode.Code;
                        replyData.message = ErrorCode.Message;
                        replyData.data = "";
                        Log.Error($"SSO登入取得token失敗！用戶{UserId}不存在！");
                        return JsonConvert.SerializeObject(replyData);
                    }

                }
                //2.存在則取出UserCode，產生Token: jwt.GenerateToken(UserCode)
                // 回傳結果，結果同OTP驗證
                TokenInfo sso = new TokenInfo();

                var UserCode = "";

                DataTable dtR = GetUserInfoBy(UserId);
                if (dtR.Rows.Count > 0)
                {
                    DataRow row = dtR.Rows[0];
                    UserCode = row["UserCode"] == DBNull.Value ? "" : row["UserCode"].ToString().Trim();
                    sso.UserId = row["UserId"];
                    sso.UserName = row["UserName"];
                    sso.RoleId = row["RoleId"];
                    sso.RoleName = row["RoleName"];
                }
                else
                {
                    ErrorCode.Code = "101";    //帳號錯誤
                    replyData.code = ErrorCode.Code;
                    replyData.message = ErrorCode.Message;
                    replyData.data = "";
                    Log.Error($"SSO登入取得token失敗！用戶{UserId}不存在！");
                    return JsonConvert.SerializeObject(replyData);
                }
                //目前從SSEC001_UserInfo取得UserName/UserCode/Telephone請改取CHT_MSCRM的SystemUserBase裡面的FullName/EmployeeId/MobilePhone
                Log.Debug("UserId:"+ UserId);
                DataTable dtCrm;
                if(AppSettingsHelper.EnvSwitchToCRM.SwitchToCRM)
                {
                    dtCrm = CRMDbApiHelper.SystemController_GetCRMUserInfo(sso.UserId.ToString());
                }
                else
                {
                    dtCrm = GetCRMUserInfoBy(sso.UserId.ToString());
                }

                if (dtCrm.Rows.Count > 0)
                {
                    //User info 在CRM裡最新
                    var row = dtCrm.Rows[0];
                    UserCode = row["UserCode"] == DBNull.Value ? "" : row["UserCode"].ToString().Trim();
                    sso.UserId = row["UserId"];
                    sso.UserName = row["UserName"];

                    //從CRM取得用戶訊息，要更新問卷平台的資料
                    var uptUser = $" UPDATE SSEC001_UserInfo SET  UserName = @UserName" +
                            $", Telephone = @Telephone " +
                            $"   WHERE UserId=@UserId " ;
                    //-------sql para----start
                    SqlParameter[] sqlParamsB = new SqlParameter[] {
                        new SqlParameter("@UserName", SqlDbType.NVarChar),
                        new SqlParameter("@Telephone", SqlDbType.VarChar),
                        new SqlParameter("@UserId", SqlDbType.UniqueIdentifier)
                    };

                    sqlParamsB[0].Value = row["UserName"].Valid();
                    sqlParamsB[1].Value = row["Telephone"].Valid();
                    sqlParamsB[2].Value = row["UserId"].ToString().ValidGuid();
                    //-------sql para----end
                    var updateNum = _db.ExecuteSql(uptUser, sqlParamsB);
                    Log.Debug($"更新CRM用戶訊息!"+$"UserName:{row["UserName"]}，Telephone:{row["Telephone"]}，UserId:{row["UserId"]}，筆數:{updateNum.ToString()}");

                }

                //var UserCode = GetUserCodeBy(UserId);
                //產生Token: jwt.GenerateToken(UserCode)
                var token = jwt.GenerateToken(UserCode);

                sso.Token = token;
                //產生token後，還是需要寫入SYS001
                var Ip = "SSO"; //allen說，此處寫成SSO
                int i = WriteToken2DB(sso.UserId.ToString(), Ip, token);
                replyData.code = "200";
                replyData.message = $"SSO登入取得token成功。";
                replyData.data = sso;
                Log.Debug($"SSO登入取得token成功！token='{token}' ");
                return JsonConvert.SerializeObject(replyData);
            }
            catch (Exception ex)
            {
                //報告錯誤
                replyData.code = "-1";
                replyData.message = ex.Message;
                replyData.data = null;
                Log.Error("SSO登入取得token失敗！" + ex.Message);
                return JsonConvert.SerializeObject(replyData);
            }
        }

        private DataTable GetCRMUserInfoBy(String userId)
        {
            string sSql = $" SELECT SystemUserId AS UserId, FullName AS UserName, SUBSTRING(DomainName,CHARINDEX('\\',DomainName)+1,LEN(DomainName)-CHARINDEX('\\',DomainName)) AS UserCode, MobilePhone AS Telephone " +
                " FROM SystemUserBase " +
                $" WHERE SystemUserId=@userId  ";
            //-------sql para----start
            SqlParameter[] sqlParams = new SqlParameter[] {
                new SqlParameter("@userId", SqlDbType.UniqueIdentifier)
            };
            sqlParams[0].Value = userId.ValidGuid();
            //-------sql para----end
            try
            {
                Log.Debug("SSO登入取得token, GetCRMUserInfoBy sql:" + sSql);
                return _crmDB.GetQueryData(sSql, sqlParams);
            }
            catch (Exception ex)
            {
                Log.Error("SSO登入取得token: GetUserInfoBy fail！" + ex.Message);
                Log.Error(ex.StackTrace);
                throw ex;
            }
        }

        private DataTable GetUserInfoBy(String userId)
        {
            Guid resultId;
            var isGuid = Guid.TryParse(userId, out resultId);
            //-------sql para----start
            List<SqlParameter> sqlParams = new List<SqlParameter>();
            //-------sql para----end

            string sSql = "";
            if (!isGuid)
            {
                sSql += $" SELECT TOP 1  A.UserId, A.UserName, A.UserCode, B.RoleId, C.RoleName " +
                " FROM SSEC001_UserInfo A " +
                " LEFT JOIN SSEC005_UserRole B ON B.UserId = A.UserId AND B.UsedMark = '1' " +
                " LEFT JOIN SSEC004_RoleId C ON C.RoleId = B.RoleId AND C.UsedMark = '1' " +
                $" WHERE A.UserCode=@userId AND A.UsedMark = '1' ";

                var obj = new SqlParameter("@userId", SqlDbType.VarChar);
                obj.Value = userId.Valid();
                sqlParams.Add(obj);
            }
            else
            {
                sSql += $" SELECT TOP 1  A.UserId, A.UserName, A.UserCode, B.RoleId, C.RoleName " +
                " FROM SSEC001_UserInfo A " +
                " LEFT JOIN SSEC005_UserRole B ON B.UserId = A.UserId AND B.UsedMark = '1' " +
                " LEFT JOIN SSEC004_RoleId C ON C.RoleId = B.RoleId AND C.UsedMark = '1' " +
                $" WHERE A.UserId=@resultId AND A.UsedMark = '1' ";

                var obj = new SqlParameter("@resultId", SqlDbType.UniqueIdentifier);
                obj.Value = resultId.ToString().ValidGuid();
                sqlParams.Add(obj);


            }

            try
            {
                Log.Debug("SSO登入取得token, GetUserInfoBy sql:" + sSql);
                return _db.GetQueryData(sSql, sqlParams.ToArray());
            }
            catch (Exception ex)
            {
                Log.Error("SSO登入取得token GetUserInfoBy fail！" + ex.Message);
                Log.Error(ex.StackTrace);
                throw ex;
            }
        }

        /// <summary>
        /// 依UserId取得UserCode
        /// </summary>
        /// <param name="userId"></param>
        /// <returns>UserCode</returns>
        private String GetUserCodeBy(String userId)
        {
            string sSql = $"SELECT UserCode FROM SSEC001_UserInfo WHERE UserId=@userId AND UsedMark='1' ";
            //-------sql para----start
            SqlParameter[] sqlParams = new SqlParameter[] {
                new SqlParameter("@userId", SqlDbType.UniqueIdentifier)
            };
            sqlParams[0].Value = userId.ValidGuid();
            //-------sql para----end
            try
            {
                DataTable dtR = _db.GetQueryData(sSql, sqlParams);
                if (dtR.Rows.Count > 0)
                    return dtR.Rows[0]["UserCode"] == DBNull.Value ? "" : dtR.Rows[0]["UserCode"].ToString().Trim();
                return "";
            }
            catch (Exception ex)
            {
                Log.Error("SSO登入取得token:" + ex.StackTrace);
                Log.Error("SSO登入取得token:" + ex.Message);
                throw ex;
            }
        }

        /// <summary>
        /// 將生成的Token寫入SYS001
        /// </summary>
        /// <param name="userId"></param>
        /// <returns></returns>
        private int WriteToken2DB(String userId, string ip, string token)
        {
            int i = -1;
            try
            {
                //傳入的這個token格式不對，寫不到db，所以用GUID即可，SYS001僅僅為了去LastLoginDateTime，所以，其他欄位沒關係
                token =  Guid.NewGuid().ToString("D");

                var UserId = "";
                var UserName = "";
                var sSql1 = $"SELECT UserId, UserName FROM SSEC001_UserInfo WHERE UserId=@userId ";
                //-------sql para----start
                SqlParameter[] sqlParams = new SqlParameter[] {
                    new SqlParameter("@userId", SqlDbType.UniqueIdentifier)
                };
                sqlParams[0].Value = userId.ValidGuid();
                //-------sql para----end
                DataTable dtR;
                if(AppSettingsHelper.EnvSwitchToCRM.SwitchToCRM)
                {
                    dtR = CRMDbApiHelper.SystemController_GetCRMUserInfo(userId);
                }
                else
                {
                    dtR = _db.GetQueryData(sSql1, sqlParams);
                }
                if (dtR.Rows.Count > 0)
                {
                    DataRow dr = dtR.Rows[0];
                    UserId = dr["UserId"].ToString();
                    UserName = dr["UserName"].ToString();
                }
                //User Info在CRM DB為最新
                var sSql = "";

                sSql = " INSERT INTO SYS001_SystemToken (Token, UserId, UserName, Ip, ExpiredDate, UpdDate) VALUES " +
                        $"(@token, @UserId, @UserName,@ip,SYSDATETIME(),SYSDATETIME())";
                //-------sql para----start
                SqlParameter[] sqlParamsA = new SqlParameter[] {
                    new SqlParameter("@token", SqlDbType.UniqueIdentifier),
                    new SqlParameter("@UserId", SqlDbType.UniqueIdentifier),
                    new SqlParameter("@UserName", SqlDbType.NVarChar),
                    new SqlParameter("@ip", SqlDbType.VarChar)
                };
                sqlParamsA[0].Value = token.ValidGuid();
                sqlParamsA[1].Value = UserId.ValidGuid();
                sqlParamsA[2].Value = UserName.Valid();
                sqlParamsA[3].Value = ip.Valid();
                //-------sql para----end
                i = _db.ExecuteSql(sSql, sqlParamsA);
                return i;
            }
            catch (Exception ex)
            {
                Log.Error("SSO Write Token fail！" + ex.Message);
                Log.Error(ex.StackTrace);
                throw ex;
            }
        }

        [Authorize]
        [Route("tokenAuth")]
        [HttpGet]
        public String tokenAuth()
        {
            var key = User.Identity.Name;
            var info = Utility.Common.GetConnectionInfo(key);

            ReplyData replyData = new ReplyData();

            // 用戶資料為空，則回傳錯誤
            if (info == null)
            {
                // 回傳錯誤
                replyData.code = "-1";
                replyData.message = $"未傳入用戶資料！";
                replyData.data = "";
                Log.Error("驗證token失敗!" + "未傳入用戶資料！");
                return JsonConvert.SerializeObject(replyData);
            }

            replyData.code = "200";
            replyData.message = $"token合法！";
            replyData.data = info;

            // 返回結果
            return JsonConvert.SerializeObject(replyData);
        }

        /// <summary>
        /// 傳入中華SSO返回的所有資料，並進行資料解析
        /// </summary>
        /// <param name="innerText"></param>
        /// <returns></returns>
        [Route("fetchSSO")]
        [HttpGet]
        public String fetchSSO(string innerText)
        {

            ReplyData replyData = new ReplyData();

            // 用戶資料為空，則回傳錯誤
            if (innerText == null)
            {
                // 回傳錯誤
                replyData.code = "-1";
                replyData.message = $"未傳入資料！";
                replyData.data = "";
                Log.Error("fetchSSO失敗!" + "未傳入資料！");
                return JsonConvert.SerializeObject(replyData);
            }

            replyData.code = "200";
            replyData.message = $"SSO資料處理完畢！";
            //回傳跳轉頁
            replyData.data = "https://crmsurvey.cht.com.tw";

            // 返回結果
            return JsonConvert.SerializeObject(replyData);
        }

        [Route("sqlTest")]
        [HttpGet]
        public String sqlTest(string userId)
        {
            string sqlStr = $"SELECT SystemUserId AS UserId, FullName AS UserName " + //, EmployeeId AS UserCode, MobilePhone AS CellPhone " +
                    " FROM SystemUserBase " +
                    $" WHERE SystemUserId=@userId ";

            ReplyData replyData = new ReplyData();

            Log.Debug("sqlTest...");
            try {
                var connStr = AppSettingsHelper.CRMConnectionString;
                Log.Debug("SqlConnection宣告..."+ connStr);
                SqlConnection conn = new SqlConnection(connStr);
                Log.Debug("SqlDataAdapter宣告...");
                SqlDataAdapter ada = new SqlDataAdapter(sqlStr, conn);
                ada.SelectCommand.Parameters.AddWithValue("@userId", userId.Valid());

                Log.Debug("連線建立...");
                conn.Open();
                Log.Debug("連線建立完畢...");
                DataSet ds = new DataSet();
                Log.Debug("ada.Fill(ds)開始...");
                ada.Fill(ds);
                Log.Debug("ada.Fill(ds)取得結束...");
                replyData.code = "200";
                replyData.message = $"連線執行完畢！";
                replyData.data = ds.ToString();

                ada.Dispose();
                conn.Close();
            }
            catch (SqlException e)
            {
                Log.Error("sqlTest1:" + JsonConvert.SerializeObject(e));
                replyData.code = "200";
                replyData.message = $"連線錯誤！";
                replyData.data = JsonConvert.SerializeObject(e);
            }


            // 返回結果
            return JsonConvert.SerializeObject(replyData);
        }

        [Route("sqlTest2")]
        [HttpGet]
        public String sqlTest2(string userId)
        {
            string sqlStr = $"SELECT SystemUserId AS UserId, FullName AS UserName " + //, EmployeeId AS UserCode, MobilePhone AS CellPhone " +
                    " FROM SystemUserBase " +
                    $" WHERE SystemUserId=@userId ";

            ReplyData replyData = new ReplyData();
            Log.Debug("sqlTest2...");
            try
            {
                var connStr = AppSettingsHelper.CRMConnectionString;
                Log.Debug("SqlConnection宣告..." + connStr);
                SqlConnection conn = new SqlConnection(connStr);

                SqlCommand command = new SqlCommand(sqlStr, conn);
                command.Parameters.AddWithValue("@userId", userId.Valid() );
                Log.Debug("連線建立...");
                conn.Open();
                Log.Debug("連線建立完畢...");
                SqlDataReader reader = command.ExecuteReader();
                Log.Debug("reader執行完畢...");
                replyData.code = "200";
                replyData.message = $"連線執行完畢！";
                replyData.data = reader.ToString();
                Log.Debug("資料取得結束...");
                reader.Close();

                conn.Close();
            }
            catch (SqlException e)
            {
                Log.Error("sqlTest2:"+ JsonConvert.SerializeObject(e));
                replyData.code = "200";
                replyData.message = $"連線錯誤！";
                replyData.data = JsonConvert.SerializeObject(e);

            }

            // 返回結果
            return JsonConvert.SerializeObject(replyData);
        }

        [Route("sqlTest3")]
        [HttpGet]
        public String sqlTest3(string userId)
        {
            string sqlStr = $"SELECT UserId, UserName " + //, EmployeeId AS UserCode, MobilePhone AS CellPhone " +
                    " FROM SSEC001_UserInfo " +
                    $" WHERE UserId=@userId ";

            ReplyData replyData = new ReplyData();

            Log.Debug("sqlTest3...");
            try
            {
                var connStr = AppSettingsHelper.DefaultConnectionString;
                Log.Debug("SqlConnection宣告..." + connStr);
                SqlConnection conn = new SqlConnection(connStr);
                Log.Debug("SqlDataAdapter宣告...");
                SqlDataAdapter ada = new SqlDataAdapter(sqlStr, conn);
                ada.SelectCommand.Parameters.AddWithValue("@userId", userId.Valid());

                Log.Debug("連線建立...");
                conn.Open();
                Log.Debug("連線建立完畢...");
                DataSet ds = new DataSet();
                Log.Debug("ada.Fill(ds)開始...");
                ada.Fill(ds);
                Log.Debug("ada.Fill(ds)取得結束...");
                replyData.code = "200";
                replyData.message = $"連線執行完畢！";
                replyData.data = ds.ToString();

                ada.Dispose();
                conn.Close();
            }
            catch (SqlException e)
            {
                Log.Error("sqlTest1:" + JsonConvert.SerializeObject(e));
                replyData.code = "200";
                replyData.message = $"連線錯誤！";
                replyData.data = JsonConvert.SerializeObject(e);
            }


            // 返回結果
            return JsonConvert.SerializeObject(replyData);
        }

        //20210719 sam新增登出
        [Route("LogOut")]
        [HttpPost]
        public String LogOut([FromBody] Object value)
        {
            JObject jo = (JObject)JsonConvert.DeserializeObject(value.ToString());
            var replyData = new ReplyData();
            var UserCode = jo["UserCode"].ToString();
            if (jo["UserCode"] == null)
            {
                //報告錯誤
                replyData.code = "-1";
                replyData.message = $"未傳入參數UserCode！";
                replyData.data = "";
                Log.Error("LogOut失敗!" + "未傳入參數UserCode！");
                return JsonConvert.SerializeObject(replyData);
            }



            try
            {
                var rs = CleanOTP(UserCode);
                if (!rs)
                {
                    //驗證不通過
                    // 回傳結果
                    ErrorCode.Code = "103";
                    replyData.code = ErrorCode.Code;
                    replyData.message = "登出失敗";
                    replyData.data = "";
                    Log.Error($"登出失敗！, UserCode='{UserCode}',result={rs}");
                    return JsonConvert.SerializeObject(replyData);
                }
                else
                {
                    // 回傳結果
                    replyData.code = "200";
                    replyData.message = $"登出成功。";
                    replyData.data = "";
                    Log.Debug($"登出成功！, UserCode='{UserCode}'");
                    return JsonConvert.SerializeObject(replyData);
                }
            }
            catch (SqlException e)
            {
                Log.Error("LogOut:" + JsonConvert.SerializeObject(e));
                replyData.code = "200";
                replyData.message = $"登出錯誤！";
                replyData.data = JsonConvert.SerializeObject(e);
                return JsonConvert.SerializeObject(replyData);
            }


        }

        //20210718 sam 新增改版OTP檢核方式
        private bool CleanOTP( string userCode)
        {

            int i = -1;
            try
            {

                var sSql = $" Update SSEC001_UserInfo " +
                    $" SET OTP=NULL, OTPTime=NULL " +
                    $" WHERE UserCode=@userCode ";
                //-------sql para----start
                SqlParameter[] sqlParams = new SqlParameter[] {
                    new SqlParameter("@userCode", SqlDbType.VarChar)
                };
                sqlParams[0].Value = userCode.Valid();
                //-------sql para----end

                i = _db.ExecuteSql(sSql, sqlParams);
                if (i > 0)
                    return true;
                else
                    return false;
            }
            catch (Exception ex)
            {
                Log.Error("UPDATE OTP 2 Database fail！" + ex.Message);
                Log.Error(ex.StackTrace);
                throw ex;
            }
        }


        //20210725 sam新增 驗證身分
        [Route("VerifyUser")]
        [HttpPost]
        public String VerifyUser([FromBody] Object value)
        {
            JObject jo = (JObject)JsonConvert.DeserializeObject(value.ToString());
            var replyData = new ReplyData();
            var UserCode = jo["UserCode"].ToString();
            if (jo["UserCode"] == null)
            {
                //報告錯誤
                replyData.code = "-1";
                replyData.message = $"未傳入參數UserCode！";
                replyData.data = "";
                Log.Error("驗證失敗!" + "未傳入參數UserCode！");
                return JsonConvert.SerializeObject(replyData);
            }

            var CellPhone = jo["CellPhone"].ToString();
            if (jo["CellPhone"] == null)
            {
                //報告錯誤
                replyData.code = "-1";
                replyData.message = $"未傳入參數CellPhone！";
                replyData.data = "";
                Log.Error("驗證失敗!" + "未傳入參數CellPhone！");
                return JsonConvert.SerializeObject(replyData);
            }



            try
            {
                var rs = CheckUserMobile(UserCode, CellPhone);
                if (!rs)
                {
                    //驗證不通過
                    // 回傳結果
                    ErrorCode.Code = "103";
                    replyData.code = ErrorCode.Code;
                    replyData.message = "帳號或手機號碼錯誤";
                    replyData.data = "";
                    Log.Error($"帳號或手機號碼錯誤！, UserCode='{UserCode}',CellPhone='{CellPhone}',result={rs}");
                    return JsonConvert.SerializeObject(replyData);
                }
                else
                {
                    // 回傳結果
                    replyData.code = "200";
                    replyData.message = $"驗證成功。";
                    replyData.data = "";
                    Log.Debug($"驗證成功！, UserCode='{UserCode}',CellPhone='{CellPhone}'");
                    return JsonConvert.SerializeObject(replyData);
                }
            }
            catch (SqlException e)
            {
                Log.Error("LogOut:" + JsonConvert.SerializeObject(e));
                replyData.code = "100";
                replyData.message = $"驗證錯誤！";
                replyData.data = JsonConvert.SerializeObject(e);
                return JsonConvert.SerializeObject(replyData);
            }


        }

        //20210725 sam 新增檢核方式
        private bool CheckUserMobile(string userCode,string cellPhone)
        {

            int i = -1;
            try
            {

                var sSql = $" SELECT UserId FROM  SSEC001_UserInfo " +
                    $" WHERE UserCode=@userCode AND Telephone=@cellPhone";
                //-------sql para----start
                SqlParameter[] sqlParams = new SqlParameter[] {
                    new SqlParameter("@userCode", SqlDbType.VarChar),
                    new SqlParameter("@cellPhone", SqlDbType.VarChar)
                };
                sqlParams[0].Value = userCode.Valid();
                sqlParams[1].Value = cellPhone.Valid();
                //-------sql para----end
                DataTable dtR = _db.GetQueryData(sSql, sqlParams);
                if (dtR.Rows.Count > 0)
                    return true;
                else
                    return false;

            }
            catch (Exception ex)
            {
                Log.Error("Query CheckUserMobile fail！" + ex.Message);
                Log.Error(ex.StackTrace);
                throw ex;
            }
        }

        //20210725 sam新增 修改密碼
        [Authorize]
        [Route("ChangePass")]
        [HttpPost]
        public String ChangePass([FromBody] Object value)
        {
            JObject jo = (JObject)JsonConvert.DeserializeObject(value.ToString());
            var replyData = new ReplyData();
            var UserId = jo["UserId"].ToString();
            if (jo["UserId"] == null)
            {
                //報告錯誤
                replyData.code = "-1";
                replyData.message = $"未傳入參數UserId！";
                replyData.data = "";
                Log.Error("修改失敗!" + "未傳入參數UserId！");
                return JsonConvert.SerializeObject(replyData);
            }

            var Password = jo["Password"].ToString();
            if (jo["Password"] == null)
            {
                //報告錯誤
                replyData.code = "-1";
                replyData.message = $"未傳入參數Password！";
                replyData.data = "";
                Log.Error("修改失敗!" + "未傳入參數Password！");
                return JsonConvert.SerializeObject(replyData);
            }



            try
            {
                var rs = UpdPassword(UserId, Password);
                if (!rs)
                {
                    //驗證不通過
                    // 回傳結果
                    ErrorCode.Code = "103";
                    replyData.code = ErrorCode.Code;
                    replyData.message = "該用戶不存在";
                    replyData.data = "";
                    Log.Error($"該用戶不存在！, UserId='{UserId}',Password='{Password}',result={rs}");
                    return JsonConvert.SerializeObject(replyData);
                }
                else
                {
                    // 回傳結果
                    replyData.code = "200";
                    replyData.message = $"修改成功。";
                    replyData.data = "";
                    Log.Debug($"修改成功！, UserId='{UserId}',Password='{Password}'");
                    return JsonConvert.SerializeObject(replyData);
                }
            }
            catch (SqlException e)
            {
                Log.Error("LogOut:" + JsonConvert.SerializeObject(e));
                replyData.code = "200";
                replyData.message = $"修改錯誤！";
                replyData.data = JsonConvert.SerializeObject(e);
                return JsonConvert.SerializeObject(replyData);
            }


        }

        //20210725 sam 新增修改密碼
        private bool UpdPassword(string userid,string password)

        {

            int i = -1;
            try
            {
                var encryption_code = Utility.Common.SHA256Encrypt(password);
                var sSql = $" Update SSEC001_UserInfo " +
                    $" SET Password=@encryption_code, PwdUpdTime=GETDATE() " +
                    $" WHERE UserCode=@userid ";
                //-------sql para----start
                SqlParameter[] sqlParams = new SqlParameter[] {
                    new SqlParameter("@encryption_code", SqlDbType.VarChar),
                    new SqlParameter("@userid", SqlDbType.UniqueIdentifier)
                };
                sqlParams[0].Value = encryption_code.Valid();
                sqlParams[1].Value = userid.ValidGuid();
                //-------sql para----end
                i = _db.ExecuteSql(sSql);
                if (i > 0)
                    return true;
                else
                    return false;
            }
            catch (Exception ex)
            {
                Log.Error("UpdPassword SSEC001_UserInfo 2 Database fail！" + ex.Message);
                Log.Error(ex.StackTrace);
                throw ex;
            }
        }




    }



    public class SSOAuth
    {
        /// <summary>
        /// Token Value
        /// </summary>
        public Object Token { get; set; }
    }
}
