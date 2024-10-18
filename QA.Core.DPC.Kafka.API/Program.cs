using Microsoft.AspNetCore.Mvc.Formatters;
using Microsoft.Extensions.Logging.Configuration;
using NLog.Web;
using QA.Core.DPC.Kafka.API.Factories;
using QA.Core.DPC.Kafka.API.Interfaces;
using QA.Core.DPC.Kafka.API.Services;
using QA.Core.DPC.Kafka.Extensions;
using QA.Core.DPC.Kafka.Interfaces;
using QA.Core.DPC.Kafka.Services;
using QA.DPC.Core.Helpers;

namespace QA.Core.DPC.Kafka.API
{
    public class Program
    {
        public static void Main(string[] args)
        {
            WebApplicationBuilder builder = WebApplication.CreateBuilder(args);
            builder.Services.AddControllers(options =>
            {
                options.InputFormatters.RemoveType<SystemTextJsonInputFormatter>();
                options.InputFormatters.Add(new TextUniversalInputFormatter());
            });
            builder.Services.AddEndpointsApiExplorer();
            builder.Services.AddSwaggerGen();
            builder.Logging
                .ClearProviders()
                .SetMinimumLevel(LogLevel.Trace)
                .AddConfiguration(builder.Configuration.GetSection("Logging"))
                .AddNLog("NLog.config");

            builder.Services.RegisterKafka(builder.Configuration);
            builder.Services.AddSingleton<IProducerService<string>, ProducerService<string>>();
            builder.Services.AddScoped<IKafkaService, KafkaService>();
            builder.Services.AddSingleton<IMessageModifierFactory, MessageModifierFactory>();
            builder.Services.AddSingleton<JsonMessageModifier>();
            builder.Services.AddSingleton<XmlMessageModifier>();
            var name = builder.Configuration["Properties:Name"];

            WebApplication app = builder.Build();
            
            app.UseSwagger();
            app.UseSwaggerUI();

            app.UseAuthorization();
            app.MapControllers();

            app.Run();
            
            app.Logger.LogInformation("{appName} started", name);
        }
    }
}