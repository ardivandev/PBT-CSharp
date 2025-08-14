using System;
using System.Text;
using Gtk;

public class frmLaporan : Gtk.Window
{
    private ComboBoxText cmbWarga;
    private Button btnLaporanKeuangan;
    private Button btnStatistik;
    private TextView txtOutput;
    private DatabaseManager dbManager = new DatabaseManager();

    public frmLaporan() : base("Laporan Warga")
    {
        SetDefaultSize(600, 400);
        SetPosition(WindowPosition.Center);
        DeleteEvent += delegate { this.Destroy(); };

        VBox root = new VBox(false, 6);

        // ComboBox Warga
        Label lblPilih = new Label("Pilih NIK - Nama Warga");
        cmbWarga = new ComboBoxText();
        LoadComboBoxWarga();

        root.PackStart(lblPilih, false, false, 2);
        root.PackStart(cmbWarga, false, false, 2);

        // Tombol
        HBox boxTombol = new HBox(true, 5);
        btnLaporanKeuangan = new Button("Laporan Keuangan Saya");
        btnStatistik = new Button("Statistik Warga");

        btnLaporanKeuangan.Clicked += OnLaporanKeuangan;
        btnStatistik.Clicked += OnStatistik;

        boxTombol.PackStart(btnLaporanKeuangan, true, true, 0);
        boxTombol.PackStart(btnStatistik, true, true, 0);

        root.PackStart(boxTombol, false, false, 2);

        // Output
        txtOutput = new TextView()
        {
            Editable = false,
            WrapMode = WrapMode.Word
        };

        ScrolledWindow scroll = new ScrolledWindow();
        scroll.Add(txtOutput);

        root.PackStart(scroll, true, true, 5);

        Add(root);
        ShowAll();
    }

    private void LoadComboBoxWarga()
    {
        var wargaList = dbManager.GetWargaForComboBox();
        cmbWarga.RemoveAll();
        foreach (var warga in wargaList)
        {
            cmbWarga.AppendText($"{warga.Item1} - {warga.Item2}");
        }
        if (wargaList.Count > 0)
            cmbWarga.Active = 0;
    }

    private void OnLaporanKeuangan(object sender, EventArgs e)
    {
        string selected = cmbWarga.ActiveText;
        if (string.IsNullOrWhiteSpace(selected))
        {
            ShowMessage("Pilih warga terlebih dahulu!", MessageType.Warning);
            return;
        }

        string nik = selected.Split(' ')[0];
        int totalKas = dbManager.GetTotalKasPerWarga(nik);
        int totalIuran = dbManager.GetTotalIuranPerWarga(nik);

        StringBuilder sb = new StringBuilder();
        sb.AppendLine($"Laporan Keuangan Warga ({selected})");
        sb.AppendLine($"Total Pemasukan dari Kas Warga : Rp {totalKas:N0}");
        sb.AppendLine($"Total Iuran Rutin yang Dibayar : Rp {totalIuran:N0}");
        sb.AppendLine($"Saldo Akhir (Kas - Iuran)      : Rp {(totalKas - totalIuran):N0}");

        txtOutput.Buffer.Text = sb.ToString();
    }

    private void OnStatistik(object sender, EventArgs e)
    {
        var data = dbManager.GetStatistikWarga();

        StringBuilder sb = new StringBuilder();
        sb.AppendLine("Statistik Warga:");
        sb.AppendLine("-----------------------------");

        foreach (var item in data)
        {
            sb.AppendLine($"{item.Key} : {item.Value}");
        }

        txtOutput.Buffer.Text = sb.ToString();
    }

    private void ShowMessage(string message, MessageType type)
    {
        using (MessageDialog md = new MessageDialog(this, DialogFlags.Modal, type, ButtonsType.Ok, message))
        {
            md.Run();
            md.Destroy();
        }
    }
}
