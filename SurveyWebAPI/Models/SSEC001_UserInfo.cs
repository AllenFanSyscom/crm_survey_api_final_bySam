using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace SurveyWebAPI.Models
{
    public class SSEC001_UserInfo
    {
		public Object UserId { get; set; }
		public Object UserCode { get; set; }
		public Object UserName { get; set; }
		public Object UserFullName { get; set; }
		public Object DeptNo { get; set; }
		public Object Telephone { get; set; }
		public Object EMail { get; set; }
		public Object StartDate { get; set; }
		public Object StopDate { get; set; }
		public Object StyleNo { get; set; }
		public Object UsedMark { get; set; }
		public Object Remark { get; set; }
		public Object PwdErrorTime { get; set; }
		public Object UpdUserId { get; set; }
		public Object UpdDateTime { get; set; }
		public Object LoginDate { get; set; }
		public Object OTP { get; set; }
		public Object OTPTime { get; set; }
	}
}
