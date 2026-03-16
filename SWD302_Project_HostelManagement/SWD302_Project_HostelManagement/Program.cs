using Microsoft.EntityFrameworkCore;

namespace SWD302_Project_HostelManagement
{
    public class Program
    {
        public static async Task Main(string[] args)
        {
            var builder = WebApplication.CreateBuilder(args);

            // Add services to the container.
            builder.Services.AddControllersWithViews();

            // Add DbContext - Choose database provider based on environment
            builder.Services.AddDbContext<SWD302_Project_HostelManagement.Data.AppDbContext>(options =>
            {
                // ===== POSTGRESQL (Production - Supabase) =====
                // Uncomment when ready to deploy to production
                //options.UseNpgsql(
                //    builder.Configuration.GetConnectionString("DefaultConnection"),
                //    npgsqlOptions =>
                //    {
                //        // Specify PostgreSQL version to avoid reading internal Supabase schemas
                //        npgsqlOptions.SetPostgresVersion(15, 0);
                //    }
                //);

                // ===== SQL SERVER (Development - Local) =====
                // Using local SQL Server for migrations and development
                options.UseSqlServer(
                    builder.Configuration.GetConnectionString("DefaultConnectionSqlServer")
                );
            });

            var app = builder.Build();

            // Seed data chỉ chạy trong môi trường Development
            if (app.Environment.IsDevelopment())
            {
                await SWD302_Project_HostelManagement.Data.DbSeeder.SeedAsync(app.Services);
            }

            // Configure the HTTP request pipeline.
            if (!app.Environment.IsDevelopment())
            {
                app.UseExceptionHandler("/Home/Error");
                // The default HSTS value is 30 days. You may want to change this for production scenarios, see https://aka.ms/aspnetcore-hsts.
                app.UseHsts();
            }

            app.UseHttpsRedirection();
            app.UseStaticFiles();

            app.UseRouting();

            app.UseAuthorization();

            app.MapControllerRoute(
                name: "default",
                pattern: "{controller=Home}/{action=Index}/{id?}");

            app.Run();
        }
    }
}
