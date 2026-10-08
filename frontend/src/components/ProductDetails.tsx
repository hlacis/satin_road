import type { Product } from '../types/Product'

interface ProductDetailsProps {
    product: Product
    onBack: () => void
    onAddToCart: (product: Product) => void
}

function ProductDetails({ product, onBack, onAddToCart }: ProductDetailsProps) {

    return (
        <section className="product-details">
            <button
                type="button"
                className="product-details-back"
                onClick={onBack}
            >
                ← Back to Marketplace
            </button>

            <div className="product-details-layout">
                <div className="product-details-info">
                    <h1>{product.name}</h1>
                    <p className="product-details-description">
                        {product.description}
                    </p>

                    <div className="product-details-meta">
                        <span>Condition: {product.condition}</span>
                        <span>Stock: {product.stock}</span>
                    </div>

                    <strong className="product-details-price">
                        {product.price.toFixed(2)} DKK
                    </strong>

                    <button
                        type="button"
                        className="product-details-cart"
                        onClick={() => onAddToCart(product)}
                        disabled={product.stock <= 0}
                    >
                        Add to cart
                    </button>
                </div>

                <div className="product-details-photo">
                    {product.imageUrl ? (
                        <img
                            src={`http://localhost:5234${product.imageUrl}`}
                            alt={product.name}
                            className="product-details-image"
                        />
                    ) : (
                        <p>No product image available</p>
                    )}
                </div>
            </div>
        </section>
    )


}

export default ProductDetails