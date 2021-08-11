using log4net;
using System;
using System.Collections.Generic;
using System.Text;
using System.Text.RegularExpressions;

namespace Common
{
    /// <summary>
    /// 利用log4net產生log
    /// </summary>
    public class Log
    {
        private static readonly ILog log = LogManager.GetLogger("SurveyWebAPILog", typeof(Common.Log));
        /// <summary>
        /// Debug訊息
        /// </summary>
        /// <param name="msg"></param>
        /// <param name="obj"></param>
        public static void Debug(string msg, object obj=null)
        {
            if(log.IsDebugEnabled && !string.IsNullOrWhiteSpace(msg))
            {
                if(obj==null)
                {
                    log.Debug(removeSpecialCharactersPath(msg));
                }
                else
                {
                    log.DebugFormat(removeSpecialCharactersPath(msg), obj);
                }
            }
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
        /// <summary>
        /// 一般訊息
        /// </summary>
        /// <param name="msg"></param>
        /// <param name="obj"></param>
        public static void Info(string msg, object obj=null)
        {
            if (log.IsInfoEnabled && !string.IsNullOrEmpty(msg))
            {
                if (obj == null)
                {
                    log.Info(removeSpecialCharactersPath(msg));
                }
                else
                {
                    log.InfoFormat(removeSpecialCharactersPath(msg), obj);
                }
            }
        }
        /// <summary>
        /// 錯誤訊息
        /// </summary>
        /// <param name="msg"></param>
        /// <param name="obj"></param>
        public static void Error(string msg, object obj = null)
        {
            if (log.IsErrorEnabled && !string.IsNullOrEmpty(msg))
            {
                if (obj == null)
                {
                    log.Error(removeSpecialCharactersPath(msg));
                }
                else
                {
                    log.ErrorFormat(removeSpecialCharactersPath(msg), obj);
                }
            }
        }
        /// <summary>
        /// 重要訊息
        /// </summary>
        /// <param name="msg"></param>
        /// <param name="obj"></param>
        public static void Fatal(string msg, object obj = null)
        {
            if (log.IsFatalEnabled && !string.IsNullOrEmpty(msg))
            {
                if (obj == null)
                {
                    log.Fatal(removeSpecialCharactersPath(msg));
                }
                else
                {
                    log.FatalFormat(removeSpecialCharactersPath(msg), obj);
                }
            }
        }
    }
}
