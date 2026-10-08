import { useEffect, useState } from 'react'
import ProductCard from './components/ProductCard'
import type { Product } from './types/Product'
import { Api } from './api/Api'
import type { CartItem } from './types/CartItem'
import Cart from './components/Cart'
import Sell from './components/Sell'
import './App.css'
import Orders from './components/Orders'

const api = new Api({
  baseUrl: 'http://localhost:5234',
})

function App() {
  const [products, setProducts] = useState<Product[]>([])
  const [cart, setCart] = useState<CartItem[]>([])
  const [showCart, setShowCart] = useState(false)
  const [showSell, setShowSell] = useState(false)
  const [showOrders, setShowOrders] = useState(false)


  useEffect(() => {
    api.api.productsList()
        .then(response => {
          setProducts(response.data as Product[])
        })
        .catch(error => {
          console.error('Products API error:', error)
        })
  }, [])
  
  
  const handleAddToCart = (product: Product) => {
    setCart(currentCart => {
      if (currentCart.length === 0) {
        return [{ product, quantity: 1 }]
      }

      const currentItem = currentCart[0]

      if (currentItem.product.id === product.id) {
        if (currentItem.quantity >= product.stock) {
          return currentCart
        }

        return [
          {
            ...currentItem,
            quantity: currentItem.quantity + 1,
          },
        ]
      }

      alert(
          'Your cart already contains another listing. Complete or remove the current order before adding a different product.'
      )

      return currentCart
    })
  }
  const handleIncreaseQuantity = (productId: number) => {
    setCart(currentCart =>
        currentCart.map(item =>
            item.product.id === productId &&
            item.quantity < item.product.stock
                ? { ...item, quantity: item.quantity + 1 }
                : item
        )
    )
  }

  const handleDecreaseQuantity = (productId: number) => {
    setCart(currentCart =>
        currentCart
            .map(item =>
                item.product.id === productId
                    ? { ...item, quantity: item.quantity - 1 }
                    : item
            )
            .filter(item => item.quantity > 0)
    )
  }

  const handleCheckout = async () => {
    if (cart.length === 0) {
      return
    }

    const item = cart[0]

    const response = await fetch('http://localhost:5234/api/purchases', {
      method: 'POST',
      headers: {
        'Content-Type': 'application/json',
      },
      body: JSON.stringify({
        buyerId: 2,
        productId: item.product.id,
        quantity: item.quantity,
      }),
    })

    if (!response.ok) {
      const message = await response.text()
      alert(message)
      return
    }
    
    const data = await response.json()

    console.log('Purchase response:', data)

    if (data.message) {
      alert(data.message)

      setProducts(currentProducts =>
          currentProducts.filter(product => product.vendorId !== data.vendorId)
      )

      setCart([])
      setShowCart(false)

      return
    }
    
    setProducts(currentProducts =>
        currentProducts.map(product =>
            product.id === data.productId
                ? { ...product, stock: data.remainingStock }
                : product
        )
    )
    
    setCart([])
    setShowCart(false)
  }
  
  return (
      <>
        <header className="header">
          <div className="logo">
            SATIN <span>ROAD</span>
          </div>

          <nav>
            <a href="#" onClick={(e) => {
              e.preventDefault()
              setShowSell(false)
              setShowOrders(false)
            }}>
              Marketplace
            </a>

            <a href="#" onClick={(e) => {
              e.preventDefault()
              setShowSell(true)
              setShowCart(false)
              setShowOrders(false)
            }}>
              Sell
            </a>

            <a
                href="#"
                onClick={(e) => {
                  e.preventDefault()
                  setShowOrders(true)
                  setShowSell(false)
                  setShowCart(false)
                }}
            >
              Orders
            </a>

            <a href="#" onClick={(e) => {
              e.preventDefault()
              setShowSell(false)
              setShowOrders(false)
              setShowCart(!showCart)
            }}>
              Cart ({cart.reduce((total, item) => total + item.quantity, 0)})
            </a>
          </nav>
          
          <button className="account-button">
            Account
          </button>
        </header>

        <main className="marketplace">
          {showOrders ? (
            <Orders />
        ) : showSell ? (
            <Sell />
        ) : (
            <>
              <h1>Marketplace</h1>
          <p>Browse anonymous listings from vendors.</p>
          {showCart && (
              <Cart
                  cart={cart}
                  onIncrease={handleIncreaseQuantity}
                  onDecrease={handleDecreaseQuantity}
                  onCheckout={handleCheckout}
              />
          )}

          <div className="product-grid">
            {products.map(product => (
                <ProductCard
                    key={product.id}
                    product={product}
                    onAddToCart={handleAddToCart}
                />
            ))}
          </div>
              </>
          )}
        </main>
      </>
  )
}

export default App