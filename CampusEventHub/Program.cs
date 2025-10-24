    using CampusEventHub.Data;
    using CampusEventHub.Service;
    using Microsoft.EntityFrameworkCore;
    using Microsoft.AspNetCore.Http;
    using VNPAY.NET;

    var builder = WebApplication.CreateBuilder(args);

    builder.Services.AddDbContext<ApplicationDbContext>(options =>
        options.UseMySql(
            builder.Configuration.GetConnectionString("CampusEventHub"),
            new MySqlServerVersion(new Version(10, 11, 0)),
            mySqlOptions => mySqlOptions.EnableRetryOnFailure(5, TimeSpan.FromSeconds(10), null)
        ));

    
    builder.Services.AddScoped<SeatService>();
    builder.Services.AddScoped<MailService>();
    builder.Services.AddHttpClient<PayOSService>();

    builder.Services.AddScoped<IVnpay, Vnpay>();

    builder.Services.AddControllersWithViews();

    builder.Services.AddDistributedMemoryCache();

    builder.Services.AddSession(options =>
    {
        options.IdleTimeout = TimeSpan.FromMinutes(30);
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
    app.UseSession();
    app.UseAuthorization();

    app.MapStaticAssets();

    app.MapControllerRoute(
            name: "default",
            pattern: "{controller=Home}/{action=Index}/{id?}")
        .WithStaticAssets();

    app.Run();