using System.Collections.Generic;
using System.Data.SqlClient;
using System.Text;

namespace GameForestCore.Common
{
    public static class GFXDatabaseHelper
    {
        public static void Insert(SqlConnection connection, string table, IEnumerable<object> columns, IEnumerable<object> values, string condition = null)
        {
            var builder = new StringBuilder(200);

            foreach (var column in columns)
            {
                builder.Append(column).Append(", ");
            }

            var columnInfo = builder.ToString();

            foreach (var value in values)
            {
                if (value is string)
                {
                    builder.Append("'").Append(value).Append("'").Append(", ");
                }
                else
                {
                    builder.Append(value).Append(", ");
                }
            }

            var valuesInfo = builder.ToString();

            if (string.IsNullOrEmpty(condition) == false)
            {
                var query = string.Format("INSERT INTO {0} ({1}) VALUES ({2})", table, columnInfo, valuesInfo);
                new SqlCommand(query, connection).ExecuteNonQuery();
            }
            else
            {
                var query = string.Format("INSERT INTO {0} ({1}) VALUES ({2}) WHERE {3}", table, columnInfo, valuesInfo, condition);
                new SqlCommand(query, connection).ExecuteNonQuery();
            }
        }
    }
}
