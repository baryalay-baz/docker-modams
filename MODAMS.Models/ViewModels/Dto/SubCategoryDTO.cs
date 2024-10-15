using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MODAMS.Models.ViewModels.Dto
{
    public class SubCategoryDTO
    {
        public int Id { get; set; }
        public string SubCategoryCode { get; set; }
        public string SubCategoryName { get; set;}
        public int LifeSpan { get; set; }
        public int CategoryId { get; set; }
    }
}
