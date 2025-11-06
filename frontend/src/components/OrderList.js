import React, { useState, useEffect } from 'react';
import { orderApi } from '../services/api';

const OrderList = () => {
  const [orders, setOrders] = useState([]);
  const [loading, setLoading] = useState(true);
  const [error, setError] = useState(null);
  const [pageNumber, setPageNumber] = useState(1);
  const [totalCount, setTotalCount] = useState(0);
  const pageSize = 10;

  useEffect(() => {
    fetchOrders();
  }, [pageNumber]);

  const fetchOrders = async () => {
    try {
      setLoading(true);
      const response = await orderApi.getAll(pageNumber, pageSize);
      setOrders(response.data.orders);
      setTotalCount(response.data.totalCount);
      setError(null);
    } catch (err) {
      setError('Failed to fetch orders: ' + err.message);
    } finally {
      setLoading(false);
    }
  };

  const handleStatusUpdate = async (orderId, newStatus) => {
    try {
      await orderApi.updateStatus(orderId, newStatus);
      fetchOrders();
    } catch (err) {
      alert('Failed to update order status: ' + err.message);
    }
  };

  const getStatusColor = (status) => {
    switch (status) {
      case 'Pending': return '#ffc107';
      case 'Confirmed': return '#28a745';
      case 'Completed': return '#17a2b8';
      case 'Cancelled': return '#dc3545';
      default: return '#6c757d';
    }
  };

  if (loading) return <div className="loading">Loading orders...</div>;
  if (error) return <div className="error">{error}</div>;

  const totalPages = Math.ceil(totalCount / pageSize);

  return (
    <div className="order-list">
      <h2>Orders ({totalCount} total)</h2>

      {orders.length === 0 ? (
        <p>No orders yet. Create your first order!</p>
      ) : (
        <div className="orders-table">
          <table>
            <thead>
              <tr>
                <th>Order ID</th>
                <th>Product</th>
                <th>Quantity</th>
                <th>Total Price</th>
                <th>Customer</th>
                <th>Status</th>
                <th>Actions</th>
              </tr>
            </thead>
            <tbody>
              {orders.map((order) => (
                <tr key={order.id}>
                  <td>{order.id.substring(0, 8)}...</td>
                  <td>{order.productName}</td>
                  <td>{order.quantity}</td>
                  <td>${order.totalPrice.toFixed(2)}</td>
                  <td>
                    <div>{order.customerName}</div>
                    <small>{order.customerEmail}</small>
                  </td>
                  <td>
                    <span
                      className="status-badge"
                      style={{ backgroundColor: getStatusColor(order.status) }}
                    >
                      {order.status}
                    </span>
                  </td>
                  <td>
                    {order.status === 'Pending' && (
                      <>
                        <button
                          onClick={() => handleStatusUpdate(order.id, 'Confirmed')}
                          className="btn-small btn-success"
                        >
                          Confirm
                        </button>
                        <button
                          onClick={() => handleStatusUpdate(order.id, 'Cancelled')}
                          className="btn-small btn-danger"
                        >
                          Cancel
                        </button>
                      </>
                    )}
                    {order.status === 'Confirmed' && (
                      <button
                        onClick={() => handleStatusUpdate(order.id, 'Completed')}
                        className="btn-small btn-info"
                      >
                        Complete
                      </button>
                    )}
                  </td>
                </tr>
              ))}
            </tbody>
          </table>
        </div>
      )}

      {totalPages > 1 && (
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
      )}
    </div>
  );
};

export default OrderList;
