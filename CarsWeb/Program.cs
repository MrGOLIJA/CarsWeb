namespace CarsWeb
{
    internal class Program
    {
        static async Task Main(string[] args)
        {
            using CarPgSQLHandler carHandler = CarPgSQLHandler.GetHandler();
            using DilerPgSQLHandler dilerHandler = DilerPgSQLHandler.GetHandler();
            await carHandler.FillFromFile("cars.json");
            await dilerHandler.FillFromFile("dilers.json");

            var builder = WebApplication.CreateBuilder(args);

            builder.Services.AddControllers();
            builder.Services.AddSingleton<IDataBaseHandler<Car,CarPgSQLHandler>>(_ => CarPgSQLHandler.GetHandler());
            builder.Services.AddSingleton<IDataBaseHandler<Diler,DilerPgSQLHandler>>(_ => DilerPgSQLHandler.GetHandler());
            
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
