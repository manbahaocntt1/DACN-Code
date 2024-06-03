using BanSachWeb.Models;
using BanSachWeb.Models.Payments;
using BanSachWeb.ViewModels;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.Linq;
using System.Net;
using System.Web;
using System.Web.Mvc;

namespace BanSachWeb.Controllers
{
    public class CartController : Controller
    {
        QuanLyBanSachModel db=new QuanLyBanSachModel();
        // GET: Cart
        public ActionResult Index()
        {
            
            if (Session["emailOrPhone"] == null)
            {
               
                return RedirectToAction("Login", "Account");
            }
            else
            {
                var username = Session["emailOrPhone"].ToString();
                var account = db.TaiKhoans.Where(s => s.Email == username).FirstOrDefault();
                var cart = GetCart();
                TempData["totalPrice"] = cart.GetTotalPrice();
                var viewModel = new CheckoutViewModel
                {
                    TaiKhoan = account,
                    GioHang = cart
                };
                return View(viewModel);
            }
            
        }

        [HttpPost]
        public ActionResult AddToCart(int maSach, int quantity, string type)
        {
            var sach = GetSachById(maSach);
            var totalPrice = sach.GiaBan * quantity;
            var cart = GetCart();
            cart.AddItem(sach, quantity);
            SaveCart(cart);

            // Kiểm tra xem tham số type có giá trị "ajax" hay không
            if (type == "ajax")
            {
                // Trả về một phản hồi JSON nếu yêu cầu là AJAX
                return Json(new { success = true });
            }

            return RedirectToAction("Index");
        }


        public ActionResult RemoveFromCart(int maSach)
        {
            var cart = GetCart();
            cart.RemoveItem(maSach);
            SaveCart(cart);
            return RedirectToAction("Index");
        }

        public ActionResult ClearCart()
        {
            Session.Remove("Cart");
            return RedirectToAction("Index");
        }

        private GioHang GetCart()
        {
            var cart = (GioHang)Session["Cart"];
            if (cart == null)
            {
                cart = new GioHang();
                Session["Cart"] = cart;
            }
            return cart;
        }

        private void SaveCart(GioHang cart)
        {
            Session["Cart"] = cart;
        }

        private Sach GetSachById(int id)
        {
           
                return db.Saches.Find(id);
         
        }

        public int CountItemInCart()
        {
            var cart = GetCart();
            int countItem = 0;
            if (cart != null)
            {
                foreach (var item in cart.GetItems())
                {
                    countItem += item.quantity;
                }
            }
            return countItem;
        }
        public int CreateOrder(string paymentMethod)
        {
            var username = Session["username"]?.ToString();
            if (string.IsNullOrEmpty(username))
            {
                return -1; // Indicate that no order was created due to missing user session
            }

            var account = db.TaiKhoans.FirstOrDefault(s => s.Email == username);
            if (account == null)
            {
                return -1; // Indicate that no order was created due to invalid account
            }

            var cart = GetCart();
            if (cart == null || !cart.GetItems().Any())
            {
                return -1; // Indicate that no order was created due to empty cart
            }

            var order = new DonHang
            {
                ThoiGianDatHang = DateTime.Now,
                TrangThai = "Đã tiếp nhận",
                TongGiaTri = cart.GetTotalPrice(),
                MaTaiKhoan = account.MaTaiKhoan,
                PhuongThucThanhToan = paymentMethod,
                ChiTietDonHangs = cart.GetItems().Select(item => new ChiTietDonHang
                {
                    MaSach = item.product.MaSach,
                    SoLuong = item.quantity,
                    GiaBan = item.product.GiaBan,
                    ThanhTien = item.product.GiaBan * item.quantity
                }).ToList()
            };

            db.DonHangs.Add(order);
            db.SaveChanges();
            ClearCart();
            return order.MaDonHang; // Return the order ID
        }
        [HttpPost]
        public JsonResult GenerateOrder(string paymentMethod)
        {
            int orderId = CreateOrder(paymentMethod);
            if (orderId == -1)
            {
                return Json(new { success = false, message = "Order creation failed" });
            }
            return Json(new { success = true, orderId = orderId });
        }

        public ActionResult CheckOut(string paymentMethod)
        {
            int orderId = CreateOrder(paymentMethod);
            if (orderId == -1)
            {
                return RedirectToAction("Index"); // Redirect to cart if order creation fails
            }

            return RedirectToAction("OrderConfirmation", "Cart", new { orderId = orderId });
        }
        public ActionResult OnlinePayment(int typePaymentVN, int orderId)
        {
            var paymentUrl = UrlPayment(typePaymentVN, orderId);
            return Redirect(paymentUrl);
        }
        public ActionResult PaymentReturn()
        {
            var vnpayData = Request.QueryString;
            VnPayLibrary vnpay = new VnPayLibrary();
            foreach (string s in vnpayData)
            {
                vnpay.AddResponseData(s, vnpayData[s]);
            }
            string vnp_HashSecret = ConfigurationManager.AppSettings["vnp_HashSecret"];
            bool checkSignature = vnpay.ValidateSignature(vnpay.GetResponseData("vnp_SecureHash"), vnp_HashSecret);
            if (checkSignature)
            {
                // Handle payment result
                // e.g. update order status, show success message, etc.
                return RedirectToAction("CheckOut");
            }
            else
            {
                // Handle invalid signature
                return View("PaymentError");
            }
        }
        [HttpPost]
        public ActionResult UpdateQuantity(int maSach, int newQuantity)
        {
            // Lấy sản phẩm từ cơ sở dữ liệu dựa trên mã sách
            var sach = GetSachById(maSach);
            if (sach == null)
            {
                return HttpNotFound();
            }

            // Kiểm tra xem số lượng mới có hợp lệ không
            if (newQuantity < 1)
            {
                // Trả về một phản hồi lỗi nếu số lượng không hợp lệ
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }

            // Lấy giỏ hàng của người dùng từ Session hoặc cơ sở dữ liệu
            var cart = GetCart();

            // Cập nhật số lượng của sản phẩm trong giỏ hàng
            cart.UpdateQuantity(maSach, newQuantity);

            // Lưu lại giỏ hàng đã cập nhật vào Session hoặc cơ sở dữ liệu
            SaveCart(cart);

            // Trả về một phản hồi không có nội dung, chỉ cần thông báo rằng yêu cầu đã được xử lý thành công
            return RedirectToAction("Index");
        }

        public ActionResult OrderConfirmation(int orderId)
        {
            var order = db.DonHangs.Include("ChiTietDonHangs").FirstOrDefault(s => s.MaDonHang == orderId);
            if(order == null)
            {
                return HttpNotFound();
            }
            return View(order);
            
        }
        public string UrlPayment(int typePaymentVN,int orderCode)
        {
            var urlPayment = "";
            var order = db.DonHangs.FirstOrDefault(x => x.MaDonHang == orderCode);
            //Get Config Info
            string vnp_Returnurl = ConfigurationManager.AppSettings["vnp_Returnurl"]; //URL nhan ket qua tra ve 
            string vnp_Url = ConfigurationManager.AppSettings["vnp_Url"]; //URL thanh toan cua VNPAY 
            string vnp_TmnCode = ConfigurationManager.AppSettings["vnp_TmnCode"]; //Ma định danh merchant kết nối (Terminal Id)
            string vnp_HashSecret = ConfigurationManager.AppSettings["vnp_HashSecret"]; //Secret Key

            //Get payment input
            //OrderInfo order = new OrderInfo();
            //order.OrderId = DateTime.Now.Ticks; // Giả lập mã giao dịch hệ thống merchant gửi sang VNPAY
            //order.Amount = 100000; // Giả lập số tiền thanh toán hệ thống merchant gửi sang VNPAY 100,000 VND
            //order.Status = "0"; //0: Trạng thái thanh toán "chờ thanh toán" hoặc "Pending" khởi tạo giao dịch chưa có IPN
            //order.CreatedDate = DateTime.Now;
            //Save order to db

            //Build URL for VNPAY
            VnPayLibrary vnpay = new VnPayLibrary();
            var price = (long)(order.TongGiaTri * 100);
            vnpay.AddRequestData("vnp_Version", VnPayLibrary.VERSION);
            vnpay.AddRequestData("vnp_Command", "pay");
            vnpay.AddRequestData("vnp_TmnCode", vnp_TmnCode);
            vnpay.AddRequestData("vnp_Amount", price.ToString()); //Số tiền thanh toán. Số tiền không mang các ký tự phân tách thập phân, phần nghìn, ký tự tiền tệ. Để gửi số tiền thanh toán là 100,000 VND (một trăm nghìn VNĐ) thì merchant cần nhân thêm 100 lần (khử phần thập phân), sau đó gửi sang VNPAY là: 10000000
            if (typePaymentVN == 1)
            {
                vnpay.AddRequestData("vnp_BankCode", "VNPAYQR");
            }
            else if (typePaymentVN == 2)
            {
                vnpay.AddRequestData("vnp_BankCode", "VNBANK");
            }
            else if (typePaymentVN == 3)
            {
                vnpay.AddRequestData("vnp_BankCode", "INTCARD");
            }
            var time = DateTime.Now;
            vnpay.AddRequestData("vnp_CreateDate", time.ToString("yyyyMMddHHmmss"));
            vnpay.AddRequestData("vnp_CurrCode", "VND");
            vnpay.AddRequestData("vnp_IpAddr", Utils.GetIpAddress());

            vnpay.AddRequestData("vnp_Locale", "vn");
            vnpay.AddRequestData("vnp_OrderInfo", "Thanh toán đơn hàng:" + order.MaDonHang);
            vnpay.AddRequestData("vnp_OrderType", "other"); //default value: other

            vnpay.AddRequestData("vnp_ReturnUrl", vnp_Returnurl);
            vnpay.AddRequestData("vnp_TxnRef", order.MaDonHang.ToString()); // Mã tham chiếu của giao dịch tại hệ thống của merchant. Mã này là duy nhất dùng để phân biệt các đơn hàng gửi sang VNPAY. Không được trùng lặp trong ngày

            //Add Params of 2.1.0 Version
            //Billing

            urlPayment = vnpay.CreateRequestUrl(vnp_Url, vnp_HashSecret);
            return urlPayment;
        }
    }
}
