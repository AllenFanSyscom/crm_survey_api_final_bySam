using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Threading.Tasks;
using System.Net.Http;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using SurveyWebApi.Models;
using System.Net.Http.Headers;
using Common;
using System.Drawing;

namespace SurveyWebApi.Utility
{
    public static class CRMDbApiHelper
    {
        private static string CRMDbApiUrl;
        private static HttpClient client;
        private static string queryString;
        private static string apiUrl;
        static CRMDbApiHelper()
        {
            CRMDbApiUrl = AppSettingsHelper.EnvSwitchToCRM.CRMAPIurl;
            initHttpClient();
            queryString = "";
            apiUrl = "";
        }

        private static void initHttpClient()
        {
            client = new HttpClient();
            client.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));
        }

        public static DataTable VerifyOTPController_GetCRMUserInfo(String userCode, String cellPhone)
        {
            DataTable dt = null;
            try
            {
                queryString = "?userCode=" + userCode + "&CellPhone=" + cellPhone;
                apiUrl = "/api/VerifyOTP/GetCRMUserInfoBy2";

                var result = JsonConvert.DeserializeObject<CRMUserInfo>(GetresponseBody());
                dt = JsonConvert.DeserializeObject<DataTable>(JsonConvert.SerializeObject(result.data));

            }
            catch (Exception ex)
            {
                Log.Error("VerifyOTP: VerifyOTPController_GetCRMUserInfo API fail！" + ex.Message);
                Log.Error(ex.StackTrace);
                throw ex;
            }

            return dt;
        }

        public static DataTable SystemController_GetCRMUserInfo(String UserId)
        {
            DataTable dt = null;
            try
            {
                queryString = "?UserId=" + UserId;
                apiUrl = "/api/System/GetCRMUserInfoBy";
                
                var result = JsonConvert.DeserializeObject<CRMUserInfo>(GetresponseBody());
                dt = JsonConvert.DeserializeObject<DataTable>(JsonConvert.SerializeObject(result.data));

            }
            catch (Exception ex)
            {
                Log.Error("QueryByPage: SystemController_GetCRMUserInfo API fail！" + ex.Message);
                Log.Error(ex.StackTrace);
                throw ex;
            }

            return dt;
        }

        public static DataTable SurveyAccountController_GetCRMUserInfo(String UserId)
        {
            DataTable dt = null;
            try
            {
                queryString = "?UserId=" + UserId;
                apiUrl = "/api/SurveyAccount/GetCRMUserInfoBy";

                var result = JsonConvert.DeserializeObject<CRMUserInfo>(GetresponseBody());
                dt = JsonConvert.DeserializeObject<DataTable>(JsonConvert.SerializeObject(result.data));

            }
            catch (Exception ex)
            {
                Log.Error("QueryByPage: SurveyAccountController_GetCRMUserInfo API fail！" + ex.Message);
                Log.Error(ex.StackTrace);
                throw ex;
            }

            return dt;
        }

        public static DataTable OutsideSurveyController_VerifyBy(string surveyId, int validField, string validData)
        {
            DataTable dt = null;
            try
            {
                queryString = "?validField=" + validField + "&validData=" + validData + "&surveyId=" + surveyId;
                apiUrl = "/api/OutsideSurvey/VerifyBy";

                var result = JsonConvert.DeserializeObject<CRMContactInfo>(GetresponseBody());
                dt = JsonConvert.DeserializeObject<DataTable>(JsonConvert.SerializeObject(result.data));

            }
            catch (Exception ex)
            {
                Log.Error("VerifyBy: OutsideSurveyController_GetCRMContactInfo API fail！" + ex.Message);
                Log.Error(ex.StackTrace);
                throw ex;
            }

            return dt;
        }

        public static DataTable SurveyInfoController_GetCrmStatus(string surveyId)
        {
            DataTable dt = null;
            try
            {
                queryString = "?surveyId=" + surveyId;
                apiUrl = "/api/SurveyInfo/GetCrmStatusBy";

                var result = JsonConvert.DeserializeObject<CRMStatusInfo>(GetresponseBody());
                dt = JsonConvert.DeserializeObject<DataTable>(JsonConvert.SerializeObject(result.data));

            }
            catch (Exception ex)
            {
                Log.Error("GetCrmStatusBy: SurveyInfoController_GetCrmStatus API fail！" + ex.Message);
                Log.Error(ex.StackTrace);
                throw ex;
            }

            return dt;
        }

        public static DataTable SurveyQuesionCollectionWay_ContactList(string surveyId)
        {
            DataTable dt = null;
            try
            {
                queryString = "?surveyId=" + surveyId;
                apiUrl = "/api/SurveyQuesionCollectionWay/ContactList";

                var result = JsonConvert.DeserializeObject<CRMContactList>(GetresponseBody());
                dt = JsonConvert.DeserializeObject<DataTable>(JsonConvert.SerializeObject(result.data));

            }
            catch (Exception ex)
            {
                Log.Error("ContactList: SurveyQuesionCollectionWay_ContactList API fail！" + ex.Message);
                Log.Error(ex.StackTrace);
                throw ex;
            }

            return dt;
        }

        public static DataTable OutsideSurveyController_CheckValidDateTimeFromCRM(string surveyId)
        {
            DataTable dt = null;
            try
            {
                Log.Debug("/api/OutsideSurvey/CheckValidDateTimeFromCRM");
                queryString = "?surveyId=" + surveyId;
                apiUrl = "/api/OutsideSurvey/CheckValidDateTimeFromCRM";

                var result = JsonConvert.DeserializeObject<CRMCampActEBDateTime>(GetresponseBody());
                Log.Debug(result.ToString());
                dt = JsonConvert.DeserializeObject<DataTable>(JsonConvert.SerializeObject(result.data));

            }
            catch (Exception ex)
            {
                Log.Error("CheckValidDateTimeFromCRM: OutsideSurveyController_CheckValidDateTimeFromCRM API fail！" + ex.Message);
                Log.Error(ex.StackTrace);
                throw ex;
            }

            return dt;
        }

        public static DataTable VerifyOTPController_WriteToken2DB(String userCode, string cellPhone)
        {
            DataTable dt = null;
            try
            {
                queryString = "?userCode=" + userCode + "&cellPhone=" + cellPhone;
                apiUrl = "/api/VerifyOTP/WriteToken2DB";

                var result = JsonConvert.DeserializeObject<CRMUserInfo>(GetresponseBody());
                dt = JsonConvert.DeserializeObject<DataTable>(JsonConvert.SerializeObject(result.data));

            }
            catch (Exception ex)
            {
                Log.Error("WriteToken2DB: VerifyOTPController_WriteToken2DB API fail！" + ex.Message);
                Log.Error(ex.StackTrace);
                throw ex;
            }

            return dt;
        }

        public static string GetresponseBody()
        {
            string responseBody = "";

            HttpResponseMessage response =
                   client.GetAsync(CRMDbApiUrl + apiUrl + queryString).Result;
            response.EnsureSuccessStatusCode();
            responseBody =
               response.Content.ReadAsStringAsync().Result;

            return responseBody;
        }
    }
}
