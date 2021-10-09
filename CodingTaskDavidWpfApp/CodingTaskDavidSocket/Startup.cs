using CodingTaskDavidSocketModels;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.WebSockets;
using System.Text;
using System.Threading.Tasks;
using System.Text.Json;

namespace CodingTaskDavidSocket
{
    public class Startup
    {
        // This method gets called by the runtime. Use this method to add services to the container.
        // For more information on how to configure your application, visit https://go.microsoft.com/fwlink/?LinkID=398940
        public void ConfigureServices(IServiceCollection services)
        {
        }

        // This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
        public void Configure(IApplicationBuilder app, IWebHostEnvironment env)
        {
            if (env.IsDevelopment())
            {
                app.UseDeveloperExceptionPage();
            }

            app.UseRouting();

            var wsOptions = new WebSocketOptions { KeepAliveInterval = TimeSpan.FromSeconds(120) };
            app.UseWebSockets(wsOptions);
            app.Use(async (context, next) =>
            {
                if (context.Request.Path == "/send")
                {
                    if (context.WebSockets.IsWebSocketRequest)
                    {
                        using (WebSocket webSocket = await context.WebSockets.AcceptWebSocketAsync())
                        {
                            await Send(context, webSocket);
                        }
                    }
                    else
                    {
                        context.Response.StatusCode = (int)HttpStatusCode.BadRequest;
                    }
                }
            });
        }
        private async Task Send(HttpContext context, WebSocket webSocket)
        {
            var buffer = new byte[1024 * 4];
            WebSocketReceiveResult result = await webSocket.ReceiveAsync(new ArraySegment<byte>(buffer), System.Threading.CancellationToken.None);
            if (result != null)
            {
                while (!result.CloseStatus.HasValue)
                {
                    string msg = Encoding.UTF8.GetString(new ArraySegment<byte>(buffer, 0, result.Count));
                    Console.WriteLine($"Cliend says: {msg}");

                    Random random = new Random();
                    List<ListModel> listModels = new List<ListModel>();

                    listModels.Add(new ListModel
                    {
                        dateTime = DateTime.Now.ToString("G"),
                        stockName = "Stock 1",
                        price = (random.NextDouble() * (270.00 - 240.00) + 240.00).ToString("0.00")
                    });

                    listModels.Add(new ListModel
                    {
                        dateTime = DateTime.Now.ToString("G"),
                        stockName = "Stock 2",
                        price = (random.NextDouble() * (210.00 - 180.00) + 180.00).ToString("0.00")
                    });

                    //listModels.Add(new ListModel
                    //{
                    //    dateTime = DateTime.Now.ToString("G"),
                    //    stockName = "Stock 3",
                    //    price = (random.NextDouble() * (210.00 - 180.00) + 180.00).ToString("0.00")
                    //});
                    string jsonString = JsonSerializer.Serialize(listModels);
                    await webSocket.SendAsync(new ArraySegment<byte>(Encoding.UTF8.GetBytes(jsonString)), result.MessageType, result.EndOfMessage, System.Threading.CancellationToken.None);
                    result = await webSocket.ReceiveAsync(new ArraySegment<byte>(buffer), System.Threading.CancellationToken.None);

                }
            }
            await webSocket.CloseAsync(result.CloseStatus.Value, result.CloseStatusDescription, System.Threading.CancellationToken.None);
        }
    }
}
