using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Ria.Database.Services
{
    public interface ISqlBulkCopy<T>
    {
        void Load(IEnumerable<T> collection, string[] columns, string tableName, bool truncateFirst=false);
    }
}
