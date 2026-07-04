using AutoMapper;
using Microsoft.AspNetCore.Mvc;
using Talabat.Controllers;
using Talabat.Core.Entitys;
using Talabat.Core.Reposatory;
using Talabat.Core.Specification;
using Talabat.Dto;
using Talabat.Erorr;
using Talabat.Helper;

public class ProductController : BaseController
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly IMapper _mapper;

    public ProductController(IUnitOfWork unitOfWork, IMapper mapper)
    {
        _unitOfWork = unitOfWork;
        _mapper = mapper;
    }

    [HttpGet]
    public async Task<ActionResult<Pagination<ProductResultToDto>>> GetProductSpec([FromQuery] ProductSpecParams specParams)
    {
        // استخدم UnitOfWork بدل _productRepo
        var products = await _unitOfWork.Repository<Product>()
            .GetAllWithSpecAsync(new ProductWithTypesAndBrandsSpecification(specParams));

        var data = _mapper.Map<IReadOnlyList<ProductResultToDto>>(products);

        var count = await _unitOfWork.Repository<Product>()
            .GetCountAsync(new ProductWithFiltersForCountSpecification(specParams));

        return Ok(new Pagination<ProductResultToDto>(
            specParams.PageIndex.Value,
            specParams.PageSize.Value,
            count,
            data
        ));
    }

    [HttpGet("{id}")]
    public async Task<ActionResult<ProductResultToDto>> GetProductSpec(int id)
    {
        var product = await _unitOfWork.Repository<Product>()
            .GetByIdWithSpecAsync(new ProductWithTypesAndBrandsSpecification(id));

        if (product == null)
            return NotFound(new APIRespose(404));

        return Ok(_mapper.Map<ProductResultToDto>(product));
    }

    [HttpGet("brands")]
    public async Task<ActionResult<IEnumerable<ProductBrand>>> GetProductBrands()
    {
        var brands = await _unitOfWork.Repository<ProductBrand>().GetAllAsync();
        return Ok(brands);
    }

    [HttpGet("types")]
    public async Task<ActionResult<IEnumerable<ProductType>>> GetProductType()
    {
        var types = await _unitOfWork.Repository<ProductType>().GetAllAsync();
        return Ok(types);
    }
}
