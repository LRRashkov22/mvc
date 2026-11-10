
using FluentAssertions;
using Moq;
using WebApplication1.Models;
using WebApplication1.Repositories.Contracts;
using WebApplication1.Services.Implementations;
using Xunit;

namespace WebApplication1.Tests;

public class CategoriesServiceTests
{
    private readonly Mock<ICategoryRepository> _repository = new();
    private readonly CategoriesService _service;

    public CategoriesServiceTests()
    {
        _service = new CategoriesService(_repository.Object);
    }

    [Fact]
    public async Task GetAllAsync_ShouldReturnCategories()
    {
        // Arrange
        IReadOnlyList<Category> categories =
        [
            new() { CategoryId = 1, Name = "Electronics" },
            new() { CategoryId = 2, Name = "Tools" }
        ];

        _repository
            .Setup(r => r.GetAllAsync(It.IsAny<CancellationToken>()))
            .ReturnsAsync(categories);

        // Act
        var result = await _service.GetAllAsync();

        // Assert
        result.Should().HaveCount(2);
    }

    [Fact]
    public async Task GetByIdAsync_ShouldReturnCategory()
    {
        var category = new Category
        {
            CategoryId = 1,
            Name = "Electronics"
        };

        _repository
            .Setup(r => r.GetByIdAsync(1, It.IsAny<CancellationToken>()))
            .ReturnsAsync(category);

        var result = await _service.GetByIdAsync(1);

        result.Should().NotBeNull();
        result!.Name.Should().Be("Electronics");
    }

    [Fact]
    public async Task GetByIdAsync_ShouldReturnNull_WhenNotFound()
    {
        _repository
            .Setup(r => r.GetByIdAsync(99, It.IsAny<CancellationToken>()))
            .ReturnsAsync((Category?)null);

        var result = await _service.GetByIdAsync(99);

        result.Should().BeNull();
    }

    [Fact]
    public async Task CreateAsync_ShouldTrimNameAndSave()
    {
        var category = new Category
        {
            Name = "  Electronics  "
        };

        await _service.CreateAsync(category);

        category.Name.Should().Be("Electronics");

        _repository.Verify(
            r => r.AddAsync(category, It.IsAny<CancellationToken>()),
            Times.Once);

        _repository.Verify(
            r => r.SaveChangesAsync(It.IsAny<CancellationToken>()),
            Times.Once);
    }

    [Fact]
    public async Task UpdateAsync_ShouldUpdateExistingCategory()
    {
        var existing = new Category
        {
            CategoryId = 1,
            Name = "Old Name"
        };

        var updated = new Category
        {
            CategoryId = 1,
            Name = "  New Name  "
        };

        _repository
            .Setup(r => r.GetTrackedByIdAsync(
                1, It.IsAny<CancellationToken>()))
            .ReturnsAsync(existing);

        await _service.UpdateAsync(updated);

        existing.Name.Should().Be("New Name");

        _repository.Verify(
            r => r.SaveChangesAsync(It.IsAny<CancellationToken>()),
            Times.Once);
    }

    [Fact]
    public async Task UpdateAsync_ShouldNotSave_WhenNotFound()
    {
        _repository
            .Setup(r => r.GetTrackedByIdAsync(
                99, It.IsAny<CancellationToken>()))
            .ReturnsAsync((Category?)null);

        await _service.UpdateAsync(new Category
        {
            CategoryId = 99,
            Name = "Test"
        });

        _repository.Verify(
            r => r.SaveChangesAsync(It.IsAny<CancellationToken>()),
            Times.Never);
    }

    [Fact]
    public async Task DeleteAsync_ShouldDeleteCategory_WhenNoProducts()
    {
        var category = new Category
        {
            CategoryId = 1,
            Name = "Tools"
        };

        _repository
            .Setup(r => r.GetTrackedByIdAsync(
                1, It.IsAny<CancellationToken>()))
            .ReturnsAsync(category);

        _repository
            .Setup(r => r.HasProductsAsync(
                1, It.IsAny<CancellationToken>()))
            .ReturnsAsync(false);

        await _service.DeleteAsync(1);

        _repository.Verify(
            r => r.DeleteAsync(category, It.IsAny<CancellationToken>()),
            Times.Once);

        _repository.Verify(
            r => r.SaveChangesAsync(It.IsAny<CancellationToken>()),
            Times.Once);
    }

    [Fact]
    public async Task DeleteAsync_ShouldThrow_WhenCategoryHasProducts()
    {
        var category = new Category
        {
            CategoryId = 1,
            Name = "Electronics"
        };

        _repository
            .Setup(r => r.GetTrackedByIdAsync(
                1, It.IsAny<CancellationToken>()))
            .ReturnsAsync(category);

        _repository
            .Setup(r => r.HasProductsAsync(
                1, It.IsAny<CancellationToken>()))
            .ReturnsAsync(true);

        Func<Task> act = () => _service.DeleteAsync(1);

        await act.Should()
            .ThrowAsync<InvalidOperationException>()
            .WithMessage("Category has products and cannot be deleted.");

        _repository.Verify(
            r => r.DeleteAsync(
                It.IsAny<Category>(),
                It.IsAny<CancellationToken>()),
            Times.Never);

        _repository.Verify(
            r => r.SaveChangesAsync(It.IsAny<CancellationToken>()),
            Times.Never);
    }

    [Fact]
    public async Task DeleteAsync_ShouldNotDelete_WhenNotFound()
    {
        _repository
            .Setup(r => r.GetTrackedByIdAsync(
                99, It.IsAny<CancellationToken>()))
            .ReturnsAsync((Category?)null);

        await _service.DeleteAsync(99);

        _repository.Verify(
            r => r.DeleteAsync(
                It.IsAny<Category>(),
                It.IsAny<CancellationToken>()),
            Times.Never);

        _repository.Verify(
            r => r.SaveChangesAsync(It.IsAny<CancellationToken>()),
            Times.Never);
    }
}
