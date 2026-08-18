var builder = WebApplication.CreateBuilder(args);

// 1. Session Servisini Ekle
builder.Services.AddControllersWithViews();
builder.Services.AddDistributedMemoryCache();
builder.Services.AddSession(options =>

{
    options.IdleTimeout = TimeSpan.FromMinutes(30); // 30 dakika iþlem yapýlmazsa oturumu sonlandýr
    options.Cookie.HttpOnly = true;
    options.Cookie.IsEssential = true;
});

var app = builder.Build();

if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Home/Error");
    app.UseHsts();
}

app.UseHttpsRedirection();
app.UseStaticFiles();

app.UseRouting();

// 2. Session Middleware'ini Aktif Et (UseRouting ile UseAuthorization arasýnda olmalý)
app.UseSession();

app.UseAuthorization();

app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Home}/{action=Index}/{id?}");

app.Run();