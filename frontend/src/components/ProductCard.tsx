interface Product {
    id: number
    name: string
    description: string
    price: number
    stock: number
    vendorId: number
    categoryId: number
    condition: string
    isActive: boolean
    imageUrl?: string | null
    
}

interface ProductCardProps {
    product: Product
    onAddToCart: (product: Product) => void
}

function ProductCard({ product, onAddToCart }: ProductCardProps) {
    return (
        <article className="product-card">
            <div className="product-card-top">
                <span className="condition">{product.condition}</span>
                <span className="stock">Stock: {product.stock}</span>
            </div>

            {product.imageUrl && (
                <img
                    src={`http://localhost:5234${product.imageUrl}`}
                    alt={product.name}
                    className="product-image"
                />
            )}

            <h2>{product.name}</h2>
            <p className="description">{product.description}</p>

            <div className="product-card-bottom">
                <strong>{product.price.toFixed(2)} DKK</strong>
                <button onClick={() => onAddToCart(product)}>
                    Add to cart
                </button>
            </div>
        </article>
    )
}

export default ProductCard