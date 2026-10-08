import type { CartItem } from '../types/CartItem'

interface CartProps {
    cart: CartItem[]
    onIncrease: (productId: number) => void
    onDecrease: (productId: number) => void
    onCheckout: () => void
}

function Cart({ cart, onIncrease, onDecrease, onCheckout }: CartProps) {
    const total = cart.reduce(
        (sum, item) => sum + item.product.price * item.quantity,
        0
    )

    return (
        <div className="cart">
            <h2>Your Cart</h2>

            {cart.length === 0 ? (
                <p className="cart-empty">Cart is currently empty.</p>
            ) : (
                <>
                    <div className="cart-items">
                        {cart.map(item => (
                            <div className="cart-item" key={item.product.id}>
                                <div className="cart-item-info">
                                    <h3>{item.product.name}</h3>
                                    <p>{item.product.price.toFixed(2)} DKK per item</p>
                                </div>

                                <div className="cart-quantity">
                                    <button
                                        type="button"
                                        onClick={() => onDecrease(item.product.id)}
                                    >
                                        −
                                    </button>

                                    <span>{item.quantity}</span>

                                    <button
                                        type="button"
                                        onClick={() => onIncrease(item.product.id)}
                                    >
                                        +
                                    </button>
                                </div>

                                <strong className="cart-item-price">
                                    {(item.product.price * item.quantity).toFixed(2)} DKK
                                </strong>
                            </div>
                        ))}
                    </div>

                    <div className="cart-total">
                        <span>Total</span>
                        <strong>{total.toFixed(2)} DKK</strong>
                    </div>

                    <button
                        className="checkout-button"
                        onClick={onCheckout}
                    >
                        Complete Purchase
                    </button>
                </>
            )}
        </div>
    )
}

export default Cart