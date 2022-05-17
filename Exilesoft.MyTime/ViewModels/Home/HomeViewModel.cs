using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Web;

namespace Exilesoft.MyTime.ViewModels.Home
{
    public class HomeViewModel : ViewModelBase
    {
        [DisplayName("User Name")]
        [StringLength(30, ErrorMessage = "User Name Must be less than 30 characters")]
        public string UserName { get; set; }

        [DisplayName("Password")]
        [StringLength(30, ErrorMessage = "Password Must be less than 30 characters")]
        [DataType(DataType.Password)]
        public string Password { get; set; }
    }
}