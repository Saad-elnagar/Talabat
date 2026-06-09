using AutoMapper;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Talabat.Core.Entitys;
using Talabat.Core.Reposatory;
using Talabat.Core.Specification;
using Talabat.Dto;
using Talabat.Erorr;
using Talabat.Helper;
using Talabat.Reposatory.Repository;

namespace Talabat.Controllers
{

    public class ProductController : BaseController
    {
        private readonly IGenericRepository<Product> _productRepo;
        private readonly IGenericRepository<ProductBrand> _productBrandRepo;
        private readonly IGenericRepository<ProductType> _productTypeRepo;
        private readonly IMapper _mapper;

        public ProductController(IGenericRepository<Product> productRepo, IGenericRepository<ProductBrand> productBrandRepo, IGenericRepository<ProductType> ProductTypeRepo, IMapper mapper)
        {
            _productRepo = productRepo;
            _productBrandRepo = productBrandRepo;
            _productTypeRepo = ProductTypeRepo;
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

        public async Task<ActionResult<Pagination<ProductResultToDto>>> GetProductSpec([FromQuery] ProductSpecParams specParams)
        {
            var products = await _productRepo.GetAllWithSpecAsync(new ProductWithTypesAndBrandsSpecification(specParams));

            var data = _mapper.Map<IReadOnlyList<ProductResultToDto>>(products);
            var count = await _productRepo.GetCountAsync(new ProductWithFiltersForCountSpecification(specParams));

            return Ok(new Pagination<ProductResultToDto>(specParams.PageIndex.Value, specParams.PageSize.Value,count ,data));

        }



        [HttpGet("{id}")]
        public async Task<ActionResult<ProductResultToDto>> GetProductSpec(int id)
        {
            var product = await _productRepo.GetByIdWithSpecAsync(new ProductWithTypesAndBrandsSpecification(id));
            if (product == null) return NotFound(new APIRespose(404));
            return Ok(_mapper.Map<ProductResultToDto>(product));

        }



        [HttpGet("brands")]
        public async Task<ActionResult<IEnumerable<ProductBrand>>> GetProductBrands()
        {
            var brands = await _productBrandRepo.GetAllAsync();
            return Ok(brands);
        }

        [HttpGet("types")]
        public async Task<ActionResult<IEnumerable<ProductType>>> GetProductType()
        {
            var type = await _productTypeRepo.GetAllAsync();
            return Ok(type);
        }
    }
}
