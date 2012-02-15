namespace MvcMiniProfilerSample.Models
{
    using System.Data.Entity;

    public class ProductsContext : DbContext
    {
        public DbSet<Product> Products { get; set; }

        public DbSet<Category> Categories { get; set; }
    }
}