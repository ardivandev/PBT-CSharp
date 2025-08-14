using System;
using System.Data;
using System.IO;
using System.Collections.Generic;
using Microsoft.Data.Sqlite;

public class DatabaseManager
{
  private string dbPath = "data/dbwarga.db";

  public DatabaseManager()
  {
    InitializeDatabase();
  }

  private SqliteConnection GetConnection()
  {
    return new SqliteConnection($"Data Source={dbPath}");
  }

  private void InitializeDatabase()
  {
    if (!Directory.Exists("data"))
      Directory.CreateDirectory("data");

    using (var conn = GetConnection())
    {
      conn.Open();

      var createWarga = @"
            CREATE TABLE IF NOT EXISTS Warga (
                NIK TEXT PRIMARY KEY,
                Nama TEXT,
                TanggalLahir TEXT,
                JenisKelamin TEXT,
                Alamat TEXT,
                Pekerjaan TEXT,
                StatusPerkawinan TEXT
            );";

      var createKegiatan = @"
            CREATE TABLE IF NOT EXISTS KegiatanRutin (
                IdKegiatan INTEGER PRIMARY KEY AUTOINCREMENT,
                NIK_Warga TEXT,
                NamaKegiatan TEXT,
                TanggalKegiatan TEXT,
                Keterangan TEXT,
                FOREIGN KEY(NIK_Warga) REFERENCES Warga(NIK) ON DELETE CASCADE
            );";

      var createIuran = @"
            CREATE TABLE IF NOT EXISTS IuranRutin (
                IdIuran INTEGER PRIMARY KEY AUTOINCREMENT,
                NIK_Warga TEXT NOT NULL,
                TanggalIuran TEXT NOT NULL,
                Jumlah INTEGER NOT NULL,
                Keterangan TEXT,
                FOREIGN KEY(NIK_Warga) REFERENCES Warga(NIK) ON DELETE CASCADE
            );";

      var createKas = @"
        CREATE TABLE IF NOT EXISTS KasWarga (
            IdKas INTEGER PRIMARY KEY AUTOINCREMENT,
            NIK_Warga TEXT NOT NULL,
            Tanggal TEXT NOT NULL,
            Jumlah INTEGER NOT NULL,
            Keterangan TEXT,
            FOREIGN KEY(NIK_Warga) REFERENCES Warga(NIK) ON DELETE CASCADE
        );";


      using var cmd = conn.CreateCommand();
      cmd.CommandText = createWarga;
      cmd.ExecuteNonQuery();
      cmd.CommandText = createKegiatan;
      cmd.ExecuteNonQuery();
      cmd.CommandText = createIuran;
      cmd.ExecuteNonQuery();
      cmd.CommandText = createKas;
      cmd.ExecuteNonQuery();
    }
  }

  // -------------------- WARGA --------------------
  public void InsertWarga(string nik, string nama, string tanggal, string jk, string alamat, string pekerjaan, string status)
  {
    using var conn = GetConnection();
    conn.Open();

    var query = @"INSERT INTO Warga VALUES (@nik, @nama, @tanggal, @jk, @alamat, @pekerjaan, @status);";
    using var cmd = new SqliteCommand(query, conn);
    cmd.Parameters.AddWithValue("@nik", nik);
    cmd.Parameters.AddWithValue("@nama", nama);
    cmd.Parameters.AddWithValue("@tanggal", tanggal);
    cmd.Parameters.AddWithValue("@jk", jk);
    cmd.Parameters.AddWithValue("@alamat", alamat);
    cmd.Parameters.AddWithValue("@pekerjaan", pekerjaan);
    cmd.Parameters.AddWithValue("@status", status);
    cmd.ExecuteNonQuery();
  }

  public void UpdateWarga(string nik, string nama, string tanggal, string jk, string alamat, string pekerjaan, string status)
  {
    using var conn = GetConnection();
    conn.Open();

    var query = @"UPDATE Warga SET Nama=@nama, TanggalLahir=@tanggal, JenisKelamin=@jk,
                      Alamat=@alamat, Pekerjaan=@pekerjaan, StatusPerkawinan=@status WHERE NIK=@nik;";
    using var cmd = new SqliteCommand(query, conn);
    cmd.Parameters.AddWithValue("@nik", nik);
    cmd.Parameters.AddWithValue("@nama", nama);
    cmd.Parameters.AddWithValue("@tanggal", tanggal);
    cmd.Parameters.AddWithValue("@jk", jk);
    cmd.Parameters.AddWithValue("@alamat", alamat);
    cmd.Parameters.AddWithValue("@pekerjaan", pekerjaan);
    cmd.Parameters.AddWithValue("@status", status);
    cmd.ExecuteNonQuery();
  }

  public void DeleteWarga(string nik)
  {
    using var conn = GetConnection();
    conn.Open();

    var query = "DELETE FROM Warga WHERE NIK=@nik;";
    using var cmd = new SqliteCommand(query, conn);
    cmd.Parameters.AddWithValue("@nik", nik);
    cmd.ExecuteNonQuery();
  }

  public DataTable GetAllWarga()
  {
    var dt = new DataTable();
    using var conn = GetConnection();
    conn.Open();

    var query = "SELECT * FROM Warga ORDER BY Nama ASC;";
    using var cmd = new SqliteCommand(query, conn);
    using var reader = cmd.ExecuteReader();
    dt.Load(reader);
    return dt;
  }

  public List<Tuple<string, string>> GetWargaForComboBox()
  {
    var list = new List<Tuple<string, string>>();
    using var conn = GetConnection();
    conn.Open();

    var query = "SELECT NIK, Nama FROM Warga;";
    using var cmd = new SqliteCommand(query, conn);
    using var reader = cmd.ExecuteReader();
    while (reader.Read())
    {
      string nik = reader.GetString(0);
      string nama = reader.GetString(1);
      list.Add(Tuple.Create(nik, nama));
    }
    return list;
  }

  // -------------------- IURAN --------------------
  public bool SaveIuran(int idIuran, string nikWarga, DateTime tanggalIuran, int jumlah, string keterangan)
  {
    using var conn = GetConnection();
    conn.Open();

    string query;
    if (idIuran == 0)
    {
      query = @"INSERT INTO IuranRutin (NIK_Warga, TanggalIuran, Jumlah, Keterangan)
                      VALUES (@nikWarga, @tanggalIuran, @jumlah, @keterangan);";
    }
    else
    {
      query = @"UPDATE IuranRutin SET NIK_Warga=@nikWarga, TanggalIuran=@tanggalIuran,
                      Jumlah=@jumlah, Keterangan=@keterangan WHERE IdIuran=@idIuran;";
    }

    using var cmd = new SqliteCommand(query, conn);
    if (idIuran != 0)
      cmd.Parameters.AddWithValue("@idIuran", idIuran);

    cmd.Parameters.AddWithValue("@nikWarga", nikWarga);
    cmd.Parameters.AddWithValue("@tanggalIuran", tanggalIuran.ToString("yyyy-MM-dd"));
    cmd.Parameters.AddWithValue("@jumlah", jumlah);
    cmd.Parameters.AddWithValue("@keterangan", keterangan);

    cmd.ExecuteNonQuery();
    return true;
  }

  public DataTable GetAllIuran()
  {
    var dt = new DataTable();
    using var conn = GetConnection();
    conn.Open();

    var query = @"
            SELECT i.IdIuran, i.NIK_Warga, w.Nama as Nama, i.TanggalIuran, i.Jumlah, i.Keterangan
            FROM IuranRutin i
            JOIN Warga w ON i.NIK_Warga = w.NIK
            ORDER BY i.TanggalIuran DESC;";

    using var cmd = new SqliteCommand(query, conn);
    using var reader = cmd.ExecuteReader();
    dt.Load(reader);
    return dt;
  }

  public bool DeleteIuran(int idIuran)
  {
    using var conn = GetConnection();
    conn.Open();

    var query = "DELETE FROM IuranRutin WHERE IdIuran = @idIuran;";
    using var cmd = new SqliteCommand(query, conn);
    cmd.Parameters.AddWithValue("@idIuran", idIuran);
    return cmd.ExecuteNonQuery() > 0;
  }

  // -------------------- KEGIATAN --------------------
  public DataTable GetAllKegiatan()
  {
    var dt = new DataTable();
    using var conn = GetConnection();
    conn.Open();

    var query = @"
            SELECT k.IdKegiatan, k.NIK_Warga, w.Nama as Nama, k.NamaKegiatan, k.TanggalKegiatan, k.Keterangan
            FROM KegiatanRutin k
            JOIN Warga w ON k.NIK_Warga = w.NIK
            ORDER BY k.TanggalKegiatan DESC;";

    using var cmd = new SqliteCommand(query, conn);
    using var reader = cmd.ExecuteReader();
    dt.Load(reader);
    return dt;
  }

  public void InsertKegiatan(string nik, string namaKegiatan, DateTime tanggal, string keterangan)
  {
    using var conn = GetConnection();
    conn.Open();

    var query = @"INSERT INTO KegiatanRutin (NIK_Warga, NamaKegiatan, TanggalKegiatan, Keterangan)
                      VALUES (@nik, @namaKegiatan, @tanggal, @keterangan);";
    using var cmd = new SqliteCommand(query, conn);
    cmd.Parameters.AddWithValue("@nik", nik);
    cmd.Parameters.AddWithValue("@namaKegiatan", namaKegiatan);
    cmd.Parameters.AddWithValue("@tanggal", tanggal.ToString("yyyy-MM-dd"));
    cmd.Parameters.AddWithValue("@keterangan", keterangan);
    cmd.ExecuteNonQuery();
  }

  public void UpdateKegiatan(int id, string nik, string namaKegiatan, DateTime tanggal, string keterangan)
  {
    using var conn = GetConnection();
    conn.Open();

    var query = @"UPDATE KegiatanRutin SET NIK_Warga=@nik, NamaKegiatan=@namaKegiatan,
                      TanggalKegiatan=@tanggal, Keterangan=@keterangan WHERE IdKegiatan=@id;";
    using var cmd = new SqliteCommand(query, conn);
    cmd.Parameters.AddWithValue("@id", id);
    cmd.Parameters.AddWithValue("@nik", nik);
    cmd.Parameters.AddWithValue("@namaKegiatan", namaKegiatan);
    cmd.Parameters.AddWithValue("@tanggal", tanggal.ToString("yyyy-MM-dd"));
    cmd.Parameters.AddWithValue("@keterangan", keterangan);
    cmd.ExecuteNonQuery();
  }

  public void DeleteKegiatan(int id)
  {
    using var conn = GetConnection();
    conn.Open();

    var query = "DELETE FROM KegiatanRutin WHERE IdKegiatan=@id;";
    using var cmd = new SqliteCommand(query, conn);
    cmd.Parameters.AddWithValue("@id", id);
    cmd.ExecuteNonQuery();
  }

  // -------------------- KAS WARGA --------------------
  // -------------------- KAS WARGA --------------------
  public bool SaveKas(int idKas, string nikWarga, DateTime tanggal, int jumlah, string keterangan)
  {
    using var conn = GetConnection();
    conn.Open();

    string query;
    if (idKas == 0)
    {
      query = @"INSERT INTO KasWarga (NIK_Warga, Tanggal, Jumlah, Keterangan)
                  VALUES (@nikWarga, @tanggal, @jumlah, @keterangan);";
    }
    else
    {
      query = @"UPDATE KasWarga SET NIK_Warga=@nikWarga, Tanggal=@tanggal,
                  Jumlah=@jumlah, Keterangan=@keterangan WHERE IdKas=@idKas;";
    }

    using var cmd = new SqliteCommand(query, conn);
    if (idKas != 0)
      cmd.Parameters.AddWithValue("@idKas", idKas);

    cmd.Parameters.AddWithValue("@nikWarga", nikWarga);
    cmd.Parameters.AddWithValue("@tanggal", tanggal.ToString("yyyy-MM-dd"));
    cmd.Parameters.AddWithValue("@jumlah", jumlah);
    cmd.Parameters.AddWithValue("@keterangan", keterangan);

    cmd.ExecuteNonQuery();
    return true;
  }

  public DataTable GetAllKas()
  {
    var dt = new DataTable();
    using var conn = GetConnection();
    conn.Open();

    var query = @"
        SELECT k.IdKas, k.NIK_Warga, w.Nama, k.Tanggal, k.Jumlah, k.Keterangan
        FROM KasWarga k
        JOIN Warga w ON k.NIK_Warga = w.NIK
        ORDER BY k.Tanggal DESC;";

    using var cmd = new SqliteCommand(query, conn);
    using var reader = cmd.ExecuteReader();
    dt.Load(reader);
    return dt;
  }

  public bool DeleteKas(int idKas)
  {
    using var conn = GetConnection();
    conn.Open();

    var query = "DELETE FROM KasWarga WHERE IdKas = @idKas;";
    using var cmd = new SqliteCommand(query, conn);
    cmd.Parameters.AddWithValue("@idKas", idKas);
    return cmd.ExecuteNonQuery() > 0;
  }

 // -------------------- LAPORAN --------------------
    public int GetTotalKasPerWarga(string nikWarga)
    {
        using var conn = GetConnection();
        conn.Open();
        var query = "SELECT COALESCE(SUM(Jumlah), 0) FROM KasWarga WHERE NIK_Warga = @nik;";
        using var cmd = new SqliteCommand(query, conn);
        cmd.Parameters.AddWithValue("@nik", nikWarga);
        return Convert.ToInt32(cmd.ExecuteScalar());
    }

    public int GetTotalIuranPerWarga(string nikWarga)
    {
        using var conn = GetConnection();
        conn.Open();
        var query = "SELECT COALESCE(SUM(Jumlah), 0) FROM IuranRutin WHERE NIK_Warga = @nik;";
        using var cmd = new SqliteCommand(query, conn);
        cmd.Parameters.AddWithValue("@nik", nikWarga);
        return Convert.ToInt32(cmd.ExecuteScalar());
    }

    public Dictionary<string, int> GetStatistikWarga()
    {
        var hasil = new Dictionary<string, int>();
        using var conn = GetConnection();
        conn.Open();

        hasil["Kepala Keluarga (Kawin)"] = CountQuery(conn, "SELECT COUNT(*) FROM Warga WHERE StatusPerkawinan = 'Kawin';");
        hasil["Balita (0-4)"] = CountQuery(conn, AgeRangeQuery(0, 4));
        hasil["Anak-anak (5-11)"] = CountQuery(conn, AgeRangeQuery(5, 11));
        hasil["Remaja (12-18)"] = CountQuery(conn, AgeRangeQuery(12, 18));
        hasil["Dewasa Belum Kawin (>18)"] = CountQuery(conn,
            "SELECT COUNT(*) FROM Warga WHERE StatusPerkawinan = 'Belum Kawin' " +
            "AND (CAST(strftime('%Y','now') AS INTEGER) - CAST(strftime('%Y',TanggalLahir) AS INTEGER)) > 18;");
        hasil["Duda/Janda"] = CountQuery(conn, "SELECT COUNT(*) FROM Warga WHERE StatusPerkawinan IN ('Cerai Hidup','Cerai Mati');");

        return hasil;
    }

    private string AgeRangeQuery(int min, int max)
    {
        return $"SELECT COUNT(*) FROM Warga WHERE (CAST(strftime('%Y','now') AS INTEGER) - CAST(strftime('%Y',TanggalLahir) AS INTEGER)) BETWEEN {min} AND {max};";
    }

    private int CountQuery(SqliteConnection conn, string query)
    {
        using var cmd = new SqliteCommand(query, conn);
        return Convert.ToInt32(cmd.ExecuteScalar());
    }
}
