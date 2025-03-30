using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using _21BITV03_Nhom04_website_clothes.Data;
using System.Linq;

namespace _21BITV03_Nhom04_website_clothes.Controllers
{
    public class StatisticsController : Controller
    {
        private readonly WebsiteClothesContext _context;

        public StatisticsController(WebsiteClothesContext context)
        {
            _context = context;
        }

        public IActionResult Index()
        {
            SetGeneralStatistics();
            SetUserStatistics();
            SetTopUserStatistics();
            SetMonthlyRevenueStatistics();
            SetBestSellerStatistics();
            SetYearlyRevenueStatistics();
            SetOrderStatusStatistics();
            SetWarehouseStockStatistics();
            SetBusiestPeriodStatistics();

            return View();
        }
        private void SetBusiestPeriodStatistics()
        {
            var currentDate = DateTime.Now;
            var currentMonth = currentDate.Month;
            var currentYear = currentDate.Year;

            var ordersInMonth = _context.Orders
                .Where(o => o.OrderDate.HasValue &&
                            o.OrderDate.Value.Month == currentMonth &&
                            o.OrderDate.Value.Year == currentYear)
                .ToList();

            int firstPeriodOrders = ordersInMonth.Count(o => o.OrderDate!.Value.Day >= 1 && o.OrderDate!.Value.Day <= 10);
            int secondPeriodOrders = ordersInMonth.Count(o => o.OrderDate!.Value.Day >= 11 && o.OrderDate!.Value.Day <= 20);
            int thirdPeriodOrders = ordersInMonth.Count(o => o.OrderDate!.Value.Day >= 21);

            string busiestPeriod = "Đầu tháng";
            int maxOrders = firstPeriodOrders;

            if (secondPeriodOrders > maxOrders)
            {
                busiestPeriod = "Giữa tháng";
                maxOrders = secondPeriodOrders;
            }

            if (thirdPeriodOrders > maxOrders)
            {
                busiestPeriod = "Cuối tháng";
                maxOrders = thirdPeriodOrders;
            }

            ViewBag.FirstPeriodOrders = firstPeriodOrders;
            ViewBag.SecondPeriodOrders = secondPeriodOrders;
            ViewBag.ThirdPeriodOrders = thirdPeriodOrders;
            ViewBag.BusiestPeriod = busiestPeriod;
        }

        private void SetGeneralStatistics()
        {
            ViewBag.TotalProducts = _context.Products.Count();
            ViewBag.TotalSubProducts = _context.SubProducts.Count();
            ViewBag.TotalOrders = _context.Orders.Count();
            ViewBag.TotalRevenue = _context.Orders.Sum(o => (decimal?)o.OrderTotal) ?? 0;
            ViewBag.TotalStock = _context.WarehouseProducts.Sum(wp => (int?)wp.Quantity) ?? 0;
            ViewBag.TotalSuppliers = _context.Suppliers.Count();
            ViewBag.TotalWarehouses = _context.Warehouses.Count();
        }

        private void SetUserStatistics()
        {
            var totalUsers = _context.UserInfos.Count();
            var usersWithOrders = _context.Orders.Select(o => o.UserId).Distinct().Count();

            ViewBag.TotalUsers = totalUsers;
            ViewBag.UsersWithOrders = usersWithOrders;
            ViewBag.UsersWithoutOrders = totalUsers - usersWithOrders;
        }

        private void SetTopUserStatistics()
        {
            var topUser = _context.Orders
                .GroupBy(o => o.UserId)
                .Select(g => new { UserId = g.Key, OrderCount = g.Count() })
                .OrderByDescending(x => x.OrderCount)
                .FirstOrDefault();

            var topUserName = topUser != null
                ? _context.UserInfos
                    .Where(u => u.UserId == topUser.UserId)
                    .Select(u => u.FullName ?? u.UserName ?? "Không xác định")
                    .FirstOrDefault()
                : "Không có dữ liệu";

            ViewBag.TopUserName = topUserName;
            ViewBag.TopUserOrderCount = topUser?.OrderCount ?? 0;
        }

        private void SetMonthlyRevenueStatistics()
        {
            var currentYear = DateTime.Now.Year;
            var monthLabels = Enumerable.Range(1, 12).Select(m => $"Tháng {m}").ToList();
            var monthlyRevenue = Enumerable.Range(1, 12)
                .Select(m => _context.Orders
                    .Where(o => o.OrderDate.HasValue &&
                                o.OrderDate.Value.Month == m &&
                                o.OrderDate.Value.Year == currentYear &&
                                o.OrderStatus == "Completed")
                    .Sum(o => (decimal?)o.OrderTotal) ?? 0)
                .ToList();

            ViewBag.MonthLabels = monthLabels;
            ViewBag.MonthlyRevenue = monthlyRevenue;
        }

        private void SetBestSellerStatistics()
        {
            var bestSelling = _context.OrderProductLists
                .GroupBy(o => o.SubproductId)
                .Select(g => new { SubProductId = g.Key, QuantitySold = g.Sum(x => x.Quanity) })
                .OrderByDescending(x => x.QuantitySold)
                .FirstOrDefault();

            var subProductName = bestSelling != null
                ? _context.SubProducts
                    .Where(sp => sp.SubProductId == bestSelling.SubProductId)
                    .Select(sp => sp.MainProduct!.ProductName + " - " + sp.Status)
                    .FirstOrDefault()
                : "Không có dữ liệu";

            ViewBag.BestSeller = subProductName;
            ViewBag.BestSellerQuantity = bestSelling?.QuantitySold ?? 0;
        }

        private void SetYearlyRevenueStatistics()
        {
            var yearList = _context.Orders
                .Where(o => o.OrderDate.HasValue && o.OrderStatus == "Completed")
                .Select(o => o.OrderDate.Value.Year)
                .Distinct()
                .OrderBy(y => y)
                .ToList();

            var yearlyRevenue = yearList
                .Select(year => _context.Orders
                    .Where(o => o.OrderDate.HasValue &&
                                o.OrderDate.Value.Year == year &&
                                o.OrderStatus == "Completed")
                    .Sum(o => (decimal?)o.OrderTotal) ?? 0)
                .ToList();

            ViewBag.YearlyLabels = yearList;
            ViewBag.YearlyRevenue = yearlyRevenue;
        }

        private void SetOrderStatusStatistics()
        {
            var totalOrders = _context.Orders.Count();
            var completedOrders = _context.Orders.Count(o => o.OrderStatus == "Completed");
            var cancelledOrders = _context.Orders.Count(o => o.OrderStatus == "Cancelled");

            ViewBag.CompletedOrders = completedOrders;
            ViewBag.CancelledOrders = cancelledOrders;
            ViewBag.OtherOrders = totalOrders - (completedOrders + cancelledOrders);
        }

        private void SetWarehouseStockStatistics()
        {
            var warehouseProductCount = _context.WarehouseProducts
                .Include(wp => wp.Warehouse)
                .GroupBy(wp => wp.WarehouseId)
                .Select(g => new
                {
                    WarehouseId = g.Key,
                    WarehouseName = g.First().Warehouse.WarehouseName ?? $"Kho {g.Key}",
                    TotalQuantity = g.Sum(wp => wp.Quantity)
                })
                .ToList();

            ViewBag.WarehouseLabels = warehouseProductCount.Select(wp => wp.WarehouseName).ToList();
            ViewBag.WarehouseProductQuantities = warehouseProductCount.Select(wp => wp.TotalQuantity).ToList();
        }

    }
}