using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.IdentityModel.Tokens;
using Microsoft.OpenApi.Models;
using System.Text;
using Taskify.DataStore;
using Taskify.Services;
using Taskify.Services.Utilities;
using Microsoft.AspNetCore.Identity;
using Taskify.Domain.Entities; // added
using Taskify.Services.Interface;
using Taskify.Services.Implementation;
using Microsoft.Extensions.Options;
using Microsoft.Extensions.AI; // Add this line

var builder = WebApplication.CreateBuilder(args);
// Add services to the container.
builder.Services.Configure<JwtConfig>(builder.Configuration.GetSection(nameof(JwtConfig)));
builder.Services.Configure<AiConfig>(builder.Configuration.GetSection(nameof(AiConfig)));
builder.Services.Configure<CloudinarySettings>(builder.Configuration.GetSection(nameof(CloudinarySettings)));
builder.Services.ConfigService(builder.Configuration);
builder.Services.AddControllers();
// Learn more about configuring OpenAPI at https://aka.ms/aspnet/openapi
builder.Services.AddOpenApi();
builder.Services.AddSwaggerGen();
builder.Services.AddHttpClient();

// Register AI service and configure HttpClient for OpenAI-compatible API
//builder.Services.Configure<AiConfig>(builder.Configuration.GetSection("AiConfig"));
//builder.Services.ConfigService(builder.Configuration);

builder.Services.AddCors(o =>
{
    o.AddPolicy("frontend", op =>
    {
        op.WithOrigins("http://localhost:8080")
        .AllowAnyHeader()
        .AllowAnyMethod()
        .AllowCredentials();

    });
});

builder.Services.AddAuthentication(options =>
{
    options.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
    options.DefaultScheme = JwtBearerDefaults.AuthenticationScheme;
    options.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
})
    .AddJwtBearer(opt =>
    {
        var config = new JwtConfig();
        builder.Configuration.GetSection(nameof(JwtConfig)).Bind(config);
        opt.TokenValidationParameters = new Microsoft.IdentityModel.Tokens.TokenValidationParameters
        {
            ValidIssuer = config.Issuer,
            ValidAudience = config.Audiences,
            ValidateIssuer = true,
            ValidateAudience = true,
            ValidateLifetime = true,
            ValidateIssuerSigningKey = true,
            IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(config.SigningKey)),
            ClockSkew = TimeSpan.Zero
        };
        // Validate security stamp claim to support immediate token invalidation on logout
        opt.Events = new JwtBearerEvents
        {
            OnTokenValidated = async context =>
            {
                var claims = context.Principal?.Claims;
                var secStampClaim = claims?.FirstOrDefault(c => c.Type == "secStamp")?.Value;
                var userId = claims?.FirstOrDefault(c => c.Type == System.Security.Claims.ClaimTypes.NameIdentifier)?.Value;
                if (!string.IsNullOrEmpty(secStampClaim) && !string.IsNullOrEmpty(userId))
                {
                    // resolve UserManager from DI to check current security stamp
                    var userManager = context.HttpContext.RequestServices.GetRequiredService<UserManager<AppUser>>();
                    var user = await userManager.FindByIdAsync(userId);
                    if (user == null)
                    {
                        context.Fail("User not found");
                        return;
                    }
                    var currentStamp = await userManager.GetSecurityStampAsync(user);
                    if (!string.Equals(currentStamp, secStampClaim, StringComparison.Ordinal))
                    {
                        // security stamp mismatch -> token invalid
                        context.Fail("Token security stamp is invalid");
                        return;
                    }
                }
            }
        };
    });

builder.Services.AddSwaggerGen(c =>
{
    c.SwaggerDoc("v1", new OpenApiInfo { Title = "Your API", Version = "v1" });

    // Add JWT Authentication support in Swagger UI
    var securityScheme = new OpenApiSecurityScheme
    {
        Name = "JWT Authentication",
        Description = "Enter JWT Bearer token",
        In = ParameterLocation.Header,
        Type = SecuritySchemeType.Http,
        Scheme = "bearer", // must be lowercase
        BearerFormat = "JWT",
        Reference = new OpenApiReference
        {
            Id = JwtBearerDefaults.AuthenticationScheme,
            Type = ReferenceType.SecurityScheme
        }
    };

    c.AddSecurityDefinition(securityScheme.Reference.Id, securityScheme);

    c.AddSecurityRequirement(new OpenApiSecurityRequirement
    {
        { securityScheme, Array.Empty<string>() }
    });
});

var app = builder.Build();


// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
    app.MapOpenApi();
}

app.UseHttpsRedirection();
app.UseCors("frontend");

app.UseAuthentication();
app.UseAuthorization();

app.MapControllers();
await DataInitializer.SeedDatabase(app.Services);

app.Run();
