using System;
using System.Collections.Generic;
using Microsoft.EntityFrameworkCore;

namespace _21BITV03_Nhom04_website_clothes.Data;

public partial class WebsiteClothesContext : DbContext
{
    public WebsiteClothesContext()
    {
    }

    public WebsiteClothesContext(DbContextOptions<WebsiteClothesContext> options)
        : base(options)
    {
    }

    public virtual DbSet<AspNetRole> AspNetRoles { get; set; }

    public virtual DbSet<AspNetRoleClaim> AspNetRoleClaims { get; set; }

    public virtual DbSet<AspNetUser> AspNetUsers { get; set; }

    public virtual DbSet<AspNetUserClaim> AspNetUserClaims { get; set; }

    public virtual DbSet<AspNetUserLogin> AspNetUserLogins { get; set; }

    public virtual DbSet<AspNetUserToken> AspNetUserTokens { get; set; }

    public virtual DbSet<Cart> Carts { get; set; }

    public virtual DbSet<CartProductList> CartProductLists { get; set; }

    public virtual DbSet<Discount> Discounts { get; set; }

    public virtual DbSet<DiscountedProductList> DiscountedProductLists { get; set; }

    public virtual DbSet<Order> Orders { get; set; }

    public virtual DbSet<OrderDiscountList> OrderDiscountLists { get; set; }

    public virtual DbSet<OrderProductList> OrderProductLists { get; set; }

    public virtual DbSet<Product> Products { get; set; }

    public virtual DbSet<ProductColor> ProductColors { get; set; }

    public virtual DbSet<ProductSize> ProductSizes { get; set; }

    public virtual DbSet<ProductType> ProductTypes { get; set; }

    public virtual DbSet<ProductTypeLink> ProductTypeLinks { get; set; }

    public virtual DbSet<SubProduct> SubProducts { get; set; }

    public virtual DbSet<UserInfo> UserInfos { get; set; }

    protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
#warning To protect potentially sensitive information in your connection string, you should move it out of source code. You can avoid scaffolding the connection string by using the Name= syntax to read it from configuration - see https://go.microsoft.com/fwlink/?linkid=2131148. For more guidance on storing connection strings, see https://go.microsoft.com/fwlink/?LinkId=723263.
        => optionsBuilder.UseSqlServer("Data Source=.;Initial Catalog=Website_clothes;Integrated Security=True;Trust Server Certificate=True;");

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<AspNetRole>(entity =>
        {
            entity.HasIndex(e => e.NormalizedName, "RoleNameIndex")
                .IsUnique()
                .HasFilter("([NormalizedName] IS NOT NULL)");

            entity.Property(e => e.Name).HasMaxLength(256);
            entity.Property(e => e.NormalizedName).HasMaxLength(256);
        });

        modelBuilder.Entity<AspNetRoleClaim>(entity =>
        {
            entity.HasIndex(e => e.RoleId, "IX_AspNetRoleClaims_RoleId");

            entity.HasOne(d => d.Role).WithMany(p => p.AspNetRoleClaims).HasForeignKey(d => d.RoleId);
        });

        modelBuilder.Entity<AspNetUser>(entity =>
        {
            entity.HasIndex(e => e.NormalizedEmail, "EmailIndex");

            entity.HasIndex(e => e.NormalizedUserName, "UserNameIndex")
                .IsUnique()
                .HasFilter("([NormalizedUserName] IS NOT NULL)");

            entity.Property(e => e.Email).HasMaxLength(256);
            entity.Property(e => e.NormalizedEmail).HasMaxLength(256);
            entity.Property(e => e.NormalizedUserName).HasMaxLength(256);
            entity.Property(e => e.UserName).HasMaxLength(256);

            entity.HasMany(d => d.Roles).WithMany(p => p.Users)
                .UsingEntity<Dictionary<string, object>>(
                    "AspNetUserRole",
                    r => r.HasOne<AspNetRole>().WithMany().HasForeignKey("RoleId"),
                    l => l.HasOne<AspNetUser>().WithMany().HasForeignKey("UserId"),
                    j =>
                    {
                        j.HasKey("UserId", "RoleId");
                        j.ToTable("AspNetUserRoles");
                        j.HasIndex(new[] { "RoleId" }, "IX_AspNetUserRoles_RoleId");
                    });
        });

        modelBuilder.Entity<AspNetUserClaim>(entity =>
        {
            entity.HasIndex(e => e.UserId, "IX_AspNetUserClaims_UserId");

            entity.HasOne(d => d.User).WithMany(p => p.AspNetUserClaims).HasForeignKey(d => d.UserId);
        });

        modelBuilder.Entity<AspNetUserLogin>(entity =>
        {
            entity.HasKey(e => new { e.LoginProvider, e.ProviderKey });

            entity.HasIndex(e => e.UserId, "IX_AspNetUserLogins_UserId");

            entity.HasOne(d => d.User).WithMany(p => p.AspNetUserLogins).HasForeignKey(d => d.UserId);
        });

        modelBuilder.Entity<AspNetUserToken>(entity =>
        {
            entity.HasKey(e => new { e.UserId, e.LoginProvider, e.Name });

            entity.HasOne(d => d.User).WithMany(p => p.AspNetUserTokens).HasForeignKey(d => d.UserId);
        });

        modelBuilder.Entity<Cart>(entity =>
        {
            entity.ToTable("Cart");

            entity.Property(e => e.CartId).HasColumnName("Cart_id");
            entity.Property(e => e.UserId).HasColumnName("User_id");

            entity.HasOne(d => d.User).WithMany(p => p.Carts)
                .HasForeignKey(d => d.UserId)
                .HasConstraintName("FK_Cart_UserInfo");
        });

        modelBuilder.Entity<CartProductList>(entity =>
        {
            entity.HasKey(e => e.CartProductList1);

            entity.ToTable("Cart_Product_List");

            entity.Property(e => e.CartProductList1).HasColumnName("Cart_product_list");
            entity.Property(e => e.CartId).HasColumnName("Cart_id");
            entity.Property(e => e.ProductId).HasColumnName("Product_id");
            entity.Property(e => e.SubProductId).HasColumnName("Sub_Product_ID");

            entity.HasOne(d => d.Cart).WithMany(p => p.CartProductLists)
                .HasForeignKey(d => d.CartId)
                .OnDelete(DeleteBehavior.Cascade)
                .HasConstraintName("FK_Cart_Product_List_Cart");

            entity.HasOne(d => d.Product).WithMany(p => p.CartProductLists)
                .HasForeignKey(d => d.ProductId)
                .OnDelete(DeleteBehavior.Cascade)
                .HasConstraintName("FK_Cart_Product_List_Products");

            entity.HasOne(d => d.SubProduct).WithMany(p => p.CartProductLists)
                .HasForeignKey(d => d.SubProductId)
                .OnDelete(DeleteBehavior.Cascade)
                .HasConstraintName("FK_Cart_Product_List_Sub_Product");
        });

        modelBuilder.Entity<Discount>(entity =>
        {
            entity.ToTable("Discount");

            entity.Property(e => e.DiscountId).HasColumnName("Discount_ID");
            entity.Property(e => e.DiscountAmount).HasColumnName("Discount_Amount");
            entity.Property(e => e.DiscountConditions)
                .HasMaxLength(50)
                .HasColumnName("Discount_Conditions");
            entity.Property(e => e.DiscountType)
                .HasMaxLength(50)
                .HasColumnName("Discount_Type");
            entity.Property(e => e.EndTime)
                .HasColumnType("datetime")
                .HasColumnName("End_Time");
            entity.Property(e => e.StartTime)
                .HasColumnType("datetime")
                .HasColumnName("Start_Time");
        });

        modelBuilder.Entity<DiscountedProductList>(entity =>
        {
            entity.ToTable("Discounted_Product__List");

            entity.Property(e => e.DiscountedProductListId).HasColumnName("Discounted_Product_List_ID");
            entity.Property(e => e.DiscountId).HasColumnName("Discount_ID");
            entity.Property(e => e.ProductId).HasColumnName("Product_ID");

            entity.HasOne(d => d.Discount).WithMany(p => p.DiscountedProductLists)
                .HasForeignKey(d => d.DiscountId)
                .OnDelete(DeleteBehavior.Cascade)
                .HasConstraintName("FK_Discounted_Product__List_Discount");

            entity.HasOne(d => d.Product).WithMany(p => p.DiscountedProductLists)
                .HasForeignKey(d => d.ProductId)
                .OnDelete(DeleteBehavior.Cascade)
                .HasConstraintName("FK_Discounted_Product__List_Products");
        });

        modelBuilder.Entity<Order>(entity =>
        {
            entity.ToTable("Order");

            entity.Property(e => e.OrderId).HasColumnName("Order_id");
            entity.Property(e => e.Message).HasMaxLength(50);
            entity.Property(e => e.OrderStatus)
                .HasMaxLength(50)
                .HasColumnName("Order_Status");
            entity.Property(e => e.PaymentMethod)
                .HasMaxLength(50)
                .HasColumnName("Payment_Method");
            entity.Property(e => e.UserId).HasColumnName("User_id");

            entity.HasOne(d => d.User).WithMany(p => p.Orders)
                .HasForeignKey(d => d.UserId)
                .HasConstraintName("FK_Order_UserInfo");
        });

        modelBuilder.Entity<OrderDiscountList>(entity =>
        {
            entity.ToTable("Order_Discount_List");

            entity.Property(e => e.OrderDiscountListId).HasColumnName("Order_Discount_List_ID");
            entity.Property(e => e.DiscountId).HasColumnName("Discount_id");
            entity.Property(e => e.DiscoutType)
                .HasMaxLength(50)
                .HasColumnName("Discout_type");
            entity.Property(e => e.OrderId).HasColumnName("Order_id");

            entity.HasOne(d => d.Discount).WithMany(p => p.OrderDiscountLists)
                .HasForeignKey(d => d.DiscountId)
                .OnDelete(DeleteBehavior.Cascade)
                .HasConstraintName("FK_Order_Discount_List_Discount");

            entity.HasOne(d => d.Order).WithMany(p => p.OrderDiscountLists)
                .HasForeignKey(d => d.OrderId)
                .OnDelete(DeleteBehavior.Cascade)
                .HasConstraintName("FK_Order_Discount_List_Order");
        });

        modelBuilder.Entity<OrderProductList>(entity =>
        {
            entity.ToTable("Order_Product_List");

            entity.Property(e => e.OrderProductListId).HasColumnName("Order_Product_List_ID");
            entity.Property(e => e.ColorName)
                .HasMaxLength(50)
                .HasColumnName("Color_name");
            entity.Property(e => e.OrderId).HasColumnName("Order_id");
            entity.Property(e => e.ProductId).HasColumnName("Product_ID");
            entity.Property(e => e.ProductName)
                .HasMaxLength(50)
                .HasColumnName("Product_Name");
            entity.Property(e => e.SizeName)
                .HasMaxLength(50)
                .HasColumnName("Size_name");

            entity.HasOne(d => d.Order).WithMany(p => p.OrderProductLists)
                .HasForeignKey(d => d.OrderId)
                .OnDelete(DeleteBehavior.Cascade)
                .HasConstraintName("FK_Order_Product_List_Order");

            entity.HasOne(d => d.Product).WithMany(p => p.OrderProductLists)
                .HasForeignKey(d => d.ProductId)
                .OnDelete(DeleteBehavior.Cascade)
                .HasConstraintName("FK_Order_Product_List_Products");
        });

        modelBuilder.Entity<Product>(entity =>
        {
            entity.Property(e => e.ProductId).HasColumnName("Product_id");
            entity.Property(e => e.DeleteStatus).HasColumnName("Delete_status");
            entity.Property(e => e.DeletionDate)
                .HasColumnType("datetime")
                .HasColumnName("Deletion_date");
            entity.Property(e => e.Description).HasMaxLength(50);
            entity.Property(e => e.ProductName)
                .HasMaxLength(50)
                .HasColumnName("Product_name");
        });

        modelBuilder.Entity<ProductColor>(entity =>
        {
            entity.HasKey(e => e.ColorId);

            entity.ToTable("Product_color");

            entity.Property(e => e.ColorId).HasColumnName("Color_id");
            entity.Property(e => e.ColorName)
                .HasMaxLength(50)
                .HasColumnName("Color_name");
            entity.Property(e => e.Description).HasMaxLength(255);
        });

        modelBuilder.Entity<ProductSize>(entity =>
        {
            entity.ToTable("Product_size");

            entity.Property(e => e.ProductSizeId).HasColumnName("Product__size_id");
            entity.Property(e => e.Description).HasMaxLength(50);
            entity.Property(e => e.SizeName)
                .HasMaxLength(50)
                .HasColumnName("Size_name");
        });

        modelBuilder.Entity<ProductType>(entity =>
        {
            entity.ToTable("Product_type");

            entity.Property(e => e.ProductTypeId).HasColumnName("Product_type_id");
            entity.Property(e => e.Description).HasMaxLength(50);
            entity.Property(e => e.ProductTypeName)
                .HasMaxLength(50)
                .HasColumnName("Product_type_name");
        });

        modelBuilder.Entity<ProductTypeLink>(entity =>
        {
            entity.ToTable("Product_type_link");

            entity.Property(e => e.ProductTypeLinkId).HasColumnName("Product_type_link_id");
            entity.Property(e => e.ProductId).HasColumnName("Product_id");
            entity.Property(e => e.ProductTypeId).HasColumnName("Product_type_id");

            entity.HasOne(d => d.Product).WithMany(p => p.ProductTypeLinks)
                .HasForeignKey(d => d.ProductId)
                .OnDelete(DeleteBehavior.Cascade)
                .HasConstraintName("FK_Product_type_link_Products");

            entity.HasOne(d => d.ProductType).WithMany(p => p.ProductTypeLinks)
                .HasForeignKey(d => d.ProductTypeId)
                .OnDelete(DeleteBehavior.Cascade)
                .HasConstraintName("FK_Product_type_link_Product_type");
        });

        modelBuilder.Entity<SubProduct>(entity =>
        {
            entity.ToTable("Sub_Product");

            entity.Property(e => e.SubProductId).HasColumnName("Sub_Product_ID");
            entity.Property(e => e.ColorId).HasColumnName("Color_ID");
            entity.Property(e => e.CreationDate)
                .HasColumnType("datetime")
                .HasColumnName("Creation_Date");
            entity.Property(e => e.DiscountedPrice).HasColumnName("Discounted_Price");
            entity.Property(e => e.Linkimage).HasMaxLength(50);
            entity.Property(e => e.MainProductId).HasColumnName("Main_product_id");
            entity.Property(e => e.OriginalPrice).HasColumnName("Original_Price");
            entity.Property(e => e.SizeId).HasColumnName("Size_ID");

            entity.HasOne(d => d.Color).WithMany(p => p.SubProducts)
                .HasForeignKey(d => d.ColorId)
                .OnDelete(DeleteBehavior.Cascade)
                .HasConstraintName("FK_Sub_Product_Product_color");

            entity.HasOne(d => d.MainProduct).WithMany(p => p.SubProducts)
                .HasForeignKey(d => d.MainProductId)
                .HasConstraintName("FK_Sub_Product_Products");

            entity.HasOne(d => d.Size).WithMany(p => p.SubProducts)
                .HasForeignKey(d => d.SizeId)
                .OnDelete(DeleteBehavior.Cascade)
                .HasConstraintName("FK_Sub_Product_Product_size");
        });

        modelBuilder.Entity<UserInfo>(entity =>
        {
            entity.HasKey(e => e.UserId);

            entity.ToTable("UserInfo");

            entity.Property(e => e.UserId)
                .ValueGeneratedNever()
                .HasColumnName("UserID");
            entity.Property(e => e.Address).HasMaxLength(50);
            entity.Property(e => e.Email).HasMaxLength(100);
            entity.Property(e => e.FullName).HasMaxLength(100);
            entity.Property(e => e.Gender).HasMaxLength(50);
            entity.Property(e => e.Password).HasMaxLength(100);
            entity.Property(e => e.UserName).HasMaxLength(50);

            entity.HasOne(d => d.User).WithOne(p => p.UserInfo)
                .HasForeignKey<UserInfo>(d => d.UserId)
                .HasConstraintName("FK_UserInfo_AspNetUsers");
        });

        OnModelCreatingPartial(modelBuilder);
    }

    partial void OnModelCreatingPartial(ModelBuilder modelBuilder);
}
