using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Windows.Forms;

using Unity;

using Microsoft.Practices.Unity.Configuration;

using Enivate.ResponseHub.Common;

namespace ResponseHub.GeocodingTestUtil
{
    static class Program
    {
        /// <summary>
        /// The main entry point for the application.
        /// </summary>
        [STAThread]
        static void Main()
        {

            // Unity configuration loader
            UnityConfiguration.Container = new UnityContainer().LoadConfiguration();

            Application.EnableVisualStyles();
            Application.SetCompatibleTextRenderingDefault(false);
            Application.Run(new frmMain());
        }
    }
}
