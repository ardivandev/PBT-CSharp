using System;
using System.IO;
using System.Data;
using Microsoft.Data.Sqlite;

namespace AplikasiPencatatanWarga
{
    public class DatabaseManager
    {
        private string dbPath;

        public DatabaseManager()
        {
            string appDirectory = AppDomain.CurrentDomain.BaseDirectory;
            string dataFolder = Path.Combine(appDirectory, "Data");

            if (!Directory.Exists(dataFolder))
            {
                Directory.CreateDirectory(dataFolder);
            }

            dbPath = Path.Combine(dataFolder, "warga.db");

            InitializeDatabase();
        }

        public SqliteConnection GetConnection()
        {
            return new SqliteConnection($"Data Source={dbPath}");
        }

        private void InitializeDatabase()
        {
            using (var connection = GetConnection())
            {
                try
                {
                    connection.Open();
                    string createTableQuery = @"
                        CREATE TABLE IF NOT EXISTS Warga (
                            nik TEXT PRIMARY KEY UNIQUE NOT NULL,
                            namalengkap TEXT NOT NULL,
                            tanggallahir TEXT,
                            jeniskelamin TEXT NOT NULL,
                            alamat TEXT,
                            pekerjaan TEXT,
                            statusperkawinan TEXT
                        );";
                    using (var command = new SqliteCommand(createTableQuery, connection))
                    {
                        command.ExecuteNonQuery();
                    }
                    Console.WriteLine("Database berhasil diinisialisasi dan tabel 'Warga' siap digunakan.");
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"Error saat inisialisasi database: {ex.Message}");
                }
            }
        }

        public bool SaveWarga(string nik, string namaLengkap, DateTime tanggalLahir, string jenisKelamin,
                              string alamat, string pekerjaan, string statusPerkawinan)
        {
            using (var conn = GetConnection())
            {
                try
                {
                    conn.Open();
                    string query = @"
                        INSERT INTO Warga (nik, namalengkap, tanggallahir, jeniskelamin, alamat, pekerjaan, statusperkawinan)
                        VALUES (@nik, @namalengkap, @tanggallahir, @jeniskelamin, @alamat, @pekerjaan, @statusperkawinan)
                        ON CONFLICT(nik) DO UPDATE SET
                            namalengkap = excluded.namalengkap,
                            tanggallahir = excluded.tanggallahir,
                            jeniskelamin = excluded.jeniskelamin,
                            alamat = excluded.alamat,
                            pekerjaan = excluded.pekerjaan,
                            statusperkawinan = excluded.statusperkawinan;
                    ";

                    using (var cmd = new SqliteCommand(query, conn))
                    {
                        cmd.Parameters.AddWithValue("@nik", nik);
                        cmd.Parameters.AddWithValue("@namalengkap", namaLengkap);
                        cmd.Parameters.AddWithValue("@tanggallahir", tanggalLahir.ToString("yyyy-MM-dd"));
                        cmd.Parameters.AddWithValue("@jeniskelamin", jenisKelamin);
                        cmd.Parameters.AddWithValue("@alamat", alamat);
                        cmd.Parameters.AddWithValue("@pekerjaan", pekerjaan);
                        cmd.Parameters.AddWithValue("@statusperkawinan", statusPerkawinan);
                        cmd.ExecuteNonQuery();
                    }
                    return true;
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"Error saat menyimpan data warga: {ex.Message}");
                    return false;
                }
            }
        }

        public DataTable GetAllWarga()
        {
            var dt = new DataTable();
            using (var conn = GetConnection())
            {
                try
                {
                    conn.Open();
                    string query = "SELECT * FROM Warga ORDER BY namalengkap ASC;";
                    using (var cmd = new SqliteCommand(query, conn))
                    using (var reader = cmd.ExecuteReader())
                    {
                        dt.Load(reader); // Load langsung dari reader
                    }
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"Error saat mengambil data warga: {ex.Message}");
                }
            }
            return dt;
        }

        public bool DeleteWarga(string nik)
        {
            using (var conn = GetConnection())
            {
                try
                {
                    conn.Open();
                    string query = "DELETE FROM Warga WHERE nik = @nik;";
                    using (var cmd = new SqliteCommand(query, conn))
                    {
                        cmd.Parameters.AddWithValue("@nik", nik);
                        int affected = cmd.ExecuteNonQuery();
                        return affected > 0;
                    }
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"Error saat menghapus data warga: {ex.Message}");
                    return false;
                }
            }
        }

        public DataRow? GetWargaByNIK(string nik)
        {
            var dt = new DataTable();
            using (var conn = GetConnection())
            {
                try
                {
                    conn.Open();
                    string query = "SELECT * FROM Warga WHERE nik = @nik;";
                    using (var cmd = new SqliteCommand(query, conn))
                    {
                        cmd.Parameters.AddWithValue("@nik", nik);
                        using (var reader = cmd.ExecuteReader())
                        {
                            dt.Load(reader);
                        }
                    }
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"Error saat mengambil data berdasarkan NIK: {ex.Message}");
                }
            }
            return dt.Rows.Count > 0 ? dt.Rows[0] : null;
        }

        public bool IsNikExist(string nik)
        {
            using (var conn = GetConnection())
            {
                try
                {
                    conn.Open();
                    string query = "SELECT COUNT(*) FROM Warga WHERE nik = @nik;";
                    using (var cmd = new SqliteCommand(query, conn))
                    {
                        cmd.Parameters.AddWithValue("@nik", nik);
                        object? result = cmd.ExecuteScalar();
                        long count = result != null ? Convert.ToInt64(result) : 0;
                        return count > 0;
                    }
                }
                catch
                {
                    return false;
                }
            }
        }
    }
}
