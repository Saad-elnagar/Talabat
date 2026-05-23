using AutoMapper;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Talabat.Core.Entitys;
using Talabat.Core.Reposatory;
using Talabat.Core.Specification;
using Talabat.Dto;
using Talabat.Reposatory.Repository;

namespace Talabat.Controllers
{

    public class ProductController : BaseController
    {
        private readonly IGenericRepository<Product> _productRepo;
        private readonly IMapper _mapper;

        public ProductController(IGenericRepository<Product> productRepo , IMapper mapper)
        {
            _productRepo = productRepo;
            _mapper = mapper;
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

        public async Task<ActionResult<IEnumerable<ProductResultToDto>>> GetProductSpec()
        {
            var products = await _productRepo.GetAllWithSpecAsync(new ProductWithTypesAndBrandsSpecification());
            return Ok(_mapper.Map<IEnumerable<ProductResultToDto>>(products));
        }
        [HttpGet("{id}")]
        public async Task<ActionResult<ProductResultToDto>> GetProductSpec(int id)
        {
            var product = await _productRepo.GetByIdWithSpecAsync(new ProductWithTypesAndBrandsSpecification(id));
            if (product == null) return NotFound();
            return Ok(_mapper.Map<ProductResultToDto>(product));

        }
    }
}
