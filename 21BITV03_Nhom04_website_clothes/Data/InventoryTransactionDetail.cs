using System;
using System.Collections.Generic;

namespace _21BITV03_Nhom04_website_clothes.Data;

public partial class InventoryTransactionDetail
{
    public int TransactionDetailId { get; set; }

    public int TransactionId { get; set; }

    public int SubProductId { get; set; }

    public int Quantity { get; set; }

    public decimal? UnitPrice { get; set; }

    public virtual SubProduct SubProduct { get; set; } = null!;

    public virtual InventoryTransaction Transaction { get; set; } = null!;
}
