using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Assignment3.Data;
//using ClassDemo.Data;
using Assignment3.Areas.Identity.Data;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.Services.AddDbContext<ApplicationDbContext>(options =>
    options.UseSqlServer(builder.Configuration.GetConnectionString("DefaultConnection")));

builder.Services.AddDefaultIdentity<IdentityUser>(options => options.SignIn.RequireConfirmedAccount = false)
    .AddRoles<IdentityRole>() // Add this line to enable role management
    .AddEntityFrameworkStores<ApplicationDbContext>();

builder.Services.AddHttpClient<AIAnalysisService>();

builder.Services.AddScoped<AIAnalysisService>();
builder.Services.AddScoped<MealGeneratorService>();

builder.Services.AddControllersWithViews();
builder.Services.AddRazorPages(); // Necessary for Identity UI

var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseDeveloperExceptionPage();
    // app.UseMigrationsEndPoint(); // Optional
}
else
{
    app.UseExceptionHandler("/Home/Error");
    app.UseHsts();
}

app.UseHttpsRedirection();
app.UseStaticFiles();

app.UseRouting();

app.UseAuthentication(); // Must come before UseAuthorization
app.UseAuthorization();

app.UseEndpoints(endpoints =>
{
    endpoints.MapControllerRoute(
        name: "default",
        pattern: "{controller=Home}/{action=Index}/{id?}");
});

app.MapRazorPages(); // Necessary for Identity UI

app.Run();
