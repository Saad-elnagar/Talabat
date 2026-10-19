using AutoMapper;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.RateLimiting;
using Talabat.Domain.Entities;
using Talabat.Domain.Repositories;
using Talabat.Domain.Specifications;
using Talabat.Application.DTOs;
using Talabat.Api.Errors;
using Talabat.Api.Filters;
using Talabat.Application.Mapping;

namespace Talabat.Api.Controllers;

public class ProductController : BaseController
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly IMapper _mapper;

    public ProductController(IUnitOfWork unitOfWork, IMapper mapper)
    {
        _unitOfWork = unitOfWork;
        _mapper = mapper;
    }


    [Cache(600)]
    [EnableRateLimiting("Search")]
    [HttpGet]
    public async Task<ActionResult<Pagination<ProductResultToDto>>> GetProductSpec([FromQuery] ProductSpecParams specParams)
    {
       
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
   [Cache(300)]
    [HttpGet("{id}")]
    public async Task<ActionResult<ProductResultToDto>> GetProductSpec(int id)
    {
        var product = await _unitOfWork.Repository<Product>()
            .GetByIdWithSpecAsync(new ProductWithTypesAndBrandsSpecification(id));

        if (product == null)
            return NotFound(new APIRespose(404));

        return Ok(_mapper.Map<ProductResultToDto>(product));
    }
    [Cache(600)]
    [HttpGet("brands")]
    public async Task<ActionResult<IEnumerable<ProductBrand>>> GetProductBrands()
    {
        var brands = await _unitOfWork.Repository<ProductBrand>().GetAllAsync();
        return Ok(brands);
    }
    [Cache(600)]

    [HttpGet("types")]
    public async Task<ActionResult<IEnumerable<ProductType>>> GetProductType()
    {
        var types = await _unitOfWork.Repository<ProductType>().GetAllAsync();
        return Ok(types);
    }

}
