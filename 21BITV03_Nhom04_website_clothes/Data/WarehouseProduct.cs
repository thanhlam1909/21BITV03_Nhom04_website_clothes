using System;
using System.Collections.Generic;

namespace _21BITV03_Nhom04_website_clothes.Data;

public partial class WarehouseProduct
{
    public int WarehouseProductId { get; set; }

    public int WarehouseId { get; set; }

    public int Quantity { get; set; }

    public DateTime? LastUpdated { get; set; }

    public int? SkuId { get; set; }

    public virtual Sku? Sku { get; set; }

    public virtual Warehouse Warehouse { get; set; } = null!;
}
