using System.Diagnostics;
using System.Text.Json;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Motel.Database;
using Motel.Models;
using Npgsql;

namespace Motel.Tests;

public class CreateLeaseAgreementTest
{
    private const string ConnectionString = "Host=localhost;Port=5432;Database=motel_test;Username=motel;Password=motel";

    private NpgsqlDataSource npgsqlDataSource = null!;
    private DbContextOptions<ApplicationContext> contextOptions = null!;

    private ApplicationContext? _context;
    ApplicationContext Context => _context ??= new ApplicationContext(contextOptions);

    ApplicationContext CreateContext() => new(contextOptions);

    [OneTimeSetUp]
    public void Setup()
    {
        var dataSourceBuilder = new NpgsqlDataSourceBuilder(ConnectionString);
        npgsqlDataSource = dataSourceBuilder.Build();

        var optionsBuilder = new DbContextOptionsBuilder<ApplicationContext>();
        optionsBuilder.UseNpgsql(npgsqlDataSource);
        optionsBuilder.LogTo(TestContext.WriteLine, LogLevel.Trace).EnableSensitiveDataLogging();
        contextOptions = optionsBuilder.Options;

        RoomType roomType = new RoomType() { Name = "Одноместный номер", Capacity = 1, PricePerHour = 100 };
        Context.RoomTypes.Add(roomType);
        Context.Rooms.Add(new Room() { Number = 1, RoomType = roomType, IsCleaningNeeded = false, IsReady = true });
        Context.SaveChanges();

        TestContext.WriteLine("Setup");
    }

    [Test]
    public void Test()
    {
        var context = CreateContext();

        string clientName = "Ivan";
        DateTime start = new(2022, 10, 10, 0, 0, 0, DateTimeKind.Utc);
        DateTime end = new(2022, 10, 12, 0, 0, 0, DateTimeKind.Utc);
        int[] rooms = new[] { 1 };

        context.Database.ExecuteSqlInterpolated($"call create_lease_agreement({clientName}, {start}, {end}, {rooms})");

        TestContext.WriteLine($"Execute create_lease_agreement");

        var leaseAgreement = context.LeaseAgreements.Include(l => l.LeaseRooms).First();
        Assert.AreEqual(leaseAgreement.ClientName, clientName);
        Assert.AreEqual(leaseAgreement.StartAt, start);
        Assert.AreEqual(leaseAgreement.EndAt, end);
        CollectionAssert.AreEquivalent(leaseAgreement.LeaseRooms.Select(r => r.RoomNumber).ToArray(), rooms);
        
        TestContext.WriteLine($"Lease agreement > {JsonSerializer.Serialize(leaseAgreement)}\n rooms: {JsonSerializer.Serialize(leaseAgreement.LeaseRooms.Select(r => r.RoomNumber))}");
        
        Assert.Pass();
    }

    [OneTimeTearDown]
    public void TearDown()
    {
        Context.LeaseAgreements.ExecuteDelete();
        Context.Rooms.ExecuteDelete();
        Context.RoomTypes.ExecuteDelete();
        TestContext.WriteLine("Data deleted");
    }
}