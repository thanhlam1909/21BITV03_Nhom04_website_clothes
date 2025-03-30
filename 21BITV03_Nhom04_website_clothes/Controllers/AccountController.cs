using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Mvc;
using Microsoft.VisualStudio.Web.CodeGenerators.Mvc.Templates.BlazorIdentity.Pages;
using System.Security.Claims;
using _21BITV03_Nhom04_website_clothes.Models;
using Microsoft.AspNetCore.Authorization;
using _21BITV03_Nhom04_website_clothes.Data;
using Microsoft.EntityFrameworkCore;
using _21BITV03_Nhom04_website_clothes.Helper;
using ClosedXML.Excel;
//using OfficeOpenXml.Style;
//using OfficeOpenXml;

namespace _21BITV03_Nhom04_website_clothes.Controllers
{
    public class AccountController : Controller
    {
        private readonly WebsiteClothesContext _context;

        public AccountController(WebsiteClothesContext context)
        {
            _context = context;
        }

        // Trang tài khoản người dùng
        public async Task<IActionResult> Index()
        {
            var redirectResult = NavigationHelper.RedirectToRoleBasedPage(this);
            if (redirectResult != null)
            {
                return redirectResult;
            }

            var username = HttpContext.User.Identity.Name; // Lấy tên người dùng từ phiên đăng nhập
            if (username == null)
            {
                return Json(new { success = false, message = "Người dùng chưa đăng nhập." });
            }

            // Lấy thông tin người dùng dựa theo tên đăng nhập
            var userInfo = await _context.UserInfos
                .Include(u => u.User) // Bao gồm thông tin người dùng từ bảng AspNetUser
                .Include(u => u.Orders) // Bao gồm đơn hàng của người dùng
                    .ThenInclude(o => o.OrderProductLists)
                        .ThenInclude (or => or.Product)// Bao gồm chi tiết sản phẩm trong đơn hàng
                .Where(u => u.UserName == username)
                .FirstOrDefaultAsync();

            if (userInfo == null)
            {
                return Json(new { success = false, message = "Không tìm thấy người dùng." });
            }

            // Chuẩn bị dữ liệu cho ViewModel hiển thị lên giao diện
            var accountViewModel = new AccountViewModel
            {
                Users = userInfo.User, // Thông tin người dùng từ AspNetUser
                UserInfoes = userInfo, // Thông tin từ bảng UserInfo
                Orders = userInfo.Orders.FirstOrDefault(), // Lấy đơn hàng đầu tiên (ví dụ)
                OrderProductLists = userInfo.Orders.SelectMany(o => o.OrderProductLists).ToList() // Danh sách tất cả sản phẩm trong đơn hàng
            };

            return View(accountViewModel); // Truyền ViewModel cho View để hiển thị
        }
        [HttpGet]
        [ActionName("ExportOrdersToExcel")]
        public async Task<IActionResult> ExportOrdersToExcel(int orderId)
        {
            var username = HttpContext.User.Identity.Name;
            if (username == null)
            {
                return Json(new { success = false, message = "Người dùng chưa đăng nhập." });
            }

            var userInfo = await _context.UserInfos
                .Include(u => u.User)
                .Include(u => u.Orders)
                    .ThenInclude(o => o.OrderProductLists)
                .FirstOrDefaultAsync(u => u.UserName == username);

            if (userInfo == null || userInfo.Orders == null || !userInfo.Orders.Any())
            {
                return Json(new { success = false, message = "Không tìm thấy dữ liệu." });
            }

            // Tìm đơn hàng theo OrderId
            var orderToExport = userInfo.Orders.FirstOrDefault(o => o.OrderId == orderId);
            if (orderToExport == null || orderToExport.OrderProductLists == null)
            {
                return Json(new { success = false, message = "Không tìm thấy đơn hàng này." });
            }

            try
            {
                using (var workbook = new XLWorkbook())
                {
                    var worksheet = workbook.Worksheets.Add("Order");

                    // Headers
                    worksheet.Cell(1, 1).Value = "Mã sản phẩm";
                    worksheet.Cell(1, 2).Value = "Tên sản phẩm";
                    worksheet.Cell(1, 3).Value = "Số lượng";
                    worksheet.Cell(1, 4).Value = "Màu";
                    worksheet.Cell(1, 5).Value = "Kích thước";
                    worksheet.Cell(1, 6).Value = "Chất liệu";
                    worksheet.Cell(1, 7).Value = "Giá";
                    worksheet.Cell(1, 8).Value = "Tổng giá"; // Moved to column 9 for Total Price

                    worksheet.Range("A1:H1").Style.Font.Bold = true;
                    worksheet.Range("A1:H1").Style.Fill.SetBackgroundColor(XLColor.LightGray);

                    int row = 2;
                    double orderTotal = 0;

                    // Loop through products in the selected order
                    foreach (var item in orderToExport.OrderProductLists)
                    {
                        double totalPrice = (item.Price ?? 0) * (item.Quanity ?? 0);
                        orderTotal += totalPrice;

                        worksheet.Cell(row, 1).Value = orderToExport.OrderId;
                        worksheet.Cell(row, 2).Value = item.ProductName ?? "N/A";
                        worksheet.Cell(row, 3).Value = item.Quanity ?? 0;
                        worksheet.Cell(row, 4).Value = item.ColorName ?? "N/A";
                        worksheet.Cell(row, 5).Value = item.SizeName ?? "N/A";
                        worksheet.Cell(row, 6).Value = item.MaterialName ?? "N/A";
                        worksheet.Cell(row, 7).Value = item.Price ?? 0.0;
                        worksheet.Cell(row, 8).Value = totalPrice; // Total Price for each product moved to column 9

                        row++;
                    }
                    // Tổng cộng dòng cuối
                    worksheet.Cell(row, 7).Value = "Tổng cộng:";
                    worksheet.Cell(row, 7).Style.Font.Bold = true;

                    worksheet.Cell(row, 8).Value = orderTotal;
                    worksheet.Cell(row, 8).Style.Font.Bold = true;
                    worksheet.Cell(row, 8).Style.NumberFormat.Format = "#,##0 \"VNĐ\"";

                    // Tô nền dòng tổng cộng cho dễ nhìn
                    worksheet.Range(row, 7, row, 8).Style.Fill.BackgroundColor = XLColor.LightGreen;

                    // AutoFit columns
                    worksheet.Columns().AdjustToContents();

                    var stream = new MemoryStream();
                    workbook.SaveAs(stream);
                    stream.Position = 0;

                    string excelName = $"Order_{orderToExport.OrderId}_{DateTime.Now:yyyyMMddHHmmss}.xlsx";
                    return File(stream, "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet", excelName);
                }
            }
            catch (Exception ex)
            {
                return Json(new { success = false, message = "Đã xảy ra lỗi khi xuất file: " + ex.Message });
            }
        }

        // Đăng xuất
        public async Task<IActionResult> LogOut()
        {
            await HttpContext.SignOutAsync(CookieAuthenticationDefaults.AuthenticationScheme);
            return RedirectToAction("Login", "Account");
        }

        // Kiểm tra người dùng đã đăng nhập chưa
        [HttpGet]
        public IActionResult IsUserLoggedIn()
        {
            return Json(User.Identity.IsAuthenticated);
        }

        // Trang đăng nhập (GET)
        public IActionResult Login()
        {
            ClaimsPrincipal claimUser = HttpContext.User;

            // Nếu người dùng đã đăng nhập, chuyển hướng về trang chính
            if (claimUser.Identity.IsAuthenticated)
                return RedirectToAction("Index", "Account");

            return View();
        }

        // Xử lý đăng nhập (POST)
        [HttpPost]
        public async Task<IActionResult> Login(LoginViewModel modelLogin)
        {
            if (!ModelState.IsValid)
            {
                ViewData["ValidateMessage"] = "Vui lòng nhập dữ liệu hợp lệ.";
                return View(modelLogin);
            }

            var user = await _context.AspNetUsers
                .Include(u => u.Roles) // Bao gồm thông tin vai trò của người dùng
                .FirstOrDefaultAsync(u => u.UserName == modelLogin.UserName);

            if (user != null && modelLogin.UserName == user.UserName && modelLogin.PassWord == user.PasswordHash)
            {
                // Lấy vai trò của người dùng
                var userRole = user.Roles.FirstOrDefault();
                var roleName = userRole?.Name;

                List<Claim> claims = new List<Claim>
                {
                    new Claim(ClaimTypes.NameIdentifier, modelLogin.UserName),
                    new Claim(ClaimTypes.Name, user.UserName),
                    new Claim(ClaimTypes.Role, roleName ?? "User") // Gán vai trò mặc định nếu không có
                };

                ClaimsIdentity claimsIdentity = new ClaimsIdentity(claims, CookieAuthenticationDefaults.AuthenticationScheme);

                AuthenticationProperties properties = new AuthenticationProperties
                {
                    AllowRefresh = true,
                    IsPersistent = modelLogin.KeepLoggedIn
                };

                await HttpContext.SignInAsync(CookieAuthenticationDefaults.AuthenticationScheme, new ClaimsPrincipal(claimsIdentity), properties);

                // Chuyển hướng dựa trên vai trò
                if (roleName == "Admin")
                {
                    return RedirectToAction("Index", "Admin");
                }
                else
                {
                    return RedirectToAction("Index", "Home"); // Trang mặc định cho người dùng thường
                }
            }

            ViewData["ValidateMessage"] = "Không tìm thấy người dùng hoặc mật khẩu không đúng.";
            return View(modelLogin);
        }

        // Trang đăng ký (GET)
        [HttpGet]
        public IActionResult Register()
        {
            ViewBag.Roles = _context.AspNetRoles.ToList();
            return View();
        }

        // Xử lý đăng ký (POST)
        [HttpPost]
        public async Task<IActionResult> Register(UserViewModel model)
        {
            if (ModelState.IsValid)
            {
                // Kiểm tra tên đăng nhập đã tồn tại chưa
                var existingUser = await _context.AspNetUsers.FirstOrDefaultAsync(u => u.UserName == model.UserName);
                if (existingUser != null)
                {
                    ModelState.AddModelError("UserName", "Tên đăng nhập đã tồn tại. Vui lòng chọn tên khác.");
                    ViewBag.Roles = _context.AspNetRoles.ToList();
                    return View(model);
                }

                var user = new AspNetUser
                {
                    UserName = model.UserName,
                    Email = model.Email,
                    PasswordHash = model.Password, // Lưu ý: cần mã hóa mật khẩu trong thực tế
                    PhoneNumber = model.Phone.ToString(),
                    UserInfo = new UserInfo
                    {
                        FullName = model.FullName,
                        Address = model.Address,
                        Phone = model.Phone,
                        Email = model.Email,
                        Password = model.Password,
                        UserName = model.UserName
                    }
                };

                // Gán vai trò "user" cho người dùng mới
                var role = await _context.AspNetRoles.FirstOrDefaultAsync(r => r.Name == "user");
                if (role != null)
                {
                    user.Roles.Add(role);
                }

                _context.AspNetUsers.Add(user);
                await _context.SaveChangesAsync();

                return RedirectToAction("Login");
            }

            ViewBag.Roles = _context.AspNetRoles.ToList();
            return View(model);
        }

        // Trang truy cập bị từ chối
        public IActionResult AccessDenied()
        {
            return View();
        }
    }
}
