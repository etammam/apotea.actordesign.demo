
using Apotea.Design.ActorModel.Services.IMessages;
using Apotea.Design.ActorModel.Services.ServicesDefault;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Orleans;

namespace Apotea.Design.ActorModel.Services.Barcode
{
    public class Program
    {
        public static void Main(string[] args)
        {
            var builder = WebApplication.CreateBuilder(args);

            builder.Services.AddAuthorization();
            builder.Services.AddEndpointsApiExplorer();
            builder.Services.AddSwaggerGen();

            builder.Services.AddOrleansClusterOptions();
            builder.Services.AddOrleansClusterSetup();

            var app = builder.Build();

            // Configure the HTTP request pipeline.
            if (app.Environment.IsDevelopment())
            {
                app.UseSwagger();
                app.UseSwaggerUI();
            }

            app.UseHttpsRedirection();

            app.UseAuthorization();


            app.MapGet("/api/get-weight/{id:int}", ([FromRoute] int id, IGrainFactory grainFactory) =>
            {
                var scaleService = grainFactory.GetGrain<IScaleGrain>(id);
                var weight = scaleService.GetCurrentWeight();
                return weight;
            })
            .WithName("get box weight")
            .WithOpenApi();

            app.Run();
        }
    }
}
