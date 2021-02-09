using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.DataProtection;
using Microsoft.AspNetCore.DataProtection.AuthenticatedEncryption;
using Microsoft.AspNetCore.DataProtection.AuthenticatedEncryption.ConfigurationModel;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.HttpsPolicy;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Microsoft.IdentityModel.Tokens;
using NetDevPack.Security.JwtSigningCredentials;
using NetDevPack.Security.JwtSigningCredentials.AspNetCore;
using NetDevPack.Security.JwtSigningCredentials.Jwk;
using DataTrust.Pod.Api.Infrastructure;

namespace DataTrust.Pod.Api
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
            services.AddControllers();
            services.AddMemoryCache();

            services
                .AddJwksManager(delegate(JwksOptions options)
                {
                    options.Algorithm = Algorithm.Create(SecurityAlgorithms.RsaSha512, KeyType.RSA);
                    options.KeyPrefix = "signingKey-";
                })
                .PersistKeysToFileSystem(FileSystemHelper.GetDirectory(@"/certs/rotation/"));

            services.AddDataProtection().PersistKeysToFileSystem(FileSystemHelper.GetDirectory(@"/certs/local/"))
                .SetDefaultKeyLifetime(TimeSpan.FromDays(60))
                .SetApplicationName("Api.Swarm")
                .UseCryptographicAlgorithms(new AuthenticatedEncryptorConfiguration()
                {
                    EncryptionAlgorithm = EncryptionAlgorithm.AES_256_CBC,
                    ValidationAlgorithm = ValidationAlgorithm.HMACSHA256
                });
                //.ProtectKeysWithCertificate(
                //new X509Certificate2("certificate.pfx", "password"));
        }

        // This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
        public void Configure(IApplicationBuilder app, IWebHostEnvironment env)
        {
            if (env.IsDevelopment())
            {
                app.UseDeveloperExceptionPage();
            }

            app.UseHttpsRedirection();

            app.UseRouting();

            app.UseAuthorization();

            app.UseEndpoints(endpoints =>
            {
                endpoints.MapControllers();
            });

            app.UseJwksDiscovery();
        }
    }
}
