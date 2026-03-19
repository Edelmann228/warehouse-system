namespace WarehouseClient;

// Собственный делегат: вызывается после каждого HTTP-запроса
// endpoint — адрес эндпоинта, statusCode — HTTP-статус, elapsedMs — время выполнения
public delegate void OnRequestCompleted(string endpoint, int statusCode, long elapsedMs);

// Делегат для обработки ошибок HTTP-запросов
public delegate void OnRequestError(string endpoint, string errorMessage);
