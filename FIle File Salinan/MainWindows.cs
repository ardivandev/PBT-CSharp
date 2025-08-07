using Gtk;
using System;
using System.Collections.Generic;
using System.Data;

public class MainWindow : Window
{
    Entry txtNIK, txtNamaLengkap, txtAlamat, txtPekerjaan;
    Calendar calendarTanggalLahir;
    ComboBoxText cmbJenisKelamin, cmbStatusPerkawinan;
    Button btnSimpan, btnReset, btnHapus;
    TreeView treeWarga;
    ListStore store;

    DatabaseManager dbManager;
    string selectedNIK = "";

    public MainWindow() : base("Pencatatan Data Warga")
    {
        SetDefaultSize(800, 600);
        SetPosition(WindowPosition.Center);
        DeleteEvent += delegate { Application.Quit(); };

        dbManager = new DatabaseManager();

        VBox root = new VBox(false, 8);

        // Group Box untuk Form Input
        Frame frame = new Frame("Data Warga");
        VBox form = new VBox(false, 4);

        txtNIK = CreateEntry("NIK");
        txtNamaLengkap = CreateEntry("Nama Lengkap");
        calendarTanggalLahir = new Calendar();

        cmbJenisKelamin = new ComboBoxText();
        cmbJenisKelamin.AppendText("Laki-laki");
        cmbJenisKelamin.AppendText("Perempuan");
        cmbJenisKelamin.Active = 0;

        txtAlamat = CreateEntry("Alamat");
        txtPekerjaan = CreateEntry("Pekerjaan");

        cmbStatusPerkawinan = new ComboBoxText();
        cmbStatusPerkawinan.AppendText("Belum Kawin");
        cmbStatusPerkawinan.AppendText("Kawin");
        cmbStatusPerkawinan.AppendText("Cerai Hidup");
        cmbStatusPerkawinan.AppendText("Cerai Mati");
        cmbStatusPerkawinan.Active = 0;

        form.PackStart(txtNIK, false, false, 2);
        form.PackStart(txtNamaLengkap, false, false, 2);
        form.PackStart(calendarTanggalLahir, false, false, 2);
        form.PackStart(cmbJenisKelamin, false, false, 2);
        form.PackStart(txtAlamat, false, false, 2);
        form.PackStart(txtPekerjaan, false, false, 2);
        form.PackStart(cmbStatusPerkawinan, false, false, 2);

        frame.Add(form);
        root.PackStart(frame, false, false, 5);

        // Tombol
        HBox tombolBox = new HBox(true, 5);
        btnSimpan = new Button("Simpan");
        btnReset = new Button("Reset");
        btnHapus = new Button("Hapus");

        btnSimpan.Clicked += OnSimpan;
        btnReset.Clicked += OnReset;
        btnHapus.Clicked += OnHapus;

        tombolBox.PackStart(btnSimpan, true, true, 0);
        tombolBox.PackStart(btnReset, true, true, 0);
        tombolBox.PackStart(btnHapus, true, true, 0);

        root.PackStart(tombolBox, false, false, 5);

        // TreeView
        treeWarga = new TreeView();
        treeWarga.HeadersVisible = true;
        treeWarga.Selection.Changed += OnRowSelected;
        ScrolledWindow scroll = new ScrolledWindow();
        scroll.Add(treeWarga);
        root.PackStart(scroll, true, true, 5);

        Add(root);
        ShowAll();

        LoadData();
    }

    Entry CreateEntry(string placeholder)
    {
        Entry entry = new Entry { PlaceholderText = placeholder };
        return entry;
    }

    void LoadData()
    {
        DataTable dt = dbManager.GetAllWarga();

        if (treeWarga.Columns.Length == 0)
        {
            treeWarga.AppendColumn("NIK", new CellRendererText(), "text", 0);
            treeWarga.AppendColumn("Nama", new CellRendererText(), "text", 1);
            treeWarga.AppendColumn("Tanggal Lahir", new CellRendererText(), "text", 2);
            treeWarga.AppendColumn("JK", new CellRendererText(), "text", 3);
            treeWarga.AppendColumn("Alamat", new CellRendererText(), "text", 4);
            treeWarga.AppendColumn("Pekerjaan", new CellRendererText(), "text", 5);
            treeWarga.AppendColumn("Status", new CellRendererText(), "text", 6);
        }

        store = new ListStore(typeof(string), typeof(string), typeof(string), typeof(string), typeof(string), typeof(string), typeof(string));
        foreach (DataRow row in dt.Rows)
        {
            store.AppendValues(
                row["nik"].ToString(),
                row["nama"].ToString(),
                row["tanggal_lahir"].ToString(),
                row["jenis_kelamin"].ToString(),
                row["alamat"].ToString(),
                row["pekerjaan"].ToString(),
                row["status_perkawinan"].ToString()
            );
        }

        treeWarga.Model = store;
    }

    void OnSimpan(object sender, EventArgs e)
    {
        string nik = txtNIK.Text;
        string nama = txtNamaLengkap.Text;
        string tanggal = calendarTanggalLahir.Date.ToString("yyyy-MM-dd");
        string jk = cmbJenisKelamin.ActiveText;
        string alamat = txtAlamat.Text;
        string pekerjaan = txtPekerjaan.Text;
        string status = cmbStatusPerkawinan.ActiveText;

        if (string.IsNullOrEmpty(selectedNIK))
        {
            dbManager.InsertWarga(nik, nama, tanggal, jk, alamat, pekerjaan, status);
        }
        else
        {
            dbManager.UpdateWarga(nik, nama, tanggal, jk, alamat, pekerjaan, status);
        }

        ResetForm();
        LoadData();
    }

    void OnReset(object sender, EventArgs e)
    {
        ResetForm();
    }

    void OnHapus(object sender, EventArgs e)
    {
        if (!string.IsNullOrEmpty(selectedNIK))
        {
            dbManager.DeleteWarga(selectedNIK);
            ResetForm();
            LoadData();
        }
    }

    void OnRowSelected(object sender, EventArgs e)
    {
        if (treeWarga.Selection.GetSelected(out TreeIter iter))
        {
            selectedNIK = (string)treeWarga.Model.GetValue(iter, 0);
            txtNIK.Text = (string)treeWarga.Model.GetValue(iter, 0);
            txtNamaLengkap.Text = (string)treeWarga.Model.GetValue(iter, 1);
            calendarTanggalLahir.Date = DateTime.Parse((string)treeWarga.Model.GetValue(iter, 2));
            cmbJenisKelamin.Active = cmbJenisKelamin.GetTextIndex((string)treeWarga.Model.GetValue(iter, 3));
            txtAlamat.Text = (string)treeWarga.Model.GetValue(iter, 4);
            txtPekerjaan.Text = (string)treeWarga.Model.GetValue(iter, 5);
            cmbStatusPerkawinan.Active = cmbStatusPerkawinan.GetTextIndex((string)treeWarga.Model.GetValue(iter, 6));
        }
    }

    void ResetForm()
    {
        selectedNIK = "";
        txtNIK.Text = "";
        txtNamaLengkap.Text = "";
        calendarTanggalLahir.Date = DateTime.Today;
        cmbJenisKelamin.Active = 0;
        txtAlamat.Text = "";
        txtPekerjaan.Text = "";
        cmbStatusPerkawinan.Active = 0;
    }
}

static class ComboBoxTextExtensions
{
    public static int GetTextIndex(this ComboBoxText comboBox, string text)
    {
        for (int i = 0; i < comboBox.Model.IterNChildren(); i++)
        {
            comboBox.Model.IterNthChild(out TreeIter iter, i);
            if (comboBox.Model.GetValue(iter, 0).ToString() == text)
                return i;
        }
        return -1;
    }
}
