//// Talabat.Core/Specification/ProductSpecificationHelper.cs
//using Microsoft.EntityFrameworkCore;
//using System;
//using System.Linq.Expressions;
//using Talabat.Core.Entitys;

//namespace Talabat.Core.Specification
//{
//    public static class ProductSpecificationHelper
//    {
//        public static Expression<Func<Product, bool>> BuildCriteria(ProductSpecParams specParams)
//        {
//            return p =>
//                (string.IsNullOrEmpty(specParams.Search) ||
//                 EF.Functions.Like(p.Name, $"%{specParams.Search}%")) &&
//                (!specParams.BrandId.HasValue || p.ProductBrandId == specParams.BrandId.Value) &&
//                (!specParams.TypeId.HasValue || p.ProductTypeId == specParams.TypeId.Value);
//        }
//    }
//}