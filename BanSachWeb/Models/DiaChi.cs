namespace BanSachWeb.Models
{
    using System;
    using System.Collections.Generic;
    using System.ComponentModel.DataAnnotations;
    using System.ComponentModel.DataAnnotations.Schema;
    using System.Data.Entity.Spatial;

    [Table("DiaChi")]
    public partial class DiaChi
    {
        public DiaChi()
        {
            DonHangs = new HashSet<DonHang>();
        }

        [Key]
        public int MaDiaChi { get; set; }

        [Required(ErrorMessage = "??a ch? c? th? là b?t bu?c.")]
        [StringLength(500, ErrorMessage = "??a ch? c? th? không ???c v??t quá 500 ký t?.")]
        public string DiaChiCuThe { get; set; }

        [Required]
        public bool? MacDinh { get; set; }

        [Required(ErrorMessage = "S? ?i?n tho?i nh?n hàng là b?t bu?c.")]
        [StringLength(15, ErrorMessage = "S? ?i?n tho?i không ???c v??t quá 15 ký t?.")]
        [RegularExpression(@"^(\d{10,15})$", ErrorMessage = "S? ?i?n tho?i không h?p l?.")]
        public string SoDienThoaiNhanHang { get; set; }

        [Required(ErrorMessage = "Tên ng??i nh?n là b?t bu?c.")]
        [StringLength(255, ErrorMessage = "Tên ng??i nh?n không ???c v??t quá 255 ký t?.")]
        public string TenNguoiNhan { get; set; }

        public int? MaTaiKhoan { get; set; }

        public virtual TaiKhoan TaiKhoan { get; set; }

        public virtual ICollection<DonHang> DonHangs { get; set; }
    }
}
