using Npgsql;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.Json.Serialization;
using System.Threading.Tasks;

namespace CarsWeb
{
    public interface IDataBaseHandler<TEntity, THandler>
    {
        Task FillFromFile(string fileName);

        Task<List<TEntity>> GetAll(CancellationToken ct = default);
        Task<TEntity> GetByID(int ID, CancellationToken ct = default);
        Task<int?> Post(TEntity t, CancellationToken ct = default);
        Task<bool> Delete(int ID, CancellationToken ct = default);
        Task<bool> Put(int ID,TEntity t, CancellationToken ct = default);

    }
}
