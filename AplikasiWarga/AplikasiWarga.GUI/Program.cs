using System;
using Gtk;

namespace AplikasiWarga.GUI
{
    class Program
    {
        static void Main(string[] args)
        {
            Application.Init();
            frmMainMenu mainMenu = new frmMainMenu();
            mainMenu.Show();
            Application.Run();
        }
    }
}
