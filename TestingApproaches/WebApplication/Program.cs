using BusinessLogicModule;

WebApplicationBuilder builder = WebApplication.CreateBuilder(args);

builder.Services.AddOpenApi();
builder.Services.AddBusinessLogicModule();

WebApplication app = builder.Build();

if (app.Environment.IsDevelopment())
    app.MapOpenApi();

app.MapBusinessLogicModule();

app.Run();

public partial class Program;