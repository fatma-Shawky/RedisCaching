using AutoMapper;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using RedisExample.DTO;
using RedisExample.Model;
using RedisExample.Repository;

namespace RedisExample.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class ProductsController : ControllerBase
    {
        private readonly IProductRepository _productRepository;
        private readonly IMapper _mapper;

        public ProductsController(IProductRepository productRepository, IMapper mapper)
        {
            _productRepository = productRepository;
            _mapper = mapper;
        }

        [HttpPost]
        public async Task<IActionResult> AddProduct(ProductDto productDto)
        {
            // Map ProductDto to Product entity
            var product = _mapper.Map<Product>(productDto);

            await _productRepository.AddProductAsync(product);

            return CreatedAtAction(nameof(GetProduct), new { id = product.Id }, product);
        }
        [HttpGet]
        public async Task<IActionResult> GetProducts()
        {
            var products = await _productRepository.GetProductsAsync();
            return Ok(products);
        }

        [HttpGet("{id}")]
        public async Task<IActionResult> GetProduct(int id)
        {
            var product = await _productRepository.GetProductByIdAsync(id);
            if (product == null)
            {
                return NotFound();
            }

            // Map Product entity to ProductDto
            var productDto = _mapper.Map<ProductDto>(product);
            return Ok(productDto);
        }

        [HttpPut("{id}")]
        public async Task<IActionResult> UpdateProduct(int id, ProductDto productDto)
        {
            if (id <= 0)
            {
                return BadRequest();
            }

            var existingProduct = await _productRepository.GetProductByIdAsync(id);
            if (existingProduct == null)
            {
                return NotFound();
            }

            // Update existing product with new data from ProductDto
            _mapper.Map(productDto, existingProduct);
            await _productRepository.UpdateProductAsync(existingProduct);

            return NoContent();
        }

        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteProduct(int id)
        {
            await _productRepository.DeleteProductAsync(id);
            return NoContent();
        }
    }

    //[Route("api/[controller]")]
    //[ApiController]
    //public class ProductsController : ControllerBase
    //{
    //    private readonly IProductRepository _productRepository;

    //    public ProductsController(IProductRepository productRepository)
    //    {
    //        _productRepository = productRepository;
    //    }

    //    [HttpGet]
    //    public async Task<IActionResult> GetProducts()
    //    {
    //        var products = await _productRepository.GetProductsAsync();
    //        return Ok(products);
    //    }

    //    [HttpGet("{id}")]
    //    public async Task<IActionResult> GetProduct(int id)
    //    {
    //        var product = await _productRepository.GetProductByIdAsync(id);
    //        if (product == null)
    //        {
    //            return NotFound();
    //        }
    //        return Ok(product);
    //    }

    //    [HttpPost]
    //    public async Task<IActionResult> AddProduct(Product product)
    //    {
    //        await _productRepository.AddProductAsync(product);
    //        return CreatedAtAction(nameof(GetProduct), new { id = product.Id }, product);
    //    }

    //    [HttpPut("{id}")]
    //    public async Task<IActionResult> UpdateProduct(int id, Product product)
    //    {
    //        if (id != product.Id)
    //        {
    //            return BadRequest();
    //        }
    //        await _productRepository.UpdateProductAsync(product);
    //        return NoContent();
    //    }

    //    [HttpDelete("{id}")]
    //    public async Task<IActionResult> DeleteProduct(int id)
    //    {
    //        await _productRepository.DeleteProductAsync(id);
    //        return NoContent();
    //    }
    //}

}
