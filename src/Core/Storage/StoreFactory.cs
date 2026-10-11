using Core.Abstractions;

namespace Core.Storage;

public static class StoreFactory
{
    public static ICatalogStore Create(string[] args, string dataPath)
    {
        bool useFile = args.Contains("--file");
        bool useCache = args.Contains("--cache");

        ICatalogStore baseStore = useFile
            ? new FileCatalogStore(dataPath)
            : new InMemoryCatalogStore(SampleData.Products());

        // Якщо передано прапорець --cache, огортаємо сховище в декоратор
        return useCache
            ? new CachingCatalogStore(baseStore)
            : baseStore;
    }
}