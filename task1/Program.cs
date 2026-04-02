using System.Text.Json;
using task1.Application.DependencyInjection;
using task1.Application.Interfaces;
using task1.Models;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Authorization;
using Microsoft.IdentityModel.Tokens;
using System.Security.Claims;
using System.Text;
using task1;
using task1.Authorization;
using task1.Hubs;
using Microsoft.AspNetCore.RateLimiting;
using System.Threading.RateLimiting;
var builder = WebApplication.CreateBuilder(args);

// ===== Add Services =====
builder.Services.AddControllers();
builder.Services.AddSignalR();
builder.Services.AddCors(options =>
{
    options.AddPolicy("NextPolicy",
        policy => policy
            .WithOrigins(
                "http://localhost:3000",
                "http://127.0.0.1:3000",
                "https://localhost:3000",
                "https://127.0.0.1:3000",
                "https://dashboard-frontend-k2sf.vercel.app")
            .AllowAnyHeader()
            .AllowAnyMethod());
});

builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

builder.Services.AddApplicationInfrastructureServices(builder.Configuration);
builder.Services.AddApplicationLayerServices();
builder.Services.AddScoped<INotificationRealtimePublisher, NotificationsRealtimePublisher>();

// ===== JWT Authentication =====
var jwtSettings = builder.Configuration.GetSection("Jwt");

builder.Services.AddAuthentication(options =>
{
    options.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
    options.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
})
.AddJwtBearer(options =>
{
    options.TokenValidationParameters = new TokenValidationParameters
    {
        ValidateIssuer = true,
        ValidateAudience = true,
        ValidateLifetime = true,
        ValidateIssuerSigningKey = true,

        ValidIssuer = jwtSettings["Issuer"],
        ValidAudience = jwtSettings["Audience"],
        IssuerSigningKey = new SymmetricSecurityKey(
            Encoding.UTF8.GetBytes(jwtSettings["Key"])),

        RoleClaimType = "role",
        NameClaimType = "name"
    };

    options.Events = new JwtBearerEvents
    {
        OnMessageReceived = context =>
        {
            var accessToken = context.Request.Query["access_token"];
            var path = context.HttpContext.Request.Path;

            if (!string.IsNullOrEmpty(accessToken) &&
                path.StartsWithSegments("/hubs/notifications"))
            {
                context.Token = accessToken;
            }

            return Task.CompletedTask;
        },
        OnTokenValidated = context =>
        {
            // Support old tokens that used long claim URIs: ensure "role" and "name" exist for [Authorize] and User.Identity.Name
            var identity = (ClaimsIdentity?)context.Principal?.Identity;
            if (identity == null) return Task.CompletedTask;

            if (identity.FindFirst("role") == null)
            {
                var oldRole = identity.FindFirst(ClaimTypes.Role);
                if (oldRole != null)
                    identity.AddClaim(new System.Security.Claims.Claim("role", oldRole.Value));
            }
            if (identity.FindFirst("name") == null)
            {
                var oldName = identity.FindFirst(ClaimTypes.Name) ?? identity.FindFirst("unique_name");
                if (oldName != null)
                    identity.AddClaim(new System.Security.Claims.Claim("name", oldName.Value));
            }

            return Task.CompletedTask;
        },
        OnChallenge = async context =>
        {
            context.HandleResponse();
            context.Response.StatusCode = 401;
            context.Response.ContentType = "application/json";
            var body = new ApiResponse<object> { Error = new ApiError { Code = "UNAUTHORIZED", Message = "Unauthorized: please sign in again." } };
            await context.Response.WriteAsync(JsonSerializer.Serialize(body));
        },
        OnForbidden = async context =>
        {
            context.Response.StatusCode = 403;
            context.Response.ContentType = "application/json";
            var body = new ApiResponse<object> { Error = new ApiError { Code = "FORBIDDEN", Message = "Forbidden: you do not have permission to access this resource." } };
            await context.Response.WriteAsync(JsonSerializer.Serialize(body));
        }
    };
});

builder.Services.AddMemoryCache();
builder.Services.AddScoped<IAuthorizationHandler, PermissionHandler>();
builder.Services.AddScoped<ITokenService, JwtService>();
builder.Services.AddAuthorization(options =>
{
    var permissions = new[]
    {
        "ViewUsers", "SearchUsers", "ViewUser", "CreateUser", "EditUser", "DeleteUser", "ChangeUserRole",
        "ViewCustomers", "SearchCustomers", "ViewCustomer", "CreateCustomer", "EditCustomer", "DeleteCustomer",
        "ViewCars", "ViewCar", "CreateCar", "EditCar", "DeleteCar",
        "ViewPosts", "ViewPost", "CreatePost", "EditPost", "DeletePost",
        "ViewDashboard",
        "ViewNotifications", "DeleteNotifications"
    };

    foreach (var permission in permissions)
    {
        options.AddPolicy(permission, policy =>
            policy.Requirements.Add(new PermissionRequirement(permission)));
    }
});
builder.Services.AddRateLimiter(options =>
{
    options.RejectionStatusCode = StatusCodes.Status429TooManyRequests;
    options.AddPolicy("auth-login-register", context =>
        RateLimitPartition.GetFixedWindowLimiter(
            partitionKey: context.Connection.RemoteIpAddress?.ToString() ?? "unknown",
            factory: _ => new FixedWindowRateLimiterOptions
            {
                PermitLimit = 10,                 
                Window = TimeSpan.FromMinutes(1),
                QueueLimit = 0
            }));
});
var app = builder.Build();


    app.UseSwagger();
    app.UseSwaggerUI();

if (!app.Environment.IsDevelopment())
{
    app.UseHttpsRedirection();
}
app.UseRouting();
app.UseCors("NextPolicy");

app.UseAuthentication();
app.UseRateLimiter();

app.UseAuthorization();

app.MapControllers();
app.MapHub<NotificationsHub>("/hubs/notifications");
app.Run();