using Microsoft.AspNetCore.Mvc;

namespace CarsWeb.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class DilersController : ControllerBase
    {
        public DilersController(IDataBaseHandler db) => _dataBaseHandler = db;

        private IDataBaseHandler _dataBaseHandler;
        [HttpGet]
        public async Task<List<Diler>> GetDilers(CancellationToken ct)
        {
            return await _dataBaseHandler.GetAllDilers(ct);
        }

        [HttpGet("{id:int}")]
        public async Task<ActionResult<Diler>> GetDilerByID(int id, CancellationToken ct)
        {
            var diler = await _dataBaseHandler.GetDilerByID(id, ct);
            return diler is null ? NotFound() : Ok(diler);
        }

        [HttpDelete("{id:int}")]
        public async Task<ActionResult<bool>> DeleteDiler(int id, CancellationToken ct)
        {
            var deleted = await _dataBaseHandler.DeleteDiler(id, ct);
            return deleted is false ? Problem("Не удалось удалить") : NoContent();
        }

        [HttpPost]
        public async Task<ActionResult<Diler>> PostDiler(Diler diler, CancellationToken ct = default)
        {
            var id = await _dataBaseHandler.PostDiler(diler, ct);
            if (id is null)
                return Problem("Не удалось создать дилера");

            return CreatedAtAction(nameof(GetDilerByID), new { id = id.Value }, diler);
        }

        [HttpPut("{id:int}")]
        public async Task<ActionResult<bool>> PutDiler(int id, Diler diler, CancellationToken ct = default)
        {
            var updated = await _dataBaseHandler.PutDiler(id, diler, ct);
            return updated is false ? NotFound() : Ok(updated);
        }
    }
}
