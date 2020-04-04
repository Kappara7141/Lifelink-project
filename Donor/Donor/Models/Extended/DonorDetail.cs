using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Web;

namespace Donor.Models
{
    [MetadataType(typeof(DonorDetailMetaData))]
    public partial class DonorDetail
    {

    }

    public class DonorDetailMetaData
    {
        [Display(Name = "Donor Name")]
        [Required(AllowEmptyStrings = false, ErrorMessage = "Donor Name Required")]
        public string DonorName { get; set; }
        [Display(Name = "Donor Address")]
        [Required(AllowEmptyStrings = false, ErrorMessage = "Address Required")]
        public string DonorAddress { get; set; }
        [Display(Name = "Email ID")]
        [Required(AllowEmptyStrings = false, ErrorMessage = "Email ID required")]
        [DataType(DataType.EmailAddress)]
        public string DonorEmail { get; set; }
        [Display(Name = "Donor Mobile")]
        [Required(AllowEmptyStrings = false, ErrorMessage = "Mobile Number Required")]
        public string DonorMobile { get; set; }
        public string DonorGender { get; set; }
        public string OrganName { get; set; }
        public bool IsHospitalAuthorized { get; set; }
    }
}