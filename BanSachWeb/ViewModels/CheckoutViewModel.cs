﻿using BanSachWeb.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace BanSachWeb.ViewModels
{
    public class CheckoutViewModel
    {
        public DonHang MaDonHang {  get; set; }
        public GioHang GioHang { get; set; }
        public TaiKhoan TaiKhoan { get; set; }
        public DiaChi DiaChi { get; set; }
        public decimal totalPrice
        {
            get
            {
                return GioHang.GetTotalPrice();
            }
        }
    }
}