using Microsoft.EntityFrameworkCore;
using Prometheus;
using StackExchange.Redis;
using WarehouseApi.Data;
using WarehouseApi.Services;

var builder = WebApplication.CreateBuilder(args);

// ── 1. База данных PostgreSQL ──────────────────────────────────────────
var connStr = Environment.GetEnvironmentVariable("CONNECTION_STRING")
              ?? "Host=localhost;Database=warehouse;Username=postgres;Password=postgres";
builder.Services.AddDbContext<AppDbContext>(opt => opt.UseNpgsql(connStr));

// ── 2. Redis ───────────────────────────────────────────────────────────
var redisConn = Environment.GetEnvironmentVariable("REDIS_CONNECTION") ?? "localhost:6379";
builder.Services.AddSingleton<IConnectionMultiplexer>(
    ConnectionMultiplexer.Connect(redisConn));
builder.Services.AddScoped<CacheService>();

// ── 3. Стандартные сервисы ─────────────────────────────────────────────
builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

// ── ДОБАВЛЕНО: Поддержка статических файлов для веб-интерфейса ────────
builder.Services.AddSpaStaticFiles(configuration =>
{
    configuration.RootPath = "wwwroot";
});

var app = builder.Build();

// ── 4. Применение миграций и заполнение тестовыми данными ─────────────
using (var scope = app.Services.CreateScope())
{
    var db = scope.ServiceProvider.GetRequiredService<AppDbContext>();
    db.Database.Migrate();
    DbSeeder.Seed(db);
}

// ── 5. Middleware ──────────────────────────────────────────────────────
app.UseSwagger();
app.UseSwaggerUI();

app.UseHttpMetrics();
app.MapMetrics();

app.UseStaticFiles();           // ← Добавлено
app.UseSpaStaticFiles();        // ← Добавлено

app.UseAuthorization();
app.MapControllers();

// ── ДОБАВЛЕНО: Маршрут для открытия веб-интерфейса ────────────────────
app.MapGet("/warehouse", context =>
{
    context.Response.Redirect("/warehouse/index.html");
    return Task.CompletedTask;
});

app.Run();


