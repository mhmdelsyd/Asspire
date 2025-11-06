var builder = DistributedApplication.CreateBuilder(args);

// Databases
var readPostgres = builder.AddPostgres("postgres-read")
    .WithPgAdmin()
    .AddDatabase("readdb");

var writePostgres = builder.AddPostgres("postgres-write")
    .AddDatabase("writedb");

// Message Broker
var rabbitmq = builder.AddRabbitMQ("rabbitmq")
    .WithManagementPlugin();

// Cache
var redis = builder.AddRedis("redis");

// Product Service
var productService = builder.AddProject<Projects.Asspire_ProductService>("productservice")
    .WithReference(readPostgres)
    .WithReference(writePostgres)
    .WithReference(rabbitmq)
    .WithReference(redis);

// Order Service
builder.AddProject<Projects.Asspire_OrderService>("orderservice")
    .WithReference(readPostgres)
    .WithReference(writePostgres)
    .WithReference(rabbitmq)
    .WithReference(redis)
    .WithReference(productService);

builder.Build().Run();
