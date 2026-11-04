namespace CarsWeb
{
    internal class Program
    {
        static async Task Main(string[] args)
        {
            EventPublisher publisher = new EventPublisher("localhost");
            await publisher.InitAsync("cars_exchange", "cars_queue");
            DataBaseHandlerWrapp<Car, CarPgSQLHandler> carWrapp = new DataBaseHandlerWrapp<Car, CarPgSQLHandler>(CarPgSQLHandler.GetHandler(), publisher);
            DataBaseHandlerWrapp<Diler, DilerPgSQLHandler> dilerWrapp = new DataBaseHandlerWrapp<Diler, DilerPgSQLHandler>(DilerPgSQLHandler.GetHandler(), publisher);
            await carWrapp.FillFromFile("cars.json");
            await dilerWrapp.FillFromFile("dilers.json");

            var builder = WebApplication.CreateBuilder(args);

            builder.Services.AddControllers();
            builder.Services.AddSingleton<IDataBaseHandler<Car,CarPgSQLHandler>>(carWrapp);
            builder.Services.AddSingleton<IDataBaseHandler<Diler,DilerPgSQLHandler>>(dilerWrapp);
            
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
