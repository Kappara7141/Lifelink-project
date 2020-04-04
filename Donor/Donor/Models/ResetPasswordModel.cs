using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Web;

namespace Donor.Models
{
	public class ResetPasswordModel
	{
		[Required(ErrorMessage ="New Password Required",AllowEmptyStrings =false)]
		[DataType(DataType.Password)]
		public string NewPassword { get; set; }
		[DataType(DataType.Password)]
		[Compare("NewPassword",ErrorMessage ="Password and ConfirmPassword Should Match")]
		public string ConfirmPassword { get; set; }
		[Required]
		public string ResetCode { get; set; }
	}
}