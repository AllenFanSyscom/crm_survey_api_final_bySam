using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Common;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Configuration.Json;

namespace SurveyWebAPI.Utility
{
    /// <summary>
    /// 錯誤代碼和訊息對照
    /// </summary>
    public static class ErrorCode
    {
        private static List<ErrorCodeMapping> errorCodes;
        private static string _code;
        private static string _message;
        static ErrorCode()
        {
            errorCodes= AppSettingsHelper.ErrorCodes.ToList<ErrorCodeMapping>();
        }
        public static String Code
        {
            get { return _code; }
            set { _code = value; }
        }
        public static String Message
        {
            get
            {
                _message = GetErrorMessageBy(_code);
                return _message;
            }
        }
        /// <summary>
        /// 依據錯誤代碼取得錯誤訊息
        /// </summary>
        /// <param name="errorCode"></param>
        /// <returns></returns>
        private static string GetErrorMessageBy(string errorCode)
        {
            if (String.IsNullOrWhiteSpace(errorCode))
                return "";
            var error = errorCodes.Where(c => c.Code.Equals(errorCode)).FirstOrDefault();
            return error == null ? "" : error.Message;
        }
    }
    //public class ErrorCodeMapping
    //{
    //    public Object Code { get; set; }
    //    public Object Message { get; set; }
    //}

}
