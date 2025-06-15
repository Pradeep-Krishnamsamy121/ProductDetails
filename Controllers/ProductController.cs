using Microsoft.AspNetCore.Mvc;
using Oracle.ManagedDataAccess.Client;
using ProductDetails.Models;

namespace ProductDetails.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class ProductController : Controller
    {
        private readonly IConfiguration _config;

        public ProductController(IConfiguration config)
        {
            _config = config;
        }

        [HttpGet("types")]
        public IActionResult GetProductTypes()
        {
            var types = new List<ProductModel.ProductTypes>();
            using var conn = new OracleConnection(_config.GetConnectionString("OracleDb"));
            conn.Open();
            var cmd = new OracleCommand("SELECT ID,Type from ProductType", conn);
            using var reader = cmd.ExecuteReader();
            while (reader.Read())
            {
                types.Add(new ProductModel.ProductTypes { Id = reader.GetInt32(0), Type = reader.GetString(1) });
            }
            return Ok(types);
            
        }

     
        [HttpGet("colors")]
        public IActionResult GetProductColors()
        {
            var colors = new List<ProductModel.ProductColors>();
            using var conn = new OracleConnection(_config.GetConnectionString("OracleDb"));
            conn.Open();
            var cmd = new OracleCommand("SELECT ID,ColorName from Colors", conn);
            using var reader = cmd.ExecuteReader();
            while (reader.Read())
            {
                colors.Add(new ProductModel.ProductColors { Id = reader.GetInt32(0), colorName = reader.GetString(1) });
            }
            return Ok(colors);
        }

        [HttpPost]
        public IActionResult AddProduct(ProductModel.ProductDetails product)
        {
            try
            {
                using (var conn = new OracleConnection(_config.GetConnectionString("OracleDb")))
                {
                    conn.Open();

                    // Convert List of color IDs to comma-separated string
                    string colorIds = string.Join(",", product.ColorIds);

                    using (var cmd = new OracleCommand("INSERT INTO Product (ID,Name, ProductTypeId, ColorId,CREATEDATE) VALUES (PRODUCT_SEQ.Nextval,:name, :typeId, :colorIds, SYSDATE)", conn))
                    {
                        cmd.Parameters.Add("name", product.ProductName);
                        cmd.Parameters.Add("typeId", product.TypeId);
                        cmd.Parameters.Add("colorIds", colorIds); // Store as comma string

                        cmd.ExecuteNonQuery();
                    }
                }

                return Ok("Product added successfully");
            }
            catch (Exception ex)
            {
                return StatusCode(500, "Error: " + ex.Message);
            }
        }

        [HttpGet]
        public IActionResult GetProductsDetails()
        {
            var products = new List<object>();
            using var conn = new OracleConnection(_config.GetConnectionString("OracleDb"));
            conn.Open();

            var cmd = new OracleCommand(@"select P.Id, P.Name, pt.type, p.colorid from product P join ProductType PT on p.producttypeid=pt.id order by createdate desc", conn);

            using var reader = cmd.ExecuteReader();
            while (reader.Read())
            {
                var colorIdString = reader.IsDBNull(3) ? "" : reader.GetString(3);
                var colorIds = colorIdString.Split(',', StringSplitOptions.RemoveEmptyEntries).Select(int.Parse).ToList();

                var colorNames = new List<string>();
                foreach (var colorId in colorIds)
                {
                    using var colorCmd = new OracleCommand("SELECT COLORNAME FROM Colors WHERE id=:id", conn);
                    colorCmd.Parameters.Add(new OracleParameter("id", colorId));
                    var name = colorCmd.ExecuteScalar()?.ToString();
                    if (name != null)
                    {
                        colorNames.Add(name);
                    }

                }
                products.Add(new
                {
                    Id=reader.GetInt32(0),
                    ProductName = reader.GetString(1),
                    TypeName = reader.GetString(2),
                    colorNames = colorNames
                });

            }
            return Ok(products);
        }

     
    }
}
