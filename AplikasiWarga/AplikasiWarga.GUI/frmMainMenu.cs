using System;
using Gtk;

public class frmMainMenu : Window
{
    public frmMainMenu() : base("Menu Utama Aplikasi")
    {
        SetDefaultSize(400, 400);
        SetPosition(WindowPosition.Center);

        VBox vbox = new VBox(false, 10)
        {
            BorderWidth = 20
        };

        Button btnDataWarga = new Button("Data Warga");
        btnDataWarga.Clicked += (sender, e) =>
        {
            new frmDataWarga().ShowAll();
        };

        Button btnKegiatanRutin = new Button("Kegiatan Rutin Warga");
        btnKegiatanRutin.Clicked += (sender, e) =>
        {
            new frmKegiatanRutin().ShowAll();
        };

        Button btnIuranRutin = new Button("Iuran Rutin Warga");
        btnIuranRutin.Clicked += (sender, e) =>
        {
            new frmIuranRutin().ShowAll();
        };

        Button btnKasWarga = new Button("Pemasukan Kas Warga");
        btnKasWarga.Clicked += (sender, e) =>
        {
            new frmKasWarga().ShowAll();
        };

         Button btnLaporan = new Button("Laporan Warga");
        btnLaporan.Clicked += (sender, e) =>
        {
            new frmLaporan().ShowAll();
        };

        vbox.PackStart(btnDataWarga, false, false, 5);
        vbox.PackStart(btnKegiatanRutin, false, false, 5);
        vbox.PackStart(btnIuranRutin, false, false, 5);
        vbox.PackStart(btnKasWarga, false, false, 5);
        vbox.PackStart(btnLaporan, false, false, 5);

        Add(vbox);
        DeleteEvent += delegate { Application.Quit(); };
        ShowAll();
    }
}
