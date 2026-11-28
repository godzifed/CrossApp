# CrossApp
Наскрізний проєкт з крос-платформного програмування.

Предметна область: **Склад**.
Сутності: `Product` (товар), `StockBatch` (партія), `Warehouse` (склад), `Movement` (переміщення).
Призначення: облік залишків товарів по партіях.

## Інваріанти доменної моделі (Лабораторна робота 4)

1. **Обов'язковість ідентифікаторів:** `Id`, `Sku`, `Name` не можуть бути порожніми або складатись лише з пробілів (`ArgumentException`).
2. **Невід'ємний початковий залишок:** Початкова кількість товару при створенні не може бути меншою за нуль (`ArgumentOutOfRangeException`).
3. **Валідація величини операцій:** Кількість товару в операціях `RegisterArrival` та `Issue` повинна бути більшою за нуль (`ArgumentOutOfRangeException`).
4. **Захист від перевитрати:** Кількість видачі товару не може перевищувати його фактичний залишок (`InvalidOperationException`).
5. **Інваріант місткості (дві сутності):** Сумарна кількість товарів не може перевищувати ліміт місткості складу `Warehouse.MaxCapacity` (перевіряється в `WarehouseTransferService`, викликає `InvalidOperationException`).
6. **Допустимість переходів станів:** Переміщення товару `StockMovement` дозволяє перехід з `Draft` у `Confirmed` або `Cancelled`, але забороняє зміну вже підтвердженого чи скасованого документа (`InvalidOperationException`).


## Сервісний шар та сховища (Лабораторна робота 5)

- **Контракт сховища:** `ICatalogStore`
- **Реалізації:** `InMemoryCatalogStore` (у пам'яті), `FileCatalogStore` (JSON-файл `data/catalog.json`)
- **Сервіс:** `CatalogService` (залежить виключно від `ICatalogStore`, ін'єкція через конструктор)
- **Точка збірки (Composition Root):** `src/Cli/Program.cs` (перемикання прапорцем `--file`)

## Лабораторна робота 7: LINQ

**Відкладене виконання (Deferred Execution)**
LINQ-запити (такі як `Where`, `Select`) не виконуються в момент їх оголошення, а лише створюють "рецепт". Вони виконуються тільки тоді, коли ми перебираємо результат або викликаємо методи `Count()`, `ToList()`. 
У моєму демо в `Program.cs` перший виклик `query.Count()` зчитав 4 дефіцитні товари. Після цього я додав ще один товар у колекцію. Оскільки `query` — це відкладений запит, другий виклик `Count()` заново пройшовся по колекції і вивів вже число 5. Коли ж я викликав `ToList()`, дані зафіксувалися в пам'яті (snapshot), і подальше очищення списку на них не вплинуло.

**Порівняння синтаксису запитів (Query Syntax) та методів (Method Syntax)**
У додатковому завданні я використав `Query Syntax` для звіту групування (`report-groups`):
```csharp
from p in source
group p by p.Unit into g
orderby g.Sum(x => x.Quantity) descending
select new GroupSummary(g.Key, g.Count(), g.Sum(x => x.Quantity));

## Лабораторна робота №8: Асинхронний імпорт, скасування та доменні винятки

## 1. Які операції стали асинхронними і чому саме вони (а які лишились синхронними)
*   **Стали асинхронними:**
    *   **Читання файлу з диска** (`FileStream(useAsync: true)` та `JsonSerializer.DeserializeAsync`). Це операції введення-виведення (I/O), які можуть тривати довго. Асинхронність дозволяє звільнити потік, поки система чекає на відповідь від диска.
    *   **Імпорт та обробка даних у сервісі** (`CatalogService.ImportAsync`). Оскільки він викликає асинхронний імпортер, весь ланцюжок до точки входу (`Program.cs`) також має стати асинхронним (принцип "асинхронність на всіх рівнях").
*   **Залишились синхронними:**
    *   **Робота зі сховищем в пам'яті** (`_store.Add`, `_store.GetById`). Це прості операції з колекціями в оперативній пам'яті (CPU-bound), вони виконуються миттєво і не блокують потоки I/O.
    *   **Підняття подій** (`OnChanged`).
    *   **Генерація звітів** (`ReportService`). Це чисті функції, які швидко обробляють наявні дані в пам'яті.

## 2. Фрагменти коду

**Фрагмент LoadAsync:** 
```csharp
public async IAsyncEnumerable<Product> LoadAsync(string path, [EnumeratorCancellation] CancellationToken ct = default)
{
    ct.ThrowIfCancellationRequested();
    await using var stream = new FileStream(path, FileMode.Open, FileAccess.Read, FileShare.Read, 4096, useAsync: true);
    
    // ...

## 4. Додаткове завдання: Покриття коду (Code Coverage)
Під час виконання команди `dotnet test --collect:"XPlat Code Coverage"` було згенеровано звіт про покриття. Ядро програми (`Core`) покрито тестами на високому рівні, оскільки вся бізнес-логіка (`CatalogService`, `JsonCatalogImporter`, `Product`) ретельно перевіряється.

**Яка частина коду не покрита і чому це абсолютно прийнятно:**
1. **DTO-класи та Records** (наприклад, `ProductDto`, `GroupSummary`). Це прості контейнери даних, які не містять логіки (if, for, while). Їхнє пряме тестування — це тестування самого компілятора C#, що не має сенсу.
2. **Базові конструктори винятків** (наприклад, порожній конструктор `DomainException()`). Ми протестували лише ті конструктори, які реально використовуємо для загортання помилок із JSON.
3. **Шар CLI (`Program.cs`)**. Точка входу та вивід у консоль не покриваються модульними тестами (Unit Tests), оскільки це рівень інтеграційного та ручного тестування.

Такий підхід є правильною інженерною практикою. Мета модульних тестів — перевірити поведінку (Behavior), інваріанти та скасування, а не досягти "100% покриття" заради галочки.


## Лабораторна робота №9: Шляхи, JSON, конфігурація та Secrets

### 1. Профілі середовищ (Development і Production)
Конфігурація формується послідовно: `appsettings.json` → `appsettings.{Environment}.json` → `user-secrets` → змінні середовища (`CROSSAPP_*`) → аргументи командного рядка.

| Профіль | Файл налаштувань | `Logging:MinimumLevel` | Розташування даних за замовчуванням |
| :--- | :--- | :--- | :--- |
| **Production** | `appsettings.json` | `Warning` | `<AppContext.BaseDirectory>/data/items.json` |
| **Development** | `appsettings.Development.json` | `Debug` | `<AppContext.BaseDirectory>/data/items.json` |

### 2. Таблиця ключів конфігурації

| Ключ | Призначення | Значення за замовчуванням | Змінна середовища |
| :--- | :--- | :--- | :--- |
| `EnvironmentName` | Назва поточного профілю | `Development` (у dev) / `Production` | `CROSSAPP_EnvironmentName` |
| `Logging:MinimumLevel` | Мінімальний рівень логування | `Debug` (у dev) / `Warning` | `CROSSAPP_Logging__MinimumLevel` |
| `Storage:DataPath` | Каталог збереження даних | `data` | `CROSSAPP_Storage__DataPath` або `CROSSAPP_DATA_PATH` |
| `Storage:FileName` | Ім'я JSON-файлу сховища | `items.json` | `CROSSAPP_Storage__FileName` |
| `Warehouse:DefaultUnit` | Одиниця виміру за замовчуванням | `шт` | `CROSSAPP_Warehouse__DefaultUnit` |
| `Integration:ApiKey` | Навчальний секрет (не зберігається в Git) | *(із `user-secrets`)* | `CROSSAPP_Integration__ApiKey` |

### 3. Побудова кросплатформних шляхів (`Path.Combine`)
Фрагмент із `src/Core/Configuration/AppOptions.cs`:
```csharp
public string ResolveFullPath(string baseDir)
{
    var root = Path.IsPathRooted(DataPath)
        ? DataPath
        : Path.Combine(baseDir, DataPath);

    Directory.CreateDirectory(root);
    return Path.GetFullPath(Path.Combine(root, FileName));
}


## Структура Solution
Проєкт розділено на окрему бібліотеку логіки та точку входу:
- src/Core (classlib): Бібліотека класів, що не має точки входу та компілюється в `.dll`. Містить збір інформації про середовище (`EnvironmentInfo`, `EnvironmentReport`). Підтримує multi-targeting (`net8.0;net10.0`).
  - Каталоги для наступних робіт:
    - `Core/Dto/` — record-типи формату даних.
    - `Core/Domain/` — доменні сутності з інваріантами.
    - `Core/Storage/` — реалізації сховищ.
- src/Cli (console): Консольний застосунок, що посилається на `Core` через `ProjectReference`. Містить лише виклик бібліотеки та форматування виводу без бізнес-логіки.

## Команди для роботи

### Збірка та запуск
```bash
# Збірка всього solution
dotnet build

# Запуск консольного застосунку
dotnet run --project src/Cli

## Порівняння режимів публікації

```

| RID | Режим | Додаткові параметри | Розмір publish | Потрібен runtime |
| :--- | :--- | :--- | :--- | :--- |
| win-x64 | self-contained | немає | 71 МБ | ні |
| win-x64 | framework-dependent | немає | 0.2 МБ | так (.NET 10) |
| win-x64 | self-contained | `PublishSingleFile=true` | 70 МБ (1 файл) | ні |
| win-x64 | self-contained | `PublishTrimmed=true` | 19 МБ | ні |
|



## Лабораторна робота №10: Minimal API (CRUD, валідація, OpenAPI / Scalar)

### Запуск API
Зафіксований порт (також прописаний у `src/Api/Properties/launchSettings.json`): **`http://localhost:5080`**

```bash
dotnet run --project src/Api --urls http://localhost:5080