using System;
using System.Collections.Generic;

namespace _21BITV03_Nhom04_website_clothes.Data;

public partial class Sku
{
    public int SkuId { get; set; }

    public string SkuCode { get; set; } = null!;

    public virtual ICollection<SubProduct> SubProducts { get; set; } = new List<SubProduct>();

    public virtual ICollection<WarehouseBatch> WarehouseBatches { get; set; } = new List<WarehouseBatch>();

    public virtual ICollection<WarehouseProduct> WarehouseProducts { get; set; } = new List<WarehouseProduct>();
}
