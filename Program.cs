using Microsoft.EntityFrameworkCore;
using SistemaTickets.Data;
using System.IO;

var builder = WebApplication.CreateBuilder(args);

// Cargar variables de entorno desde el archivo .env si existe
var root = Directory.GetCurrentDirectory();
var dotenv = Path.Combine(root, ".env");
if (File.Exists(dotenv))
{
    foreach (var line in File.ReadAllLines(dotenv))
    {
        if (string.IsNullOrWhiteSpace(line) || line.StartsWith("#")) continue;
        var parts = line.Split('=', 2, StringSplitOptions.RemoveEmptyEntries);
        if (parts.Length == 2)
        {
            Environment.SetEnvironmentVariable(parts[0].Trim(), parts[1].Trim(' ', '"'));
        }
    }
}
builder.Configuration.AddEnvironmentVariables();

// Add services to the container.
builder.Services.AddControllersWithViews();
builder.Services.AddDistributedMemoryCache();
builder.Services.AddSession(options =>
{
    options.IdleTimeout = TimeSpan.FromMinutes(30);
    options.Cookie.HttpOnly = true;
    options.Cookie.IsEssential = true;
});

var connectionLocal = builder.Configuration.GetConnectionString("ConexionSQL"); // Lee ConnectionStrings__ConexionSQL
var connectionRemote = builder.Configuration["Connection:ConexionSQL"];        // Lee Connection__ConexionSQL

// Registro del contexto Local (Base de datos 1)
builder.Services.AddDbContext<AppDbContext>(options =>
    options.UseSqlServer(connectionLocal));

// Registro del contexto Remoto (Base de datos 2)
if (!string.IsNullOrWhiteSpace(connectionRemote))
{
    builder.Services.AddDbContext<RemoteDbContext>(options =>
        options.UseSqlServer(connectionRemote));
}

var app = builder.Build();

// Probar la conexión a la base de datos al iniciar la aplicación
using (var scope = app.Services.CreateScope())
{
    var services = scope.ServiceProvider;
    try
    {
        var context = services.GetRequiredService<AppDbContext>();
        if (context.Database.CanConnect())
        {
            Console.WriteLine("✅ Conexión a la base de datos SQL Server establecida con éxito.");
        }
    }
    catch (Exception ex)
    {
        Console.WriteLine($"❌ Error crítico al conectar a la base de datos: {ex.Message}");
    }
}

// Configure the HTTP request pipeline.
if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Home/Error");
    // The default HSTS value is 30 days. You may want to change this for production scenarios, see https://aka.ms/aspnetcore-hsts.
    app.UseHsts();
}

app.UseHttpsRedirection();
app.UseRouting();

app.UseSession();
app.UseAuthorization();

app.MapStaticAssets();

app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Home}/{action=Index}/{id?}")
    .WithStaticAssets();


app.Run();
