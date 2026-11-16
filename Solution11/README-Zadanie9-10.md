# Задания 9 и 10 – Warehouse REST API + C# Console Client

## Какво е добавено

- `WarehouseApi` – отделен ASP.NET Core Web API host.
- `WarehouseClient` – отделно C# Console приложение, което консумира API чрез `HttpClient` и JSON.
- API endpoints:
  - `POST /api/auth/register`
  - `POST /api/auth/login`
  - `POST /api/auth/refresh`
  - `GET /api/categories` (JWT)
  - `GET /api/products` (JWT)
  - `GET /api/products/{id}` (JWT)
  - `POST /api/products` (JWT)
  - `PUT /api/products/{id}` (JWT)
  - `DELETE /api/products/{id}` (JWT)
- API използва съществуващите Service и Repository класове от MVC проекта.
- SQLite базата е споделена: `WebApp/warehouse.db`.

## Стартиране

Изисква се .NET 9 SDK.

1. Отвори `Solution11.sln` в Rider или Visual Studio.
2. Възстанови NuGet пакетите (Restore).
3. Стартирай `WarehouseApi` проекта. API адреси:
   - HTTPS: `https://localhost:7001`
   - HTTP: `http://localhost:5001`
   - Scalar: `https://localhost:7001/scalar`
4. Стартирай `WarehouseClient` проекта.
5. В конзолата избери `1. Register`, създай потребител, след това `2. Login`.
6. След успешен вход използвай `3. List products` или `4. Create product`.

При локална разработка, ако HTTPS сертификатът не е доверен, изпълни:

```bash
dotnet dev-certs https --trust
```

Или използвай HTTP адреса `http://localhost:5001/` в `WarehouseClient/Program.cs`.

## Валидации при регистрация

Съществуващият `AuthService` изисква:
- Username: 3–20 символа, само букви.
- Password: 8–30 символа, поне една главна, една малка буква, цифра и специален символ.

Примерни тестови данни:
- Username: `Student`
- Password: `Student123!`

## Задание 10 – Design Patterns

### Repository Pattern

API не използва директно `WarehouseDbContext` в контролерите. Бизнес услугите използват интерфейси като `IProductRepository`, а реализациите инкапсулират Entity Framework Core достъпа до данните.

### Dependency Injection

Интерфейсите и имплементациите се регистрират в `WarehouseApi/Program.cs`. ASP.NET Core създава необходимите зависимости и ги подава чрез constructor injection. Например `ProductsController` получава `IProductService`, а `ProductService` получава `IProductRepository`.

## Важни бележки

- MVC приложението остава отделно и не е премахнато.
- API и MVC използват един и същ SQLite файл. Стартирай приложенията последователно, а не едновременно, ако правиш промени по схемата чрез миграции.
- Промени примерния JWT secret в `WarehouseApi/appsettings.json` преди реално публикуване. В production използвай User Secrets или environment variables.
- Регистрацията създава потребител с роля `User`. Тя не дава административни права.
