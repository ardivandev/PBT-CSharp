using System;
using Gtk;

namespace AplikasiPencatatanWarga
{
    static class Program
    {
       [STAThread]
        static void Main(string[] args)
    {
      // Inisialisasi database
      var dbManager = new DatabaseManager();

      // Inisialisasi GTK Application
      Application.Init();

      // Buat dan tampilkan window utama
      var mainWindow = new MainWindow();
      mainWindow.ShowAll();

      // Jalankan event loop GTK
      Application.Run();
    }
    }
}
