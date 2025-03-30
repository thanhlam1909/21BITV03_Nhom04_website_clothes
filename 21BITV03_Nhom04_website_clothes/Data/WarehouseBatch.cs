using System;
using System.Collections.Generic;

namespace _21BITV03_Nhom04_website_clothes.Data;

public partial class WarehouseBatch
{
    public int BatchId { get; set; }

    public int WarehouseId { get; set; }

    public int SkuId { get; set; }

    public int SupplierId { get; set; }

    public int QuantityImported { get; set; }

    public int QuantityAvailable { get; set; }

    public DateTime ImportDate { get; set; }

    public decimal UnitCost { get; set; }

    public string? Note { get; set; }

    public virtual Sku Sku { get; set; } = null!;

    public virtual Supplier Supplier { get; set; } = null!;

    public virtual Warehouse Warehouse { get; set; } = null!;
}
