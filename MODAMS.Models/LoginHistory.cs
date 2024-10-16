﻿using Microsoft.AspNetCore.Mvc.ModelBinding.Validation;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MODAMS.Models
{
    public class LoginHistory
    {
        [Key]
        public int Id { get; set; }
        public int EmployeeId { get; set; }
        public DateTime TimeStamp { get; set; }
        public string IPAddress { get; set; }

        [ValidateNever]
        public Employee Employee { get; set; }
    }
}
