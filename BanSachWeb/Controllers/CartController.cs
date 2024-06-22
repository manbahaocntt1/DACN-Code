using BanSachWeb.Models;
using BanSachWeb.Models.Payments;
using BanSachWeb.ViewModels;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data.Odbc;
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

            var cart = GetCart();

            

            return View(cart);

        }

        private JsonResult AddToCartLogic(int maSach, int quantity, bool isSelected)
        {
            var sach = GetSachById(maSach);
            var cart = GetCart();
            if (cart == null) // if not logged in, cart will be null
            {
                Session["ReturnUrl"] = Request.UrlReferrer?.ToString(); // Store the return URL
                
                return Json(new { success = false, notLoggedIn = true, message = "Vui lòng đăng nhập." });
            }
            else
            {
                var cartItems = db.ChiTietGioHangs.Where(c => c.MaGioHang == cart.MaGioHang).ToList();
                foreach (var item in cartItems)
                {
                    item.IsSelected = false;
                }
                // Find the item in the cart
                var cartItem = cartItems.FirstOrDefault(c => c.MaSach == maSach);

                if (cartItem != null)
                {
                    // Update the quantity if the item already exists in the cart
                    cartItem.SoLuong += quantity;
                    cartItem.ThanhTien += quantity * db.Saches.Find(maSach).GiaBan;
                    cartItem.IsSelected = isSelected;
                }
                else
                {
                    // Add a new item to the cart
                    cartItem = new ChiTietGioHang
                    {
                        MaGioHang = cart.MaGioHang,
                        MaSach = maSach,
                        SoLuong = quantity,
                        ThanhTien = quantity * db.Saches.Find(maSach).GiaBan,
                        IsSelected = isSelected
                    };
                    db.ChiTietGioHangs.Add(cartItem);
                }

                db.SaveChanges();
                return Json(new { success = true, message = "Thêm vào giỏ hàng thành công." });
            }
        }

        [HttpPost]
        public ActionResult AddToCart(int maSach, int quantity, bool isSelected = false)
        {

             
            
            return AddToCartLogic(maSach, quantity, isSelected);
        }

        [HttpPost]
        public ActionResult BuyNow(int maSach, int quantity = 1)
        {
            var result = AddToCartLogic(maSach, quantity, true); // Set isSelected to true
            var jsonResult = result as JsonResult;
            if (jsonResult != null)
            {
                var jsonData = jsonResult.Data as dynamic;
                if (jsonData.success == false && jsonData.message == "Vui lòng đăng nhập.")
                {
                    return RedirectToAction("Login", "Account");
                }
            }
            return RedirectToAction("Index", "Cart");
        }


        public ActionResult RemoveFromCart(int maSach)
        {
            var cart = GetCart(); // Assuming you have a method to get the cart
            var itemToRemove = cart.ChiTietGioHangs.FirstOrDefault(i => i.MaSach == maSach);
            if (itemToRemove != null)
            {
                cart.ChiTietGioHangs.Remove(itemToRemove);
            }
            db.SaveChanges();

            return RedirectToAction("Index", "Cart"); // Redirect to the cart page

            
        }

        public ActionResult ClearCart()
        {
            var cart = GetCart();
            cart.ChiTietGioHangs.Clear();
            db.SaveChanges();
            return RedirectToAction("Index", "Cart");
        }
        //Hàm xử lý logic mỗi tài khoản chỉ có 1 giỏ hàng duy nhất 
        private GioHang GetCart()
        {

            if (Session["MaTaiKhoan"] != null)
            {
                int maTaiKhoan = (int)Session["MaTaiKhoan"];
                var cart= db.GioHangs.FirstOrDefault(g => g.MaTaiKhoan == maTaiKhoan);
                if (cart != null)
                {
                    return cart;
                }
                else
                {
                    cart = new GioHang { MaTaiKhoan = maTaiKhoan };
                    db.GioHangs.Add(cart);
                    db.SaveChanges();
                    return cart;
                }
            }
            return null;


        }
       


        private Sach GetSachById(int id)
        {
           
                return db.Saches.Find(id);
         
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

            var cartItem = cart.ChiTietGioHangs.FirstOrDefault(item => item.MaSach == maSach);

            if (cartItem != null)
            {
                // Cập nhật số lượng của sản phẩm trong giỏ hàng
                cartItem.SoLuong = newQuantity;
                cartItem.ThanhTien = sach.GiaBan * newQuantity; // Assuming sach.GiaBan is not null

                // Lưu lại giỏ hàng đã cập nhật vào Session hoặc cơ sở dữ liệu
                db.SaveChanges();
            }


            // Trả về một phản hồi không có nội dung, chỉ cần thông báo rằng yêu cầu đã được xử lý thành công
            return RedirectToAction("Index");
        }


        public int CountItemInCart()
        {
            var cart = GetCart();
            int countItem = 0;
            if (cart != null && cart.ChiTietGioHangs != null)
            {
                foreach (var item in cart.ChiTietGioHangs)
                {
                    countItem += item.SoLuong ?? 0; // Assuming SoLuong is the property representing the quantity of each item
                }
            }
            return countItem;
        }
        public int CreateOrder(string paymentMethod, List<int> selectedProducts)
        {
            var username = Session["emailOrPhone"]?.ToString();
            if (string.IsNullOrEmpty(username))
            {
                System.Diagnostics.Debug.WriteLine("CreateOrder failed: Missing user session.");
                return -1; // Indicate that no order was created due to missing user session
            }

            var account = db.TaiKhoans.FirstOrDefault(s => s.Email == username);
            if (account == null)
            {
                System.Diagnostics.Debug.WriteLine("CreateOrder failed: Invalid account.");
                return -1; // Indicate that no order was created due to invalid account
            }

            var cart = GetCart();
            if (cart == null || !cart.ChiTietGioHangs.Any())
            {
                System.Diagnostics.Debug.WriteLine("CreateOrder failed: Empty cart.");
                return -1; // Indicate that no order was created due to empty cart
            }

            var selectedItems = cart.ChiTietGioHangs
                .Where(item => item.MaSach.HasValue && selectedProducts.Contains(item.MaSach.Value))
                .ToList();

            if (!selectedItems.Any())
            {
                System.Diagnostics.Debug.WriteLine("CreateOrder failed: No selected items.");
                return -1; // Indicate that no order was created due to no selected items
            }

            var defaultAddress = db.DiaChis.FirstOrDefault(d => d.MaTaiKhoan == account.MaTaiKhoan && d.MacDinh == true);
            
           

            decimal totalOrderPrice = selectedItems.Sum(item => item.ThanhTien ?? 0); // Calculate the total price of the order
            var order = new DonHang
            {
                ThoiGianDatHang = DateTime.Now,
                TrangThai = "Chờ xác nhận",
                TongGiaTri = totalOrderPrice,
                MaTaiKhoan = account.MaTaiKhoan,
                PhuongThucThanhToan = paymentMethod,

                MaDiaChi = defaultAddress?.MaDiaChi,
                ChiTietDonHangs = selectedItems.Select(item => new ChiTietDonHang
                {
                    MaSach = item.MaSach,
                    SoLuong = item.SoLuong,
                    GiaBan = item.Sach.GiaBan,
                    ThanhTien = item.ThanhTien
                }).ToList()
            };

            try
            {
                db.DonHangs.Add(order);
                // Update the SoLuongDaBan for each selected item
                foreach (var item in selectedItems)
                {
                    var product = db.Saches.Find(item.MaSach);
                    if (product != null)
                    {
                        product.SoLuongDaBan = (product.SoLuongDaBan ?? 0) + item.SoLuong;
                        System.Diagnostics.Debug.WriteLine("New sold quantity: " + product.SoLuongDaBan);

                    }
                }
                db.SaveChanges();
              
                System.Diagnostics.Debug.WriteLine("CreateOrder succeeded: Order ID " + order.MaDonHang);
                return order.MaDonHang; // Return the order ID
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine("CreateOrder failed: Exception " + ex.Message);
                return -1; // Indicate that no order was created due to an exception
            }
        }


        [HttpPost]
        public ActionResult CheckOut(string paymentMethod, List<int> selectedProducts)
        {
            // Log received data
            System.Diagnostics.Debug.WriteLine("Payment Method: " + paymentMethod);
            System.Diagnostics.Debug.WriteLine("Selected Products: " + string.Join(", ", selectedProducts));

            int orderId = CreateOrder(paymentMethod, selectedProducts);
            if (orderId == -1)
            {
                return Json(new { success = false, message = "Order creation failed" });
            }
            Session["oderID"]=orderId;

            return Json(new { success = true, orderId = orderId });
        }




        public ActionResult OrderConfirmation(int orderId)
        {
            var order = db.DonHangs.Include("ChiTietDonHangs").FirstOrDefault(s => s.MaDonHang == orderId);
            if(Session["madiachi"]!=null)
            {
                var MaDiaChi = (int)Session["madiachi"];
                order.DiaChi=db.DiaChis.Where(t=> t.MaDiaChi==MaDiaChi).FirstOrDefault();
                System.Diagnostics.Debug.WriteLine("New address " + order.MaDiaChi);

            }
            
            if (order == null)
            {
                System.Diagnostics.Debug.WriteLine("Errors");
                return HttpNotFound();
            }
            ViewBag.SuccessMessage = TempData["SuccessMessage"];
            ViewBag.ErrorMessage = TempData["ErrorMessage"];
            return View(order);
        }
        



        //Hàm xử lý chức năng thanh toán
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
        public ActionResult ConfirmOrder(int orderId, string paymentMethod)
        {
            if (paymentMethod.Equals("cash", StringComparison.OrdinalIgnoreCase))
            {
                // Handle cash payment, show success message
                TempData["SuccessMessage"] = "Your order has been placed successfully!";
                return RedirectToAction("OrderConfirmation", new { orderId = orderId });
            }
            else if (paymentMethod.Equals("online", StringComparison.OrdinalIgnoreCase))
            {
                // Redirect to VNPAY for online payment
                return RedirectToAction("OnlinePayment", new { typePaymentVN = 1, orderId = orderId }); // Adjust typePaymentVN as needed
            }
            else
            {
                // Handle invalid payment method
                TempData["ErrorMessage"] = "Invalid payment method.";
                return RedirectToAction("OrderConfirmation", new { orderId = orderId });
            }
        }
        //View list of oders
        public ActionResult UserOrders()
        {
            // Assuming you store the username in the session
            var maTaiKhoan = Session["MaTaiKhoan"] as int?; ;

            if (maTaiKhoan == null)
            {
                return RedirectToAction("Login", "Account"); // Redirect to login if session expired
            }

            // Fetch orders for the logged-in user
            var orders = db.DonHangs.Where(o => o.MaTaiKhoan== maTaiKhoan).ToList(); // Adjust property names as needed

            return View(orders); // Pass orders to the view
        }
        public ActionResult OrderDetails(int id)
        {
            var order = db.DonHangs.Include("ChiTietDonHangs")
                                   .FirstOrDefault(o => o.MaDonHang == id);

            if (order == null)
            {
                return HttpNotFound();
            }

            return View(order);
        }
        public ActionResult CancelOrder(int id)
        {
            var order = db.DonHangs.FirstOrDefault(o => o.MaDonHang == id);
            if (order == null)
            {
                return HttpNotFound();
            }

            

            order.TrangThai = "cancelled"; // or any status indicating the order is cancelled
            db.SaveChanges();

            return RedirectToAction("UserOrders");
        }

    }
}
