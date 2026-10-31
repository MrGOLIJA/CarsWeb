using Npgsql;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.Json.Serialization;
using System.Threading.Tasks;

namespace CarsWeb
{
    public interface IDataBaseHandler<T>
    {
        static abstract Task Create(CancellationToken ct = default);
        Task FillFromFile(string fileName);

        Task<List<T>> GetAll(CancellationToken ct = default);
        Task<T> GetByID(int ID, CancellationToken ct = default);
        Task<int?> Post(T t, CancellationToken ct = default);
        Task<bool> Delete(int ID, CancellationToken ct = default);
        Task<bool> Put(int ID,T t, CancellationToken ct = default);

    }
}
