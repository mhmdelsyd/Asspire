import React, { useState } from 'react';
import { productApi } from '../services/api';

const ProductForm = ({ onProductCreated }) => {
  const [formData, setFormData] = useState({
    name: '',
    description: '',
    price: '',
    stockQuantity: '',
    category: 'Electronics',
  });
  const [loading, setLoading] = useState(false);
  const [error, setError] = useState(null);

  const categories = ['Electronics', 'Clothing', 'Food', 'Books', 'Toys', 'Sports', 'Home', 'Garden'];

  const handleChange = (e) => {
    setFormData({
      ...formData,
      [e.target.name]: e.target.value,
    });
  };

  const handleSubmit = async (e) => {
    e.preventDefault();
    setLoading(true);
    setError(null);

    try {
      const productData = {
        name: formData.name,
        description: formData.description,
        price: parseFloat(formData.price),
        stockQuantity: parseInt(formData.stockQuantity),
        category: formData.category,
      };

      const response = await productApi.create(productData);
      alert('Product created successfully!');

      // Reset form
      setFormData({
        name: '',
        description: '',
        price: '',
        stockQuantity: '',
        category: 'Electronics',
      });

      if (onProductCreated) {
        onProductCreated(response.data);
      }
    } catch (err) {
      setError('Failed to create product: ' + err.message);
    } finally {
      setLoading(false);
    }
  };

  return (
    <div className="product-form-container">
      <h2>Add New Product</h2>

      <form onSubmit={handleSubmit} className="product-form">
        <div className="form-group">
          <label htmlFor="name">Product Name:</label>
          <input
            type="text"
            id="name"
            name="name"
            value={formData.name}
            onChange={handleChange}
            required
          />
        </div>

        <div className="form-group">
          <label htmlFor="description">Description:</label>
          <textarea
            id="description"
            name="description"
            value={formData.description}
            onChange={handleChange}
            rows="3"
            required
          />
        </div>

        <div className="form-row">
          <div className="form-group">
            <label htmlFor="price">Price ($):</label>
            <input
              type="number"
              id="price"
              name="price"
              step="0.01"
              min="0"
              value={formData.price}
              onChange={handleChange}
              required
            />
          </div>

          <div className="form-group">
            <label htmlFor="stockQuantity">Stock Quantity:</label>
            <input
              type="number"
              id="stockQuantity"
              name="stockQuantity"
              min="0"
              value={formData.stockQuantity}
              onChange={handleChange}
              required
            />
          </div>
        </div>

        <div className="form-group">
          <label htmlFor="category">Category:</label>
          <select
            id="category"
            name="category"
            value={formData.category}
            onChange={handleChange}
            required
          >
            {categories.map((cat) => (
              <option key={cat} value={cat}>
                {cat}
              </option>
            ))}
          </select>
        </div>

        {error && <div className="error">{error}</div>}

        <button type="submit" className="btn-primary" disabled={loading}>
          {loading ? 'Creating Product...' : 'Create Product'}
        </button>
      </form>
    </div>
  );
};

export default ProductForm;
