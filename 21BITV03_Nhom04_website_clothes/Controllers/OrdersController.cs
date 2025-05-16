using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using _21BITV03_Nhom04_website_clothes.Data;
using Microsoft.AspNetCore.Authorization;
using ClosedXML.Excel;

namespace _21BITV03_Nhom04_website_clothes.Controllers
{
    [Authorize]
    [Authorize(Roles = "Admin")]
    public class OrdersController : Controller
    {
        private readonly WebsiteClothesContext _context;

        public OrdersController(WebsiteClothesContext context)
        {
            _context = context;
        }

        // GET: Orders
        public async Task<IActionResult> Index()
        {
            var websiteClothesContext = _context.Orders.Include(o => o.User);
            return View(await websiteClothesContext.ToListAsync());
        }


        // GET: Orders/Create
        public IActionResult Create()
        {
            ViewData["UserId"] = new SelectList(_context.UserInfos, "UserId", "UserId");
            return View();
        }

        // POST: Orders/Create
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create([Bind("OrderId,UserId,Message,PaymentMethod,OrderStatus")] Order order)
        {
            if (ModelState.IsValid)
            {
                _context.Add(order);
                await _context.SaveChangesAsync();
                return RedirectToAction(nameof(Index));
            }
            ViewData["UserId"] = new SelectList(_context.UserInfos, "UserId", "UserId", order.UserId);
            return View(order);
        }

        // GET: Orders/Edit/5
        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var order = await _context.Orders.FindAsync(id);
            if (order == null)
            {
                return NotFound();
            }
            ViewData["UserId"] = new SelectList(_context.UserInfos, "UserId", "UserId", order.UserId);
            return View(order);
        }

        // POST: Orders/Edit/5
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, [Bind("OrderId,UserId,Message,PaymentMethod,OrderStatus")] Order order)
        {
            if (id != order.OrderId)
            {
                return NotFound();
            }

            if (ModelState.IsValid)
            {
                try
                {
                    _context.Update(order);
                    await _context.SaveChangesAsync();
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!OrderExists(order.OrderId))
                    {
                        return NotFound();
                    }
                    else
                    {
                        throw;
                    }
                }
                return RedirectToAction(nameof(Index));
            }
            ViewData["UserId"] = new SelectList(_context.UserInfos, "UserId", "UserId", order.UserId);
            return View(order);
        }

        // GET: Orders/Delete/5
        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var order = await _context.Orders
                .Include(o => o.User)
                .FirstOrDefaultAsync(m => m.OrderId == id);
            if (order == null)
            {
                return NotFound();
            }

            return View(order);
        }

        // POST: Orders/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var order = await _context.Orders.FindAsync(id);
            if (order != null)
            {
                _context.Orders.Remove(order);
            }

            await _context.SaveChangesAsync();
            return RedirectToAction(nameof(Index));
        }

        private bool OrderExists(int id)
        {
            return _context.Orders.Any(e => e.OrderId == id);
        }
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> UpdateStatusToCancelled(int id)
        {
            var order = await _context.Orders.FindAsync(id);
            if (order == null)
            {
                return NotFound();
            }

            order.OrderStatus = "Cancelled";
            order.DeliveryDate = DateTime.Now;
            await _context.SaveChangesAsync();
            return RedirectToAction(nameof(Index));
        }

        // New method to update order status to "Shipped"
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> UpdateStatusToShipped(int id)
        {
            var order = await _context.Orders.FindAsync(id);
            if (order == null)
            {
                return NotFound();
            }

            order.OrderStatus = "Shipped";
            order.DeliveryDate = DateTime.Now;
            await _context.SaveChangesAsync();
            return RedirectToAction(nameof(Index));
        }

        // New method to update order status to "Completed"
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> UpdateStatusToCompleted(int id)
        {
            var order = await _context.Orders.FindAsync(id);
            if (order == null)
            {
                return NotFound();
            }

            order.OrderStatus = "Completed";
            order.DeliveryDate = DateTime.Now;
            await _context.SaveChangesAsync();
            return RedirectToAction(nameof(Index));
        }
        /*Nếu Muốn chung một sheet thì dùng cái này*/
        [HttpPost]
        public async Task<IActionResult> ExportToExcel()
        {
            var orders = await _context.Orders
                .Include(o => o.User)
                .Include(o => o.OrderProductLists)
                .ToListAsync();

            using var workbook = new XLWorkbook();
            var worksheet = workbook.Worksheets.Add("Orders");

            // Header đơn hàng
            var headers = new[]
            {
        "Order ID", "Customer Name", "Order Date", "Delivery Date",
        "Payment Method", "Status", "Total",
        "Product Name", "Quantity", "Color", "Size", "Material", "Price"
    };

            for (int i = 0; i < headers.Length; i++)
            {
                worksheet.Cell(1, i + 1).Value = headers[i];
                worksheet.Cell(1, i + 1).Style.Font.Bold = true;
                worksheet.Cell(1, i + 1).Style.Fill.BackgroundColor = XLColor.LightGray;
            }

            int row = 2;
            foreach (var order in orders)
            {
                if (order.OrderProductLists?.Any() == true)
                {
                    foreach (var detail in order.OrderProductLists)
                    {
                        worksheet.Cell(row, 1).Value = order.OrderId;
                        worksheet.Cell(row, 2).Value = order.User?.FullName ?? "N/A";
                        worksheet.Cell(row, 3).Value = order.OrderDate;
                        worksheet.Cell(row, 3).Style.DateFormat.Format = "dd/MM/yyyy";

                        worksheet.Cell(row, 4).Value = order.DeliveryDate;
                        worksheet.Cell(row, 4).Style.DateFormat.Format = "dd/MM/yyyy";

                        worksheet.Cell(row, 5).Value = order.PaymentMethod ?? "";
                        worksheet.Cell(row, 6).Value = order.OrderStatus ?? "";

                        worksheet.Cell(row, 7).Value = order.OrderTotal;
                        worksheet.Cell(row, 7).Style.NumberFormat.Format = "#,##0.00";

                        worksheet.Cell(row, 8).Value = detail.ProductName ?? "";
                        worksheet.Cell(row, 9).Value = detail.Quanity;
                        worksheet.Cell(row, 10).Value = detail.ColorName ?? "";
                        worksheet.Cell(row, 11).Value = detail.SizeName ?? "";
                        worksheet.Cell(row, 12).Value = detail.MaterialName ?? "";
                        worksheet.Cell(row, 13).Value = detail.Price;
                        worksheet.Cell(row, 13).Style.NumberFormat.Format = "#,##0.00";

                        row++;
                    }
                }
                else
                {
                    // Nếu đơn hàng không có sản phẩm
                    worksheet.Cell(row, 1).Value = order.OrderId;
                    worksheet.Cell(row, 2).Value = order.User?.FullName ?? "N/A";
                    worksheet.Cell(row, 3).Value = order.OrderDate;
                    worksheet.Cell(row, 3).Style.DateFormat.Format = "dd/MM/yyyy";

                    worksheet.Cell(row, 4).Value = order.DeliveryDate;
                    worksheet.Cell(row, 4).Style.DateFormat.Format = "dd/MM/yyyy";

                    worksheet.Cell(row, 5).Value = order.PaymentMethod ?? "";
                    worksheet.Cell(row, 6).Value = order.OrderStatus ?? "";
                    worksheet.Cell(row, 7).Value = order.OrderTotal;
                    worksheet.Cell(row, 7).Style.NumberFormat.Format = "#,##0.00";

                    row++;
                }
            }

            // Autofit và thêm viền
            worksheet.Columns().AdjustToContents();
            var usedRange = worksheet.RangeUsed();
            usedRange.Style.Border.OutsideBorder = XLBorderStyleValues.Thin;
            usedRange.Style.Border.InsideBorder = XLBorderStyleValues.Thin;

            using var stream = new MemoryStream();
            workbook.SaveAs(stream);
            stream.Position = 0;

            return File(stream.ToArray(),
                "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet",
                "Orders_WithDetails.xlsx");
        }

        /* Nếu muốn chia ra nhiều sheet cho hóa đơn chi tiết*/
        //[HttpPost]
        //public async Task<IActionResult> ExportToExcel()
        //{
        //    var orders = await _context.Orders
        //        .Include(o => o.User)
        //        .Include(o => o.OrderProductLists)
        //            .ThenInclude(od => od.Product)
        //        .ToListAsync();

        //    using var workbook = new XLWorkbook();

        //    // Sheet 1: Orders Summary
        //    var orderSheet = workbook.Worksheets.Add("Orders");
        //    orderSheet.Cell(1, 1).Value = "Order ID";
        //    orderSheet.Cell(1, 2).Value = "Customer Name";
        //    orderSheet.Cell(1, 3).Value = "Order Date";
        //    orderSheet.Cell(1, 4).Value = "Delivery Date";
        //    orderSheet.Cell(1, 5).Value = "Payment Method";
        //    orderSheet.Cell(1, 6).Value = "Status";
        //    orderSheet.Cell(1, 7).Value = "Total";

        //    var headerRange = orderSheet.Range("A1:G1");
        //    headerRange.Style.Font.Bold = true;
        //    headerRange.Style.Fill.BackgroundColor = XLColor.LightGray;
        //    headerRange.Style.Alignment.Horizontal = XLAlignmentHorizontalValues.Center;

        //    int orderRow = 2;
        //    foreach (var order in orders)
        //    {
        //        orderSheet.Cell(orderRow, 1).Value = order.OrderId;
        //        orderSheet.Cell(orderRow, 2).Value = order.User?.FullName;
        //        orderSheet.Cell(orderRow, 3).Value = order.OrderDate;
        //        orderSheet.Cell(orderRow, 3).Style.DateFormat.Format = "dd/MM/yyyy";

        //        orderSheet.Cell(orderRow, 4).Value = order.DeliveryDate;
        //        orderSheet.Cell(orderRow, 4).Style.DateFormat.Format = "dd/MM/yyyy";

        //        orderSheet.Cell(orderRow, 5).Value = order.PaymentMethod;
        //        orderSheet.Cell(orderRow, 6).Value = order.OrderStatus;

        //        orderSheet.Cell(orderRow, 7).Value = order.OrderTotal;
        //        orderSheet.Cell(orderRow, 7).Style.NumberFormat.Format = "#,##0.00";

        //        orderRow++;
        //    }

        //    orderSheet.Columns().AdjustToContents();

        //    // Sheet per Order
        //    foreach (var order in orders)
        //    {
        //        var detailSheet = workbook.Worksheets.Add($"Order_{order.OrderId}");

        //        detailSheet.Cell(1, 1).Value = "Product Name";
        //        detailSheet.Cell(1, 2).Value = "Quantity";
        //        detailSheet.Cell(1, 3).Value = "Price";
        //        detailSheet.Cell(1, 4).Value = "Size";
        //        detailSheet.Cell(1, 5).Value = "Color";
        //        detailSheet.Cell(1, 6).Value = "Subtotal";

        //        var detailHeader = detailSheet.Range("A1:F1");
        //        detailHeader.Style.Font.Bold = true;
        //        detailHeader.Style.Fill.BackgroundColor = XLColor.LightGray;
        //        detailHeader.Style.Alignment.Horizontal = XLAlignmentHorizontalValues.Center;

        //        int detailRow = 2;
        //        foreach (var detail in order.OrderProductLists)
        //        {
        //            detailSheet.Cell(detailRow, 1).Value = detail.Product?.ProductName ?? "N/A";
        //            detailSheet.Cell(detailRow, 2).Value = detail.Quanity;

        //            detailSheet.Cell(detailRow, 3).Value = detail.Price;
        //            detailSheet.Cell(detailRow, 3).Style.NumberFormat.Format = "#,##0.00";

        //            detailSheet.Cell(detailRow, 4).Value = detail.SizeName ?? "";
        //            detailSheet.Cell(detailRow, 5).Value = detail.ColorName ?? "";

        //            var subtotal = detail.Quanity * detail.Price;
        //            detailSheet.Cell(detailRow, 6).Value = subtotal;
        //            detailSheet.Cell(detailRow, 6).Style.NumberFormat.Format = "#,##0.00";

        //            detailRow++;
        //        }

        //        detailSheet.Columns().AdjustToContents();
        //    }

        //    using var stream = new MemoryStream();
        //    workbook.SaveAs(stream);
        //    stream.Position = 0;

        //    return File(stream.ToArray(),
        //        "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet",
        //        "Orders_With_Details.xlsx");
        //}

        [HttpGet]
        public IActionResult Details(int id)
        {
            var order = _context.Orders
                .Include(o => o.User)
                .Include(o => o.OrderProductLists)
                    .ThenInclude(op => op.Product)
                .FirstOrDefault(o => o.OrderId == id);

            if (order == null)
            {
                return NotFound();
            }

            return View(order);
        }


    }
}