using Cinema_Backend.Data;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.IdentityModel.Tokens;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Cinema_Backend.Models;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.  
builder.Services.AddDbContext<ApplicationDbContext>(options =>
   options.UseSqlServer(builder.Configuration.GetConnectionString("DefaultConnection")));

builder.Services
    .AddIdentity<Cinema_Backend.Models.User, IdentityRole>(options =>
    {
        options.Password.RequireDigit = true;
        options.Password.RequiredLength = 6;
        options.Password.RequireNonAlphanumeric = false;
        options.User.RequireUniqueEmail = true;
    })
    .AddEntityFrameworkStores<ApplicationDbContext>()   // magazyn danych do Identity
    .AddDefaultTokenProviders();                        // umo¿liwia generowanie tokenów (np. reset has³a)

builder.Services.AddAuthentication(options => {
    options.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
    options.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
})
.AddJwtBearer(options => {
    options.TokenValidationParameters = new TokenValidationParameters
    {
        ValidateIssuer = true,
        ValidateAudience = true,
        ValidateLifetime = true,
        ValidateIssuerSigningKey = true,
        ValidIssuer = builder.Configuration["Jwt:Issuer"],
        ValidAudience = builder.Configuration["Jwt:Audience"],
        IssuerSigningKey = new SymmetricSecurityKey(System.Text.Encoding.UTF8.GetBytes(builder.Configuration["Jwt:Key"]))
    };
});

builder.Services.AddAuthorization();

builder.Services.AddControllers();
// Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle  
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(c =>
{
    c.SwaggerDoc("v1", new() { Title = "Cinema API", Version = "v1" });

    // Add Bearer authentication to Swagger
    c.AddSecurityDefinition("Bearer", new Microsoft.OpenApi.Models.OpenApiSecurityScheme
    {
        Name = "Authorization",
        Type = Microsoft.OpenApi.Models.SecuritySchemeType.Http,
        Scheme = "Bearer",
        BearerFormat = "JWT",
        In = Microsoft.OpenApi.Models.ParameterLocation.Header,
        Description = "Wpisz 'Bearer {token}'"
    });

    c.AddSecurityRequirement(new Microsoft.OpenApi.Models.OpenApiSecurityRequirement
    {
        {
            new Microsoft.OpenApi.Models.OpenApiSecurityScheme
            {
                Reference = new Microsoft.OpenApi.Models.OpenApiReference
                {
                    Type = Microsoft.OpenApi.Models.ReferenceType.SecurityScheme,
                    Id = "Bearer"
                }
            },
            Array.Empty<string>()
        }
    });
});

var app = builder.Build();

// Configure the HTTP request pipeline.  
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

//// Creating admin user if it doesn't exist
//using (var scope = app.Services.CreateScope())
//{
//    var services = scope.ServiceProvider;
//    var roleMgr = services.GetRequiredService<RoleManager<IdentityRole>>();
//    var userMgr = services.GetRequiredService<UserManager<Cinema_Backend.Models.User>>();

//    const string adminRole = "Admin";
//    if (!await roleMgr.RoleExistsAsync(adminRole))
//        await roleMgr.CreateAsync(new IdentityRole(adminRole));

//    const string adminEmail = "272659@student.pwr.edu.pl";
//    const string adminPass = "P@ssw0rd";
//    var admin = await userMgr.FindByEmailAsync(adminEmail);
//    if (admin == null)
//    {
//        admin = new User
//        {
//            UserName = adminEmail,
//            Email = adminEmail,
//            FirstName = "Admin",
//            LastName = "User",
//            EmailConfirmed = true
//        };

//        var result = await userMgr.CreateAsync(admin, adminPass);
//        if (result.Succeeded)
//        {
//            await userMgr.AddToRoleAsync(admin, adminRole);
//            Console.WriteLine($"Seeded ADMIN user: {adminEmail}");
//        }
//        else
//        {
//            Console.WriteLine("Failed to seed ADMIN user:");
//            foreach (var err in result.Errors) Console.WriteLine($" - {err.Description}");
//        }
//    }
//}

app.UseHttpsRedirection();

app.UseAuthentication();

app.UseAuthorization();

app.MapControllers();

app.Run();
