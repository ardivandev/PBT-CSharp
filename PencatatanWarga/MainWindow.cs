using System;
using Gtk;
using System.Data;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace AplikasiPencatatanWarga
{
    public class MainWindow : Window
    {
        private readonly TreeView treeView;
        private readonly ListStore listStore;
        private readonly DatabaseManager db;

        public MainWindow() : base("Aplikasi Pencatatan Warga")
        {
            // Window configuration
            SetDefaultSize(800, 400);
            SetPosition(WindowPosition.Center);
            DeleteEvent += (o, args) => Application.Quit();

            // Initialize components
            db = new DatabaseManager();
            listStore = new ListStore(
                typeof(string), typeof(string), typeof(string),
                typeof(string), typeof(string), typeof(string),
                typeof(string), typeof(string)
            );

            // Main layout
            var vbox = new Box(Orientation.Vertical, 5);
            Add(vbox);

            // Add button
            var btnTambah = new Button("Tambah Warga") { Margin = 5 };
            btnTambah.Clicked += OnTambahClicked;
            vbox.PackStart(btnTambah, false, false, 0);

            // TreeView setup
            treeView = new TreeView { Expand = true };
            SetupTreeViewColumns();
            vbox.PackStart(new ScrolledWindow { Child = treeView }, true, true, 0);

            // Load initial data
            _ = RefreshDataAsync();
        }

        private void SetupTreeViewColumns()
        {
            treeView.Model = listStore;

            // Data columns
            string[] headers = { "NIK", "Nama", "Tanggal Lahir", "JK", "Alamat", "Pekerjaan", "Status" };
            for (int i = 0; i < headers.Length; i++)
            {
                treeView.AppendColumn(headers[i], new CellRendererText(), "text", i);
            }

            // Action column
            var actionCol = new TreeViewColumn { Title = "Aksi" };
            var actionRenderer = new CellRendererText
            {
                Foreground = "blue",
                Editable = true
            };

            actionCol.PackStart(actionRenderer, true);
            actionCol.SetCellDataFunc(actionRenderer, new TreeCellDataFunc((col, cell, model, iter) =>
            {
                ((CellRendererText)cell).Text = "Edit | Hapus";
            }));

            actionRenderer.Edited += OnActionEdited;
            treeView.AppendColumn(actionCol);
        }

        private async void OnActionEdited(object o, EditedArgs args)
        {
            if (listStore.GetIterFromString(out var iter, args.Path))
            {
                string nik = (string)listStore.GetValue(iter, 0);
                string action = args.NewText.ToLower();

                if (action.Contains("hapus"))
                {
                    await DeleteWargaAsync(nik);
                }
                else if (action.Contains("edit"))
                {
                    await EditWargaAsync(nik);
                }
            }
        }

        private async Task DeleteWargaAsync(string nik)
        {
            var confirm = new MessageDialog(
                this,
                DialogFlags.Modal,
                MessageType.Question,
                ButtonsType.YesNo,
                $"Apakah Anda yakin ingin menghapus data warga dengan NIK {nik}?"
            );

            if (confirm.Run() == (int)ResponseType.Yes)
            {
                bool success = await db.DeleteWargaAsync(nik);
                if (success)
                {
                    ShowMessage("Data berhasil dihapus", MessageType.Info);
                    await RefreshDataAsync();
                }
                else
                {
                    ShowMessage("Gagal menghapus data", MessageType.Error);
                }
            }
            confirm.Destroy();
        }
        private async Task EditWargaAsync(string nik)
{
    var row = await db.GetWargaByNIKAsync(nik);
    if (row == null) return;

    var dialog = new Dialog("Edit Warga", this, DialogFlags.Modal)
    {
        DefaultWidth = 350,
        DefaultHeight = 400,
        BorderWidth = 10
    };

    var form = new Box(Orientation.Vertical, 5) { Margin = 10 };
    dialog.ContentArea.PackStart(form, true, true, 0);

    // NIK Field (non-editable)
    form.PackStart(new Label("NIK:"), false, false, 0);
    var nikEntry = new Entry(row["nik"]?.ToString() ?? "")
    {
        IsEditable = false,
        WidthRequest = 300
    };
    form.PackStart(nikEntry, false, false, 0);

    // Nama Lengkap
    form.PackStart(new Label("Nama Lengkap:"), false, false, 0);
    var namaEntry = new Entry(row["namalengkap"]?.ToString() ?? "")
    {
        PlaceholderText = "Nama Lengkap",
        WidthRequest = 300
    };
    form.PackStart(namaEntry, false, false, 0);

    // Tanggal Lahir
    form.PackStart(new Label("Tanggal Lahir:"), false, false, 0);
    var tglEntry = new Entry(row["tanggallahir"]?.ToString() ?? "")
    {
        PlaceholderText = "Tanggal Lahir (YYYY-MM-DD)",
        WidthRequest = 300
    };
    form.PackStart(tglEntry, false, false, 0);

    // Jenis Kelamin ComboBox
    form.PackStart(new Label("Jenis Kelamin:"), false, false, 0);
    var jkCombo = new ComboBoxText();
    jkCombo.AppendText("Laki-laki");
    jkCombo.AppendText("Perempuan");

    // Set active value from database
    string currentJK = row["jeniskelamin"]?.ToString() ?? "";
    if (currentJK == "Laki-laki") jkCombo.Active = 0;
    else if (currentJK == "Perempuan") jkCombo.Active = 1;

    form.PackStart(jkCombo, false, false, 0);

    // Alamat
    form.PackStart(new Label("Alamat:"), false, false, 0);
    var alamatEntry = new Entry(row["alamat"]?.ToString() ?? "")
    {
        PlaceholderText = "Alamat",
        WidthRequest = 300
    };
    form.PackStart(alamatEntry, false, false, 0);

    // Pekerjaan
    form.PackStart(new Label("Pekerjaan:"), false, false, 0);
    var pekerjaanEntry = new Entry(row["pekerjaan"]?.ToString() ?? "")
    {
        PlaceholderText = "Pekerjaan",
        WidthRequest = 300
    };
    form.PackStart(pekerjaanEntry, false, false, 0);

    // Status Perkawinan ComboBox
    form.PackStart(new Label("Status Perkawinan:"), false, false, 0);
    var statusCombo = new ComboBoxText();
    statusCombo.AppendText("Sudah Menikah");
    statusCombo.AppendText("Belum Menikah");
    statusCombo.AppendText("Cerai Hidup");
    statusCombo.AppendText("Cerai Mati");

    // Set active value from database
    string currentStatus = row["statusperkawinan"]?.ToString() ?? "";
    statusCombo.Active = currentStatus switch
    {
        "Sudah Menikah" => 0,
        "Belum Menikah" => 1,
        "Cerai Hidup" => 2,
        "Cerai Mati" => 3,
        _ => -1
    };

    form.PackStart(statusCombo, false, false, 0);

    dialog.AddButton("Batal", ResponseType.Cancel);
    dialog.AddButton("Simpan", ResponseType.Ok);

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

        if (ValidateWargaData(values, false))
        {
            var spinner = new Spinner { Active = true };
            form.PackStart(spinner, false, false, 5);
            form.ShowAll();

            bool result = await db.SaveWargaAsync(
                values.nik, values.nama,
                DateTime.Parse(values.tanggalLahir),
                values.jenisKelamin, values.alamat,
                values.pekerjaan, values.status);

            spinner.Destroy();

            if (result)
            {
                ShowMessage("Data berhasil diubah", MessageType.Info);
                await RefreshDataAsync();
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
      _ = TambahWargaAsync();
    }

        private async Task TambahWargaAsync()
        {
            try
            {
                var dialog = new Dialog("Tambah Warga", this, DialogFlags.Modal)
                {
                    DefaultWidth = 350,
                    DefaultHeight = 400,
                    BorderWidth = 10
                };

                var form = new Box(Orientation.Vertical, 5) { Margin = 10 };
                dialog.ContentArea.PackStart(form, true, true, 0);

                // Create form fields
                var nikEntry = new Entry { PlaceholderText = "NIK", WidthRequest = 300 };
                var namaEntry = new Entry { PlaceholderText = "Nama Lengkap", WidthRequest = 300 };
                var tglEntry = new Entry { PlaceholderText = "Tanggal Lahir (YYYY-MM-DD)", WidthRequest = 300 };

                var jkCombo = new ComboBoxText();
                jkCombo.AppendText("Laki-laki");
                jkCombo.AppendText("Perempuan");

                var alamatEntry = new Entry { PlaceholderText = "Alamat", WidthRequest = 300 };
                var pekerjaanEntry = new Entry { PlaceholderText = "Pekerjaan", WidthRequest = 300 };

                var statusCombo = new ComboBoxText();
                statusCombo.AppendText("Sudah Menikah");
                statusCombo.AppendText("Belum Menikah");
                statusCombo.AppendText("Cerai Hidup");
                statusCombo.AppendText("Cerai Mati");

                // Add fields to form
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
                        var spinner = new Spinner { Active = true };
                        form.PackStart(spinner, false, false, 5);
                        form.ShowAll();

                        bool result = await db.SaveWargaAsync(
                            values.nik, values.nama,
                            DateTime.Parse(values.tanggalLahir),
                            values.jenisKelamin, values.alamat,
                            values.pekerjaan, values.status);

                        spinner.Destroy();

                        if (result)
                        {
                            ShowMessage("Data berhasil disimpan", MessageType.Info);
                            await RefreshDataAsync();
                        }
                        else
                        {
                            ShowMessage("Gagal menyimpan data", MessageType.Error);
                        }
                    }
                }
                dialog.Destroy();
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Add error: {ex.Message}");
                ShowMessage($"Error: {ex.Message}", MessageType.Error);
            }
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

            if (Regex.IsMatch(data.nama, @"\d"))
            {
                ShowMessage("Nama tidak boleh mengandung angka", MessageType.Error);
                return false;
            }

            if (!DateTime.TryParse(data.tanggalLahir, out _))
            {
                ShowMessage("Format tanggal tidak valid (gunakan yyyy-MM-dd)", MessageType.Error);
                return false;
            }

            return true;
        }

        private async Task RefreshDataAsync()
        {
            try
            {
                var dt = await db.GetAllWargaAsync();

                Application.Invoke(delegate
                {
                    listStore.Clear();
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
                });
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Refresh error: {ex.Message}");
                ShowMessage($"Error: {ex.Message}", MessageType.Error);
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
