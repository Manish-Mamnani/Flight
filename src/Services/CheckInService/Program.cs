using System.Text;
using CheckInService.Data;
using CheckInService.HttpClients;
using CheckInService.Middleware;
using CheckInService.Repositories;
using CheckInService.Repositories.Interfaces;
using CheckInService.Services;
using CheckInService.Services.Interfaces;
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
    options.SwaggerDoc("v1", new OpenApiInfo { Title = "SkyLink - CheckIn Service API", Version = "v1" });

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

builder.Services.AddDbContext<CheckInDbContext>(options =>
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
var bookingServiceUrl = builder.Configuration["Services:BookingService"] ?? "http://localhost:5104";
builder.Services.AddHttpClient<IBookingServiceClient, BookingServiceClient>(client =>
{
    client.BaseAddress = new Uri(bookingServiceUrl);
    client.Timeout = TimeSpan.FromSeconds(5);
});

var searchServiceUrl = builder.Configuration["Services:SearchService"] ?? "http://localhost:5102";
builder.Services.AddHttpClient<ISearchServiceClient, SearchServiceClient>(client =>
{
    client.BaseAddress = new Uri(searchServiceUrl);
    client.Timeout = TimeSpan.FromSeconds(5);
});

// Register Repositories and Services
builder.Services.AddScoped<ICheckInRepository, CheckInRepository>();
builder.Services.AddScoped<ICheckInService, CheckInServiceImpl>();

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
