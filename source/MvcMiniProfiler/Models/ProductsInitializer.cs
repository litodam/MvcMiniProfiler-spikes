namespace MvcMiniProfilerSample.Models
{
    using System.Data.Entity;

    public class ProductsInitializer : DropCreateDatabaseIfModelChanges<ProductsContext>
    {
        private int nextId = 1;

        protected override void Seed(ProductsContext context)
        {
            this.AddCategory(context, "Beverages");
            this.AddCategory(context, "Condiments");
            this.AddCategory(context, "Confections");
            this.AddCategory(context, "Dairy Products");
            this.AddCategory(context, "Grains/Cereals");
            this.AddCategory(context, "Meat/Poultry");
            this.AddCategory(context, "Produce");
            this.AddCategory(context, "Seafood");

            MiniProfilerInitialization.InitializeMiniProfilerSqlStorage();
        }        

        private void AddCategory(ProductsContext context, string categoryName)
        {
            context.Categories.Add(new Category()
            {
                Id = this.nextId,
                CategoryName = categoryName
            });

            this.nextId++;
        }
    }
}