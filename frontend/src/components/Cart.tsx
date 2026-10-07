import type { CartItem } from '../types/CartItem'

interface CartProps {
    cart: CartItem[]
    onIncrease: (productId: number) => void
    onDecrease: (productId: number) => void
    onCheckout: () => void
}

function Cart({ cart, onIncrease, onDecrease, onCheckout }: CartProps) {
    return (
        <div className="cart">
            <h2>Cart</h2>

            {cart.length === 0 ? (
                <p>Your cart is empty.</p>
            ) : (
                <>
                    {cart.map(item => (
                        <div key={item.product.id}>
                            <span>{item.product.name}</span>

                            <strong>
                                {(item.product.price * item.quantity).toFixed(2)} DKK
                            </strong>

                            <button onClick={() => onDecrease(item.product.id)}>
                                −
                            </button>

                            <span>{item.quantity}</span>

                            <button onClick={() => onIncrease(item.product.id)}>
                                +
                            </button>
                        </div>
                    ))}

                    <div className="cart-total">
                        <span>Total</span>

                        <strong>
                            {cart
                                .reduce(
                                    (total, item) =>
                                        total + item.product.price * item.quantity,
                                    0
                                )
                                .toFixed(2)} DKK
                        </strong>
                    </div>
                    <button className="checkout-button" onClick={onCheckout}>
                        Complete Purchase
                    </button>
                </>
            )}
        </div>
    )
}

export default Cart