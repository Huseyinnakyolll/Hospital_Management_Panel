using System;
using System.Collections.Generic;
using System.Data.SqlClient;
using System.Data;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Hastane_Yönetim_Paneli.Classes
{
    public class SqlOperations
    {

        public static SqlConnection connection = new SqlConnection("Data Source=HšSEYININ\\SQLEXPRESS;Initial Catalog=Hastana_Yönetimi;Integrated Security=True;");

        public static void CheckConnection(SqlConnection tempConnection)
        {
            if (tempConnection.State == ConnectionState.Closed)
            {
                tempConnection.Open();
            }
            else
            {

            }
        }
    }
}
