# Warehouse Manager — Система управления складом
---

## Стек технологий

| Компонент | Технология |
|-----------|-----------|
| Backend | ASP.NET Core 8 Web API |
| База данных | PostgreSQL 16 |
| Кэш | Redis 7 |
| Обратный прокси | Nginx |
| Мониторинг | Prometheus + Grafana |
| Контейнеризация | Docker + Docker Compose |
| CI/CD | GitHub Actions |

---

## Архитектура

<img width="814" height="496" alt="Снимок экрана 2026-04-04 005851" src="https://github.com/user-attachments/assets/454bc9b7-9b67-4b1b-8b5f-86a182309a23" />


Все компоненты запускаются в отдельных Docker-контейнерах и общаются через внутреннюю сеть Docker.

---

## Быстрый старт

### Требования

- Docker Desktop (с включённым WSL2 на Windows)
- Git

### Запуск

```bash
# 1. Клонировать репозиторий
git clone https://github.com/Edelmann_228/warehouse.git
cd warehouse

# 2. Запустить все контейнеры
docker-compose up --build

# 3. Открыть в браузере
# Веб-интерфейс:  http://localhost/warehouse
# Swagger UI:     http://localhost/swagger
# Grafana:        http://localhost:3000   (admin / admin)
# Метрики:        http://localhost/metrics
```

### Остановка

```bash
docker-compose down
```

### Полный сброс (включая данные БД)

```bash
docker-compose down -v
```

---

## Структура проекта

<img width="812" height="475" alt="Снимок экрана 2026-04-04 005941" src="https://github.com/user-attachments/assets/7ba8c72c-fcbf-4b40-9f4b-6dc4f2132fc6" />



---

## API

Базовый URL: `http://localhost/api`

Полная документация доступна в **Swagger UI**: `http://localhost/swagger`

### Товары (`/api/products`)

| Метод | Эндпоинт | Описание |
|-------|----------|----------|
| GET | `/api/products` | Список всех товаров (кэш Redis, TTL 5 мин) |
| GET | `/api/products/{id}` | Товар по ID |
| POST | `/api/products` | Создать товар |
| PUT | `/api/products/{id}` | Обновить товар |
| DELETE | `/api/products/{id}` | Удалить товар |

Пример тела запроса (POST/PUT):
```json
{
  "name": "Гайка М8",
  "sku": "42",
  "unit": "шт",
  "price": 3.50,
  "stockQuantity": 100
}
```

### Поставки (`/api/supplies`)

| Метод | Эндпоинт | Описание |
|-------|----------|----------|
| GET | `/api/supplies` | Список поставок |
| GET | `/api/supplies/{id}` | Поставка по ID |
| POST | `/api/supplies` | Создать поставку |
| PUT | `/api/supplies/{id}` | Обновить поставку |
| DELETE | `/api/supplies/{id}` | Удалить поставку |

Пример тела запроса:
```json
{
  "supplierName": "ООО Металл-Трейд",
  "supplyDate": "2025-04-04",
  "status": "Pending",
  "items": []
}
```

### Списания (`/api/writeoffs`)

| Метод | Эндпоинт | Описание |
|-------|----------|----------|
| GET | `/api/writeoffs` | Список списаний |
| GET | `/api/writeoffs/{id}` | Списание по ID |
| POST | `/api/writeoffs` | Создать списание |
| PUT | `/api/writeoffs/{id}` | Обновить списание |
| DELETE | `/api/writeoffs/{id}` | Удалить списание |

Пример тела запроса:
```json
{
  "productId": 3,
  "quantity": 5,
  "reason": "Истёк срок годности",
  "writtenOffAt": "2025-04-04"
}
```

---

## Веб-интерфейс

Открывается по адресу: `http://localhost/warehouse`

Файл: `WarehouseApi/wwwroot/warehouse/index.html`

### Как обновить интерфейс

**Вариант 1 — через Docker (без пересборки):**
```bash
docker cp index.html warehouse-app-1:/app/wwwroot/warehouse/index.html
```

**Вариант 2 — через volume в docker-compose.yml:**
```yaml
volumes:
  - ./WarehouseApi/wwwroot:/app/wwwroot
```
После этого достаточно просто сохранить файл — изменения подхватываются сразу.

---

## Роли пользователей

Авторизация реализована на стороне фронтенда.

| Логин | Пароль | Роль | Возможности |
|-------|--------|------|-------------|
| admin | admin123 | Администратор | Полный доступ: просмотр, изменение, поставки, списания |
| user | user123 | Пользователь | Каталог товаров + корзина + покупка |

### Что видит администратор

- **Товары** — таблица с сортировкой по артикулу, кнопка «Изм.»
- **Поставки** — пополнение остатков существующих товаров (PUT) и добавление новых (POST), удаление товаров
- **Списания** — выбор товаров и количества, указание причины, уменьшение остатков (PUT)

### Что видит пользователь

- Каталог товаров с ценами и остатками
- Выбор количества кнопками +/−
- Корзина с итоговой суммой
- Кнопка «Купить» — уменьшает `stockQuantity` в БД

---

## Мониторинг (Grafana)

### Открыть

`http://localhost:3000` → логин `admin`, пароль `admin`

### Настройка источника данных

1. Левое меню → **Connections** → **Data sources**
2. **Add data source** → **Prometheus**
3. URL: `http://prometheus:9090`
4. **Save & test**

### Полезные PromQL-запросы для дашборда

| Панель | Запрос |
|--------|--------|
| Запросов в секунду | `rate(http_requests_received_total[1m])` |
| Новые товары (POST) | `increase(http_requests_received_total{method="POST",path="/api/products"}[5m])` |
| Покупки/списания (PUT) | `increase(http_requests_received_total{method="PUT",path=~"/api/products.*"}[5m])` |
| Ошибки 5xx | `rate(http_requests_received_total{code=~"5.."}[1m])` |
| Время ответа (p95) | `histogram_quantile(0.95, rate(http_request_duration_seconds_bucket[5m]))` |
| Память процесса | `process_working_set_bytes` |

> Точные имена метрик смотрите на `http://localhost/metrics`

### Автозагрузка дашборда

Экспортируйте дашборд в JSON (шестерня → Export) и положите в `grafana/dashboards/`. В `docker-compose.yml` примонтируйте папку:

```yaml
grafana:
  volumes:
    - ./grafana/dashboards:/etc/grafana/provisioning/dashboards
```

---

## CI/CD

Файл: `.github/workflows/ci.yml`

Пайплайн запускается при каждом `push` в репозиторий:

<img width="800" height="408" alt="Снимок экрана 2026-04-04 010100" src="https://github.com/user-attachments/assets/8e7f0b49-6cbf-4b3b-95e8-f026b2dc2d14" />


Статус пайплайна виден во вкладке **Actions** на GitHub.

### Создать тестовый проект (если ещё не создан)

```bash
dotnet new xunit -n WarehouseApi.Tests
cd WarehouseApi.Tests
dotnet add reference ../WarehouseApi/WarehouseApi.csproj
```

---

## C#-клиент

Консольное приложение, демонстрирующее использование делегатов.

### Запуск

```bash
cd WarehouseClient
dotnet run
```

### Что делает клиент

1. Получает список товаров (GET)
2. Создаёт тестовый товар (POST)
3. Получает товар по ID (GET)
4. Обновляет товар (PUT)
5. Удаляет товар (DELETE)

Каждый запрос обрабатывается через делегат `OnRequestCompleted` — выводит в консоль статус-код и время выполнения. После трёх первых операций обработчик логирования отключается (`-=`).

### Пример вывода

```
[200] GET /api/products — 45ms
[201] POST /api/products — 112ms
[200] GET /api/products/6 — 23ms
--- логирование отключено ---
[200] PUT /api/products/6 — 67ms
[204] DELETE /api/products/6 — 38ms
```

---

## Проверка работоспособности

```bash
# Все контейнеры запущены
docker-compose ps

# API отвечает
curl http://localhost/api/products

# Метрики доступны
curl http://localhost/metrics

# БД содержит данные
docker exec -it warehouse-db-1 psql -U postgres -d warehouse -c "SELECT * FROM \"Products\";"
```
