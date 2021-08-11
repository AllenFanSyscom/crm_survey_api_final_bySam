using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Configuration.Json;
using System;
using System.Collections.Generic;
using System.Dynamic;

namespace Common
{
    public class AppSettingsHelper
    {
        public static IConfiguration Configuration { get; set; }
        /// <summary>
        /// 是否需要發送OTP簡訊 true:需要，false:不需要
        /// </summary>
        public static bool NeedSendOtpMessage
        {
            get
            {
                //有四種方式，可以get到value
                var result= Configuration.GetSection("OTPSetting").GetSection("NeedSendOTPMessage").Value;
                return (result == null || result.ToString().ToLower() == "false" || result.ToString()=="0") ? false : true;
            }
        }
        /// <summary>
        /// Token碼有效時間，單位毫秒
        /// </summary>
        public static int OTPEffectPeriod
        {
            get
            {
                //有四種方式，可以get到value
                var result = Configuration.GetSection("OTPSetting").GetSection("OTPEffectPeriod").Value;
                return (result == null) ? 0 : Convert.ToInt32(result);
            }
        }
        /// <summary>
        /// 是否OTP Test，配合下面參數OTPTestValue使用：如果設定傳入測試的VertifyCode和設定的相同（目前=111111），則認為校驗通過
        /// </summary>
        public static bool OTPTest
        {
            get
            {
                //有四種方式，可以get到value
                var result = Configuration.GetSection("OTPSetting").GetSection("OTPTest").Value;
                return (result == null || result.ToString().ToLower() == "false" || result.ToString() == "0") ? false : true;
            }
        }
        /// <summary>
        /// OTP Test 配合OTPTest參數使用：如果設定傳入測試的VertifyCode和設定的相同（目前=111111），則認為校驗通過
        /// </summary>
        public static int OTPTestValue
        {
            get
            {
                //有四種方式，可以get到value
                var result = Configuration.GetSection("OTPSetting").GetSection("OTPTestValue").Value;
                return (result == null) ? 0 : Convert.ToInt32(result);
            }
        }
        /// <summary>
        /// 特殊用戶登入用戶資料
        /// </summary>
        public static object SpecificUsers
        {
            get
            {
                //有四種方式，可以get到value
                return Configuration.GetSection("OTPSetting").GetSection("SpecificUsers").Value;
                
            }
        }
        /// <summary>
        /// 特殊用戶使用的手機號碼
        /// </summary>
        public static object SpecificCellPhone
        {
            get
            {
                //有四種方式，可以get到value
                return Configuration.GetSection("OTPSetting").GetSection("SpecificCellPhone").Value;

            }
        }
        /// <summary>
        /// 當Status空白時，是否需要產生1-7之間的隨機數 true:需要回傳隨機數，false:回傳1
        /// </summary>
        public static bool NeedRandomStatus
        {
            get
            {
                //有四種方式，可以get到value
                var result = Configuration.GetSection("StatusSetting").GetSection("NeedRandomStatus").Value;
                return (result == null || result.ToString().ToLower() == "false" || result.ToString() == "0") ? false : true;
            }
        }
        //public static List<ErrorCodeMapping> errorCode
        //{
        //    get
        //    {
        //        List<ErrorCodeMapping> codelist = new List<ErrorCodeMapping>();
        //        var fields = Configuration.GetSection("ErrorCodes").GetChildren();
        //        foreach (var keyValuePair in fields)
        //        {
        //            var code = keyValuePair.Key;
        //            var msg = (keyValuePair.Value);
        //            codelist.Add(new ErrorCodeMapping() { Code = code, Message = msg });
        //        }
        //        return codelist;
        //    }
        //}
        /// <summary>
        /// 取得錯誤代碼和訊息對照列表
        /// </summary>
        public static ErrorCodeMapping[] ErrorCodes
        {
            get
            {
                //ASP.NET Core 1.1 and higher can use Get<T>, which works with entire sections. Get<T> can be more convenient than using Bind
                var errorCodes = Configuration.GetSection("ErrorCodes").Get<ErrorCodeMapping[]>();
                return errorCodes;
            }
        }
        /// <summary>
        /// 連接API 端 Database的 CHT_NewSurvey connection string
        /// </summary>
        public static String DefaultConnectionString 
        { 
            get 
            {
                //有四種方式，可以get到value
                return Configuration.GetSection("ConnectionStrings").GetSection("DefaultConnection").Value;
                //return Configuration.GetConnectionString("DefaultConnection");
                //return Configuration["ConnectionStrings:DefaultConnection"];
                //return Configuration.GetSection("ConnectionStrings")["DefaultConnection"];

            }
        }
        /// <summary>
        /// 連接 CHT_MSCRM DB的 connection string
        /// </summary>
        public static String CRMConnectionString
        {
            get
            {
                //有四種方式，可以get到value
                return Configuration.GetSection("ConnectionStrings").GetSection("MSCRMConnection").Value;
                //return Configuration.GetConnectionString("MSCRMConnection");
                //return Configuration["ConnectionStrings:MSCRMConnection"];
                //return Configuration.GetSection("ConnectionStrings")["MSCRMConnection"];

            }
        }
        /// <summary>
        /// 連接IMPORT DB 的 connection string
        /// </summary>
        public static String IMPTConnectionString
        {
            get
            {
                //有四種方式，可以get到value
                return Configuration.GetSection("ConnectionStrings").GetSection("IMPORTConnection").Value;
                //return Configuration.GetConnectionString("IMPORTConnection");
                //return Configuration["ConnectionStrings:IMPORTConnection"];
                //return Configuration.GetSection("ConnectionStrings")["IMPORTConnection"];

            }
        }

        /// <summary>
        /// 允許CORS的清單
        /// </summary>
        public static String WithOrigins
        {
            get
            {
                return Configuration.GetSection("Cors").GetSection("WithOrigins").Value;
            }
        }

        /// <summary>
        /// 專案配置檔
        /// </summary>
        static AppSettingsHelper()
        {
            Configuration = new ConfigurationBuilder()
            .Add(new JsonConfigurationSource { Path = "appsettings.json", ReloadOnChange = true })
            .Build();
        }

        /// <summary>
        /// 測試環境問卷填寫數量限制
        /// </summary>
        public static int ReplyLimit
        {
            get
            {
                //有四種方式，可以get到value
                var result = Configuration.GetSection("ReplySetting").GetSection("ReplyLimit").Value;
                return (result == null) ? 0 : Convert.ToInt32(result);
            }
        }
        public static string DownloadKey
        {
            get
            {
                //有四種方式，可以get到value
                var result = Configuration.GetSection("DownloadSetting").GetSection("DownloadKey").Value;
                return (result == null) ? "" : result.ToString();
            }
        }

        public static string DownloadIV
        {
            get
            {
                //有四種方式，可以get到value
                var result = Configuration.GetSection("DownloadSetting").GetSection("DownloadIV").Value;
                return (result == null) ? "" : result.ToString();
            }
        }

        /// <summary>
        /// 結束頁預設值
        /// </summary>
        public static EndPageDefault EndPageDefault
        {
            get
            {
                //ASP.NET Core 1.1 and higher can use Get<T>, which works with entire sections. Get<T> can be more convenient than using Bind
                var endPageDefault = Configuration.GetSection("EndPageDefault").Get<EndPageDefault>();
                return endPageDefault;
            }
        }

        /// <summary>
        /// 取得JWT的配置檔
        /// </summary>
        public static JwtSettings JwtSettings
        {
            get
            {
                //ASP.NET Core 1.1 and higher can use Get<T>, which works with entire sections. Get<T> can be more convenient than using Bind
                var jwtSettings = Configuration.GetSection("JwtSettings").Get<JwtSettings>();
                return jwtSettings;
            }
        }

        /// <summary>
        /// 取得SMS服務器的配置檔
        /// </summary>
        public static SMSInfo SMSInfo
        {
            get
            {
                //ASP.NET Core 1.1 and higher can use Get<T>, which works with entire sections. Get<T> can be more convenient than using Bind
                var smsInfo = Configuration.GetSection("SMSInfo").Get<SMSInfo>();
                return smsInfo;
            }

        }

        /// <summary>
        /// 切換CRM環境
        /// </summary>
        public static EnvSwitchToCRM EnvSwitchToCRM
        {
            get
            {
                //ASP.NET Core 1.1 and higher can use Get<T>, which works with entire sections. Get<T> can be more convenient than using Bind
                var envSwitchToCRM = Configuration.GetSection("EnvSwitchToCRM").Get<EnvSwitchToCRM>();
                return envSwitchToCRM;
            }

        }

        /// <summary>
        /// 密碼輸入錯誤超過三次，帳號鎖定時間
        /// </summary>
        public static int ErrorLockTime
        {
            get
            {
                //有四種方式，可以get到value
                var result = Configuration.GetSection("LoginRelated").GetSection("ErrorLockTime").Value;
                return (result == null) ? 0 : Convert.ToInt32(result);
            }
        }
    }
    /// <summary>
    /// 錯誤代碼訊息對照
    /// </summary>
    public class ErrorCodeMapping
    {
        /// <summary>
        /// 錯誤代碼
        /// </summary>
        public String Code { get; set; }
        /// <summary>
        /// 錯誤訊息
        /// </summary>
        public String Message { get; set; }
    }

    /// <summary>
    /// JWT 配置
    /// </summary>
    public class JwtSettings
    {
        /// <summary>
        /// 錯誤代碼
        /// </summary>
        public String Issuer { get; set; }
        /// <summary>
        /// 錯誤訊息
        /// </summary>
        public String SignKey { get; set; }
        /// <summary>
        /// Token有效時間
        /// </summary>
        public String EffectiveTime { get; set; }
    }

    /// <summary>
    /// 結束頁預設資料
    /// </summary>
    public class EndPageDefault
    {
        /// <summary>
        /// 結束頁頂圖
        /// </summary>
        public String EndPagePic { get; set; }
        /// <summary>
        /// 結束頁文字樣式
        /// </summary>
        public String EndPageStyle { get; set; }
        /// <summary>
        /// 預設按鈕文字
        /// </summary>
        public String ButtonSentence { get; set; }
        /// <summary>
        /// 轉導開關
        /// </summary>
        public bool EnableRedirect { get; set; }
        /// <summary>
        /// 轉導網址
        /// </summary>
        public String RedirectUrl { get; set; }
    }

    /// <summary>
    /// SMS服務器訊息
    /// </summary>
    public class SMSInfo
    {
        /// <summary>
        /// 特碼帳號
        /// </summary>
        public string Account { get; set; }
        /// <summary>
        /// 特碼密碼
        /// </summary>
        public string Password { get; set; }
        /// <summary>
        /// SMS服務器PORT
        /// </summary>
        public string Port { get; set; }
        /// <summary>
        /// SMS服務器IP
        /// </summary>
        public string ServerIP { get; set; }
    }

    /// <summary>
    /// 流程切換到CRM
    /// </summary>
    public class EnvSwitchToCRM
    {
        /// <summary>
        /// 是否切換到CRM
        /// </summary>
        public bool SwitchToCRM { get; set; }
        /// <summary>
        /// CRM Web API Url
        /// </summary>
        public string CRMAPIurl { get; set; }
        
    }
}
