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

        [Required(ErrorMessage = "??a ch? c? th? l� b?t bu?c.")]
        [StringLength(500, ErrorMessage = "??a ch? c? th? kh�ng ???c v??t qu� 500 k� t?.")]
        public string DiaChiCuThe { get; set; }

        [Required]
        public bool? MacDinh { get; set; }

        [Required(ErrorMessage = "S? ?i?n tho?i nh?n h�ng l� b?t bu?c.")]
        [StringLength(15, ErrorMessage = "S? ?i?n tho?i kh�ng ???c v??t qu� 15 k� t?.")]
        [RegularExpression(@"^(\d{10,15})$", ErrorMessage = "S? ?i?n tho?i kh�ng h?p l?.")]
        public string SoDienThoaiNhanHang { get; set; }

        [Required(ErrorMessage = "T�n ng??i nh?n l� b?t bu?c.")]
        [StringLength(255, ErrorMessage = "T�n ng??i nh?n kh�ng ???c v??t qu� 255 k� t?.")]
        public string TenNguoiNhan { get; set; }

        public int? MaTaiKhoan { get; set; }

        public virtual TaiKhoan TaiKhoan { get; set; }

        public virtual ICollection<DonHang> DonHangs { get; set; }
    }
}
