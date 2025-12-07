using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.Tokens;
using Microsoft.OpenApi.Models;
using System.Security.Claims;
using System.Text;
using Taskify.DataStore;
using Taskify.Domain.Entities;
using Taskify.Services;
using Taskify.Services.DTOs;
using Taskify.Services.Implementation;
using Taskify.Services.Interface;
using Taskify.Services.Utilities;

var builder = WebApplication.CreateBuilder(args);

// Bind strongly typed settings
builder.Services.Configure<JwtConfig>(builder.Configuration.GetSection(nameof(JwtConfig)));
builder.Services.Configure<AiConfig>(builder.Configuration.GetSection(nameof(AiConfig)));
builder.Services.Configure<CloudinarySettings>(builder.Configuration.GetSection(nameof(CloudinarySettings)));

// Register custom services
builder.Services.ConfigService(builder.Configuration);

// Controllers + custom validation behavior
builder.Services.AddControllers()
    .ConfigureApiBehaviorOptions(opt =>
    {
        var factory = opt.InvalidModelStateResponseFactory;
        opt.InvalidModelStateResponseFactory = (context) =>
        {
            if (!context.ModelState.IsValid)
            {
                var resp = new ApiResponse<object>
                {
                    Message = "Validation Error",
                    StatusCode = StatusCodes.Status400BadRequest,
                    Errors = context.ModelState.Select(x => new ValidationError
                    {
                        Property = x.Key,
                        Error = x.Value.Errors.Select(e => e.ErrorMessage)
                    })
                };
                return new BadRequestObjectResult(resp);
            }
            return factory(context);
        };
    });

// Swagger
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(c =>
{
    c.SwaggerDoc("v1", new OpenApiInfo { Title = "Taskify API", Version = "v1" });

    var securityScheme = new OpenApiSecurityScheme
    {
        Name = "JWT Authentication",
        Description = "Enter JWT Bearer token",
        In = ParameterLocation.Header,
        Type = SecuritySchemeType.Http,
        Scheme = "bearer",
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

builder.Services.AddHttpClient();

// CORS
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

// JWT AUTHENTICATION
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

    opt.TokenValidationParameters = new TokenValidationParameters
    {
        ValidIssuer = config.Issuer,
        ValidAudience = config.Audiences,
        ValidateIssuer = true,
        ValidateAudience = true,
        ValidateLifetime = true,
        IssuerSigningKey = new SymmetricSecurityKey(
            Encoding.UTF8.GetBytes(config.SigningKey)
        )
    };

    // SECURITY STAMP VALIDATION
    opt.Events = new JwtBearerEvents
    {
        OnTokenValidated = async context =>
        {
            var claims = context.Principal?.Claims;
            var secStampClaim = claims?.FirstOrDefault(c => c.Type == "secStamp")?.Value;
            var userId = claims?.FirstOrDefault(c => c.Type == ClaimTypes.NameIdentifier)?.Value;

            if (!string.IsNullOrEmpty(secStampClaim) && !string.IsNullOrEmpty(userId))
            {
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
                    context.Fail("Token security stamp is invalid");
                    return;
                }
            }
        }
    };
});

var app = builder.Build();

// Swagger
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

// Middleware order
app.UseCors("frontend");
app.UseAuthentication();
app.UseAuthorization();

app.MapControllers();

// Log all endpoints
var endpointSource = app.Services.GetRequiredService<Microsoft.AspNetCore.Routing.EndpointDataSource>();
var logger = app.Services.GetRequiredService<ILoggerFactory>().CreateLogger("Startup");

foreach (var ep in endpointSource.Endpoints)
{
    logger.LogInformation("Mapped endpoint: {Endpoint}", ep.DisplayName);
}

// Seed database
await DataInitializer.SeedDatabase(app.Services);

app.Run();
