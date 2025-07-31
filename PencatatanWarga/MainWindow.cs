using System;
using Gtk;
using System.Data;
using System.Text.RegularExpressions;

namespace AplikasiPencatatanWarga
{
    public class MainWindow : Window
    {
        private readonly TreeView treeView;
        private readonly ListStore listStore;
        private readonly DatabaseManager db;

        public MainWindow() : base("Aplikasi Pencatatan Warga")
        {
            // Inisialisasi window
            SetDefaultSize(800, 400);
            SetPosition(WindowPosition.Center);
            DeleteEvent += (o, args) => Application.Quit();

            // Inisialisasi komponen
            db = new DatabaseManager();
            listStore = new ListStore(
                typeof(string), typeof(string), typeof(string),
                typeof(string), typeof(string), typeof(string),
                typeof(string), typeof(string)
            );

            // Layout utama
            var vbox = new Box(Orientation.Vertical, 5);
            Add(vbox);

            // Tombol Tambah
            var btnTambah = new Button("Tambah Warga") { Margin = 5 };
            btnTambah.Clicked += OnTambahClicked;
            vbox.PackStart(btnTambah, false, false, 0);

            // TreeView
            treeView = new TreeView { Expand = true };
            SetupTreeViewColumns();
            vbox.PackStart(new ScrolledWindow { Child = treeView }, true, true, 0);

            // Muat data awal
            RefreshData();
        }

        private void SetupTreeViewColumns()
        {
            treeView.Model = listStore;

            // Kolom data
            string[] headers = { "NIK", "Nama", "Tanggal Lahir", "JK", "Alamat", "Pekerjaan", "Status" };
            for (int i = 0; i < headers.Length; i++)
            {
                treeView.AppendColumn(headers[i], new CellRendererText(), "text", i);
            }

            // Kolom aksi
            var actionCol = new TreeViewColumn { Title = "Aksi" };
            var actionRenderer = new CellRendererText { Foreground = "blue" };

            actionCol.PackStart(actionRenderer, true);
            actionCol.SetCellDataFunc(actionRenderer, (TreeCellDataFunc)((col, cell, model, iter) =>
            {
                ((CellRendererText)cell).Text = "Edit | Hapus";
            }));

            actionRenderer.Edited += OnActionEdited;
            treeView.AppendColumn(actionCol);
        }

        private void OnActionEdited(object? sender, EditedArgs args)
        {
            if (listStore.GetIterFromString(out var iter, args.Path))
            {
                string nik = (string)listStore.GetValue(iter, 0);
                string action = args.NewText.ToLower();

                if (action.Contains("hapus"))
                {
                    DeleteWarga(nik);
                }
                else if (action.Contains("edit"))
                {
                    EditWarga(nik);
                }
            }
        }

        private void DeleteWarga(string nik)
        {
            var confirm = new MessageDialog(
                this,
                DialogFlags.Modal,
                MessageType.Question,
                ButtonsType.YesNo,
                "Apakah Anda yakin ingin menghapus data ini?"
            );

            if (confirm.Run() == (int)ResponseType.Yes && db.DeleteWarga(nik))
            {
                ShowMessage("Data berhasil dihapus", MessageType.Info);
                RefreshData();
            }
            confirm.Destroy();
        }

        private void EditWarga(string nik)
        {
            var row = db.GetWargaByNIK(nik);
            if (row == null) return;

            var dialog = new Dialog("Edit Warga", this, DialogFlags.Modal)
            {
                DefaultWidth = 350,
                DefaultHeight = 300
            };

            // Buat form edit
            var form = new Box(Orientation.Vertical, 5) { BorderWidth = 10 };
            dialog.ContentArea.PackStart(form, true, true, 0);
            var nikEntry = CreateFormField(form, "NIK", row["nik"]?.ToString() ?? "", false);
            var namaEntry = CreateFormField(form, "Nama Lengkap", row["namalengkap"]?.ToString() ?? "");
            var tglEntry = CreateFormField(form, "Tanggal Lahir (yyyy-MM-dd)", row["tanggallahir"]?.ToString() ?? "");
            var jkEntry = CreateFormField(form, "Jenis Kelamin", row["jeniskelamin"]?.ToString() ?? "");
            var alamatEntry = CreateFormField(form, "Alamat", row["alamat"]?.ToString() ?? "");
            var pekerjaanEntry = CreateFormField(form, "Pekerjaan", row["pekerjaan"]?.ToString() ?? "");
            var statusEntry = CreateFormField(form, "Status Perkawinan", row["statusperkawinan"]?.ToString() ?? "");

            dialog.AddButton("Batal", ResponseType.Cancel);
            dialog.AddButton("Simpan", ResponseType.Ok);

            if (dialog.Run() == (int)ResponseType.Ok)
            {
                var values = (
                    nik: nikEntry.Text.Trim(),
                    nama: namaEntry.Text.Trim(),
                    tanggalLahir: tglEntry.Text.Trim(),
                    jenisKelamin: jkEntry.Text.Trim(),
                    alamat: alamatEntry.Text.Trim(),
                    pekerjaan: pekerjaanEntry.Text.Trim(),
                    status: statusEntry.Text.Trim()
                );

                if (ValidateWargaData(values, false))
                {
                    if (db.SaveWarga(
                        values.nik, values.nama,
                        DateTime.Parse(values.tanggalLahir),
                        values.jenisKelamin, values.alamat,
                        values.pekerjaan, values.status))
                    {
                        ShowMessage("Data berhasil diubah", MessageType.Info);
                        RefreshData();
                    }
                    else
                    {
                        ShowMessage("Gagal mengubah data", MessageType.Error);
                    }
                }
            }
            dialog.Destroy();
        }

      private void OnTambahClicked(object? sender, EventArgs e)
{
    var dialog = new Dialog("Tambah Warga", this, DialogFlags.Modal)
    {
        DefaultWidth = 350,
        DefaultHeight = 400,
        BorderWidth = 10
    };

    // Buat form tambah
    var form = new Box(Orientation.Vertical, 5);
    dialog.ContentArea.PackStart(form, true, true, 0);

    var nikEntry = new Entry { PlaceholderText = "NIK", WidthRequest = 300 };
    var namaEntry = new Entry { PlaceholderText = "Nama Lengkap", WidthRequest = 300 };
    var tglEntry = new Entry { PlaceholderText = "Tanggal Lahir (YYYY-MM-DD)", WidthRequest = 300 };

    // ComboBox Jenis Kelamin
    var jkCombo = new ComboBoxText();
    jkCombo.AppendText("Laki-laki");
    jkCombo.AppendText("Perempuan");

    var alamatEntry = new Entry { PlaceholderText = "Alamat", WidthRequest = 300 };
    var pekerjaanEntry = new Entry { PlaceholderText = "Pekerjaan", WidthRequest = 300 };

    // ComboBox Status
    var statusCombo = new ComboBoxText();
    statusCombo.AppendText("Sudah Menikah");
    statusCombo.AppendText("Belum Menikah");
    statusCombo.AppendText("Cerai Hidup");
    statusCombo.AppendText("Cerai Mati");

    form.PackStart(new Label("NIK:"), false, false, 0);
    form.PackStart(nikEntry, false, false, 0);
    form.PackStart(new Label("Nama Lengkap:"), false, false, 0);
    form.PackStart(namaEntry, false, false, 0);
    form.PackStart(new Label("Tanggal Lahir:"), false, false, 0);
    form.PackStart(tglEntry, false, false, 0);
    form.PackStart(new Label("Jenis Kelamin:"), false, false, 0);
    form.PackStart(jkCombo, false, false, 0);
    form.PackStart(new Label("Alamat:"), false, false, 0);
    form.PackStart(alamatEntry, false, false, 0);
    form.PackStart(new Label("Pekerjaan:"), false, false, 0);
    form.PackStart(pekerjaanEntry, false, false, 0);
    form.PackStart(new Label("Status Perkawinan:"), false, false, 0);
    form.PackStart(statusCombo, false, false, 0);

    dialog.AddButton("Batal", ResponseType.Cancel);
    dialog.AddButton("Simpan", ResponseType.Ok);

    // Tampilkan dialog
    dialog.ShowAll();

    if (dialog.Run() == (int)ResponseType.Ok)
    {
        var values = (
            nik: nikEntry.Text.Trim(),
            nama: namaEntry.Text.Trim(),
            tanggalLahir: tglEntry.Text.Trim(),
            jenisKelamin: jkCombo.ActiveText,
            alamat: alamatEntry.Text.Trim(),
            pekerjaan: pekerjaanEntry.Text.Trim(),
            status: statusCombo.ActiveText
        );

        if (ValidateWargaData(values, true))
        {
            if (db.SaveWarga(
                values.nik, values.nama,
                DateTime.Parse(values.tanggalLahir),
                values.jenisKelamin, values.alamat,
                values.pekerjaan, values.status))
            {
                ShowMessage("Data berhasil disimpan", MessageType.Info);
                RefreshData();
            }
            else
            {
                ShowMessage("Gagal menyimpan data", MessageType.Error);
            }
        }
    }
    dialog.Destroy();
}

        private Entry CreateFormField(Box container, string label, string defaultValue, bool editable = true)
        {
            var hbox = new Box(Orientation.Horizontal, 5);
            hbox.PackStart(new Label(label + ":") { Xalign = 0 }, false, false, 0);

            var entry = new Entry(defaultValue)
            {
                IsEditable = editable,
                WidthRequest = 200
            };

            hbox.PackStart(entry, true, true, 0);
            container.PackStart(hbox, false, false, 0);

            return entry;
        }

        private bool ValidateWargaData(
            (string nik, string nama, string tanggalLahir, string jenisKelamin,
             string alamat, string pekerjaan, string status) data, bool checkNIK)
        {
            // Validasi kelengkapan data
            if (string.IsNullOrWhiteSpace(data.nik) ||
        string.IsNullOrWhiteSpace(data.nama) ||
        string.IsNullOrWhiteSpace(data.tanggalLahir) ||
        string.IsNullOrWhiteSpace(data.jenisKelamin) ||
        string.IsNullOrWhiteSpace(data.alamat) ||
        string.IsNullOrWhiteSpace(data.pekerjaan) ||
        string.IsNullOrWhiteSpace(data.status))
    {
        ShowMessage("Semua field harus diisi", MessageType.Error);
        return false;
    }

            // Validasi NIK
            if (checkNIK)
            {
                if (data.nik.Length != 16 || !Regex.IsMatch(data.nik, @"^\d+$"))
                {
                    ShowMessage("NIK harus 16 digit angka", MessageType.Error);
                    return false;
                }

                if (db.IsNikExist(data.nik))
                {
                    ShowMessage("NIK sudah terdaftar", MessageType.Error);
                    return false;
                }
            }

            // Validasi nama
            if (Regex.IsMatch(data.nama, @"\d"))
            {
                ShowMessage("Nama tidak boleh mengandung angka", MessageType.Error);
                return false;
            }

            // Validasi tanggal
            if (!DateTime.TryParse(data.tanggalLahir, out _))
            {
                ShowMessage("Format tanggal tidak valid (gunakan yyyy-MM-dd)", MessageType.Error);
                return false;
            }

            return true;
        }

        private void RefreshData()
        {
            listStore.Clear();
            var dt = db.GetAllWarga();

            foreach (DataRow row in dt.Rows)
            {
                listStore.AppendValues(
                    row["nik"].ToString(),
                    row["namalengkap"]?.ToString() ?? "",
                    row["tanggallahir"].ToString(),
                    row["jeniskelamin"].ToString(),
                    row["alamat"].ToString(),
                    row["pekerjaan"].ToString(),
                    row["statusperkawinan"].ToString(),
                    "Edit | Hapus"
                );
            }
        }

        private void ShowMessage(string message, MessageType type)
        {
            var md = new MessageDialog(
                this,
                DialogFlags.Modal,
                type,
                ButtonsType.Ok,
                message
            );
            md.Run();
            md.Destroy();
        }
    }
}
