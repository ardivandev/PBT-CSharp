using System;
using System.Data;
using Gtk;

public class frmDataWarga : Window
{
  private DatabaseManager dbManager;
  private string selectedNIK = string.Empty;
  private Entry txtNIK, txtNamaLengkap, txtAlamat, txtPekerjaan;
  private Calendar dtpTanggalLahir;
  private ComboBoxText cmbJenisKelamin, cmbStatusPerkawinan;
  private Button btnSimpan, btnReset, btnHapus;
  private TreeView treeWarga;
  private ListStore store;

  public frmDataWarga() : base("Pencatatan Data Warga")
  {
    SetDefaultSize(800, 600);
    SetPosition(WindowPosition.Center);
    DeleteEvent += delegate { this.Hide(); };

    dbManager = new DatabaseManager();

    VBox root = new VBox(false, 8);

    Frame frame = new Frame("Data Warga");
    VBox form = new VBox(false, 4);

    txtNIK = CreateEntry("NIK");
    txtNamaLengkap = CreateEntry("Nama Lengkap");
    dtpTanggalLahir = new Calendar();

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
    form.PackStart(dtpTanggalLahir, false, false, 2);
    form.PackStart(cmbJenisKelamin, false, false, 2);
    form.PackStart(txtAlamat, false, false, 2);
    form.PackStart(txtPekerjaan, false, false, 2);
    form.PackStart(cmbStatusPerkawinan, false, false, 2);

    frame.Add(form);
    root.PackStart(frame, false, false, 5);

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
          row["Nik"].ToString(),
          row["Nama"].ToString(),
          row["TanggalLahir"].ToString(),
          row["JenisKelamin"].ToString(),
          row["Alamat"].ToString(),
          row["Pekerjaan"].ToString(),
          row["StatusPerkawinan"].ToString()
      );
    }

    treeWarga.Model = store;
  }

  void OnSimpan(object sender, EventArgs e)
  {
    string nik = txtNIK.Text.Trim();
    string nama = txtNamaLengkap.Text.Trim();
    string tanggal = dtpTanggalLahir.Date.ToString("yyyy-MM-dd");
    string jk = cmbJenisKelamin.ActiveText;
    string alamat = txtAlamat.Text.Trim();
    string pekerjaan = txtPekerjaan.Text.Trim();
    string status = cmbStatusPerkawinan.ActiveText;

    // Validasi
    if (string.IsNullOrWhiteSpace(nik) || string.IsNullOrWhiteSpace(nama) ||
        string.IsNullOrWhiteSpace(alamat) || string.IsNullOrWhiteSpace(pekerjaan))
    {
      ShowMessage("Semua field wajib diisi.", MessageType.Warning);
      return;
    }

    if (nik.Length != 16 || !ulong.TryParse(nik, out _))
    {
        ShowMessage("NIK harus terdiri dari 16 digit angka.", MessageType.Warning);
        return;
    }

    try
    {
      if (string.IsNullOrEmpty(selectedNIK))
      {
        dbManager.InsertWarga(nik, nama, tanggal, jk, alamat, pekerjaan, status);
        ShowMessage("Data warga berhasil ditambahkan.", MessageType.Info);
      }
      else
      {
        dbManager.UpdateWarga(nik, nama, tanggal, jk, alamat, pekerjaan, status);
        ShowMessage("Data warga berhasil diperbarui.", MessageType.Info);
      }

      ResetForm();
      LoadData();
    }
    catch (Exception ex)
    {
      ShowMessage("Terjadi kesalahan saat menyimpan: " + ex.Message, MessageType.Error);
    }
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
      ShowMessage("Data warga berhasil dihapus.", MessageType.Info);
      ResetForm();
      LoadData();
    }
    else
    {
      ShowMessage("Pilih data yang ingin dihapus.", MessageType.Warning);
    }
  }


  void OnRowSelected(object sender, EventArgs e)
  {
    if (treeWarga.Selection.GetSelected(out TreeIter iter))
    {
      selectedNIK = (string)treeWarga.Model.GetValue(iter, 0);
      txtNIK.Text = (string)treeWarga.Model.GetValue(iter, 0);
      txtNamaLengkap.Text = (string)treeWarga.Model.GetValue(iter, 1);
      dtpTanggalLahir.Date = DateTime.Parse((string)treeWarga.Model.GetValue(iter, 2));
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
    dtpTanggalLahir.Date = DateTime.Today;
    cmbJenisKelamin.Active = 0;
    txtAlamat.Text = "";
    txtPekerjaan.Text = "";
    cmbStatusPerkawinan.Active = 0;
  }

    void ShowMessage(string message, MessageType type)
    {
        using var msgDialog = new MessageDialog(
            this,
            DialogFlags.Modal,
            type,
            ButtonsType.Ok,
            message
        );
        msgDialog.Run();
        msgDialog.Destroy();
    }
}
