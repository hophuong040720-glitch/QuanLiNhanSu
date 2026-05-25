using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Authentication.Cookies;
using QuanLiNhanSu.Models;

var builder = WebApplication.CreateBuilder(args);

// 1. Lấy chuỗi kết nối từ file appsettings.json
var connectionString = builder.Configuration.GetConnectionString("DefaultConnection");

// 2. Đăng ký AppDbContext vào hệ thống
builder.Services.AddDbContext<AppDbContext>(options =>
    options.UseSqlServer(connectionString));

// 3. Đăng ký cơ chế Bảo mật bằng Cookie
builder.Services.AddAuthentication(CookieAuthenticationDefaults.AuthenticationScheme)
    .AddCookie(options =>
    {
        options.LoginPath = "/Account/Login";
        options.AccessDeniedPath = "/Account/AccessDenied";
    });

builder.Services.AddControllersWithViews();

var app = builder.Build();

// Configure the HTTP request pipeline.
if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Home/Error");
    app.UseHsts();
}

app.UseHttpsRedirection();
app.UseRouting();

// THỨ TỰ CỰC KỲ QUAN TRỌNG: Authentication phải đứng TRƯỚC Authorization
app.UseAuthentication();
app.UseAuthorization();

app.MapStaticAssets();

app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Account}/{action=Login}/{id?}");

// =================== TỰ ĐỘNG CẤP QUYỀN & NẠP DỮ LIỆU (SEED DATA) ===================
using (var scope = app.Services.CreateScope())
{
    var context = scope.ServiceProvider.GetRequiredService<AppDbContext>();
    try
    {
        //context.Database.EnsureCreated();

        // 1. Tự động chuẩn hóa & Nâng quyền cho acc 'admin' và 'huy' lên Admin
        var adminAcc = context.Users.FirstOrDefault(u => u.Username == "admin");
        if (adminAcc != null) adminAcc.Role = "Admin";

        var huyAcc = context.Users.FirstOrDefault(u => u.Username == "huy");
        if (huyAcc != null) huyAcc.Role = "Admin";

        if (!context.Users.Any(u => u.Username == "phuong"))
        {
            context.Users.Add(new User
            {
                Username = "phuong",
                Password = "123",
                Role = "Admin",
                Email = "phuong@gmail.com"
            });
        }
        // Cập nhật lưu các thay đổi Role trước
        context.SaveChanges();

        // 2. Tự động đổ 5 nhân sự mẫu nếu Database đang trắng (Phục vụ vẽ biểu đồ)
        if (!context.Employees.Any())
        {
            context.Employees.AddRange(
                new Employee { MaNV = "NV001", HoTen = "Ngô Gia Huy", PhongBan = "IT", ChucVu = "Developer", Luong = 25000000 },
                new Employee { MaNV = "NV002", HoTen = "Hồ Thúy Phượng", PhongBan = "IT", ChucVu = "Developer", Luong = 25000000 },
                new Employee { MaNV = "NV003", HoTen = "Trần Văn A", PhongBan = "Kế toán", ChucVu = "Kế toán trưởng", Luong = 20000000 },
                new Employee { MaNV = "NV004", HoTen = "Lê Thị B", PhongBan = "Nhân sự", ChucVu = "Chuyên viên", Luong = 18000000 },
                new Employee { MaNV = "NV005", HoTen = "Phạm Văn C", PhongBan = "Kinh doanh", ChucVu = "Sale Manager", Luong = 22000000 }
            );
            context.SaveChanges();
        }
    }
    catch (System.Exception ex)
    {
        System.Console.WriteLine($"Lỗi nạp Seed Data: {ex.Message}");
    }
}
// ===================================================================================
app.MapControllers();

app.Run();