using Mango.GatewaySolution.Extensions;
using Ocelot.DependencyInjection;
using Ocelot.Middleware;

var builder = WebApplication.CreateBuilder(args);
builder.Configuration.AddJsonFile("ocelot.json",optional:false,reloadOnChange:true);
builder.Services.AddOcelot(builder.Configuration);
builder.AddAuthenticationAndAuthorization();  //extension method
var app = builder.Build();
app.UseOcelot().GetAwaiter().GetResult();
app.UseAuthentication();
app.UseAuthorization();

app.MapGet("/", () => "Hello World!");

app.Run();
