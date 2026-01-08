using ContosoPizza.Data;
using ContosoPizza.Models;
using Microsoft.EntityFrameworkCore;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddOpenApiDocument(config =>
{
    config.Title = "Contoso Pizza API";
    config.Version = "v1";
});

builder.Services.AddDbContext<PizzaDb>(options => 
    options.UseInMemoryDatabase("Pizzas"));

var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseOpenApi();
    app.UseSwaggerUi(c =>
    {
        c.Path = "/swagger";
        c.DocumentTitle = "Contoso Pizza API";
    });
}

app.UseHttpsRedirection();
app.UseAuthorization();
app.MapControllers();

// Seed data including your additional record
using (var scope = app.Services.CreateScope())
{
    var db = scope.ServiceProvider.GetRequiredService<PizzaDb>();
    if (!db.Pizzas.Any())
    {
        db.Pizzas.AddRange(
            new Pizza { Id = 1, Name = "Classic Italian", Price = 12.99m, Description = "Tomato sauce and mozzarella" },
            new Pizza { Id = 2, Name = "Veggie", Price = 11.99m, Description = "Lots of vegetables" },
            new Pizza { Id = 3, Name = "Margherita", Price = 9.99m, Description = "Fresh basil and tomato" },
            new Pizza { Id = 4, Name = "Pepperoni Feast", Price = 14.99m, Description = "Loaded with pepperoni" },
            new Pizza { Id = 5, Name = "Hawaiian Delight", Price = 14.99m, Description = "Pineapple, ham, and mozzarella on tomato sauce" } // <-  New record
        );
        db.SaveChanges();
    }
}

app.Run();