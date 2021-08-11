using Common;
using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using SurveyWebAPI.Controllers;

namespace SurveyWebAPI.Utility
{
    /// <summary>
    /// 写一些共用的Method
    /// </summary>
    public static class Common
    {
        private static DBHelper _db;
        private static DBHelper _dbCRM;
        static Common()
        {
            _db = new DBHelper(AppSettingsHelper.DefaultConnectionString);
            _dbCRM = new DBHelper(AppSettingsHelper.CRMConnectionString);
        }
        /// <summary>
        /// Check 问卷是否存在
        /// </summary>
        /// <param name="SurveyId"></param>
        /// <returns>true: 存在，false: 不存在</returns>
        public static bool IsSurveyIdExist(String surveyId)
        {
            string sSql = $"SELECT COUNT(1) FROM QUE001_QuestionnaireBase WHERE SurveyId=@surveyId";

            //-------sql para----start
            SqlParameter[] sqlParams = new SqlParameter[] {
                new SqlParameter("@surveyId", SqlDbType.UniqueIdentifier),

                };
            sqlParams[0].Value = surveyId.ValidGuid();

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
        /// <summary>
        /// Check 题目是否存在
        /// </summary>
        /// <param name="surveyId"> Survey ID</param>
        /// <param name="questionId">Question ID</param>
        /// <returns>true: 存在， false: 不存在</returns>
        public static bool IsQuestionIdExists(String surveyId, String questionId)
        {
            string sSql = $"SELECT COUNT(1) FROM QUE002_QuestionnaireDetail WHERE SurveyId=@surveyId AND QuestionId=@questionId ";

            //-------sql para----start
            SqlParameter[] sqlParams = new SqlParameter[] {
                new SqlParameter("@surveyId", SqlDbType.UniqueIdentifier),
                new SqlParameter("@questionId", SqlDbType.UniqueIdentifier),

                };
            sqlParams[0].Value = surveyId.ValidGuid();
            sqlParams[1].Value = questionId.ValidGuid();

            //-------sql para----end

            try
            {
                var result = _db.GetSingle(sSql,sqlParams);
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
        /// <summary>
        /// Check傳入UserId是否已存在
        /// </summary>
        /// <param name="UserId"></param>
        /// <returns></returns>
        public static bool IsUserIdExists(object UserId)
        {
            Guid resultId;
            var isGuid = Guid.TryParse(UserId.ToString(), out resultId);

            //-------sql para----start
            List<SqlParameter> sqlParams = new List<SqlParameter>();
            //-------sql para----end
            string sSql = "";
            if (!isGuid)
            {
                sSql += $"SELECT COUNT(1) FROM SSEC001_UserInfo WHERE UserCode=@UserId AND UsedMark='1' ";

                var obj = new SqlParameter("@UserId", SqlDbType.VarChar);
                obj.Value = UserId.Valid();
                sqlParams.Add(obj);
            }
            else
            {
                sSql += $"SELECT COUNT(1) FROM SSEC001_UserInfo WHERE UserId=@resultId AND UsedMark='1' ";

                var obj = new SqlParameter("@resultId", SqlDbType.UniqueIdentifier);
                obj.Value = UserId.ToString().ValidGuid();
                sqlParams.Add(obj);
            }

            try
            {
                var result = _db.GetSingle(sSql, sqlParams.ToArray());
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
        public static DataTable GetCRMUserInfoBy(String userId)
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
                Log.Debug("從CRM取得UserInfo, GetCRMUserInfoBy sql:" + sSql);
                return _dbCRM.GetQueryData(sSql,sqlParams);
            }
            catch (Exception ex)
            {
                Log.Error("從CRM取得UserInfo: GetCRMUserInfoBy fail！" + ex.Message);
                Log.Error(ex.StackTrace);
                throw ex;
            }
        }
        /// <summary>
        /// 利用token資料，取得用戶信息
        /// </summary>
        /// <param name="token">token值</param>
        /// <returns>回傳連線相關訊息(用戶資料)</returns>
        public static ConnectionInfo GetConnectionInfo(string key)
        {
            Guid resultId;
            var isGuid = Guid.TryParse(key, out resultId);

            //-------sql para----start
            List<SqlParameter> sqlParams = new List<SqlParameter>();
            //-------sql para----end

            string sSql = "";
            if (!isGuid)
            {
                sSql += "  SELECT u.UserId,u.UserCode,u.UserName,r.RoleId, r.RoleName from SSEC001_UserInfo u "
                                + " LEFT JOIN SSEC005_UserRole ur on u.UserId = ur.UserId "
                                + " LEFT JOIN SSEC004_RoleId r on ur.RoleId = r.RoleId "
                                + $"WHERE u.UserCode = @key AND u.UsedMark = '1' ";

                var obj = new SqlParameter("@key", SqlDbType.VarChar);
                obj.Value = key.Valid();
                sqlParams.Add(obj);
            }
            else
            {
                sSql += "  SELECT u.UserId,u.UserCode,u.UserName,r.RoleId, r.RoleName from SSEC001_UserInfo u "
                                + " LEFT JOIN SSEC005_UserRole ur on u.UserId = ur.UserId "
                                + " LEFT JOIN SSEC004_RoleId r on ur.RoleId = r.RoleId "
                                + $"WHERE u.UserId = @resultId AND u.UsedMark = '1'  ";


                var obj = new SqlParameter("@resultId", SqlDbType.UniqueIdentifier);
                obj.Value = key.ValidGuid();
                sqlParams.Add(obj);
            }

            try
            {
                DataTable dtR = _db.GetQueryData(sSql,sqlParams.ToArray());

                if (dtR.Rows.Count > 0)
                    //只取第一筆資料回傳。
                    return new ConnectionInfo()
                    {
                        UserId = dtR.Rows[0]["UserId"].ToString(),
                        UserCode = dtR.Rows[0]["UserCode"].ToString(),
                        UserName = dtR.Rows[0]["UserName"].ToString(),
                        RoleId = Convert.ToInt32( String.IsNullOrEmpty(dtR.Rows[0]["RoleId"].ToString().Trim())?"2": dtR.Rows[0]["RoleId"]),
                        RoleName = dtR.Rows[0]["RoleName"].ToString()
                    };

                else
                    return null;
            }
            catch (Exception ex)
            {
                Log.Error("從問卷平台取得用戶訊息: GetConnectionInfo！" + ex.Message);
                Log.Error(ex.StackTrace);
                throw ex;
            }
        }

        /// <summary>
        /// Base64解密
        /// </summary>
        /// <param name="input">需要解密的字串</param>
        /// <returns></returns>
        public static string Base64Decrypt(string input)
        {
            return Base64Decrypt(input, new UTF8Encoding());
        }

        /// <summary>
        /// Base64解密
        /// </summary>
        /// <param name="input">需要解密的字串</param>
        /// <param name="encode">字元的編碼</param>
        /// <returns></returns>
        public static string Base64Decrypt(string input, Encoding encode)
        {
            return encode.GetString(Convert.FromBase64String(input));
        }

        public static void CleanQuestionId(string questionId, string surveyId)
        {
            try
            {
                String _sql = string.Format("EXEC DeleteRelatedQuestionId @questionId,@surveyId");

                //-------sql para----start
                SqlParameter[] sqlParams = new SqlParameter[] {
                new SqlParameter("@questionId", SqlDbType.UniqueIdentifier),
                new SqlParameter("@surveyId", SqlDbType.UniqueIdentifier),

                };
                sqlParams[0].Value = questionId.Valid();
                sqlParams[1].Value = surveyId.Valid();

                //-------sql para----end

                Log.Debug("CleanQuestionId:" + _sql);
                _db.ExecuteSql(_sql,sqlParams);
            }
            catch (Exception ex)
            {
                Log.Debug("CleanQuestionId Exception:" + ex.Message);
            }

        }

        /// <summary>
        /// SHA256加密-不可逆
        /// </summary>
        /// <param name="input">需要加密的字串</param>
        /// <returns></returns>

        public static string SHA256Encrypt(string input)
        {

            var bytes = System.Text.Encoding.Default.GetBytes(input);

            var SHA256 = new System.Security.Cryptography.SHA256CryptoServiceProvider();

            var encryptbytes = SHA256.ComputeHash(bytes);

            return Convert.ToBase64String(encryptbytes);
        }

        public static bool IsHandset(string str_handset)
        {
            return System.Text.RegularExpressions.Regex.IsMatch(str_handset, @"^09[0-9]{8}$");
        }



        public class ConnectionInfo
        {
            public string UserId { get; set; }
            public string UserCode { get; set; }
            public string UserName { get; set; }
            public string RoleName { get; set; }
            public int RoleId { get; set; }
        }

        public static string removeSpecialCharactersPath(string str)
        {
            string returnvalue = "";
            string pattern = "([A-Z]|[a-z]|[]|\\d|\\s|[+,-\\\\.*()_\"'|:<>@!#$%^&={}]|[\u4e00-\u9fa5])";
            Regex regex = new Regex(pattern, RegexOptions.IgnoreCase);
            MatchCollection a = regex.Matches(str);
            for (int i = 0; i < a.Count; i++)
            {
                returnvalue += a[i].Value.ToString();
            }
            return returnvalue;
        }

        public static Boolean checkUserId(String UpdUserId)
        {
            if (UpdUserId == null)
                return false;
            return true;
        }
    }
}
