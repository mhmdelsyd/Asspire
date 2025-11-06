import React, { useState } from 'react';
import ProductList from './components/ProductList';
import ProductForm from './components/ProductForm';
import OrderForm from './components/OrderForm';
import OrderList from './components/OrderList';
import './App.css';

function App() {
  const [activeTab, setActiveTab] = useState('products');
  const [selectedProduct, setSelectedProduct] = useState(null);
  const [refreshKey, setRefreshKey] = useState(0);

  const handleProductCreated = () => {
    setRefreshKey(k => k + 1);
  };

  const handleOrderCreated = () => {
    setRefreshKey(k => k + 1);
    setActiveTab('orders');
  };

  const handleSelectProduct = (product) => {
    setSelectedProduct(product);
    setActiveTab('create-order');
  };

  return (
    <div className="App">
      <header className="app-header">
        <h1>Asspire E-Commerce</h1>
        <p>Microservices Application with .NET Aspire</p>
      </header>

      <nav className="app-nav">
        <button
          className={activeTab === 'products' ? 'active' : ''}
          onClick={() => setActiveTab('products')}
        >
          Products
        </button>
        <button
          className={activeTab === 'add-product' ? 'active' : ''}
          onClick={() => setActiveTab('add-product')}
        >
          Add Product
        </button>
        <button
          className={activeTab === 'create-order' ? 'active' : ''}
          onClick={() => setActiveTab('create-order')}
        >
          Create Order
        </button>
        <button
          className={activeTab === 'orders' ? 'active' : ''}
          onClick={() => setActiveTab('orders')}
        >
          Orders
        </button>
      </nav>

      <main className="app-main">
        {activeTab === 'products' && (
          <ProductList key={refreshKey} onSelectProduct={handleSelectProduct} />
        )}

        {activeTab === 'add-product' && (
          <ProductForm onProductCreated={handleProductCreated} />
        )}

        {activeTab === 'create-order' && (
          <OrderForm
            selectedProduct={selectedProduct}
            onOrderCreated={handleOrderCreated}
          />
        )}

        {activeTab === 'orders' && (
          <OrderList key={refreshKey} />
        )}
      </main>

      <footer className="app-footer">
        <div className="footer-content">
          <p>Built with React, .NET 8, Aspire, CQRS, RabbitMQ, PostgreSQL, Redis, and gRPC</p>
          <div className="footer-links">
            <a href="http://localhost:5000/swagger" target="_blank" rel="noopener noreferrer">
              API Gateway Swagger
            </a>
            <a href="http://localhost:15672" target="_blank" rel="noopener noreferrer">
              RabbitMQ Management
            </a>
            <a href="http://localhost:16686" target="_blank" rel="noopener noreferrer">
              Jaeger Tracing
            </a>
          </div>
        </div>
      </footer>
    </div>
  );
}

export default App;
