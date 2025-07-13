using Imperium.Data.Extensions;
using Imperium.Service.Mapping;
using Imperium.Service.Services.Admin;
using Imperium.Service.Services.Cart;
using Imperium.Service.Services.Client;
using Imperium.Service.Services.Cloudinary;
using Imperium.Service.Services.CustomLog;
using Imperium.Service.Services.Dictionary;
using Imperium.Service.Services.Favorite;
using Imperium.Service.Services.Order;
using Imperium.Service.Services.Product;
using Imperium.Service.Services.Review;
using Imperium.Service.Services.Telegram;
using Imperium.Service.Services.Validation;
using Imperium.Service.Services.WhatsApp;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.IdentityModel.Tokens;
using System.Text;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

// Data Layer
builder.Services.AddDataLayer();

// AutoMapper
builder.Services.AddAutoMapper(typeof(MappingProfile));

// Memory Cache for registration sessions
builder.Services.AddMemoryCache();

// Services
builder.Services.AddScoped<IAdminUserService, AdminUserService>();
builder.Services.AddScoped<ICartService, CartService>();
builder.Services.AddScoped<IClientService, ClientService>();
builder.Services.AddScoped<ICloudinaryService, CloudinaryService>();
builder.Services.AddScoped<ICustomLogService, CustomLogService>();
builder.Services.AddScoped<IDictionaryService, DictionaryService>();
builder.Services.AddScoped<IFavoriteService, FavoriteService>();
builder.Services.AddScoped<IOrderService, OrderService>();
builder.Services.AddScoped<IProductService, ProductService>();
builder.Services.AddScoped<IReviewService, ReviewService>();
builder.Services.AddScoped<ITelegramService, TelegramService>();
builder.Services.AddScoped<IPhoneValidationService, PhoneValidationService>();
builder.Services.AddScoped<IWhatsAppService, WhatsAppService>();


// HTTP Client
builder.Services.AddHttpClient();

// JWT Authentication
var jwtSettings = builder.Configuration.GetSection("JwtSettings");
var key = Encoding.ASCII.GetBytes(jwtSettings["Secret"]!);

builder.Services.AddAuthentication(x =>
{
    x.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
    x.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
})
.AddJwtBearer(x =>
{
    x.RequireHttpsMetadata = false;
    x.SaveToken = true;
    x.TokenValidationParameters = new TokenValidationParameters
    {
        ValidateIssuerSigningKey = true,
        IssuerSigningKey = new SymmetricSecurityKey(key),
        ValidateIssuer = true,
        ValidateAudience = true,
        ValidIssuer = jwtSettings["Issuer"],
        ValidAudience = jwtSettings["Audience"],
        ClockSkew = TimeSpan.Zero
    };
});

// CORS
builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowAll", policy =>
    {
        policy.AllowAnyOrigin()
              .AllowAnyMethod()
              .AllowAnyHeader();
    });
});

// Logging
builder.Logging.ClearProviders();
builder.Logging.AddConsole();
builder.Logging.AddDebug();

var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseHttpsRedirection();
app.UseCors("AllowAll");
app.UseAuthentication();
app.UseAuthorization();
app.MapControllers();

app.Run();