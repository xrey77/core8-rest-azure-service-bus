// Services/ProductService.cs
using Dapper;
using System.Data;
using core8_rest_azure_service_bus.Models;
using core8_rest_azure_service_bus.Helpers;
using core8_rest_azure_service_bus.dto;
using Microsoft.Extensions.Logging;

namespace core8_rest_azure_service_bus.Services
{
    public class PagedResult<T>
    {
        public List<T> Data { get; set; } = new();
        public int Page { get; set; }
        public int TotalPages { get; set; }
        public int TotalRecords { get; set; }
    }

    public interface IProductService
    {
        Task<PagedResult<Product>> GetProductListAsync(int pageNumber);
        Task<PagedResult<Product>> GetProductSearchAsync(int pageNumber, string keyword);      
        Task<IEnumerable<CategoryDto>> GetProductsByCategoryAsync();
    }
        
    public class ProductService : IProductService 
    {
        private readonly IDbConnection _dbConnection;
        private readonly ILogger<ProductService> _logger;

        public ProductService(IDbConnection dbConnection, ILogger<ProductService> logger) 
        {
            _dbConnection = dbConnection;
            _logger = logger;
        }

        public async Task<PagedResult<Product>> GetProductListAsync(int pageNumber) 
        {
            var page = pageNumber <= 0 ? 1 : pageNumber;
            var perPage = 5;
            var offset = (page - 1) * perPage;

            const string countSql = "SELECT COUNT(*) FROM products";
            const string selectSql = @"
                SELECT Id, Descriptions, Qty, Unit, Costprice, Sellprice 
                FROM products 
                ORDER BY Id 
                OFFSET @Offset ROWS FETCH NEXT @Limit ROWS ONLY";

            // 3. Execute queries
            var totalRecords = await _dbConnection.ExecuteScalarAsync<int>(countSql);
            
            // Map directly to Product instead of dynamic
            var dbProducts = await _dbConnection.QueryAsync<Product>(selectSql, new 
            { 
                Limit = perPage, 
                Offset = offset 
            });

            var productList = dbProducts.ToList();

            if (!productList.Any())
            {
                _logger.LogWarning("Products not found for page {Page}.", page);
                throw new AppException("Products not found.");
            }

            int totalPages = (int)Math.Ceiling((double)totalRecords / perPage);

            return new PagedResult<Product>
            {
                Data = productList,
                Page = page,
                TotalPages = totalPages,
                TotalRecords = totalRecords
            };
        }


        public async Task<PagedResult<Product>> GetProductSearchAsync(int pageNumber, string keyword) 
        {
            var key = "%" + (keyword ?? string.Empty) + "%";            
            var page = pageNumber <= 0 ? 1 : pageNumber;
            var perPage = 5;
            var offset = (page - 1) * perPage;

            const string countSql = "SELECT COUNT(*) FROM products WHERE descriptions LIKE @Descriptions";

            const string selectSql = @"
                SELECT Id, Descriptions, Qty, Unit, Costprice, Sellprice 
                FROM products 
                WHERE descriptions LIKE @Descriptions
                ORDER BY Id 
                OFFSET @Offset ROWS FETCH NEXT @Limit ROWS ONLY";


            var totalRecords = await _dbConnection.ExecuteScalarAsync<int>(countSql, new { Descriptions = key });            

            var dbProducts = await _dbConnection.QueryAsync<Product>(selectSql, new 
            { 
                Descriptions = key,
                Limit = perPage, 
                Offset = offset 
            });

            var productList = dbProducts.ToList();

            if (!productList.Any())
            {
                _logger.LogWarning("Products not found for page {Page}.", page);
                throw new AppException("Products not found.");
            }

            int totalPages = (int)Math.Ceiling((double)totalRecords / perPage);

            return new PagedResult<Product>
            {
                Data = productList,
                Page = page,
                TotalPages = totalPages,
                TotalRecords = totalRecords
            };
        }


        public async Task<IEnumerable<CategoryDto>> GetProductsByCategoryAsync() 
        {
            const string sql = @"
                SELECT 
                    c.Id, 
                    c.Name,
                    p.id, 
                    p.descriptions, 
                    p.qty, 
                    p.unit, 
                    p.costprice, 
                    p.sellprice
                FROM Categories c
                INNER JOIN products p ON c.Id = p.category_id
                WHERE p.deleted_at IS NULL";

            var categoryDictionary = new Dictionary<int, CategoryDto>();

            // splitOn defaults to "Id", which matches the start of the product columns
            await _dbConnection.QueryAsync<CategoryDto, ProductDto, CategoryDto>(
                sql,
                (category, product) =>
                {
                    // Check if parent category is already tracked
                    if (!categoryDictionary.TryGetValue(category.Id, out var currentCategory))
                    {
                        currentCategory = category;
                        currentCategory.Products = new List<ProductDto>(); // Ensure list is initialized
                        categoryDictionary.Add(currentCategory.Id, currentCategory);
                    }

                    // Append the child product to the parent category
                    if (product != null)
                    {
                        currentCategory.Products.Add(product);
                    }

                    return currentCategory;
                },
                splitOn: "id" 
            );

            // Return the distinct master-detail tree structures
            return categoryDictionary.Values;
        }


    }
}
