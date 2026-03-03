using System;
using System.Text;
using BusinessLogic.Services;
using BusinessLogic.Services.Implementation;
using DataAccess.Persistence;
using DataAccess.Repositories;
using DataAccess.Repositories.Implementation;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using Presentation.Middleware;
using DotNetEnv;
using CloudinaryDotNet;

namespace Presentation
{
    public class Program
    {
        public static void Main(string[] args)
        {
            // Load .env variables
            DotNetEnv.Env.Load(
                Path.Combine(Directory.GetCurrentDirectory(), "..", ".env")
            );
            Console.WriteLine("Current Dir: " + Directory.GetCurrentDirectory());

            var builder = WebApplication.CreateBuilder(args);
            var port = Environment.GetEnvironmentVariable("PORT") ?? "8080";
            builder.WebHost.UseUrls($"http://*:{port}");

            // 1. Cấu hình db connect
            var connectionString = builder.Configuration.GetConnectionString("DbContext");

            builder.Services.AddDbContext<AppDbContext>(options =>
                options.UseSqlServer(connectionString, sqlServerOptionsAction: sqlOptions =>
                {
                    sqlOptions.EnableRetryOnFailure();
                })
            );

            // 2. Cấu hình JWT Authentication
            builder
                .Services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
                .AddJwtBearer(options =>
                {
                    options.TokenValidationParameters = new TokenValidationParameters
                    {
                        ValidateIssuer = true,
                        ValidateAudience = true,
                        ValidateLifetime = true,
                        ValidateIssuerSigningKey = true,
                        ValidIssuer = builder.Configuration["Jwt:Issuer"],
                        ValidAudience = builder.Configuration["Jwt:Audience"],
                        IssuerSigningKey = new SymmetricSecurityKey(
                            Encoding.UTF8.GetBytes(builder.Configuration["Jwt:Key"])
                        ),
                    };

                    options.Events = new JwtBearerEvents
                    {
                        OnChallenge = context =>
                        {
                            context.HandleResponse();
                            context.Response.StatusCode = StatusCodes.Status401Unauthorized;
                            context.Response.ContentType = "application/json";
                            var result = System.Text.Json.JsonSerializer.Serialize(
                                new BusinessLogic.Wrappers.ApiResponse<object>(false, "Bạn chưa đăng nhập hoặc token không hợp lệ.", StatusCodes.Status401Unauthorized),
                                new System.Text.Json.JsonSerializerOptions { PropertyNamingPolicy = System.Text.Json.JsonNamingPolicy.CamelCase }
                            );
                            return context.Response.WriteAsync(result);
                        },
                        OnForbidden = context =>
                        {
                            context.Response.StatusCode = StatusCodes.Status403Forbidden;
                            context.Response.ContentType = "application/json";
                            var result = System.Text.Json.JsonSerializer.Serialize(
                                new BusinessLogic.Wrappers.ApiResponse<object>(false, "Bạn không có quyền truy cập tài nguyên này.", StatusCodes.Status403Forbidden),
                                new System.Text.Json.JsonSerializerOptions { PropertyNamingPolicy = System.Text.Json.JsonNamingPolicy.CamelCase }
                            );
                            return context.Response.WriteAsync(result);
                        }
                    };
                });

            // Repository registration
            builder.Services.AddScoped<IUserRepository, UserRepository>();
            builder.Services.AddScoped<IMovieRepository, MovieRepository>();

            // Service registration
            builder.Services.AddScoped<IMovieService, MovieService>();
            builder.Services.AddScoped<ITokenService, TokenService>();
            builder.Services.AddScoped<IAuthService, AuthService>();
            builder.Services.AddScoped<IImageService, ImageService>();
            builder.Services.AddScoped<IEmailService, EmailService>();

            // Cloudinary

            var cloudName = Environment.GetEnvironmentVariable("Cloudinary__CloudName");
            var apiKey = Environment.GetEnvironmentVariable("Cloudinary__ApiKey");
            var apiSecret = Environment.GetEnvironmentVariable("Cloudinary__ApiSecret");

            if (string.IsNullOrEmpty(cloudName) ||
                string.IsNullOrEmpty(apiKey) ||
                string.IsNullOrEmpty(apiSecret))
            {
                throw new Exception("Cloudinary environment variables are missing!");
            }

            var account = new Account(cloudName, apiKey, apiSecret);
            var cloudinary = new Cloudinary(account);
            cloudinary.Api.Secure = true;

            builder.Services.AddSingleton(cloudinary);

            builder.Services.AddAutoMapper(AppDomain.CurrentDomain.GetAssemblies());

            builder.Services.AddControllers();
            // Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
            builder.Services.AddEndpointsApiExplorer();
            builder.Services.AddSwaggerGen();

            builder.Services.AddExceptionHandler<GlobalExceptionHandler>();
            builder.Services.AddProblemDetails();
            builder.Services.AddCors(options =>
            {
                var clientUrl = builder.Configuration["ClientBaseUrl"] ?? "http://localhost:3000";
                options.AddPolicy(
                    "AllowFrontend",
                    policy =>
                    {
                        policy.WithOrigins("http://localhost:5173", "http://localhost:3000", clientUrl)
                              .AllowAnyHeader()
                              .AllowAnyMethod()
                              .AllowCredentials();
                    }
                );
            });

            var app = builder.Build();

            // Configure the HTTP request pipeline.
            app.UseExceptionHandler();

            app.UseSwagger();
            app.UseSwaggerUI();

            app.UseHttpsRedirection();
            app.UseAuthentication();
            app.UseAuthorization();
            app.UseCors("AllowFrontend");

            app.MapControllers();

            app.Run();
        }
    }
}
