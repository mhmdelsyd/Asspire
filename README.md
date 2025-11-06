# Asspire - Microservices E-Commerce Application

A production-ready microservices application built with .NET 8 and Aspire, demonstrating modern distributed system patterns and best practices.

## 🚀 Features

- **🏗️ Microservices Architecture** - Product and Order services with clear separation of concerns
- **📊 CQRS Pattern** - Command Query Responsibility Segregation for optimal read/write performance
- **⚡ Event-Driven** - RabbitMQ for asynchronous communication and eventual consistency
- **🔄 Read/Write Databases** - Separate PostgreSQL instances for reads and writes
- **💾 Redis Caching** - Distributed caching layer for improved performance
- **🔌 gRPC Communication** - High-performance inter-service communication
- **📈 Full Observability** - OpenTelemetry + Jaeger for distributed tracing
- **🐳 Docker Ready** - Complete Docker Compose setup for easy deployment
- **📝 Auto-Seeding** - 1000 products automatically seeded for testing
- **📚 API Documentation** - Swagger/OpenAPI for all endpoints

## 🏛️ Architecture

```
Product Service ───gRPC───► Order Service
      │                          │
      ├─► PostgreSQL (Write)     │
      ├─► PostgreSQL (Read)      │
      ├─► RabbitMQ ◄─────────────┤
      ├─► Redis                  │
      └─► Jaeger (Tracing)       │
```

### Technology Stack

- **.NET 8** - Latest .NET framework
- **Aspire** - Cloud-native orchestration
- **PostgreSQL** - Relational database (read/write separation)
- **RabbitMQ** - Message broker
- **Redis** - Distributed cache
- **gRPC** - Inter-service communication
- **Entity Framework Core** - ORM
- **MediatR** - CQRS implementation
- **MassTransit** - Message bus abstraction
- **OpenTelemetry** - Observability
- **Jaeger** - Distributed tracing
- **Docker** - Containerization

## 📦 Quick Start

### Prerequisites

- Docker Desktop
- 8GB RAM (16GB recommended)
- Available ports: 5001, 5002, 5432, 5433, 5672, 6379, 15672, 16686

### Get Started in 3 Commands

```bash
# 1. Clone the repository
git clone <repository-url>
cd Asspire

# 2. Start all services
docker-compose up -d

# 3. Test the API
curl http://localhost:5001/api/products?pageNumber=1&pageSize=10
```

**That's it!** 🎉 Your microservices application is now running.

### Access Points

| Service | URL | Description |
|---------|-----|-------------|
| Product API | http://localhost:5001 | Product service REST API |
| Product Swagger | http://localhost:5001/swagger | API documentation |
| Order API | http://localhost:5002 | Order service REST API |
| Order Swagger | http://localhost:5002/swagger | API documentation |
| RabbitMQ UI | http://localhost:15672 | Message broker management (guest/guest) |
| Jaeger UI | http://localhost:16686 | Distributed tracing |

## 📖 Documentation

- **[Architecture Documentation](ARCHITECTURE.md)** - Detailed system architecture and patterns
- **[Docker Setup Guide](DOCKER-SETUP.md)** - Complete Docker setup and troubleshooting

## 🧪 Testing the Application

### Get Products (1000 seeded products)
```bash
curl http://localhost:5001/api/products?pageNumber=1&pageSize=10
```

### Create a Product
```bash
curl -X POST http://localhost:5001/api/products \
  -H "Content-Type: application/json" \
  -d '{
    "name": "Gaming Laptop",
    "description": "High-performance laptop for gaming",
    "price": 1499.99,
    "stockQuantity": 50,
    "category": "Electronics"
  }'
```

### Create an Order (uses gRPC to validate product)
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
curl -X PUT "http://localhost:5002/api/orders/{order-id}/status?status=Confirmed"
```

## 🎯 Key Concepts Demonstrated

### 1. CQRS (Command Query Responsibility Segregation)

**Commands** (writes):
- Write to PostgreSQL Write DB
- Publish events to RabbitMQ
- Handle business logic

**Queries** (reads):
- Read from PostgreSQL Read DB
- Use Redis caching
- Optimized for performance

### 2. Event-Driven Architecture

```
Write Operation → Command → Write DB → Event → RabbitMQ
                                                    ↓
Read DB ← Event Consumer ← Cache Invalidation ←────┘
```

**Events:**
- ProductCreated/Updated/Deleted
- OrderCreated/StatusUpdated

### 3. Microservices Communication

**gRPC** for synchronous calls:
- Order Service validates products via gRPC
- Strong typing with Protocol Buffers
- High performance

**RabbitMQ** for asynchronous events:
- Eventual consistency
- Decoupled services
- Reliable message delivery

### 4. Caching Strategy

**Cache-Aside Pattern:**
1. Check cache first
2. On miss, query database
3. Store in cache
4. Return data

**Invalidation:**
- Event-driven invalidation
- 5-minute TTL

### 5. Observability

**Distributed Tracing:**
- End-to-end request tracking
- Service dependency visualization
- Performance bottleneck identification

**Metrics:**
- Request rates and durations
- Error rates
- Resource utilization

**Logging:**
- Structured logging
- Correlation IDs
- Centralized collection

## 🛠️ Development

### Project Structure

```
Asspire/
├── src/
│   ├── Asspire.AppHost/              # Aspire orchestrator
│   ├── Asspire.ServiceDefaults/      # Shared configuration
│   ├── Asspire.ProductService/       # Product microservice
│   │   ├── Features/
│   │   │   ├── Commands/            # Write operations
│   │   │   └── Queries/             # Read operations
│   │   ├── Data/                    # DbContexts
│   │   ├── Models/                  # Domain models
│   │   ├── Services/                # gRPC services
│   │   ├── IntegrationEvents/       # Event publishers/consumers
│   │   └── Protos/                  # gRPC proto files
│   └── Asspire.OrderService/         # Order microservice
│       └── (similar structure)
├── docker-compose.yml                # Docker orchestration
├── ARCHITECTURE.md                   # Architecture docs
├── DOCKER-SETUP.md                   # Setup guide
└── README.md                         # This file
```

### Building Locally

```bash
# Build all services
docker-compose build

# Start services
docker-compose up -d

# View logs
docker-compose logs -f

# Stop services
docker-compose down
```

### Making Changes

```bash
# Rebuild after code changes
docker-compose up --build -d

# Restart a specific service
docker-compose restart product-service
```

## 📊 Monitoring & Observability

### View Distributed Traces

1. Open Jaeger: http://localhost:16686
2. Select a service (product-service or order-service)
3. Click "Find Traces"
4. Explore request flows and performance

**Example trace shows:**
- HTTP request → gRPC call → Database query → Cache hit
- Timing for each operation
- Error tracking

### Monitor Message Flow

1. Open RabbitMQ: http://localhost:15672
2. Login with guest/guest
3. View Queues tab
4. Monitor message rates and consumers

### Check Health

```bash
# Product Service health
curl http://localhost:5001/health

# Order Service health
curl http://localhost:5002/health
```

## 🐛 Troubleshooting

### Services won't start
```bash
# Check logs
docker-compose logs [service-name]

# Verify ports are free
netstat -an | grep LISTEN | grep -E '5001|5002'

# Clean restart
docker-compose down -v
docker-compose up --build
```

### Database connection issues
```bash
# Check database health
docker-compose ps

# View database logs
docker-compose logs postgres-write
docker-compose logs postgres-read
```

### Performance issues
```bash
# Check resource usage
docker stats

# View all logs
docker-compose logs -f
```

**See [DOCKER-SETUP.md](DOCKER-SETUP.md) for detailed troubleshooting.**

## 🔒 Security Notes

**Current Implementation:**
- Development configuration
- Default credentials
- No authentication/authorization

**For Production:**
- ✅ Implement JWT/OAuth2 authentication
- ✅ Use secrets management (Azure Key Vault, HashiCorp Vault)
- ✅ Enable HTTPS/TLS
- ✅ Add API rate limiting
- ✅ Input validation and sanitization
- ✅ Network policies and segmentation

## 📈 Scalability

**Horizontal Scaling:**
```bash
# Scale product service
docker-compose up -d --scale product-service=3
```

**Database Scaling:**
- Read replicas for PostgreSQL Read
- Connection pooling
- Query optimization

**Cache Scaling:**
- Redis cluster mode
- Cache warming strategies

## 🚢 Deployment

### Docker Compose (Current)
- Local development
- Testing
- Demo environments

### Kubernetes (Production)
- Generate K8s manifests from Aspire
- Helm charts
- Auto-scaling
- Load balancing

### Cloud Deployment
- Azure Container Apps (via Aspire)
- AWS ECS/EKS
- Google Cloud Run

## 📚 API Reference

### Product Service

| Method | Endpoint | Description |
|--------|----------|-------------|
| GET | /api/products | Get paginated products |
| GET | /api/products/{id} | Get product by ID |
| POST | /api/products | Create new product |
| PUT | /api/products/{id} | Update product |
| DELETE | /api/products/{id} | Delete product |

### Order Service

| Method | Endpoint | Description |
|--------|----------|-------------|
| GET | /api/orders | Get paginated orders |
| GET | /api/orders/{id} | Get order by ID |
| POST | /api/orders | Create new order |
| PUT | /api/orders/{id}/status | Update order status |

## 🎓 Learning Resources

- [.NET Aspire Documentation](https://learn.microsoft.com/en-us/dotnet/aspire/)
- [CQRS Pattern](https://martinfowler.com/bliki/CQRS.html)
- [Event-Driven Architecture](https://martinfowler.com/articles/201701-event-driven.html)
- [Microservices Patterns](https://microservices.io/patterns/index.html)
- [gRPC Documentation](https://grpc.io/docs/)

## 🤝 Contributing

1. Fork the repository
2. Create a feature branch
3. Make your changes
4. Test thoroughly
5. Submit a pull request

## 📄 License

This project is licensed under the MIT License.

## 🙏 Acknowledgments

Built with:
- .NET Aspire
- PostgreSQL
- RabbitMQ
- Redis
- gRPC
- OpenTelemetry
- Jaeger

## 📞 Support

- **Documentation**: See [ARCHITECTURE.md](ARCHITECTURE.md) and [DOCKER-SETUP.md](DOCKER-SETUP.md)
- **Issues**: Create an issue in the repository
- **Questions**: Check the troubleshooting section in DOCKER-SETUP.md

---

**Made with ❤️ using .NET 8 and Aspire**

Start building cloud-native applications today! 🚀
