using System;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.AspNetCore.TestHost;
using DataTrust.Pod.Api;

namespace DataTrust.Pod.Api.Tests
{
    public class CustomWebApplicationFactory<TStartup> : WebApplicationFactory<Startup>
    {
        protected override void ConfigureWebHost(IWebHostBuilder builder)
        {
            builder
                .UseSolutionRelativeContentRoot("DataTrust.Pod.Api")
                .ConfigureServices(services =>
                {
                    
                });
        }
    }
}
