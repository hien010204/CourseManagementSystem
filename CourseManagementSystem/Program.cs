using CourseManagementSystem.Filter;
using CourseManagementSystem.Services.Announcements;
using CourseManagementSystem.Services.Assignments;
using CourseManagementSystem.Services.Courses;
using CourseManagementSystem.Services.Email;
using CourseManagementSystem.Services.Models;
using CourseManagementSystem.Services.Profile;
using CourseManagementSystem.Services.QnA;
using CourseManagementSystem.Services.Users;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using Microsoft.OpenApi.Models;
using System.Text;

namespace CourseManagementSystem
{
    public class Program
    {
        public static void Main(string[] args)
        {
            var builder = WebApplication.CreateBuilder(args);

            builder.Services.AddControllers();
            // Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
            builder.Services.AddEndpointsApiExplorer();
            builder.Services.AddSwaggerGen();
            builder.Services.AddHttpClient();


            // Add services to the container.
            builder.Services.AddScoped<IUserService, UserService>();
            builder.Services.AddScoped<ICourseService, CourseService>();
            builder.Services.AddScoped<IProfileService, ProfileService>();
            builder.Services.AddScoped<IAssignmentService, AssignmentService>();
            builder.Services.AddScoped<IQnAService, QnAService>();
            builder.Services.AddScoped<IAnnouncementService, AnnouncementService>();

            builder.Services.AddControllers()
            .AddJsonOptions(options =>
            {
                options.JsonSerializerOptions.ReferenceHandler = System.Text.Json.Serialization.ReferenceHandler.Preserve;
                options.JsonSerializerOptions.MaxDepth = 64; // Tăng độ sâu nếu cần
            });

            // Đăng ký DbContext để kết nối với cơ sở dữ liệu
            builder.Services.AddDbContext<CourseManagementContext>(options =>
            {
                options.UseSqlServer(builder.Configuration.GetConnectionString("DefaultConnection"));
            });

            var configuration = builder.Configuration;

            // Configure JWT Authentication
            var key = Encoding.UTF8.GetBytes(builder.Configuration["Jwt:Key"]);
            builder.Services.AddAuthentication(options =>
            {
                options.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
                options.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
            })
            .AddJwtBearer(options =>
            {
                options.RequireHttpsMetadata = false;
                options.SaveToken = true;
                options.TokenValidationParameters = new TokenValidationParameters
                {
                    ValidateIssuerSigningKey = true,
                    IssuerSigningKey = new SymmetricSecurityKey(key),
                    ValidateIssuer = false,
                    ValidateAudience = false,
                    ValidateLifetime = true
                };
            });

            builder.Services.AddSingleton<EmailService>(); // Đăng ký EmailService như một singleton
            builder.Services.AddControllers();

            // Cấu hình phân quyền cho Admin
            builder.Services.AddAuthorization(options =>
            {
                // Quyền Admin chỉ có thể thực hiện thêm hoặc xóa
                options.AddPolicy("AdminOnly", policy =>
                    policy.RequireRole("Admin"));
            });

            builder.Services.AddSwaggerGen(c =>
            {


                // //Thêm bộ lọc để loại bỏ các trường không mong muốn
                c.OperationFilter<RemoveUnusedFieldsOperationFilter>();
                c.AddSecurityDefinition("Bearer", new OpenApiSecurityScheme
                {
                    In = ParameterLocation.Header,
                    Description = "Please enter a valid token",
                    Name = "Authorization",
                    Type = SecuritySchemeType.Http,
                    BearerFormat = "JWT",
                    Scheme = "Bearer"
                });
                c.AddSecurityRequirement(new OpenApiSecurityRequirement
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
                        new string[]{ }
                    }
                });
            });


            builder.Services.AddCors(options =>
            {
                options.AddPolicy("AllowFrontend",
                    builder => builder.WithOrigins("http://localhost:5173") // Không dùng '*'
                        .AllowCredentials()
                        .AllowAnyHeader()
                        .AllowAnyMethod());
            });
            var app = builder.Build();

            // Configure the HTTP request pipeline.
            app.UseSwagger();
            app.UseSwaggerUI(x =>
            {
                x.SwaggerEndpoint("/swagger/v1/swagger.json", "Web API V1");

#if DEBUG
                x.RoutePrefix = "swagger"; // For localhost
#else
                x.RoutePrefix = string.Empty; // For production
#endif
            });

            app.UseHttpsRedirection();
            app.UseAuthentication();
            app.UseAuthorization();
            app.UseCors("AllowFrontend"); // Áp dụng chính sách CORS

            app.MapControllers();

            app.Run();
        }
    }
}


