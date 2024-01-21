using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MODAMS.Models.ViewModels
{
    [Keyless]
    public class vwAssets
    {
        public int CategoryId { get; set; }
        public string CategoryName { get; set; } = string.Empty;
        public int SubCategoryId { get; set; }
        public string SubCategoryName { get; set; } = string.Empty;
        public int Id { get; set; }
        public string Name { get; set; } = string.Empty;
        public string Make { get; set; } = string.Empty;
        public string Model { get; set; } = string.Empty;
        public string Year { get; set; } = string.Empty;
        public string ManufacturingCountry { get; set; } = string.Empty;
        public string SerialNo { get; set; } = string.Empty;
        public string BarCode { get; set; } = string.Empty;
        public string Engine { get; set; } = string.Empty;
        public string Chasis { get; set; } = string.Empty;
        public string Plate { get; set; } = string.Empty;
        public string Specifications { get; set; } = string.Empty;
        public decimal Cost { get; set; }
        public DateTime? PurchaseDate { get; set; }
        public string PONumber { get; set; } = string.Empty;
        public DateTime? RecieptDate { get; set; }
        public string ProcuredBy { get;set; } = string.Empty;
        public string Remarks { get; set;} = string.Empty;
        public int ConditionId { get; set; }
        public int StoreId { get; set; }
        public int AssetStatusId { get; set; }
        public int DonorId { get; set; }
        public string DonorName { get; set; } = string.Empty;
        public int DepartmentId { get; set; }
        public string DepartmentName { get; set; } = string.Empty;
        public int EmployeeId { get; set; }
        public string StoreOwner { get; set; } = string.Empty;
        public string Identification { get; set; } = string.Empty;
        public string StatusName { get; set; } = string.Empty;
        public string ConditionName { get; set; } = string.Empty;

    }
}
