using System;
using System.IO;
using System.Data;
using System.Threading.Tasks;
using Microsoft.Data.Sqlite;

namespace AplikasiPencatatanWarga
{
    public class DatabaseManager
    {
        private readonly string dbPath;

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

        private SqliteConnection GetConnection()
        {
            return new SqliteConnection($"Data Source={dbPath}");
        }

        private void InitializeDatabase()
        {
            using (var connection = GetConnection())
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
                    );

                    CREATE INDEX IF NOT EXISTS idx_warga_nik ON Warga(nik);
                    CREATE INDEX IF NOT EXISTS idx_warga_nama ON Warga(namalengkap);";

                using (var command = new SqliteCommand(createTableQuery, connection))
                {
                    command.ExecuteNonQuery();
                }
            }
        }

        public async Task<bool> SaveWargaAsync(string nik, string namaLengkap, DateTime tanggalLahir,
                                              string jenisKelamin, string alamat, string pekerjaan,
                                              string statusPerkawinan)
        {
            using (var conn = GetConnection())
            {
                try
                {
                    await conn.OpenAsync();
                    using (var transaction = await conn.BeginTransactionAsync())
                    {
                        string query = @"
                            INSERT INTO Warga
                            VALUES (@nik, @namalengkap, @tanggallahir, @jeniskelamin, @alamat, @pekerjaan, @statusperkawinan)
                            ON CONFLICT(nik) DO UPDATE SET
                                namalengkap = excluded.namalengkap,
                                tanggallahir = excluded.tanggallahir,
                                jeniskelamin = excluded.jeniskelamin,
                                alamat = excluded.alamat,
                                pekerjaan = excluded.pekerjaan,
                                statusperkawinan = excluded.statusperkawinan;
                        ";

                        using (var cmd = new SqliteCommand(query, conn, transaction))
                        {
                            cmd.Parameters.AddWithValue("@nik", nik);
                            cmd.Parameters.AddWithValue("@namalengkap", namaLengkap);
                            cmd.Parameters.AddWithValue("@tanggallahir", tanggalLahir.ToString("yyyy-MM-dd"));
                            cmd.Parameters.AddWithValue("@jeniskelamin", jenisKelamin);
                            cmd.Parameters.AddWithValue("@alamat", alamat);
                            cmd.Parameters.AddWithValue("@pekerjaan", pekerjaan);
                            cmd.Parameters.AddWithValue("@statusperkawinan", statusPerkawinan);

                            await cmd.ExecuteNonQueryAsync();
                        }
                        await transaction.CommitAsync();
                        return true;
                    }
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"Error saving data: {ex.Message}");
                    return false;
                }
            }
        }

        public async Task<DataTable> GetAllWargaAsync()
        {
            var dt = new DataTable();
            using (var conn = GetConnection())
            {
                try
                {
                    await conn.OpenAsync();
                    string query = "SELECT * FROM Warga ORDER BY namalengkap ASC;";
                    using (var cmd = new SqliteCommand(query, conn))
                    using (var reader = await cmd.ExecuteReaderAsync())
                    {
                        dt.Load(reader);
                    }
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"Error getting data: {ex.Message}");
                }
            }
            return dt;
        }

        public async Task<bool> DeleteWargaAsync(string nik)
        {
            using (var conn = GetConnection())
            {
                try
                {
                    await conn.OpenAsync();
                    string query = "DELETE FROM Warga WHERE nik = @nik;";
                    using (var cmd = new SqliteCommand(query, conn))
                    {
                        cmd.Parameters.AddWithValue("@nik", nik);
                        int affected = await cmd.ExecuteNonQueryAsync();
                        return affected > 0;
                    }
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"Error deleting data: {ex.Message}");
                    return false;
                }
            }
        }

        public async Task<DataRow?> GetWargaByNIKAsync(string nik)
        {
            var dt = new DataTable();
            using (var conn = GetConnection())
            {
                try
                {
                    await conn.OpenAsync();
                    string query = "SELECT * FROM Warga WHERE nik = @nik;";
                    using (var cmd = new SqliteCommand(query, conn))
                    {
                        cmd.Parameters.AddWithValue("@nik", nik);
                        using (var reader = await cmd.ExecuteReaderAsync())
                        {
                            dt.Load(reader);
                        }
                    }
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"Error getting data by NIK: {ex.Message}");
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
