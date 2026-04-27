using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Talabat.Core.Entitys;
using Talabat.Core.Reposatory;
using Talabat.Core.Specification;
using Talabat.Reposatory.Repository;

namespace Talabat.Controllers
{

    public class ProductController : BaseController
    {
        private readonly IGenericRepository<Product> _productRepo;

        public ProductController(IGenericRepository<Product> productRepo)
        {
            _productRepo = productRepo;
        }


        //[HttpGet]
        //public async Task<ActionResult<IEnumerable<Product>>> GetProducts()
        //{
        //    var products = await _productRepo.GetAllAsync();
        //    return Ok(products);
        //}


        //[HttpGet("{id}")]
        //public async Task<ActionResult<Product>> GetProduct(int id)
        //{
        //    var product = await _productRepo.GetByIdAsync(id);
        //    if (product == null) return NotFound();
        //    return Ok(prod
        //    uct);
        //}

        [HttpGet]

        public async Task<ActionResult<IEnumerable<Product>>> GetProductSpec()
        {
            var products = await _productRepo.GetAllWithSpecAsync(new ProductWithTypesAndBrandsSpecification());
            return Ok(products);
        }
        [HttpGet("{id}")]
        public async Task<ActionResult<Product>> GetProductSpec(int id)
        {
            var product = await _productRepo.GetByIdWithSpecAsync(new ProductWithTypesAndBrandsSpecification(id));
            if (product == null) return NotFound();
            return Ok(product);

        }
    }
}
