using CSLTesting.Client.Components;
using Microsoft.AspNetCore.Components.Web;

#region CSL Test Loading
CSL.DependencyInjection.NpgsqlConnectionConstructor       = (x) => new Npgsql.NpgsqlConnection(x);
CSL.DependencyInjection.NpgsqlConnectionStringConstructor = () => new Npgsql.NpgsqlConnectionStringBuilder();
CSL.DependencyInjection.SslModeConverter                  = (x) => Enum.Parse(typeof(Npgsql.SslMode), x.ToString());

CSL.DependencyInjection.SqliteConnectionConstructor       = (x) => new Microsoft.Data.Sqlite.SqliteConnection(x);
CSL.DependencyInjection.SqliteConnectionStringConstructor = () => new Microsoft.Data.Sqlite.SqliteConnectionStringBuilder();
CSL.DependencyInjection.SqliteOpenModeConverter           = (x) => (Microsoft.Data.Sqlite.SqliteOpenMode)x;
CSL.DependencyInjection.SqliteCacheModeConverter          = (x) => (Microsoft.Data.Sqlite.SqliteCacheMode)x;

CSL.DependencyInjection.AesGcmConstructor = (x) => new System.Security.Cryptography.AesGcm(x);

CSLTesting.SQLTests.GetTestDB.Add(() => CSL.SQL.PostgreSQL.Connect("localhost", "testdb", "testuser", "testpassword", "testschema"));
CSLTesting.SQLTests.GetTestDB.Add(() => CSL.SQL.PostgreSQL.Connect("localhost:5432", "testdb", "testuser", "testpassword", "testschema"));
CSLTesting.SQLTests.GetTestDB.Add(async () =>
{
    Npgsql.NpgsqlConnection connection = new Npgsql.NpgsqlConnection("Host=localhost;Database=testdb;Username=testuser;Password=testpassword;SSL Mode=Prefer;Trust Server Certificate=False");
    CSL.SQL.PostgreSQL      sql        = new CSL.SQL.PostgreSQL(connection);
    await sql.SetSchema("testschema");
    return sql;
});
CSLTesting.SQLTests.GetSqliteDB.Add(() => new CSL.SQL.Sqlite(":memory:"));

#endregion

WebApplicationBuilder builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.Services.AddControllers();
builder.Services.AddRazorComponents()
       .AddInteractiveWebAssemblyComponents();

WebApplication app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseWebAssemblyDebugging();
}
else
{
    app.UseExceptionHandler("/Error", createScopeForErrors: true);
    // The default HSTS value is 30 days. You may want to change this for production scenarios, see https://aka.ms/aspnetcore-hsts.
    app.UseHsts();
}

app.UseStaticFiles();
app.UseAntiforgery();
app.MapControllers();
app.MapRazorComponents<App>()
   .AddInteractiveWebAssemblyRenderMode()
   .AddAdditionalAssemblies(typeof(CSLTestingClient._Imports).Assembly);

app.Run();