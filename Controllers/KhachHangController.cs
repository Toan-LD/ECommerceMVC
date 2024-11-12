using AutoMapper;
using ECommerceMVC.Data;
using ECommerceMVC.Helpers;
using ECommerceMVC.ViewModels;
using Microsoft.AspNetCore.Mvc;

namespace ECommerceMVC.Controllers;

public class KhachHangController : Controller
{
    private readonly Hshop2023Context db;
    private readonly IMapper _mapper;

    public KhachHangController(Hshop2023Context context, IMapper mapper)
    {
        db = context;
        _mapper = mapper;
    }

    [HttpGet]
    public IActionResult DangKy()
    {
        return View();
    }
    
    [HttpPost]
    public IActionResult DangKy(RegisterVM model, IFormFile Hinh)
    {
        if (ModelState.IsValid)
        {
            try
            {
                var khachhang = _mapper.Map<KhachHang>(model);
                khachhang.RandomKey = MyUtil.GenerateRandomKey();
                khachhang.MatKhau = model.MatKhau.ToMd5Hash(khachhang.RandomKey);
                khachhang.HieuLuc = true; // Se xu ly khi dung mail de active
                khachhang.VaiTro = 0;

                if (Hinh != null)
                {
                    khachhang.Hinh = MyUtil.UploadHinh(Hinh, "KhachHang");
                }

                db.Add(khachhang);
                db.SaveChanges();

                return RedirectToAction("Index", "HangHoa");
            }
            catch (Exception e)
            {
                
            }
        }
        
        return View();
    }
}

/*
  RandomKey: Hệ thống tự sinh khi Đăng ký, đổi mật khẩu
  HieuLuc = true/false
  VaiTro = 0 //default
  MaKH, MatKhau, HoTen, GioiTinh, NgaySinh?, DiaChi, DienThoai, Email, Hinh?    
  MatKhau trong DB = hash(MatKhau người dùng nhập + salt key/Random Key)
  Hash: MD5, SHA512(SHA2-512)
 
 */