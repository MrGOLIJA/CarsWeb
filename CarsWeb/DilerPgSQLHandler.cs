using Npgsql;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace CarsWeb
{
    public class Diler
    {
        [JsonPropertyName("id")]
        public int id { get; set; }
        public string Name { get; set; }
        public string City { get; set; }
        public string Address { get; set; }
        public string Area { get; set; }
        public double Rating { get; set; }
    }
    public class DilerPgSQLHandler : IDataBaseHandler<Diler>, IDisposable
    {
        private readonly NpgsqlDataSource _dataSource;

        private DilerPgSQLHandler(NpgsqlDataSource ds) => _dataSource = ds;

        public void Dispose()
        {
            _dataSource.Dispose();
        }

        public async Task FillFromFile(string fileName)
        {
            await using var conn = await _dataSource.OpenConnectionAsync();
            string json = await File.ReadAllTextAsync(fileName);
            var dilers = JsonSerializer.Deserialize<List<Diler>>(json);

            foreach (var diler in dilers)
            {
                await using var cmd = new NpgsqlCommand(
                    "INSERT INTO dilers (name, city,address,area,rating)" +
                    " VALUES (@name,@city,@address,@area,@rating)", conn);
                cmd.Parameters.AddWithValue("name", diler.Name);
                cmd.Parameters.AddWithValue("city", diler.City);
                cmd.Parameters.AddWithValue("address", diler.Address);
                cmd.Parameters.AddWithValue("area", diler.Area);
                cmd.Parameters.AddWithValue("rating", diler.Rating);
                await cmd.ExecuteNonQueryAsync();
            }
        }

        public async Task<List<Diler>> GetAll(CancellationToken ct = default)
        {
            await using var conn = await _dataSource.OpenConnectionAsync(ct);

            var dilers = new List<Diler>();
            await using var comm = new NpgsqlCommand("SELECT * FROM dilers", conn);
            await using var reader = await comm.ExecuteReaderAsync(ct);

            while (await reader.ReadAsync(ct))
            {
                Diler diler = new Diler
                {
                    id = reader.GetInt32(reader.GetOrdinal("id")),
                    Name = reader.GetString(reader.GetOrdinal("name")),
                    City = reader.GetString(reader.GetOrdinal("city")),
                    Address = reader.GetString(reader.GetOrdinal("address")),
                    Area = reader.GetString(reader.GetOrdinal("area")),
                    Rating = reader.GetFloat(reader.GetOrdinal("rating"))
                };

                dilers.Add(diler);
            }

            return dilers;
        }

        public async Task<Diler> GetByID(int ID, CancellationToken ct = default)
        {
            await using var conn = await _dataSource.OpenConnectionAsync(ct);

            await using var comm = new NpgsqlCommand("SELECT * FROM dilers WHERE id = @id", conn);
            comm.Parameters.AddWithValue("id", ID);
            await using var reader = await comm.ExecuteReaderAsync(ct);

            if (await reader.ReadAsync(ct))
            {

                Diler diler = new Diler
                {
                    id = reader.GetInt32(reader.GetOrdinal("id")),
                    Name = reader.GetString(reader.GetOrdinal("name")),
                    City = reader.GetString(reader.GetOrdinal("city")),
                    Address = reader.GetString(reader.GetOrdinal("address")),
                    Area = reader.GetString(reader.GetOrdinal("area")),
                    Rating = reader.GetFloat(reader.GetOrdinal("rating"))
                };

                return diler;
            }
            else
            {
                return null;
            }
        }

        public async Task<int?> Post(Diler diler, CancellationToken ct = default)
        {
            await using var conn = await _dataSource.OpenConnectionAsync(ct);

            await using var comm = new NpgsqlCommand(
                "INSERT INTO dilers (name, city, address, area, rating) " +
                "VALUES (@name, @city, @address, @area, @rating) " +
                "RETURNING id;", conn);

            comm.Parameters.AddWithValue("name", diler.Name);
            comm.Parameters.AddWithValue("city", diler.City);
            comm.Parameters.AddWithValue("address", diler.Address);
            comm.Parameters.AddWithValue("area", diler.Area);
            comm.Parameters.AddWithValue("rating", diler.Rating);

            var result = await comm.ExecuteScalarAsync(ct);

            if (result is null or DBNull)
                return null;

            var id = (int)result;
            diler.id = id;
            return id;
        }

        public async Task<bool> Delete(int ID, CancellationToken ct = default)
        {
            await using var conn = await _dataSource.OpenConnectionAsync(ct);
            try
            {
                await using var comm = new NpgsqlCommand("DELETE FROM dilers WHERE id = @id", conn);
                comm.Parameters.AddWithValue("id", ID);
                var deleted = await comm.ExecuteNonQueryAsync();
                return deleted > 0;
            }
            catch (PostgresException ex)
            {
                Console.WriteLine("Ошибка связи внешнего ключа");
                return false;
            }
        }

        public async Task<bool> Put(int ID, Diler diler, CancellationToken ct = default)
        {
            await using var conn = await _dataSource.OpenConnectionAsync(ct);

            await using var comm = new NpgsqlCommand(
                "UPDATE dilers SET name = @name, city = @city, address = @address, " +
                "area = @area, rating = @rating " +
                "WHERE id = @id " +
                "RETURNING id;", conn);

            comm.Parameters.AddWithValue("id", ID);
            comm.Parameters.AddWithValue("name", diler.Name);
            comm.Parameters.AddWithValue("city", diler.City);
            comm.Parameters.AddWithValue("address", diler.Address);
            comm.Parameters.AddWithValue("area", diler.Area);
            comm.Parameters.AddWithValue("rating", diler.Rating);

            await using var reader = await comm.ExecuteReaderAsync(ct);

            return await reader.ReadAsync(ct);
        }
    }
}
