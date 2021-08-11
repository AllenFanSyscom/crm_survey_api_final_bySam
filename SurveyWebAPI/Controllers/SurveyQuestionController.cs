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
using Microsoft.AspNetCore.Authorization;
using System.Data.SqlClient;

namespace SurveyWebAPI.Controllers
{
    [Authorize]
    [Route("api/Survey/Question")]
    [ApiController]
    public class SurveyQuestionController : ControllerBase
    {
        /// <summary>
        /// 問卷系統後臺--主畫面操作--問卷題型排序
        /// </summary>
        /// <designer>Allen Fan/Gem He</design>
        /// <History> 1. 2020/09/3 Gem Create
        /// </History>

        //private readonly ILogger<SurveyQuestionTextController> _logger;  //可實現log在console輸出
        private DBHelper _db;
        //public SurveyQuestionTextController(ILogger<SurveyQuestionTextController> logger)
        public SurveyQuestionController()
        {
            //_logger = logger;
            //_db = new DBHelper();
            //_logger = logger;
            //_connectionOptions = options.Value;
            _db = new DBHelper(AppSettingsHelper.DefaultConnectionString);
        }
        /// <summary>
        /// 主畫面操作--問卷題型排序
        /// </summary>
        /// <param name="value"></param>
        /// <returns></returns>
        [Route("Sort")]
        [HttpPut]
        public String Sort([FromBody] Object value)
        {
            /* 輸入格式：
             * {
             *   "SurveyId": "99999999-0000-0000-0000-000000000000",
             *   "QuestionLists": [
             *       {
             *           "QuestionId": "99999999-0000-0000-0000-000000000000",
             *           "QuestionSeq": 1
             *       },
             *       {
             *           "QuesionId": "99999999-0000-0000-0000-000000000000",
             *           "QuestionSeq": 2
             *       }
             *   ]
             * }
             */
            //"Newtonsoft.Json.Linq.JArray"
            //"Newtonsoft.Json.Linq.JObject"
            //多筆資料的話，此處需要處理，暫不管
            //if(value.GetType().Name=="JArray")
            //{
            //    foreach (var val in value as JArray)
            //    {
            //        InsertOne(val);
            //    }
            //}
            JObject jo = (JObject)JsonConvert.DeserializeObject(value.ToString());
            var replyData = new ReplyData();

            if (jo["SurveyId"] == null || String.IsNullOrWhiteSpace(jo["SurveyId"].ToString()))
            {
                //報告錯誤
                replyData.code = "-1";
                replyData.message = $"問卷題型排序失敗！參數SurveyId不能為空！";
                replyData.data = "";
                Log.Error("問卷題型排序失敗！" + "參數SurveyId不能為空！");
                return JsonConvert.SerializeObject(replyData);
            }
            var SurveyId = jo["SurveyId"].ToString();

            //-------sql para----start
            SqlParameter[] sqlParams = new SqlParameter[] {
                    new SqlParameter("@SurveyId", SqlDbType.UniqueIdentifier),

                    };
            sqlParams[0].Value = SurveyId.ValidGuid();


            //-------sql para----end



            var i = _db.GetSingle($"SELECT COUNT(1) FROM QUE002_QuestionnaireDetail WHERE SurveyId=@SurveyId", sqlParams);
            if(i=="0")
            {
                //報告錯誤
                ErrorCode.Code = "202";
                replyData.code = ErrorCode.Code;
                replyData.message = ErrorCode.Message;

                //replyData.code = "-1";
                //replyData.message = $"問卷題型排序失敗！問卷'{SurveyId}'不存在！";
                replyData.data = "";
                Log.Error("問卷題型排序失敗！" + $"問卷題型排序失敗！問卷'{SurveyId}'不存在！");
                return JsonConvert.SerializeObject(replyData);
            }

            if (jo["QuestionLists"] == null)
            {
                //報告錯誤
                replyData.code = "-1";
                replyData.message = $"問卷題型排序失敗！參數QuestionLists不能為空！";
                replyData.data = "";
                Log.Error("問卷題型排序失敗！" + "參數QuestionLists不能為空！");
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
            //var UpdUserId = jo["UpdUserId"] == null ? "00000000-0000-0000-0000-000000000000" : jo["UpdUserId"].ToString();
            //var UpdUserId = "00000000-0000-0000-0000-000000000000";
            var arrQuestion = jo["QuestionLists"];

            if (arrQuestion.Count() < 1)
            {
                Log.Error("參數問卷題型為空白，不用排序啦！");
                ErrorCode.Code = "205";
                replyData.code = ErrorCode.Code;
                replyData.message = ErrorCode.Message;
                //replyData.code = "-1";
                //replyData.message = $"問卷題型排序失敗！參數問卷題型列表為空白！";
                replyData.data = "";
                return JsonConvert.SerializeObject(replyData);
            }
            //開始新增
            var sqls = new List<KeyValuePair<string, SqlParameter[]>>();

            //var questionItems = "";
            foreach (var question in arrQuestion)
            {
                JObject jo2 = (JObject)JsonConvert.DeserializeObject(question.ToString());
                if (jo2["QuestionId"] == null)
                {
                    //報告錯誤
                    replyData.code = "-1";
                    replyData.message = $"問卷題型排序失敗！參數QuestionId不能為空！";
                    replyData.data = "";
                    Log.Error("問卷題型排序失敗！" + "參數QuestionId不能為空！");
                    return JsonConvert.SerializeObject(replyData);
                }
                var QuestionId= jo2["QuestionId"];
                //返回的結果中只包含該問卷下輸入的題目，還是該問卷下所有的題目？
                //questionItems += $"'{QuestionId}',";
                if (jo2["QuestionSeq"] == null)
                {
                    //報告錯誤
                    replyData.code = "-1";
                    replyData.message = $"問卷題型排序失敗！參數QuestionSeq不能為空！";
                    replyData.data = "";
                    Log.Error("問卷題型排序失敗！" + "參數參數QuestionSeq不能為空！");
                    return JsonConvert.SerializeObject(replyData);
                }

                var QuestionSeq= jo2["QuestionSeq"];
                var sSql = " UPDATE QUE002_QuestionnaireDetail "+
                    $" SET QuestionSeq=@QuestionSeq, UpdUserId=@UpdUserId, " +
                    " UpdDateTime=SYSDATETIME() "+
                    $" WHERE SurveyId=@SurveyId AND QuestionId=@QuestionId" ;

                //-------sql para----start
                SqlParameter[] sql1Params = new SqlParameter[] {
                    new SqlParameter("@QuestionSeq", SqlDbType.Int),
                    new SqlParameter("@UpdUserId", SqlDbType.UniqueIdentifier),
                    new SqlParameter("@SurveyId", SqlDbType.UniqueIdentifier),
                    new SqlParameter("@QuestionId", SqlDbType.UniqueIdentifier),

                    };
                sql1Params[0].Value = QuestionSeq.ValidStrOrDBNull();
                sql1Params[1].Value = UpdUserId.ValidGuid();
                sql1Params[2].Value = SurveyId.ValidGuid();
                sql1Params[3].Value = new System.Data.SqlTypes.SqlGuid(QuestionId.ToString());

                //-------sql para----end

                sqls.Add(new KeyValuePair<string, SqlParameter[]>(sSql, sql1Params));

            }
            //questionItems = questionItems.Remove(questionItems.LastIndexOf(','));

            try
            {
                int iR = _db.ExecuteSqlTran(sqls);

                replyData.code = "200";
                replyData.message = $"排序儲存完畢。";
                //排序後，查詢所有問卷下的題目？
                try
                {
                    //-------sql para----start
                    SqlParameter[] sql2Params = new SqlParameter[] {
                    new SqlParameter("@SurveyId", SqlDbType.UniqueIdentifier),

                    };
                    sql2Params[0].Value = SurveyId.ValidGuid();


                    //-------sql para----end
                    var querySql = $"select QuestionId,QuestionSeq from QUE002_QuestionnaireDetail where SurveyId=@SurveyId " +
                        //" and QuestionId in("+ questionItems+") " +
                        " order by QuestionSeq";
                    var result = ExecuteQuery(SurveyId, querySql, sql2Params);
                    replyData.data = result;
                }
                catch (Exception ex)
                {
                    //新增成功，查詢失敗
                    replyData.data = value;
                    Log.Debug("排序儲存完畢,查詢返回結果失敗!" + ex.Message);
                }
                //還是直接返回輸入的Json資料就可以？------Allen說全部
                //replyData.data = value;

            }
            catch (Exception ex)
            {
                replyData.code = "-1";
                replyData.message = $"排序儲存完畢失敗！{ex.Message}.";
                replyData.data = "";
                Log.Error("排序儲存完畢失敗!" + ex.Message);
            }
            //返回
            return JsonConvert.SerializeObject(replyData);

        }

        private SurveyQuestionSort ExecuteQuery(string surveyId,String sSql)
        {
            //只有一筆問卷資料
            SurveyQuestionSort sqs = new SurveyQuestionSort();
            sqs.SurveyId = surveyId;
            try
            {
                DataTable dtR = _db.GetQueryData(sSql);

                List<QuestionList> lstql = new List<QuestionList>();
                foreach (DataRow dr in dtR.Rows)
                {
                    QuestionList ql = new QuestionList();
                    ql.QuestionId = dr["QuestionId"];
                    ql.QuestionSeq = dr["QuestionSeq"];

                    lstql.Add(ql);
                }
                sqs.QuestionLists = lstql;
            }
            catch (Exception)
            {
                throw;
            }
            return sqs;
        }

        private SurveyQuestionSort ExecuteQuery(string surveyId, String sSql, SqlParameter[] cmdParams)
        {
            //只有一筆問卷資料
            SurveyQuestionSort sqs = new SurveyQuestionSort();
            sqs.SurveyId = surveyId;
            try
            {
                DataTable dtR = _db.GetQueryData(sSql, cmdParams);

                List<QuestionList> lstql = new List<QuestionList>();
                foreach (DataRow dr in dtR.Rows)
                {
                    QuestionList ql = new QuestionList();
                    ql.QuestionId = dr["QuestionId"];
                    ql.QuestionSeq = dr["QuestionSeq"];

                    lstql.Add(ql);
                }
                sqs.QuestionLists = lstql;
            }
            catch (Exception)
            {
                throw;
            }
            return sqs;
        }
    }
    public class SurveyQuestionSort
    {
        //以後如果有補充欄位從Models->QUE002 copy
        public Object SurveyId { get; set; }
        public List<QuestionList> QuestionLists { get; set; }
    }
    public class QuestionList
    {
        public Object QuestionId { get; set; }
        public Object QuestionSeq { get; set; }
    }
}
