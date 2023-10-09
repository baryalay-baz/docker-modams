using Microsoft.AspNetCore.Identity;
using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MODAMS.DataAccess
{
    public interface IDBFunctions
    {
        public int GetEmployeeId();
        public DataRow[] GetRows(string sTableName, string sCriteria);
        public DataTable GetTableFromDB(string sTableName, string sCriteria);
        public DataRow GetFirstRow(string sTable, string sCriteria);
        public bool RecordExists(string sTable, string sCriteria);
        public DataTable GetTable(string sSQL);
    }
}
