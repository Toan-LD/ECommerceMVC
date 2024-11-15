using System.Security.Claims;
using AutoMapper;
using ECommerceMVC.Data;
using ECommerceMVC.Helpers;
using ECommerceMVC.ViewModels;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Authorization;
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

    #region Register
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
    
    #endregion

    #region Login

    [HttpGet]
    public IActionResult DangNhap(string? ReturnUrl)
    {
        ViewBag.ReturnUrl = ReturnUrl;
        return View();
    }
    
    
    
    [HttpPost]
    public async Task<ActionResult> DangNhap(LoginVM model, string? ReturnUrl)
    {
        ViewBag.ReturnUrl = ReturnUrl;
        if (ModelState.IsValid)
        {
            var khachHang = db.KhachHangs.SingleOrDefault(kh => kh.MaKh == model.UserName);
            if (khachHang == null)
            {
                ModelState.AddModelError("Lỗi", "Sai thông tin đăng nhập");
            }
            else
            {
                if (!khachHang.HieuLuc)
                {
                    ModelState.AddModelError("Lỗi", "Tài khoản đã bị khóa. Vui lòng liên hệ admin");
                }
                else 
                {
                    if(khachHang.MatKhau != model.Password.ToMd5Hash(khachHang.RandomKey))
                        ModelState.AddModelError("Lỗi", "Sai thông tin đăng nhập");
                    else
                    {
                        var claims = new List<Claim>
                        {
                            new Claim(ClaimTypes.Email, khachHang.Email),
                            new Claim(ClaimTypes.Name, khachHang.HoTen),
                            new Claim("Customer ID", khachHang.MaKh),
                            
                            //claim - role động
                            new Claim(ClaimTypes.Role, "Customer")
                        };

                        var claimsIdentity = new ClaimsIdentity(claims, CookieAuthenticationDefaults.AuthenticationScheme);
                        var claimsPrincipal = new ClaimsPrincipal(claimsIdentity);

                        await HttpContext.SignInAsync(claimsPrincipal);

                        if (Url.IsLocalUrl(ReturnUrl))
                        {
                            return Redirect(ReturnUrl);
                        }
                        else
                        {
                            return Redirect("/");
                        }
                    }
                }
            }
        }
        return View();
    }
    
    #endregion
    
    
    [Authorize]
    public IActionResult Profile()
    {
        return View();
    }

    [Authorize]
    public async Task<IActionResult> DangXuat()
    {
        await HttpContext.SignOutAsync();
        return Redirect("/");
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