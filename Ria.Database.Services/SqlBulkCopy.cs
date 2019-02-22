using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data.SqlClient;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using FastMember;

namespace Ria.Database.Services
{
    public class SqlBulkCopy<T> : ISqlBulkCopy<T>
    {
        public void Load(IEnumerable<T> collection, string[] columns, string tableName)
        {
            using (SqlConnection connection = new SqlConnection(ConfigurationManager.ConnectionStrings["BulkCopyConnection"].ConnectionString))
            {
                using (var bcp = new SqlBulkCopy(connection.ConnectionString, SqlBulkCopyOptions.TableLock))
                using (var reader = ObjectReader.Create(collection, columns))
                {

                    bcp.ColumnMappings.Add(columns[0], "Date");
                    bcp.ColumnMappings.Add(columns[1], "Ticker");
                    bcp.ColumnMappings.Add(columns[2], "Price");

                    bcp.BulkCopyTimeout = 120;
                    bcp.BatchSize = 0;
                    bcp.DestinationTableName = tableName;
                    bcp.WriteToServer(reader);
                }
            }
        }
    }
}
