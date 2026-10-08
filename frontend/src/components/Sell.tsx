
import { useEffect, useState } from 'react'
import type { ChangeEvent, FormEvent } from 'react'
import { Api } from '../api/Api'
import type { Product } from '../types/Product'

interface Category {
    id: number
    name: string
}

interface SellProps {
    editingProduct: Product | null
    onSaved: () => Promise<void>
    onCancelEdit: () => void
}

const api = new Api({
    baseUrl: 'http://localhost:5234',
})

function Sell({ editingProduct, onSaved, onCancelEdit }: SellProps) {
    const [categories, setCategories] = useState<Category[]>([])
    const [productImage, setProductImage] = useState<File | null>(null)
    const [imagePreview, setImagePreview] = useState<string | null>(null)
    const [isSaving, setIsSaving] = useState(false)
    const [formVersion, setFormVersion] = useState(0)

    useEffect(() => {
        api.api.categoriesList()
            .then(response => {
                setCategories(
                    Array.isArray(response.data)
                        ? response.data as Category[]
                        : []
                )
            })
            .catch(error => {
                console.error('Categories API error:', error)
            })
    }, [])

    useEffect(() => {
        setProductImage(null)

        if (editingProduct?.imageUrl) {
            setImagePreview(
                editingProduct.imageUrl.startsWith('http')
                    ? editingProduct.imageUrl
                    : `http://localhost:5234${editingProduct.imageUrl}`
            )
        } else {
            setImagePreview(null)
        }
    }, [editingProduct])

    const handleImageChange = (event: ChangeEvent<HTMLInputElement>) => {
        const file = event.target.files?.[0]

        if (!file) return

        if (!['image/jpeg', 'image/png', 'image/webp'].includes(file.type)) {
            alert('Please select a JPG, PNG or WebP image.')
            event.target.value = ''
            return
        }

        if (file.size > 5 * 1024 * 1024) {
            alert('Image must be smaller than 5 MB.')
            event.target.value = ''
            return
        }

        setProductImage(file)
        setImagePreview(URL.createObjectURL(file))
    }

    const handleSubmit = async (event: FormEvent<HTMLFormElement>) => {
        event.preventDefault()

        if (isSaving) return

        const formData = new FormData(event.currentTarget)

        const product = {
            name: String(formData.get('name')),
            description: String(formData.get('description')),
            price: Number(formData.get('price')),
            stock: Number(formData.get('stock')),
            categoryId: Number(formData.get('categoryId')),
            condition: String(formData.get('condition')),
            vendorId: editingProduct?.vendorId ?? 2,
            isActive: editingProduct?.isActive ?? true,
            imageUrl: editingProduct?.imageUrl ?? '',
        }

        setIsSaving(true)

        try {
            if (productImage) {
                const imageData = new FormData()
                imageData.append('file', productImage)

                const uploadResponse = await fetch(
                    'http://localhost:5234/api/uploads',
                    {
                        method: 'POST',
                        body: imageData,
                    }
                )

                if (!uploadResponse.ok) {
                    throw new Error('Image upload failed')
                }

                const uploadedImage = await uploadResponse.json()
                product.imageUrl = uploadedImage.imageUrl
            }

            if (editingProduct) {
                await api.api.productsUpdate(editingProduct.id, product)
            } else {
                await api.api.productsCreate(product)
            }

            await onSaved()

            alert(
                editingProduct
                    ? 'Listing updated successfully!'
                    : 'Listing created successfully!'
            )

            setProductImage(null)
            setImagePreview(null)
            setFormVersion(version => version + 1)

        } catch (error) {
            console.error('Failed to save listing:', error)
            alert('Failed to save listing. Please check the product details.')
        } finally {
            setIsSaving(false)
        }
    }

    return (
        <section className="sell-page">
            <h1>
                {editingProduct ? 'Edit Product' : 'Sell a Product'}
            </h1>

            <p>
                {editingProduct
                    ? 'Update your existing product listing.'
                    : 'Create a listing and offer your products on Satin Road.'}
            </p>

            <form
                key={`${editingProduct?.id ?? 'new'}-${formVersion}`}
                className="sell-form"
                onSubmit={handleSubmit}
            >
                <div className="sell-form-columns">
                    <div className="sell-form-section">
                        <h2>Product Details</h2>

                        <label>
                            Product Name
                            <input
                                type="text"
                                name="name"
                                defaultValue={editingProduct?.name ?? ''}
                                required
                            />
                        </label>

                        <label>
                            Description
                            <textarea
                                name="description"
                                defaultValue={editingProduct?.description ?? ''}
                                required
                            />
                        </label>

                        <label>
                            Category
                            <select
                                name="categoryId"
                                required
                                defaultValue={editingProduct?.categoryId ?? ''}
                            >
                                <option value="" disabled>
                                    Select a category
                                </option>

                                {categories.map(category => (
                                    <option
                                        key={category.id}
                                        value={category.id}
                                    >
                                        {category.name}
                                    </option>
                                ))}
                            </select>
                        </label>
                    </div>

                    <div className="sell-form-section">
                        <h2>Pricing & Inventory</h2>

                        <label>
                            Price (DKK)
                            <input
                                type="number"
                                name="price"
                                min="0.01"
                                step="0.01"
                                defaultValue={editingProduct?.price ?? ''}
                                required
                            />
                        </label>

                        <label>
                            Stock
                            <input
                                type="number"
                                name="stock"
                                min="0"
                                step="1"
                                defaultValue={editingProduct?.stock ?? ''}
                                required
                            />
                        </label>

                        <label>
                            Condition
                            <select
                                name="condition"
                                defaultValue={editingProduct?.condition ?? 'New'}
                            >
                                <option value="New">New</option>
                                <option value="Used">Used</option>
                            </select>
                        </label>
                    </div>
                </div>

                <div className="sell-image-upload">
                    <h2>Product Photo</h2>

                    <input
                        type="file"
                        accept="image/jpeg,image/png,image/webp"
                        onChange={handleImageChange}
                    />

                    {imagePreview && (
                        <img
                            src={imagePreview}
                            alt="Product preview"
                            className="sell-image-preview"
                        />
                    )}
                </div>

                <button type="submit" disabled={isSaving}>
                    {isSaving
                        ? 'Saving...'
                        : editingProduct
                            ? 'Update Listing'
                            : 'Create Listing'}
                </button>

                {editingProduct && (
                    <button
                        type="button"
                        onClick={onCancelEdit}
                        disabled={isSaving}
                        style={{ marginTop: '12px' }}
                    >
                        Cancel Editing
                    </button>
                )}
            </form>
        </section>
    )
}

export default Sell
