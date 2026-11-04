using Microsoft.AspNetCore.Mvc;

namespace CarsWeb.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class DilersController : ControllerBase
    {
        public DilersController(IDataBaseHandler<Diler, DilerPgSQLHandler> db) => _dataBaseHandler = db;

        private IDataBaseHandler<Diler, DilerPgSQLHandler> _dataBaseHandler;
        [HttpGet]
        public async Task<List<Diler>> GetDilers(CancellationToken ct)
        {
            return await _dataBaseHandler.GetAll(ct);
        }

        [HttpGet("{id:int}")]
        public async Task<ActionResult<Diler>> GetDilerByID(int id, CancellationToken ct)
        {
            var diler = await _dataBaseHandler.GetByID(id, ct);
            return diler is null ? NotFound() : Ok(diler);
        }

        [HttpDelete("{id:int}")]
        public async Task<ActionResult<bool>> DeleteDiler(int id, CancellationToken ct)
        {
            var deleted = await _dataBaseHandler.Delete(id, ct);
            return deleted is false ? Problem("Не удалось удалить") : NoContent();
        }

        [HttpPost]
        public async Task<ActionResult<Diler>> PostDiler(Diler diler, CancellationToken ct = default)
        {
            var id = await _dataBaseHandler.Post(diler, ct);
            if (id is null)
                return Problem("Не удалось создать дилера");

            return CreatedAtAction(nameof(GetDilerByID), new { id = id.Value }, diler);
        }

        [HttpPut("{id:int}")]
        public async Task<ActionResult<bool>> PutDiler(int id, Diler diler, CancellationToken ct = default)
        {
            var updated = await _dataBaseHandler.Put(id, diler, ct);
            return updated is false ? NotFound() : Ok(updated);
        }
    }
}
