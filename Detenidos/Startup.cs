using AutoMapper;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.IdentityModel.Tokens;
using Microsoft.OpenApi.Models;
using System;
using System.Text;
using Detenidos.Models;
using Detenidos.Utilidades;
using Detenidos.Utilidades.Filters;
using Detenidos.Utilidades.Interfaces;
using System.IdentityModel.Tokens.Jwt;
using NetTopologySuite.Geometries;
using NetTopologySuite;
using Detenidos.Utilidades.Middlewares;
using Detenidos.Services;
using Microsoft.Extensions.Options;


namespace Detenidos
{
	public class Startup
	{
		// En la configuración de ILogger se pueden usar los siguientes valores
		// Critical, Error, Warning, Information, Debug, Trace
		public Startup(IConfiguration configuration)
		{
			JwtSecurityTokenHandler.DefaultInboundClaimTypeMap.Clear();
			Configuration = configuration;
		}

		public IConfiguration Configuration { get; }

		// This method gets called by the runtime. Use this method to add services to the container.
		public void ConfigureServices(IServiceCollection services)
		{
			// Se configura automapper para la transnformación de datos DTO
			services.AddAutoMapper(typeof(Startup));

			// Se define la fábrica de datos geométricos
			services.AddSingleton(provider =>
				new MapperConfiguration(config => {
					GeometryFactory geometryFactory = provider.GetRequiredService<GeometryFactory>();
					config.AddProfile(new AutoMapperProfiles(geometryFactory));
				}).CreateMapper()
			);

			// Configuración de almacenamiento en Azure Storage
			services.AddTransient<IFileStorage, FileStorageSystem>();
			
			services.AddHttpContextAccessor();
			
			// Se habilita conexión a DB
			services.AddDbContext<ApplicationDbContext>(options => options.UseSqlServer(Configuration.GetConnectionString("DataBaseConnection"), sqlServer => sqlServer.UseNetTopologySuite()));

			// Se habilita la fábrica de datos geométricos
			services.AddSingleton(NtsGeometryServices.Instance.CreateGeometryFactory(srid: 4326));

			/*-----MongoDB-------*/
			services.Configure<MongoDatabaseSettings>(
				Configuration.GetSection(nameof(MongoDatabaseSettings)));	

			services.AddSingleton<IMongoDatabaseSettings>(sp =>
				 sp.GetRequiredService<IOptions<MongoDatabaseSettings>>().Value);

			services.AddScoped<DetenidoService>();
			services.AddSingleton<AuditoriaMongoService>();
			services.AddScoped<MailService>();
			services.AddSingleton<GestionCenapiService>();


			// Solo funciona para peticiones Web
			// Para recibir peticiones de otros dominios
			services.AddCors(options =>
			{
				var frontendURL = Configuration.GetValue<string>("frontend_url");
				options.AddDefaultPolicy(builder =>
				{
					builder.WithOrigins(frontendURL).AllowAnyMethod().AllowAnyHeader()
					.WithExposedHeaders(new string[] { "cantidadTotalRegistros" });
				});
			});

			// Se agrega un mensaje por defecto cuando no haya valor válido para algún campo de cualquier formulario.s
			services.AddRazorPages()
			.AddMvcOptions(options =>
			{
				options.ModelBindingMessageProvider.SetValueMustNotBeNullAccessor(
					_ => "El campo es requerido con un valor válido.");
			});

			// Se configura Identity
			services.AddIdentity<ApplicationUser, ApplicationRole>(options =>
			{
				options.SignIn.RequireConfirmedAccount = false;
				options.Password.RequireDigit = false;
				options.Password.RequiredLength = 6;
				options.Password.RequireLowercase = false;
				options.Password.RequireNonAlphanumeric = false;
				options.Password.RequireUppercase = false;

				options.Lockout.DefaultLockoutTimeSpan = TimeSpan.FromDays(1);
				options.Lockout.MaxFailedAccessAttempts = 6;
				options.Lockout.AllowedForNewUsers = true;
			})
				.AddEntityFrameworkStores<ApplicationDbContext>()
				.AddDefaultTokenProviders();

			// Manejador de Jwt
			//services.AddTransient<IJwtHandler, JwtHandler>();

			// Se agrega Middlware para gestionar la autenticación
			services.AddTransient<TokenManagerMiddleware>();
			services.AddTransient<ITokenManager, TokenManager>();
			//services.AddSingleton<IHttpContextAccessor, HttpContextAccessor>();

			// Configuracion de Redis
			services.AddDistributedRedisCache(r =>
			{
				r.Configuration = Configuration["redis:connectionString"];
			});

			// Se habilita autenticación por Web Tokens
			var jwtSection = Configuration.GetSection("jwt");
			var jwtOptions = new JwtOptions();
			jwtSection.Bind(jwtOptions);
			services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
				.AddJwtBearer(options =>
				{
					options.TokenValidationParameters = new TokenValidationParameters
					{
						ValidIssuer = jwtOptions.Issuer,
						ValidateIssuer = jwtOptions.ValidateIssuer,
						ValidateAudience = jwtOptions.ValidateAudience,
						ValidateLifetime = jwtOptions.ValidateLifetime,
						ValidateIssuerSigningKey = jwtOptions.ValidateIssuerSigningKey,
						IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(jwtOptions.SecretKey)),
						ClockSkew = TimeSpan.Zero
					};
				});
			services.Configure<JwtOptions>(jwtSection);

			services.AddControllers(options =>
			{
				options.Filters.Add(typeof(ExceptionFilter));
			});
			services.AddSwaggerGen(c =>
			{
				c.SwaggerDoc("v1", new OpenApiInfo { Title = "Detenidos", Version = "v1" });
			});
		}

		// This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
		public void Configure(IApplicationBuilder app, IWebHostEnvironment env)
		{
			if (env.IsDevelopment())
			{
				app.UseDeveloperExceptionPage();
				app.UseSwagger();
				app.UseSwaggerUI(c => c.SwaggerEndpoint("/swagger/v1/swagger.json", "Detenidos v1"));
			}

			app.UseStaticFiles();

			app.UseRouting();

			app.UseCors();

			app.UseAuthentication();

			// Para configurar el Middleware de la gestión de los token
			app.UseMiddleware<TokenManagerMiddleware>();

			app.UseAuthorization();

			app.UseEndpoints(endpoints =>
			{
				endpoints.MapControllers();
			});
		}
	}
}
