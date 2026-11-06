using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data;
using System.Data.SqlClient;
using StrayCat.Model;

namespace StrayCat.DAL
{
    public class DbHelper
    {
        private readonly string _connectionString;

        public DbHelper()
            : this(null)
        {
        }

        public DbHelper(string connectionString)
        {
            if (string.IsNullOrWhiteSpace(connectionString))
            {
                ConnectionStringSettings settings = ConfigurationManager.ConnectionStrings["StrayCatDb"];
                if (settings == null || string.IsNullOrWhiteSpace(settings.ConnectionString))
                {
                    throw new InvalidOperationException("缺少连接字符串 StrayCatDb。");
                }
                connectionString = settings.ConnectionString;
            }
            _connectionString = connectionString;
        }

        public List<T> Query<T>(string procedure, Func<SqlDataReader, T> map, params SqlParameter[] parameters)
        {
            List<T> list = new List<T>();
            using (SqlConnection connection = Open())
            using (SqlCommand command = Create(connection, procedure, parameters))
            using (SqlDataReader reader = command.ExecuteReader())
            {
                while (reader.Read())
                {
                    list.Add(map(reader));
                }
            }
            return list;
        }

        public int ExecuteScalarInt(string procedure, params SqlParameter[] parameters)
        {
            using (SqlConnection connection = Open())
            using (SqlCommand command = Create(connection, procedure, parameters))
            {
                object value = command.ExecuteScalar();
                if (value == null || value == DBNull.Value)
                {
                    return 0;
                }
                return Convert.ToInt32(value);
            }
        }

        public OperationResult<int> ExecuteReturnId(string procedure, string outputName, params SqlParameter[] parameters)
        {
            SqlParameter output = new SqlParameter(outputName, SqlDbType.Int);
            output.Direction = ParameterDirection.Output;
            OperationResult<int> result = Execute(procedure, Combine(parameters, output));
            if (!result.Success)
            {
                return result;
            }
            int id = output.Value == null || output.Value == DBNull.Value ? 0 : Convert.ToInt32(output.Value);
            return OperationResult<int>.Ok(id);
        }

        public PagedResult<T> QueryPage<T>(string procedure, Func<SqlDataReader, T> map, params SqlParameter[] parameters)
        {
            SqlParameter total = new SqlParameter("@TotalCount", SqlDbType.Int);
            total.Direction = ParameterDirection.Output;
            PagedResult<T> page = new PagedResult<T>();
            using (SqlConnection connection = Open())
            using (SqlCommand command = Create(connection, procedure, Combine(parameters, total)))
            {
                using (SqlDataReader reader = command.ExecuteReader())
                {
                    while (reader.Read())
                    {
                        page.Items.Add(map(reader));
                    }
                }
                page.TotalCount = total.Value == null || total.Value == DBNull.Value ? 0 : Convert.ToInt32(total.Value);
            }
            return page;
        }

        public OperationResult<int> Execute(string procedure, params SqlParameter[] parameters)
        {
            try
            {
                using (SqlConnection connection = Open())
                using (SqlCommand command = Create(connection, procedure, parameters))
                {
                    int count = command.ExecuteNonQuery();
                    return OperationResult<int>.Ok(count);
                }
            }
            catch (SqlException ex)
            {
                string message = SqlErrorMapper.ToMessage(ex.Number, ex.Message);
                if (message == null)
                {
                    throw;
                }
                return OperationResult<int>.Fail(message);
            }
        }

        private SqlConnection Open()
        {
            SqlConnection connection = new SqlConnection(_connectionString);
            connection.Open();
            return connection;
        }

        private static SqlCommand Create(SqlConnection connection, string procedure, SqlParameter[] parameters)
        {
            SqlCommand command = new SqlCommand(procedure, connection);
            command.CommandType = CommandType.StoredProcedure;
            if (parameters != null)
            {
                for (int i = 0; i < parameters.Length; i++)
                {
                    if (parameters[i] != null)
                    {
                        command.Parameters.Add(parameters[i]);
                    }
                }
            }
            return command;
        }

        private static SqlParameter[] Combine(SqlParameter[] parameters, SqlParameter extra)
        {
            int length = parameters == null ? 0 : parameters.Length;
            SqlParameter[] all = new SqlParameter[length + 1];
            for (int i = 0; i < length; i++)
            {
                all[i] = parameters[i];
            }
            all[length] = extra;
            return all;
        }
    }
}
