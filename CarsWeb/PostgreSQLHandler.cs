using Npgsql;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.Json;
using System.Text.Json.Serialization;
using System.Threading.Tasks;

namespace CarsWeb
{
    internal class PostgreSQLHandler : IDataBaseHandler, IDisposable
    {
        private readonly NpgsqlDataSource _dataSource;

        private PostgreSQLHandler(NpgsqlDataSource ds) => _dataSource = ds;

        public void Dispose()
        {
           _dataSource.Dispose();
        }
        public static async Task<PostgreSQLHandler> Create(CancellationToken ct = default)
        {
            var ds = NpgsqlDataSource.Create(Config.connect);
            await using var conn = await ds.OpenConnectionAsync(ct);

            await using var clearTable = new NpgsqlCommand("DROP TABLE IF EXISTS cars, dilers CASCADE;", conn);
            await clearTable.ExecuteNonQueryAsync(ct);

            await using var createTableCmd = new NpgsqlCommand(
                "CREATE TABLE IF NOT EXISTS dilers(" +
                "id SERIAL PRIMARY KEY," +
                "name TEXT NOT NULL," +
                "city TEXT NOT NULL," +
                "address TEXT NOT NULL," +
                "area  TEXT NOT NULL," +
                "rating REAL NOT NULL);" +

                "CREATE TABLE IF NOT EXISTS cars (" +
                "id SERIAL PRIMARY KEY, " +
                "firm TEXT NOT NULL, " +
                "model TEXT NOT NULL, " +
                "year INTEGER NOT NULL, " +
                "power INTEGER NOT NULL, " +
                "color TEXT NOT NULL, " +
                "price INTEGER NOT NULL, " +
                "idDiler INTEGER REFERENCES dilers(id)" +
                ");" +

                "CREATE OR REPLACE FUNCTION assign_random_diler()" +
                "RETURNS TRIGGER AS $$" +
                "BEGIN " +
                "IF NEW.idDiler IS NULL THEN " +
                "SELECT id INTO NEW.idDiler " +
                "FROM dilers " +
                "ORDER BY random() " +
                "LIMIT 1;" +
                "END IF;" +
                "RETURN NEW;" +
                "END;" +
                "$$ LANGUAGE plpgsql;" +

                "CREATE TRIGGER trg_assign_random_diler " +
                "BEFORE INSERT ON cars " +
                "FOR EACH ROW " +
                "EXECUTE FUNCTION assign_random_diler();", conn);
            await createTableCmd.ExecuteNonQueryAsync(ct);

            return new PostgreSQLHandler(ds);
        }
        
        public async Task FillCarsFromFile(string nameFile)
        {
            await using var conn = await _dataSource.OpenConnectionAsync();

            string json = await File.ReadAllTextAsync(nameFile);
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

        public async Task FillDilersFromFile(string nameFile)
        {
            await using var conn = await _dataSource.OpenConnectionAsync();
            string json = await File.ReadAllTextAsync(nameFile);
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

        public async Task<List<Car>> GetAllCars(CancellationToken ct = default)
        {
            await using var conn = await _dataSource.OpenConnectionAsync(ct);

            var cars = new List<Car>();
            await using var comm = new NpgsqlCommand("SELECT * FROM cars ORDER BY id", conn);
            await using var reader = await comm.ExecuteReaderAsync(ct);

            while(await reader.ReadAsync(ct))
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

        public async Task<Car?> GetCarByID(int ID, CancellationToken ct = default)
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

        public async Task<List<Diler>> GetAllDilers(CancellationToken ct = default)
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

        public async Task<Diler?> GetDilerByID(int ID, CancellationToken ct = default)
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

        public async Task<int?> PostCar(Car car, CancellationToken ct = default)
        {
            await using var conn = await _dataSource.OpenConnectionAsync(ct);

            await using var comm = new NpgsqlCommand("INSERT INTO cars (firm,model,year,power,color,price) " +
                                                     "VALUES (@firm, @model, @year, @power, @color, @price)" +
                                                     "RETURNING id;", conn);

            comm.Parameters.AddWithValue("firm",car.Firm);
            comm.Parameters.AddWithValue("model",car.Model);
            comm.Parameters.AddWithValue("year",car.Year);
            comm.Parameters.AddWithValue("power",car.Power);
            comm.Parameters.AddWithValue("color",car.Color);
            comm.Parameters.AddWithValue("price",car.Price);

            var result = await comm.ExecuteScalarAsync(ct);

            if (result is null or DBNull)
                return null;

            var id = (int)result;
            car.id = id;
            return id;
        }

        public async Task<bool> DeleteCar(int ID, CancellationToken ct = default)
        {
            await using var conn = await _dataSource.OpenConnectionAsync(ct);

            await using var comm = new NpgsqlCommand("DELETE FROM cars WHERE id = @id", conn);

            comm.Parameters.AddWithValue("id", ID);

            var deleted = await comm.ExecuteNonQueryAsync(ct);
            return deleted > 0;
        }

        public async Task<bool> PutCar(int ID, Car car, CancellationToken ct = default)
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

        public async Task<int?> PostDiler(Diler diler, CancellationToken ct = default)
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

        public async Task<bool> DeleteDiler(int ID, CancellationToken ct = default)
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

        public async Task<bool> PutDiler(int ID, Diler diler, CancellationToken ct = default) 
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
