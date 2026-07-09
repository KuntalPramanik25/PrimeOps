using Dapper;
using DuckDB.NET.Data;
using PrimeOps.DAL.Enums;
using Parquet.Serialization;


namespace PrimeOps.DAL
{
    public class DbContext
    {
        private const string ConnectionString = "Data Source=:memory:";
        private readonly string _folderPath = @"C:\ParquetFiles";

        public DuckDBConnection GetConnection()
        {
            var connection = new DuckDBConnection(ConnectionString);
            connection.Open();
            return connection;
        }

        public string GetFilePath(TableName table) => Path.Combine(_folderPath, $"{table}.parquet");

        public void EnsureFileExists(string file)
        {
            if (!File.Exists(file))
                throw new FileNotFoundException(file);
        }

        public string Escape(string path) => path.Replace("'", "''");


        public void LoadIntoTempTable(DuckDBConnection conn, string file, string columnDefinitionsSql)
        {
            using var cmd = conn.CreateCommand();
            cmd.CommandText = File.Exists(file)
                ? $"CREATE TABLE tmp AS SELECT * FROM read_parquet('{Escape(file)}')"
                : $"CREATE TABLE tmp ({columnDefinitionsSql})";
            cmd.ExecuteNonQuery();
        }

        public void FlushTempTableToFile(DuckDBConnection conn, string file)
        {
            string tempFile = file + ".tmp";
            if (File.Exists(tempFile)) File.Delete(tempFile);

            using (var cmd = conn.CreateCommand())
            {
                cmd.CommandText = $"COPY tmp TO '{Escape(tempFile)}' (FORMAT PARQUET)";
                cmd.ExecuteNonQuery();
            }

            File.Copy(tempFile, file, overwrite: true);
            File.Delete(tempFile);
        }
    }

    public class Linq
    {
        private readonly DbContext DB = new DbContext();
        
        public IEnumerable<T> Select<T>(TableName table)
        {
            string file = DB.GetFilePath(table);

            DB.EnsureFileExists(file);

            using var conn = DB.GetConnection();
            string sql = $"SELECT * FROM read_parquet('{DB.Escape(file)}')";
            return conn.Query<T>(sql);
        }

        public IEnumerable<T> SelectColumns<T>(TableName table, TItemsMasterColumn[] columns)
        {
            string file = DB.GetFilePath(table);

            DB.EnsureFileExists(file);

            if (columns == null || columns.Length == 0)
                return Enumerable.Empty<T>();

            using var conn = DB.GetConnection();

            // Convert columns array into SQL format
            string selectedColumns = string.Join(", ", columns);
            string sql = $@"SELECT {selectedColumns} FROM read_parquet('{file}')";

            return conn.Query<T>(sql);
        }

        public T? SelectById<T>(TableName table, object id)
        {
            string file = DB.GetFilePath(table);
            DB.EnsureFileExists(file);

            using var conn = DB.GetConnection();
            string sql = $"SELECT * FROM read_parquet('{DB.Escape(file)}') WHERE Id = @Id";
            return conn.QueryFirstOrDefault<T>(sql, new { Id = id });
        }

        public T Insert<T>(TableName table, string[] columns, string columnDefinitionsSql, T entity, Func<T, object?[]> toValues, Action<T, long> setId)
        {
            string file = DB.GetFilePath(table);

            using var conn = DB.GetConnection();
            using var tx = conn.BeginTransaction();

            DB.LoadIntoTempTable(conn, file, columnDefinitionsSql);

            long nextId;
            using (var idCmd = conn.CreateCommand())
            {
                idCmd.CommandText = "SELECT COALESCE(MAX(Id), 0) + 1 FROM tmp";
                nextId = Convert.ToInt64(idCmd.ExecuteScalar());
            }
            setId(entity, nextId);

            using (var insertCmd = conn.CreateCommand())
            {
                string cols = string.Join(", ", columns);
                string placeholders = string.Join(", ", columns.Select(_ => "?"));
                insertCmd.CommandText = $"INSERT INTO tmp ({cols}) VALUES ({placeholders})";

                foreach (var value in toValues(entity))
                    insertCmd.Parameters.Add(new DuckDBParameter(value ?? DBNull.Value));

                insertCmd.ExecuteNonQuery();
            }

            DB.FlushTempTableToFile(conn, file);
            tx.Commit();

            return entity;
        }

        public void Update<T>(TableName table, string[] columns, T entity, long id, Func<T, object?[]> toValues)
        {
            string file = DB.GetFilePath(table);
            DB.EnsureFileExists(file);

            using var conn = DB.GetConnection();
            using var tx = conn.BeginTransaction();

            using (var loadCmd = conn.CreateCommand())
            {
                loadCmd.CommandText = $"CREATE TABLE tmp AS SELECT * FROM read_parquet('{DB.Escape(file)}')";
                loadCmd.ExecuteNonQuery();
            }

            var nonIdColumns = columns.Where(c => c != "Id").ToArray();
            string setClause = string.Join(", ", nonIdColumns.Select(c => $"{c} = ?"));

            int rows;
            using (var updateCmd = conn.CreateCommand())
            {
                updateCmd.CommandText = $"UPDATE tmp SET {setClause} WHERE Id = ?";

                var values = toValues(entity);
                for (int i = 0; i < columns.Length; i++)
                {
                    if (columns[i] == "Id") continue;
                    updateCmd.Parameters.Add(new DuckDBParameter(values[i] ?? DBNull.Value));
                }
                updateCmd.Parameters.Add(new DuckDBParameter(id));

                rows = updateCmd.ExecuteNonQuery();
            }

            if (rows == 0)
                throw new KeyNotFoundException($"No row with Id = {id} found in {table}.");

            DB.FlushTempTableToFile(conn, file);
            tx.Commit();
        }

        public void Delete(TableName table, long id)
        {
            string file = DB.GetFilePath(table);
            DB.EnsureFileExists(file);

            using var conn = DB.GetConnection();
            using var tx = conn.BeginTransaction();

            using (var loadCmd = conn.CreateCommand())
            {
                loadCmd.CommandText = $"CREATE TABLE tmp AS SELECT * FROM read_parquet('{DB.Escape(file)}')";
                loadCmd.ExecuteNonQuery();
            }

            int rows;
            using (var deleteCmd = conn.CreateCommand())
            {
                deleteCmd.CommandText = "DELETE FROM tmp WHERE Id = ?";
                deleteCmd.Parameters.Add(new DuckDBParameter(id));
                rows = deleteCmd.ExecuteNonQuery();
            }

            if (rows == 0)
                throw new KeyNotFoundException($"No row with Id = {id} found in {table}.");

            DB.FlushTempTableToFile(conn, file);
            tx.Commit();
        }

        public void Truncate(TableName table, string columnDefinitionsSql)
        {
            string file = DB.GetFilePath(table);

            using var conn = DB.GetConnection();
            using var tx = conn.BeginTransaction();

            using (var cmd = conn.CreateCommand())
            {
                cmd.CommandText = $"CREATE TABLE tmp ({columnDefinitionsSql})";
                cmd.ExecuteNonQuery();
            }

            DB.FlushTempTableToFile(conn, file);
            tx.Commit();
        }
    }
}
