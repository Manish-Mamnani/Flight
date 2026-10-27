using System.Text;
using BookingService.Data;
using BookingService.HttpClients;
using BookingService.Middleware;
using BookingService.Repositories;
using BookingService.Repositories.Interfaces;
using BookingService.Services;
using BookingService.Services.Interfaces;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using Microsoft.OpenApi.Models;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();

// Swagger with Authenticate/Authorize Button
builder.Services.AddSwaggerGen(options =>
{
    options.SwaggerDoc("v1", new OpenApiInfo { Title = "SkyLink - Booking Service API", Version = "v1" });

    options.AddSecurityDefinition("Bearer", new OpenApiSecurityScheme
    {
        Name = "Authorization",
        Type = SecuritySchemeType.Http,
        Scheme = "Bearer",
        BearerFormat = "JWT",
        In = ParameterLocation.Header,
        Description = "Enter your JWT token to authorize API testing.\n\nExample: eyJhbGci..."
    });

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

builder.Services.AddDbContext<BookingDbContext>(options =>
    options.UseSqlServer(builder.Configuration.GetConnectionString("DefaultConnection")));

builder.Services.AddAuthentication(options =>
{
    options.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
    options.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
})
.AddJwtBearer(options =>
{
    var jwtKey = builder.Configuration["Jwt:Key"]!;
    var key = Encoding.UTF8.GetBytes(jwtKey);

    options.TokenValidationParameters = new TokenValidationParameters
    {
        ValidateIssuer = true,
        ValidateAudience = true,
        ValidateLifetime = true,
        ValidateIssuerSigningKey = true,
        ValidIssuer = builder.Configuration["Jwt:Issuer"],
        ValidAudience = builder.Configuration["Jwt:Audience"],
        IssuerSigningKey = new SymmetricSecurityKey(key),
        NameClaimType = "UserId",
        RoleClaimType = "Role"
    };
});

builder.Services.AddAuthorization();

// Register Typed HttpClients
var searchServiceUrl = builder.Configuration["Services:SearchService"] ?? "http://localhost:5102";
builder.Services.AddHttpClient<ISearchServiceClient, SearchServiceClient>(client =>
{
    client.BaseAddress = new Uri(searchServiceUrl);
    client.Timeout = TimeSpan.FromSeconds(5);
});

var fareServiceUrl = builder.Configuration["Services:FareService"] ?? "http://localhost:5103";
builder.Services.AddHttpClient<IFareServiceClient, FareServiceClient>(client =>
{
    client.BaseAddress = new Uri(fareServiceUrl);
    client.Timeout = TimeSpan.FromSeconds(5);
});

var checkInServiceUrl = builder.Configuration["Services:CheckInService"] ?? "http://localhost:5105";
builder.Services.AddHttpClient<ICheckInServiceClient, CheckInServiceClient>(client =>
{
    client.BaseAddress = new Uri(checkInServiceUrl);
    client.Timeout = TimeSpan.FromSeconds(5);
});

// Register Repositories and Services
builder.Services.AddScoped<IBookingRepository, BookingRepository>();
builder.Services.AddScoped<IBookingService, BookingServiceImpl>();

var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseHttpsRedirection();
app.UseMiddleware<CorrelationIdMiddleware>();
app.UseMiddleware<ExceptionHandlingMiddleware>();

app.UseAuthentication();
app.UseAuthorization();

app.MapControllers();

app.Run();
