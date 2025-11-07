using Asspire.OrderService.Data;
using Asspire.OrderService.Features.Orders.Commands;
using Asspire.OrderService.Features.Orders.Queries;
using Asspire.OrderService.IntegrationEvents;
using Asspire.OrderService.Models;
using Asspire.ProductService.Grpc;
using MassTransit;
using MediatR;

var builder = WebApplication.CreateBuilder(args);

// Add service defaults & Aspire components
builder.AddServiceDefaults();

// Add PostgreSQL for Write operations
builder.AddNpgsqlDbContext<OrderWriteDbContext>("writedb");

// Add PostgreSQL for Read operations
builder.AddNpgsqlDbContext<OrderReadDbContext>("readdb");

// Add Redis caching
builder.Services.AddStackExchangeRedisCache(options =>
{
    options.Configuration = builder.Configuration.GetConnectionString("redis");
});

// Add gRPC client for Product Service
builder.Services.AddGrpcClient<ProductGrpc.ProductGrpcClient>(options =>
{
    options.Address = new Uri("https+http://productservice");
})
.AddServiceDiscovery();

// Add MediatR for CQRS
builder.Services.AddMediatR(cfg =>
{
    cfg.RegisterServicesFromAssembly(typeof(Program).Assembly);
});

// Add MassTransit with RabbitMQ
builder.Services.AddMassTransit(x =>
{
    // Add consumers
    x.AddConsumer<OrderCreatedEventConsumer>();
    x.AddConsumer<OrderStatusUpdatedEventConsumer>();

    x.UsingRabbitMq((context, cfg) =>
    {
        var rabbitMqConnection = builder.Configuration.GetConnectionString("rabbitmq");
        cfg.Host(new Uri(rabbitMqConnection ?? "amqp://guest:guest@localhost:5672"));

        cfg.ConfigureEndpoints(context);
    });
});

// Add controllers and Swagger
builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

var app = builder.Build();

// Ensure databases are created
using (var scope = app.Services.CreateScope())
{
    var writeContext = scope.ServiceProvider.GetRequiredService<OrderWriteDbContext>();
    var readContext = scope.ServiceProvider.GetRequiredService<OrderReadDbContext>();
    await writeContext.Database.EnsureCreatedAsync();
    await readContext.Database.EnsureCreatedAsync();
}

// Configure the HTTP request pipeline
app.MapDefaultEndpoints();

if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

// Map REST API endpoints
app.MapGet("/api/orders", async (int pageNumber, int pageSize, IMediator mediator) =>
{
    var query = new GetOrdersQuery(pageNumber, pageSize);
    var result = await mediator.Send(query);
    return Results.Ok(result);
});

app.MapGet("/api/orders/{id}", async (Guid id, IMediator mediator) =>
{
    var query = new GetOrderByIdQuery(id);
    var result = await mediator.Send(query);
    return result != null ? Results.Ok(result) : Results.NotFound();
});

app.MapPost("/api/orders", async (CreateOrderCommand command, IMediator mediator) =>
{
    var result = await mediator.Send(command);
    return result != null
        ? Results.Created($"/api/orders/{result.Id}", result)
        : Results.BadRequest("Product not available");
});

app.MapPut("/api/orders/{id}/status", async (Guid id, OrderStatus status, IMediator mediator) =>
{
    var command = new UpdateOrderStatusCommand(id, status);
    var result = await mediator.Send(command);
    return result != null ? Results.Ok(result) : Results.NotFound();
});

app.Run();
