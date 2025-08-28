using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using PTM2._0.Data;
using NLog;
using NLog.Web;
using PTM2._0.Filters; // 只保留这个命名空间
using Microsoft.AspNetCore.Authentication.Cookies;

// 加载NLog配置
var logger = LogManager.Setup()
    .LoadConfigurationFromFile("CfgFile/nlog.config")
    .GetCurrentClassLogger();

try
{
    var builder = WebApplication.CreateBuilder(args);

    // 配置NLog作为日志提供程序
    builder.Logging.ClearProviders();
    builder.Host.UseNLog();

    // 添加Session服务
    builder.Services.AddSession(options =>
    {
        options.IdleTimeout = TimeSpan.FromMinutes(30);
        options.Cookie.HttpOnly = true;
        options.Cookie.IsEssential = true;
    });
    
    // 添加HttpContext访问器
    builder.Services.AddHttpContextAccessor();
    
    // 添加Cookie认证服务
    builder.Services.AddAuthentication(CookieAuthenticationDefaults.AuthenticationScheme)
        .AddCookie(options => 
        {
            options.LoginPath = "/Login/Index";
            options.AccessDeniedPath = "/Home/AccessDenied";
        });
    
    // 注册自定义过滤器
    builder.Services.AddScoped<CustomActionFilter>();
    
    // 添加MVC服务并注册全局过滤器
    builder.Services.AddControllersWithViews(options =>
    {
        options.Filters.AddService<CustomActionFilter>();
    });

    // 添加数据库上下文
    builder.Services.AddDbContext<PTM2_0Context>(options =>
        options.UseSqlServer(builder.Configuration.GetConnectionString("PTM2_0Context") ??
        throw new InvalidOperationException("Connection string 'PTM2_0Context' not found.")));

    var app = builder.Build();

    // 配置HTTP请求管道
    if (!app.Environment.IsDevelopment())
    {
        app.UseExceptionHandler("/Home/Error");
        app.UseHsts();
    }
    
    app.UseHttpsRedirection();
    app.UseStaticFiles();
    app.UseRouting();
    
    // 添加认证中间件（关键）
    app.UseAuthentication();
    app.UseAuthorization();
    
    // 添加Session中间件
    app.UseSession();

    // 设置默认路由到登录页
    app.MapControllerRoute(
        name: "default",
        pattern: "{controller=Login}/{action=Index}/{id?}");

    app.Run();
}
catch (Exception ex)
{
    logger.Error(ex, "应用程序启动失败");
    throw;
}
finally
{
    LogManager.Shutdown();
}