using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace CarsWeb.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class CarsController : ControllerBase
    {
        public CarsController(IDataBaseHandler db) => _dataBaseHandler = db;

        private IDataBaseHandler _dataBaseHandler;
        [HttpGet]
        public async Task<List<Car>> GetCars(CancellationToken ct)
        {
            return await _dataBaseHandler.GetAllCars(ct);
        }

        [HttpGet("{id:int}")]
        public async Task<ActionResult<Car>> GetCarByID(int id, CancellationToken ct)
        {
            var car = await _dataBaseHandler.GetCarByID(id, ct);
            return car is null ? NotFound() : Ok(car);
        }

        [HttpDelete("{id:int}")]
        public async Task<ActionResult<bool>> DeleteCar(int id, CancellationToken ct)
        {
            var delete = await _dataBaseHandler.DeleteCar(id, ct);
            return delete is false ? NoContent() : NotFound();
        }

        [HttpPost]
        public async Task<ActionResult<Car>> PostCar(Car car, CancellationToken ct = default)
        {
            var id = await _dataBaseHandler.PostCar(car, ct);
            if (id is null)
                return Problem("Не удалось создать машину");

            return CreatedAtAction(nameof(GetCarByID), new { id = id.Value }, car);
        }

        [HttpPut("{id:int}")]
        public async Task<ActionResult<bool>> PutCar(int id, Car car, CancellationToken ct = default)
        {
            var updated = await _dataBaseHandler.PutCar(id, car, ct);
            return updated is false ? NotFound() : Ok(updated);
        }

        
    }
}
