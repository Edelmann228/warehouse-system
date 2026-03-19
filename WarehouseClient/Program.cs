using WarehouseClient;

Console.WriteLine("=== Склад: демонстрация делегатов ===");

var api = new ApiService("http://localhost");

// ── Объявляем обработчики ──────────────────────────────────────────────
// Обработчик 1: вывод результата в консоль
OnRequestCompleted consoleHandler = (endpoint, code, ms) =>
{
    Console.ForegroundColor = code < 400 ? ConsoleColor.Green : ConsoleColor.Red;
    Console.WriteLine($"  [{code}] {endpoint}  ({ms} мс)");
    Console.ResetColor();
};

// Обработчик 2: запись в лог-файл
var logPath = "requests.log";
OnRequestCompleted fileHandler = (endpoint, code, ms) =>
    File.AppendAllText(logPath, $"{DateTime.Now:O}  {code}  {endpoint}  {ms}ms{Environment.NewLine}");

// Обработчик ошибок
api.RequestError = (endpoint, msg) =>
    Console.WriteLine($"  [ОШИБКА] {endpoint}: {msg}");

// ── Подписываем оба обработчика (многоадресный делегат +=) ────────────
api.RequestCompleted += consoleHandler;
api.RequestCompleted += fileHandler;

// ── Операция 1: GET список товаров ────────────────────────────────────
Console.WriteLine("\nОп.1 — Получение списка товаров:");
var products = await api.GetProductsAsync();
Console.WriteLine(products.Length > 300 ? products[..300] + "..." : products);

// ── Операция 2: POST создание нового товара ───────────────────────────
Console.WriteLine("\nОп.2 — Создание товара:");
var created = await api.CreateProductAsync(() => new {
    name = "Шуруп М6",
    sku = "SCREW-M6",
    unit = "шт",
    price = 0.80,
    stockQuantity = 1000
});
Console.WriteLine(created);

// ── Операция 3: GET товар по ID ───────────────────────────────────────
Console.WriteLine("\nОп.3 — Товар ID=1:");
var one = await api.GetProductByIdAsync(1);
Console.WriteLine(one);

// ── Отключаем fileHandler после трёх операций ─────────────────────────
Console.WriteLine("\n>> Отписываем fileHandler (fileHandler -= ...)");
Console.WriteLine("   Дальнейшие операции НЕ пишутся в файл.");
api.RequestCompleted -= fileHandler;

// ── Операция 4: PUT обновление ────────────────────────────────────────
Console.WriteLine("\nОп.4 — Обновление товара ID=1:");
var updated = await api.UpdateProductAsync(1, d => {
    d["name"] = "Болт М10 v2"; d["sku"] = "BOLT-M10";
    d["unit"] = "шт"; d["price"] = 3.00; d["stockQuantity"] = 450;
});
Console.WriteLine(updated);

// ── Операция 5: DELETE удаление ───────────────────────────────────────
Console.WriteLine("\nОп.5 — Удаление товара ID=2:");
var status = await api.DeleteProductAsync(2);
Console.WriteLine($"  Статус: {status}");

Console.WriteLine("\n=== Готово ===");
Console.WriteLine($"Лог первых 3 операций: {Path.GetFullPath(logPath)}");
