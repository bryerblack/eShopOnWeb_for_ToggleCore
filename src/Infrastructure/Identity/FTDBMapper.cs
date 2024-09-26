using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Configuration;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using Newtonsoft.Json;
using ToggleCoreLibrary.Contexts;
using ToggleCoreLibrary.Models;
using ToggleCoreLibrary.Operations;
using Unleash;
using Unleash.Internal;

namespace Microsoft.eShopWeb.Infrastructure.Identity;
public class FTDBMapper : FeatureToggleMapper
{

    //public ApplicationDbContext Context { get; set; }
    //public static NameValueCollection section = (NameValueCollection)ConfigurationManager.GetSection("FeatureToggleConfig");
    //public readonly DbContextOptions<ApplicationDbContext> contextOptions = new DbContextOptionsBuilder<ApplicationDbContext>()
    //   .UseSqlServer(@$"Server={section["server"]};Database={section["database"]};ConnectRetryCount=0;Integrated Security={section["integratedSecurity"]};TrustServerCertificate={section["trustServerCertificate"]}")
    //   .Options;
    
    

    public override FeatureToggleModel Map(string featureToggleId)
    {
        var settings = new UnleashSettings()
        {
            AppName = "dotnet-tutorial",
            UnleashApi = new Uri("http://localhost:4242/api/"),
            CustomHttpHeaders = new Dictionary<string, string>() { {"Authorization","default:development.unleash-insecure-api-token" } }
        };

        DefaultUnleash unleash = new DefaultUnleash(settings);
        var featureToggle = unleash.FeatureToggles.FirstOrDefault(x => x.Name.Equals(featureToggleId));

        if (featureToggle == null)
        {
            return new FeatureToggleModel()
            {
                ToggleId = featureToggleId,
                Toggle = false,

            };
        }

        return new FeatureToggleModel(
            featureToggle.Name,
            featureToggle.Enabled,
            null,
            null,
            null);
    }
}
