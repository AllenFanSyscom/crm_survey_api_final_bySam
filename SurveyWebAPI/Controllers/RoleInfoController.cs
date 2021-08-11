using System;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using SurveyWebAPI.Models;
using SurveyWebAPI.Utility;
using System.Collections.Generic;
using System.Data;
using Common;
using log4net;
using Microsoft.AspNetCore.Authorization;
using System.Data.SqlClient;

namespace SurveyWebAPI.Controllers
{
    [Authorize]
    [Route("api/Role/Info")]
    [ApiController]
    public class RoleInfoController : Controller
    {
        private readonly DBHelper _db;

        public RoleInfoController()
        {
            _db = new DBHelper(AppSettingsHelper.DefaultConnectionString);
        }
        [Route("QueryAll")]
        [HttpGet]
        public object QueryAll()
        {
            Log.Debug("角色-查詢全部角色");
            //返回數據
            ReplyData replyData = new ReplyData();
            try
            {
                //data實體
                replyData.code = "200";
                replyData.message = $"查詢記錄完成。";
                replyData.data = QueryData("");
            }
            catch (Exception ex)
            {
                replyData.code = "-1";
                replyData.message = $"查詢記錄失敗!{ex.Message}.";
                replyData.data = null;
                Log.Error("查詢記錄失敗!" + ex.Message);
            }
            return JsonConvert.SerializeObject(replyData);
        }
        [Route("QueryOne")]
        [HttpGet]
        public object QueryOne(string roleId)
        {
            Log.Debug("角色-查詢全部角色");
            //返回數據
            ReplyData replyData = new ReplyData();
            try
            {
                //data實體
                replyData.code = "200";
                replyData.message = $"查詢記錄完成。";
                var dataRole = QueryData(roleId);
                if (dataRole != null && dataRole.Count == 1)
                    replyData.data = dataRole[0];
            }
            catch (Exception ex)
            {
                replyData.code = "-1";
                replyData.message = $"查詢記錄失敗!{ex.Message}.";
                replyData.data = null;
                Log.Error("查詢記錄失敗!" + ex.Message);
            }
            return JsonConvert.SerializeObject(replyData);
        }
        [Route("Insert")]
        [HttpPost]
        public object Insert([FromBody] Object value)
        {
            SSEC004_RoleId roleInfo = JsonConvert.DeserializeObject<SSEC004_RoleId>(value.ToString());

            var replyData = new ReplyData();
            try
            {
                var key = User.Identity.Name;
                var info = Utility.Common.GetConnectionInfo(key);
                if (info == null)
                {
                    //報告錯誤
                    replyData.code = "-1";
                    replyData.message = "用戶不存在!";
                    replyData.data = "";
                    return JsonConvert.SerializeObject(replyData);
                }
                var userId = info.UserId;

                string sqlS = string.Format("insert into SSEC004_RoleId(RoleName,RoleDescription,UsedMark,Remark,UpdUserId,UpdDateTime) values(@RoleName,@RoleDescription,@UsedMark,@Remark,@userId,SYSDATETIME()) ");

                //-------sql para----start
                SqlParameter[] sqlParams = new SqlParameter[] {
                new SqlParameter("@RoleName", SqlDbType.NVarChar),
                new SqlParameter("@RoleDescription", SqlDbType.NVarChar),
                new SqlParameter("@UsedMark", SqlDbType.Bit),
                new SqlParameter("@Remark", SqlDbType.NVarChar),
                new SqlParameter("@userId", SqlDbType.UniqueIdentifier),

                };
                sqlParams[0].Value = roleInfo.RoleName.Valid();
                sqlParams[1].Value = roleInfo.RoleDescription.Valid();
                sqlParams[2].Value = roleInfo.UsedMark.ValidBit();
                sqlParams[3].Value = roleInfo.Remark.Valid();
                sqlParams[4].Value = userId.ValidGuid();
                //-------sql para----end

                Log.Debug("新增SSEC004_RoleId:" + sqlS);
                int iR = _db.ExecuteSql(sqlS,sqlParams);

                replyData.code = "200";
                replyData.message = "新增記錄完成。";
                //var dataRole = QueryData(roleInfo.RoleId.ToString());
                //if(dataRole != null && dataRole.Count == 1)
                replyData.data = iR;
            }
            catch (Exception ex)
            {
                replyData.code = "-1";
                replyData.message = $"新增記錄失敗!{ex.Message}.";
                replyData.data = null;
                Log.Error("新增記錄失敗!" + ex.Message);
            }
            return JsonConvert.SerializeObject(replyData);
        }
        [Route("Update")]
        [HttpPut]
        public object Update([FromBody] Object value)
        {
            SSEC004_RoleId roleInfo = JsonConvert.DeserializeObject<SSEC004_RoleId>(value.ToString());

            var replyData = new ReplyData();
            try
            {
                var key = User.Identity.Name;
                var info = Utility.Common.GetConnectionInfo(key);
                if (info == null)
                {
                    //報告錯誤
                    replyData.code = "-1";
                    replyData.message = "用戶不存在!";
                    replyData.data = "";
                    return JsonConvert.SerializeObject(replyData);
                }
                var userId = info.UserId;

                string sqlS = string.Format("Update SSEC004_RoleId set RoleName=@RoleName,RoleDescription=@RoleDescription,UsedMark=@UsedMark,Remark=@Remark,UpdUserId=@userId, UpdDateTime=SYSDATETIME() where RoleId=@RoleId ");

                //-------sql para----start
                SqlParameter[] sqlParams = new SqlParameter[] {
                new SqlParameter("@RoleName", SqlDbType.NVarChar),
                new SqlParameter("@RoleDescription", SqlDbType.NVarChar),
                new SqlParameter("@UsedMark", SqlDbType.Bit),
                new SqlParameter("@Remark", SqlDbType.NVarChar),
                new SqlParameter("@userId", SqlDbType.UniqueIdentifier),
                new SqlParameter("@RoleId", SqlDbType.Int),

                };
                sqlParams[0].Value = roleInfo.RoleName.Valid();
                sqlParams[1].Value = roleInfo.RoleDescription.Valid();
                sqlParams[2].Value = roleInfo.UsedMark.ValidBit();
                sqlParams[3].Value = roleInfo.Remark.Valid();
                sqlParams[4].Value = userId.ValidGuid();
                sqlParams[5].Value = roleInfo.RoleId.ValidInt();
                //-------sql para----end

                Log.Debug("修改SSEC004_RoleId:" + sqlS);
                int iR = _db.ExecuteSql(sqlS,sqlParams);

                replyData.code = "200";
                replyData.message = "修改記錄完成。";
                //var dataRole = QueryData(roleInfo.RoleId.ToString());
                //if (dataRole != null && dataRole.Count == 1)
                replyData.data = iR;
            }
            catch (Exception ex)
            {
                replyData.code = "-1";
                replyData.message = $"修改記錄失敗!{ex.Message}.";
                replyData.data = null;
                Log.Error("修改記錄失敗!" + ex.Message);
            }
            return JsonConvert.SerializeObject(replyData);
        }
        [Route("Delete")]
        [HttpDelete]
        public object Delete(string roleId)
        {
            var replyData = new ReplyData();
            try
            {
                string sqlS = string.Format("Delete from SSEC004_RoleId where RoleId=@roleId ");

                //-------sql para----start
                SqlParameter[] sqlParams = new SqlParameter[] {
                new SqlParameter("@roleId", SqlDbType.Int),
                };
                sqlParams[0].Value = roleId.ValidInt();

                //-------sql para----end

                Log.Debug("刪除SSEC004_RoleId:" + sqlS);
                int iR = _db.ExecuteSql(sqlS,sqlParams);

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

        protected List<SSEC004_RoleId> QueryData(string roleId)
        {
            //data實體
            List<SSEC004_RoleId> lstRole = new List<SSEC004_RoleId>();

            try
            {
                string sqlS = "select * from SSEC004_RoleId where 1=1";
                if (!string.IsNullOrEmpty(roleId))
                {
                    sqlS += string.Format(" and RoleId=@roleId");

                }

                //-------sql para----start
                SqlParameter[] sqlParams = new SqlParameter[] {
                    new SqlParameter("@roleId", SqlDbType.Int),
                    };
                sqlParams[0].Value = roleId.ValidInt();

                //-------sql para----end
                DataTable dtR = _db.GetQueryData(sqlS, sqlParams);



                if (dtR != null && dtR.Rows.Count > 0)
                {
                    //Fill data
                    foreach (DataRow dr in dtR.Rows)
                    {
                        SSEC004_RoleId roleInfo = new SSEC004_RoleId();
                        roleInfo.RoleId = Convert.ToInt32(dr["RoleId"]);
                        roleInfo.RoleName = dr["RoleName"].ToString();
                        roleInfo.RoleDescription = dr["RoleDescription"].ToString();
                        roleInfo.UsedMark = dr["UsedMark"].ToString();
                        roleInfo.Remark = dr["Remark"].ToString();
                        roleInfo.UpdUserId = dr["UpdUserId"].ToString();
                        roleInfo.UpdDateTime = dr["UpdDateTime"].ToString();
                        lstRole.Add(roleInfo);
                    }
                    return lstRole;
                }
                else
                {
                    return null;
                }
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }
    }
}
