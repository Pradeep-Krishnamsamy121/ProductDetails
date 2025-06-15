namespace ProductDetails.Models
{
    public class ProductModel
    {
        public class ProductTypes
        {
            public int Id { get; set; }
            public string Type { get; set; }

        }

        public class ProductColors
        {
            public int Id { get; set; }
            public string colorName { get; set; }
        }

        public class ProductDetails
        {
            public string ProductName { get; set; }
            public string TypeId { get; set; }

            public List<int> ColorIds { get; set; }

        }
    }
}
