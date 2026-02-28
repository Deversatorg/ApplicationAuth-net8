using ApplicationAuth.Common.Constants;
using ApplicationAuth.Common.Exceptions;
using ApplicationAuth.Common.Extensions;
using ApplicationAuth.Features.Account.Shared;
using ApplicationAuth.Common.Utilities;
using ApplicationAuth.Common.Utilities.Interfaces;
using ApplicationAuth.DAL;
using ApplicationAuth.DAL.Abstract;
using ApplicationAuth.Domain.Entities.Identity;
using ApplicationAuth.SharedModels.ResponseModels;
using ApplicationAuth.ResourceLibrary;

using ApplicationAuth.Features.Account.Login;
using ApplicationAuth.Features.Account.Register;
using ApplicationAuth.Features.Account.AdminLogin;
using ApplicationAuth.Features.Account.RefreshToken;
using ApplicationAuth.Features.Account.Logout;
using ApplicationAuth.Features.Account.VerifyEmail;
using ApplicationAuth.Features.Account.SocialAuth;
using ApplicationAuth.Features.Account.PasswordRecovery;
using ApplicationAuth.Features.AdminUsers.GetAll;
using ApplicationAuth.Features.AdminUsers.Delete;
using ApplicationAuth.Features.Admins;
using ApplicationAuth.Features.Telegram;
using ApplicationAuth.Features.Test;
using Asp.Versioning;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.CookiePolicy;
using Microsoft.Extensions.Diagnostics.HealthChecks;
using Microsoft.AspNetCore.RateLimiting;
using System.Threading.RateLimiting;
using FluentValidation;
using ApplicationAuth.Helpers.Behaviors;
using Microsoft.AspNetCore.Diagnostics;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.HttpOverrides;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Localization;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Localization;
using Microsoft.Extensions.Logging;
using Microsoft.IdentityModel.Tokens;
using Microsoft.OpenApi.Models;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Net;
using System.Text.Json;
using System.Text.Json.Serialization;
using Wangkanai.Detection;
using Serilog;
using Serilog.Formatting.Compact;

Log.Logger = new LoggerConfiguration()
    .WriteTo.Console(new CompactJsonFormatter())
    .CreateBootstrapLogger();

try
{
    var builder = WebApplication.CreateBuilder(args);

    // --- Configuration ---
    var configuration = builder.Configuration;

    // --- Logging ---
    builder.Services.AddSerilog((services, lc) => lc
        .ReadFrom.Configuration(configuration)
        .ReadFrom.Services(services)
        .Enrich.FromLogContext()
        .WriteTo.Console(new CompactJsonFormatter()));

// --- Database ---
builder.Services.AddSingleton<ApplicationAuth.DAL.Interceptors.AuditableEntityInterceptor>();
builder.Services.AddDbContext<DataContext>((sp, options) =>
{
    var interceptor = sp.GetRequiredService<ApplicationAuth.DAL.Interceptors.AuditableEntityInterceptor>();
    options.UseSqlite(configuration.GetConnectionString("Connection"))
           .AddInterceptors(interceptor);
    options.EnableSensitiveDataLogging(builder.Environment.IsDevelopment());
});

// --- Identity ---
builder.Services.AddIdentity<ApplicationUser, ApplicationRole>(options =>
{
    options.Password.RequireDigit = true;
    options.Password.RequiredLength = 6;
    options.Password.RequireNonAlphanumeric = false;
    options.Password.RequireUppercase = false;
    options.Password.RequireLowercase = false;
    options.User.AllowedUserNameCharacters = "abcdefghijklmnopqrstuvwxyzABCDEFGHIJKLMNOPQRSTUVWXYZ0123456789-._@+#=";
})
.AddEntityFrameworkStores<DataContext>()
.AddDefaultTokenProviders();

builder.Services.Configure<DataProtectionTokenProviderOptions>(o =>
{
    o.Name = "Default";
    o.TokenLifespan = TimeSpan.FromHours(12);
});

// --- Services Registration ---
builder.Services.AddScoped<IDataContext, DataContext>();
builder.Services.AddSingleton<IHttpContextAccessor, HttpContextAccessor>();
builder.Services.AddSingleton<IHashUtility, HashUtility>();
builder.Services.AddTransient<IEmailService, SmtpEmailService>();
builder.Services.AddScoped<IJWTService, JWTService>();
builder.Services.AddMediatR(cfg => {
    cfg.RegisterServicesFromAssembly(typeof(Program).Assembly);
    cfg.AddOpenBehavior(typeof(ValidationBehavior<,>));
});
builder.Services.AddValidatorsFromAssembly(typeof(Program).Assembly);
builder.Services.AddScoped<ITelegramService, TelegramHandler>();
builder.Services.AddSingleton<ITelegramCoreService, TelegramCoreService>();

// Detection
builder.Services.AddDetection();

// Http Client
builder.Services.AddHttpClient();

// Health Checks
builder.Services.AddHealthChecks()
    .AddDbContextCheck<DataContext>();

// Rate Limiting
builder.Services.AddRateLimiter(options =>
{
    options.RejectionStatusCode = StatusCodes.Status429TooManyRequests;
    options.AddFixedWindowLimiter("AuthPolicy", opt =>
    {
        opt.Window = TimeSpan.FromMinutes(1);
        opt.PermitLimit = 5;
        opt.QueueLimit = 0;
        opt.QueueProcessingOrder = QueueProcessingOrder.OldestFirst;
    });
});

// Localization
builder.Services.AddLocalization(options => options.ResourcesPath = "Resources");

// --- API Versioning ---
builder.Services.AddApiVersioning(options =>
{
    options.DefaultApiVersion = new ApiVersion(1, 0);
    options.AssumeDefaultVersionWhenUnspecified = true;
    options.ReportApiVersions = true;
})
.AddApiExplorer(options =>
{
    options.GroupNameFormat = "'v'VVV";
    options.SubstituteApiVersionInUrl = true;
});



builder.Services.ConfigureHttpJsonOptions(options =>
{
    options.SerializerOptions.ReferenceHandler = System.Text.Json.Serialization.ReferenceHandler.IgnoreCycles;
    options.SerializerOptions.TypeInfoResolverChain.Insert(0, ApplicationAuth.SharedModels.AppJsonSerializerContext.Default);
});

builder.Services.AddEndpointsApiExplorer();

// --- Swagger ---
builder.Services.AddSwaggerGen(options =>
{
    options.EnableAnnotations();

    options.AddSecurityDefinition("Bearer", new OpenApiSecurityScheme
    {
        In = ParameterLocation.Header,
        Description = "Access token",
        Name = "Authorization",
        Type = SecuritySchemeType.ApiKey
    });
    
    // Simple document generation for v1
    options.SwaggerDoc("v1", new OpenApiInfo { Title = "ApplicationAuth API v1", Version = "v1" });

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
            Array.Empty<string>()
        }
    });
});

// --- Authentication ---
builder.Services.AddAuthentication(options =>
{
    options.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
    options.DefaultSignInScheme = JwtBearerDefaults.AuthenticationScheme;
    options.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
})
.AddJwtBearer(options =>
{
    options.RequireHttpsMetadata = true;
    options.SaveToken = true;
    options.TokenValidationParameters = new TokenValidationParameters
    {
        ValidateIssuer = true,
        ValidIssuer = AuthOptions.ISSUER,
        ValidateAudience = true,
        ValidateActor = false,
        ValidAudience = AuthOptions.AUDIENCE,
        ValidateLifetime = true,
        IssuerSigningKey = AuthOptions.GetSymmetricSecurityKey(),
        ValidateIssuerSigningKey = true,
        LifetimeValidator = (DateTime? notBefore, DateTime? expires, SecurityToken securityToken, TokenValidationParameters validationParameters) =>
        {
            if (!notBefore.HasValue || !expires.HasValue || DateTime.Compare(expires.Value, DateTime.UtcNow) <= 0)
            {
                return false;
            }
            return true;
        }
    };
});

builder.Services.AddMemoryCache();
builder.Services.AddCors();

var app = builder.Build();

// --- Middleware Pipeline ---

var cultures = configuration.GetSection("SupportedCultures").Get<string[]>() ?? new[] { "en" };
var supportedCultures = cultures.Select(c => new CultureInfo(c)).ToList();

app.UseRequestLocalization(new RequestLocalizationOptions
{
    DefaultRequestCulture = new RequestCulture("en"),
    SupportedCultures = supportedCultures,
    SupportedUICultures = supportedCultures
});

// Cookie Auth extraction
app.UseCookiePolicy(new CookiePolicyOptions
{
    MinimumSameSitePolicy = SameSiteMode.Strict,
    HttpOnly = HttpOnlyPolicy.Always,
    Secure = CookieSecurePolicy.Always
});

app.Use(async (context, next) =>
{
    var token = context.Request.Cookies[".AspNetCore.Application.Id"];
    if (!string.IsNullOrEmpty(token))
        context.Request.Headers.Append("Authorization", "Bearer " + token);

    await next();
});

if (app.Environment.IsDevelopment())
{
    app.UseDeveloperExceptionPage();
    app.UseForwardedHeaders(new ForwardedHeadersOptions
    {
        ForwardedHeaders = ForwardedHeaders.XForwardedFor,
        ForwardLimit = 2
    });
}

if (!Directory.Exists("Logs"))
{
    Directory.CreateDirectory("Logs");
}

app.UseSwagger();
app.UseSwaggerUI(options =>
{
    options.SwaggerEndpoint("/swagger/v1/swagger.json", "V1");
});

app.UseCors(builder =>
{
    // Minimal CORS policy for simplicity. Update for prod as needed.
    builder.WithOrigins("http://localhost:4200").AllowAnyHeader().AllowAnyMethod().AllowCredentials();
});

app.UseStaticFiles();
app.UseRouting();

app.UseSerilogRequestLogging();

// Custom Global Exception Handler
app.UseExceptionHandler(appBuilder =>
{
    appBuilder.Run(async context =>
    {
        var localizer = context.RequestServices.GetService<IStringLocalizer<ErrorsResource>>();
        var errorModel = new ErrorResponseModel(localizer);
        var exceptionHandlerPathFeature = context.Features.Get<IExceptionHandlerPathFeature>();
        
        var exception = exceptionHandlerPathFeature?.Error;
        var logger = context.RequestServices.GetRequiredService<ILogger<Program>>();
        
        if (exception is CustomException ex)
        {
            logger.LogWarning(ex, "CustomException thrown: {Message}", ex.Message);
            var resultObject = errorModel.Error(ex);
            context.Response.StatusCode = (int)(resultObject.StatusCode ?? 500);
            context.Response.ContentType = "application/json";
            await context.Response.WriteAsync(resultObject.Content);
        }
        else if (exception is FluentValidation.ValidationException validationException)
        {
            logger.LogWarning(validationException, "ValidationException thrown");
            foreach (var error in validationException.Errors)
            {
                var propName = string.IsNullOrEmpty(error.PropertyName) ? "request" : error.PropertyName.ToLower();
                errorModel.AddError(propName, error.ErrorMessage);
            }
            var resultObject = errorModel.BadRequest();
            context.Response.StatusCode = (int)(resultObject.StatusCode ?? 400);
            context.Response.ContentType = "application/json";
            await context.Response.WriteAsync(resultObject.Content);
        }
        else if (exception != null)
        {
            logger.LogError(exception, "Unhandled exception in middleware pipeline!");
            context.Response.StatusCode = 500;
            context.Response.ContentType = "text/plain";
            await context.Response.WriteAsync($"FATAL CRASH:\n{exception.ToString()}");
        }
    });
});

app.UseAuthentication();
app.UseAuthorization();
app.UseRateLimiter();


// Endpoint for Minimal APIs (Vertical Slices will use this)
app.MapLoginEndpoint();
app.MapRegisterEndpoint();
app.MapVerifyEmailEndpoints();
app.MapSocialAuthEndpoints();
app.MapPasswordRecoveryEndpoints();
app.MapAdminLoginEndpoint();
app.MapRefreshTokenEndpoints();
app.MapLogoutEndpoints();
app.MapGetAllUsersEndpoints();
app.MapDeleteUserEndpoints();
app.MapGetAllAdminsEndpoints();
app.MapTelegramEndpoints();
app.MapTestEndpoints();

// Health Check Endpoint
app.MapHealthChecks("/health");

// Run DB Migrations / Initialization at startup
using (var scope = app.Services.CreateScope())
{
    var services = scope.ServiceProvider;
    try
    {
        var context = services.GetRequiredService<DataContext>();
        var dbInitLogger = services.GetRequiredService<ILogger<Program>>();
        dbInitLogger.LogInformation("Initializing database...");
        context.Database.Migrate();
        
        var roleManager = services.GetRequiredService<RoleManager<ApplicationRole>>();
        string[] roles = { Role.User, Role.Admin, Role.SuperAdmin };
        foreach (var role in roles)
        {
            if (!roleManager.RoleExistsAsync(role).GetAwaiter().GetResult())
            {
                roleManager.CreateAsync(new ApplicationRole { Name = role }).GetAwaiter().GetResult();
            }
        }

        var userManager = services.GetRequiredService<UserManager<ApplicationUser>>();
        if (userManager.FindByEmailAsync("admin@test.com").GetAwaiter().GetResult() == null)
        {
            var adminUser = new ApplicationUser
            {
                UserName = "admin@test.com",
                Email = "admin@test.com",
                EmailConfirmed = true,
                IsActive = true,
                RegistratedAt = DateTime.UtcNow
            };
            userManager.CreateAsync(adminUser, "Welcome1!").GetAwaiter().GetResult();
            userManager.AddToRoleAsync(adminUser, Role.SuperAdmin).GetAwaiter().GetResult();
        }
    }
    catch (Exception ex)
    {
        var logger = services.GetRequiredService<ILogger<Program>>();
        logger.LogError(ex, "An error occurred while seeding the database.");
    }
}

    app.Run();
}
catch (Exception ex)
{
    Log.Fatal(ex, "Application terminated unexpectedly");
}
finally
{
    Log.CloseAndFlush();
}
