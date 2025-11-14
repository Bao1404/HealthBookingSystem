using BusinessObject.Models;
using HealthBookingSystem.Models;
using HealthBookingSystem.Helper;
using Microsoft.EntityFrameworkCore;
using Repositories;
using Repositories.Interface;
using Repositories.IRepositories;
using Repositories.Repositories;
using Services;
using Services.Interface;
using Services.Service;
using HealthBookingSystem.Service;

namespace HealthBookingSystem
{
    public class Program
    {
        public static void Main(string[] args)
        {
            var builder = WebApplication.CreateBuilder(args);

            // Add services to the container.
            builder.Services.AddControllersWithViews();

            builder.Services.AddHttpClient("APIClient", client =>
            {
                client.BaseAddress = new Uri("https://localhost:7127/api/");
                client.DefaultRequestHeaders.Add("Accept", "application/json");
            });

            builder.Services.AddDbContext<HealthCareSystemContext>(options =>
                options.UseSqlServer(builder.Configuration.GetConnectionString("HealthCareSystemContext")));

            builder.Services.Configure<OpenAIOptions>(builder.Configuration.GetSection("Gemini"));
            builder.Services.Configure<GmailApiOption>(builder.Configuration.GetSection("gmailApi"));
            builder.Services.Configure<CloudinarySettings>(builder.Configuration.GetSection("CloudinarySettings"));

            builder.Services.AddSingleton(x =>
            {
                var config = builder.Configuration.GetSection("CloudinarySettings").Get<CloudinarySettings>();
                var account = new CloudinaryDotNet.Account(config.CloudName, config.ApiKey, config.ApiSecret);
                return new CloudinaryDotNet.Cloudinary(account);
            });


            builder.Services.AddHttpClient();

            builder.Services.AddScoped<IUserRepository, UserRepository>();
            builder.Services.AddScoped<IPatientRepository, PatientRepository>();
            builder.Services.AddScoped<IDoctorRepository, DoctorRepository>();
            builder.Services.AddScoped<IAiMessageRepository, AiMessageRepository>();
            builder.Services.AddScoped<IAiConversationRepository, AiConversationRepository>();
            builder.Services.AddScoped<IAiConversationService, AiConversationService>();
            builder.Services.AddScoped<IAiMessageService, AiMessageService>();
            builder.Services.AddScoped<IMedicalHistoriesService, MedicalHistoriesService>();
            builder.Services.AddScoped<IMedicalHistoriesRepository, MedicalHistoriesRepository>();
            builder.Services.AddScoped<ITimeOffRepository, TimeOffRepository>();
            // Register repositories and services
            builder.Services.AddScoped<IUserService, UserService>();
            builder.Services.AddScoped<IPatientService, PatientService>();
            builder.Services.AddScoped<IDoctorService, DoctorService>();
            builder.Services.AddScoped<IUserRepository, UserRepository>();
            builder.Services.AddScoped<IDoctorRepository, DoctorRepository>();
            builder.Services.AddScoped<IPatientRepository, PatientRepository>();
            builder.Services.AddScoped<IMessageRepository, MessageRepository>();
            builder.Services.AddScoped<IConversationRepository, ConversationRepository>();
            builder.Services.AddControllersWithViews();
            builder.Services.AddRazorPages(); // ✅ Thêm dòng này để tránh lỗi
            builder.Services.AddScoped<IAppointmentService, AppointmentService>();
            builder.Services.AddScoped<IAppointmentRepository, AppointmentRepository>();
            builder.Services.AddScoped<ISpecialtyService, SpecialtyService>();
            builder.Services.AddScoped<ISpecialtyRepository, SpecialtyRepository>();
            builder.Services.AddScoped<ITimeOffService, TimeOffService>();

            builder.Services.AddScoped<GmailHelper>();
            builder.Services.AddScoped<PhotoService>();

            builder.Services.AddCors(options =>
            {
                options.AddPolicy("AllowAll", builder =>
                {
                    builder.AllowAnyOrigin()
                           .AllowAnyMethod()
                           .AllowAnyHeader();
                });
            });

            builder.Services.AddSession();
            builder.Services.AddSignalR();

            var app = builder.Build();

            // Configure the HTTP request pipeline.
            if (!app.Environment.IsDevelopment())
            {
                app.UseExceptionHandler("/Home/Error");
                app.UseHsts();
            }

            app.UseHttpsRedirection();
            app.UseStaticFiles();

            app.UseRouting();

            app.UseAuthorization();
            app.UseSession();

            // Map endpoints (must be after UseRouting)
            app.UseEndpoints(endpoints =>
            {
                endpoints.MapControllers();
                endpoints.MapHub<ChatHub>("/chathub");
                endpoints.MapRazorPages();
                endpoints.MapControllerRoute(
                    name: "default",
                    pattern: "{controller=Home}/{action=Index}/{id?}");
            });

            app.Run();
        }
    }
}
