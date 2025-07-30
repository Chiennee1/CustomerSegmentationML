using System;
using System.Windows.Forms;
using CustomerSegmentationML.Forms;

namespace CustomerSegmentationML
{
    internal static class Program
    {
        [STAThread]
        static void Main()
        {
            Application.EnableVisualStyles();
            Application.SetCompatibleTextRenderingDefault(false);

            // Initialize data if needed
            InitializeApplication();

            Application.Run(new MainForm());
        }

        private static void InitializeApplication()
        {
            // Create Data directory if not exists
            if (!System.IO.Directory.Exists("Data"))
                System.IO.Directory.CreateDirectory("Data");

            // Create Results directory if not exists
            if (!System.IO.Directory.Exists("Results"))
                System.IO.Directory.CreateDirectory("Results");
        }
    }
}