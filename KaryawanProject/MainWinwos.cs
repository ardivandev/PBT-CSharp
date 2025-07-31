using MySql.Data.MySqlClient;
using System.Data;
using Gtk;
// using Gdk;
// using System.IO; // Tambahkan ini untuk Path.GetFileName

public class MainWindow : Window
{
    Entry textNik, textNama, textSearch;
    Calendar calendarTanggalLahir; // Ganti Entry ke Calendar
    RadioButton radioLaki, radioPerempuan;
    ComboBoxText comboAgama;
    Button buttonSubmit, buttonEdit, buttonDelete, buttonSearch, buttonPilihGambar;
    TreeView dataTampil;
    Image imagePreview;
    string gender = "";
    string pathGambar = "";

    MySqlConnection? connection;
    MySqlCommand? command;
    MySqlDataAdapter? dataAdapter;
    DataSet? dataset;

    string selectedNik = "";

    public MainWindow() : base("CRUD Karyawan - GtkSharp")
    {
        DefaultWidth = 800;
        DefaultHeight = 600;
        WindowPosition = WindowPosition.Center;

        DeleteEvent += (o, args) => Application.Quit();

        Box vbox = new Box(Orientation.Vertical, 10) { BorderWidth = 15 };

        // Form Grid
        Grid formGrid = new Grid
        {
            RowSpacing = 8,
            ColumnSpacing = 10,
            ColumnHomogeneous = false,
            RowHomogeneous = false
        };

        formGrid.Attach(new Label("NIK:"), 0, 0, 1, 1);
        textNik = new Entry() { PlaceholderText = "NIK" };
        formGrid.Attach(textNik, 1, 0, 2, 1);

        formGrid.Attach(new Label("Nama:"), 0, 1, 1, 1);
        textNama = new Entry() { PlaceholderText = "Nama" };
        formGrid.Attach(textNama, 1, 1, 2, 1);

        formGrid.Attach(new Label("Tanggal Lahir:"), 0, 2, 1, 1);
        calendarTanggalLahir = new Calendar();
        formGrid.Attach(calendarTanggalLahir, 1, 2, 2, 1);

        formGrid.Attach(new Label("Jenis Kelamin:"), 0, 3, 1, 1);
        Box genderBox = new Box(Orientation.Horizontal, 5);
        radioLaki = new RadioButton("Laki-laki");
        radioPerempuan = new RadioButton(radioLaki, "Perempuan");
        radioLaki.Active = false;         // Tidak aktif saat awal
        radioPerempuan.Active = false;    // Tidak aktif saat awal
        gender = "";                      // Gender kosong saat awal

        radioLaki.Toggled += (s, e) => { if (radioLaki.Active) gender = "Laki-laki"; };
        radioPerempuan.Toggled += (s, e) => { if (radioPerempuan.Active) gender = "Perempuan"; };
        genderBox.PackStart(radioLaki, false, false, 0);
        genderBox.PackStart(radioPerempuan, false, false, 0);
        formGrid.Attach(genderBox, 1, 3, 2, 1);

        formGrid.Attach(new Label("Agama:"), 0, 4, 1, 1);
        comboAgama = new ComboBoxText();
        comboAgama.AppendText("Islam");
        comboAgama.AppendText("Kristen");
        comboAgama.AppendText("Hindu");
        comboAgama.AppendText("Budha");
        comboAgama.Active = 0;
        formGrid.Attach(comboAgama, 1, 4, 2, 1);

        formGrid.Attach(new Label("Foto:"), 0, 5, 1, 1);
        Box fotoBox = new Box(Orientation.Horizontal, 5);
        buttonPilihGambar = new Button("Pilih Gambar");
        // Atur ukuran tombol pilih gambar agar kecil
        buttonPilihGambar.SetSizeRequest(90, 30); // Lebar 90px, tinggi 30px
        imagePreview = new Image();
        imagePreview.SetSizeRequest(200, 200);
        fotoBox.PackStart(buttonPilihGambar, false, false, 0);
        fotoBox.PackStart(imagePreview, false, false, 10);
        formGrid.Attach(fotoBox, 1, 5, 2, 1);

        buttonPilihGambar.Clicked += OnPilihGambar;

        vbox.PackStart(formGrid, false, false, 0);

        // Tombol Bar
        Box tombolBar = new Box(Orientation.Horizontal, 10);
        buttonSubmit = new Button("Tambah");
        buttonEdit = new Button("Edit");
        buttonDelete = new Button("Hapus");
        Button buttonReset = new Button("Reset"); // Tambah tombol reset
        tombolBar.PackStart(buttonSubmit, false, false, 0);
        tombolBar.PackStart(buttonEdit, false, false, 0);
        tombolBar.PackStart(buttonDelete, false, false, 0);
        tombolBar.PackStart(buttonReset, false, false, 0); // Tampilkan tombol reset
        vbox.PackStart(tombolBar, false, false, 10);

        // Search Bar
        Box searchBox = new Box(Orientation.Horizontal, 5);
        textSearch = new Entry() { PlaceholderText = "Cari nama..." };
        buttonSearch = new Button("Cari");
        searchBox.PackStart(textSearch, false, false, 0);
        searchBox.PackStart(buttonSearch, false, false, 0);
        vbox.PackStart(searchBox, false, false, 5);

        buttonSubmit.Clicked += OnSubmit;
        buttonEdit.Clicked += OnEdit;
        buttonDelete.Clicked += OnDelete;
        buttonSearch.Clicked += OnSearch;
        buttonReset.Clicked += (s, e) => ResetForm(); // Event handler tombol reset

        // Data Table
        dataTampil = new TreeView();
        dataTampil.ButtonPressEvent += OnRowClicked;

        ScrolledWindow scroll = new ScrolledWindow();
        scroll.Add(dataTampil);
        scroll.SetPolicy(PolicyType.Automatic, PolicyType.Automatic);
        scroll.SetSizeRequest(760, 220);

        vbox.PackStart(scroll, true, true, 10);

        Add(vbox);
        ShowAll();

        InitDB();
        TampilData("SELECT * FROM tabel_karyawan");
    }

    void InitDB()
    {
        string connStr = "server=localhost;user=root;database=db_karyawan;port=3306;password=";
        connection = new MySqlConnection(connStr);
        try
        {
            connection.Open();
            Console.WriteLine("Koneksi ke database berhasil.");
        }
        catch (Exception ex)
        {
            Console.WriteLine("Koneksi gagal: " + ex.Message);
        }
    }

    void OnSubmit(object? sender, EventArgs e)
    {
        // Validasi input tidak boleh kosong
        if (string.IsNullOrWhiteSpace(textNik.Text) ||
            string.IsNullOrWhiteSpace(textNama.Text) ||
            gender == "" ||
            comboAgama.ActiveText == null || comboAgama.ActiveText == "")
        {
            MessageBox("Input tidak boleh kosong.");
            return;
        }

        // Validasi NIK harus 10 digit
        if (textNik.Text.Length != 10)
        {
            MessageBox("NIK harus 10 karakter.");
            return;
        }

        // Validasi nama tidak boleh mengandung angka
        if (System.Text.RegularExpressions.Regex.IsMatch(textNama.Text, @"\d"))
        {
            MessageBox("Nama tidak boleh mengandung angka.");
            return;
        }

        // Validasi NIK sudah ada
        string cekNikSql = "SELECT COUNT(*) FROM tabel_karyawan WHERE nik=@nik";
        using (var cekNikCmd = new MySqlCommand(cekNikSql, connection))
        {
            cekNikCmd.Parameters.AddWithValue("@nik", textNik.Text);
            long count = (long)cekNikCmd.ExecuteScalar();
            if (count > 0)
            {
                MessageBox("NIK sudah ada.");
                return;
            }
        }

        try
        {
            string sql = "INSERT INTO tabel_karyawan (nik, nama, tanggal_lahir, jenis_kelamin, agama, foto) VALUES (@nik, @nama, @tgl, @jk, @agama, @foto)";
            command = new MySqlCommand(sql, connection);
            command.Parameters.AddWithValue("@nik", textNik.Text);
            command.Parameters.AddWithValue("@nama", textNama.Text);

            // Ambil tanggal dari Calendar
            DateTime tgl = new DateTime(calendarTanggalLahir.Year, calendarTanggalLahir.Month + 1, calendarTanggalLahir.Day);
            command.Parameters.AddWithValue("@tgl", tgl.ToString("yyyy-MM-dd"));

            command.Parameters.AddWithValue("@jk", gender);
            command.Parameters.AddWithValue("@agama", comboAgama.ActiveText);
            command.Parameters.AddWithValue("@foto", pathGambar);
            command.ExecuteNonQuery();

            MessageBox("Data berhasil ditambahkan!");
            TampilData("SELECT * FROM tabel_karyawan");
            ResetForm();
        }
        catch (Exception ex)
        {
            MessageBox("Gagal: " + ex.Message);
        }
    }

    void OnEdit(object? sender, EventArgs e)
    {
        if (string.IsNullOrEmpty(selectedNik))
        {
            MessageBox("Pilih data yang ingin diedit.");
            return;
        }

        // Validasi NIK harus 10 digit
        if (textNik.Text.Length != 10)
        {
            MessageBox("NIK harus 10 karakter.");
            return;
        }

        // Validasi nama tidak boleh mengandung angka
        if (System.Text.RegularExpressions.Regex.IsMatch(textNama.Text, @"\d"))
        {
            MessageBox("Nama tidak boleh mengandung angka.");
            return;
        }

        string sql = "UPDATE tabel_karyawan SET nama=@nama, tanggal_lahir=@tgl, jenis_kelamin=@jk, agama=@agama, foto=@foto WHERE nik=@nik";
        command = new MySqlCommand(sql, connection);
        command.Parameters.AddWithValue("@nama", textNama.Text);

        // Ambil tanggal dari Calendar
        DateTime tgl = new DateTime(calendarTanggalLahir.Year, calendarTanggalLahir.Month + 1, calendarTanggalLahir.Day);
        command.Parameters.AddWithValue("@tgl", tgl.ToString("yyyy-MM-dd"));

        command.Parameters.AddWithValue("@jk", gender);
        command.Parameters.AddWithValue("@agama", comboAgama.ActiveText);
        command.Parameters.AddWithValue("@foto", pathGambar);
        command.Parameters.AddWithValue("@nik", selectedNik);
        command.ExecuteNonQuery();

        MessageBox("Data berhasil diperbarui!");
        TampilData("SELECT * FROM tabel_karyawan");
        ResetForm();
    }

    void OnDelete(object? sender, EventArgs e)
    {
        if (string.IsNullOrEmpty(selectedNik))
        {
            MessageBox("Pilih data yang ingin dihapus.");
            return;
        }

        string sql = "DELETE FROM tabel_karyawan WHERE nik=@nik";
        command = new MySqlCommand(sql, connection);
        command.Parameters.AddWithValue("@nik", selectedNik);
        command.ExecuteNonQuery();

        MessageBox("Data berhasil dihapus!");
        TampilData("SELECT * FROM tabel_karyawan");
        ResetForm();
    }

    void OnSearch(object? sender, EventArgs e)
    {
        string keyword = textSearch.Text;
        string sql = "SELECT * FROM tabel_karyawan WHERE nama LIKE @keyword";
        dataAdapter = new MySqlDataAdapter(sql, connection);
        dataAdapter.SelectCommand.Parameters.AddWithValue("@keyword", "%" + keyword + "%");
        dataset = new DataSet();
        dataAdapter.Fill(dataset, "tabel_karyawan");
        UpdateTreeView(dataset.Tables[0]);
    }

    void TampilData(string sql)
    {
        dataAdapter = new MySqlDataAdapter(sql, connection);
        dataset = new DataSet();
        dataAdapter.Fill(dataset, "tabel_karyawan");
        UpdateTreeView(dataset.Tables[0]);
    }

    void UpdateTreeView(DataTable table)
    {
        while (dataTampil.Columns.Length > 0)
        {
            dataTampil.RemoveColumn(dataTampil.Columns[0]);
        }

        ListStore store = new ListStore(
            typeof(string), typeof(string), typeof(string),
            typeof(string), typeof(string), typeof(string)
        );

        foreach (DataRow row in table.Rows)
        {
            // Tampilkan hanya nama file untuk kolom foto
            string foto = row["foto"]?.ToString() ?? "";
            string namaFile = string.IsNullOrEmpty(foto) ? "" : System.IO.Path.GetFileName(foto);

            store.AppendValues(
                row["nik"].ToString(),
                row["nama"].ToString(),
                row["tanggal_lahir"].ToString(),
                row["jenis_kelamin"].ToString(),
                row["agama"].ToString(),
                namaFile // hanya nama file
            );
        }

        for (int i = 0; i < table.Columns.Count; i++)
        {
            dataTampil.AppendColumn(table.Columns[i].ColumnName, new CellRendererText(), "text", i);
        }

        dataTampil.Model = store;
    }

    void OnRowClicked(object o, ButtonPressEventArgs args)
    {
        TreeSelection selection = dataTampil.Selection;
        if (selection.GetSelected(out TreeIter iter))
        {
            var model = dataTampil.Model;
            if (model == null) return;

            selectedNik = (string)model.GetValue(iter, 0);
            textNik.Text = (string)model.GetValue(iter, 0);
            textNama.Text = (string)model.GetValue(iter, 1);

            // Set tanggal ke Calendar
            if (DateTime.TryParse((string)model.GetValue(iter, 2), out DateTime tgl))
            {
                calendarTanggalLahir.Date = tgl;
            }

            string jk = (string)model.GetValue(iter, 3);
            gender = jk;
            radioLaki.Active = (jk == "Laki-laki");
            radioPerempuan.Active = (jk == "Perempuan");

            string agama = (string)model.GetValue(iter, 4);
            for (int i = 0; i < comboAgama.Model.IterNChildren(); i++)
            {
                comboAgama.Model.IterNthChild(out TreeIter agamaIter, i);
                if (comboAgama.Model.GetValue(agamaIter, 0).ToString() == agama)
                {
                    comboAgama.Active = i;
                    break;
                }
            }

            // Ambil path asli dari database, bukan dari tampilan
            string foto = "";
            if (dataset != null && dataset.Tables.Count > 0)
            {
                foreach (DataRow row in dataset.Tables[0].Rows)
                {
                    if (row["nik"].ToString() == selectedNik)
                    {
                        foto = row["foto"]?.ToString() ?? "";
                        break;
                    }
                }
            }
            pathGambar = foto;
            if (!string.IsNullOrEmpty(pathGambar) && File.Exists(pathGambar))
            {
                imagePreview.Pixbuf = new Gdk.Pixbuf(pathGambar).ScaleSimple(200, 200, Gdk.InterpType.Bilinear);
            }
            else
            {
                imagePreview.Clear();
            }
        }
    }

    void OnPilihGambar(object? sender, EventArgs e)
    {
        FileChooserDialog fileChooser = new FileChooserDialog(
            "Pilih Gambar", this, FileChooserAction.Open,
            "Batal", ResponseType.Cancel,
            "Pilih", ResponseType.Accept
        );

        FileFilter filter = new FileFilter();
        filter.Name = "Gambar";
        filter.AddPattern("*.png");
        filter.AddPattern("*.jpg");
        filter.AddPattern("*.jpeg");
        fileChooser.Filter = filter;

        if (fileChooser.Run() == (int)ResponseType.Accept)
        {
            pathGambar = fileChooser.Filename;
            imagePreview.Pixbuf = new Gdk.Pixbuf(pathGambar).ScaleSimple(200, 200, Gdk.InterpType.Bilinear);
        }

        fileChooser.Destroy();
    }

    void ResetForm()
    {
        textNik.Text = "";
        textNama.Text = "";
        // Reset calendar ke hari ini
        calendarTanggalLahir.Date = DateTime.Today;
        gender = "";
        selectedNik = "";
        radioLaki.Active = false;         // Tidak aktif saat reset
        radioPerempuan.Active = false;    // Tidak aktif saat reset
        comboAgama.Active = 0;
        pathGambar = "";
        imagePreview.Clear();
    }

    void MessageBox(string pesan)
    {
        MessageDialog msg = new MessageDialog(this, DialogFlags.Modal, MessageType.Info, ButtonsType.Ok, pesan);
        msg.Run();
        msg.Destroy();
    }
}
