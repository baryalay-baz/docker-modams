using Microsoft.AspNetCore.Mvc;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MODAMS.Models.ViewModels.Dto
{


    public class dtoRedirection
    {
        public dtoRedirection()
        {

        }
        public dtoRedirection(string area, string controller, string action)
        {
            Area = area;
            Controller = controller;
            Action = action;
        }

        public string Area { get; set; }
        public string Controller { get; set; }
        public string Action { get; set; }

    }
}
