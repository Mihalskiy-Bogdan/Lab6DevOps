using AutoMapper;
using ECommerce.Api.Orders.Db;
using ECommerce.Api.Orders.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace ECommerce.Api.Orders.Providers;

public class OrdersProvider : IOrdersProvider
{
    private readonly OrdersDbContext dbContext;
    private readonly ILogger<OrdersProvider> logger;
    private readonly IMapper mapper;

    public OrdersProvider(OrdersDbContext dbContext, ILogger<OrdersProvider> logger, IMapper mapper)
    {
        this.dbContext = dbContext;
        this.logger = logger;
        this.mapper = mapper;
        SeedData();
    }

    private void SeedData()
    {
        if (!dbContext.Orders.Any())
        {
            dbContext.Orders.Add(new Order
            {
                Id = 1,
                CustomerId = 1,
                OrderDate = DateTime.Now,
                Total = 100m,
                Items = new List<OrderItem>
                {
                    new() { ProductId = 1, Quantity = 10, UnitPrice = 10m },
                    new() { ProductId = 2, Quantity = 10, UnitPrice = 10m },
                    new() { ProductId = 3, Quantity = 10, UnitPrice = 10m }
                }
            });

            dbContext.Orders.Add(new Order
            {
                Id = 2,
                CustomerId = 1,
                OrderDate = DateTime.Now.AddDays(-1),
                Total = 100m,
                Items = new List<OrderItem>
                {
                    new() { ProductId = 2, Quantity = 10, UnitPrice = 10m },
                    new() { ProductId = 3, Quantity = 1, UnitPrice = 100m }
                }
            });

            dbContext.Orders.Add(new Order
            {
                Id = 3,
                CustomerId = 2,
                OrderDate = DateTime.Now,
                Total = 100m,
                Items = new List<OrderItem>
                {
                    new() { ProductId = 1, Quantity = 10, UnitPrice = 10m },
                    new() { ProductId = 2, Quantity = 10, UnitPrice = 10m },
                    new() { ProductId = 3, Quantity = 1, UnitPrice = 100m }
                }
            });

            dbContext.SaveChanges();
        }
    }

    public async Task<(bool IsSuccess, IEnumerable<Models.Order>? Orders, string? ErrorMessage)> GetOrdersAsync(int customerId)
    {
        try
        {
            var orders = await dbContext.Orders
                .Where(o => o.CustomerId == customerId)
                .Include(o => o.Items)
                .ToListAsync();

            if (orders != null && orders.Any())
            {
                var result = mapper.Map<IEnumerable<Db.Order>, IEnumerable<Models.Order>>(orders);
                return (true, result, null);
            }

            return (false, null, "Not Found");
        }
        catch (Exception ex)
        {
            logger?.LogError(ex.ToString());
            return (false, null, ex.Message);
        }
    }
}