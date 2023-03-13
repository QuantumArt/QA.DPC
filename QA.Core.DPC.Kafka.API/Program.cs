using Microsoft.AspNetCore.Mvc.Formatters;
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
            builder.Logging.AddNLog("NLog.config");

            if (!builder.Configuration.GetSection("Kafka")
               .GetValue<bool>("IsEnabled"))
            {
                throw new ApplicationException("Kafka disabled in config.");
            }
            builder.Services.RegisterKafka(builder.Configuration);
            builder.Services.AddSingleton<IProducerService<string>, ProducerService<string>>();
            builder.Services.AddScoped<IKafkaService, KafkaService>();
            builder.Services.AddSingleton<IMessageModifierFactory, MessageModifierFactory>();
            builder.Services.AddSingleton<JsonMessageModifier>();
            builder.Services.AddSingleton<XmlMessageModifier>();

            WebApplication app = builder.Build();
            
            if (app.Environment.IsDevelopment())
            {
                app.UseSwagger();
                app.UseSwaggerUI();
            }

            app.UseAuthorization();
            app.MapControllers();

            app.Run();
        }
    }
}