# Inventory Ingestion

## References

* [Setting up Serilog in Azure functions](https://stackoverflow.com/questions/71034036/how-to-setup-serilog-with-azure-functions-v4-correctly)

* [Making internal classes available to test](https://stackoverflow.com/questions/358196/c-sharp-internal-access-modifier-when-doing-unit-testing)

* Using `use` in LanguageExt with nested disposable resources

**_Decided not to use this, because it's not be the best readable code_**

```csharp
    private static Aff<List<TData>> GetRecords<TData, TDataMap>(BlobClient client)
        where TDataMap : ClassMap<TData> =>
        (
            from stream in use(
                new MemoryStream(),
                async st =>
                {
                    await client.DownloadToAsync(st);
                    st.Position = 0;
                    return st;
                }
            )
            let items = use(
                new StreamReader(stream),
                sr =>
                    use(
                        () =>
                        {
                            var csvReader = new CsvReader(sr, CultureInfo.InvariantCulture);
                            csvReader.Context.RegisterClassMap<TDataMap>();
                            return csvReader;
                        },
                        csvr => csvr.GetRecords<TData>().ToList()
                    )
            )
            select items
        ).ToAff();
```