using ContosoPizza.Data;
using ContosoPizza.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace ContosoPizza.Controllers;

[Route("[controller]")]
[ApiController]
public class PizzaController : ControllerBase
{
    private readonly PizzaDb _context;

    public PizzaController(PizzaDb context)
    {
        _context = context;
    }

    // GET: /Pizza
    [HttpGet]
    public async Task<ActionResult<IEnumerable<Pizza>>> GetPizzas()
    {
        return await _context.Pizzas.ToListAsync();
    }

    // GET: /Pizza/1
    [HttpGet("{id}")]
    public async Task<ActionResult<Pizza>> GetPizza(int id)
    {
        var pizza = await _context.Pizzas.FindAsync(id);
        if (pizza == null) return NotFound();
        return pizza;
    }

    // POST: /Pizza
    [HttpPost]
    public async Task<ActionResult<Pizza>> PostPizza(Pizza pizza)
    {
        _context.Pizzas.Add(pizza);
        await _context.SaveChangesAsync();
        return CreatedAtAction(nameof(GetPizza), new { id = pizza.Id }, pizza);
    }

    // PUT: /Pizza/1
    [HttpPut("{id}")]
    public async Task<IActionResult> PutPizza(int id, Pizza pizza)
    {
        if (id != pizza.Id) return BadRequest();
        _context.Entry(pizza).State = EntityState.Modified;
        await _context.SaveChangesAsync();
        return NoContent();
    }

    // DELETE: /Pizza/1
    [HttpDelete("{id}")]
    public async Task<IActionResult> DeletePizza(int id)
    {
        var pizza = await _context.Pizzas.FindAsync(id);
        if (pizza == null) return NotFound();
        _context.Pizzas.Remove(pizza);
        await _context.SaveChangesAsync();
        return NoContent();
    }
}