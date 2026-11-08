using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using Microsoft.OpenApi.Models;
using StoreAPI.Data;
using StoreAPI.Models;
using StoreAPI.Services;
using System.Text;

var builder = WebApplication.CreateBuilder(args);

// ==========================================
// 1. تنظیمات کنترلرها (Controllers)
// ==========================================
builder.Services.AddControllers()
    .AddJsonOptions(options =>
    {
        // این خط برای جلوگیری از خطای حلقه بی‌نهایت (Circular Reference) در Json است.
        // مثلاً وقتی یک محصول به دسته‌بندی وصل است و دسته‌بندی به لیست محصولات، سریالایز کردن آن‌ها بدون این خط خطا می‌دهد.
        options.JsonSerializerOptions.ReferenceHandler =
            System.Text.Json.Serialization.ReferenceHandler.IgnoreCycles;
    });

builder.Services.AddEndpointsApiExplorer();

// ==========================================
// 2. تنظیمات Swagger (برای تست APIها)
// ==========================================
builder.Services.AddSwaggerGen(options =>
{
    options.SwaggerDoc("v1", new OpenApiInfo { Title = "StoreAPI", Version = "v1" });

    // این بخش به Swagger می‌گوید چطور با توکن JWT کار کند.
    // یک دکمه "Authorize" بالای صفحه اضافه می‌کند تا بتوانی توکن را آنجا وارد کنی.
    options.AddSecurityDefinition("Bearer", new OpenApiSecurityScheme
    {
        Name = "Authorization",                           // نام هدر
        Type = SecuritySchemeType.Http,                   // نوع: HTTP
        Scheme = "bearer",                                // طرح: Bearer
        BearerFormat = "JWT",                             // فرمت: JWT
        In = ParameterLocation.Header,                    // محل: هدر درخواست
        Description = "توکن JWT را وارد کنید. مثال: eyJhbGciOiJIUzI1NiIs..."
    });

    // این بخش می‌گوید که همه‌ی Endpointها (به جز آنهایی که آزاد هستند) نیاز به این Security Scheme دارند.
    options.AddSecurityRequirement(new OpenApiSecurityRequirement
    {
        {
            new OpenApiSecurityScheme
            {
                Reference = new OpenApiReference
                {
                    Type = ReferenceType.SecurityScheme,
                    Id = "Bearer"
                }
            },
            new string[] { }
        }
    });
});

// ==========================================
// 3. اتصال به دیتابیس (Entity Framework)
// ==========================================
// Connection String را از فایل appsettings.json می‌خواند.
builder.Services.AddDbContext<AppDbContext>(option =>
{
    option.UseSqlServer(builder.Configuration.GetConnectionString("Default"));
}, ServiceLifetime.Transient);

// ==========================================
// 4. تنظیمات ASP.NET Identity (مدیریت کاربران)
// ==========================================
builder.Services.AddIdentity<IdentityUser, IdentityRole>()
    .AddEntityFrameworkStores<AppDbContext>() // اطلاعات کاربران در AppDbContext ذخیره می‌شود
    .AddDefaultTokenProviders();              // برای بازیابی رمز عبور و تأیید ایمیل

// ==========================================
// 5. ثبت سرویس JWT (سرویس اختصاصی خودمان)
// ==========================================
// به سیستم می‌گوییم هر جا JwtService خواسته شد، یک نمونه از آن بساز (Scoped = یک نمونه برای هر درخواست).
builder.Services.AddScoped<JwtService>();

// ==========================================
// 6. تنظیمات احراز هویت و اعتبارسنجی JWT (Authentication)
// ==========================================
// خواندن کلیدها از appsettings.json
var jwtKey = builder.Configuration["Jwt:Key"];
var jwtIssuer = builder.Configuration["Jwt:Issuer"];
var jwtAudience = builder.Configuration["Jwt:Audience"];

 
builder.Services.AddMediatR(cfg => cfg.RegisterServicesFromAssembly(typeof(Program).Assembly));


builder.Services.AddAuthentication(options =>
{
    // می‌گوییم که پیش‌فرض احراز هویت در سیستم، JWT باشد (نه کوکی)
    options.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
    options.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
})
.AddJwtBearer(options =>
{
    // تنظیمات مربوط به اعتبارسنجی توکن دریافتی
    options.TokenValidationParameters = new TokenValidationParameters
    {
        ValidateIssuer = true,             // آیا Issuer توکن درست است؟
        ValidateAudience = true,           // آیا Audience توکن درست است؟
        ValidateLifetime = true,           // آیا توکن منقضی نشده است؟
        ValidateIssuerSigningKey = true,   // آیا امضای توکن با کلید ما تطابق دارد؟
        ValidIssuer = jwtIssuer,           // Issuer معتبر ما
        ValidAudience = jwtAudience,       // Audience معتبر ما
        IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(jwtKey)) // کلید مخفی
    };
});

// ==========================================
// 7. مجوزدهی (Authorization)
// ==========================================
// این خط به سیستم اجازه می‌دهد از اتریبیوت [Authorize] استفاده کند.
builder.Services.AddAuthorization();

var app = builder.Build();

// ==========================================
// 8. خط لوله پردازش درخواست (Middleware Pipeline)
// ==========================================
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseHttpsRedirection();

// این دو خط بسیار مهم هستند و ترتیبشان اجباری است:
// 1. UseAuthentication: بررسی می‌کند آیا کاربر توکن معتبر فرستاده است؟ (هویت را مشخص می‌کند)
// 2. UseAuthorization: بررسی می‌کند آیا کاربر احراز هویت شده اجازه دسترسی به این Endpoint را دارد؟ (مجوز را چک می‌کند)
app.UseAuthentication();
app.UseAuthorization();

app.MapControllers();

// ==========================================
// 9. اجرای Migration خودکار و دیتای نمونه (Seed Data)
// ==========================================
using (var scope = app.Services.CreateScope())
{
    var dbContext = scope.ServiceProvider.GetRequiredService<AppDbContext>();

    // هر بار که برنامه اجرا می‌شود، مایگریشن‌ها را روی دیتابیس اعمال می‌کند (برای توسعه عالی است)
    dbContext.Database.Migrate();

    // اگر جدول محصولات خالی بود، چند محصول نمونه اضافه می‌کند تا تست راحت‌تر باشد
    if (!dbContext.Products.Any())
    {
        dbContext.Products.AddRange(
            new Product { Name = "Laptop", Price = 1200, StockQuantity = 10 },
            new Product { Name = "Mouse", Price = 25, StockQuantity = 50 },
            new Product { Name = "Keyboard", Price = 75, StockQuantity = 30 }
        );
        dbContext.SaveChanges();
    }
}

app.Run();