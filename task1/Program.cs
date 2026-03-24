using System.Text.Json;
using task1.Application.DependencyInjection;
using task1.DataLayer.DependencyInjection;
using task1.DataLayer.Entities;
using task1.DataLayer.DbContexts;
using task1.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Authorization;
using Microsoft.IdentityModel.Tokens;
using System.Security.Claims;
using System.Text;
using task1.Authorization;

var builder = WebApplication.CreateBuilder(args);

// ===== Add Services =====
builder.Services.AddControllers();

builder.Services.AddCors(options =>
{
    options.AddPolicy("NextPolicy",
        policy => policy
            .WithOrigins("http://localhost:3000")
            .AllowAnyHeader()
            .AllowAnyMethod());
});

builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

builder.Services.AddApplicationLayerServices(builder.Configuration);
// builder.Services.AddDataLayerRepositories(); // keep if needed

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
builder.Services.AddScoped<JwtService>();
var app = builder.Build();

if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseCors("NextPolicy");
app.UseHttpsRedirection();

app.UseAuthentication(); 
app.UseAuthorization();

app.MapControllers();

app.Run();