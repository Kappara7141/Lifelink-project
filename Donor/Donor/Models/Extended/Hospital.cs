using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Web;

namespace Donor.Models
{
    [MetadataType(typeof(HospitalMetaData))]
    public partial class Hospital
    {
        public string ConfirmPassword { get; set; }
    }
    public class HospitalMetaData
    {
        [Required(AllowEmptyStrings = false, ErrorMessage = "Hospital Name Cannot Be Empty")]
        [Display(Name = "Hospital Name")]
        public string HospitalName { get; set; }
        [Required(AllowEmptyStrings =false,ErrorMessage ="Email Address Required")]
        [Display(Name ="Hospital Email")]
        [DataType(DataType.EmailAddress)]
        public string EmailID { get; set; }
        [Required(AllowEmptyStrings =false,ErrorMessage ="Mobile Number Required")]
        [Display(Name ="Hospital Mobile")]
        [DataType(DataType.PhoneNumber)]
        public string Mobile { get; set; }
        [Required(AllowEmptyStrings =false,ErrorMessage ="Location Required")]
        public string Location { get; set; }
        [Required(AllowEmptyStrings = false, ErrorMessage = "Password is required")]
        [DataType(DataType.Password)]
        [MinLength(6, ErrorMessage = "Minimum 6 characters required")]
        public string Password { get; set; }

        [Display(Name = "Confirm Password")]
        [DataType(DataType.Password)]
        [Compare("Password", ErrorMessage = "Confirm password and password do not match")]
        public string ConfirmPassword { get; set; }

    }
}