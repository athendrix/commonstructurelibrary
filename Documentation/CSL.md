# Common Structure Library
The Common Structure Library consists of several interconnected namespaces designed to make a variety of applications easier to create without needing to start from scratch each time.

## Dependency Injection
Integrated dependencies are kept minimal with only a few official Microsoft Packages and Newtonsoft.Json the most popular package on nuget at this time. All other dependencies support a simple form of dependency injection.

If dependency injection is required for certain classes, the code neccessary to inject it is included in the CSL.DependencyMissingException error message itself.

Some examples include:
### AesGcm (Only available in newer versions of .NET)
```cs
CSL.DependencyInjection.AesGcmConstructor = (x) => new System.Security.Cryptography.AesGcm(x);
```

### Npgsql (Npgsql Nuget Package)
```cs
CSL.DependencyInjection.NpgsqlConnectionConstructor = (x) => new Npgsql.NpgsqlConnection(x);
CSL.DependencyInjection.NpgsqlConnectionStringConstructor = () => new Npgsql.NpgsqlConnectionStringBuilder();
CSL.DependencyInjection.SslModeConverter = (x) => (Npgsql.SslMode)x;
```

### Sqlite (Microsoft.Data.Sqlite Nuget Package)
```cs
CSL.DependencyInjection.SqliteConnectionConstructor = (x) => new Microsoft.Data.Sqlite.SqliteConnection(x);
CSL.DependencyInjection.SqliteConnectionStringConstructor = () => new Microsoft.Data.Sqlite.SqliteConnectionStringBuilder();
CSL.DependencyInjection.SqliteOpenModeConverter = (x) => (Microsoft.Data.Sqlite.SqliteOpenMode)x;
CSL.DependencyInjection.SqliteCacheModeConverter = (x) => (Microsoft.Data.Sqlite.SqliteCacheMode)x;
```

These can just be dropped in at the beginning of your main method to provide the necessary functionality if you're using those parts of the CSL.