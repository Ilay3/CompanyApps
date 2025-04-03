using CompanyApp.Infrastructure.InterfacesRepository;
using CompanyApp.Application.Mappings;
using CompanyApp.Application.Repositories;
using CompanyApp.Application.Services;
using CompanyApp.Core.Interfaces;
using CompanyApp.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;
using CompanyApp.Infrastructure.Repositories;

var builder = WebApplication.CreateBuilder(args);


// Добавление сервисов и конфигураций
builder.Services.AddRazorPages();
builder.Services.AddControllersWithViews();
builder.Services.AddAutoMapper(typeof(MappingProfile));
builder.Services.AddDbContext<OfficeDbContext>(options =>
    options.UseSqlServer(builder.Configuration.GetConnectionString("DefaultConnection")));

// Регистрация репозиториев и сервисов
builder.Services.AddScoped<IOfficeRepository, OfficeRepository>();
builder.Services.AddScoped<IBuildingRepository, BuildingRepository>();
builder.Services.AddScoped<IDepartmentRepository, DepartmentRepository>();
builder.Services.AddScoped<IEmployeeRepository, EmployeeRepository>();
builder.Services.AddScoped<IComputerRepository, ComputerRepository>();
builder.Services.AddScoped<IDisplayMonitorRepository, DisplayMonitorRepository>();
builder.Services.AddScoped<IEquipmentRepository, EquipmentRepository>();

builder.Services.AddScoped<IOfficeService, OfficeService>();
builder.Services.AddScoped<IBuildingService, BuildingService>();
builder.Services.AddScoped<IDepartmentService, DepartmentService>();
builder.Services.AddScoped<IEmployeeService, EmployeeService>();
builder.Services.AddScoped<IComputerService, ComputerService>();
builder.Services.AddScoped<IMonitorService, MonitorService>();
builder.Services.AddScoped<IEquipmentService, EquipmentService>();

var app = builder.Build();

if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Home/Error");
    app.UseHsts();
}

app.UseHttpsRedirection();
app.UseStaticFiles();
app.UseRouting();
app.UseAuthorization();

app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Office}/{action=Index}/{id?}");

app.Run();