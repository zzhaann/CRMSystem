using CRMSystem.Data;
using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Authentication.Cookies;
using CRMSystem.Models;
using Microsoft.AspNetCore.Identity;
using Serilog;
using Serilog.Context;
using Microsoft.AspNetCore.Localization;
using System.Globalization;
using Microsoft.Extensions.Options;
using CRMSystem.Services;
using CRMSystem.Hubs;
using System.Text.Json.Serialization;
using CRMSystem.Filters;
using CRMSystem.AppMiddleware;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.IdentityModel.Tokens;
using System.Text;

//string connectionstring=builder.Configuration.GetConnectionString("DefaultConnection");
var builder = WebApplication.CreateBuilder(args);

builder.Services.AddDbContext<ApplicationDbContext>(options =>
    options.UseSqlServer(builder.Configuration.GetConnectionString("DefaultConnection"))); //connectionstring 

builder.Services.AddIdentity<ApplicationUser, IdentityRole>()
    .AddEntityFrameworkStores<ApplicationDbContext>()
    .AddDefaultTokenProviders();

builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowAll", builder =>
    {
        builder.AllowAnyOrigin()
               .AllowAnyMethod()
               .AllowAnyHeader();
    });
});


builder.Services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
    .AddJwtBearer(options =>
    {
        options.TokenValidationParameters = new TokenValidationParameters
        {
            ValidateIssuer = true,
            ValidateAudience = true,
            ValidIssuer = builder.Configuration["Jwt:Issuer"],
            ValidAudience = builder.Configuration["Jwt:Audience"],
            IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(builder.Configuration["Jwt:Key"]))
        };
    });



builder.Services.AddAuthentication(CookieAuthenticationDefaults.AuthenticationScheme)
       .AddCookie(options =>
       {
           options.LoginPath = "/Account/Login";
           options.LogoutPath = "/Account/Logout";
       });


builder.Services.AddScoped<BrowserCheckResourceFilter>();
builder.Services.AddScoped<GlobalExceptionFilter>();
builder.Services.AddScoped<LoggingActionFilter>();
builder.Services.AddScoped<CustomResultFilter>();
builder.Services.AddScoped<TokenService>();



builder.Services.AddControllersWithViews(options =>
{
    options.Filters.Add<BrowserCheckResourceFilter>(); 
})
    .AddViewLocalization()
    .AddDataAnnotationsLocalization()
    .AddJsonOptions(options =>
 {
     options.JsonSerializerOptions.PropertyNamingPolicy = System.Text.Json.JsonNamingPolicy.CamelCase;
     options.JsonSerializerOptions.DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull;
 });




//Integration with GreenApi
builder.Services.AddSignalR();
builder.Services.AddSingleton<GreenApiService>();
builder.Services.AddSingleton<MessageStore>();



// Локализация через файлы ресурсов
builder.Services.AddLocalization(options => options.ResourcesPath = "Resources");

builder.Services.Configure<RequestLocalizationOptions>(options =>
{
    var supportedCultures = new[]
    {
        new CultureInfo("en-US"),
        new CultureInfo("ru-RU"),
        new CultureInfo("kk-KZ")
    };

    options.DefaultRequestCulture = new RequestCulture(culture: "kk-KZ", uiCulture: "kk-KZ");
    options.SupportedCultures = supportedCultures;
    options.SupportedUICultures = supportedCultures;
});

Log.Logger = new LoggerConfiguration()
    .WriteTo.Console()
    .WriteTo.Seq("http://localhost:5341")
    .MinimumLevel.Debug()
    .Enrich.FromLogContext()
    .CreateLogger();

builder.Host.UseSerilog();

var app = builder.Build();

app.UseCors("AllowAllOrigins");


//app.UseWorkTime();      
app.UseLogRequest();



// Configure the HTTP request pipeline.
if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Home/Error");
    // The default HSTS value is 30 days. You may want to change this for production scenarios, see https://aka.ms/aspnetcore-hsts.
    app.UseHsts();
}

app.UseHttpsRedirection();
app.UseStaticFiles();

app.UseRouting();

app.UseJwtTokenMiddleware();

app.UseAuthentication();
app.UseAuthorization();




var localizationOptions = app.Services.GetService<IOptions<RequestLocalizationOptions>>().Value;
app.UseRequestLocalization(localizationOptions);

app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Home}/{action=Index}/{id?}");

app.MapHub<ChatHub>("/chatHub");

app.Run();
