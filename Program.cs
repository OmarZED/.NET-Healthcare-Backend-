using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using Microsoft.OpenApi.Models;
using Swashbuckle.AspNetCore.SwaggerGen;
using System.Text;
using System.Text.Json.Serialization;
using medical.Models;       // Assuming your Models (ApplicationUser) are still in medical.Models

// --- Updated Namespaces ---
using medical.Interface;    // For IAuthService, etc.
using medical.Reposotiry;   // For AuthService implementation, etc. (Verify this is correct)
using medical.DBContext;    // For ApplicationDbContext

namespace medical // Your project's namespace
{
    public class Program
    {
        public static void Main(string[] args)
        {
            var builder = WebApplication.CreateBuilder(args);

            // --- Add services to the container ---

            // 1. Standard Controllers & API Explorer
            builder.Services.AddControllers();
            builder.Services.AddEndpointsApiExplorer();

            // 2. Swagger Configuration with JWT Support
            builder.Services.AddSwaggerGen(options =>
            {
                options.SwaggerDoc("v1", new OpenApiInfo { Title = "Medical API", Version = "v1" });

                // Configure JWT Authentication in Swagger
                options.AddSecurityDefinition(JwtBearerDefaults.AuthenticationScheme, new OpenApiSecurityScheme
                {
                    Name = "Authorization",
                    Description = "Enter 'Bearer' followed by your token. Example: Bearer {token}",
                    In = ParameterLocation.Header,
                    Type = SecuritySchemeType.Http,
                    Scheme = JwtBearerDefaults.AuthenticationScheme.ToLower(),
                    BearerFormat = "JWT"
                });

                // Add Security Requirement for JWT
                options.AddSecurityRequirement(new OpenApiSecurityRequirement
                {
                    {
                        new OpenApiSecurityScheme
                        {
                            Reference = new OpenApiReference
                            {
                                Type = ReferenceType.SecurityScheme,
                                Id = JwtBearerDefaults.AuthenticationScheme
                            }
                        },
                        Array.Empty<string>()
                    }
                });

                // Add Operation Filter to automatically apply lock icon to protected endpoints
                options.OperationFilter<AuthorizationOperationFilter>();
            });

            // 3. JSON Enum Serialization
            builder.Services.AddControllers()
                .AddJsonOptions(options =>
                    options.JsonSerializerOptions.Converters.Add(new JsonStringEnumConverter()));

            // 4. Register DbContext (Using medical.DBContext namespace)
            var connectionString = builder.Configuration.GetConnectionString("DefaultConnection")
                ?? throw new InvalidOperationException("Connection string 'DefaultConnection' not found.");
            // Ensure ApplicationDbContext class is in the medical.DBContext namespace
            builder.Services.AddDbContext<ApplicationDbContext>(options =>
                options.UseSqlServer(connectionString));

            // 5. Configure Identity using ApplicationUser (Using medical.DBContext for EF Stores)
            // Ensure ApplicationUser class is in the medical.Models namespace
            builder.Services.AddIdentity<ApplicationUser, IdentityRole>(options =>
            {
                options.SignIn.RequireConfirmedAccount = false;
                options.Password.RequireDigit = true;
                options.Password.RequireLowercase = true;
                options.Password.RequireUppercase = true;
                options.Password.RequireNonAlphanumeric = false;
                options.Password.RequiredLength = 8;
            })
            // Ensure ApplicationDbContext is correctly namespaced here too
            .AddEntityFrameworkStores<ApplicationDbContext>()
            .AddDefaultTokenProviders();

            // 6. Configure JWT Authentication
            var jwtIssuer = builder.Configuration["JWT:Issuer"] ?? throw new InvalidOperationException("JWT:Issuer not found.");
            var jwtAudience = builder.Configuration["JWT:Audience"] ?? throw new InvalidOperationException("JWT:Audience not found.");
            var jwtSigningKey = builder.Configuration["JWT:SigningKey"] ?? throw new InvalidOperationException("JWT:SigningKey not found.");

            builder.Services.AddAuthentication(options =>
            {
                options.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
                options.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
                options.DefaultScheme = JwtBearerDefaults.AuthenticationScheme;
            })
            .AddJwtBearer(options =>
            {
                options.TokenValidationParameters = new TokenValidationParameters
                {
                    ValidateIssuer = true,
                    ValidIssuer = jwtIssuer,
                    ValidateAudience = true,
                    ValidAudience = jwtAudience,
                    ValidateLifetime = true,
                    ValidateIssuerSigningKey = true,
                    IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(jwtSigningKey)),
                    ClockSkew = TimeSpan.Zero
                };
                options.Events = new JwtBearerEvents { /* ... handlers as before ... */ };
            });

            // 7. Configure Authorization
            builder.Services.AddAuthorization();

            // 8. Configure CORS
            builder.Services.AddCors(options =>
            {
                options.AddPolicy("AllowSpecificOrigin",
                    policyBuilder => { /* ... policy as before ... */
                        policyBuilder
                            .WithOrigins("http://127.0.0.1:5500", "http://localhost:4200") // Replace with your frontend URL(s)
                            .AllowAnyMethod()
                            .AllowAnyHeader();
                    });
            });

            // --- 9. Register Custom Application Services ---
           
            builder.Services.AddScoped<IAuthService, AuthService>();

            // Add registrations for IPatientProfileService and IDoctorProfileService when implemented
              builder.Services.AddScoped<IPatientProfileService, PatientProfileService>();
             builder.Services.AddScoped<IDoctorProfileService, DoctorProfileService>();


            // 10. AutoMapper (Optional)
            // builder.Services.AddAutoMapper(typeof(MappingProfiles).Assembly);

            // --- Build the application ---
            var app = builder.Build();

            // --- Configure the HTTP request pipeline ---
            if (app.Environment.IsDevelopment())
            { /* ... Dev config as before ... */
                app.UseDeveloperExceptionPage();
                app.UseSwagger();
                app.UseSwaggerUI(c => c.SwaggerEndpoint("/swagger/v1/swagger.json", "Medical API V1"));
            }
            else
            { /* ... Prod config as before ... */
                app.UseExceptionHandler("/Error");
                app.UseHsts();
            }

            // Middleware Pipeline Order
            app.UseHttpsRedirection();
            app.UseRouting();
            app.UseCors("AllowSpecificOrigin"); // Or your policy name
            app.UseAuthentication(); // MUST be before UseAuthorization
            app.UseAuthorization();
            app.MapControllers();

            // Optional: Seeding
            // using (var scope = app.Services.CreateScope()) { /* ... seeding logic ... */ }

            // --- Run the application ---
            app.Run();
        }
    }

    // AuthorizationOperationFilter class remains the same as before
    public class AuthorizationOperationFilter : IOperationFilter
    {
        public void Apply(OpenApiOperation operation, OperationFilterContext context)
        {
            // --- Get attributes robustly ---
            var methodAuthAttributes = context.MethodInfo.GetCustomAttributes(true).OfType<AuthorizeAttribute>();
            var controllerAuthAttributes = context.MethodInfo.DeclaringType?.GetCustomAttributes(true).OfType<AuthorizeAttribute>() ?? Enumerable.Empty<AuthorizeAttribute>();
            var allAuthAttributes = methodAuthAttributes.Union(controllerAuthAttributes).ToList();

            var methodAllowAnonymous = context.MethodInfo.GetCustomAttributes(true).OfType<AllowAnonymousAttribute>().Any();
            var controllerAllowAnonymous = context.MethodInfo.DeclaringType?.GetCustomAttributes(true).OfType<AllowAnonymousAttribute>().Any() ?? false;
            bool allowAnonymous = methodAllowAnonymous || controllerAllowAnonymous;

            // --- Apply Security Requirement ---
            if (allAuthAttributes.Any() && !allowAnonymous)
            {
                operation.Security ??= new List<OpenApiSecurityRequirement>();
                var securityRequirement = new OpenApiSecurityRequirement { /* ... Requirement as before ... */
                     {
                        new OpenApiSecurityScheme
                        {
                            Reference = new OpenApiReference
                            {
                                Id = JwtBearerDefaults.AuthenticationScheme,
                                Type = ReferenceType.SecurityScheme
                            }
                        },
                        Array.Empty<string>()
                    }
                };
                if (!operation.Security.Any(req => req.ContainsKey(securityRequirement.First().Key)))
                {
                    operation.Security.Add(securityRequirement);
                }
            }
        }
    }
}