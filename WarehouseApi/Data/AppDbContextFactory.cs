using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;

namespace WarehouseApi.Data;

public class AppDbContextFactory : IDesignTimeDbContextFactory<AppDbContext>
{
    public AppDbContext CreateDbContext(string[] args)
    {
        var optionsBuilder = new DbContextOptionsBuilder<AppDbContext>();

        // Строка подключения только для генерации миграций — не для продакшена
        optionsBuilder.UseNpgsql("Host=localhost;Database=warehouse;Username=postgres;Password=postgres");

        return new AppDbContext(optionsBuilder.Options);
    }
}