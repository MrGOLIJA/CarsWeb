//using System.Text;
//using System.Text.Json;

//namespace CarsWeb
//{
//    public class DataBaseHandlerWrapp : IDataBaseHandler
//    {
//        private readonly IDataBaseHandler inner;
//        private readonly EventPublisher eventPublisher;

//        public DataBaseHandlerWrapp(IDataBaseHandler handler, EventPublisher eventPublisher)
//        {
//            this.inner = handler;
//            this.eventPublisher = eventPublisher;
//        }

//        public async Task FillCarsFromFile(string file)
//        {
//            await inner.FillCarsFromFile(file);
//        }

//        public async Task FillDilersFromFile(string file)
//        {
//            await inner.FillDilersFromFile(file);
//        }

//        public async Task<List<Car>> GetAllCars(CancellationToken ct = default)
//        {
//            return await inner.GetAllCars(ct);
//        }

//        public async Task<Car?> GetCarByID(int id, CancellationToken ct = default)
//        {
//            return await inner.GetCarByID(id, ct);
//        }

//        public async Task<int?> PostCar(Car car, CancellationToken ct = default)
//        {
//            var res = await inner.PostCar(car, ct);
//            var mes = new Message { Event = "CREATE", Payload = res };
//            var body = Encoding.UTF8.GetBytes(JsonSerializer.Serialize(mes));
//            eventPublisher.PublishAsync(body).GetAwaiter().GetResult();
//            return res;
//        }


//    }
//}
