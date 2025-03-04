using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using MudBlazor.Services;
using GestApp.Components;
using GestApp.Infrastructure.Data;
using GestApp.Application.Configurations;
using GestApp.Application.Services.Interfaces;
using GestApp.Application.Services;
using GestApp.Client.Services.Interfaces;
using GestApp.Client.Services;
using MudBlazor;

var builder = WebApplication.CreateBuilder(args);

// Carica il file appsettings.json di default
builder.Configuration.AddJsonFile("appsettings.json", optional: false, reloadOnChange: true);

#if DEBUG
// Carica il file di configurazione locale, se esiste
builder.Configuration.AddJsonFile("local.appsettings.json", optional: true, reloadOnChange: true);
#endif

// Add HttpClient service
var baseUri = builder.Configuration["Settings:BaseUri"] ?? throw new InvalidOperationException("Base Uri not found.");
builder.Services.AddScoped<HttpClient>(sp =>
    new HttpClient { BaseAddress = new Uri(baseUri) });

// Add MudBlazor services
builder.Services.AddMudServices(config =>
{
    config.SnackbarConfiguration.PositionClass = Defaults.Classes.Position.TopRight;

    config.SnackbarConfiguration.PreventDuplicates = false;
    config.SnackbarConfiguration.NewestOnTop = false;
    config.SnackbarConfiguration.ShowCloseIcon = true;
    config.SnackbarConfiguration.VisibleStateDuration = 10000;
    config.SnackbarConfiguration.HideTransitionDuration = 500;
    config.SnackbarConfiguration.ShowTransitionDuration = 500;
    config.SnackbarConfiguration.SnackbarVariant = Variant.Filled;
});

// Add services to the container.
builder.Services.AddRazorComponents()
    .AddInteractiveServerComponents()
    .AddInteractiveWebAssemblyComponents()
    .AddAuthenticationStateSerialization(
        options => options.SerializeAllClaims = true);
builder.Services.AddCascadingAuthenticationState();

// Add AutoMapper service
builder.Services.AddAutoMapper(typeof(MappingProfile));

// Add custom services
builder.Services.AddScoped<ITransactionService, TransactionService>();
builder.Services.AddScoped<IBreadcrumbService, BreadcrumbService>();
builder.Services.AddScoped<IAuthenticationService, AuthenticationService>();
builder.Services.AddScoped<IUserService, UserService>();
//builder.Services.AddScoped<ITransactionService, TransactionService>();
//builder.Services.AddScoped<IUserService, UserService>();

// Add Options binding
builder.Services.Configure<Settings>(builder.Configuration.GetSection("Settings"));

// Configura l’autenticazione tramite cookie (Identity)
builder.Services.AddAuthentication(options =>
{
    options.DefaultAuthenticateScheme = IdentityConstants.ApplicationScheme;
    options.DefaultChallengeScheme = IdentityConstants.ApplicationScheme;
    options.DefaultSignInScheme = IdentityConstants.ApplicationScheme;
})
.AddCookie(IdentityConstants.ApplicationScheme, options =>
{
    options.LoginPath = "/Login";
    options.AccessDeniedPath = "/Access-Denied";
})
.AddCookie(IdentityConstants.TwoFactorUserIdScheme, options =>
{
    // Imposta il nome del cookie (facoltativo, ma consigliato)
    options.Cookie.Name = IdentityConstants.TwoFactorUserIdScheme;
});

// Aggiungi i controller (API)
builder.Services.AddControllers();

var connectionString = builder.Configuration.GetConnectionString("DefaultConnection") ?? throw new InvalidOperationException("Connection string 'DefaultConnection' not found.");
builder.Services.AddDbContext<ApplicationDbContext>(options =>
    options.UseMySql(connectionString, ServerVersion.AutoDetect(connectionString)));
builder.Services.AddDatabaseDeveloperPageExceptionFilter();

// Configura Identity
builder.Services.AddIdentityCore<ApplicationUser>(options => options.SignIn.RequireConfirmedAccount = false)
    .AddRoles<IdentityRole>()
    .AddEntityFrameworkStores<ApplicationDbContext>()
    .AddSignInManager()
    .AddUserManager<UserManager<ApplicationUser>>()
    .AddRoleManager<RoleManager<IdentityRole>>()
    .AddDefaultTokenProviders();

//builder.Services.AddSingleton<IEmailSender<ApplicationUser>, IdentityNoOpEmailSender>();

var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseWebAssemblyDebugging();
    app.UseMigrationsEndPoint();
}
else
{
    app.UseExceptionHandler("/Error", createScopeForErrors: true);
    // The default HSTS value is 30 days. You may want to change this for production scenarios, see https://aka.ms/aspnetcore-hsts.
    app.UseHsts();
}

app.UseHttpsRedirection();

app.UseAuthentication();
app.UseAuthorization();
// Applica l’antiforgery solo per le richieste non API (se necessario)
app.UseWhen(context => !context.Request.Path.StartsWithSegments("/api"), appBuilder =>
{
    appBuilder.UseAntiforgery();
});

app.MapControllers();

app.MapStaticAssets();
app.MapRazorComponents<App>()
    .AddInteractiveServerRenderMode()
    .AddInteractiveWebAssemblyRenderMode()
    .AddAdditionalAssemblies(typeof(GestApp.Client._Imports).Assembly);

#if DEBUG
using (var scope = app.Services.CreateScope())
{
    var services = scope.ServiceProvider;
    await SeedData.SeedRolesAndUser(services);
}
#endif

app.Run();
