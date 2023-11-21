using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MODAMS.Models.ViewModels
{
    public class vwStoreEmployee
    {
        public int Id { get; set; }
        public string FullName { get; set; }
        public string Title { get; set; }
        public string Role { get; set; }
        public string Email { get; set; }
        public string ImageUrl { get; set; } = string.Empty;
        public int StoreId { get; set; }
    }
}
