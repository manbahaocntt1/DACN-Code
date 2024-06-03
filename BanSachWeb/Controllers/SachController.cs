using BanSachWeb.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using PagedList;
using BanSachWeb.ViewModels;
using System.Data.Entity;
namespace BanSachWeb.Controllers
{
    public class SachController : Controller
    {
        // GET: Sach
        QuanLyBanSachModel db=new QuanLyBanSachModel();
        
        public ActionResult Index(int? page)
        {
            ViewBag.Title = "Sách";
            ViewBag.nameAction = "Index";
            int pageSize = 10;
            int pageNumber = (page ?? 1);
            var products = GetBookQuery().OrderBy(s => s.MaSach).ToPagedList(pageNumber, pageSize);
            return View("Index", products);
        }
        // GET: Book/GetCategories
        public ActionResult GetCategories()
        {
            var categories = db.DanhMucChinhs.Include("DanhMucPhus").ToList();
            return PartialView("GetCategories", categories);
        }
       

        public IQueryable<BookViewModel> GetBookQuery()
        {
            return db.Saches.AsNoTracking()
                .Select(s => new BookViewModel
                {
                    MaSach = s.MaSach,
                    TenSach = s.TenSach,
                    SoLuongDaBan = s.SoLuongDaBan,
                    AnhSach = s.AnhSach,
                    GiaBan = s.GiaBan,
                    GiaGoc = s.GiaGoc
                });
        }
        public IQueryable<BookViewModel> GetTopBookQuery(int amount = 13)
        {

            return db.Saches.AsNoTracking()
                .Select(s => new BookViewModel
                {
                    MaSach = s.MaSach,
                    TenSach = s.TenSach,
                    SoLuongDaBan = s.SoLuongDaBan,
                    AnhSach = s.AnhSach,
                    GiaBan = s.GiaBan,
                    GiaGoc = s.GiaGoc
                }).OrderByDescending(s => s.SoLuongDaBan).Take(amount);
        }
        public IQueryable<BookViewModel> GetSearchedBookQuery(List<Sach> sachs)
        {
            return sachs.Select(s => new BookViewModel
            {
                MaSach = s.MaSach,
                TenSach = s.TenSach,
                SoLuongDaBan = s.SoLuongDaBan,
                AnhSach = s.AnhSach,
                GiaBan = s.GiaBan,
                GiaGoc = s.GiaGoc
            }).AsQueryable();
        }

        


        public ActionResult SachDetail(int id)
        {
            // Assuming you have a DbContext called 'db'
            var sach = db.Saches.Include(s => s.TacGias).FirstOrDefault(s => s.MaSach == id);


            if (sach == null)
            {
                // Handle case where book with the given ID is not found
                return HttpNotFound(); // Or any other appropriate action, such as redirecting to an error page
            }

            return View(sach); // Pass the retrieved book object to the view
        }
        public ActionResult TopSale(int? page)
        {
            int pageNumber = (page ?? 1);
            int pageSize = 10;
            var topBooks = GetTopBookQuery().ToPagedList(pageNumber, pageSize);
            ViewBag.Title = "Sách bán chạy";
            ViewBag.nameAction = "TopSale";
            return View("Index", topBooks);
        }
        public PartialViewResult TopSalePartial()
        {
            var topSaleBooks = GetTopBookQuery(4);
            return PartialView("_TopSale", topSaleBooks);
        }
        public ActionResult Search(SearchViewModel model, int? page)
        {
            var sachViewModelList = db.Saches.Include(s => s.TacGias)
                .Where(s => string.IsNullOrEmpty(model.SearchKey) ||
                            s.TacGias.Any(a => a.TenTacGia.Contains(model.SearchKey) || s.TenSach.Contains(model.SearchKey)))
                .Select(s => new BookViewModel
                {
                    MaSach = s.MaSach,
                    TenSach = s.TenSach,
                    SoLuongDaBan = s.SoLuongDaBan,
                    AnhSach = s.AnhSach,
                    GiaBan = s.GiaBan,
                    GiaGoc = s.GiaGoc
                    // Map other properties here if necessary
                }).ToList();

            // Convert SachViewModel to Sach if necessary
            model.BooksResult = sachViewModelList.Select(s => new Sach
            {
                MaSach = s.MaSach,
                TenSach = s.TenSach,
                SoLuongDaBan = s.SoLuongDaBan,
                AnhSach = s.AnhSach,
                GiaBan = s.GiaBan,
                GiaGoc = s.GiaGoc
                // Map other properties here if necessary
            }).ToList();

            ViewBag.Title = "Sách";
            ViewBag.nameAction = "Search";

            int pageSize = 10;
            int pageNumber = (page ?? 1);
            var book = GetSearchedBookQuery(model.BooksResult).ToPagedList(pageNumber, pageSize);

            return View("Index", book);
        }

    }
}