using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MODAMS.Models.ViewModels
{
    public class vwAsset
    {
        public int Id { get; set; }
        public int CategoryId { get; set; }
        public string CategoryName { get; set; }
        public int SubCategoryId { get; set; }
        public string SubCategoryName { get; set; }
        public string Name { get; set; }
        public string Make { get; set; }
        public string Model { get; set; }
        public string Year { get; set; }
        public string ManufacturingCountry { get; set; }
        public string SerialNo { get; set; }
        public string BarCode { get; set; }
        public string Engine { get; set; }
        public string Chasis { get; set; }
        public string Plate { get; set; }
        public string Specifications { get; set; }
        public decimal Cost { get; set; }
        public DateTime? PurchaseDate { get; set; }
        public string PONumber { get; set; }
        public DateTime? RecieptDate { get; set; }
        public string ProcuredBy { get;set; }
        public string Remarks { get; set;}
        public int ConditionId { get; set; }
        public int StoreId { get; set; }
        public int AssetStatusId { get; set; }
        public int DonorId { get; set; }
        public string DonorName { get; set; }
        public int DepartmentId { get; set; }
        public string DepartmentName { get; set; }
        public int? EmployeeId { get; set; }
        public string StoreOwner { get; set; }
        public string Identification { get; set; }
        public string StatusName { get; set; }
        public string ConditionName { get; set; }

    }
}
