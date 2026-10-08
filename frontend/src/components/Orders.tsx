import { useEffect, useState } from 'react'

interface Order {
    orderId: number
    vendorId: number
    productName: string
    quantity: number
    originalUnitPrice: number
    unitPrice: number
    totalPrice: number
}

const Orders = () => {
    const [orders, setOrders] = useState<Order[]>([])
    const [loading, setLoading] = useState(true)

    useEffect(() => {
        // Fetch the current user's order history
        fetch('http://localhost:5234/api/orders/2')
            .then(response => response.json())
            .then(data => {
                setOrders(data)
                setLoading(false)
            })
            .catch(error => {
                console.error('Orders API error:', error)
                setLoading(false)
            })
    }, [])

    // Show a loading message while orders are being fetched
    if (loading) {
        return <p>Loading orders...</p>
    }

    return (
        <div className="orders-page">
            <h1>Orders</h1>

            {orders.length === 0 ? (
                <p>You have no orders yet.</p>
            ) : (
                <div className="orders-grid">
                    {orders.map(order => (
                        <div className="order-card" key={order.orderId}>
                            <h2>Order #{order.orderId}</h2>

                            <p>
                                <strong>Product:</strong> {order.productName}
                            </p>

                            <p>
                                <strong>Quantity:</strong> {order.quantity}
                            </p>

                            <p>
                                <strong>Original price:</strong>{' '}
                                {order.originalUnitPrice.toFixed(2)} DKK
                            </p>

                            <p>
                                <strong>Price per item:</strong>{' '}
                                {order.unitPrice.toFixed(2)} DKK
                            </p>

                            <p>
                                <strong>Vendor:</strong> {order.vendorId}
                            </p>

                            <div className="order-total">
                                <span>Total</span>
                                <strong>{order.totalPrice.toFixed(2)} DKK</strong>
                            </div>
                        </div>
                    ))}
                </div>
            )}
        </div>
    )
}

export default Orders