import { useEffect, useState } from 'react'
import type { ChangeEvent, FormEvent } from 'react'
import { Api } from '../api/Api'


interface Category {
    id: number
    name: string
}

const api = new Api({
    baseUrl: 'http://localhost:5234',
})

function Sell() {
    const [categories, setCategories] = useState<Category[]>([])
    const [productImage, setProductImage] = useState<File | null>(null)
    const [imagePreview, setImagePreview] = useState<string | null>(null)

    useEffect(() => {
        api.api.categoriesList()
            .then(response => {
                setCategories(Array.isArray(response.data) ? response.data as Category[] : [])
            })
            .catch(error => {
                console.error('Categories API error:', error)
            })
    }, [])
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

        const formData = new FormData(event.currentTarget)

        const product = {
            name: String(formData.get('name')),
            description: String(formData.get('description')),
            price: Number(formData.get('price')),
            stock: Number(formData.get('stock')),
            categoryId: Number(formData.get('categoryId')),
            condition: String(formData.get('condition')),
            vendorId: 2,
            isActive: true,
            imageUrl: '',
        }

        try {
            if (productImage) {
                const imageData = new FormData()
                imageData.append('file', productImage)

                const uploadResponse = await fetch('http://localhost:5234/api/uploads', {
                    method: 'POST',
                    body: imageData
                })

                if (!uploadResponse.ok) {
                    throw new Error('Image upload failed')
                }

                const uploadedImage = await uploadResponse.json()
                product.imageUrl = uploadedImage.imageUrl
            }

            await api.api.productsCreate(product)
            alert('Listing created successfully!')
        } catch (error) {
            console.error('Failed to create listing:', error)
            alert('Failed to create listing. Please check the product details.')
        }
    }
    return (
        <section className="sell-page">
            <h1>Sell a Product</h1>
            <p>Create a listing and offer your products on Satin Road.</p>


            <form className="sell-form" onSubmit={handleSubmit}>
                <div className="sell-form-columns">

                    <div className="sell-form-section">
                        <h2>Product Details</h2>

                        <label>
                            Product Name
                            <input type="text" name="name" required />
                        </label>

                        <label>
                            Description
                            <textarea name="description" required />
                        </label>

                        <label>
                            Category
                            <select name="categoryId" required defaultValue="">
                                <option value="" disabled>Select a category</option>
                                {categories.map(category => (
                                    <option key={category.id} value={category.id}>
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
                            <input type="number" name="price" min="0.01" step="0.01" required />
                        </label>

                        <label>
                            Stock
                            <input type="number" name="stock" min="0" step="1" required />
                        </label>

                        <label>
                            Condition
                            <select name="condition">
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

                <button type="submit">Create Listing</button>
            </form>

        </section>
    )
}

export default Sell
