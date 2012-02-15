namespace MvcMiniProfilerSample.Models
{
    using System;

    public class Product
    {
        public int Id { get; set; }

        public string Name { get; set; }

        public int CategoryId { get; set; }

        public decimal? Price { get; set; }

        public Category Category { get; set; }
    }
}