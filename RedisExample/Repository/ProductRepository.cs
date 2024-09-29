using Microsoft.Extensions.Caching.Distributed;
using RedisExample.DBContext;
using RedisExample.DTO;
using RedisExample.Model;
using System.Text.Json;

namespace RedisExample.Repository
{
    public class ProductRepository : IProductRepository
    {
        private readonly IDistributedCache _cache;
        private readonly string _cacheKey = "Product26-9";
        private readonly TimeSpan _cacheDuration = TimeSpan.FromMinutes(10);

        private readonly ApplicationDbContext _dbContext;
        public ProductRepository(ApplicationDbContext dbContext, IDistributedCache cache)
        {
            _dbContext = dbContext;
            _cache = cache;
        }

        public async Task<IEnumerable<Product>> GetProductsAsync()
        {
            var cachedProducts = await _cache.GetStringAsync(_cacheKey);

            if (!string.IsNullOrEmpty(cachedProducts))
            {
                return JsonSerializer.Deserialize<IEnumerable<Product>>(cachedProducts);
            }

            // Simulate DB fetch
            var products = await FetchProductsFromDbAsync();

            var serializedProducts = JsonSerializer.Serialize(products);
            await _cache.SetStringAsync(_cacheKey, serializedProducts, new DistributedCacheEntryOptions
            {
                AbsoluteExpirationRelativeToNow = _cacheDuration
            });

            return products;
        }

        public async Task<Product> GetProductByIdAsync(int id)
        {
            // Cache key specific to a product
            var cacheKey = $"{_cacheKey}_{id}";
            var cachedProduct = await _cache.GetStringAsync(cacheKey);

            if (!string.IsNullOrEmpty(cachedProduct))
            {
                return JsonSerializer.Deserialize<Product>(cachedProduct);
            }

            // Simulate DB fetch
            var product = await FetchProductByIdFromDbAsync(id);

            if (product != null)
            {
                var serializedProduct = JsonSerializer.Serialize(product);
                await _cache.SetStringAsync(cacheKey, serializedProduct, new DistributedCacheEntryOptions
                {
                    AbsoluteExpirationRelativeToNow = _cacheDuration
                });
            }

            return product;
        }

        public async Task AddProductAsync(Product product)
        {
            // Simulate DB insert
            await InsertProductIntoDbAsync(product);

            // Invalidate cache
            await _cache.RemoveAsync(_cacheKey);
        }

        public async Task UpdateProductAsync(Product product)
        {
            // Simulate DB update
            await UpdateProductInDbAsync(product);

            // Invalidate cache
            await _cache.RemoveAsync(_cacheKey);
            await _cache.RemoveAsync($"{_cacheKey}_{product.Id}");
        }

        public async Task DeleteProductAsync(int id)
        {
            // Simulate DB delete
            await DeleteProductFromDbAsync(id);

            // Invalidate cache
            await _cache.RemoveAsync(_cacheKey);
            await _cache.RemoveAsync($"{_cacheKey}_{id}");
        }

        // Simulate database operations
        private Task<IEnumerable<Product>> FetchProductsFromDbAsync()
        {
            // Dummy data
            var products = _dbContext.Products;
            return Task.FromResult(products.AsEnumerable());
        }

        private async Task<Product> FetchProductByIdFromDbAsync(int id)
        {
            var product = await _dbContext.Products.FindAsync(id); //new Product { Id = id, Name = $"Product {id}", CategoryId = id };
            return product;
        }

     

       
        public async Task InsertProductIntoDbAsync(Product product)
        {
            _dbContext.Products.Add(product);
            await _dbContext.SaveChangesAsync();
        }

        public async Task UpdateProductInDbAsync(Product product)
        {
            var existingProduct = await _dbContext.Products.FindAsync(product.Id);
            if (existingProduct != null)
            {
                existingProduct.Name = product.Name;
                existingProduct.CategoryId = product.CategoryId;
                _dbContext.Products.Update(existingProduct);
                await _dbContext.SaveChangesAsync();
            }
        }

        public async Task DeleteProductFromDbAsync(int id)
        {
            var product = await _dbContext.Products.FindAsync(id);
            if (product != null)
            {
                _dbContext.Products.Remove(product);
                await _dbContext.SaveChangesAsync();
            }
        }

    }

}
