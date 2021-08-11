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
using Microsoft.AspNetCore.Authorization;
using SurveyWebAPI.Models;
using System.Data.SqlClient;

namespace SurveyWebAPI.Controllers
{
    [Authorize]
    [Route("api/Survey/EndPage")]
    [ApiController]
    public class SurveyEndPageController : ControllerBase
    {
        /// <summary>
        /// 設計問卷--單一頁面--(結束頁/額滿頁...等)
        /// </summary>
        /// <designer>Allen/Gem</designer>
        private DBHelper _db;
        public SurveyEndPageController()
        {
            _db = new DBHelper(AppSettingsHelper.DefaultConnectionString);
        }
        #region 結束頁--查詢
        /// <summary>
        /// 設計問卷-結束頁--查詢
        /// </summary>
        /// <returns></returns>
        [Route("Query")]
        [HttpGet]
        public String Query(String SurveyId)
        {
            Log.Debug("設計問卷-結束頁--查詢...");

            ReplyData replyData = new ReplyData();
            if (SurveyId==null || String.IsNullOrWhiteSpace(SurveyId))
            {
                //報告錯誤
                replyData.code = "-1";
                replyData.message = $"查詢失敗！參數SurveyId不能為空！";
                replyData.data = "";
                Log.Error("設計問卷-結束頁--查詢記錄失敗!" + "參數SurveyId不能為空！");
                return JsonConvert.SerializeObject(replyData);
            }
            string sWhere = " WHERE 1=1 ";
            if (SurveyId != null)
                sWhere += $" AND SurveyId=@SurveyId";
             string sSql = "SELECT * FROM QUE007_QuestionnaireEndPage " + sWhere;

            //-------sql para----start
            SqlParameter[] sqlParams = new SqlParameter[] {
                new SqlParameter("@SurveyId", SqlDbType.UniqueIdentifier),
            };
            sqlParams[0].Value = SurveyId.ValidGuid();
            //-------sql para----end

            List<QUE007_QuestionnaireEndPage> lstEndPage = new List<QUE007_QuestionnaireEndPage>();
            try
            {
                lstEndPage = ExecuteQuery(sSql,sqlParams);
                replyData.code = "200";
                replyData.message = $"查詢記錄完成。";
                Log.Debug($"查詢記錄完成。共{lstEndPage.Count}筆。");
                //先不要SerializeObject list 應該也可以
                if (lstEndPage.Count == 1)  //SurveyId+QuestionId 應該只有一筆
                    replyData.data = lstEndPage[0];  // JsonConvert.SerializeObject(lstBaseicSetting);
                else
                    replyData.data = lstEndPage;
            }
            catch (Exception ex)
            {
                replyData.code = "-1";
                replyData.message = $"查詢失敗！{ex.Message}.";
                replyData.data = "";
                Log.Error("設計問卷-結束頁-查詢記錄失敗!" + ex.Message);
                Log.Error("設計問卷-結束頁-查詢記錄失敗!" + ex.StackTrace);
            }

            //返回結果
            return JsonConvert.SerializeObject(replyData);
            //return lstUserInfo.ToArray();
        }
        private List<QUE007_QuestionnaireEndPage> ExecuteQuery(String sSql)
        {
            List<QUE007_QuestionnaireEndPage> lstEndPage = new List<QUE007_QuestionnaireEndPage>();
            try
            {
                DataTable dtR = _db.GetQueryData(sSql);
                foreach (DataRow dr in dtR.Rows)
                {
                    QUE007_QuestionnaireEndPage endPage = new QUE007_QuestionnaireEndPage();
                    endPage.SurveyId = dr["SurveyId"];
                    endPage.EndPagePic = dr["EndPagePic"];
                    endPage.EndPageStyle = dr["EndPageStyle"];
                    endPage.ButtonSentence = dr["ButtonSentence"];
                    endPage.EnableRedirect = dr["EnableRedirect"];
                    endPage.RedirectUrl = dr["RedirectUrl"];
                    endPage.UpdUserId = dr["UpdUserId"];
                    endPage.UpdDateTime = dr["UpdDateTime"];
                    lstEndPage.Add(endPage);
                }
            }
            catch (Exception)
            {
                throw;
            }
            return lstEndPage;
        }


         private List<QUE007_QuestionnaireEndPage> ExecuteQuery(String sSql, SqlParameter[] cmdParams)
        {
            List<QUE007_QuestionnaireEndPage> lstEndPage = new List<QUE007_QuestionnaireEndPage>();
            try
            {
                DataTable dtR = _db.GetQueryData(sSql, cmdParams);
                foreach (DataRow dr in dtR.Rows)
                {
                    QUE007_QuestionnaireEndPage endPage = new QUE007_QuestionnaireEndPage();
                    endPage.SurveyId = dr["SurveyId"];
                    endPage.EndPagePic = dr["EndPagePic"];
                    endPage.EndPageStyle = dr["EndPageStyle"];
                    endPage.ButtonSentence = dr["ButtonSentence"];
                    endPage.EnableRedirect = dr["EnableRedirect"];
                    endPage.RedirectUrl = dr["RedirectUrl"];
                    endPage.UpdUserId = dr["UpdUserId"];
                    endPage.UpdDateTime = dr["UpdDateTime"];
                    lstEndPage.Add(endPage);
                }
            }
            catch (Exception)
            {
                throw;
            }
            return lstEndPage;
        }

        #endregion

        #region 新增
        [Route("Add")]
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
            //如果JSON格式的參數，不需要區分大小寫，就如下處理，先註解掉
            //String SurveyId;
            ////Question 自動產生
            //var QuestionId = Guid.NewGuid().ToString();
            //var QuestionSeq = 0; //這個需要自動累加
            //var QuestionType = 12;
            //var QuestionSubject = "" ;
            //var SubjectStyle =  "";
            //var QuestionNote =  "";
            //var PageNo =  1 ;
            //var IsRequired =  "0" ;
            //var HasOther =  "0" ;
            //var OtherIsShowText = "0" ;
            //var OtherVerify = 0;
            //var OtherTextMandatory =  "0" ;
            //var OtherCheckMessage =  "" ;
            //var IsSetShowNum =  "0" ;
            //var PCRowNum =  0 ;
            //var MobileRowNum = 0 ;
            //var IsRamdomOption = "0" ;
            //var ExcludeOther =  "0" ;
            //var BaseDataValidType =  0 ;
            //var BlankDefaultWords = "" ;
            //var BlankValidType =  0 ;
            ////Json格式傳入的參數名要不要處理大小寫？
            //foreach (var item in jo)
            //{
            //    switch (item.Key.ToLower())
            //    {
            //        case "surveyid":
            //            SurveyId = item.Value.ToString();
            //            break;

            //    }
            //}

            //SurveyId 必須有?
            if (jo["SurveyId"] == null || String.IsNullOrWhiteSpace(jo["SurveyId"].ToString()))
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
                Log.Error("設計問卷-結束頁-新增失敗!" + "參數SurveyId不能為空！");
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

            var SurveyId = jo["SurveyId"] ;
            var EndPagePic = jo["EndPagePic"] == null ? "" : jo["EndPagePic"].ToString();
            var EndPageStyle = jo["EndPageStyle"] == null ? "" : jo["EndPageStyle"].ToString();
            var ButtonSentence = jo["ButtonSentence"] == null ? "" : jo["ButtonSentence"].ToString();
            var EnableRedirect = jo["EnableRedirect"];
            var RedirectUrl = jo["RedirectUrl"] == null ? "" : jo["RedirectUrl"].ToString();
            var MultiOptionLimit = jo["MultiOptionLimit"] == null ? "" : jo["MultiOptionLimit"].ToString();
            //UpdUserId 會有程式依據Token取得，所以,目前暫時寫成00000000-0000-0000-0000-000000000000
            //var UpdUserId = jo["UpdUserId"] == null ? "00000000-0000-0000-0000-000000000000" : jo["UpdUserId"].ToString();
            //var UpdUserId = "00000000-0000-0000-0000-000000000000";
            //UpdDateTime 為datetime2: yyyy-MM-dd HH:mm:ss.ffffffff
            //var UpdDateTime = DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss.fffffff");

            Log.Debug($"設計問卷-結束頁-新增, 問卷:{SurveyId}");
            //開始新增
            string sSql = " INSERT INTO QUE007_QuestionnaireEndPage " +
                " (SurveyId, EndPagePic, EndPageStyle, ButtonSentence, EnableRedirect, " +
                " RedirectUrl, UpdUserId, UpdDateTime ) ";

            //SurveyId, 序號遞增
            var vSql = $" VALUES(@SurveyId,@EndPagePic,@EndPageStyle,@ButtonSentence,@EnableRedirect," +
                    $"@RedirectUrl, @UpdUserId, SYSDATETIME())";
            sSql = string.Concat(sSql, vSql);

            //-------sql para----start
            SqlParameter[] sqlParams = new SqlParameter[] {
                new SqlParameter("@SurveyId", SqlDbType.UniqueIdentifier),
                new SqlParameter("@EndPagePic", SqlDbType.NVarChar),
                new SqlParameter("@EndPageStyle", SqlDbType.NVarChar),
                new SqlParameter("@ButtonSentence", SqlDbType.NVarChar),
                new SqlParameter("@EnableRedirect", SqlDbType.Bit),
                new SqlParameter("@RedirectUrl", SqlDbType.NVarChar),
                new SqlParameter("@UpdUserId", SqlDbType.UniqueIdentifier),
            };
            sqlParams[0].Value = new System.Data.SqlTypes.SqlGuid(SurveyId.ToString());
            sqlParams[1].Value = EndPagePic.Valid();
            sqlParams[2].Value = EndPageStyle.Valid();
            sqlParams[3].Value = ButtonSentence.Valid();
            sqlParams[4].Value = EnableRedirect.ValidBit();
            sqlParams[5].Value = RedirectUrl.Valid();
            sqlParams[6].Value = UpdUserId.ValidGuid();

            //-------sql para----end

            try
            {
                Log.Debug($"設計問卷 - 結束頁 - 新增,sql={sSql}");
                int iR = _db.ExecuteSql(sSql, sqlParams);

                replyData.code = "200";
                replyData.message = $"新增記錄完成。";
                try
                {
                    //執行成功後,需要將本筆資料帶回前端

                    //-------sql para----start
                    SqlParameter[] sqlSParams = new SqlParameter[] {
                        new SqlParameter("@SurveyId", SqlDbType.UniqueIdentifier),

                    };
                    sqlSParams[0].Value = new System.Data.SqlTypes.SqlGuid(SurveyId.ToString());


                    //-------sql para----end
                    var result = ExecuteQuery($"SELECT * FROM QUE007_QuestionnaireEndPage WHERE SurveyId=@SurveyId ", sqlSParams);
                    if (result.Count == 1)
                        replyData.data = result[0];  //應該只有一筆
                    else
                        replyData.data = result;
                }
                catch (Exception ex)
                {
                    //新增成功，查詢失敗
                    replyData.data = "";
                    Log.Debug("設計問卷-結束頁-新增成功,查詢返回結果失敗!" + ex.Message);
                }
            }
            catch (Exception ex)
            {
                replyData.code = "-1";
                replyData.message = $"新增記錄失敗！{ex.Message}.";
                replyData.data = "";
                Log.Error("設計問卷-結束頁-新增失敗!" + ex.Message);
            }
            //返回
            return JsonConvert.SerializeObject(replyData);
        }
        #endregion

        #region 修改
        [Route("Edit")]
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
            if (jo["SurveyId"] == null)
            {
                //報告錯誤
                replyData.code = "-1";
                replyData.message = $"編輯失敗！參數SurveyId不能為空！";
                replyData.data = "";
                Log.Error("設計問卷-結束頁-編輯失敗!" + "參數SurveyId不能為空！");
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
            var SurveyId = jo["SurveyId"].ToString();
            Log.Debug($"設計問卷-結束頁-編輯問卷:{SurveyId}");

            string sWhereCondition = $" WHERE SurveyId=@SurveyId ";
            string sSql = " UPDATE QUE007_QuestionnaireEndPage SET ";
            string sUpdate = "";

            //-------sql para----start
            List<SqlParameter> sqlParams = new List<SqlParameter>();
            //-------sql para----end

            var obj1 = new SqlParameter("@SurveyId", SqlDbType.UniqueIdentifier);
            obj1.Value = SurveyId.ValidGuid();
            sqlParams.Add(obj1);

            if (jo["EndPagePic"] != null)
            {
                var EndPagePic = jo["EndPagePic"].ToString();
                sUpdate += $" EndPagePic=@EndPagePic,";

                var obj = new SqlParameter("@EndPagePic", SqlDbType.NVarChar);
                obj.Value = EndPagePic.Valid();
                sqlParams.Add(obj);

            }
            if (jo["EndPageStyle"] != null)
            {
                var EndPageStyle = jo["EndPageStyle"].ToString();
                sUpdate += $" EndPageStyle=@EndPageStyle,";

                var obj = new SqlParameter("@EndPageStyle", SqlDbType.NVarChar);
                obj.Value = EndPageStyle.Valid();
                sqlParams.Add(obj);

            }
            if (jo["ButtonSentence"] != null)
            {
                var ButtonSentence = jo["ButtonSentence"].ToString();
                sUpdate += $" ButtonSentence=@ButtonSentence,";

                var obj = new SqlParameter("@ButtonSentence", SqlDbType.NVarChar);
                obj.Value = ButtonSentence.Valid();
                sqlParams.Add(obj);
            }
            if (jo["EnableRedirect"] != null)
            {
                var EnableRedirect = jo["EnableRedirect"].ToString();
                sUpdate += $" EnableRedirect=@EnableRedirect,";

                var obj = new SqlParameter("@EnableRedirect", SqlDbType.Bit);
                obj.Value = EnableRedirect.ValidBit();
                sqlParams.Add(obj);

            }
            if (jo["RedirectUrl"] != null)
            {
                var RedirectUrl = jo["RedirectUrl"].ToString();
                sUpdate += $" RedirectUrl=@RedirectUrl, ";

                var obj = new SqlParameter("@RedirectUrl", SqlDbType.NVarChar);
                obj.Value = RedirectUrl.Valid();
                sqlParams.Add(obj);

            }
            //if(sUpdate.Trim().Length==0)
            //{
            //    //報告錯誤
            //    replyData.code = "-1";
            //    replyData.message = $"編輯失敗！請傳入需要更新的欄位！";
            //    replyData.data = "";
            //    Log.Error("設計問卷-結束頁-編輯失敗!" + "請傳入需要更新的欄位！");
            //    return JsonConvert.SerializeObject(replyData);
            //}
            //因為此Table沒有updatetime 和updUserId,所以，需要去掉最後一個逗號
            //sUpdate = sUpdate.Remove(sUpdate.LastIndexOf(','));

            //UpdUserId 會有程式依據Token取得，所以,目前暫時寫成00000000 - 0000 - 0000 - 0000 - 000000000000
            //var UpdUserId = "00000000-0000-0000-0000-000000000000";
            sUpdate += $" UpdUserId=@UpdUserId,";

            var Sobj = new SqlParameter("@UpdUserId", SqlDbType.UniqueIdentifier);
            Sobj.Value = UpdUserId.ValidGuid();
            sqlParams.Add(Sobj);

            //if (jo["UpdUserId"] != null)
            //{
            //    var UpdUserId = jo["UpdUserId"].ToString();
            //    sSql += $" UpdUserId=NEWID(),";
            //}
            //updatetime 為 datetime2  yyyy-MM-dd HH:mm:ss.fffffff
            var UpdDateTime = DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss.fffffff");
            sUpdate += $" UpdDateTime=SYSDATETIME() ";

            sSql += sUpdate + sWhereCondition;

            Log.Debug("設計問卷-結束頁-編輯:" + sSql);
            try
            {
                int iR = _db.ExecuteSql(sSql,sqlParams.ToArray());

                if (iR < 1)
                {
                    ErrorCode.Code = "206";
                    replyData.code = ErrorCode.Code;
                    replyData.message = ErrorCode.Message;
                    //replyData.code = "-1";
                    //replyData.message = $"無此問卷資料！";
                    Log.Debug($"設計問卷-結束頁-編輯：無問卷{SurveyId}的結束頁記錄！");
                }
                else
                {
                    replyData.code = "200";
                    replyData.message = $"編輯記錄完成。";
                }

                try
                {
                    SqlParameter[] sqlSParams = new SqlParameter[] {
                        new SqlParameter("@SurveyId", SqlDbType.UniqueIdentifier),

                    };
                    sqlSParams[0].Value = new System.Data.SqlTypes.SqlGuid(SurveyId.ToString());
                    //執行成功後,需要將本筆資料帶回前端
                    var result = ExecuteQuery($"SELECT * FROM QUE007_QuestionnaireEndPage WHERE SurveyId=@SurveyId", sqlSParams);
                    if (result.Count == 1)
                        replyData.data = result[0];  //應該只有一筆
                    else
                        replyData.data = result;
                }
                catch (Exception ex)
                {
                    //新增成功，查詢失敗
                    replyData.data = "";
                    Log.Debug("設計問卷-結束頁-編輯成功,查詢返回結果失敗!" + ex.Message);
                }
            }
            catch (Exception ex)
            {
                replyData.code = "-1";
                replyData.message = $"編輯記錄失敗！{ex.Message}.";
                replyData.data = "";
                Log.Error("設計問卷-結束頁-編輯記錄失敗!" + ex.Message);
            }
            //返回
            return JsonConvert.SerializeObject(replyData);

        }
        #endregion

        #region 刪除
        /// <summary>
        /// 設計問卷-結束頁-刪除
        /// </summary>
        /// <param name="value"></param>
        /// <returns></returns>
        [Route("Delete")]
        [HttpDelete]
        //public IActionResult Delete([FromBody] Object value)
        public String Delete(String surveyId)
        {
            Log.Debug($"設計問卷-結束頁-刪除問卷:{surveyId}");
            //JObject jo = (JObject)JsonConvert.DeserializeObject(value.ToString());
            var replyData = new ReplyData();
            if (surveyId == null || String.IsNullOrWhiteSpace(surveyId))
            {
                //報告錯誤
                replyData.code = "-1";
                replyData.message = $"刪除失敗！參數surveyId！";
                replyData.data = "";
                Log.Error("設計問卷-結束頁-刪除失敗!" + "參數surveyId！");
                return JsonConvert.SerializeObject(replyData);
            }
            string SurveyId = surveyId;// jo["SurveyId"].ToString();

            string sSql = string.Format("DELETE FROM QUE007_QuestionnaireEndPage WHERE SurveyId=@SurveyId");

            //-------sql para----start
            SqlParameter[] sqlSParams = new SqlParameter[] {
                        new SqlParameter("@SurveyId", SqlDbType.UniqueIdentifier),

                    };
            sqlSParams[0].Value = SurveyId.ValidGuid();


            //-------sql para----end

            try
            {
                int iR = _db.ExecuteSql(sSql, sqlSParams) ;
                replyData.code = "000";
                replyData.message = $"刪除記錄完成。";
                replyData.data = iR;
                Log.Debug("設計問卷-結束頁-刪除記錄完成。共刪除{iR}筆。");
                //return Ok(iR);
            }
            catch (Exception ex)
            {
                replyData.code = "-1";
                replyData.message = $"刪除記錄失敗！{ex.Message}.";
                replyData.data = "";
                Log.Error("設計問卷-結束頁-刪除失敗!" + ex.Message);
            }
            //return Ok(replyData);
            return JsonConvert.SerializeObject(replyData);
        }
        #endregion
    }
}
