using BanSachWeb.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using PagedList;
namespace BanSachWeb.Controllers
{
    public class SachController : Controller
    {
        // GET: Sach
        QuanLyBanSachModel db=new QuanLyBanSachModel();
        
        public ActionResult Index(int? page)
        {
            var product=db.Saches.ToList();
            int pageSize = 10;
            int pageNumber = (page ?? 1);
            var products = db.Saches.OrderBy(p => p.MaSach).ToPagedList(pageNumber, pageSize);
            return View(products);
        }
        // GET: Book/GetCategories
        public ActionResult GetCategories()
        {
            var categories = db.DanhMucChinhs.Include("DanhMucPhus").ToList();
            return PartialView("GetCategories", categories);
        }

    }
}