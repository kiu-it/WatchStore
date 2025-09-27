using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using WatchStore.Models;

namespace WatchStore.Library
{
    public class CartService
    {
        private readonly WatchStoreDbContext _db;

        public CartService()
        {
            _db = new WatchStoreDbContext();
        }

        // Lấy giỏ hàng của người dùng hoặc tạo mới nếu chưa có
        public MCart GetCart(int userId)
        {
            var cart = _db.Carts.FirstOrDefault(c => c.UserId == userId);
            if (cart == null)
            {
                cart = new MCart
                {
                    UserId = userId,
                    CreatedAt = DateTime.Now,
                    UpdatedAt = DateTime.Now
                };
                _db.Carts.Add(cart);
                _db.SaveChanges();
            }
            return cart;
        }

        // Lấy danh sách sản phẩm trong giỏ hàng
        public List<ModelCart> GetCartItems(int userId)
        {
            var cart = GetCart(userId);
            var cartItems = _db.CartItems.Where(ci => ci.CartId == cart.Id).ToList();

            var result = new List<ModelCart>();
            foreach (var item in cartItems)
            {
                var product = _db.Products.Find(item.ProductId);
                if (product != null)
                {
                    result.Add(new ModelCart
                    {
                        ProductID = product.ID,
                        Name = product.Name,
                        Slug = product.Slug,
                        Image = product.Image,
                        Quantity = item.Quantity,
                        Price = item.Price
                    });
                }
            }
            return result;
        }

        // Thêm sản phẩm vào giỏ hàng
        public bool AddToCart(int userId, int productId, int quantity)
        {
            try
            {
                var cart = GetCart(userId);
                var product = _db.Products.FirstOrDefault(p => p.Status == 1 && p.ID == productId);

                if (product == null || product.Quantity < quantity)
                    return false;

                var cartItem = _db.CartItems.FirstOrDefault(ci => ci.CartId == cart.Id && ci.ProductId == productId);

                if (cartItem != null)
                {
                    cartItem.Quantity += quantity;
                    cartItem.UpdatedAt = DateTime.Now;
                }
                else
                {
                    cartItem = new MCartItem
                    {
                        CartId = cart.Id,
                        ProductId = productId,
                        Quantity = quantity,
                        Price = product.Discount == 1 ? product.ProPrice : product.Price,
                        CreatedAt = DateTime.Now,
                        UpdatedAt = DateTime.Now
                    };
                    _db.CartItems.Add(cartItem);
                }

                cart.UpdatedAt = DateTime.Now;
                _db.SaveChanges();
                return true;
            }
            catch
            {
                return false;
            }
        }

        // Cập nhật số lượng sản phẩm trong giỏ hàng
        public bool UpdateCartItem(int userId, int productId, int quantity)
        {
            try
            {
                var cart = GetCart(userId);
                var product = _db.Products.FirstOrDefault(p => p.Status == 1 && p.ID == productId);

                if (product == null || product.Quantity < quantity)
                    return false;

                var cartItem = _db.CartItems.FirstOrDefault(ci => ci.CartId == cart.Id && ci.ProductId == productId);

                if (cartItem != null)
                {
                    cartItem.Quantity = quantity;
                    cartItem.UpdatedAt = DateTime.Now;
                    cart.UpdatedAt = DateTime.Now;
                    _db.SaveChanges();
                    return true;
                }

                return false;
            }
            catch
            {
                return false;
            }
        }

        // Xóa sản phẩm khỏi giỏ hàng
        public bool RemoveFromCart(int userId, int productId)
        {
            try
            {
                var cart = GetCart(userId);
                var cartItem = _db.CartItems.FirstOrDefault(ci => ci.CartId == cart.Id && ci.ProductId == productId);

                if (cartItem != null)
                {
                    _db.CartItems.Remove(cartItem);
                    cart.UpdatedAt = DateTime.Now;
                    _db.SaveChanges();
                    return true;
                }

                return false;
            }
            catch
            {
                return false;
            }
        }

        // Xóa toàn bộ giỏ hàng
        public bool ClearCart(int userId)
        {
            try
            {
                var cart = GetCart(userId);
                var cartItems = _db.CartItems.Where(ci => ci.CartId == cart.Id).ToList();

                foreach (var item in cartItems)
                {
                    _db.CartItems.Remove(item);
                }

                cart.UpdatedAt = DateTime.Now;
                _db.SaveChanges();
                return true;
            }
            catch
            {
                return false;
            }
        }

        // Chuyển giỏ hàng từ session sang database khi đăng nhập
        public void MigrateCart(int userId, List<ModelCart> sessionCart)
        {
            if (sessionCart == null || !sessionCart.Any())
                return;

            var cart = GetCart(userId);

            foreach (var item in sessionCart)
            {
                var product = _db.Products.Find(item.ProductID);
                if (product != null && product.Status == 1)
                {
                    var cartItem = _db.CartItems.FirstOrDefault(ci => ci.CartId == cart.Id && ci.ProductId == item.ProductID);

                    if (cartItem != null)
                    {
                        cartItem.Quantity += item.Quantity;
                        cartItem.UpdatedAt = DateTime.Now;
                    }
                    else
                    {
                        cartItem = new MCartItem
                        {
                            CartId = cart.Id,
                            ProductId = item.ProductID,
                            Quantity = item.Quantity,
                            Price = item.Price,
                            CreatedAt = DateTime.Now,
                            UpdatedAt = DateTime.Now
                        };
                        _db.CartItems.Add(cartItem);
                    }
                }
            }

            cart.UpdatedAt = DateTime.Now;
            _db.SaveChanges();
        }
    }
}