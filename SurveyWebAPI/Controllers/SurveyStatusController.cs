using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Threading.Tasks;
using Common;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Newtonsoft.Json;
using SurveyWebAPI.Models;
using SurveyWebAPI.Utility;
using System.Data.SqlClient;

namespace SurveyWebAPI.Controllers
{
    [Authorize]
    [Route("api/Survey/Status")]
    [ApiController]
    public class SurveyStatusController : ControllerBase
    {
        /// <summary>
        /// 問卷系統後臺之主畫面操作--Get可選的問卷狀態
        /// </summary>
        /// <designer>Allen/Gem</designer>
        private DBHelper _db;
        public SurveyStatusController()
        {
            _db = new DBHelper(AppSettingsHelper.DefaultConnectionString);
        }
        /// <summary>
        /// GET 可選的問卷狀態
        /// </summary>
        /// <returns></returns>
        [Route("List")]
        [HttpGet]
        public String GetSurveyStatusList()
        {
            Log.Debug("主畫面操作-取得可選的問卷狀態...");
            /*
             * GEN004_AllCode, CodeCode = 0102
             */

            List<SurveyStatus> lstStatus = new List<SurveyStatus>();
            ReplyData replyData = new ReplyData();
            var codeCode = "0102";
            string sSql = $"SELECT * FROM GEN004_AllCode WHERE CodeCode=@codeCode "+
                " AND UsedMark='1' ORDER BY Cast(CodeSubCode as int) ";
            //-------sql para----start
            SqlParameter[] sqlParams = new SqlParameter[] {
                new SqlParameter("@codeCode", SqlDbType.Char)
            };
            sqlParams[0].Value = codeCode.Valid();
            //-------sql para----end
            try
            {
                DataTable dtR = _db.GetQueryData(sSql, sqlParams);
                foreach (DataRow dr in dtR.Rows)
                {
                    SurveyStatus suvstatus = new SurveyStatus();
                    suvstatus.status = dr["CodeSubCode"];
                    suvstatus.description = dr["CodeSubName"];

                    lstStatus.Add(suvstatus);
                }

                replyData.code = "200";
                replyData.message = $"資料取得成功。共{lstStatus.Count}筆。";
                Log.Debug($"資料取得成功。共{lstStatus.Count}筆。");
                //先不要SerializeObject list 應該也可以
                replyData.data = lstStatus;  // JsonConvert.SerializeObject(lstBaseicSetting);
            }
            catch (Exception ex)
            {
                replyData.code = "-1";
                replyData.message = $"資料取得失敗！{ex.Message}.";
                replyData.data = "";
                Log.Error("資料取得失敗!" + ex.Message);
            }

            //返回結果
            return JsonConvert.SerializeObject(replyData);
            //return lstUserInfo.ToArray();
        }
    }
    /// <summary>
    /// 可選問卷狀態
    /// </summary>
    public class SurveyStatus
    {
        /// <summary>
        /// 狀態
        /// </summary>
        public Object status { get; set; }
        /// <summary>
        /// 描述
        /// </summary>
        public Object description { get; set; }
    }
}
