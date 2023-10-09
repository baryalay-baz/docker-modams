using System;
using System.Web;
using System.Data;
using System.Data.SqlClient;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using MODAMS.DataAccess;
//using MODAMS.DataAccess;

public static class DbFunctions
{

    public static SqlConnection objConn = new SqlConnection();
    static DataSet objDS = new DataSet();


    public static int EmployeeId(string userIdClaim) {
        DataRow oRow = GetFirstRow("AspNetUsers","Id='" +  userIdClaim + "'");
        int employeeId = 0;
        if (oRow != null) {
            employeeId =  (int)oRow["EmployeeId"];
        }

        return employeeId;
    }



    public static void CloseConn()
    {
        if (objConn.State != ConnectionState.Closed)
        {
            objConn.Close();
        }
    }
    public static void OpenConn()
    {
        objConn.ConnectionString = "Server=.;Database=MODAMS;Trusted_Connection=True;TrustServerCertificate=True;";
        if (objConn.State != ConnectionState.Open)
        {
            objConn.Open();
        }
    }

    private static void LoadTable(string sTableName)
    {
        SqlCommand objCMD = new SqlCommand("Select * from " + sTableName, objConn);
        SqlDataAdapter objADP = new SqlDataAdapter(objCMD);

        try
        {
            OpenConn();
            objADP.Fill(objDS, sTableName);
            CloseConn();
        }
        catch (Exception ex)
        {
            objCMD.Dispose();
            objADP.Dispose();
            CloseConn();
            throw (ex);
        }
        finally
        {
            objCMD.Dispose();
            objADP.Dispose();
            CloseConn();
        }
    }
    public static DataRow[] GetRows(string sTableName, string sCriteria)
    {
        LoadTable(sTableName);
        DataRow[] oRows = null;
        if (sCriteria == "-")
        {

            oRows = objDS.Tables[sTableName].Select();
        }
        else
        {
            oRows = objDS.Tables[sTableName].Select(sCriteria);
        }
        return oRows;
    }
    public static DataTable GetTableFromDB(string sTableName, string sCriteria)
    {
        string sSQL = "";

        if (sCriteria == "-")
        {
            sSQL = "Select * from " + sTableName;
        }
        else
        {
            sSQL = "Select * from " + sTableName + " Where " + sCriteria;
        }

        SqlCommand objCMD = new SqlCommand(sSQL, objConn);
        SqlDataAdapter objADP = new SqlDataAdapter(objCMD);
        DataTable tblData = default(DataTable);

        try
        {
            OpenConn();
            objADP.Fill(objDS, sTableName);

            tblData = objDS.Tables["DataTable"];
            CloseConn();
        }
        catch (Exception ex)
        {
            objCMD.Dispose();
            objADP.Dispose();
            objDS.Dispose();
            CloseConn();
            throw (ex);
        }
        finally
        {
            objCMD.Dispose();
            objADP.Dispose();
            objDS.Dispose();
            CloseConn();
        }

        return tblData;

    }

    public static DataRow GetFirstRow(string sTable, string sCriteria)
    {
        DataTable tblData = GetTable("Select * from " + sTable + " Where " + sCriteria);
        DataRow oRow = tblData.Rows[0];
        tblData.Dispose();

        return oRow;
    }
    public static bool RecordExists(string sTable, string sCriteria)
    {
        bool blnCheck = false;
        DataRow[] oRows = GetRows(sTable, sCriteria);
        if (oRows.Length > 0)
        {
            blnCheck = true;
        }
        else
        {
            blnCheck = false;
        }
        oRows = null;
        return blnCheck;
    }

    public static DataTable GetTable(string sSQL)
    {
        SqlCommand objCMD = new SqlCommand(sSQL, objConn);

        SqlDataAdapter objADP = new SqlDataAdapter(objCMD);
        DataSet objDS = new DataSet();

        DataTable tblData = default(DataTable);

        try
        {
            OpenConn();
            objADP.Fill(objDS, "DataTable");

            tblData = objDS.Tables["DataTable"];
            CloseConn();
        }
        catch (Exception ex)
        {
            objCMD.Dispose();
            objADP.Dispose();
            objDS.Dispose();
            CloseConn();
            throw (ex);
        }

        try
        {
        }
        finally
        {
            CloseConn();
        }

        objCMD.Dispose();
        objADP.Dispose();
        objDS.Dispose();
        CloseConn();

        return tblData;
    }
}