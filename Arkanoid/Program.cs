using System;
using System.Windows.Forms;
using Arkanoid.UI;

namespace Arkanoid
{
    internal static class Program
    {
        [STAThread]
        static void Main()
        {
            Application.EnableVisualStyles();
            Application.SetCompatibleTextRenderingDefault(false);
            Application.Run(new Arkanoid.UI.Arkanoid());
        }
    }
}