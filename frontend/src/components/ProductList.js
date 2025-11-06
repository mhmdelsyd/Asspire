import React, { useState, useEffect } from 'react';
import { productApi } from '../services/api';

const ProductList = ({ onSelectProduct }) => {
  const [products, setProducts] = useState([]);
  const [loading, setLoading] = useState(true);
  const [error, setError] = useState(null);
  const [pageNumber, setPageNumber] = useState(1);
  const [totalCount, setTotalCount] = useState(0);
  const pageSize = 20;

  useEffect(() => {
    fetchProducts();
  }, [pageNumber]);

  const fetchProducts = async () => {
    try {
      setLoading(true);
      const response = await productApi.getAll(pageNumber, pageSize);
      setProducts(response.data.products);
      setTotalCount(response.data.totalCount);
      setError(null);
    } catch (err) {
      setError('Failed to fetch products: ' + err.message);
    } finally {
      setLoading(false);
    }
  };

  const handleDelete = async (id) => {
    if (window.confirm('Are you sure you want to delete this product?')) {
      try {
        await productApi.delete(id);
        fetchProducts();
      } catch (err) {
        alert('Failed to delete product: ' + err.message);
      }
    }
  };

  if (loading) return <div className="loading">Loading products...</div>;
  if (error) return <div className="error">{error}</div>;

  const totalPages = Math.ceil(totalCount / pageSize);

  return (
    <div className="product-list">
      <h2>Products ({totalCount} total)</h2>

      <div className="products-grid">
        {products.map((product) => (
          <div key={product.id} className="product-card">
            <h3>{product.name}</h3>
            <p className="description">{product.description}</p>
            <div className="product-details">
              <span className="price">${product.price.toFixed(2)}</span>
              <span className="stock">Stock: {product.stockQuantity}</span>
              <span className="category">{product.category}</span>
            </div>
            <div className="product-actions">
              <button
                onClick={() => onSelectProduct(product)}
                className="btn-primary"
              >
                Order
              </button>
              <button
                onClick={() => handleDelete(product.id)}
                className="btn-danger"
              >
                Delete
              </button>
            </div>
          </div>
        ))}
      </div>

      <div className="pagination">
        <button
          onClick={() => setPageNumber(p => Math.max(1, p - 1))}
          disabled={pageNumber === 1}
        >
          Previous
        </button>
        <span>Page {pageNumber} of {totalPages}</span>
        <button
          onClick={() => setPageNumber(p => Math.min(totalPages, p + 1))}
          disabled={pageNumber === totalPages}
        >
          Next
        </button>
      </div>
    </div>
  );
};

export default ProductList;
