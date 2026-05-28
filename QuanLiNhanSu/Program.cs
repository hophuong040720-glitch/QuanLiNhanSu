using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Authentication.Cookies;
using QuanLiNhanSu.Models;
using QuanLiNhanSu.Services;

var builder = WebApplication.CreateBuilder(args);

// 1. Lấy chuỗi kết nối từ file appsettings.json
var connectionString = builder.Configuration.GetConnectionString("DefaultConnection");

// 2. Đăng ký AppDbContext
builder.Services.AddDbContext<AppDbContext>(options =>
    options.UseSqlServer(connectionString));

// 3. Đăng ký Cookie Authentication
builder.Services.AddAuthentication(CookieAuthenticationDefaults.AuthenticationScheme)
    .AddCookie(options =>
    {
        options.LoginPath = "/Account/Login";
        options.AccessDeniedPath = "/Account/AccessDenied";
        options.SlidingExpiration = true;
        options.ExpireTimeSpan = TimeSpan.FromHours(8); // Phiên đăng nhập 8 tiếng
    });

// 4. Đăng ký các Custom Services (DI)
builder.Services.AddScoped<PasswordService>();
builder.Services.AddScoped<AuditService>();

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

// =================== TỰ ĐỘNG SEED DỮ LIỆU MẶC ĐỊNH ===================
using (var scope = app.Services.CreateScope())
{
    var context = scope.ServiceProvider.GetRequiredService<AppDbContext>();
    var pwdService = scope.ServiceProvider.GetRequiredService<PasswordService>();

    try
    {
        // 1. Tự động cập nhật & cấp quyền Admin cho các tài khoản cốt lõi
        var adminAccounts = new[] { "admin", "huy", "phuong" };
        foreach (var uname in adminAccounts)
        {
            var acc = context.Users.FirstOrDefault(u => u.Username == uname);
            if (acc != null)
            {
                acc.Role = "Admin";
                // Auto-hash mật khẩu cũ nếu chưa hash
                if (!pwdService.IsHashed(acc.Password))
                    acc.Password = pwdService.HashPassword(acc.Password);
            }
        }

        // 2. Tạo tài khoản phuong nếu chưa có
        if (!context.Users.Any(u => u.Username == "phuong"))
        {
            context.Users.Add(new User
            {
                Username = "phuong",
                Password = pwdService.HashPassword("123"),
                Role = "Admin",
                Email = "phuong@gmail.com"
            });
        }

        context.SaveChanges();

        // 3. Tự động đổ nhân sự mẫu nếu DB trắng
        if (!context.Employees.Any())
        {
            // Lấy ID phòng ban và chức vụ từ seed data
            var pbIT = context.PhongBans.FirstOrDefault(p => p.TenPB == "IT")?.MaPB;
            var pbKT = context.PhongBans.FirstOrDefault(p => p.TenPB == "Kế toán")?.MaPB;
            var pbNS = context.PhongBans.FirstOrDefault(p => p.TenPB == "Nhân sự")?.MaPB;
            var pbKD = context.PhongBans.FirstOrDefault(p => p.TenPB == "Kinh doanh")?.MaPB;

            var cvDev = context.ChucVus.FirstOrDefault(c => c.TenCV == "Developer")?.MaCV;
            var cvKeToan = context.ChucVus.FirstOrDefault(c => c.TenCV == "Chuyên viên")?.MaCV;
            var cvSale = context.ChucVus.FirstOrDefault(c => c.TenCV == "Sale Manager")?.MaCV;
            var cvNV = context.ChucVus.FirstOrDefault(c => c.TenCV == "Nhân viên")?.MaCV;

            context.Employees.AddRange(
                new Employee { MaNV = "NV001", HoTen = "Ngô Gia Huy",    PhongBanId = pbIT, ChucVuId = cvDev,    Luong = 25000000 },
                new Employee { MaNV = "NV002", HoTen = "Hồ Thúy Phượng", PhongBanId = pbIT, ChucVuId = cvDev,    Luong = 25000000 },
                new Employee { MaNV = "NV003", HoTen = "Trần Văn A",      PhongBanId = pbKT, ChucVuId = cvKeToan, Luong = 20000000 },
                new Employee { MaNV = "NV004", HoTen = "Lê Thị B",        PhongBanId = pbNS, ChucVuId = cvNV,     Luong = 18000000 },
                new Employee { MaNV = "NV005", HoTen = "Phạm Văn C",      PhongBanId = pbKD, ChucVuId = cvSale,   Luong = 22000000 }
            );
            context.SaveChanges();
        }
    }
    catch (System.Exception ex)
    {
        System.Console.WriteLine($"Lỗi Seed Data: {ex.Message}");
    }
}
// =====================================================================

app.MapControllers();

app.Run();