using com.cht.messaging.sns;
using Common;
using Newtonsoft.Json;
using System;
using System.Text.RegularExpressions;

namespace SurveyWebAPI.Utility
{
    public class SMSServer
    {
        public string ServerIP { get; set; }
        public string Port { get; set; }
        public string Account { get; set; }
        public string Password { get; set; }
    }

    public class SMSResult
    {
        public string ReturnCode { get; set; }
        public string ReturnMsg { get; set; }
        public string MsgDn { get; set; }
    }


    public static class SMSSender
    {
        private static SMSServer _smsServerInfo;
        //private SMSResult _result;

        /// <summary>
        /// Send SMS Message
        /// </summary>
        /// <param name="phone"></param>
        /// <param name="message"></param>
        public static SMSResult SendSMS(string phone, string message)
        {
            _smsServerInfo = new SMSServer()
            {
                Account = AppSettingsHelper.SMSInfo.Account,
                Password = removeSpecialCharactersPath(AppSettingsHelper.SMSInfo.Password),
                Port = AppSettingsHelper.SMSInfo.Port,
                ServerIP = AppSettingsHelper.SMSInfo.ServerIP
            };

            Log.Debug("SendSMS 發送資訊:" + JsonConvert.SerializeObject(_smsServerInfo));


            Sns_Client sns_Client1 = new Sns_Client(_smsServerInfo.ServerIP, Convert.ToInt32(_smsServerInfo.Port));


            String account = _smsServerInfo.Account;
            String password = removeSpecialCharactersPath(_smsServerInfo.Password);

            try
            {
                sns_Client1.submitMessage(account, password, phone, message);


            }
            catch (Exception ex)
            {
                Log.Error("OTP發送失敗(SendSMS):" + ex.Message);
                return new SMSResult()
                {
                    ReturnCode = "104",
                    ReturnMsg = ex.Message
                };
            }
            Log.Debug("OTP發送成功(SendSMS):");
            return new SMSResult()
            {
                ReturnCode = "200",
                ReturnMsg = "發送成功"
            };
        }


        private static string removeSpecialCharactersPath(string str)
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
    }
}
