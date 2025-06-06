using CompanyApp.Application.InterfacesService;
using CompanyApp.Application.Mappings;
using CompanyApp.Application.Repositories;
using CompanyApp.Application.Services;
using CompanyApp.Core.Entities;
using CompanyApp.Core.Interfaces;
using CompanyApp.Infrastructure.Data;
using CompanyApp.Infrastructure.InterfacesRepository;
using CompanyApp.Infrastructure.Repositories;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Diagnostics;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.Services.AddControllersWithViews();

// �����������: ������ ���� DbContext � ����������� �����������
builder.Services.AddDbContext<OfficeDbContext>(options =>
{
    options.UseSqlServer(builder.Configuration.GetConnectionString("DefaultConnection"));

    // �����: ���������� TrackAll ��� ���������� ������ Update ��������
    options.UseQueryTrackingBehavior(QueryTrackingBehavior.TrackAll);

    // �������� ���������� �������� ��� ������������� ���������
    options.ConfigureWarnings(warnings =>
    {
        warnings.Ignore(RelationalEventId.MultipleCollectionIncludeWarning);
    });
});

// ��������� Identity
builder.Services.AddIdentity<ApplicationUser, IdentityRole>(options =>
{
    options.Password.RequireDigit = true;
    options.Password.RequiredLength = 6;
    options.Password.RequireNonAlphanumeric = false;
    options.Password.RequireUppercase = false;
    options.Password.RequireLowercase = true;
    options.User.RequireUniqueEmail = true;
})
.AddEntityFrameworkStores<OfficeDbContext>()
.AddDefaultTokenProviders();

// ��������� Cookie
builder.Services.ConfigureApplicationCookie(options =>
{
    options.LoginPath = "/Account/Login";
    options.LogoutPath = "/Account/Logout";
    options.AccessDeniedPath = "/Account/AccessDenied";
});

// AutoMapper
builder.Services.AddAutoMapper(typeof(MappingProfile));

// �����������
builder.Services.AddScoped<IOfficeRepository, OfficeRepository>();
builder.Services.AddScoped<IBuildingRepository, BuildingRepository>();
builder.Services.AddScoped<IDepartmentRepository, DepartmentRepository>();
builder.Services.AddScoped<IEmployeeRepository, EmployeeRepository>();
builder.Services.AddScoped<IComputerRepository, ComputerRepository>();
builder.Services.AddScoped<IDisplayMonitorRepository, DisplayMonitorRepository>();
builder.Services.AddScoped<IEquipmentRepository, EquipmentRepository>();
builder.Services.AddScoped<IMaintenanceRepository, MaintenanceRepository>();
builder.Services.AddScoped<ISoftwareLicenseRepository, SoftwareLicenseRepository>();

// �������
builder.Services.AddScoped<IOfficeService, OfficeService>();
builder.Services.AddScoped<IBuildingService, BuildingService>();
builder.Services.AddScoped<IDepartmentService, DepartmentService>();
builder.Services.AddScoped<IEmployeeService, EmployeeService>();
builder.Services.AddScoped<IComputerService, ComputerService>();
builder.Services.AddScoped<IMonitorService, MonitorService>();
builder.Services.AddScoped<IEquipmentService, EquipmentService>();
builder.Services.AddScoped<IMaintenanceService, MaintenanceService>();
builder.Services.AddScoped<ISoftwareLicenseService, SoftwareLicenseService>();
builder.Services.AddScoped<IServiceRequestService, ServiceRequestService>();
builder.Services.AddScoped<IReportService, ReportService>();

var app = builder.Build();

// ������������� ����� � �������������� ��� �������
using (var scope = app.Services.CreateScope())
{
    var services = scope.ServiceProvider;
    try
    {
        var context = services.GetRequiredService<OfficeDbContext>();
        var userManager = services.GetRequiredService<UserManager<ApplicationUser>>();
        var roleManager = services.GetRequiredService<RoleManager<IdentityRole>>();

        // ��������� ��������
        await context.Database.MigrateAsync();

        // �������������� ���� � ������
        await InitializeRolesAndAdmin(userManager, roleManager);
    }
    catch (Exception ex)
    {
        var logger = services.GetRequiredService<ILogger<Program>>();
        logger.LogError(ex, "������ ��� ������������� ���� ������");
    }
}

// Configure the HTTP request pipeline.
if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Home/Error");
    app.UseHsts();
}

app.UseHttpsRedirection();
app.UseStaticFiles();

app.UseRouting();

app.UseAuthentication();
app.UseAuthorization();

app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Office}/{action=Index}/{id?}");

app.Run();

// ����� ������������� ����� � ��������������
async Task InitializeRolesAndAdmin(UserManager<ApplicationUser> userManager, RoleManager<IdentityRole> roleManager)
{
    // ������� ����
    string[] roleNames = { "SysAdmin", "Manager", "Accountant", "User" };

    foreach (var roleName in roleNames)
    {
        var roleExist = await roleManager.RoleExistsAsync(roleName);
        if (!roleExist)
        {
            await roleManager.CreateAsync(new IdentityRole(roleName));
        }
    }

    // ������� �������������� �� ���������
    var adminEmail = "admin@company.com";
    var adminUser = await userManager.FindByEmailAsync(adminEmail);

    if (adminUser == null)
    {
        var admin = new ApplicationUser
        {
            UserName = adminEmail,
            Email = adminEmail,
            EmailConfirmed = true,
            FullName = "��������� �������������",
            CreatedAt = DateTime.Now,
            IsActive = true
        };

        var result = await userManager.CreateAsync(admin, "Admin123!");
        if (result.Succeeded)
        {
            await userManager.AddToRoleAsync(admin, "SysAdmin");
        }
    }
}