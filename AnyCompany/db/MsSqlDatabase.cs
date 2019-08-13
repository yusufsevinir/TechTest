using System;
using System.Collections.Generic;
using System.Data.SqlClient;

namespace AnyCompany.db
{
    class MsSqlDatabase : IDatabase
    {
        private string _connectionString;

        public MsSqlDatabase(string connectionString)
        {
            _connectionString = connectionString;
        }

        public void ExecuteNonQuery(string command, Dictionary<string, object> parameters)
        {
            using (SqlConnection connection = new SqlConnection(_connectionString))
            {
                connection.Open();

                SqlCommand sqlCommand = new SqlCommand(command, connection);
                AddParametersToCommand(sqlCommand, parameters);

                sqlCommand.ExecuteNonQuery();
            }
        }

        public IEnumerable<Dictionary<string, object>> ExtractData(SqlDataReader reader)
        {
            var results = new List<Dictionary<string, object>>();
            var cols = new List<string>();
            for (var i = 0; i < reader.FieldCount; i++)
                cols.Add(reader.GetName(i));

            while (reader.Read())
                results.Add(ExtractRow(cols, reader));

            return results;
        }

        private Dictionary<string, object> ExtractRow(IEnumerable<string> cols,
                                                SqlDataReader reader)
        {
            var result = new Dictionary<string, object>();
            foreach (var col in cols)
                result.Add(col, reader[col]);
            return result;
        }

        private void AddParametersToCommand(SqlCommand sqlCommand, Dictionary<string, object> parameters)
        {
            if (parameters == null)
                return;

            foreach(KeyValuePair<string, object> parameterSet in parameters)
            {
                sqlCommand.Parameters.AddWithValue(parameterSet.Key, parameterSet.Value);
            }
        }

        public IEnumerable<Dictionary<string, object>> ExecuteReader(string command, Dictionary<string, object> parameters)
        {
            using (SqlConnection connection = new SqlConnection(_connectionString))
            {
                connection.Open();

                SqlCommand sqlCommand = new SqlCommand(command, connection);
                AddParametersToCommand(sqlCommand, parameters);

                using (SqlDataReader reader = sqlCommand.ExecuteReader())
                {

                    if (reader.HasRows)
                    {
                        return ExtractData(reader);
                    }
                }
            }

            return new List<Dictionary<string, object>>(); //empty list pattern 
        }

    }
}
