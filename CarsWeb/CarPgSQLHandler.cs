using Microsoft.AspNetCore.Routing.Matching;
using Npgsql;
using System.Collections.Generic;
using System.Data.Common;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace CarsWeb
{
    public class Car
    {
        [JsonPropertyName("id")]
        public int id { get; set; }

        [JsonPropertyName("firm")]
        public string Firm { get; set; }
        [JsonPropertyName("model")]
        public string Model { get; set; }
        [JsonPropertyName("year")]
        public int Year { get; set; }
        [JsonPropertyName("power")]
        public int Power { get; set; }
        [JsonPropertyName("color")]
        public string Color { get; set; }
        [JsonPropertyName("price")]
        public int Price { get; set; }
    }
    public class CarPgSQLHandler : IDataBaseHandler<Car,CarPgSQLHandler>, IDisposable
    {
        private readonly NpgsqlDataSource _dataSource;
        private static CarPgSQLHandler _inst;

        private CarPgSQLHandler(NpgsqlDataSource ds) => _dataSource = ds;

        public void Dispose()
        {
            _dataSource?.Dispose();
        }

        public static CarPgSQLHandler GetHandler()
        {
            if (_inst == null) _inst = CarPgSQLHandler.Create().Result;
            return _inst;
        }

        private static async Task<CarPgSQLHandler> Create(CancellationToken ct = default)
        {
            var ds = NpgsqlDataSource.Create(Config.connect);
            await using var conn = await ds.OpenConnectionAsync(ct);

            await using var clearTable = new NpgsqlCommand("DROP TABLE IF EXISTS cars CASCADE;", conn);
            await clearTable.ExecuteNonQueryAsync(ct);

            await using var createTableCmd = new NpgsqlCommand(
                "CREATE TABLE IF NOT EXISTS cars (" +
                "id SERIAL PRIMARY KEY, " +
                "firm TEXT NOT NULL, " +
                "model TEXT NOT NULL, " +
                "year INTEGER NOT NULL, " +
                "power INTEGER NOT NULL, " +
                "color TEXT NOT NULL, " +
                "price INTEGER NOT NULL, " +
                "idDiler INTEGER REFERENCES dilers(id)" +
                ");", conn);
            await createTableCmd.ExecuteNonQueryAsync(ct);

            return new CarPgSQLHandler(ds);
        }

        public async Task FillFromFile(string fileName)
        {
            await using var conn = await _dataSource.OpenConnectionAsync();

            string json = await File.ReadAllTextAsync(fileName);
            using var doc = JsonDocument.Parse(json);
            var cars = doc.RootElement.GetProperty("cars").Deserialize<List<Car>>();

            foreach (var car in cars)
            {
                await using var cmd = new NpgsqlCommand(
                    "INSERT INTO cars (firm, model,year,power,color,price)" +
                    " VALUES (@firm,@model,@year,@power,@color,@price)", conn);
                cmd.Parameters.AddWithValue("firm", car.Firm);
                cmd.Parameters.AddWithValue("model", car.Model);
                cmd.Parameters.AddWithValue("year", car.Year);
                cmd.Parameters.AddWithValue("power", car.Power);
                cmd.Parameters.AddWithValue("color", car.Color);
                cmd.Parameters.AddWithValue("price", car.Price);
                await cmd.ExecuteNonQueryAsync();
            }
        }

        public async Task<List<Car>> GetAll(CancellationToken ct = default)
        {
            await using var conn = await _dataSource.OpenConnectionAsync(ct);

            var cars = new List<Car>();
            await using var comm = new NpgsqlCommand("SELECT * FROM cars ORDER BY id", conn);
            await using var reader = await comm.ExecuteReaderAsync(ct);

            while (await reader.ReadAsync(ct))
            {
                Car car = new Car
                {
                    id = reader.GetInt32(reader.GetOrdinal("id")),
                    Firm = reader.GetString(reader.GetOrdinal("firm")),
                    Model = reader.GetString(reader.GetOrdinal("model")),
                    Year = reader.GetInt32(reader.GetOrdinal("year")),
                    Power = reader.GetInt32(reader.GetOrdinal("power")),
                    Color = reader.GetString(reader.GetOrdinal("color")),
                    Price = reader.GetInt32(reader.GetOrdinal("price"))
                };

                cars.Add(car);
            }

            return cars;
        }

        public async Task<Car> GetByID(int ID, CancellationToken ct = default)
        {
            await using var conn = await _dataSource.OpenConnectionAsync(ct);

            await using var comm = new NpgsqlCommand("SELECT * FROM cars WHERE id = @id", conn);
            comm.Parameters.AddWithValue("id", ID);
            await using var reader = await comm.ExecuteReaderAsync(ct);

            if (await reader.ReadAsync(ct))
            {

                Car car = new Car
                {
                    id = reader.GetInt32(reader.GetOrdinal("id")),
                    Firm = reader.GetString(reader.GetOrdinal("firm")),
                    Model = reader.GetString(reader.GetOrdinal("model")),
                    Year = reader.GetInt32(reader.GetOrdinal("year")),
                    Power = reader.GetInt32(reader.GetOrdinal("power")),
                    Color = reader.GetString(reader.GetOrdinal("color")),
                    Price = reader.GetInt32(reader.GetOrdinal("price"))
                };

                return car;
            }
            else
            {
                return null;
            }
        }

        public async Task<int?> Post(Car car, CancellationToken ct = default)
        {
            await using var conn = await _dataSource.OpenConnectionAsync(ct);

            await using var comm = new NpgsqlCommand("INSERT INTO cars (firm,model,year,power,color,price) " +
                                                     "VALUES (@firm, @model, @year, @power, @color, @price)" +
                                                     "RETURNING id;", conn);

            comm.Parameters.AddWithValue("firm", car.Firm);
            comm.Parameters.AddWithValue("model", car.Model);
            comm.Parameters.AddWithValue("year", car.Year);
            comm.Parameters.AddWithValue("power", car.Power);
            comm.Parameters.AddWithValue("color", car.Color);
            comm.Parameters.AddWithValue("price", car.Price);

            var result = await comm.ExecuteScalarAsync(ct);

            if (result is null or DBNull)
                return null;

            var id = (int)result;
            car.id = id;
            return id;
        }

        public async Task<bool> Delete(int ID, CancellationToken ct = default)
        {
            await using var conn = await _dataSource.OpenConnectionAsync(ct);

            await using var comm = new NpgsqlCommand("DELETE FROM cars WHERE id = @id", conn);

            comm.Parameters.AddWithValue("id", ID);

            var deleted = await comm.ExecuteNonQueryAsync(ct);
            return deleted > 0;
        }

        public async Task<bool> Put(int ID, Car car, CancellationToken ct = default)
        {
            await using var conn = await _dataSource.OpenConnectionAsync(ct);

            await using var comm = new NpgsqlCommand("UPDATE cars SET firm = @firm, model = @model, year = @year, " +
                                                     "power = @power, color = @color, price = @price " +
                                                     "WHERE id = @id " +
                                                     "RETURNING id;", conn);

            comm.Parameters.AddWithValue("id", ID);
            comm.Parameters.AddWithValue("firm", car.Firm);
            comm.Parameters.AddWithValue("model", car.Model);
            comm.Parameters.AddWithValue("year", car.Year);
            comm.Parameters.AddWithValue("power", car.Power);
            comm.Parameters.AddWithValue("color", car.Color);
            comm.Parameters.AddWithValue("price", car.Price);

            await using var reader = await comm.ExecuteReaderAsync(ct);

            return await reader.ReadAsync(ct);
        }
    }
}
