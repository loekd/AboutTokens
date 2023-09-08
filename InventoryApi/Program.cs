namespace InventoryApi
{
    public class Program
    {
        public static void Main(string[] args)
        {
            Console.WriteLine("This is the Inventory API");

            var builder = WebApplication.CreateBuilder(args);

            builder.Services.AddHttpClient("Idp", client => 
            {
                client.BaseAddress = new Uri("https://localhost:7242");
            });

            // Add services to the container.
            builder.Services
                .AddOptions<IdentityServerOptions>()
                .BindConfiguration(IdentityServerOptions.ConfigurationSectionName)
                .ValidateDataAnnotations();


            builder.Services.AddControllers();
            // Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
            builder.Services.AddEndpointsApiExplorer();
            builder.Services.AddSwaggerGen();

            builder.ConfigureIdentityServer();

            builder.Services.AddCors(options =>
            {
                options.AddDefaultPolicy(policy =>
                {
                    //allow the frontend with tokens
                    policy.WithOrigins("https://localhost:7267")
                    .AllowAnyHeader()
                    .AllowAnyMethod();
                });
            });

            var app = builder.Build();

            // Configure the HTTP request pipeline.
            if (app.Environment.IsDevelopment())
            {
                app.UseSwagger();
                app.UseSwaggerUI();
            }
            app.UseCors();
            app.UseHttpsRedirection();
            app.UseAuthorization();
            app.MapControllers();
            app.Run();
        }
    }
}