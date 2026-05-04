using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.EntityFrameworkCore;
using Moq;
using PCShop_Backend.Data;
using PCShop_Backend.Dtos;
using PCShop_Backend.Dtos.ProductDtos.CreateDto;
using PCShop_Backend.Exceptions;
using PCShop_Backend.Models;
using PCShop_Backend.Service;
using Xunit;
using FluentAssertions;
using System.ComponentModel.DataAnnotations;

namespace PCShop_Backend.Tests
{
    public class ProductServiceTests
    {
        private readonly ApplicationDbContext _context;
        private readonly Mock<IHttpContextAccessor> _httpContextAccessorMock;
        private readonly Mock<ICacheService> _cacheServiceMock;
        private readonly ProductService _productService;

        public ProductServiceTests()
        {
            var options = new DbContextOptionsBuilder<ApplicationDbContext>()
                .UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString())
                .Options;

            _context = new ApplicationDbContext(options);
            _httpContextAccessorMock = new Mock<IHttpContextAccessor>();
            _cacheServiceMock = new Mock<ICacheService>();

            _productService = new ProductService(_context, _httpContextAccessorMock.Object, _cacheServiceMock.Object);
        }

        [Fact]
        public async Task getComponentById_ShouldReturnComponent_WhenComponentExists()
        {
            // Arrange
            var category = new ComponentCategory { CategoryId = 1, CategoryName = "Test Category" };
            var component = new Models.Component
            {
                ComponentId = 1,
                Name = "Test Component",
                CategoryId = 1,
                Category = category,
                Price = 100,
                StockQuantity = 10,
                Brand = "Test Brand",
                Description = "Test Description",
                ImageUrl = "http://test.com/image.jpg",
                RowVersion = new byte[] { 1, 2, 3, 4 }
            };
            await _context.ComponentCategories.AddAsync(category);
            await _context.Components.AddAsync(component);
            await _context.SaveChangesAsync();

            _cacheServiceMock.Setup(x => x.GetAsync<ComponentDto>(It.IsAny<string>()))
                .ReturnsAsync((ComponentDto)null);

            // Act
            var result = await _productService.getComponentById(1);

            // Assert
            result.Should().NotBeNull();
            result.ComponentId.Should().Be(1);
            result.Name.Should().Be("Test Component");
            result.CategoryName.Should().Be("Test Category");
        }

        [Fact]
        public async Task getComponentById_ShouldThrowNotFoundException_WhenComponentDoesNotExist()
        {
            // Arrange
            _cacheServiceMock.Setup(x => x.GetAsync<ComponentDto>(It.IsAny<string>()))
                .ReturnsAsync((ComponentDto)null);

            // Act
            Func<Task> act = async () => await _productService.getComponentById(99);

            // Assert
            await act.Should().ThrowAsync<NotFoundException>();
        }

        [Fact]
        public async Task createComponent_ShouldThrowValidationException_WhenCategoryDoesNotExist()
        {
            // Arrange
            var dto = new createComponentDto
            {
                Name = "Test Component",
                CategoryId = 99, // Non-existent category
                Price = 100,
                StockQuantity = 10
            };

            // Act
            Func<Task> act = async () => await _productService.createComponent(dto);

            // Assert
            await act.Should().ThrowAsync<ValidationException>()
                .WithMessage("*Category with ID 99 not found*");
        }
    }
}
