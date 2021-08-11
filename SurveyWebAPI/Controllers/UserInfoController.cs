using System.Threading.Tasks;
using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using SurveyWebAPI.Models;
using SurveyWebAPI.Utility;
using Microsoft.AspNetCore.Routing;
using Common;
using log4net;
using Microsoft.AspNetCore.Authorization;
using System.Data.SqlClient;

namespace SurveyWebAPI.Controllers
{
    [Authorize]
    [Route("/api/User/Info/")]
    [ApiController]

    public class UserInfoController : ControllerBase
    {
        /// <summary>
        /// 問卷系統後臺之使用者維護API
        /// </summary>
        /// <designer> Allen Fan/Gem He</designer>
        //private readonly ILogger<SSEC001_UserInfoController> _logger;  //可實現log在console輸出
        //private readonly ConnectionStrings _connectionOptions;
        private DBHelper _db;
        //public SSEC001_UserInfoController(ILogger<SSEC001_UserInfoController> logger)
        public UserInfoController()
        {
            //_logger = logger;
            //_db = new DBHelper();
            //_logger = logger;
            //_connectionOptions = options.Value;
            _db = new DBHelper(AppSettingsHelper.DefaultConnectionString);
        }

        /// <summary>
        /// 查詢所有資料
        /// </summary>
        /// <returns></returns>
        [Route("QueryAll")]
        [HttpGet]
        //public IEnumerable<SSEC001_UserInfo> QueryAll()
        public String QueryAll()
        {
            Log.Debug("查詢：Query all data from SSEC001_UserInfo...");
            List<SSEC001_UserInfo> lstUserInfo = new List<SSEC001_UserInfo>();
            ReplyData replyData = new ReplyData();
            string sSql = "select * from SSEC001_UserInfo ";
            try
            {
                lstUserInfo = ExecuteQuery(sSql);
                replyData.code = "200";
                replyData.message = $"查詢記錄完成。共{lstUserInfo.Count}筆。";
                replyData.data = lstUserInfo;// JsonConvert.SerializeObject(lstUserInfo);
            }
            catch (Exception ex)
            {
                replyData.code = "-1";
                replyData.message = $"查詢失敗！{ex.Message}.";
                replyData.data = "";
                Log.Error("查詢SSEC001_UserInfo資料失敗!" + ex.Message);
            }

            //返回結果
            return JsonConvert.SerializeObject(replyData);
            //return lstUserInfo.ToArray();
        }

        /// <summary>
        /// 依據條件查詢資料
        /// </summary>
        /// <param name="value"></param>
        /// <returns></returns>
        [Route("QueryBy")]
        [HttpGet]
        public string QueryByCondition([FromBody] Object value)
        {
            JObject jo = (JObject)JsonConvert.DeserializeObject(value.ToString());
            ReplyData replyData = new ReplyData();
            List<SSEC001_UserInfo> lstUserInfo = new List<SSEC001_UserInfo>();
            string sSql = "SELECT * FROM SSEC001_UserInfo ";
            string sWhereCondition = " WHERE 1=1 ";

            //-------sql para----start
            List<SqlParameter> sqlParams = new List<SqlParameter>();
            //-------sql para----end

            if (jo["UserId"] != null)
            {
                var UserId = jo["UserId"].ToString();
                sWhereCondition += $" AND UserId=@UserId ";

                var obj = new SqlParameter("@UserId", SqlDbType.UniqueIdentifier);
                obj.Value = UserId.ValidGuid();
                sqlParams.Add(obj);
            }
            if (jo["UserCode"] != null)
            {
                var UserCode = jo["UserCode"].ToString();
                sWhereCondition += $" AND UserCode=@UserCode ";

                var obj = new SqlParameter("@UserCode", SqlDbType.VarChar);
                obj.Value = UserCode.Valid();;
                sqlParams.Add(obj);
            }
            if (jo["DeptNo"] != null)
            {
                var DeptNo = jo["DeptNo"].ToString();
                sWhereCondition += $" AND DeptNo=@DeptNo ";

                var obj = new SqlParameter("@DeptNo", SqlDbType.Char);
                obj.Value = DeptNo.Valid();;
                sqlParams.Add(obj);
            }
            if (jo["UserName"] != null)
            {
                var UserName = jo["UserName"].ToString();
                sWhereCondition += $" AND UserName LIKE @UserName ";

                var obj = new SqlParameter("@UserName", SqlDbType.NVarChar);
                obj.Value = ( "%"+UserName+"%" ).Valid();
                sqlParams.Add(obj);
            }
            if (jo["UserFullName"] != null)
            {
                var UserFullName = jo["UserFullName"].ToString();
                sWhereCondition += $" AND UserFullName LIKE @UserFullName ";

                var obj = new SqlParameter("@UserFullName", SqlDbType.NVarChar);
                obj.Value = ( "%"+ UserFullName + "%" ).Valid();
                sqlParams.Add(obj);
            }
            if (jo["Telephone"] != null)
            {
                var Telephone = jo["Telephone"].ToString();
                sWhereCondition += $" AND Telephone=@Telephone ";

                var obj = new SqlParameter("@Telephone", SqlDbType.VarChar);
                obj.Value = Telephone.Valid();;
                sqlParams.Add(obj);
            }
            if (jo["EMail"] != null)
            {
                var EMail = jo["EMail"].ToString();
                sWhereCondition += $" AND EMail=@EMail ";

                var obj = new SqlParameter("@EMail", SqlDbType.VarChar);
                obj.Value = EMail.Valid();;
                sqlParams.Add(obj);
            }
            if (jo["StartDate"] != null)
            {
                var StartDate = jo["StartDate"].ToString();
                sWhereCondition += $" AND StartDate=@StartDate ";

                var obj = new SqlParameter("@StartDate", SqlDbType.DateTime2);
                obj.Value = StartDate.Valid();;
                sqlParams.Add(obj);
            }
            if (jo["StopDate"] != null)
            {
                var StopDate = jo["StopDate"].ToString();
                sWhereCondition += $" AND StopDate=@StopDate ";

                var obj = new SqlParameter("@StopDate", SqlDbType.DateTime2);
                obj.Value = StopDate.Valid();;
                sqlParams.Add(obj);
            }
            if (jo["StyleNo"] != null)
            {
                var StyleNo = Convert.ToInt32(jo["StyleNo"]);
                sWhereCondition += $" AND StyleNo=@StyleNo ";

                var obj = new SqlParameter("@StyleNo", SqlDbType.Int);
                obj.Value = StyleNo.ValidInt();;
                sqlParams.Add(obj);
            }
            if (jo["UsedMark"] != null)
            {
                var UsedMark = jo["UsedMark"].ToString();
                sWhereCondition += $" AND UsedMark=@UsedMark ";

                var obj = new SqlParameter("@UsedMark", SqlDbType.Bit);
                obj.Value = UsedMark.Valid();;
                sqlParams.Add(obj);
            }
            if (jo["Remark"] != null)
            {
                var Remark = jo["Remark"].ToString();
                sWhereCondition += $" AND Remark=@Remark ";

                var obj = new SqlParameter("@Remark", SqlDbType.VarChar);
                obj.Value = Remark.Valid();;
                sqlParams.Add(obj);
            }
            if (jo["PwdErrorTime"] != null)
            {
                var PwdErrorTime = Convert.ToInt32(jo["PwdErrorTime"]);
                sWhereCondition += $" AND PwdErrorTime=@PwdErrorTime ";

                var obj = new SqlParameter("@PwdErrorTime", SqlDbType.Int);
                obj.Value = PwdErrorTime.ValidInt();;
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

                var obj = new SqlParameter("@UpdDateTime", SqlDbType.DateTime2);
                obj.Value = UpdDateTime.Valid();;
                sqlParams.Add(obj);
            }
            sSql += sWhereCondition;

            Log.Debug("Query SSEC001_UserInfo by condition:" + sSql);
            try
            {
                lstUserInfo = ExecuteQuery(sSql, sqlParams.ToArray());
                replyData.code = "200";
                replyData.message = $"查詢記錄完成。共{lstUserInfo.Count}筆。";
                replyData.data = lstUserInfo;// JsonConvert.SerializeObject(lstUserInfo);
            }
            catch(Exception ex)
            {
                replyData.code = "-1";
                replyData.message = $"查詢失敗！{ex.Message}.";
                replyData.data = "";
                Log.Error("查詢SSEC001_UserInfo資料失敗!"+ex.Message);
            }
            //返回
            return JsonConvert.SerializeObject(replyData);
        }

        [Route("Insert")]
        [HttpPost]
       // public IActionResult Insert([FromBody] Object value)
        public String Insert([FromBody] Object value)
        {
            JObject jo = (JObject)JsonConvert.DeserializeObject(value.ToString());
            var replyData = new ReplyData();
            //UserId 必須有?
            var UserId = Guid.NewGuid().ToString("D");
            if (String.IsNullOrWhiteSpace(UserId))
            {
                //報告錯誤
                replyData.code = "-1";
                replyData.message = $"新增資料失敗！產生UserId失敗！";
                replyData.data = "";
                Log.Error("新增SSEC001_UserInfo資料失敗!" + "參數UserId不能為空！");
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

            var UserCode = jo["UserCode"] == null ? "" : jo["UserCode"].ToString();
            var DeptNo = jo["DeptNo"] == null ? "" : jo["DeptNo"].ToString();
            var UserName = jo["UserName"] == null ? "" : jo["UserName"].ToString();
            var UserFullName = jo["UserFullName"] == null ? "" : jo["UserFullName"].ToString();
            var Telephone = jo["Telephone"] == null ? "" : jo["Telephone"].ToString();
            var EMail = jo["EMail"] == null ? "" : jo["EMail"].ToString();
            var StartDate = jo["StartDate"] == null ? "SYSDATETIME()" : jo["StartDate"].ToString();
            var StopDate = jo["StopDate"] == null ? " DATEADD(year, 50, SYSDATETIME())" : jo["StopDate"].ToString();
            var StyleNo = jo["StyleNo"] == null ? 0 : Convert.ToInt32(jo["StyleNo"]);
            var UsedMark = jo["UsedMark"] == null ? "1" : jo["UsedMark"].ToString();
            var Remark = jo["Remark"] == null ? "" : jo["Remark"].ToString();
            var PwdErrorTime = jo["PwdErrorTime"] == null ? 0 : Convert.ToInt32(jo["PwdErrorTime"]);
            //UpdUserId 會有程式依據Token取得，所以,目前暫時寫成00000000-0000-0000-0000-000000000000
            //var UpdUserId = "00000000-0000-0000-0000-000000000000";
            //var UpdUserId = " NEWID() ";// jo["UpdUserId"] == null ? "" : jo["UpdUserId"].ToString();
            //var UpdDateTime = "SYSDATETIME()";// DateTime.Now.ToString("yyyy /MM/dd HH:mm:ss");
            string sSql = $"INSERT INTO SSEC001_UserInfo (" +
                " UserId, UserCode, DeptNo, UserName, UserFullName, Telephone, " +
                " EMail,  StartDate, StopDate, StyleNo, UsedMark," +
                " Remark, PwdErrorTime, UpdUserId, UpdDateTime ) VALUES (  " +
                $" @UserId, @UserCode, @DeptNo, @UserName, @UserFullName,@Telephone," +
                $" @EMail, " +
                (jo["StartDate"] != null ? $"@StartDate," : $"null,") +
                (jo["StopDate"] != null ? $"@StopDate," : $"null,") +
                $"@StyleNo, @UsedMark," +
                $" @Remark, @PwdErrorTime, @UpdUserId, " + " SYSDATETIME() )";

            //-------sql para----start
            SqlParameter[] sqlParams = new SqlParameter[] {
                new SqlParameter("@UserId", SqlDbType.UniqueIdentifier),
                new SqlParameter("@UserCode", SqlDbType.VarChar),
                new SqlParameter("@DeptNo", SqlDbType.Char),
                new SqlParameter("@UserName", SqlDbType.NVarChar),
                new SqlParameter("@UserFullName", SqlDbType.NVarChar),
                new SqlParameter("@Telephone", SqlDbType.VarChar),
                new SqlParameter("@EMail", SqlDbType.VarChar),
                new SqlParameter("@StartDate", SqlDbType.DateTime2),
                new SqlParameter("@StopDate", SqlDbType.DateTime2),
                new SqlParameter("@StyleNo", SqlDbType.Int),
                new SqlParameter("@UsedMark", SqlDbType.Bit),
                new SqlParameter("@Remark", SqlDbType.VarChar),
                new SqlParameter("@PwdErrorTime", SqlDbType.Int),
                new SqlParameter("@UpdUserId", SqlDbType.UniqueIdentifier)
            };
            sqlParams[0].Value = UserId.ValidGuid();
            sqlParams[1].Value = UserCode.Valid();
            sqlParams[2].Value = DeptNo.Valid();
            sqlParams[3].Value = UserName.Valid();
            sqlParams[4].Value = UserFullName.Valid();
            sqlParams[5].Value = Telephone.Valid();
            sqlParams[6].Value = EMail.Valid();
            sqlParams[7].Value = StartDate.Valid();
            sqlParams[8].Value = StopDate.Valid();
            sqlParams[9].Value = StyleNo.ValidInt();
            sqlParams[10].Value = UsedMark.Valid();
            sqlParams[11].Value = Remark.Valid();
            sqlParams[12].Value = PwdErrorTime.ValidInt();
            sqlParams[13].Value = UpdUserId.ValidGuid();
            //-------sql para----end

            Log.Debug("新增SSE001_UserInfo:" + sSql);
            try
            {
                int iR = _db.ExecuteSql(sSql, sqlParams);

                replyData.code = "200";
                replyData.message = $"新增記錄完成。";
                try
                {
                    //-------sql para----start
                    SqlParameter[] sqlParams2 = new SqlParameter[] {
                        new SqlParameter("@UserId", SqlDbType.UniqueIdentifier),
                    };
                    sqlParams2[0].Value = UserId.ValidGuid();
                    //-------sql para----end

                    //執行成功後,需要將本筆資料帶回前端
                    var result = ExecuteQuery($"SELECT * FROM SSEC001_UserInfo WHERE UserId=@UserId ", sqlParams2);
                    if (result.Count == 1)
                        replyData.data = result[0];  //應該只有一筆
                    else
                        replyData.data = result;
                }
                catch (Exception ex)
                {
                    //新增成功，查詢失敗
                    Log.Debug("新增成功，再查詢時失敗," + ex.Message);
                    replyData.data = "";
                }
            }
            catch (Exception ex)
            {
                replyData.code = "-1";
                replyData.message = $"新增資料失敗！{ex.Message}.";
                replyData.data = "";
                Log.Error("新增SSEC001_UserInfo資料失敗!"+ex.Message);
            }
            //返回
            return JsonConvert.SerializeObject(replyData);

        }

        [Route("Update")]
        [HttpPut]
        //public IActionResult Update([FromBody] Object value)
        public String Update([FromBody] Object value)
        {
            JObject jo = (JObject)JsonConvert.DeserializeObject(value.ToString());
            var replyData = new ReplyData();
            //必須依UserId update?
            if (jo["UserId"]==null)
            {
                //報告錯誤
            }
            var UserId = jo["UserId"].ToString();

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

            string sWhereCondition = $" WHERE UserId=@UserId ";
            var obj1 = new SqlParameter("@UserId", SqlDbType.UniqueIdentifier);
            obj1.Value = UserId.ValidGuid();
            sqlParams.Add(obj1);

            string sSql = " UPDATE SSEC001_UserInfo SET ";
            if (jo["UserCode"] != null)
            {
                var UserCode = jo["UserCode"].ToString();
                sSql += $" UserCode=@UserCode,";

                var obj2 = new SqlParameter("@UserCode", SqlDbType.VarChar);
                obj2.Value = UserCode.Valid();
                sqlParams.Add(obj2);
            }
            if (jo["UserName"] != null)
            {
                var UserName = jo["UserName"].ToString();
                sSql += $" UserName=@UserName,";

                var obj2 = new SqlParameter("@UserName", SqlDbType.NVarChar);
                obj2.Value = UserName.Valid();
                sqlParams.Add(obj2);
            }
            if (jo["UserFullName"] != null)
            {
                var UserFullName = jo["UserFullName"].ToString();
                sSql += $" UserFullName=@UserFullName,";

                var obj2 = new SqlParameter("@UserFullName", SqlDbType.NVarChar);
                obj2.Value = UserFullName.Valid();
                sqlParams.Add(obj2);
            }
            if (jo["DeptNo"] != null)
            {
                var DeptNo = jo["DeptNo"].ToString();
                sSql += $" DeptNo=@DeptNo,";

                var obj2 = new SqlParameter("@DeptNo", SqlDbType.VarChar);
                obj2.Value = DeptNo.Valid();
                sqlParams.Add(obj2);
            }

            if (jo["Telephone"] != null)
            {
                var Telephone = jo["Telephone"].ToString();
                sSql += $" Telephone=@Telephone,";

                var obj2 = new SqlParameter("@Telephone", SqlDbType.VarChar);
                obj2.Value = Telephone.Valid();
                sqlParams.Add(obj2);
            }
            if (jo["EMail"] != null)
            {
                var EMail = jo["EMail"].ToString();
                sSql += $" EMail=@EMail,";

                var obj2 = new SqlParameter("@EMail", SqlDbType.VarChar);
                obj2.Value = EMail.Valid();
                sqlParams.Add(obj2);
            }
            if (jo["StartDate"] != null)
            {
                var StartDate = jo["StartDate"].ToString();
                sSql += $" StartDate=@StartDate,";

                var obj2 = new SqlParameter("@StartDate", SqlDbType.DateTime2);
                obj2.Value = StartDate.Valid();
                sqlParams.Add(obj2);
            }
            if (jo["StopDate"] != null)
            {
                var StopDate = jo["StopDate"].ToString();
                sSql += $" StopDate=@StopDate,";

                var obj2 = new SqlParameter("@StopDate", SqlDbType.DateTime2);
                obj2.Value = StopDate.Valid();
                sqlParams.Add(obj2);
            }
            if (jo["StyleNo"] != null)
            {
                var StyleNo = Convert.ToInt32(jo["StyleNo"]);
                sSql += $" StyleNo=@StyleNo,";

                var obj2 = new SqlParameter("@StyleNo", SqlDbType.Int);
                obj2.Value = StyleNo.ValidInt();
                sqlParams.Add(obj2);
            }
            if (jo["UsedMark"] != null)
            {
                var UsedMark = jo["UsedMark"].ToString();
                sSql += $" UsedMark=@UsedMark,";

                var obj2 = new SqlParameter("@UsedMark", SqlDbType.Bit);
                obj2.Value = UsedMark.Valid();
                sqlParams.Add(obj2);
            }
            if (jo["Remark"] != null)
            {
                var Remark = jo["Remark"].ToString();
                sSql += $" Remark=@Remark,";

                var obj2 = new SqlParameter("@Remark", SqlDbType.VarChar);
                obj2.Value = Remark.Valid();
                sqlParams.Add(obj2);
            }
            if (jo["PwdErrorTime"] != null)
            {
                var PwdErrorTime = Convert.ToInt32(jo["PwdErrorTime"]);
                sSql += $" PwdErrorTime=@PwdErrorTime,";

                var obj2 = new SqlParameter("@PwdErrorTime", SqlDbType.Int);
                obj2.Value = PwdErrorTime.ValidInt();
                sqlParams.Add(obj2);
            }
            //UpdUserId 會有程式依據Token取得，所以,目前暫時寫成00000000-0000-0000-0000-000000000000
            //var UpdUserId = "00000000-0000-0000-0000-000000000000";
            sSql += $" UpdUserId=@UpdUserId,";
            var obj = new SqlParameter("@UpdUserId", SqlDbType.UniqueIdentifier);
            obj.Value = UpdUserId.ValidGuid();
            sqlParams.Add(obj);
            //if (jo["UpdUserId"] != null)
            //{
            //    var UpdUserId = jo["UpdUserId"].ToString();
            //    sSql += $" UpdUserId=NEWID(),";
            //}

            //var UpdDateTime = DateTime.Now.ToString("yyyy/MM/dd HH:mm:ss");
             sSql += $" UpdDateTime=SYSDATETIME()";

            sSql += sWhereCondition;
            Log.Debug("修改SSEC001_UserInfo:" + sSql);
            try
            {
                int iR = _db.ExecuteSql(sSql, sqlParams.ToArray());

                replyData.code = "200";
                replyData.message = $"修改記錄完成。";
                try
                {
                    //-------sql para----start
                    SqlParameter[] sqlParams2 = new SqlParameter[] {
                        new SqlParameter("@UserId", SqlDbType.UniqueIdentifier),
                    };
                    sqlParams2[0].Value = UserId.ValidGuid();
                    //-------sql para----end

                    //執行成功後,需要將本筆資料帶回前端
                    var result = ExecuteQuery($"SELECT * FROM SSEC001_UserInfo WHERE UserId=@UserId ", sqlParams2);
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
            }
            catch (Exception ex)
            {
                replyData.code = "-1";
                replyData.message = $"更新資料失敗！{ex.Message}.";
                replyData.data = "";
                Log.Error("修改SSEC001_UserInfo資料失敗!"+ex.Message);
            }
            //return Ok(replyData);
            return JsonConvert.SerializeObject(replyData);
        }

        [Route("Delete")]
        [HttpDelete]
        //public IActionResult Delete([FromBody] Object value)
        public String Delete(String UserId)
        {
            //JObject jo = (JObject)JsonConvert.DeserializeObject(value.ToString());
            var replyData = new ReplyData();
            //string UserId = jo["UserId"].ToString();

            string sSql = string.Format("DELETE FROM SSEC001_UserInfo WHERE UserId=@UserId ");
            SqlParameter[] sqlParams = new SqlParameter[] { new SqlParameter("@UserId", SqlDbType.VarChar) };
            sqlParams[0].Value = new System.Data.SqlTypes.SqlChars(UserId);
            Log.Debug("刪除SSEC001_UserInfo:" + sSql);
            try
            {
                int iR = _db.ExecuteSql(sSql, sqlParams);
                replyData.code = "200";
                replyData.message = $"刪除資料成功！共{iR}筆。";
                replyData.data = "";
                //return Ok(iR);
            }
            catch(Exception ex)
            {
                replyData.code = "-1";
                replyData.message = $"刪除資料失敗！{ex.Message}.";
                replyData.data = "";
                Log.Error("刪除SSEC001_UserInfo資料失敗!"+ex.Message);
            }
            //return Ok(replyData);
            return JsonConvert.SerializeObject(replyData);
        }

        private List<SSEC001_UserInfo> ExecuteQuery(String sSql)
        {
            List<SSEC001_UserInfo> lstUserInfo = new List<SSEC001_UserInfo>();
            try
            {
                DataTable dtR = _db.GetQueryData(sSql);
                foreach (DataRow dr in dtR.Rows)
                {
                    SSEC001_UserInfo userInfo = new SSEC001_UserInfo();
                    userInfo.UserId = dr["UserId"];
                    userInfo.UserCode = dr["UserCode"];
                    userInfo.UserName = dr["UserName"];
                    userInfo.UserFullName = dr["UserFullName"];
                    userInfo.DeptNo = dr["DeptNo"];
                    userInfo.Telephone = dr["Telephone"];
                    userInfo.EMail = dr["EMail"];
                    userInfo.StartDate = dr["StartDate"];
                    userInfo.StopDate = dr["StopDate"];
                    userInfo.StyleNo = dr["StyleNo"];
                    userInfo.UsedMark = dr["UsedMark"];
                    userInfo.Remark = dr["Remark"];
                    userInfo.PwdErrorTime = dr["PwdErrorTime"];
                    userInfo.UpdUserId = dr["UpdUserId"];
                    userInfo.UpdDateTime = dr["UpdDateTime"];
                    userInfo.LoginDate = dr["LoginDate"];
                    userInfo.OTP = dr["OTP"];
                    userInfo.OTPTime = dr["OTPTime"];
                    //add to list
                    lstUserInfo.Add(userInfo);
                }
            }
            catch (Exception)
            {
                throw;
            }
            return lstUserInfo;
        }

        private List<SSEC001_UserInfo> ExecuteQuery(String sSql, SqlParameter[] cmdParams)
        {
            List<SSEC001_UserInfo> lstUserInfo = new List<SSEC001_UserInfo>();
            try
            {
                DataTable dtR = _db.GetQueryData(sSql, cmdParams);
                foreach (DataRow dr in dtR.Rows)
                {
                    SSEC001_UserInfo userInfo = new SSEC001_UserInfo();
                    userInfo.UserId = dr["UserId"];
                    userInfo.UserCode = dr["UserCode"];
                    userInfo.UserName = dr["UserName"];
                    userInfo.UserFullName = dr["UserFullName"];
                    userInfo.DeptNo = dr["DeptNo"];
                    userInfo.Telephone = dr["Telephone"];
                    userInfo.EMail = dr["EMail"];
                    userInfo.StartDate = dr["StartDate"];
                    userInfo.StopDate = dr["StopDate"];
                    userInfo.StyleNo = dr["StyleNo"];
                    userInfo.UsedMark = dr["UsedMark"];
                    userInfo.Remark = dr["Remark"];
                    userInfo.PwdErrorTime = dr["PwdErrorTime"];
                    userInfo.UpdUserId = dr["UpdUserId"];
                    userInfo.UpdDateTime = dr["UpdDateTime"];
                    userInfo.LoginDate = dr["LoginDate"];
                    userInfo.OTP = dr["OTP"];
                    userInfo.OTPTime = dr["OTPTime"];
                    //add to list
                    lstUserInfo.Add(userInfo);
                }
            }
            catch (Exception)
            {
                throw;
            }
            return lstUserInfo;
        }
    }
}
