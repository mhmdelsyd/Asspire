using Asspire.ProductService.Data;
using Asspire.ProductService.Features.Products.Commands;
using Asspire.ProductService.Features.Products.Queries;
using Asspire.ProductService.IntegrationEvents;
using Asspire.ProductService.Services;
using MassTransit;
using MediatR;
using Microsoft.EntityFrameworkCore;

var builder = WebApplication.CreateBuilder(args);

// Add service defaults & Aspire components
builder.AddServiceDefaults();

// Add PostgreSQL for Write operations
builder.AddNpgsqlDbContext<ProductWriteDbContext>("writedb");

// Add PostgreSQL for Read operations
builder.AddNpgsqlDbContext<ProductReadDbContext>("readdb");

// Add Redis caching
builder.Services.AddStackExchangeRedisCache(options =>
{
    options.Configuration = builder.Configuration.GetConnectionString("redis");
});

// Add MediatR for CQRS
builder.Services.AddMediatR(cfg =>
{
    cfg.RegisterServicesFromAssembly(typeof(Program).Assembly);
});

// Add MassTransit with RabbitMQ
builder.Services.AddMassTransit(x =>
{
    // Add consumers
    x.AddConsumer<ProductCreatedEventConsumer>();
    x.AddConsumer<ProductUpdatedEventConsumer>();
    x.AddConsumer<ProductDeletedEventConsumer>();

    x.UsingRabbitMq((context, cfg) =>
    {
        var rabbitMqConnection = builder.Configuration.GetConnectionString("rabbitmq");
        cfg.Host(new Uri(rabbitMqConnection ?? "amqp://guest:guest@localhost:5672"));

        cfg.ConfigureEndpoints(context);
    });
});

// Add gRPC
builder.Services.AddGrpc();

// Add controllers and Swagger
builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

// Add database seeder
builder.Services.AddScoped<DatabaseSeeder>();

var app = builder.Build();

// Seed database
using (var scope = app.Services.CreateScope())
{
    var seeder = scope.ServiceProvider.GetRequiredService<DatabaseSeeder>();
    await seeder.SeedAsync();
}

// Configure the HTTP request pipeline
app.MapDefaultEndpoints();

if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

// Map gRPC service
app.MapGrpcService<ProductGrpcService>();

// Map REST API endpoints
app.MapGet("/api/products", async (int pageNumber, int pageSize, IMediator mediator) =>
{
    var query = new GetProductsQuery(pageNumber, pageSize);
    var result = await mediator.Send(query);
    return Results.Ok(result);
});

app.MapGet("/api/products/{id}", async (int id, IMediator mediator) =>
{
    var query = new GetProductByIdQuery(id);
    var result = await mediator.Send(query);
    return result != null ? Results.Ok(result) : Results.NotFound();
});

app.MapPost("/api/products", async (CreateProductCommand command, IMediator mediator) =>
{
    var result = await mediator.Send(command);
    return Results.Created($"/api/products/{result.Id}", result);
});

app.MapPut("/api/products/{id}", async (int id, UpdateProductCommand command, IMediator mediator) =>
{
    if (id != command.Id)
        return Results.BadRequest("ID mismatch");

    var result = await mediator.Send(command);
    return result != null ? Results.Ok(result) : Results.NotFound();
});

app.MapDelete("/api/products/{id}", async (int id, IMediator mediator) =>
{
    var command = new DeleteProductCommand(id);
    var result = await mediator.Send(command);
    return result ? Results.NoContent() : Results.NotFound();
});

app.Run();
