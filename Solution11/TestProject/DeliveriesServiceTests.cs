
using FluentAssertions;
using Moq;
using WebApplication1.Models;
using WebApplication1.Repositories.Contracts;
using WebApplication1.Services.Implementations;
using Xunit;

namespace WebApplication1.Tests;

public class DeliveriesServiceTests
{
    private readonly Mock<IDeliveryRepository> _repository = new();
    private readonly DeliveriesService _service;

    public DeliveriesServiceTests()
    {
        _service = new DeliveriesService(_repository.Object);
    }

    [Fact]
    public async Task GetAllAsync_ShouldReturnDeliveries()
    {
        var deliveries = new List<Delivery>
        {
            new() { DeliveryId = 1, Quantity = 10 },
            new() { DeliveryId = 2, Quantity = 20 }
        };

        _repository
            .Setup(r => r.GetAllAsync(It.IsAny<CancellationToken>()))
            .ReturnsAsync(deliveries);

        var result = await _service.GetAllAsync();

        result.Should().HaveCount(2);
    }

    [Fact]
    public async Task GetByIdAsync_ShouldReturnDelivery()
    {
        var delivery = new Delivery
        {
            DeliveryId = 1,
            Quantity = 10
        };

        _repository
            .Setup(r => r.GetByIdAsync(
                1, It.IsAny<CancellationToken>()))
            .ReturnsAsync(delivery);

        var result = await _service.GetByIdAsync(1);

        result.Should().NotBeNull();
        result!.Quantity.Should().Be(10);
    }

    [Theory]
    [InlineData(0)]
    [InlineData(-1)]
    [InlineData(-100)]
    public async Task CreateAsync_ShouldThrow_WhenQuantityIsInvalid(
        int quantity)
    {
        var delivery = new Delivery
        {
            Quantity = quantity
        };

        Func<Task> act = () => _service.CreateAsync(delivery);

        await act.Should()
            .ThrowAsync<ArgumentOutOfRangeException>();

        _repository.Verify(
            r => r.AddAsync(
                It.IsAny<Delivery>(),
                It.IsAny<CancellationToken>()),
            Times.Never);
    }

    [Fact]
    public async Task CreateAsync_ShouldSave_WhenQuantityIsValid()
    {
        var delivery = new Delivery
        {
            ProductId = 1,
            WarehouseId = 1,
            Quantity = 15
        };

        await _service.CreateAsync(delivery);

        _repository.Verify(
            r => r.AddAsync(delivery, It.IsAny<CancellationToken>()),
            Times.Once);

        _repository.Verify(
            r => r.SaveChangesAsync(It.IsAny<CancellationToken>()),
            Times.Once);
    }

    [Fact]
    public async Task UpdateAsync_ShouldReturnTrue_WhenDeliveryExists()
    {
        var existing = new Delivery
        {
            DeliveryId = 1,
            Quantity = 10
        };

        var updated = new Delivery
        {
            DeliveryId = 1,
            ProductId = 2,
            WarehouseId = 3,
            Quantity = 50,
            DeliveryDate = new DateTime(2026, 11, 10)
        };

        _repository
            .Setup(r => r.GetTrackedByIdAsync(
                1, It.IsAny<CancellationToken>()))
            .ReturnsAsync(existing);

        var result = await _service.UpdateAsync(updated);

        result.Should().BeTrue();

        existing.Quantity.Should().Be(50);
        existing.ProductId.Should().Be(2);
        existing.WarehouseId.Should().Be(3);
        existing.DeliveryDate.Should().Be(updated.DeliveryDate);

        _repository.Verify(
            r => r.SaveChangesAsync(It.IsAny<CancellationToken>()),
            Times.Once);
    }

    [Fact]
    public async Task UpdateAsync_ShouldReturnFalse_WhenNotFound()
    {
        _repository
            .Setup(r => r.GetTrackedByIdAsync(
                99, It.IsAny<CancellationToken>()))
            .ReturnsAsync((Delivery?)null);

        var result = await _service.UpdateAsync(new Delivery
        {
            DeliveryId = 99,
            Quantity = 10
        });

        result.Should().BeFalse();

        _repository.Verify(
            r => r.SaveChangesAsync(It.IsAny<CancellationToken>()),
            Times.Never);
    }

    [Fact]
    public async Task DeleteAsync_ShouldDeleteExistingDelivery()
    {
        var delivery = new Delivery
        {
            DeliveryId = 1,
            Quantity = 10
        };

        _repository
            .Setup(r => r.GetTrackedByIdAsync(
                1, It.IsAny<CancellationToken>()))
            .ReturnsAsync(delivery);

        await _service.DeleteAsync(1);

        _repository.Verify(
            r => r.DeleteAsync(delivery, It.IsAny<CancellationToken>()),
            Times.Once);

        _repository.Verify(
            r => r.SaveChangesAsync(It.IsAny<CancellationToken>()),
            Times.Once);
    }

    [Fact]
    public async Task DeleteAsync_ShouldNotSave_WhenNotFound()
    {
        _repository
            .Setup(r => r.GetTrackedByIdAsync(
                99, It.IsAny<CancellationToken>()))
            .ReturnsAsync((Delivery?)null);

        await _service.DeleteAsync(99);

        _repository.Verify(
            r => r.DeleteAsync(
                It.IsAny<Delivery>(),
                It.IsAny<CancellationToken>()),
            Times.Never);

        _repository.Verify(
            r => r.SaveChangesAsync(It.IsAny<CancellationToken>()),
            Times.Never);
    }

    [Fact]
    public async Task ExistsAsync_ShouldReturnTrue_WhenExists()
    {
        _repository
            .Setup(r => r.ExistsAsync(
                1, It.IsAny<CancellationToken>()))
            .ReturnsAsync(true);

        var result = await _service.ExistsAsync(1);

        result.Should().BeTrue();
    }

    [Fact]
    public async Task ExistsAsync_ShouldReturnFalse_WhenNotFound()
    {
        _repository
            .Setup(r => r.ExistsAsync(
                99, It.IsAny<CancellationToken>()))
            .ReturnsAsync(false);

        var result = await _service.ExistsAsync(99);

        result.Should().BeFalse();
    }
}
