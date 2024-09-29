using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Caching.Distributed;
using RedisExample.DBContext;
using RedisExample.Model;
using System.Text.Json;

namespace RedisExample.Repository
{
    public class CategoryRepository : ICategoryRepository
    {
        private readonly ApplicationDbContext _dbContext;
        private readonly IDistributedCache _cache;
        private readonly string _cacheKey = "CategoryList";
        private readonly TimeSpan _cacheDuration = TimeSpan.FromMinutes(10);

        public CategoryRepository(ApplicationDbContext dbContext, IDistributedCache cache)
        {
            _dbContext = dbContext;
            _cache = cache;
        }

        public async Task<IEnumerable<Category>> GetCategoriesAsync()
        {
            var cachedCategories = await _cache.GetStringAsync(_cacheKey);

            if (!string.IsNullOrEmpty(cachedCategories))
            {
                return JsonSerializer.Deserialize<IEnumerable<Category>>(cachedCategories);
            }

            var categories = await _dbContext.Categories.ToListAsync();
            var serializedCategories = JsonSerializer.Serialize(categories);

            await _cache.SetStringAsync(_cacheKey, serializedCategories, new DistributedCacheEntryOptions
            {
                AbsoluteExpirationRelativeToNow = _cacheDuration
            });

            return categories;
        }

        public async Task<Category> GetCategoryByIdAsync(int id)
        {
            var cacheKey = $"{_cacheKey}_{id}";
            var cachedCategory = await _cache.GetStringAsync(cacheKey);

            if (!string.IsNullOrEmpty(cachedCategory))
            {
                return JsonSerializer.Deserialize<Category>(cachedCategory);
            }

            var category = await _dbContext.Categories.FindAsync(id);

            if (category != null)
            {
                var serializedCategory = JsonSerializer.Serialize(category);
                await _cache.SetStringAsync(cacheKey, serializedCategory, new DistributedCacheEntryOptions
                {
                    AbsoluteExpirationRelativeToNow = _cacheDuration
                });
            }

            return category;
        }

        public async Task AddCategoryAsync(Category category)
        {
            _dbContext.Categories.Add(category);
            await _dbContext.SaveChangesAsync();

            // Invalidate cache
            await _cache.RemoveAsync(_cacheKey);
        }

        public async Task UpdateCategoryAsync(Category category)
        {
            var existingCategory = await _dbContext.Categories.FindAsync(category.Id);
            if (existingCategory != null)
            {
                existingCategory.Name = category.Name;
                _dbContext.Categories.Update(existingCategory);
                await _dbContext.SaveChangesAsync();

                // Invalidate cache
                await _cache.RemoveAsync(_cacheKey);
                await _cache.RemoveAsync($"{_cacheKey}_{category.Id}");
            }
        }

        public async Task DeleteCategoryAsync(int id)
        {
            var category = await _dbContext.Categories.FindAsync(id);
            if (category != null)
            {
                _dbContext.Categories.Remove(category);
                await _dbContext.SaveChangesAsync();

                // Invalidate cache
                await _cache.RemoveAsync(_cacheKey);
                await _cache.RemoveAsync($"{_cacheKey}_{id}");
            }
        }
    }
    
}
