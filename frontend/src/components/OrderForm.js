import React, { useState } from 'react';
import { orderApi } from '../services/api';

const OrderForm = ({ selectedProduct, onOrderCreated }) => {
  const [formData, setFormData] = useState({
    quantity: 1,
    customerName: '',
    customerEmail: '',
  });
  const [loading, setLoading] = useState(false);
  const [error, setError] = useState(null);

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
      const orderData = {
        productId: selectedProduct.id,
        quantity: parseInt(formData.quantity),
        customerName: formData.customerName,
        customerEmail: formData.customerEmail,
      };

      const response = await orderApi.create(orderData);
      alert('Order created successfully!');

      // Reset form
      setFormData({
        quantity: 1,
        customerName: '',
        customerEmail: '',
      });

      if (onOrderCreated) {
        onOrderCreated(response.data);
      }
    } catch (err) {
      if (err.response?.status === 400) {
        setError('Product not available. Please check stock quantity.');
      } else {
        setError('Failed to create order: ' + err.message);
      }
    } finally {
      setLoading(false);
    }
  };

  if (!selectedProduct) {
    return (
      <div className="order-form-container">
        <h2>Create Order</h2>
        <p>Please select a product from the list to create an order.</p>
      </div>
    );
  }

  const totalPrice = (selectedProduct.price * formData.quantity).toFixed(2);

  return (
    <div className="order-form-container">
      <h2>Create Order</h2>

      <div className="selected-product">
        <h3>Selected Product</h3>
        <p><strong>{selectedProduct.name}</strong></p>
        <p>Price: ${selectedProduct.price.toFixed(2)}</p>
        <p>Available Stock: {selectedProduct.stockQuantity}</p>
      </div>

      <form onSubmit={handleSubmit} className="order-form">
        <div className="form-group">
          <label htmlFor="quantity">Quantity:</label>
          <input
            type="number"
            id="quantity"
            name="quantity"
            min="1"
            max={selectedProduct.stockQuantity}
            value={formData.quantity}
            onChange={handleChange}
            required
          />
        </div>

        <div className="form-group">
          <label htmlFor="customerName">Your Name:</label>
          <input
            type="text"
            id="customerName"
            name="customerName"
            value={formData.customerName}
            onChange={handleChange}
            required
          />
        </div>

        <div className="form-group">
          <label htmlFor="customerEmail">Your Email:</label>
          <input
            type="email"
            id="customerEmail"
            name="customerEmail"
            value={formData.customerEmail}
            onChange={handleChange}
            required
          />
        </div>

        <div className="total-price">
          <strong>Total Price: ${totalPrice}</strong>
        </div>

        {error && <div className="error">{error}</div>}

        <button type="submit" className="btn-primary" disabled={loading}>
          {loading ? 'Creating Order...' : 'Place Order'}
        </button>
      </form>
    </div>
  );
};

export default OrderForm;
