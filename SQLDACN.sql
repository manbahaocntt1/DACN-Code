USE master
GO

IF EXISTS (SELECT name FROM sys.sysdatabases WHERE name = 'CuaHangSachDB')
    DROP DATABASE CuaHangSachDB

CREATE DATABASE CuaHangSachDB
GO

USE CuaHangSachDB
GO

-- Bảng Sách
CREATE TABLE Sach (
    MaSach INT PRIMARY KEY IDENTITY(1,1),
    TenSach NVARCHAR(255),
    AnhSach NVARCHAR(255),
    GiaGoc DECIMAL(10, 2),
    GiaBan DECIMAL(10, 2),
    SoLuongDaBan INT,
    SoLuongConDu INT,
    TomTat NVARCHAR(MAX),
    NhaXuatBan NVARCHAR(255),
    NamXuatBan INT,
    HinhThuc NVARCHAR(255),
    SoTrang INT,
    KichThuoc NVARCHAR(50),
    TrongLuong FLOAT,
    MaTacGia INT,
    MaDanhMuc INT,
    Visible BIT
);

-- Bảng Tác giả
CREATE TABLE TacGia (
    MaTacGia INT PRIMARY KEY IDENTITY(1,1),
    TenTacGia NVARCHAR(255),
    AnhMinhHoa NVARCHAR(255),
    MoTa NVARCHAR(MAX),
    Visible BIT
);



-- Bảng Danh mục chính
CREATE TABLE DanhMucChinh (
    MaDanhMucChinh INT PRIMARY KEY IDENTITY(1,1),
    TenDanhMuc NVARCHAR(255),
    MoTa NVARCHAR(MAX),
    Visible BIT
);

-- Bảng Danh mục phụ
CREATE TABLE DanhMucPhu (
    MaDanhMucPhu INT PRIMARY KEY IDENTITY(1,1),
    TenDanhMuc NVARCHAR(255),
    MoTa NVARCHAR(MAX),
    Visible BIT,
    MaDanhMucChinh INT,
    FOREIGN KEY (MaDanhMucChinh) REFERENCES DanhMucChinh(MaDanhMucChinh)
);

-- Bảng Chi nhánh
CREATE TABLE ChiNhanh (
    MaChiNhanh INT PRIMARY KEY IDENTITY(1,1),
    TenChiNhanh NVARCHAR(255),
    DiaChi NVARCHAR(MAX),
    GioiThieu NVARCHAR(MAX)
);

-- Bảng Sách - Chi nhánh
CREATE TABLE Sach_ChiNhanh (
    MaSach INT,
    MaChiNhanh INT,
    PRIMARY KEY (MaSach, MaChiNhanh),
    FOREIGN KEY (MaSach) REFERENCES Sach(MaSach),
    FOREIGN KEY (MaChiNhanh) REFERENCES ChiNhanh(MaChiNhanh)
);

-- Bảng Tài khoản
CREATE TABLE TaiKhoan (
    MaTaiKhoan INT PRIMARY KEY IDENTITY(1,1),
    TenDangNhap NVARCHAR(255),
    MatKhau NVARCHAR(255),
    Email NVARCHAR(255),
    HoTen NVARCHAR(255),
    SoDienThoai NVARCHAR(15),
    VaiTro NVARCHAR(50),
    DiemThuong INT
);

-- Bảng Địa chỉ
CREATE TABLE DiaChi (
    MaDiaChi INT PRIMARY KEY IDENTITY(1,1),
    DiaChiCuThe NVARCHAR(MAX),
    MacDinh BIT,
    SoDienThoaiNhanHang NVARCHAR(15),
    TenNguoiNhan NVARCHAR(255),
    MaTaiKhoan INT,
    FOREIGN KEY (MaTaiKhoan) REFERENCES TaiKhoan(MaTaiKhoan)
);

-- Bảng Đơn hàng
CREATE TABLE DonHang (
    MaDonHang INT PRIMARY KEY IDENTITY(1,1),
    ThoiGianDatHang DATETIME,
    TrangThai NVARCHAR(100),
    DonViVanChuyen NVARCHAR(255),
    ThoiGianGiaoHangDuKien DATETIME,
    PhuongThucThanhToan NVARCHAR(100),
    MaQR NVARCHAR(255),
    PhiVanChuyen DECIMAL(10, 2),
    TongGiaTri DECIMAL(10, 2),
    LoiNhuan DECIMAL(10, 2),
    DaThanhToan BIT,
    MaTaiKhoan INT,
    MaDiaChi INT,
    FOREIGN KEY (MaTaiKhoan) REFERENCES TaiKhoan(MaTaiKhoan),
    FOREIGN KEY (MaDiaChi) REFERENCES DiaChi(MaDiaChi)
);

-- Bảng Chi tiết đơn hàng
CREATE TABLE ChiTietDonHang (
    MaChiTietDonHang INT PRIMARY KEY IDENTITY(1,1),
    SoLuong INT,
    GiaBan DECIMAL(10, 2),
    ThanhTien DECIMAL(10, 2),
    MaDonHang INT,
    MaSach INT,
    FOREIGN KEY (MaDonHang) REFERENCES DonHang(MaDonHang),
    FOREIGN KEY (MaSach) REFERENCES Sach(MaSach)
);

-- Bảng Khuyến mãi
CREATE TABLE KhuyenMai (
    MaKhuyenMai INT PRIMARY KEY IDENTITY(1,1),
    MoTa NVARCHAR(MAX),
    MucGiam DECIMAL(5, 2),
    DieuKienApDung NVARCHAR(MAX),
    ThoiGianBatDau DATETIME,
    ThoiGianKetThuc DATETIME,
    MaChiNhanh INT,
    FOREIGN KEY (MaChiNhanh) REFERENCES ChiNhanh(MaChiNhanh)
);

-- Bảng Khuyến mãi - Tài khoản
CREATE TABLE KhuyenMai_TaiKhoan (
    MaKhuyenMai INT,
    MaTaiKhoan INT,
    PRIMARY KEY (MaKhuyenMai, MaTaiKhoan),
    FOREIGN KEY (MaKhuyenMai) REFERENCES KhuyenMai(MaKhuyenMai),
    FOREIGN KEY (MaTaiKhoan) REFERENCES TaiKhoan(MaTaiKhoan)
);

-- Bảng Khuyến mãi - Sách
CREATE TABLE KhuyenMai_Sach (
    MaKhuyenMai INT,
    MaSach INT,
    PRIMARY KEY (MaKhuyenMai, MaSach),
    FOREIGN KEY (MaKhuyenMai) REFERENCES KhuyenMai(MaKhuyenMai),
    FOREIGN KEY (MaSach) REFERENCES Sach(MaSach)
);

-- Bảng Giỏ hàng
CREATE TABLE GioHang (
    MaGioHang INT PRIMARY KEY IDENTITY(1,1),
    MaTaiKhoan INT,
    FOREIGN KEY (MaTaiKhoan) REFERENCES TaiKhoan(MaTaiKhoan)
);

-- Bảng Chi tiết giỏ hàng
CREATE TABLE ChiTietGioHang (
    MaChiTietGioHang INT PRIMARY KEY IDENTITY(1,1),
    MaGioHang INT,
    MaSach INT,
    SoLuong INT,
    ThanhTien DECIMAL(10, 2),
    FOREIGN KEY (MaGioHang) REFERENCES GioHang(MaGioHang),
    FOREIGN KEY (MaSach) REFERENCES Sach(MaSach)
);

-- Bảng Đơn hàng - Khuyến mãi
CREATE TABLE DonHang_KhuyenMai (
    MaKhuyenMai INT,
    MaDonHang INT,
    PRIMARY KEY (MaKhuyenMai, MaDonHang),
    FOREIGN KEY (MaKhuyenMai) REFERENCES KhuyenMai(MaKhuyenMai),
    FOREIGN KEY (MaDonHang) REFERENCES DonHang(MaDonHang)
);

-- Bảng Phản hồi
CREATE TABLE PhanHoi (
    MaPhanHoi INT PRIMARY KEY IDENTITY(1,1),
    NoiDung NVARCHAR(MAX),
    DiemDanhGia INT,
    MaSach INT,
    MaTaiKhoan INT,
    FOREIGN KEY (MaSach) REFERENCES Sach(MaSach),
    FOREIGN KEY (MaTaiKhoan) REFERENCES TaiKhoan(MaTaiKhoan)
);

-- insert data
-- Account 
INSERT INTO TaiKhoan VALUES ('HaoMB', 'Leo15823', 'manbahao1508@gmail.com', 'Man Ba Hao', '0968518163', 'admin', 0);

-- Insert sample data into DanhMucChinh
INSERT INTO DanhMucChinh (TenDanhMuc, MoTa, Visible) VALUES
(N'Văn học', N'Danh mục sách văn học', 1),
(N'Khoa học', N'Danh mục sách khoa học', 1),
(N'Lịch sử', N'Danh mục sách lịch sử', 1),
(N'Thiếu nhi', N'Danh mục sách thiếu nhi', 1),
(N'Ngoại ngữ', N'Danh mục sách ngoại ngữ', 1);

-- Insert sample data into DanhMucPhu
INSERT INTO DanhMucPhu (TenDanhMuc, MoTa, Visible, MaDanhMucChinh) VALUES
(N'Tiểu thuyết', N'Sách tiểu thuyết', 1, 1), -- Assuming 200 is the ID for Văn học
(N'Truyện ngắn', N'Sách truyện ngắn', 1, 1),
(N'Vật lý', N'Sách vật lý', 1, 2),           -- Assuming 201 is the ID for Khoa học
(N'Hóa học', N'Sách hóa học', 1, 2),
(N'Tiếng Anh', N'Sách tiếng Anh', 1, 5);     -- Assuming 204 is the ID for Ngoại ngữ

-- Inserting authors into the TacGia table
INSERT INTO TacGia (TenTacGia, AnhMinhHoa, MoTa, Visible)
VALUES
(N'Gabriel Garcia Marquez', N'gabriel-garcia-marquez.jpg', N'Tác giả nổi tiếng của Colombia.', 1),
(N'Victor Hugo', N'victor-hugo.jpg', N'Tác giả nổi tiếng của Pháp.', 1),
(N'Nam Cao', N'nam-cao.jpg', N'Tác giả nổi tiếng của Việt Nam.', 1),
(N'Haruki Murakami', N'haruki-murakami.jpg', N'Tác giả nổi tiếng của Nhật Bản.', 1),
(N'Ngô Tất Tố', N'ngo-tat-to.jpg', N'Tác giả nổi tiếng của Việt Nam.', 1),
(N'Tô Hoài', N'to-hoai.jpg', N'Tác giả nổi tiếng của Việt Nam.', 1),
(N'Nguyễn Minh Châu', N'nguyen-minh-chau.jpg', N'Tác giả nổi tiếng của Việt Nam.', 1),
(N'Nguyễn Tuân', N'nguyen-tuan.jpg', N'Tác giả nổi tiếng của Việt Nam.', 1),
(N'Maxim Gorky', N'maxim-gorky.jpg', N'Tác giả nổi tiếng của Nga.', 1),
(N'Stephen Hawking', N'stephen-hawking.jpg', N'Tác giả nổi tiếng của Anh.', 1),
(N'Albert Einstein', N'albert-einstein.jpg', N'Tác giả nổi tiếng của Đức.', 1),
(N'Nguyễn Bính', N'nguyen-binh.jpg', N'Tác giả nổi tiếng của Việt Nam.', 1),
(N'J.K. Rowling', N'jk-rowling.jpg', N'Tác giả nổi tiếng của Anh.', 1),
(N'Mark Twain', N'mark-twain.jpg', N'Tác giả nổi tiếng của Mỹ.', 1),
(N'Nguyễn Du', N'nguyen-du.jpg', N'Tác giả nổi tiếng của Việt Nam.', 1);


 -- Insert Books into the Sach Table
-- Tiểu thuyết
INSERT INTO Sach (TenSach, AnhSach, GiaGoc, GiaBan, SoLuongDaBan, SoLuongConDu, TomTat, NhaXuatBan, NamXuatBan, HinhThuc, SoTrang, KichThuoc, TrongLuong, MaTacGia, MaDanhMuc, Visible)
VALUES 
(N'Trăm năm cô đơn', N'tram-nam-co-don.jpg', 150000, 120000, 500, 200, N'Một tiểu thuyết nổi tiếng của Gabriel Garcia Marquez.', N'NXB Văn Học', 1982, N'Bìa mềm', 500, N'13x20 cm', 0.5, 1, 1, 1),
(N'Những người khốn khổ', N'nhung-nguoi-khon-kho.jpg', 200000, 180000, 300, 150, N'Tiểu thuyết của Victor Hugo.', N'NXB Trẻ', 2005, N'Bìa cứng', 1000, N'15x24 cm', 1.2, 2, 1, 1),
(N'Chí Phèo', N'chi-pheo.jpg', 100000, 90000, 200, 100, N'Tiểu thuyết của Nam Cao.', N'NXB Hội Nhà Văn', 1941, N'Bìa mềm', 200, N'12x19 cm', 0.3, 3, 1, 1),
(N'Rừng Na Uy', N'rung-na-uy.jpg', 180000, 160000, 150, 100, N'Tiểu thuyết của Haruki Murakami.', N'NXB Văn Học', 1987, N'Bìa mềm', 450, N'14x21 cm', 0.6, 4, 1, 1),
(N'Tắt đèn', N'tat-den.jpg', 120000, 110000, 300, 200, N'Tiểu thuyết của Ngô Tất Tố.', N'NXB Hội Nhà Văn', 1939, N'Bìa mềm', 250, N'13x20 cm', 0.4, 5, 1, 1),

-- Truyện ngắn
(N'Vợ chồng A Phủ', N'vo-chong-a-phu.jpg', 90000, 80000, 400, 250, N'Truyện ngắn của Tô Hoài.', N'NXB Văn Học', 1952, N'Bìa mềm', 150, N'13x20 cm', 0.2, 6, 2, 1),
(N'Chiếc thuyền ngoài xa', N'chiec-thuyen-ngoai-xa.jpg', 110000, 100000, 350, 200, N'Truyện ngắn của Nguyễn Minh Châu.', N'NXB Trẻ', 1983, N'Bìa mềm', 180, N'14x21 cm', 0.3, 7, 2, 1),
(N'Người lái đò sông Đà', N'nguoi-lai-do-song-da.jpg', 95000, 85000, 300, 150, N'Truyện ngắn của Nguyễn Tuân.', N'NXB Văn Học', 1960, N'Bìa mềm', 200, N'13x20 cm', 0.3, 8, 2, 1),
(N'Thời thơ ấu', N'thoi-tho-au.jpg', 130000, 120000, 150, 100, N'Truyện ngắn của Maxim Gorky.', N'NXB Trẻ', 1913, N'Bìa mềm', 250, N'14x21 cm', 0.4, 9, 2, 1),
(N'Đôi mắt', N'doi-mat.jpg', 100000, 90000, 200, 100, N'Truyện ngắn của Nam Cao.', N'NXB Hội Nhà Văn', 1950, N'Bìa mềm', 180, N'12x19 cm', 0.2, 3, 2, 1),

-- Vật lý
(N'Vũ trụ trong vỏ hạt dẻ', N'vu-tru-trong-vo-hat-de.jpg', 250000, 220000, 100, 50, N'Sách khoa học nổi tiếng của Stephen Hawking.', N'NXB Tri Thức', 2001, N'Bìa cứng', 320, N'16x24 cm', 0.8, 10, 3, 1),
(N'Bí mật của ánh sáng', N'bi-mat-cua-anh-sang.jpg', 180000, 150000, 120, 80, N'Sách về vật lý ánh sáng.', N'NXB Khoa Học', 2010, N'Bìa mềm', 280, N'14x21 cm', 0.6, 11, 3, 1),
(N'Lý thuyết tương đối', N'ly-thuyet-tuong-doi.jpg', 200000, 180000, 150, 100, N'Sách về lý thuyết tương đối của Albert Einstein.', N'NXB Giáo Dục', 1995, N'Bìa mềm', 300, N'15x23 cm', 0.7, 12, 3, 1),
(N'Vật lý lượng tử', N'vat-ly-luong-tu.jpg', 220000, 200000, 130, 90, N'Sách về vật lý lượng tử.', N'NXB Khoa Học', 2005, N'Bìa mềm', 350, N'16x24 cm', 0.9, 13, 3, 1),
(N'Bách khoa toàn thư vật lý', N'bach-khoa-toan-thu-vat-ly.jpg', 300000, 280000, 170, 120, N'Sách bách khoa toàn thư về vật lý.', N'NXB Tri Thức', 2010, N'Bìa cứng', 500, N'17x25 cm', 1.1, 14, 3, 1),

-- Hóa học
(N'Hóa học đại cương', N'hoa-hoc-dai-cuong.jpg', 210000, 190000, 140, 100, N'Sách hóa học cơ bản.', N'NXB Giáo Dục', 2000, N'Bìa mềm', 400, N'15x22 cm', 0.8, 15, 4, 1),
(N'Hóa học hữu cơ', N'hoa-hoc-huu-co.jpg', 250000, 230000, 160, 110, N'Sách về hóa học hữu cơ.', N'NXB Khoa Học', 2005, N'Bìa mềm', 450, N'16x23 cm', 0.9, 16, 4, 1),
(N'Hóa học vô cơ', N'hoa-hoc-vo-co.jpg', 220000, 200000, 130, 90, N'Sách về hóa học vô cơ.', N'NXB Tri Thức', 2010, N'Bìa mềm', 350, N'15x22 cm', 0.7, 17, 4, 1),
(N'Hóa học polymer', N'hoa-hoc-polymer.jpg', 280000, 260000, 110, 70, N'Sách về hóa học polymer.', N'NXB Giáo Dục', 2015, N'Bìa mềm', 500, N'16x24 cm', 1.0, 18, 4, 1),
(N'Hóa học phân tích', N'hoa-hoc-phan-tich.jpg', 230000, 210000, 120, 80, N'Sách về hóa học phân tích.', N'NXB Khoa Học', 2020, N'Bìa mềm', 380, N'15x23 cm', 0.9, 19, 4, 1),

-- Tiếng Anh
(N'Tiếng Anh cơ bản', N'tieng-anh-co-ban.jpg', 180000, 160000, 200, 150, N'Sách học tiếng Anh cơ bản.', N'NXB Giáo Dục', 2015, N'Bìa mềm', 300, N'14x21 cm', 0.6, 20, 5, 1),
(N'Tiếng Anh giao tiếp', N'tieng-anh-giao-tiep.jpg', 190000, 170000, 220, 160, N'Sách học tiếng Anh giao tiếp.', N'NXB Trẻ', 2018, N'Bìa mềm', 320, N'14x22 cm', 0.7, 21, 5, 1),
(N'Từ vựng tiếng Anh', N'tu-vung-tieng-anh.jpg', 160000, 140000, 180, 130, N'Sách từ vựng tiếng Anh.', N'NXB Tri Thức', 2020, N'Bìa mềm', 280, N'13x20 cm', 0.5, 22, 5, 1),
(N'Tiếng Anh cho người đi làm', N'tieng-anh-cho-nguoi-di-lam.jpg', 200000, 180000, 150, 100, N'Sách tiếng Anh chuyên dụng cho người đi làm.', N'NXB Giáo Dục', 2021, N'Bìa mềm', 350, N'15x23 cm', 0.8, 23, 5, 1),
(N'Luyện thi TOEIC', N'luyen-thi-toeic.jpg', 220000, 200000, 170, 120, N'Sách luyện thi TOEIC.', N'NXB Khoa Học', 2019, N'Bìa mềm', 400, N'16x24 cm', 0.9, 24, 5, 1);


