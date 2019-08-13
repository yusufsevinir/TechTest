using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AnyCompany.db
{
    public interface IDatabase
    {
        IEnumerable<Dictionary<string, object>> ExecuteReader(string command, Dictionary<string, object> parameters);
        void ExecuteNonQuery(string command, Dictionary<string, object> parameters); 
    }
}
