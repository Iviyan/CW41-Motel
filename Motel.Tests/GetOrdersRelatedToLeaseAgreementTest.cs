using System.Diagnostics;
using System.Text.Json;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Motel.Database;
using Motel.Models;
using Motel.Utils;
using Npgsql;

namespace Motel.Tests;

public class GetOrdersRelatedToLeaseAgreementTest
{
    private const string ConnectionString = "Host=localhost;Port=5432;Database=motel_test;Username=motel;Password=motel";

    private NpgsqlDataSource npgsqlDataSource = null!;
    private DbContextOptions<ApplicationContext> contextOptions = null!;

    ApplicationContext CreateContext() => new(contextOptions);

    [OneTimeSetUp]
    public void Setup()
    {
        var dataSourceBuilder = new NpgsqlDataSourceBuilder(ConnectionString);
        npgsqlDataSource = dataSourceBuilder.Build();

        var optionsBuilder = new DbContextOptionsBuilder<ApplicationContext>();
        optionsBuilder.UseNpgsql(npgsqlDataSource);
        optionsBuilder.LogTo(TestContext.WriteLine, LogLevel.Information).EnableSensitiveDataLogging();
        contextOptions = optionsBuilder.Options;

        DateTime now = DateTime.Today.GetKindUtc();

        RoomType roomType = new() { Name = "Одноместный номер", Capacity = 1, PricePerHour = 100 };
        Room room = new() { Number = 1, RoomType = roomType, IsCleaningNeeded = false, IsReady = true };
        LeaseAgreement leaseAgreement = new()
            { Id = 1, ClientName = "Иванов Иван", StartAt = now, EndAt = now.AddDays(2) };
        LeaseRoom leaseRoom = new() { Room = room, LeaseAgreement = leaseAgreement };
        Service service = new() { Name = "Кувшин воды в номер", Price = 100, IsActual = true };
        ServiceOrder[] serviceOrders =
        {
            new() { Room = room, Service = service, Datetime = now.AddHours(1) },
            new() { Room = room, Service = service, Datetime = now.AddHours(10) },
            new() { Room = room, Service = service, Datetime = now.AddDays(3) }
        };

        var context = CreateContext();

        context.RoomTypes.Add(roomType);
        context.Rooms.Add(room);
        context.LeaseAgreements.Add(leaseAgreement);
        context.LeaseRooms.Add(leaseRoom);
        context.Services.Add(service);
        context.ServiceOrders.AddRange(serviceOrders);
        context.SaveChanges();

        serviceOrderIds = new[] { serviceOrders[0].Id, serviceOrders[1].Id };

        TestContext.WriteLine("Setup");
    }

    private int[] serviceOrderIds = null!;

    [Test]
    public void Test()
    {
        var context = CreateContext();

        var serviceOrders = context.ServiceOrders.FromSqlRaw("select * from get_orders_related_to_lease_agreement(1)").ToList();

        TestContext.WriteLine($"Execute get_orders_related_to_lease_agreement > {serviceOrders.Count}");
        
        CollectionAssert.AreEquivalent(serviceOrders.Select(r => r.Id), serviceOrderIds);

        TestContext.WriteLine($"Service orders > {JsonSerializer.Serialize(serviceOrders)}");

        Assert.Pass();
    }

    [OneTimeTearDown]
    public void TearDown()
    {
        var context = CreateContext();
        
        context.ServiceOrders.ExecuteDelete();
        context.Services.ExecuteDelete();
        context.LeaseAgreements.ExecuteDelete();
        context.Rooms.ExecuteDelete();
        context.RoomTypes.ExecuteDelete();
        context.LeaseRooms.ExecuteDelete();
        
        TestContext.WriteLine("Data deleted");
    }
}