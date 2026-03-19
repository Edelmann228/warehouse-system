using System.Diagnostics;
using System.Text;
using System.Text.Json;

namespace WarehouseClient;

public class ApiService
{
    private readonly HttpClient _http;
    private readonly string _base;

    // Многоадресный делегат: к нему можно подключить несколько обработчиков через +=
    public OnRequestCompleted? RequestCompleted;
    public OnRequestError? RequestError;

    public ApiService(string baseUrl)
    { _http = new HttpClient(); _base = baseUrl.TrimEnd('/'); }

    // Внутренний метод: отправляет запрос, замеряет время, вызывает делегаты
    private async Task<HttpResponseMessage> SendAsync(HttpRequestMessage req, string endpoint)
    {
        var sw = Stopwatch.StartNew();
        try
        {
            var res = await _http.SendAsync(req);
            sw.Stop();
            // Вызываем все подписанные обработчики события
            RequestCompleted?.Invoke(endpoint, (int)res.StatusCode, sw.ElapsedMilliseconds);
            return res;
        }
        catch (Exception ex)
        {
            sw.Stop();
            RequestError?.Invoke(endpoint, ex.Message);
            throw;
        }
    }

    // Получить список товаров
    public async Task<string> GetProductsAsync()
    {
        var req = new HttpRequestMessage(HttpMethod.Get, $"{_base}/api/products");
        var res = await SendAsync(req, "GET /api/products");
        return await res.Content.ReadAsStringAsync();
    }

    // Получить товар по ID
    public async Task<string> GetProductByIdAsync(int id)
    {
        var req = new HttpRequestMessage(HttpMethod.Get, $"{_base}/api/products/{id}");
        var res = await SendAsync(req, $"GET /api/products/{id}");
        return await res.Content.ReadAsStringAsync();
    }

    // Создать товар. Func<object> — делегат, возвращающий тело запроса
    public async Task<string> CreateProductAsync(Func<object> bodyFactory)
    {
        var body = JsonSerializer.Serialize(bodyFactory());
        var req = new HttpRequestMessage(HttpMethod.Post, $"{_base}/api/products")
        { Content = new StringContent(body, Encoding.UTF8, "application/json") };
        var res = await SendAsync(req, "POST /api/products");
        return await res.Content.ReadAsStringAsync();
    }

    // Обновить товар. Action<Dictionary> — делегат, заполняющий поля
    public async Task<string> UpdateProductAsync(int id, Action<Dictionary<string, object>> mutate)
    {
        var data = new Dictionary<string, object>();
        mutate(data);  // вызываем переданный делегат для заполнения словаря
        var body = JsonSerializer.Serialize(data);
        var req = new HttpRequestMessage(HttpMethod.Put, $"{_base}/api/products/{id}")
        { Content = new StringContent(body, Encoding.UTF8, "application/json") };
        var res = await SendAsync(req, $"PUT /api/products/{id}");
        return await res.Content.ReadAsStringAsync();
    }

    // Удалить товар
    public async Task<string> DeleteProductAsync(int id)
    {
        var req = new HttpRequestMessage(HttpMethod.Delete, $"{_base}/api/products/{id}");
        var res = await SendAsync(req, $"DELETE /api/products/{id}");
        return res.StatusCode.ToString();
    }
}

