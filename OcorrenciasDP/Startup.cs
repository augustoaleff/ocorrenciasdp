using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.HttpsPolicy;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using OcorrenciasDP.Database;

namespace OcorrenciasDP
{
    public class Startup
    {
        public Startup(IConfiguration configuration)
        {
            Configuration = configuration;
        }

        public IConfiguration Configuration { get; }

        // This method gets called by the runtime. Use this method to add services to the container.
        public void ConfigureServices(IServiceCollection services)
        {
            services.Configure<CookiePolicyOptions>(options =>
            {
                // This lambda determines whether user consent for non-essential cookies is needed for a given request.
                options.CheckConsentNeeded = context => true;
                options.MinimumSameSitePolicy = SameSiteMode.None;
            });

            services.AddDbContext<DatabaseContext>(options =>
            {
                //options.UseSqlServer("Driver={SQL Server Native Client 10.0};Server=tcp:lz72uivirk.database.windows.net,1433;Database=ELETRO_AZURE;Uid=eletro_az@lz72uivirk;Pwd=if360alWK;Encrypt=yes;Connection Timeout=30;");
                 options.UseSqlServer("Server=tcp:lz72uivirk.database.windows.net,1433;Database=ELETRO_AZURE;Uid=eletro_az@lz72uivirk;Pwd=if360alWK;Encrypt=yes;Connection Timeout=30;");


            });

            services.AddMvc().SetCompatibilityVersion(CompatibilityVersion.Version_2_1); //Adiciona o serviço do MVC
        }

        // This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
        public void Configure(IApplicationBuilder app, IHostingEnvironment env)
        {
            if (env.IsDevelopment())
            {  //Se estiver no ambiente de desenvolvimento
                app.UseDeveloperExceptionPage();
            }
            else
            { //Se estiver no ambiente de Produção
                app.UseExceptionHandler("/Home/Error"); //Redireciona para a página de Erro
                app.UseHsts();
            }

            app.UseHttpsRedirection();
            app.UseStaticFiles(); //Usa os arquivos estáticos como imagens e documentos
            app.UseCookiePolicy();

            app.UseMvc(routes =>
            { //Habilita o uso do MVC
                routes.MapRoute(
                    name: "default",
                    template: "{controller=Home}/{action=Index}/{id?}");
            });
        }
    }
}
