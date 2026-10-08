
import { useState } from 'react'
import type { Product } from '../types/Product'
import { Api } from '../api/Api'

interface MyListingsProps {
    products: Product[]
    onEdit: (product: Product) => void
    onDeleted: () => Promise<void>
}

const api = new Api({
    baseUrl: 'http://localhost:5234',
})

function MyListings({ products, onEdit, onDeleted }: MyListingsProps) {
    const [deletingId, setDeletingId] = useState<number | null>(null)

    const myProducts = products.filter(product => product.vendorId === 2)

    const handleDelete = async (product: Product) => {
        const confirmed = window.confirm(
            `Are you sure you want to delete "${product.name}"?`
        )

        if (!confirmed) return

        setDeletingId(product.id)

        try {
            await api.api.productsDelete(product.id)
            await onDeleted()

            alert('Listing deleted successfully!')
        } catch (error) {
            console.error('Failed to delete listing:', error)
            alert('Failed to delete listing. Please try again.')
        } finally {
            setDeletingId(null)
        }
    }

    return (
        <section className="my-listings">
            <h2>My Listings</h2>

            {myProducts.length === 0 ? (
                <p>You haven't created any listings yet.</p>
            ) : (
                <div className="my-listings-grid">
                    {myProducts.map(product => (
                        <div key={product.id} className="my-listing">
                            <h3>{product.name}</h3>
                            <p>Stock: {product.stock}</p>
                            <p>{product.price.toFixed(2)} DKK</p>

                            <div className="my-listing-actions">
                                <button
                                    type="button"
                                    className="my-listing-edit"
                                    onClick={() => {
                                        onEdit(product)
                                        window.scrollTo({
                                            top: 0,
                                            behavior: 'smooth',
                                        })
                                    }}
                                >
                                    Edit
                                </button>

                                <button
                                    type="button"
                                    className="my-listing-delete"
                                    disabled={deletingId === product.id}
                                    onClick={() => handleDelete(product)}
                                >
                                    {deletingId === product.id
                                        ? 'Deleting...'
                                        : 'Delete'}
                                </button>
                            </div>
                        </div>
                    ))}
                </div>
            )}
        </section>
    )
}

export default MyListings
