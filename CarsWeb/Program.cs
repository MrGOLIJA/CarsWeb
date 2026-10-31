namespace CarsWeb
{
    internal class Program
    {
        static async Task Main(string[] args)
        {
            using PostgreSQLHandler handler = PostgreSQLHandler.Create().GetAwaiter().GetResult();
            await handler.FillDilersFromFile("dilers.json");
            await handler.FillCarsFromFile("cars.json");

            var builder = WebApplication.CreateBuilder(args);

            builder.Services.AddControllers();
            builder.Services.AddSingleton<IDataBaseHandler>(handler);

            builder.Services.AddEndpointsApiExplorer();
            builder.Services.AddSwaggerGen();

            var app = builder.Build();          

            if (app.Environment.IsDevelopment())
            {
                app.UseSwagger();
                app.UseSwaggerUI();
            }

            app.UseHttpsRedirection();
            app.UseAuthorization();

            app.MapControllers();

            app.Run();
        }
    }
}
