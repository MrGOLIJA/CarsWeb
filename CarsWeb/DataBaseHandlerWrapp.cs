using System.Text;
using System.Text.Json;

namespace CarsWeb
{
    public class DataBaseHandlerWrapp<TEntinty, THandler> : IDataBaseHandler<TEntinty, THandler>
    {
        private readonly IDataBaseHandler<TEntinty, THandler> inner;
        private readonly EventPublisher eventPublisher;

        public DataBaseHandlerWrapp(IDataBaseHandler<TEntinty, THandler> handler, EventPublisher eventPublisher)
        {
            this.inner = handler;
            this.eventPublisher = eventPublisher;
        }

        public async Task FillFromFile(string file)
        {
            await inner.FillFromFile(file);
        }

        public async Task<List<TEntinty>> GetAll(CancellationToken ct = default)
        {
            return await inner.GetAll(ct);
        }

        public async Task<TEntinty?> GetByID(int id, CancellationToken ct = default)
        {
            return await inner.GetByID(id, ct);
        }

        public async Task<int?> Post(TEntinty t, CancellationToken ct = default)
        {
            var res = await inner.Post(t, ct);
            var mes = new Message { Event = "CREATE", Payload = res };
            var body = Encoding.UTF8.GetBytes(JsonSerializer.Serialize(mes));
            await eventPublisher.PublishAsync(body);
            return res;
        }

        public async Task<bool> Delete(int ID, CancellationToken ct = default)
        {
            var res = await inner.Delete(ID, ct);
            var mes = new Message { Event = "Delete", Payload = res };
            var body = Encoding.UTF8.GetBytes(JsonSerializer.Serialize(mes));
            await eventPublisher.PublishAsync(body);
            return res;
        }

        public async Task<bool> Put(int ID, TEntinty t, CancellationToken ct = default)
        {
            var res = await inner.Put(ID, t, ct);
            var mes = new Message { Event = "Put", Payload = res };
            var body = Encoding.UTF8.GetBytes(JsonSerializer.Serialize(mes));
            await eventPublisher.PublishAsync(body);
            return res;
        }
    }
}
