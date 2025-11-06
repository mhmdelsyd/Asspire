# API Gateway & Frontend Guide

This document provides detailed information about the API Gateway and React Frontend components of the Asspire application.

## Table of Contents

- [API Gateway](#api-gateway)
  - [Overview](#overview)
  - [YARP Configuration](#yarp-configuration)
  - [Routes](#routes)
  - [Usage Examples](#usage-examples)
- [React Frontend](#react-frontend)
  - [Features](#features)
  - [Architecture](#architecture)
  - [Components](#components)
  - [API Integration](#api-integration)
- [Deployment](#deployment)

---

## API Gateway

### Overview

The API Gateway serves as the single entry point for all client requests, providing:

- **Unified API Interface** - Single endpoint for all services
- **Reverse Proxy** - Routes requests to appropriate microservices
- **CORS Support** - Enables cross-origin requests from the frontend
- **Load Balancing** - Distributes traffic (when services are scaled)
- **Service Discovery** - Automatic service location via Aspire
- **Observability** - Integrated OpenTelemetry tracing

**Technology:** YARP (Yet Another Reverse Proxy) - Microsoft's high-performance reverse proxy

**Port:** 5000

### YARP Configuration

The gateway uses YARP for intelligent request routing. Configuration is defined in `appsettings.json`:

```json
{
  "ReverseProxy": {
    "Routes": {
      "products-route": {
        "ClusterId": "product-service",
        "Match": {
          "Path": "/api/products/{**catch-all}"
        }
      },
      "orders-route": {
        "ClusterId": "order-service",
        "Match": {
          "Path": "/api/orders/{**catch-all}"
        }
      }
    },
    "Clusters": {
      "product-service": {
        "Destinations": {
          "destination1": {
            "Address": "http://product-service:8080"
          }
        }
      },
      "order-service": {
        "Destinations": {
          "destination1": {
            "Address": "http://order-service:8080"
          }
        }
      }
    }
  }
}
```

### Routes

The Gateway exposes the following routes:

#### Product Routes

| Method | Path | Forwards To | Description |
|--------|------|-------------|-------------|
| GET | `/api/products` | Product Service | Get paginated products |
| GET | `/api/products/{id}` | Product Service | Get product by ID |
| POST | `/api/products` | Product Service | Create new product |
| PUT | `/api/products/{id}` | Product Service | Update product |
| DELETE | `/api/products/{id}` | Product Service | Delete product |

#### Order Routes

| Method | Path | Forwards To | Description |
|--------|------|-------------|-------------|
| GET | `/api/orders` | Order Service | Get paginated orders |
| GET | `/api/orders/{id}` | Order Service | Get order by ID |
| POST | `/api/orders` | Order Service | Create new order |
| PUT | `/api/orders/{id}/status` | Order Service | Update order status |

#### Health Check Routes

| Path | Forwards To | Description |
|------|-------------|-------------|
| `/health/products` | Product Service `/health` | Product service health |
| `/health/orders` | Order Service `/health` | Order service health |

### Usage Examples

All requests should go through the Gateway (Port 5000):

#### Get Products
```bash
curl http://localhost:5000/api/products?pageNumber=1&pageSize=10
```

#### Create Product
```bash
curl -X POST http://localhost:5000/api/products \
  -H "Content-Type: application/json" \
  -d '{
    "name": "Gaming Mouse",
    "description": "RGB gaming mouse with 16000 DPI",
    "price": 79.99,
    "stockQuantity": 150,
    "category": "Electronics"
  }'
```

#### Create Order
```bash
curl -X POST http://localhost:5000/api/orders \
  -H "Content-Type: application/json" \
  -d '{
    "productId": 1,
    "quantity": 2,
    "customerName": "Jane Doe",
    "customerEmail": "jane@example.com"
  }'
```

#### Check Service Health
```bash
curl http://localhost:5000/health/products
curl http://localhost:5000/health/orders
```

### Benefits of the Gateway

1. **Decoupling** - Frontend doesn't need to know about individual services
2. **Security** - Single point for authentication/authorization
3. **Simplified Client** - One base URL instead of multiple
4. **Cross-Cutting Concerns** - Logging, rate limiting, etc. in one place
5. **Service Evolution** - Change backend without affecting clients
6. **Protocol Translation** - Can translate between protocols if needed

---

## React Frontend

### Features

The React frontend provides a full-featured web interface:

- **Product Management**
  - View paginated product list
  - Add new products
  - Delete products
  - Search and filter

- **Order Management**
  - Create orders with product validation
  - View order history
  - Update order status
  - Track order lifecycle

- **User Experience**
  - Responsive design (mobile-friendly)
  - Real-time updates
  - Error handling
  - Loading states
  - Modern, clean UI

**Port:** 3000

### Architecture

```
frontend/
├── public/
│   └── index.html              # HTML template
├── src/
│   ├── components/
│   │   ├── ProductList.js      # Product listing component
│   │   ├── ProductForm.js      # Add product form
│   │   ├── OrderForm.js        # Create order form
│   │   └── OrderList.js        # Order listing component
│   ├── services/
│   │   └── api.js              # API client (Axios)
│   ├── App.js                  # Main application component
│   ├── App.css                 # Application styles
│   ├── index.js                # React entry point
│   └── index.css               # Global styles
├── nginx.conf                  # Nginx configuration
├── Dockerfile                  # Multi-stage Docker build
└── package.json                # Dependencies
```

### Components

#### ProductList Component

Displays paginated products with:
- Grid layout
- Product details (name, description, price, stock, category)
- Action buttons (Order, Delete)
- Pagination controls
- Error handling

**Features:**
- Auto-refresh capability
- Cached data from backend (via Redis)
- Optimistic UI updates

#### ProductForm Component

Form for creating new products:
- Input validation
- Category dropdown
- Price and quantity inputs
- Success/error feedback
- Form reset after submission

#### OrderForm Component

Order creation interface:
- Product selection display
- Quantity selector (validates against stock)
- Customer information inputs
- Price calculation
- gRPC validation (backend)

**Flow:**
1. User selects product from ProductList
2. Form displays product details
3. User enters quantity and customer info
4. Backend validates via gRPC call to Product Service
5. Order created if product available

#### OrderList Component

Displays orders with:
- Table layout
- Order details (ID, product, quantity, price, status)
- Status badges (color-coded)
- Action buttons (Confirm, Cancel, Complete)
- Pagination

**Order Statuses:**
- **Pending** (Yellow) - Initial state
- **Confirmed** (Green) - Order confirmed
- **Completed** (Blue) - Order fulfilled
- **Cancelled** (Red) - Order cancelled

### API Integration

The frontend communicates with the backend through the API Gateway.

#### API Service (`services/api.js`)

Centralized API client using Axios:

```javascript
const API_BASE_URL = process.env.REACT_APP_API_URL || 'http://localhost:5000';

export const productApi = {
  getAll: (pageNumber, pageSize) =>
    api.get(`/api/products?pageNumber=${pageNumber}&pageSize=${pageSize}`),
  getById: (id) =>
    api.get(`/api/products/${id}`),
  create: (product) =>
    api.post('/api/products', product),
  update: (id, product) =>
    api.put(`/api/products/${id}`, product),
  delete: (id) =>
    api.delete(`/api/products/${id}`)
};

export const orderApi = {
  getAll: (pageNumber, pageSize) =>
    api.get(`/api/orders?pageNumber=${pageNumber}&pageSize=${pageSize}`),
  getById: (id) =>
    api.get(`/api/orders/${id}`),
  create: (order) =>
    api.post('/api/orders', order),
  updateStatus: (id, status) =>
    api.put(`/api/orders/${id}/status?status=${status}`)
};
```

#### Environment Configuration

Configure API URL via environment variable:

```bash
# Development
REACT_APP_API_URL=http://localhost:5000

# Production
REACT_APP_API_URL=https://api.your-domain.com
```

### Nginx Configuration

The production build uses Nginx for:

- **Static File Serving** - Optimized delivery of React build
- **API Proxying** - Routes `/api/*` to Gateway
- **React Router Support** - SPA fallback routing
- **Gzip Compression** - Reduced payload sizes
- **Security Headers** - XSS protection, content sniffing prevention

**Key Configuration:**

```nginx
# React Router support
location / {
    try_files $uri $uri/ /index.html;
}

# API proxy to gateway
location /api/ {
    proxy_pass http://gateway:8080;
    proxy_set_header Host $host;
    proxy_set_header X-Real-IP $remote_addr;
}
```

### Styling

Modern, professional design with:
- **Gradient Headers** - Purple theme
- **Card-Based Layout** - Clean, organized UI
- **Responsive Grid** - Adapts to screen size
- **Hover Effects** - Interactive feedback
- **Status Colors** - Visual order states
- **Loading States** - User feedback
- **Error Displays** - Clear error messages

**Color Scheme:**
- Primary: Purple gradient (#667eea to #764ba2)
- Success: Green (#28a745)
- Warning: Yellow (#ffc107)
- Danger: Red (#dc3545)
- Info: Blue (#17a2b8)

---

## Deployment

### Development

```bash
# Start all services
docker-compose up -d

# View frontend logs
docker-compose logs -f frontend

# View gateway logs
docker-compose logs -f gateway

# Rebuild after changes
docker-compose up --build -d frontend gateway
```

### Production Considerations

#### Frontend

1. **Environment Variables**
   ```bash
   REACT_APP_API_URL=https://api.production.com
   ```

2. **Build Optimization**
   - Code splitting
   - Tree shaking
   - Minification (automatic)
   - Gzip compression (Nginx)

3. **CDN Integration**
   - Serve static assets from CDN
   - Cache busting via hashing

4. **Performance**
   - Lazy loading
   - Image optimization
   - HTTP/2 support

#### Gateway

1. **Scalability**
   ```yaml
   gateway:
     deploy:
       replicas: 3
   ```

2. **Security**
   - Add authentication middleware
   - Implement rate limiting
   - Enable HTTPS/TLS
   - Set up API keys

3. **Monitoring**
   - Health checks
   - Request metrics
   - Error tracking
   - Performance monitoring

4. **Load Balancing**
   - Multiple gateway instances
   - Round-robin routing
   - Health-based routing

### Docker Build Process

#### Frontend Dockerfile

Multi-stage build for optimized image:

**Stage 1: Build**
```dockerfile
FROM node:18-alpine AS build
WORKDIR /app
COPY package*.json ./
RUN npm install
COPY . .
RUN npm run build
```

**Stage 2: Production**
```dockerfile
FROM nginx:alpine
COPY --from=build /app/build /usr/share/nginx/html
COPY nginx.conf /etc/nginx/conf.d/default.conf
EXPOSE 80
CMD ["nginx", "-g", "daemon off;"]
```

**Benefits:**
- Small final image (~20MB)
- No development dependencies
- Optimized for production

#### Gateway Dockerfile

.NET multi-stage build:

**Stage 1: Build**
```dockerfile
FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
WORKDIR /src
COPY . .
RUN dotnet publish -c Release
```

**Stage 2: Runtime**
```dockerfile
FROM mcr.microsoft.com/dotnet/aspnet:8.0
COPY --from=build /app/publish .
ENTRYPOINT ["dotnet", "Asspire.Gateway.dll"]
```

---

## Testing

### Frontend Testing

```bash
# Run frontend locally (without Docker)
cd frontend
npm install
npm start

# Test API integration
REACT_APP_API_URL=http://localhost:5000 npm start
```

### Gateway Testing

```bash
# Test gateway routing
curl http://localhost:5000/api/products
curl http://localhost:5000/api/orders

# Test health checks
curl http://localhost:5000/health/products
curl http://localhost:5000/health/orders

# View Swagger documentation
open http://localhost:5000/swagger
```

### End-to-End Testing

1. **Create Product via Frontend**
   - Open http://localhost:3000
   - Click "Add Product"
   - Fill form and submit
   - Verify in product list

2. **Create Order via Frontend**
   - Select a product
   - Click "Order" button
   - Fill customer details
   - Submit order
   - Verify in orders list

3. **Update Order Status**
   - Open Orders tab
   - Find pending order
   - Click "Confirm"
   - Verify status change

4. **Monitor via Jaeger**
   - Open http://localhost:16686
   - Search for traces
   - View request flow: Frontend → Gateway → Service → Database

---

## Troubleshooting

### Frontend Issues

**Problem:** "Cannot connect to API"

**Solution:**
```bash
# Check gateway is running
docker ps | grep gateway

# Check API URL configuration
docker exec -it frontend env | grep REACT_APP_API_URL

# Test gateway from frontend container
docker exec -it frontend wget -O- http://gateway:8080/api/products
```

**Problem:** "CORS error"

**Solution:**
- Verify gateway CORS configuration
- Check that requests go through gateway, not directly to services

### Gateway Issues

**Problem:** "502 Bad Gateway"

**Solution:**
```bash
# Check backend services are running
docker-compose ps

# Check gateway logs
docker-compose logs gateway

# Verify service names in configuration
docker exec -it gateway cat /app/appsettings.json
```

**Problem:** "Route not found"

**Solution:**
- Check YARP configuration in appsettings.json
- Verify path patterns match
- Test direct service access to isolate issue

---

## Best Practices

### Frontend

1. **Error Handling** - Always handle API errors gracefully
2. **Loading States** - Show spinners during async operations
3. **Input Validation** - Validate on client before sending to server
4. **User Feedback** - Confirm successful operations
5. **Accessibility** - Use semantic HTML and ARIA labels

### Gateway

1. **Timeout Configuration** - Set appropriate timeouts
2. **Retry Logic** - Implement retries for transient failures
3. **Circuit Breaker** - Prevent cascading failures
4. **Request Logging** - Log all gateway requests
5. **Health Checks** - Monitor backend service health

---

## Future Enhancements

### Gateway

- [ ] Authentication & Authorization (JWT)
- [ ] Rate Limiting
- [ ] API Versioning
- [ ] Request/Response Transformation
- [ ] WebSocket Support
- [ ] GraphQL Gateway

### Frontend

- [ ] User Authentication
- [ ] Shopping Cart
- [ ] Product Search & Filtering
- [ ] Real-time Updates (SignalR)
- [ ] Admin Dashboard
- [ ] Analytics & Reporting
- [ ] Progressive Web App (PWA)

---

## Resources

- [YARP Documentation](https://microsoft.github.io/reverse-proxy/)
- [React Documentation](https://react.dev/)
- [Nginx Configuration](https://nginx.org/en/docs/)
- [Axios Documentation](https://axios-http.com/)
- [API Gateway Pattern](https://microservices.io/patterns/apigateway.html)

---

**Note:** This application demonstrates a complete full-stack microservices architecture with a modern frontend, making it suitable for learning and production use cases.
