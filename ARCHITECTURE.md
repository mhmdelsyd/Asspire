# Asspire Application - Architecture Documentation

## Overview

Asspire is a microservices-based e-commerce application built with .NET 8 and Aspire. It demonstrates modern distributed system patterns including CQRS, event-driven architecture, service-to-service communication via gRPC, and comprehensive observability.

## Architecture Diagram

```
┌─────────────────────────────────────────────────────────────────┐
│                         Aspire Dashboard                        │
│                 (Monitoring & Distributed Tracing)              │
└─────────────────────────────────────────────────────────────────┘
                                 │
                ┌────────────────┴────────────────┐
                │                                 │
        ┌───────▼────────┐              ┌────────▼────────┐
        │ Product Service│              │  Order Service  │
        │   (Port 5001)  │◄────gRPC────│   (Port 5002)  │
        └────────────────┘              └─────────────────┘
                │                                 │
        ┌───────┴────────┬────────────────────────┴──────┐
        │                │                               │
        │                │                               │
   ┌────▼─────┐    ┌────▼──────┐              ┌─────────▼────┐
   │PostgreSQL│    │PostgreSQL │              │   RabbitMQ   │
   │  Write   │    │   Read    │              │ Message Bus  │
   │(Port 5432)│   │(Port 5433)│              │ (Port 5672)  │
   └──────────┘    └───────────┘              └──────────────┘
        │                │                           │
        └────────────────┴───────┬───────────────────┘
                                 │
                          ┌──────▼──────┐
                          │    Redis    │
                          │ Cache Layer │
                          │ (Port 6379) │
                          └─────────────┘
```

## Core Components

### 1. Product Service

**Responsibilities:**
- Manage product catalog (CRUD operations)
- Expose product data via REST API and gRPC
- Maintain product inventory
- Seed database with 1000 products for testing

**Technology Stack:**
- .NET 8 Web API
- Entity Framework Core
- PostgreSQL (separate read/write databases)
- gRPC for inter-service communication
- MediatR for CQRS pattern
- MassTransit + RabbitMQ for event publishing
- Redis for caching

**Endpoints:**
- `GET /api/products` - Get paginated products
- `GET /api/products/{id}` - Get product by ID
- `POST /api/products` - Create new product
- `PUT /api/products/{id}` - Update product
- `DELETE /api/products/{id}` - Delete product

**gRPC Services:**
- `GetProduct` - Retrieve single product
- `GetProducts` - Retrieve products with pagination
- `CheckProductAvailability` - Check stock availability

### 2. Order Service

**Responsibilities:**
- Manage customer orders
- Validate product availability via gRPC calls to Product Service
- Handle order lifecycle (Pending → Confirmed → Completed/Cancelled)
- Expose order management via REST API

**Technology Stack:**
- .NET 8 Web API
- Entity Framework Core
- PostgreSQL (separate read/write databases)
- gRPC Client for Product Service
- MediatR for CQRS pattern
- MassTransit + RabbitMQ for event publishing
- Redis for caching

**Endpoints:**
- `GET /api/orders` - Get paginated orders
- `GET /api/orders/{id}` - Get order by ID
- `POST /api/orders` - Create new order (validates product via gRPC)
- `PUT /api/orders/{id}/status` - Update order status

## Architecture Patterns

### CQRS (Command Query Responsibility Segregation)

Both services implement CQRS using MediatR:

**Commands (Write Operations):**
- Write to the write database (PostgreSQL Write)
- Publish integration events to RabbitMQ
- Handle business logic and validations

**Queries (Read Operations):**
- Read from the read database (PostgreSQL Read)
- Utilize Redis caching for frequently accessed data
- Optimized for read performance

**Benefits:**
- Scalability: Read and write databases can be scaled independently
- Performance: Queries are optimized for read operations
- Flexibility: Different data models for reads and writes

### Event-Driven Architecture

**Event Flow:**

1. **Write Operation** (e.g., Create Product):
   ```
   API Request → Command Handler → Write DB → Publish Event → RabbitMQ
   ```

2. **Event Consumption**:
   ```
   RabbitMQ → Event Consumer → Update Read DB → Invalidate Cache
   ```

**Integration Events:**

**Product Service:**
- `ProductCreatedEvent`
- `ProductUpdatedEvent`
- `ProductDeletedEvent`

**Order Service:**
- `OrderCreatedEvent`
- `OrderStatusUpdatedEvent`

**Benefits:**
- Eventual consistency between read and write databases
- Decoupling of services
- Asynchronous processing
- Event sourcing capability

### Service Communication

**gRPC for Synchronous Communication:**
- Order Service calls Product Service via gRPC to:
  - Validate product availability
  - Fetch product details
  - Check stock levels

**Advantages of gRPC:**
- High performance (binary protocol)
- Strong typing with Protocol Buffers
- Built-in code generation
- HTTP/2 multiplexing

### Caching Strategy

**Redis Caching Implementation:**

**Cache-Aside Pattern:**
1. Check cache first
2. If miss, query database
3. Store result in cache
4. Return data

**Cache Invalidation:**
- Cache is invalidated when events are consumed
- TTL: 5 minutes for automatic expiration

**Cached Entities:**
- Individual products (`product:{id}`)
- Individual orders (`order:{id}`)

## Data Storage

### PostgreSQL Write Database

**Purpose:** Handle all write operations

**Schema:**
- Products table (Product Service)
- Orders table (Order Service)

**Characteristics:**
- ACID compliance
- Strong consistency
- Transaction support

### PostgreSQL Read Database

**Purpose:** Optimized for read operations

**Schema:**
- Mirror of write database
- Denormalized for query performance

**Sync Mechanism:**
- Event-driven synchronization via RabbitMQ
- Near real-time consistency

**Benefits:**
- Read scalability
- Query optimization without affecting writes
- Geographic distribution capability

## Message Broker (RabbitMQ)

**Responsibilities:**
- Event distribution
- Asynchronous communication
- Database synchronization
- Decoupling services

**Configuration:**
- Exchange: Default (topic)
- Queues: Auto-created by MassTransit
- Durability: Persistent messages
- Acknowledgments: Automatic

**Management Interface:**
- URL: http://localhost:15672
- Credentials: guest/guest

## Observability & Monitoring

### Distributed Tracing (OpenTelemetry + Jaeger)

**Instrumentation:**
- ASP.NET Core requests
- HTTP client calls
- gRPC calls
- Database operations

**Jaeger UI:**
- URL: http://localhost:16686
- Features:
  - End-to-end request tracing
  - Service dependency graph
  - Performance metrics
  - Error tracking

**Trace Propagation:**
- W3C Trace Context standard
- Automatic context propagation across services

### Metrics

**Collected Metrics:**
- ASP.NET Core metrics (request rate, duration, errors)
- Runtime metrics (GC, memory, threads)
- HTTP client metrics
- Custom business metrics

**Export:**
- OTLP protocol to Aspire Dashboard
- Real-time metric visualization

### Logging

**Structured Logging:**
- JSON format
- Correlation IDs
- Service name tagging
- Log levels (Information, Warning, Error)

**Integration:**
- OpenTelemetry logging
- Centralized log collection
- Log correlation with traces

### Health Checks

**Endpoints:**
- `/health` - Overall health status
- `/alive` - Liveness probe

**Checks:**
- Database connectivity
- Service availability
- Dependencies health

## Aspire Dashboard

**.NET Aspire** provides a unified dashboard for:
- Service discovery
- Resource management
- Real-time monitoring
- Distributed tracing visualization
- Logs aggregation
- Metrics dashboards

**Features:**
- Automatic service discovery
- Connection string management
- Environment configuration
- Container orchestration

## Database Seeding

**Product Service Seeding:**
- Automatically seeds 1000 products on startup
- Data includes:
  - Random names (Product 1 - Product 1000)
  - Descriptions
  - Prices ($10 - $1010)
  - Stock quantities (0 - 500)
  - 8 categories (Electronics, Clothing, Food, Books, Toys, Sports, Home, Garden)

**Seeding Strategy:**
- Batch insert (100 products at a time)
- Seeds both write and read databases
- Idempotent (only seeds if database is empty)

## Security Considerations

**Current Implementation:**
- Development environment configuration
- Basic authentication for infrastructure components
- No production-grade security implemented

**Production Recommendations:**
- Implement authentication/authorization (JWT, OAuth2)
- Use secrets management (Azure Key Vault, HashiCorp Vault)
- Enable HTTPS/TLS
- Network segmentation
- API rate limiting
- Input validation and sanitization
- SQL injection prevention (using EF Core parameterized queries)

## Scalability

**Horizontal Scaling:**
- Stateless services can be scaled horizontally
- Load balancer required for multiple instances
- Redis for distributed caching
- Database read replicas for read scaling

**Vertical Scaling:**
- Increase resources for databases
- Optimize queries and indexes
- Connection pooling

**Database Sharding:**
- Partition data by category or region
- Multiple write databases
- Aggregated read databases

## Deployment

**Container Orchestration:**
- Docker Compose for local development
- Kubernetes for production
- Aspire manifest for cloud deployment

**CI/CD Pipeline:**
- Build: Docker multi-stage builds
- Test: Unit, integration, and E2E tests
- Deploy: Rolling updates with health checks

## Technology Stack Summary

| Component | Technology | Version |
|-----------|------------|---------|
| Framework | .NET | 8.0 |
| Orchestrator | Aspire | 8.2.1 |
| API Framework | ASP.NET Core | 8.0 |
| ORM | Entity Framework Core | 8.0 |
| CQRS | MediatR | 12.4.1 |
| Message Broker | RabbitMQ | 3 (Management) |
| Messaging Library | MassTransit | 8.2.5 |
| Database | PostgreSQL | 16 |
| Cache | Redis | 7 (Alpine) |
| RPC | gRPC | 2.65.0 |
| Tracing | Jaeger | Latest |
| Telemetry | OpenTelemetry | 1.9.0 |
| Containerization | Docker | Latest |
| API Documentation | Swagger/OpenAPI | 6.8.1 |

## API Examples

### Create Product
```bash
curl -X POST http://localhost:5001/api/products \
  -H "Content-Type: application/json" \
  -d '{
    "name": "Laptop",
    "description": "High-performance laptop",
    "price": 999.99,
    "stockQuantity": 50,
    "category": "Electronics"
  }'
```

### Get Products
```bash
curl http://localhost:5001/api/products?pageNumber=1&pageSize=10
```

### Create Order
```bash
curl -X POST http://localhost:5002/api/orders \
  -H "Content-Type: application/json" \
  -d '{
    "productId": 1,
    "quantity": 2,
    "customerName": "John Doe",
    "customerEmail": "john@example.com"
  }'
```

### Update Order Status
```bash
curl -X PUT http://localhost:5002/api/orders/{orderId}/status?status=Confirmed
```

## Troubleshooting

### Service Connection Issues
- Verify all containers are running: `docker-compose ps`
- Check service logs: `docker-compose logs [service-name]`
- Ensure network connectivity: `docker network inspect asspire_asspire-network`

### Database Synchronization Issues
- Check RabbitMQ management UI for message flow
- Verify consumers are registered
- Check service logs for event processing errors

### Performance Issues
- Monitor Redis cache hit ratio
- Analyze slow queries in PostgreSQL
- Use Jaeger to identify bottlenecks
- Check resource utilization: `docker stats`

## Future Enhancements

1. **API Gateway** - Centralized entry point (Ocelot, YARP)
2. **Authentication** - Identity service with JWT
3. **Payment Service** - Payment processing integration
4. **Notification Service** - Email/SMS notifications
5. **Saga Pattern** - Distributed transaction management
6. **Event Sourcing** - Complete event history
7. **GraphQL** - Flexible query API
8. **Rate Limiting** - API throttling
9. **Circuit Breaker** - Resilience patterns (Polly)
10. **API Versioning** - Version management

## References

- [.NET Aspire Documentation](https://learn.microsoft.com/en-us/dotnet/aspire/)
- [MediatR Documentation](https://github.com/jbogard/MediatR)
- [MassTransit Documentation](https://masstransit.io/)
- [gRPC Documentation](https://grpc.io/docs/)
- [OpenTelemetry .NET](https://opentelemetry.io/docs/languages/net/)
