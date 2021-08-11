using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace SurveyWebAPI.Models
{
	public class SurveyTemplate
	{
		public string TemplateId { get; set; }
		public string Subject { get; set; }
		public int TotalQuestionNum { get; set; }
	}

	public class SurveyTemplateAdd
	{
		public string TemplateId { get; set; }
		public string Title { get; set; }
	}
}
