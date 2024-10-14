using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MODAMS.Models.ViewModels.Dto
{
    public class StoresDTO
    {
        public List<vwStore> vwStores = new List<vwStore>();
        public List<vwStoreEmployee> storeEmployees = new List<vwStoreEmployee>();
        public ReportingDTO dtoReporting = new ReportingDTO();
     
    }
}
