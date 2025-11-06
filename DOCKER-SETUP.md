# Asspire - Docker Setup & Quick Start Guide

This guide will help you get the Asspire application up and running using Docker.

## Prerequisites

Before you begin, ensure you have the following installed on your system:

- **Docker** (version 20.10 or later)
  - Download: https://www.docker.com/products/docker-desktop
  - Verify installation: `docker --version`

- **Docker Compose** (version 2.0 or later)
  - Usually included with Docker Desktop
  - Verify installation: `docker-compose --version`

- **System Requirements:**
  - RAM: At least 8GB (16GB recommended)
  - Disk Space: At least 10GB free
  - Ports: Ensure the following ports are available:
    - 5001 (Product Service)
    - 5002 (Order Service)
    - 5432 (PostgreSQL Write)
    - 5433 (PostgreSQL Read)
    - 5672 (RabbitMQ)
    - 15672 (RabbitMQ Management UI)
    - 6379 (Redis)
    - 16686 (Jaeger UI)

## Quick Start (First Time Setup)

### Step 1: Download the Application

```bash
# Clone the repository
git clone <repository-url>
cd Asspire
```

### Step 2: Build and Start All Services

```bash
# Build and start all containers
docker-compose up --build
```

This command will:
1. Build Docker images for Product Service and Order Service
2. Pull required images (PostgreSQL, RabbitMQ, Redis, Jaeger)
3. Create a Docker network
4. Start all services
5. Automatically seed 1000 products in the database

**Expected Output:**
```
[+] Building ...
[+] Running 7/7
 ✔ Network asspire_asspire-network    Created
 ✔ Container postgres-write           Started
 ✔ Container postgres-read            Started
 ✔ Container rabbitmq                 Started
 ✔ Container redis                    Started
 ✔ Container jaeger                   Started
 ✔ Container product-service          Started
 ✔ Container order-service            Started
```

**Initial Startup Time:** 2-5 minutes (first time)

### Step 3: Verify Services are Running

```bash
# Check all containers are up and healthy
docker-compose ps
```

All services should show status as "Up" or "healthy".

### Step 4: Access the Application

**Service Endpoints:**
- **Product Service API**: http://localhost:5001
- **Product Service Swagger**: http://localhost:5001/swagger
- **Order Service API**: http://localhost:5002
- **Order Service Swagger**: http://localhost:5002/swagger

**Infrastructure UIs:**
- **RabbitMQ Management**: http://localhost:15672
  - Username: `guest`
  - Password: `guest`
- **Jaeger Tracing UI**: http://localhost:16686

**Health Checks:**
- Product Service: http://localhost:5001/health
- Order Service: http://localhost:5002/health

## Common Operations

### Starting Services (After Initial Setup)

```bash
# Start all services in the background
docker-compose up -d
```

### Stopping Services

```bash
# Stop all services
docker-compose down
```

### Stopping and Removing All Data

```bash
# Stop services and remove volumes (deletes all data!)
docker-compose down -v
```

### Viewing Logs

```bash
# View logs from all services
docker-compose logs -f

# View logs from a specific service
docker-compose logs -f product-service
docker-compose logs -f order-service
docker-compose logs -f rabbitmq
```

### Restarting a Single Service

```bash
# Restart a specific service
docker-compose restart product-service
```

### Rebuilding After Code Changes

```bash
# Rebuild and restart services
docker-compose up --build -d
```

## Testing the Application

### 1. Get All Products (Should return 1000 seeded products)

```bash
curl http://localhost:5001/api/products?pageNumber=1&pageSize=10
```

**Expected Response:**
```json
{
  "products": [
    {
      "id": 1,
      "name": "Product 1",
      "description": "Description for product 1...",
      "price": 125.45,
      "stockQuantity": 234,
      "category": "Electronics",
      ...
    },
    ...
  ],
  "totalCount": 1000
}
```

### 2. Get a Specific Product

```bash
curl http://localhost:5001/api/products/1
```

### 3. Create a New Product

```bash
curl -X POST http://localhost:5001/api/products \
  -H "Content-Type: application/json" \
  -d '{
    "name": "New Laptop",
    "description": "High-performance gaming laptop",
    "price": 1299.99,
    "stockQuantity": 25,
    "category": "Electronics"
  }'
```

### 4. Create an Order (Tests gRPC communication)

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

This will:
- Call Product Service via gRPC to validate product availability
- Create an order if product is available
- Publish an event to RabbitMQ
- Sync the order to the read database

### 5. Get All Orders

```bash
curl http://localhost:5002/api/orders?pageNumber=1&pageSize=10
```

### 6. Update Order Status

```bash
curl -X PUT "http://localhost:5002/api/orders/{order-id}/status?status=Confirmed"
```

## Monitoring & Observability

### RabbitMQ Management Console

1. Open: http://localhost:15672
2. Login with `guest` / `guest`
3. Check:
   - **Overview** - System health and message rates
   - **Queues** - Message queues and consumers
   - **Exchanges** - Message routing

### Jaeger Distributed Tracing

1. Open: http://localhost:16686
2. Select a service (product-service or order-service)
3. Click "Find Traces" to view request traces
4. Features:
   - End-to-end request tracking
   - Service dependency graph
   - Performance bottleneck identification
   - Error tracking

**Example Trace:**
```
order-service: POST /api/orders (350ms)
  ├─ gRPC: CheckProductAvailability (45ms)
  │   └─ product-service: Database Query (20ms)
  ├─ gRPC: GetProduct (35ms)
  │   └─ product-service: Redis Cache Hit (5ms)
  ├─ Database Insert (80ms)
  └─ RabbitMQ Publish (15ms)
```

## Troubleshooting

### Issue: Containers Won't Start

**Solution:**
```bash
# Check for port conflicts
netstat -an | grep LISTEN | grep -E '5001|5002|5432|5433|5672|6379|15672|16686'

# Stop any conflicting services
# Then restart
docker-compose down
docker-compose up -d
```

### Issue: Services Can't Connect to Database

**Check Database Health:**
```bash
# Check PostgreSQL logs
docker-compose logs postgres-write
docker-compose logs postgres-read

# Connect to database manually
docker exec -it postgres-write psql -U postgres -d writedb
```

**Solution:**
- Wait for databases to fully initialize (check health status)
- Restart dependent services: `docker-compose restart product-service order-service`

### Issue: RabbitMQ Connection Failures

**Check RabbitMQ Status:**
```bash
docker-compose logs rabbitmq
docker exec -it rabbitmq rabbitmq-diagnostics status
```

**Solution:**
```bash
# Restart RabbitMQ
docker-compose restart rabbitmq

# Wait 30 seconds for initialization
sleep 30

# Restart services
docker-compose restart product-service order-service
```

### Issue: Redis Connection Errors

```bash
# Check Redis status
docker exec -it redis redis-cli ping
# Should return: PONG

# View Redis logs
docker-compose logs redis
```

### Issue: Database Not Seeding

**Check Product Service Logs:**
```bash
docker-compose logs product-service | grep -i seed
```

**Manual Database Check:**
```bash
# Connect to write database
docker exec -it postgres-write psql -U postgres -d writedb

# Check product count
\c writedb
SELECT COUNT(*) FROM "Products";
# Should return 1000

# Exit
\q
```

**Force Re-seed:**
```bash
# Delete all data and restart
docker-compose down -v
docker-compose up --build
```

### Issue: Build Fails

**Common Causes:**
1. Insufficient disk space
2. Network issues downloading packages
3. Corrupted Docker cache

**Solution:**
```bash
# Clean Docker build cache
docker builder prune -a

# Remove old images
docker image prune -a

# Rebuild
docker-compose build --no-cache
docker-compose up -d
```

### Issue: Slow Performance

**Check Resource Usage:**
```bash
# View container resource usage
docker stats
```

**Solution:**
- Increase Docker Desktop memory allocation (Settings → Resources → Memory)
- Close unnecessary applications
- Check disk I/O performance

### View All Container Logs in Real-Time

```bash
docker-compose logs -f --tail=100
```

## Advanced Configuration

### Changing Database Credentials

Edit `docker-compose.yml`:
```yaml
environment:
  POSTGRES_USER: your_username
  POSTGRES_PASSWORD: your_password
```

Then update connection strings in both service environment variables.

### Scaling Services

```bash
# Run multiple instances of Product Service
docker-compose up -d --scale product-service=3
```

**Note:** Requires a load balancer for proper distribution.

### Persistent Data Locations

Docker volumes are used for data persistence:
- `postgres-write-data` - Write database data
- `postgres-read-data` - Read database data
- `rabbitmq-data` - RabbitMQ messages and configuration
- `redis-data` - Redis cache data

**View Volumes:**
```bash
docker volume ls | grep asspire
```

**Backup a Volume:**
```bash
docker run --rm -v asspire_postgres-write-data:/data \
  -v $(pwd):/backup ubuntu tar czf /backup/postgres-write-backup.tar.gz /data
```

## Production Considerations

**Security:**
- Change default credentials
- Use secrets management
- Enable TLS/SSL
- Implement authentication and authorization
- Network segmentation

**Monitoring:**
- Set up centralized logging (ELK, Splunk)
- Configure alerting (Prometheus + Alertmanager)
- Performance monitoring (Application Insights, DataDog)

**High Availability:**
- Use Kubernetes for orchestration
- Database replication and failover
- Load balancing
- Auto-scaling policies

**Backup Strategy:**
- Automated database backups
- Point-in-time recovery
- Disaster recovery plan

## Clean Up

### Remove All Containers and Volumes

```bash
# Stop and remove everything including data
docker-compose down -v

# Remove images
docker rmi $(docker images -q 'asspire*')

# Remove network
docker network rm asspire_asspire-network
```

### Remove Only Containers (Keep Data)

```bash
docker-compose down
```

## Getting Help

### Check Service Health

```bash
# Product Service
curl http://localhost:5001/health

# Order Service
curl http://localhost:5002/health
```

### View Swagger Documentation

- Product Service: http://localhost:5001/swagger
- Order Service: http://localhost:5002/swagger

### Access Container Shell

```bash
# Product Service
docker exec -it product-service /bin/sh

# PostgreSQL
docker exec -it postgres-write psql -U postgres
```

## System Architecture Summary

```
┌──────────────────────────────────────────────────────────────┐
│  YOUR MACHINE                                                 │
│                                                               │
│  ┌─────────────────────────────────────────────────────┐    │
│  │  Docker Network (asspire-network)                    │    │
│  │                                                      │    │
│  │  Product Service ─────gRPC────► Order Service       │    │
│  │       │                              │               │    │
│  │       ├─► PostgreSQL Write           │               │    │
│  │       ├─► PostgreSQL Read            │               │    │
│  │       ├─► RabbitMQ ◄─────────────────┤               │    │
│  │       └─► Redis                      │               │    │
│  │                                      │               │    │
│  │  Jaeger (Tracing)                    │               │    │
│  └──────────────────────────────────────────────────────┘    │
│                                                               │
│  Exposed Ports:                                               │
│  - 5001: Product API                                          │
│  - 5002: Order API                                            │
│  - 15672: RabbitMQ UI                                         │
│  - 16686: Jaeger UI                                           │
└──────────────────────────────────────────────────────────────┘
```

## Next Steps

1. ✅ Start the application: `docker-compose up -d`
2. ✅ Verify health: `docker-compose ps`
3. ✅ Test APIs: Use curl or Swagger UI
4. ✅ Monitor: Check Jaeger and RabbitMQ UIs
5. ✅ Explore: Read the ARCHITECTURE.md file

## Useful Docker Commands Cheat Sheet

```bash
# Start services
docker-compose up -d

# Stop services
docker-compose down

# View logs
docker-compose logs -f [service-name]

# Restart service
docker-compose restart [service-name]

# Rebuild service
docker-compose up --build [service-name]

# Check status
docker-compose ps

# View resource usage
docker stats

# Execute command in container
docker exec -it [container-name] [command]

# Clean up
docker-compose down -v
docker system prune -a
```

## Support

For issues and questions:
1. Check the ARCHITECTURE.md documentation
2. Review Docker Compose logs
3. Verify all prerequisites are met
4. Check port availability

Happy coding! 🚀
