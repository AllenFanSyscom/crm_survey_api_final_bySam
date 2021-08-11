using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Configuration.Json;
using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using System.Text.RegularExpressions;
using SurveyWebAPI.Controllers;

namespace SurveyWebAPI.Utility
{
    public class DBHelper
    {
        /// <summary>
        /// ConnStr
        /// </summary>
        //private string _connectionOptions = "Data Source=LA-QING-WIN7T;Initial Catalog=FACE-CTBC;User Id=sa;pwd=lingan;";
        private readonly string _connStr;


        private IConfigurationRoot ConfigRoot;

        public DBHelper()
        {

        }
    public DBHelper(IConfiguration configRoot)
        {
            ConfigRoot = (IConfigurationRoot)configRoot;
        }

        public DBHelper(string connStr)
        {
           _connStr = connStr;
        }
        #region Query Single Column
        /// <summary>
        /// 取得單個欄位
        /// </summary>
        /// <param name="sqlStr"></param>
        /// <returns></returns>
        public string GetSingle(string sqlStr)
        {
            using (SqlConnection conn = new SqlConnection(_connStr))
            {
                using (SqlCommand cmd = new SqlCommand(sqlStr, conn))
                {
                    try
                    {
                        conn.Open();
                        return String.Format("{0}", cmd.ExecuteScalar());
                    }
                    catch (SqlException e)
                    {
                        throw e;
                    }
                    finally
                    {
                        conn.Close();
                    }
                }
            }
        }
        /// <summary>
        /// 查詢單個欄位
        /// </summary>
        /// <param name="sqlStr"></param>
        /// <param name="cmdParams"></param>
        /// <returns></returns>
        public string GetSingle(string sqlStr, SqlParameter[] cmdParams)
        {
            using (SqlConnection conn = new SqlConnection(_connStr))
            {
                using (SqlCommand cmd = new SqlCommand())
                {
                    try
                    {
                        conn.Open();
                        cmd.Connection = conn;
                        cmd.CommandType = CommandType.Text;
                        cmd.CommandText = sqlStr;
                        if (cmdParams != null && cmdParams.Length > 0)
                        {
                            foreach (SqlParameter param in cmdParams)
                            {
                                if (param.Value == null)
                                    param.Value = DBNull.Value;
                                //cmd.Parameters.Add(param);
                                cmd.Parameters.AddRange(new SqlParameter[]
                                {
                                new SqlParameter(param.ParameterName, param.Value),
                                });
                            }
                        }
                        //cmd.Parameters.AddRange(cmdParams);
                        return String.Format("{0}", cmd.ExecuteScalar());
                    }
                    catch (SqlException e)
                    {
                        throw e;
                    }
                    finally
                    {
                        conn.Close();
                    }
                }
            }
        }
        #endregion

        #region Query Set
        /// <summary>
        /// 查詢資料,返回DataSet
        /// </summary>
        /// <param name="sqlStr"></param>
        /// <returns></returns>
        public DataSet Query(string sqlStr)
        {
            using (SqlConnection conn = new SqlConnection(_connStr))
            {
                using (SqlDataAdapter ada = new SqlDataAdapter(sqlStr, conn))
                {
                    try
                    {
                        conn.Open();
                        DataSet ds = new DataSet();
                        ada.Fill(ds);
                        return ds;
                    }
                    catch (SqlException e)
                    {
                        throw e;
                    }
                    finally
                    {
                        conn.Close();
                    }
                }
            }
        }
        /// <summary>
        /// 執行查詢SQL, 返回DataSet
        /// </summary>
        /// <param name="sqlStr"></param>
        /// <param name="cmdParams"></param>
        /// <returns></returns>
        public DataSet Query(string sqlStr, SqlParameter[] cmdParams)
        {
            using (SqlConnection conn = new SqlConnection(_connStr))
            {
                using (SqlCommand cmd = new SqlCommand())
                {
                    using (SqlDataAdapter ada = new SqlDataAdapter(cmd))
                    {
                        try
                        {
                            conn.Open();
                            cmd.Connection = conn;
                            cmd.CommandType = CommandType.Text;
                            cmd.CommandText = sqlStr;
                            if (cmdParams != null && cmdParams.Length > 0)
                            {
                                foreach (SqlParameter param in cmdParams)
                                {
                                    if (param.Value == null)
                                        param.Value = DBNull.Value;
                                    //cmd.Parameters.Add(param);
                                    cmd.Parameters.AddRange(new SqlParameter[]
                                    {
                                new SqlParameter(param.ParameterName, param.Value),
                                    });
                                }
                            }
                            //cmd.Parameters.AddRange(cmdParams);

                            DataSet ds = new DataSet();
                            ada.Fill(ds);
                            return ds;
                        }
                        catch (SqlException e)
                        {
                            throw e;
                        }
                        finally
                        {
                            conn.Close();
                        }
                    }
                }
            }
        }
        /// <summary>
        /// 執行查詢StoreProcedure 返回Dataset
        /// </summary>
        /// <param name="procName"></param>
        /// <param name="cmdParams"></param>
        /// <returns></returns>
        public DataSet RunProcedure(string procName, SqlParameter[] cmdParams)
        {
            using (SqlConnection conn = new SqlConnection(_connStr))
            {
                using (SqlCommand cmd = new SqlCommand())
                {
                    using (SqlDataAdapter ada = new SqlDataAdapter(cmd))
                    {
                        try
                        {
                            conn.Open();
                            cmd.Connection = conn;
                            cmd.CommandType = CommandType.StoredProcedure;
                            cmd.CommandText = procName;
                            if (cmdParams != null && cmdParams.Length > 0)
                            {
                                foreach (SqlParameter param in cmdParams)
                                {
                                    if (param.Value == null)
                                        param.Value = DBNull.Value;
                                    //cmd.Parameters.Add(param);
                                    cmd.Parameters.AddRange(new SqlParameter[]
                                    {
                                new SqlParameter(param.ParameterName, param.Value),
                                    });
                                }
                            }
                            //cmd.Parameters.AddRange(cmdParams);

                            DataSet ds = new DataSet();
                            ada.Fill(ds);
                            return ds;
                        }
                        catch (SqlException e)
                        {
                            throw e;
                        }
                        finally
                        {
                            conn.Close();
                        }
                    }
                }
            }
        }
        #endregion

        #region Query Single Table
        /// <summary>
        /// 執行查詢SQL, 返回DataTable
        /// </summary>
        /// <param name="sqlStr"></param>
        /// <returns></returns>
        public DataTable GetQueryData(string sqlStr)
        {
            DataSet ds = Query(sqlStr);
            if (ds != null && ds.Tables.Count > 0)
                return ds.Tables[0];
            return null;
        }
        /// <summary>
        /// 執行查詢SQL, 返回DataTable
        /// </summary>
        /// <param name="sqlStr"></param>
        /// <param name="cmdParams"></param>
        /// <returns></returns>
        public DataTable GetQueryData(string sqlStr, SqlParameter[] cmdParams)
        {
            DataSet ds = Query(sqlStr, cmdParams);
            if (ds != null && ds.Tables.Count > 0)
                return ds.Tables[0];
            return null;
        }
        /// <summary>
        /// 執行查詢SP, 返回DataTable
        /// </summary>
        /// <param name="procName"></param>
        /// <param name="cmdParams"></param>
        /// <returns></returns>
        public DataTable GetProcData(string procName, SqlParameter[] cmdParams)
        {
            DataSet ds = RunProcedure(procName, cmdParams);
            if (ds != null && ds.Tables.Count > 0)
                return ds.Tables[0];
            return null;
        }
        #endregion

        #region Query Single Record
        /// <summary>
        /// 執行查詢SQL, 返回第一筆紀錄
        /// </summary>
        /// <param name="sqlStr"></param>
        /// <returns></returns>
        public DataRow GetQueryRecord(string sqlStr)
        {
            DataTable dt = GetQueryData(sqlStr);
            if (dt != null && dt.Rows.Count > 0)
                return dt.Rows[0];
            return null;
        }
        /// <summary>
        /// 執行查詢SQL, 返回第一筆紀錄
        /// </summary>
        /// <param name="sqlStr"></param>
        /// <param name="cmdParams"></param>
        /// <returns></returns>
        public DataRow GetQueryRecord(string sqlStr, SqlParameter[] cmdParams)
        {
            DataTable dt = GetQueryData(sqlStr, cmdParams);
            if (dt != null && dt.Rows.Count > 0)
                return dt.Rows[0];
            return null;
        }
        /// <summary>
        /// 執行查詢SP, 返回第一筆紀錄
        /// </summary>
        /// <param name="procName"></param>
        /// <param name="cmdParams"></param>
        /// <returns></returns>
        public DataRow GetProcRecord(string procName, SqlParameter[] cmdParams)
        {
            DataTable dt = GetProcData(procName, cmdParams);
            if (dt != null && dt.Rows.Count > 0)
                return dt.Rows[0];
            return null;
        }
        #endregion

        #region Close Reader
        public SqlDataReader ExecuteReader(string sqlStr)
        {
            SqlConnection conn = new SqlConnection(_connStr);
            SqlCommand cmd = new SqlCommand(sqlStr, conn);
            try
            {
                conn.Open();
                return cmd.ExecuteReader(CommandBehavior.CloseConnection);
            }
            catch (SqlException e)
            {
                throw e;
            }
        }
        public SqlDataReader ExecuteReeder(string sqlStr, SqlParameter[] cmdParams)
        {
            SqlConnection conn = new SqlConnection(_connStr);
            SqlCommand cmd = new SqlCommand();
            try
            {
                conn.Open();
                cmd.Connection = conn;
                cmd.CommandType = CommandType.Text;
                cmd.CommandText = sqlStr;
                if (cmdParams != null && cmdParams.Length > 0)
                {
                    foreach (SqlParameter param in cmdParams)
                    {
                        if (param.Value == null)
                            param.Value = DBNull.Value;
                        //cmd.Parameters.Add(param);
                        cmd.Parameters.AddRange(new SqlParameter[]
                        {
                        new SqlParameter(param.ParameterName, param.Value),
                        });
                    }
                }
                //cmd.Parameters.AddRange(cmdParams);
                return cmd.ExecuteReader(CommandBehavior.CloseConnection);
            }
            catch (SqlException e)
            {
                throw e;
            }
        }
        #endregion

        #region ExecuteSql
        public int ExecuteSql(string sqlStr)
        {
            using (SqlConnection conn = new SqlConnection(_connStr))
            {
                using (SqlCommand cmd = new SqlCommand(sqlStr, conn))
                {
                    try
                    {
                        conn.Open();
                        return cmd.ExecuteNonQuery();
                    }
                    catch (SqlException e)
                    {
                        throw e;
                    }
                    finally
                    {
                        conn.Close();
                    }
                }
            }
        }
        public int ExecuteSql(string sqlStr, SqlParameter[] cmdParams)
        {
            using (SqlConnection conn = new SqlConnection(_connStr))
            {
                using (SqlCommand cmd = new SqlCommand())
                {
                    try
                    {
                        conn.Open();
                        cmd.Connection = conn;
                        cmd.CommandType = CommandType.Text;
                        cmd.CommandText = sqlStr.Valid();
                        cmd.Parameters.Clear();
                        if (cmdParams != null && cmdParams.Length > 0)
                        {
                            foreach (SqlParameter param in cmdParams)
                            {
                                if (param.Value == null)
                                    param.Value = DBNull.Value;
                                //cmd.Parameters.Add(param);
                                cmd.Parameters.AddRange(new SqlParameter[]
                                {
                                new SqlParameter(param.ParameterName, param.Value),
                                });
                            }
                        }
                        //cmd.Parameters.AddRange(cmdParams);
                        return cmd.ExecuteNonQuery();
                    }
                    catch (SqlException e)
                    {
                        throw e;
                    }
                    finally
                    {
                        conn.Close();
                    }
                }
            }
        }
        #endregion

        #region ExecuteSqlTran
        /// <summary>
        /// 執行多個SQL command with a transaction
        /// </summary>
        /// <param name="SqlStrList"></param>
        /// <returns></returns>
        public int ExecuteSqlTran(List<string> SqlStrList)
        {
            using (SqlConnection conn = new SqlConnection(_connStr))
            {
                conn.Open();
                using (SqlCommand cmd = new SqlCommand())
                {
                    using (SqlTransaction tran = conn.BeginTransaction())
                    {
                        try
                        {
                            cmd.Connection = conn;
                            cmd.CommandType = CommandType.Text;
                            cmd.Transaction = tran;
                            //conn.Open();
                            int count = 0;
                            foreach (string Sql in SqlStrList)
                            {
                                if (Sql.Length == 0)
                                    continue;
                                cmd.CommandText = Sql;
                                count += cmd.ExecuteNonQuery();
                            }
                            tran.Commit();
                            return count;
                        }
                        catch (SqlException e)
                        {
                            tran.Rollback();
                            throw e;
                        }
                        finally
                        {
                            conn.Close();
                        }
                    }
                }
            }
        }
        /// <summary>
        ///  執行多個SQL command with a transaction
        /// </summary>
        /// <param name="SqlStrList"></param>
        /// <returns></returns>
        public int ExecuteSqlTran(List<KeyValuePair<string, SqlParameter[]>> SqlStrList)
        {
            using (SqlConnection conn = new SqlConnection(_connStr))
            {
                conn.Open();
                using (SqlCommand cmd = new SqlCommand())
                {
                    int count = 0;
                    foreach (var item in SqlStrList)
                    {
                        using (SqlTransaction tran = conn.BeginTransaction())
                        {
                            try
                            {
                                cmd.Connection = conn;
                                cmd.CommandType = CommandType.Text;
                                cmd.Transaction = tran;


                                cmd.CommandText = item.Key;
                                cmd.Parameters.Clear();
                                cmd.Parameters.AddRange(item.Value);
                                count += cmd.ExecuteNonQuery();


                                tran.Commit();


                            }
                            catch (SqlException e)
                            {
                                tran.Rollback();
                                throw e;
                            }
                            finally
                            {


                            }
                        }
                    }
                    conn.Close();
                    return count;
                }
            }
        }
        /// <summary>
        /// 執行Store Procedure
        /// </summary>
        /// <param name="procName"></param>
        /// <param name="cmdParams"></param>
        /// <returns></returns>
        public int ExecuteProc(string procName, SqlParameter[] cmdParams)
        {
            using (SqlConnection conn = new SqlConnection(_connStr))
            {
                using (SqlCommand cmd = new SqlCommand())
                {
                    try
                    {
                        conn.Open();
                        cmd.Connection = conn;
                        cmd.CommandType = CommandType.StoredProcedure;
                        cmd.CommandText = procName;
                        if (cmdParams != null && cmdParams.Length > 0)
                        {
                            foreach (SqlParameter param in cmdParams)
                            {
                                if (param.Value == null)
                                    param.Value = DBNull.Value;
                                //cmd.Parameters.Add(param);
                                cmd.Parameters.AddRange(new SqlParameter[]
                                {
                                new SqlParameter(param.ParameterName, param.Value),
                                });
                            }
                        }
                        //cmd.Parameters.AddRange(cmdParams);
                        return cmd.ExecuteNonQuery();
                    }
                    catch (SqlException e)
                    {
                        throw e;
                    }
                    finally
                    {
                        conn.Close();
                    }
                }
            }
        }
        #endregion
    }
}
