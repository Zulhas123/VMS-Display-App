using System;
using System.Collections.Generic;
using System.Configuration;
using System.Linq;
using System.Threading.Tasks;
using System.Windows.Forms;
using DisplayApp.Model;
using DisplayApp.UI;
using TableDependency.SqlClient;
using TableDependency.SqlClient.Base;
using TableDependency.SqlClient.Base.EventArgs;

namespace DisplayApp
{
    static class Program
    {
        // [STAThread]
      
        static void Main()
        {
            try
            {
                Application.EnableVisualStyles();
                Application.SetCompatibleTextRenderingDefault(false);
                Application.Run(new frmTest1());
            }
            catch (Exception e)
            {
                MessageBox.Show(e.Message);
            }


        }

    }
}
