using WatchStore.Library;
using WatchStore.Models;
using WatchStore.Services;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using System.Configuration;

namespace WatchStore.Controllers
{
    public class CartController : Controller
    {
        private WatchStoreDbContext db = new WatchStoreDbContext();
        // GET: Cart
        public ActionResult Index()
        {
            return View();
        }
        [HttpPost]
        //Thêm sp vào giỏ hàng
        public ActionResult Add(int pid, int qty)
        {

            // 
            var p = db.Products.First(m => m.Status == 1 && m.ID == pid);
            if (p.Quantity < qty)
            {
                return Json(new { result = 3 });
            }

            var cart = Session["Cart"];
            if (cart == null)
            {
                var item = new ModelCart();
                item.ProductID = p.ID;
                item.Name = p.Name;
                item.Slug = p.Slug;

                item.Image = p.Image;
                item.Quantity = qty;
                if (p.Discount == 1)
                {
                    item.Price = p.ProPrice;
                }
                else
                {
                    item.Price = p.Price;
                }
                var list = new List<ModelCart>();
                list.Add(item);

                Session["Cart"] = list;
                return Json(new { result = 1 });
            }
            else
            {
                var list = (List<ModelCart>)cart;

                if (list.Exists(m => m.ProductID == pid))
                {
                    foreach (var item in list)
                    {
                        if (item.ProductID == pid)
                            item.Quantity += qty;
                        return Json(new { result = 2 });
                    }
                }
                else
                {
                    var item = new ModelCart();
                    item.ProductID = p.ID;
                    item.Name = p.Name;
                    item.Slug = p.Slug;
                    item.Image = p.Image;
                    item.Quantity = qty;
                    if (p.Discount == 1)
                    {
                        item.Price = p.ProPrice;
                    }
                    else
                    {
                        item.Price = p.Price;
                    }
                    list.Add(item);
                    return Json(new { result = 1 });
                }
            }
            return Json(new { result = 0 });
        }

        public ActionResult Set(int pid, int qty)
        {

            // 
            var p = db.Products.First(m => m.Status == 1 && m.ID == pid);
            if (p.Quantity < qty)
            {
                return Json(new { result = 2 });
            }

            var cart = Session["Cart"];
            if (cart == null)
            {

            }
            else
            {
                var list = (List<ModelCart>)cart;

                if (list.Exists(m => m.ProductID == pid))
                {
                    foreach (var item in list)
                    {
                        if (item.ProductID == pid)
                            item.Quantity = qty;
                        return Json(new { result = 1 });
                    }
                }
            }
            return Json(new { result = 0 });
        }

        public JsonResult Update(int pid, String option)
        {
            var sCart = (List<ModelCart>)Session["Cart"];
            ModelCart c = sCart.First(m => m.ProductID == pid);
            if (c != null)
            {
                switch (option)
                {
                    case "add":
                        c.Quantity++;
                        return Json(1);
                    case "minus":
                        c.Quantity--;
                        return Json(2);
                    case "remove":
                        sCart.Remove(c);
                        if (sCart.Count() == 0)
                            Session.Remove("Cart");
                        return Json(3);
                    default:
                        break;
                }
            }
            return Json(null);
        }
        public ActionResult RemoveAll()
        {
            Session.Remove("Cart");
            Notification.set_flash("Đã xóa toàn bộ sản phẩm trong giỏ hàng!", "success");
            return Redirect("~/gio-hang");
        }
        public ActionResult Remove(int id)
        {
            var cart = (List<ModelCart>)Session["Cart"];
            if (cart != null)
            {
                var item = cart.FirstOrDefault(c => c.ProductID == id);
                if (item != null)
                {
                    cart.Remove(item); // xóa sản phẩm ra khỏi list
                    Session["Cart"] = cart; // cập nhật lại giỏ hàng trong session
                }
            }

            Notification.set_flash("Đã xóa sản phẩm khỏi giỏ hàng!", "success");
            return Redirect("~/gio-hang");
        }

        public ActionResult Checkout()
        {
            if (Session["User_Name"] != null && Session["Cart"] != null)
            {
                int user_id = Convert.ToInt32(Session["User_ID"]);
                ViewBag.Info = db.Users.Where(m => m.ID == user_id).First();
            }
            else
                return RedirectToAction("Index", "Cart");
            return View();
        }

        [HttpPost]
        public JsonResult Payment(String Email, String Address, String FullName, String Phone)
        {
            var order = new MOrder();
            int user_id = Convert.ToInt32(Session["User_ID"]);
            order.Code = DateTime.Now.ToString("yyyyMMddhhMMss"); // yyyy-MM-dd hh:MM:ss
            order.CustemerId = user_id;
            order.CreateDate = DateTime.Now;
            order.DeliveryAddress = Address;
            order.DeliveryEmail = Email;
            order.DeliveryPhone = Phone;
            order.DeliveryName = FullName;
            order.Status = 1;
            db.Orders.Add(order);
            db.SaveChanges();

            var OrderID = order.Id;

            foreach (var c in (List<ModelCart>)Session["Cart"])
            {
                var orderdetails = new MOrderdetail();
                orderdetails.OrderId = OrderID;
                orderdetails.ProductId = c.ProductID;
                orderdetails.Price = c.Price;
                orderdetails.Quantity = c.Quantity;
                orderdetails.Amount = c.Price * c.Quantity;
                db.Orderdetails.Add(orderdetails);
            }
            db.SaveChanges();
            var products = db.Products.ToList();
            foreach (var product in products)
            {
                foreach (var c in (List<ModelCart>)Session["Cart"])
                {
                    if (product.ID == c.ProductID)
                    {
                        product.Quantity = product.Quantity - c.Quantity;
                        db.SaveChanges();
                    }
                }
            }

            Session.Remove("Cart");
            Notification.set_flash("Bạn đã đặt hàng thành công!", "success");
            return Json(true);

        }

        public JsonResult Tesst()
        {
            if (Session["User_Name"] != null)
                return Json(1);
            return Json(0);
        }
        public JsonResult CheckAuth()
        {
            if (Session["User_Name"] != null)
                return Json(1);
            return Json(0);
        }
        [HttpPost]
        public ActionResult PaymentWithVnPay(string Email, string Address, string FullName, string Phone)
        {
            // Kiểm tra đăng nhập
            if (Session["User_Name"] == null)
            {
                return Json(new { result = 0 }); // Chưa đăng nhập
            }

            // Kiểm tra giỏ hàng
            if (Session["Cart"] == null)
            {
                return Json(new { result = -1 }); // Giỏ hàng trống
            }

            // Tạo đơn hàng mới
            var order = new MOrder();
            int user_id = Convert.ToInt32(Session["User_ID"]);
            order.Code = DateTime.Now.ToString("yyyyMMddhhMMss");
            order.CustemerId = user_id;
            order.CreateDate = DateTime.Now;
            order.DeliveryAddress = Address;
            order.DeliveryEmail = Email;
            order.DeliveryPhone = Phone;
            order.DeliveryName = FullName;
            order.Status = 1; // Đơn hàng mới
            order.DeliveryPaymentMethod = "VnPay";
            order.StatusPayment = 0; // Chưa thanh toán
            db.Orders.Add(order);
            db.SaveChanges();

            var OrderID = order.Id;
            double totalAmount = 0;

            // Thêm chi tiết đơn hàng
            foreach (var c in (List<ModelCart>)Session["Cart"])
            {
                var orderdetails = new MOrderdetail();
                orderdetails.OrderId = OrderID;
                orderdetails.ProductId = c.ProductID;
                orderdetails.Price = c.Price;
                orderdetails.Quantity = c.Quantity;
                orderdetails.Amount = c.Price * c.Quantity;
                totalAmount += orderdetails.Amount;
                db.Orderdetails.Add(orderdetails);
            }
            db.SaveChanges();

            // Tạo URL thanh toán VnPay
            var vnpay = new VnPayLibrary();
            var vnp_Returnurl = ConfigurationManager.AppSettings["Vnpay:ReturnUrl"];
            var vnp_Url = ConfigurationManager.AppSettings["Vnpay:Url"];
            var vnp_TmnCode = ConfigurationManager.AppSettings["Vnpay:TmnCode"];
            var vnp_HashSecret = ConfigurationManager.AppSettings["Vnpay:HashSecret"];

            vnpay.AddRequestData("vnp_Version", ConfigurationManager.AppSettings["Vnpay:Version"]);
            vnpay.AddRequestData("vnp_Command", ConfigurationManager.AppSettings["Vnpay:Command"]);
            vnpay.AddRequestData("vnp_TmnCode", vnp_TmnCode);
            vnpay.AddRequestData("vnp_Amount", (totalAmount * 100).ToString()); // Số tiền * 100 (VnPay yêu cầu)
            vnpay.AddRequestData("vnp_CreateDate", DateTime.Now.ToString("yyyyMMddHHmmss"));
            vnpay.AddRequestData("vnp_CurrCode", ConfigurationManager.AppSettings["Vnpay:CurrCode"]);
            vnpay.AddRequestData("vnp_IpAddr", VnPayLibrary.GetIpAddress(HttpContext));
            vnpay.AddRequestData("vnp_Locale", ConfigurationManager.AppSettings["Vnpay:Locale"]);
            vnpay.AddRequestData("vnp_OrderInfo", "Thanh toan don hang: " + order.Code);
            vnpay.AddRequestData("vnp_OrderType", "other");
            vnpay.AddRequestData("vnp_ReturnUrl", vnp_Returnurl);
            vnpay.AddRequestData("vnp_TxnRef", order.Id.ToString()); // Mã tham chiếu đến đơn hàng

            var paymentUrl = vnpay.CreateRequestUrl(vnp_Url, vnp_HashSecret);

            return Json(new { result = 1, paymentUrl = paymentUrl });
        }

        public ActionResult VnPayReturn()
        {
            if (Request.QueryString.Count > 0)
            {
                var vnpay = new VnPayLibrary();
                var vnp_HashSecret = ConfigurationManager.AppSettings["Vnpay:HashSecret"];

                foreach (string s in Request.QueryString)
                {
                    if (!string.IsNullOrEmpty(s) && s.StartsWith("vnp_"))
                    {
                        vnpay.AddResponseData(s, Request.QueryString[s]);
                    }
                }

                // Lấy thông tin từ Response
                var orderId = Convert.ToInt32(vnpay.GetResponseData("vnp_TxnRef"));
                var vnpayTranId = vnpay.GetResponseData("vnp_TransactionNo");
                var vnp_ResponseCode = vnpay.GetResponseData("vnp_ResponseCode");
                var vnp_SecureHash = Request.QueryString["vnp_SecureHash"];
                var vnp_Amount = Convert.ToInt64(vnpay.GetResponseData("vnp_Amount")) / 100;

                bool checkSignature = vnpay.ValidateSignature(vnp_SecureHash, vnp_HashSecret);

                if (checkSignature)
                {
                    // Tìm đơn hàng trong database
                    var order = db.Orders.Find(orderId);
                    if (order != null)
                    {
                        if (vnp_ResponseCode == "00")
                        {
                            // Thanh toán thành công
                            order.StatusPayment = 1; // Đã thanh toán
                            order.VnPayTransactionId = vnpayTranId;
                            db.SaveChanges();
                            // Cập nhật số lượng sản phẩm tồn kho 
                            var products = db.Products.ToList();
                            foreach (var product in products)
                            {
                                foreach (var c in (List<ModelCart>)Session["Cart"])
                                {
                                    if (product.ID == c.ProductID)
                                    {
                                        product.Quantity = product.Quantity - c.Quantity;
                                        db.SaveChanges();
                                    }
                                }
                            }

                            // Xóa giỏ hàng
                            Session.Remove("Cart");
                            //Notification.set_flash("Thanh toán thành công! Mã giao dịch: " + vnpayTranId, "success");
                            return View();
                        }
                        else
                        {
                            // Thanh toán thất bại
                            order.StatusPayment = 2; // Thanh toán thất bại
                            db.SaveChanges();
                            //Notification.set_flash("Thanh toán thất bại! Mã lỗi: " + vnp_ResponseCode, "error");
                            return View();
                        }
                    }
                }
                else
                {
                    Notification.set_flash("Có lỗi xảy ra trong quá trình xử lý", "error");
                }
            }

            return RedirectToAction("Checkout");
        }
    }
}