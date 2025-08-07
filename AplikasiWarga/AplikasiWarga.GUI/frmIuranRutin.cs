using Gtk;
using System;
using System.Collections.Generic;
using System.Data;

public class frmIuranRutin : Gtk.Window
{
    ComboBoxText cmbWarga;
    Entry txtJumlah, txtKeterangan;
    Calendar calendarTanggal;
    Button btnSimpan, btnReset, btnHapus;
    TreeView treeIuran;
    ListStore store;

    DatabaseManager dbManager = new DatabaseManager();
    int selectedId = 0;
    string selectedNik = "";

    public frmIuranRutin() : base("Pencatatan Iuran Rutin")
    {
        SetDefaultSize(800, 600);
        SetPosition(WindowPosition.Center);
        DeleteEvent += delegate { this.Destroy(); };

        VBox root = new VBox(false, 6);

        Frame frameInput = new Frame("Form Iuran");
        VBox form = new VBox(false, 4);

        cmbWarga = new ComboBoxText();
        LoadComboBoxWarga();

        txtJumlah = new Entry() { PlaceholderText = "Jumlah Iuran" };
        txtKeterangan = new Entry() { PlaceholderText = "Keterangan (opsional)" };
        calendarTanggal = new Calendar();

        form.PackStart(new Label("NIK - Nama Warga"), false, false, 2);
        form.PackStart(cmbWarga, false, false, 2);
        form.PackStart(new Label("Tanggal Iuran"), false, false, 2);
        form.PackStart(calendarTanggal, false, false, 2);
        form.PackStart(new Label("Jumlah"), false, false, 2);
        form.PackStart(txtJumlah, false, false, 2);
        form.PackStart(new Label("Keterangan"), false, false, 2);
        form.PackStart(txtKeterangan, false, false, 2);

        frameInput.Add(form);
        root.PackStart(frameInput, false, false, 5);

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

        treeIuran = new TreeView();
        treeIuran.HeadersVisible = true;
        treeIuran.Selection.Changed += OnRowSelected;
        ScrolledWindow scroll = new ScrolledWindow();
        scroll.Add(treeIuran);

        root.PackStart(scroll, true, true, 5);

        Add(root);
        ShowAll();
        LoadData();
    }

    void LoadComboBoxWarga()
    {
        var wargaList = dbManager.GetWargaForComboBox();
        cmbWarga.RemoveAll();
        foreach (var warga in wargaList)
        {
            cmbWarga.AppendText($"{warga.Item1} - {warga.Item2}");
        }
        cmbWarga.Active = 0;
    }

    void LoadData()
    {
        DataTable dt = dbManager.GetAllIuran();
        if (treeIuran.Columns.Length == 0)
        {
            treeIuran.AppendColumn("ID", new CellRendererText(), "text", 0);
            treeIuran.AppendColumn("NIK", new CellRendererText(), "text", 1);
            treeIuran.AppendColumn("Nama", new CellRendererText(), "text", 2);
            treeIuran.AppendColumn("Tanggal", new CellRendererText(), "text", 3);
            treeIuran.AppendColumn("Jumlah", new CellRendererText(), "text", 4);
            treeIuran.AppendColumn("Keterangan", new CellRendererText(), "text", 5);
        }

        store = new ListStore(typeof(int), typeof(string), typeof(string), typeof(string), typeof(int), typeof(string));
        foreach (DataRow row in dt.Rows)
        {
            store.AppendValues(
                Convert.ToInt32(row["IdIuran"]),
                row["NIK_Warga"].ToString(),
                row["Nama"].ToString(),
                row["TanggalIuran"].ToString(),
                Convert.ToInt32(row["Jumlah"]),
                row["Keterangan"].ToString()
            );
        }
        treeIuran.Model = store;
    }

    void OnSimpan(object sender, EventArgs e)
{
    string selected = cmbWarga.ActiveText;
    if (string.IsNullOrWhiteSpace(selected))
    {
        ShowMessage("Silakan pilih warga terlebih dahulu.", MessageType.Warning);
        return;
    }

    if (!int.TryParse(txtJumlah.Text, out int jumlah) || jumlah <= 0)
    {
        ShowMessage("Jumlah iuran harus berupa angka lebih dari nol.", MessageType.Warning);
        return;
    }

    string nik = selected.Split(' ')[0];
    DateTime tanggal = calendarTanggal.Date;
    string keterangan = txtKeterangan.Text.Trim();

    try
    {
        dbManager.SaveIuran(selectedId, nik, tanggal, jumlah, keterangan);

        if (selectedId == 0)
            ShowMessage("Data iuran berhasil ditambahkan.", MessageType.Info);
        else
            ShowMessage("Data iuran berhasil diperbarui.", MessageType.Info);

        ResetForm();
        LoadData();
    }
    catch (Exception ex)
    {
        ShowMessage("Gagal menyimpan data: " + ex.Message, MessageType.Error);
    }
}


    void OnReset(object sender, EventArgs e) => ResetForm();

    void OnHapus(object sender, EventArgs e)
    {
        if (selectedId != 0)
        {
            dbManager.DeleteIuran(selectedId);
            ShowMessage("Data iuran berhasil dihapus.", MessageType.Info);
            ResetForm();
            LoadData();
        }
    }

    void OnRowSelected(object sender, EventArgs e)
    {
        if (treeIuran.Selection.GetSelected(out TreeIter iter))
        {
            selectedId = (int)treeIuran.Model.GetValue(iter, 0);
            selectedNik = (string)treeIuran.Model.GetValue(iter, 1);

            string nama = (string)treeIuran.Model.GetValue(iter, 2);
            string tgl = (string)treeIuran.Model.GetValue(iter, 3);
            string combo = $"{selectedNik} - {nama}";

            cmbWarga.Active = cmbWarga.GetTextIndex(combo);
            calendarTanggal.Date = DateTime.Parse(tgl);
            txtJumlah.Text = treeIuran.Model.GetValue(iter, 4).ToString();
            txtKeterangan.Text = treeIuran.Model.GetValue(iter, 5).ToString();
        }
    }

    void ResetForm()
    {
        selectedId = 0;
        selectedNik = "";
        cmbWarga.Active = 0;
        calendarTanggal.Date = DateTime.Today;
        txtJumlah.Text = "";
        txtKeterangan.Text = "";
    }

    void ShowMessage(string message, MessageType type)
{
    MessageDialog md = new MessageDialog(this,
        DialogFlags.Modal,
        type,
        ButtonsType.Ok,
        message);
    md.Run();
    md.Destroy();
}

}
