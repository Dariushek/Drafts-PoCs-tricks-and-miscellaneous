using BusinessLogicModule;
using Microsoft.EntityFrameworkCore;

WebApplicationBuilder builder = WebApplication.CreateBuilder(args);

builder.Services.AddOpenApi();
builder.Services.AddBusinessLogicModule(options => options.UseSqlServer(
                                            builder.Configuration.GetConnectionString("BooksDb")
                                        )
);

WebApplication app = builder.Build();

app.Services.InitializeBusinessLogicModuleDatabase();

if (app.Environment.IsDevelopment())
    app.MapOpenApi();

app.MapBusinessLogicModule();

app.Run();

public partial class Program;