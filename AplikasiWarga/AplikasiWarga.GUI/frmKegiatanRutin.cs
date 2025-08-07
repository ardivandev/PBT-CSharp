using System;
using Gtk;
using System.Data;
using System.Collections.Generic;

public class frmKegiatanRutin : Window
{
  private DatabaseManager dbManager;
  private int selectedKegiatanId = 0;

  private ComboBoxText cmbWarga;
  private Entry txtNamaKegiatan, txtKeterangan;
  private Calendar calendarTanggalKegiatan;
  private Button btnSimpan, btnReset, btnHapus, btnUbah;
  private TreeView treeKegiatan;
  private ListStore store;

  public frmKegiatanRutin() : base("Pencatatan Kegiatan Rutin Warga")
  {
    SetDefaultSize(800, 600);
    SetPosition(WindowPosition.Center);
    DeleteEvent += delegate { this.Destroy(); };

    dbManager = new DatabaseManager();

    VBox root = new VBox(false, 8);

    Frame frame = new Frame("Input Kegiatan");
    VBox form = new VBox(false, 4);

    cmbWarga = new ComboBoxText();
    LoadComboBoxWarga();
    txtNamaKegiatan = new Entry();
    calendarTanggalKegiatan = new Calendar();
    txtKeterangan = new Entry { PlaceholderText = "Keterangan" };

    form.PackStart(new Label("NIK/Nama Warga:"), false, false, 2);
    form.PackStart(cmbWarga, false, false, 2);
    form.PackStart(new Label("Nama Kegiatan:"), false, false, 2);
    form.PackStart(txtNamaKegiatan, false, false, 2);
    form.PackStart(new Label("Tanggal Kegiatan:"), false, false, 2);
    form.PackStart(calendarTanggalKegiatan, false, false, 2);
    form.PackStart(new Label("Keterangan:"), false, false, 2);
    form.PackStart(txtKeterangan, false, false, 2);

    frame.Add(form);
    root.PackStart(frame, false, false, 5);

    HBox tombolBox = new HBox(true, 5);
    btnSimpan = new Button("Simpan");
    btnReset = new Button("Reset");
    btnHapus = new Button("Hapus");
    btnUbah = new Button("Ubah");

    btnSimpan.Clicked += OnSimpan;
    btnReset.Clicked += OnReset;
    btnHapus.Clicked += OnHapus;
    btnUbah.Clicked += OnUbah;

    tombolBox.PackStart(btnSimpan, true, true, 0);
    tombolBox.PackStart(btnReset, true, true, 0);
    tombolBox.PackStart(btnHapus, true, true, 0);
    tombolBox.PackStart(btnUbah, true, true, 0);

    root.PackStart(tombolBox, false, false, 5);

    treeKegiatan = new TreeView();
    treeKegiatan.HeadersVisible = true;
    treeKegiatan.Selection.Changed += OnRowSelected;
    ScrolledWindow scroll = new ScrolledWindow();
    scroll.Add(treeKegiatan);
    root.PackStart(scroll, true, true, 5);

    Add(root);
    ShowAll();

    LoadData();
  }

  private void LoadComboBoxWarga()
  {
    var list = dbManager.GetWargaForComboBox();
    foreach (var warga in list)
    {
      cmbWarga.AppendText($"{warga.Item1} - {warga.Item2}");
    }
    if (list.Count > 0) cmbWarga.Active = 0;
  }

  private void LoadData()
  {
    DataTable dt = dbManager.GetAllKegiatan();

    if (treeKegiatan.Columns.Length == 0)
    {
      treeKegiatan.AppendColumn("ID", new CellRendererText(), "text", 0);
      treeKegiatan.AppendColumn("NIK", new CellRendererText(), "text", 1);
      treeKegiatan.AppendColumn("Nama Kegiatan", new CellRendererText(), "text", 2);
      treeKegiatan.AppendColumn("Tanggal", new CellRendererText(), "text", 3);
      treeKegiatan.AppendColumn("Keterangan", new CellRendererText(), "text", 4);
    }

    store = new ListStore(typeof(int), typeof(string), typeof(string), typeof(string), typeof(string));
    foreach (DataRow row in dt.Rows)
    {
      store.AppendValues(
          Convert.ToInt32(row["IdKegiatan"]),
          row["NIK_Warga"].ToString(),
          row["NamaKegiatan"].ToString(),
          row["TanggalKegiatan"].ToString(),
          row["Keterangan"].ToString()
      );
    }

    treeKegiatan.Model = store;
  }

  private void OnSimpan(object sender, EventArgs e)
  {
    if (!ValidateInput()) return;

    var nik = cmbWarga.ActiveText.Split('-')[0].Trim();
    var tanggal = calendarTanggalKegiatan.Date;
    var namaKegiatan = txtNamaKegiatan.Text.Trim();
    var keterangan = txtKeterangan.Text.Trim();

    try
    {
      dbManager.InsertKegiatan(nik, namaKegiatan, tanggal, keterangan);
      ShowMessage("Data kegiatan berhasil ditambahkan.", MessageType.Info);
      LoadData();
      ResetForm();
    }
    catch (Exception ex)
    {
      ShowMessage("Gagal menyimpan data: " + ex.Message, MessageType.Error);
    }
  }

  private void OnReset(object sender, EventArgs e)
  {
    ResetForm();
  }

  private void OnUbah(object sender, EventArgs e)
  {
    if (selectedKegiatanId == 0)
    {
      ShowMessage("Silakan pilih data yang ingin diubah.", MessageType.Warning);
      return;
    }

    if (!ValidateInput()) return;

    var nik = cmbWarga.ActiveText.Split('-')[0].Trim();
    var tanggal = calendarTanggalKegiatan.Date;
    var namaKegiatan = txtNamaKegiatan.Text.Trim();
    var keterangan = txtKeterangan.Text.Trim();

    try
    {
      dbManager.UpdateKegiatan(selectedKegiatanId, nik, namaKegiatan, tanggal, keterangan);
      ShowMessage("Data kegiatan berhasil diubah.", MessageType.Info);
      LoadData();
      ResetForm();
    }
    catch (Exception ex)
    {
      ShowMessage("Gagal mengubah data: " + ex.Message, MessageType.Error);
    }
  }

  private void OnHapus(object sender, EventArgs e)
  {
    if (selectedKegiatanId == 0) return;
    dbManager.DeleteKegiatan(selectedKegiatanId);
    ShowMessage("Data kegiatan berhasil dihapus.", MessageType.Info);
    LoadData();
    ResetForm();
  }

  private void OnRowSelected(object sender, EventArgs e)
  {
    if (treeKegiatan.Selection.GetSelected(out TreeIter iter))
    {
      selectedKegiatanId = (int)treeKegiatan.Model.GetValue(iter, 0);
      string nik = (string)treeKegiatan.Model.GetValue(iter, 1);
      string namaKegiatan = (string)treeKegiatan.Model.GetValue(iter, 2);
      string tanggal = (string)treeKegiatan.Model.GetValue(iter, 3);
      string keterangan = (string)treeKegiatan.Model.GetValue(iter, 4);

      int index = GetComboBoxIndexByNIK(nik);
      cmbWarga.Active = index;
      txtNamaKegiatan.Text = namaKegiatan;
      calendarTanggalKegiatan.Date = DateTime.Parse(tanggal);
      txtKeterangan.Text = keterangan;
    }
  }

  private int GetComboBoxIndexByNIK(string nik)
  {
    for (int i = 0; i < cmbWarga.Model.IterNChildren(); i++)
    {
      cmbWarga.Model.IterNthChild(out TreeIter iter, i);
      string text = cmbWarga.Model.GetValue(iter, 0).ToString();
      if (text.StartsWith(nik)) return i;
    }
    return -1;
  }

  private void ResetForm()
  {
    cmbWarga.Active = -1;
    txtNamaKegiatan.Text = "";
    txtKeterangan.Text = "";
    calendarTanggalKegiatan.Date = DateTime.Today;
    selectedKegiatanId = 0;
  }

  private bool ValidateInput()
  {
    if (cmbWarga.Active == -1)
    {
      ShowMessage("Silakan pilih warga.", MessageType.Warning);
      return false;
    }

    if (string.IsNullOrWhiteSpace(txtNamaKegiatan.Text))
    {
      ShowMessage("Nama kegiatan tidak boleh kosong.", MessageType.Warning);
      return false;
    }

    if (txtNamaKegiatan.Text.Length > 100)
    {
      ShowMessage("Nama kegiatan terlalu panjang (maksimal 100 karakter).", MessageType.Warning);
      return false;
    }

    if (txtKeterangan.Text.Length > 255)
    {
      ShowMessage("Keterangan terlalu panjang (maksimal 255 karakter).", MessageType.Warning);
      return false;
    }

    if (calendarTanggalKegiatan.Date > DateTime.Today)
    {
      ShowMessage("Tanggal kegiatan tidak boleh di masa depan.", MessageType.Warning);
      return false;
    }

    return true;
  }

private void ShowMessage(string message, MessageType type)
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
