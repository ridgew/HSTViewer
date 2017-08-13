using System;
using System.Windows.Forms;

namespace HSTViewer
{
    static class Program
    {
        /// <summary>
        /// 应用程序的主入口点。
        /// </summary>
        [STAThread]
        static void Main(string[] args)
        {
            //System.Diagnostics.Debugger.Launch();
            Application.EnableVisualStyles();
            Application.SetCompatibleTextRenderingDefault(false);
            HstForm viewForm = new HstForm();
            if (args != null && args.Length > 0)
            {
                if (System.IO.File.Exists(args[0]))
                    viewForm.InitialFilePath = args[0];
            }
            Application.Run(viewForm);
        }
    }
}
