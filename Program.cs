using System.IO;
using Microsoft.OpenApi.Models;

var builder = WebApplication.CreateBuilder(args);

// Controllers
builder.Services.AddControllers();

// Swagger + OpenAPI（合併為一次設定）
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(c =>
{
    // 基本資訊
    c.SwaggerDoc("v1", new OpenApiInfo { Title = "Digital Learning API", Version = "v1" });

    // 讓 Swagger 顯示 XML 註解（請在 .csproj 開啟 <GenerateDocumentationFile>true</GenerateDocumentationFile>）
    var xml = $"{System.Reflection.Assembly.GetExecutingAssembly().GetName().Name}.xml";
    var path = Path.Combine(AppContext.BaseDirectory, xml);
    if (File.Exists(path))
    {
        c.IncludeXmlComments(path, includeControllerXmlComments: true);
    }

    // JWT Bearer 定義
    c.AddSecurityDefinition("bearerAuth", new OpenApiSecurityScheme
    {
        Type = SecuritySchemeType.Http,
        Scheme = "bearer",
        BearerFormat = "JWT",
        In = ParameterLocation.Header,
        Name = "Authorization",
        Description = "輸入格式：Bearer {你的JWT}"
    });

    c.AddSecurityRequirement(new OpenApiSecurityRequirement
    {
        {
            new OpenApiSecurityScheme
            {
                Reference = new OpenApiReference
                {
                    Type = ReferenceType.SecurityScheme,
                    Id = "bearerAuth"
                }
            },
            Array.Empty<string>()
        }
    });
});

// 🧩 若要用 JWT，別忘了加入驗證（此處只示意，請用你的實際參數）
/*
builder.Services
    .AddAuthentication("Bearer")
    .AddJwtBearer("Bearer", options =>
    {
        options.Authority = "https://your-identity-server";
        options.Audience = "learning-api";
    });
*/

var app = builder.Build();

// 只在開發環境開 Swagger UI（正式環境建議加保護或只對內部網開放）
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI(c =>
    {
        c.SwaggerEndpoint("/swagger/v1/swagger.json", "Digital Learning API v1");
        c.RoutePrefix = "docs"; // /docs 查看 Swagger UI
    });
}

app.UseHttpsRedirection();

// 若啟用 JWT 驗證，順序要正確：Authentication -> Authorization
// app.UseAuthentication();
app.UseAuthorization();

app.MapControllers();
app.Run();
