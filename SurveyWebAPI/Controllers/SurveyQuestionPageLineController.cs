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
using Microsoft.AspNetCore.Authorization;
using System.Data.SqlClient;

namespace SurveyWebAPI.Controllers
{
    [Authorize]
    [Route("api/Survey/Question/PageLine")]
    [ApiController]
    public class SurveyQuestionPageLineController : ControllerBase
    {
        /// <summary>
        /// 問卷系統後臺--設計問卷--分頁題型
        /// </summary>
        /// <designer>Allen Fan/Gem He</design>
        /// <History> 1. 2020/08/18 Gem Create
        /// </History>


        //private readonly ILogger<SurveyQuestionPageLineController> _logger;  //可實現log在console輸出
        private DBHelper _db;
        //public SurveyQuestionPageLineController(ILogger<SurveyQuestionPageLineController> logger)
        public SurveyQuestionPageLineController()
        {
            //_logger = logger;
            //_db = new DBHelper();
            //_logger = logger;
            //_connectionOptions = options.Value;
            _db = new DBHelper(AppSettingsHelper.DefaultConnectionString);
        }
        #region 分頁題型--沒有查詢API??
        /// <summary>
        /// 設計問卷-影片題型--查詢
        /// </summary>
        /// <returns></returns>
        //[Route("query")]
        //[HttpGet]
        //public String Query(String SurveyId, String QuestionId)
        //{
        //    Log.Debug("設計問卷-文字題型--查詢...");

        //    List<SurveyQuestionPageLine> lstQDetail = new List<SurveyQuestionPageLine>();
        //    ReplyData replyData = new ReplyData();
        //    string sWhere = " WHERE 1=1 ";
        //    if (SurveyId != null)
        //        sWhere += $" AND SurveyId='{SurveyId}'";
        //    if (QuestionId != null)
        //        sWhere += $" AND QuestionId='{QuestionId}'";

        //    string sSql = "SELECT * FROM QUE002_QuestionnaireDetail " + sWhere;
        //    try
        //    {
        //        lstQDetail = ExecuteQuery(sSql);
        //        replyData.code = "200";
        //        replyData.message = $"查詢記錄完成。";
        //        Log.Debug($"查詢記錄完成。共{lstQDetail.Count}筆。");
        //        //先不要SerializeObject list 應該也可以
        //        if (lstQDetail.Count == 1)  //SurveyId+QuestionId 應該只有一筆
        //            replyData.data = lstQDetail[0];  // JsonConvert.SerializeObject(lstBaseicSetting);
        //        else
        //            replyData.data = lstQDetail;
        //    }
        //    catch (Exception ex)
        //    {
        //        replyData.code = "-1";
        //        replyData.message = $"基本資料-單題查詢失敗！{ex.Message}.";
        //        replyData.data = "";
        //        Log.Error("基本資料-單題查詢記錄失敗!" + ex.Message);
        //    }

        //    //返回結果
        //    return JsonConvert.SerializeObject(replyData);
        //    //return lstUserInfo.ToArray();
        //}
        #endregion
        /// <summary>
        /// 設計問卷-分頁題型-新增
        /// </summary>
        /// <param name="value"></param>
        /// <returns></returns>
        [Route("Insert")]
        [HttpPost]
        public String Insert([FromBody] Object value)  //暫時以Body Json的形式傳遞參數
        {
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
            //應該會傳入SurveyId,QuestionType, PageNo三個欄位
            JObject jo = (JObject)JsonConvert.DeserializeObject(value.ToString());
            var replyData = new ReplyData();
            try
            {
                //UserId 必須有?
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
                // var uuid = Guid.NewGuid().ToString();

                    //報告錯誤
                    replyData.code = "-1";
                    replyData.message = $"新增記錄失敗！參數SurveyId不能為空！";
                    replyData.data = "";
                    Log.Error("圖片題型-新增失敗!" + "參數SurveyId不能為空！");
                    return JsonConvert.SerializeObject(replyData);
                }
            var SurveyId = jo["SurveyId"].ToString();

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

            //Question 自動產生
            var QuestionId = Guid.NewGuid().ToString();
            var QuestionSeq = 0; //這個需要自動累加
            var QuestionType = jo["QuestionType"] == null ? 13 : Convert.ToInt32(jo["QuestionType"]);
            var QuestionSubject = jo["QuestionSubject"] == null ? "" : jo["QuestionSubject"].ToString();
            var SubjectStyle = jo["SubjectStyle"] == null ? "" : jo["SubjectStyle"].ToString();
            var QuestionNote = jo["QuestionNote"] == null ? "" : jo["QuestionNote"].ToString();
            var PageNo = jo["PageNo"] == null ? 1 : Convert.ToInt32(jo["PageNo"]);
            var IsRequired = jo["IsRequired"] == null ? "0" : jo["IsRequired"].ToString();
            var HasOther = jo["HasOther"] == null ? "0" : jo["HasOther"].ToString();
            var OtherIsShowText = jo["OtherIsShowText"] == null ? "0" : jo["OtherIsShowText"].ToString();
            var OtherVerify = jo["OtherVerify"] == null ? 0 : Convert.ToInt32(jo["OtherVerify"]);
            var OtherTextMandatory = jo["OtherTextMandatory"] == null ? "0" : jo["OtherTextMandatory"].ToString();
            var OtherCheckMessage = jo["OtherCheckMessage"] == null ? "" : jo["OtherCheckMessage"].ToString();
            var IsSetShowNum = jo["IsSetShowNum"] == null ? "0" : jo["IsSetShowNum"].ToString();
            var PCRowNum = jo["PCRowNum"] == null ? 0 : Convert.ToInt32(jo["PCRowNum"]);
            var MobileRowNum = jo["MobileRowNum"] == null ? 0 : Convert.ToInt32(jo["MobileRowNum"]);
            var IsRamdomOption = jo["IsRamdomOption"] == null ? "0" : jo["IsRamdomOption"].ToString();
            var ExcludeOther = jo["ExcludeOther"] == null ? "0" : jo["ExcludeOther"].ToString();
            var BaseDataValidType = jo["BaseDataValidType"] == null ? 0 : Convert.ToInt32(jo["BaseDataValidType"]);
            var BlankDefaultWords = jo["BlankDefaultWords"] == null ? "" : jo["BlankDefaultWords"].ToString();
            var BlankValidType = jo["BlankValidType"] == null ? 0 : Convert.ToInt32(jo["BlankValidType"]);
            var MatrixItems = jo["MatrixItems"] == null ? "" : jo["MatrixItems"].ToString();
            var BlankMaxLimit = jo["BlankMaxLimit"] == null ? "0" : jo["BlankMaxLimit"].ToString();
            var BlankMinLimit = jo["BlankMinLimit"] == null ? "0" : jo["BlankMinLimit"].ToString();
            var QuestionImage = jo["QuestionImage"] == null ? "" : jo["QuestionImage"].ToString();
            var QuestionVideo = jo["QuestionVideo"] == null ? "" : jo["QuestionVideo"].ToString();
            var MultiOptionLimit = jo["MultiOptionLimit"] == null ? "" : jo["MultiOptionLimit"].ToString();
            //UpdUserId 會有程式依據Token取得，所以,目前暫時寫成00000000-0000-0000-0000-000000000000
            //var UpdUserId = jo["UpdUserId"] == null ? "00000000-0000-0000-0000-000000000000" : jo["UpdUserId"].ToString();
            //var UpdUserId = "00000000-0000-0000-0000-000000000000";
            //UpdDateTime 為datetime2: yyyy-MM-dd HH:mm:ss.ffffffff
            //var UpdDateTime = DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss.fffffff");


            //開始新增
            string sSql = " INSERT INTO QUE002_QuestionnaireDetail " +
                " (SurveyId, QuestionId, QuestionSeq, QuestionType, QuestionSubject, " +
                " SubjectStyle, QuestionNote, PageNo, IsRequired, HasOther, " +
                " OtherIsShowText, OtherVerify, OtherTextMandatory, OtherCheckMessage, IsSetShowNum," +
                " PCRowNum, MobileRowNum, IsRamdomOption, ExcludeOther, BaseDataValidType," +
                " BlankDefaultWords, BlankValidType, MatrixItems, " +
                " BlankMaxLimit, BlankMinLimit, QuestionImage, QuestionVideo, MultiOptionLimit," +
                " UpdUserId, UpdDateTime ) ";

            //SurveyId, 序號遞增
            var vSql = $" VALUES(@SurveyId,@QuestionId," +
                    $"(select ISNULL(MAX(B.QuestionSeq),0)+1 from QUE002_QuestionnaireDetail B where B.SurveyId=@SurveyId)," +
                    $" @QuestionType, @QuestionSubject, " +
                    $" @SubjectStyle,@QuestionNote,@PageNo,@IsRequired,@HasOther," +
                    $"@OtherIsShowText,@OtherVerify,@OtherTextMandatory,@OtherCheckMessage,@IsSetShowNum," +
                    $"@PCRowNum, @MobileRowNum, @IsRamdomOption, @ExcludeOther,@BaseDataValidType, " +
                    $"@BlankDefaultWords,@BlankValidType, @MatrixItems,"+
                    $"@BlankMaxLimit,@BlankMinLimit,@QuestionImage, @QuestionVideo, @MultiOptionLimit," +
                    $"@UpdUserId,SYSDATETIME())";
            sSql = string.Concat(sSql, vSql);

            //-------sql para----start
            SqlParameter[] sqlParams = new SqlParameter[] {
                new SqlParameter("@SurveyId", SqlDbType.UniqueIdentifier),
                new SqlParameter("@QuestionId", SqlDbType.UniqueIdentifier),
                new SqlParameter("@QuestionType", SqlDbType.Int),
                new SqlParameter("@QuestionSubject", SqlDbType.NVarChar),
                new SqlParameter("@SubjectStyle", SqlDbType.NVarChar),

                new SqlParameter("@QuestionNote", SqlDbType.NVarChar),
                new SqlParameter("@PageNo", SqlDbType.Int),
                new SqlParameter("@IsRequired", SqlDbType.Bit),
                new SqlParameter("@HasOther", SqlDbType.Bit),
                new SqlParameter("@OtherIsShowText", SqlDbType.Bit),

                new SqlParameter("@OtherVerify", SqlDbType.Int),
                new SqlParameter("@OtherTextMandatory", SqlDbType.Bit),
                new SqlParameter("@OtherCheckMessage", SqlDbType.NVarChar),
                new SqlParameter("@IsSetShowNum", SqlDbType.Bit),
                new SqlParameter("@PCRowNum", SqlDbType.Int),

                new SqlParameter("@MobileRowNum", SqlDbType.Int),
                new SqlParameter("@IsRamdomOption", SqlDbType.Bit),
                new SqlParameter("@ExcludeOther", SqlDbType.Bit),
                new SqlParameter("@BaseDataValidType", SqlDbType.Int),
                new SqlParameter("@BlankDefaultWords", SqlDbType.NVarChar),

                new SqlParameter("@BlankValidType", SqlDbType.Int),
                new SqlParameter("@MatrixItems", SqlDbType.NVarChar),
                new SqlParameter("@BlankMaxLimit", SqlDbType.Int),
                new SqlParameter("@BlankMinLimit", SqlDbType.Int),
                new SqlParameter("@QuestionImage", SqlDbType.NVarChar),
                new SqlParameter("@QuestionVideo", SqlDbType.NVarChar),
                new SqlParameter("@MultiOptionLimit", SqlDbType.NVarChar),
                new SqlParameter("@UpdUserId", SqlDbType.UniqueIdentifier)
            };
            sqlParams[0].Value = SurveyId.ValidGuid();
            sqlParams[1].Value = QuestionId.ValidGuid();
            sqlParams[2].Value = QuestionType.ValidInt();;
            sqlParams[3].Value = QuestionSubject.Valid();;
            sqlParams[4].Value = SubjectStyle.Valid();;
            sqlParams[5].Value = QuestionNote.Valid();;
            sqlParams[6].Value = PageNo.ValidInt();;
            sqlParams[7].Value = IsRequired.ValidInt16Bit();
            sqlParams[8].Value = HasOther.ValidInt16Bit();
            sqlParams[9].Value = OtherIsShowText.ValidInt16Bit();
            sqlParams[10].Value = OtherVerify.ValidInt();;
            sqlParams[11].Value = OtherTextMandatory.ValidInt16Bit();
            sqlParams[12].Value = OtherCheckMessage.Valid();;
            sqlParams[13].Value = IsSetShowNum.ValidInt16Bit();
            sqlParams[14].Value = PCRowNum.ValidInt();;
            sqlParams[15].Value = MobileRowNum.ValidInt();;
            sqlParams[16].Value = IsRamdomOption.ValidInt16Bit();
            sqlParams[17].Value = ExcludeOther.ValidInt16Bit();
            sqlParams[18].Value = BaseDataValidType.ValidInt();
            sqlParams[19].Value = BlankDefaultWords.Valid();
            sqlParams[20].Value = BlankValidType.ValidInt();
            sqlParams[21].Value = MatrixItems.Valid();
            sqlParams[22].Value = BlankMaxLimit.ValidInt();
            sqlParams[23].Value = BlankMinLimit.ValidInt();
            sqlParams[24].Value = QuestionImage.Valid();
            sqlParams[25].Value = QuestionVideo.Valid();
            sqlParams[26].Value = MultiOptionLimit.Valid();
            sqlParams[27].Value = UpdUserId.ValidGuid();
            //-------sql para----end


                int iR = _db.ExecuteSql(sSql,sqlParams);

                replyData.code = "200";
                replyData.message = $"新增記錄完成。";
                try
                {
                    //執行成功後,需要將本筆資料帶回前端
                    //-------sql para----start
                    SqlParameter[] sqlParamsA = new SqlParameter[] {
                        new SqlParameter("@SurveyId", SqlDbType.UniqueIdentifier),
                        new SqlParameter("@QuestionId", SqlDbType.UniqueIdentifier)
                    };
                    sqlParamsA[0].Value = SurveyId.ValidGuid();
                    sqlParamsA[1].Value = QuestionId.ValidGuid();
                    //-------sql para----end
                    var result = ExecuteQuery($"SELECT * FROM QUE002_QuestionnaireDetail WHERE SurveyId=@SurveyId AND QuestionId=@QuestionId ", sqlParamsA);
                    if (result.Count == 1)
                        replyData.data = result[0];  //應該只有一筆
                    else
                        replyData.data = result;
                }
                catch (Exception ex)
                {
                    //新增成功，查詢失敗
                    replyData.data = "";
                    Log.Debug("分頁題型新增成功,查詢返回結果失敗!" + ex.Message);
                }
            }
            catch (Exception ex)
            {
                replyData.code = "-1";
                replyData.message = $"新增記錄失敗！{ex.Message}.";
                replyData.data = "";
                Log.Error("分頁題型新增失敗!" + ex.Message);
            }
            //返回
            return JsonConvert.SerializeObject(replyData);
        }
        /// <summary>
        /// 設計問卷-分頁題型--編輯
        /// </summary>
        /// <param name="value"></param>
        /// <returns></returns>
        [Route("Update")]
        [HttpPut]
        //public IActionResult Update([FromBody] Object value)
        public String Update([FromBody] Object value)
        {
            /* 輸入格式：
             * {
             *     "SurveyId": "",
             *     "QuestionId": 1,
             *     "QuestionSeq": 1,
             *     "QuestionType": 1,
             *     "PageNo": 1,
             *     "IsRequired": false,
             *     "QuestionSubject": "題目內容",
             *     "SubjectStyle": "style",
             *     "QuestionNote": "備註",
             *     "BaseDataValidType": 0
             * }
             */
            JObject jo = (JObject)JsonConvert.DeserializeObject(value.ToString());
            var replyData = new ReplyData();

            //SurveyId 必須有?
            if (jo["SurveyId"] == null || String.IsNullOrWhiteSpace(jo["SurveyId"].ToString()))
            {
                //報告錯誤
                replyData.code = "-1";
                replyData.message = $"編輯失敗！參數SurveyId不能為空！";
                replyData.data = "";
                Log.Error("分頁題型-編輯失敗!" + "參數SurveyId不能為空！");
                return JsonConvert.SerializeObject(replyData);
            }
            var SurveyId = jo["SurveyId"].ToString();
            //SurveyId 必須有?
            if (jo["QuestionId"] == null)
            {
                //報告錯誤
                replyData.code = "-1";
                replyData.message = $"編輯失敗！QuestionId不能為空！";
                replyData.data = "";
                Log.Error("分頁題型-編輯失敗!" + "QuestionId不能為空！");
                return JsonConvert.SerializeObject(replyData);
            }
            var QuestionId = jo["QuestionId"].ToString();

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

            string sWhereCondition = $" WHERE SurveyId=@SurveyId AND QuestionId=@QuestionId ";
            var obj1 = new SqlParameter("@SurveyId", SqlDbType.UniqueIdentifier);
            obj1.Value = SurveyId.ValidGuid();
            sqlParams.Add(obj1);
            var obj2 = new SqlParameter("@QuestionId", SqlDbType.UniqueIdentifier);
            obj2.Value = QuestionId.ValidGuid();
            sqlParams.Add(obj2);
            string sSql = " UPDATE QUE002_QuestionnaireDetail SET ";

            if (jo["QuestionSeq"] != null)
            {
                var QuestionSeq = Convert.ToInt32(jo["QuestionSeq"]);
                sSql += $" QuestionSeq=@QuestionSeq,";

                var obj = new SqlParameter("@QuestionSeq", SqlDbType.Int);
                obj.Value = QuestionSeq.ValidInt();
                sqlParams.Add(obj);
            }
            if (jo["QuestionType"] != null)
            {
                var QuestionType = Convert.ToInt32(jo["QuestionType"]);
                sSql += $" QuestionType=@QuestionType,";

                var obj = new SqlParameter("@QuestionType", SqlDbType.Int);
                obj.Value = QuestionType.ValidInt();
                sqlParams.Add(obj);
            }
            if (jo["PageNo"] != null)
            {
                var PageNo = Convert.ToInt32(jo["PageNo"]);
                sSql += $" PageNo=@PageNo,";

                var obj = new SqlParameter("@PageNo", SqlDbType.Int);
                obj.Value = PageNo.ValidInt();
                sqlParams.Add(obj);
            }
            if (jo["IsRequired"] != null)
            {
                var IsRequired = jo["IsRequired"].ToString();
                sSql += $" IsRequired='@IsRequired',";

                var obj = new SqlParameter("@IsRequired", SqlDbType.Bit);
                obj.Value = IsRequired.ValidBit();
                sqlParams.Add(obj);
            }
            if (jo["QuestionSubject"] != null)
            {
                var QuestionSubject = jo["QuestionSubject"].ToString();
                sSql += $" QuestionSubject=@QuestionSubject,";

                var obj = new SqlParameter("@QuestionSubject", SqlDbType.NVarChar);
                obj.Value = QuestionSubject.Valid();
                sqlParams.Add(obj);
            }
            if (jo["QuestionNote"] != null)
            {
                var QuestionNote = jo["QuestionNote"].ToString();
                sSql += $" QuestionNote=@QuestionNote,";

                var obj = new SqlParameter("@QuestionNote", SqlDbType.NVarChar);
                obj.Value = QuestionNote.Valid();
                sqlParams.Add(obj);
            }
            if (jo["BaseDataValidType"] != null)
            {
                var BaseDataValidType = Convert.ToInt32(jo["BaseDataValidType"]);
                sSql += $" BaseDataValidType=@BaseDataValidType,";

                var obj = new SqlParameter("@BaseDataValidType", SqlDbType.Int);
                obj.Value = BaseDataValidType.ValidInt();
                sqlParams.Add(obj);
            }
            //新增update 其他欄位
            if (jo["SubjectStyle"] != null)
            {
                var SubjectStyle = jo["SubjectStyle"].ToString();
                sSql += $" SubjectStyle=@SubjectStyle,";

                var obj = new SqlParameter("@SubjectStyle", SqlDbType.NVarChar);
                obj.Value = SubjectStyle.Valid();
                sqlParams.Add(obj);
            }
            if (jo["HasOther"] != null)
            {
                var HasOther = jo["HasOther"].ToString();
                sSql += $" HasOther=@HasOther,";

                var obj = new SqlParameter("@HasOther", SqlDbType.Bit);
                obj.Value = HasOther.ValidBit();
                sqlParams.Add(obj);
            }
            if (jo["OtherIsShowText"] != null)
            {
                var OtherIsShowText = jo["OtherIsShowText"].ToString();
                sSql += $" OtherIsShowText=@OtherIsShowText,";

                var obj = new SqlParameter("@OtherIsShowText", SqlDbType.Bit);
                obj.Value = OtherIsShowText.ValidBit();
                sqlParams.Add(obj);
            }
            if (jo["OtherVerify"] != null)
            {
                var OtherVerify = Convert.ToInt32(jo["OtherVerify"]);
                sSql += $" OtherVerify=@OtherVerify,";

                var obj = new SqlParameter("@OtherVerify", SqlDbType.Int);
                obj.Value = OtherVerify.ValidInt();
                sqlParams.Add(obj);
            }
            if (jo["OtherTextMandatory"] != null)
            {
                var OtherTextMandatory = jo["OtherTextMandatory"].ToString();
                sSql += $" OtherTextMandatory=@OtherTextMandatory,";

                var obj = new SqlParameter("@OtherTextMandatory", SqlDbType.Bit);
                obj.Value = OtherTextMandatory.ValidBit();
                sqlParams.Add(obj);
            }
            if (jo["OtherCheckMessage"] != null)
            {
                var OtherCheckMessage = jo["OtherCheckMessage"].ToString();
                sSql += $" OtherCheckMessage=@OtherCheckMessage,";

                var obj = new SqlParameter("@OtherCheckMessage", SqlDbType.NVarChar);
                obj.Value = OtherCheckMessage.Valid();
                sqlParams.Add(obj);
            }
            if (jo["IsSetShowNum"] != null)
            {
                var IsSetShowNum = jo["IsSetShowNum"].ToString();
                sSql += $" IsSetShowNum=@IsSetShowNum,";

                var obj = new SqlParameter("@IsSetShowNum", SqlDbType.Bit);
                obj.Value = IsSetShowNum.ValidBit();
                sqlParams.Add(obj);
            }
            if (jo["PCRowNum"] != null)
            {
                var PCRowNum = Convert.ToInt32(jo["PCRowNum"]);
                sSql += $" PCRowNum=@PCRowNum,";

                var obj = new SqlParameter("@PCRowNum", SqlDbType.Int);
                obj.Value = PCRowNum.ValidInt();
                sqlParams.Add(obj);
            }
            if (jo["MobileRowNum"] != null)
            {
                var MobileRowNum = Convert.ToInt32(jo["MobileRowNum"]);
                sSql += $" MobileRowNum=@MobileRowNum,";

                var obj = new SqlParameter("@MobileRowNum", SqlDbType.Int);
                obj.Value = MobileRowNum.ValidInt();
                sqlParams.Add(obj);
            }
            if (jo["IsRamdomOption"] != null)
            {
                var IsRamdomOption = jo["IsRamdomOption"].ToString();
                sSql += $" IsRamdomOption=@IsRamdomOption,";

                var obj = new SqlParameter("@IsRamdomOption", SqlDbType.Bit);
                obj.Value = IsRamdomOption.ValidBit();
                sqlParams.Add(obj);
            }
            if (jo["ExcludeOther"] != null)
            {
                var ExcludeOther = jo["ExcludeOther"].ToString();
                sSql += $" ExcludeOther=@ExcludeOther,";

                var obj = new SqlParameter("@ExcludeOther", SqlDbType.Bit);
                obj.Value = ExcludeOther.ValidBit();
                sqlParams.Add(obj);
            }
            if (jo["BlankDefaultWords"] != null)
            {
                var BlankDefaultWords = jo["BlankDefaultWords"].ToString();
                sSql += $" BlankDefaultWords=@BlankDefaultWords,";

                var obj = new SqlParameter("@BlankDefaultWords", SqlDbType.NVarChar);
                obj.Value = BlankDefaultWords.Valid();
                sqlParams.Add(obj);
            }
            if (jo["BlankValidType"] != null)
            {
                var BlankValidType = Convert.ToInt32(jo["BlankValidType"]);
                sSql += $" BlankValidType=@BlankValidType,";

                var obj = new SqlParameter("@BlankValidType", SqlDbType.Int);
                obj.Value = BlankValidType.ValidInt();
                sqlParams.Add(obj);
            }
            if (jo["MatrixItems"] != null)
            {
                var MatrixItems = jo["MatrixItems"].ToString();
                sSql += $" MatrixItems=@MatrixItems,";

                var obj = new SqlParameter("@MatrixItems", SqlDbType.NVarChar);
                obj.Value = MatrixItems.Valid();
                sqlParams.Add(obj);
            }
            if (jo["BlankMaxLimit"] != null)
            {
                var BlankMaxLimit = jo["BlankMaxLimit"].ToString();
                sSql += $" BlankMaxLimit=@BlankMaxLimit,";

                var obj = new SqlParameter("@BlankMaxLimit", SqlDbType.Int);
                obj.Value = BlankMaxLimit.ValidInt();
                sqlParams.Add(obj);
            }
            if (jo["BlankMinLimit"] != null)
            {
                var BlankMinLimit = jo["BlankMinLimit"].ToString();
                sSql += $" BlankMinLimit=@BlankMinLimit,";

                var obj = new SqlParameter("@BlankMinLimit", SqlDbType.Int);
                obj.Value = BlankMinLimit.ValidInt();
                sqlParams.Add(obj);
            }
            if (jo["QuestionImage"] != null)
            {
                var QuestionImage = jo["QuestionImage"].ToString();
                sSql += $" QuestionImage=@QuestionImage,";

                var obj = new SqlParameter("@QuestionImage", SqlDbType.NVarChar);
                obj.Value = QuestionImage.Valid();
                sqlParams.Add(obj);
            }
            if (jo["QuestionVideo"] != null)
            {
                var QuestionVideo = jo["QuestionVideo"].ToString();
                sSql += $" QuestionVideo=@QuestionVideo,";

                var obj = new SqlParameter("@QuestionVideo", SqlDbType.NVarChar);
                obj.Value = QuestionVideo.Valid();
                sqlParams.Add(obj);
            }
            if (jo["MultiOptionLimit"] != null)
            {
                var MultiOptionLimit = jo["MultiOptionLimit"].ToString();
                sSql += $" QuestionVideo=@MultiOptionLimit,";

                var obj = new SqlParameter("@MultiOptionLimit", SqlDbType.NVarChar);
                obj.Value = MultiOptionLimit.Valid();
                sqlParams.Add(obj);
            }

            //UpdUserId 會有程式依據Token取得，所以,目前暫時寫成00000000-0000-0000-0000-000000000000
            //var UpdUserId = "00000000-0000-0000-0000-000000000000";
            sSql += $" UpdUserId=@UpdUserId,";
            var obj3 = new SqlParameter("@UpdUserId", SqlDbType.UniqueIdentifier);
            obj3.Value = UpdUserId.ValidGuid();
            sqlParams.Add(obj3);
            //if (jo["UpdUserId"] != null)
            //{
            //    var UpdUserId = jo["UpdUserId"].ToString();
            //    sSql += $" UpdUserId=NEWID(),";
            //}
            //updatetime 為 datetime2  yyyy-MM-dd HH:mm:ss.fffffff
            //var UpdDateTime = DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss.fffffff");
            sSql += $" UpdDateTime=SYSDATETIME() ";

            sSql += sWhereCondition;

            Log.Debug("分頁題型-編輯:" + sSql);
            try
            {
                int iR = _db.ExecuteSql(sSql, sqlParams.ToArray());

                if (iR < 1)
                {
                    replyData.code = "-1";
                    replyData.message = $"無此問卷題型資料！";
                    Log.Debug($"分頁題型：問卷{SurveyId}題型{QuestionId}不存在！");
                }
                else
                {
                    replyData.code = "200";
                    replyData.message = $"編輯記錄完成。";
                }

                try
                {
                    //執行成功後,需要將本筆資料帶回前端
                    //-------sql para----start
                    SqlParameter[] sqlParamsA = new SqlParameter[] {
                        new SqlParameter("@SurveyId", SqlDbType.UniqueIdentifier),
                        new SqlParameter("@QuestionId", SqlDbType.UniqueIdentifier)
                    };
                    sqlParamsA[0].Value = SurveyId.ValidGuid();
                    sqlParamsA[1].Value = QuestionId.ValidGuid();
                    //-------sql para----end
                    var result = ExecuteQuery($"SELECT * FROM QUE002_QuestionnaireDetail WHERE SurveyId=@SurveyId AND QuestionId=@QuestionId ", sqlParamsA);
                    if (result.Count == 1)
                        replyData.data = result[0];  //應該只有一筆
                    else
                        replyData.data = result;
                }
                catch (Exception ex)
                {
                    //新增成功，查詢失敗
                    replyData.data = "";
                    Log.Debug("分頁題型-編輯成功,查詢返回結果失敗!" + ex.Message);
                }
            }
            catch (Exception ex)
            {
                replyData.code = "-1";
                replyData.message = $"編輯記錄失敗！{ex.Message}.";
                replyData.data = "";
                Log.Error("分頁題型-編輯記錄失敗!" + ex.Message);
            }
            //返回
            return JsonConvert.SerializeObject(replyData);

        }
        /// <summary>
        /// 設計問卷-分頁題型--刪除
        /// </summary>
        /// <param name="value"></param>
        /// <returns></returns>
        [Route("Delete")]
        [HttpDelete]
        //public IActionResult Delete([FromBody] Object value)
        public String Delete(String surveyId, String questionId)
        {
            //JObject jo = (JObject)JsonConvert.DeserializeObject(value.ToString());
            var replyData = new ReplyData();
            if (surveyId == null || questionId == null)
            {
                //報告錯誤
                replyData.code = "-1";
                replyData.message = $"刪除失敗！參數surveyId、questionId未傳入！";
                replyData.data = "";
                Log.Error("設計問卷-分頁--刪除失敗!" + "參數surveyId、questionId未傳入！");
                return JsonConvert.SerializeObject(replyData);
            }
            string SurveyId = surveyId;// jo["SurveyId"].ToString();
            string QuestionId = questionId;// jo["QuestionId"].ToString();

            string sSql1 = "DELETE FROM QUE002_QuestionnaireDetail WHERE SurveyId=@SurveyId AND QuestionId=@QuestionId ";
            string sSql2 = "DELETE FROM QUE003_QuestionnaireOptions WHERE QuestionId=@QuestionId ";

            var list = new List<KeyValuePair<string, SqlParameter[]>> ();

            //-------sql para----start
            SqlParameter[] sqlParams = new SqlParameter[] {
                new SqlParameter("@SurveyId", SqlDbType.UniqueIdentifier),
                new SqlParameter("@QuestionId", SqlDbType.UniqueIdentifier)
            };
            sqlParams[0].Value = SurveyId.ValidGuid();
            sqlParams[1].Value = QuestionId.ValidGuid();
            //-------sql para----end
            var obj = new KeyValuePair<string, SqlParameter[]>(sSql1, sqlParams);
            list.Add(obj);

            //-------sql para----start
            SqlParameter[] sqlParams2 = new SqlParameter[] {
                new SqlParameter("@QuestionId", SqlDbType.UniqueIdentifier)
            };
            sqlParams2[0].Value = QuestionId.ValidGuid();
            //-------sql para----end
            var obj2 = new KeyValuePair<string, SqlParameter[]>(sSql2, sqlParams2);
            list.Add(obj2);

            try
            {
                int iR = _db.ExecuteSqlTran(list);
                replyData.code = "000";
                replyData.message = $"刪除記錄完成。";
                replyData.data = iR;
                Log.Debug("分頁題型-刪除記錄完成。共刪除{iR}筆。");
                //return Ok(iR);
            }
            catch (Exception ex)
            {
                replyData.code = "-1";
                replyData.message = $"刪除記錄失敗！{ex.Message}.";
                replyData.data = "";
                Log.Error("分頁題型-刪除失敗!" + ex.Message);
            }
            //return Ok(replyData);
            return JsonConvert.SerializeObject(replyData);
        }
        /// <summary>
        /// 依據傳入sql command 執行查詢
        /// </summary>
        /// <param name="sSql"></param>
        /// <returns></returns>
        private List<QUE002_QuestionnaireDetail> ExecuteQuery(String sSql)
        {
            //改為直接返回QUE002所有欄位
            List<QUE002_QuestionnaireDetail> lstQDetail = new List<QUE002_QuestionnaireDetail>();
            try
            {
                DataTable dtR = _db.GetQueryData(sSql);
                foreach (DataRow dr in dtR.Rows)
                {
                    QUE002_QuestionnaireDetail QDetail = new QUE002_QuestionnaireDetail();
                    QDetail.SurveyId = dr["SurveyId"];
                    QDetail.QuestionId = dr["QuestionId"];
                    QDetail.QuestionSeq = dr["QuestionSeq"];
                    QDetail.QuestionType = dr["QuestionType"];
                    QDetail.QuestionSubject = dr["QuestionSubject"];
                    QDetail.SubjectStyle = dr["SubjectStyle"];
                    QDetail.QuestionNote = dr["QuestionNote"];
                    QDetail.PageNo = dr["PageNo"];
                    QDetail.IsRequired = dr["IsRequired"];
                    QDetail.HasOther = dr["HasOther"];
                    QDetail.OtherIsShowText = dr["OtherIsShowText"];
                    QDetail.OtherVerify = dr["OtherVerify"];
                    QDetail.OtherTextMandatory = dr["OtherTextMandatory"];
                    QDetail.OtherCheckMessage = dr["OtherCheckMessage"];
                    QDetail.IsSetShowNum = dr["IsSetShowNum"];
                    QDetail.PCRowNum = dr["PCRowNum"];
                    QDetail.MobileRowNum = dr["MobileRowNum"];
                    QDetail.IsRamdomOption = dr["IsRamdomOption"];
                    QDetail.ExcludeOther = dr["ExcludeOther"];
                    QDetail.BaseDataValidType = dr["BaseDataValidType"];
                    QDetail.BlankDefaultWords = dr["BlankDefaultWords"];
                    QDetail.BlankValidType = dr["BlankValidType"];
                    QDetail.MatrixItems = dr["MatrixItems"];
                    QDetail.BlankMaxLimit = dr["BlankMaxLimit"];
                    QDetail.BlankMinLimit = dr["BlankMinLimit"];
                    QDetail.QuestionImage = dr["QuestionImage"];
                    QDetail.QuestionVideo = dr["QuestionVideo"];
                    QDetail.MultiOptionLimit = dr["MultiOptionLimit"];

                    //QDetail.UpdUserId = dr["UpdUserId"];
                    //QDetail.UpdDateTime = dr["UpdDateTime"];
                    lstQDetail.Add(QDetail);
                }
            }
            catch (Exception)
            {
                throw;
            }
            return lstQDetail;
        }

        /// <summary>
        /// 依據傳入sql command 執行查詢
        /// </summary>
        /// <param name="sSql"></param>
        /// <returns></returns>
        private List<QUE002_QuestionnaireDetail> ExecuteQuery(String sSql, SqlParameter[] cmdParams)
        {
            //改為直接返回QUE002所有欄位
            List<QUE002_QuestionnaireDetail> lstQDetail = new List<QUE002_QuestionnaireDetail>();
            try
            {
                DataTable dtR = _db.GetQueryData(sSql, cmdParams);
                foreach (DataRow dr in dtR.Rows)
                {
                    QUE002_QuestionnaireDetail QDetail = new QUE002_QuestionnaireDetail();
                    QDetail.SurveyId = dr["SurveyId"];
                    QDetail.QuestionId = dr["QuestionId"];
                    QDetail.QuestionSeq = dr["QuestionSeq"];
                    QDetail.QuestionType = dr["QuestionType"];
                    QDetail.QuestionSubject = dr["QuestionSubject"];
                    QDetail.SubjectStyle = dr["SubjectStyle"];
                    QDetail.QuestionNote = dr["QuestionNote"];
                    QDetail.PageNo = dr["PageNo"];
                    QDetail.IsRequired = dr["IsRequired"];
                    QDetail.HasOther = dr["HasOther"];
                    QDetail.OtherIsShowText = dr["OtherIsShowText"];
                    QDetail.OtherVerify = dr["OtherVerify"];
                    QDetail.OtherTextMandatory = dr["OtherTextMandatory"];
                    QDetail.OtherCheckMessage = dr["OtherCheckMessage"];
                    QDetail.IsSetShowNum = dr["IsSetShowNum"];
                    QDetail.PCRowNum = dr["PCRowNum"];
                    QDetail.MobileRowNum = dr["MobileRowNum"];
                    QDetail.IsRamdomOption = dr["IsRamdomOption"];
                    QDetail.ExcludeOther = dr["ExcludeOther"];
                    QDetail.BaseDataValidType = dr["BaseDataValidType"];
                    QDetail.BlankDefaultWords = dr["BlankDefaultWords"];
                    QDetail.BlankValidType = dr["BlankValidType"];
                    QDetail.MatrixItems = dr["MatrixItems"];
                    QDetail.BlankMaxLimit = dr["BlankMaxLimit"];
                    QDetail.BlankMinLimit = dr["BlankMinLimit"];
                    QDetail.QuestionImage = dr["QuestionImage"];
                    QDetail.QuestionVideo = dr["QuestionVideo"];
                    QDetail.MultiOptionLimit = dr["MultiOptionLimit"];

                    //QDetail.UpdUserId = dr["UpdUserId"];
                    //QDetail.UpdDateTime = dr["UpdDateTime"];
                    lstQDetail.Add(QDetail);
                }
            }
            catch (Exception)
            {
                throw;
            }
            return lstQDetail;
        }
    }
    public class SurveyQuestionPageLine
    {
        //以後如果有補充欄位從Models->QUE002 copy
        public Object SurveyId { get; set; }
        public Object QuestionId { get; set; }
        public Object QuestionSeq { get; set; }
        public Object QuestionType { get; set; }
        public Object PageNo { get; set; }
        //public Object IsRequired { get; set; }
        //public Object QuestionSubject { get; set; }
        //public Object SubjectStyle { get; set; }
        //public Object QuestionNote { get; set; }
        //public Object BaseDataValidType { get; set; }

    }
}
