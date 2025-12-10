using System;
using System.Windows.Forms;

namespace GraphEditorApp
{
    static class Program
    {
        [STAThread]
        static void Main()
        {
            ApplicationConfiguration.Initialize(); // .NET 6/7
            Application.Run(new Forms.MainForm());
        }
    }
}
