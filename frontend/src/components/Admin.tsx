import { useEffect, useState } from 'react'

interface Category {
    id: number
    name: string
}

const Admin = () => {
    const [categories, setCategories] = useState<Category[]>([])
    const [newCategory, setNewCategory] = useState('')
    const [editingCategory, setEditingCategory] = useState<Category | null>(null)

    useEffect(() => {
        fetch('http://localhost:5234/api/categories')
            .then(response => response.json())
            .then(data => {
                setCategories(data)
            })
            .catch(error => {
                console.error('Categories API error:', error)
            })
    }, [])

    const handleAddCategory = async () => {
        if (!newCategory.trim()) {
            return
        }

        const response = await fetch('http://localhost:5234/api/categories', {
            method: 'POST',
            headers: {
                'Content-Type': 'application/json',
            },
            body: JSON.stringify({
                name: newCategory,
            }),
        })

        if (!response.ok) {
            alert('Could not create category')
            return
        }

        const createdCategory = await response.json()

        setCategories(currentCategories => [
            ...currentCategories,
            createdCategory,
        ])

        setNewCategory('')
    }
    const handleDeleteCategory = async (id: number) => {
        const response = await fetch(
            `http://localhost:5234/api/categories/${id}`,
            {
                method: 'DELETE',
            }
        )

        if (!response.ok) {
            alert('Could not delete category')
            return
        }

        setCategories(currentCategories =>
            currentCategories.filter(category => category.id !== id)
        )
    }

    const handleEditCategory = async () => {
        if (!editingCategory || !editingCategory.name.trim()) {
            return
        }

        const response = await fetch(
            `http://localhost:5234/api/categories/${editingCategory.id}`,
            {
                method: 'PUT',
                headers: {
                    'Content-Type': 'application/json',
                },
                body: JSON.stringify({
                    name: editingCategory.name,
                }),
            }
        )

        if (!response.ok) {
            alert('Could not update category')
            return
        }

        setCategories(currentCategories =>
            currentCategories.map(category =>
                category.id === editingCategory.id
                    ? editingCategory
                    : category
            )
        )

        setEditingCategory(null)
    }

    return (
        <div className="admin-page">
            <h1>Admin</h1>
            <p>Manage product categories.</p>

            <div className="admin-content">
                <h2 className="admin-section-title">Categories</h2>

                <div className="admin-form">
                <input
                    type="text"
                    value={newCategory}
                    onChange={event => setNewCategory(event.target.value)}
                    placeholder="Category name"
                />

                <button onClick={handleAddCategory}>
                    Add Category
                </button>
            </div>

                {editingCategory && (
                    <div className="admin-edit-form">
                    <input
                        type="text"
                        value={editingCategory.name}
                        onChange={event =>
                            setEditingCategory({
                                id: editingCategory.id,
                                name: event.target.value,
                            })
                        }
                    />

                    <button onClick={handleEditCategory}>
                        Save
                    </button>

                    <button onClick={() => setEditingCategory(null)}>
                        Cancel
                    </button>
                </div>
            )}

            <div className="category-list">
                {categories.map(category => (
                    <div className="category-item" key={category.id}>
                        <span>{category.name}</span>

                        <div className="category-item-actions">
                            <button
                                onClick={() => setEditingCategory(category)}
                            >
                                Edit
                            </button>

                            <button
                                onClick={() => handleDeleteCategory(category.id)}
                            >
                                Delete
                            </button>
                        </div>
                    </div>
                ))}
            </div>
        </div>
        </div>     
    )
}

export default Admin