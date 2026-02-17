using Microsoft.EntityFrameworkCore;
using StargateAPI.Business.Behaviors;
using StargateAPI.Business.Commands;
using StargateAPI.Business.Data;
using StargateAPI.Business.Services;
using StargateAPI.Middleware;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.

builder.Services.AddControllers();
// Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();
builder.Services.AddDbContext<StargateContext>(options =>
    options.UseSqlite(builder.Configuration.GetConnectionString("StarbaseApiDatabase")));

/*
 * Register logging service for database-backed API logging.
 * Scoped lifetime ensures one instance per HTTP request.
 */
builder.Services.AddScoped<IApiLoggingService, ApiLoggingService>();

builder.Services.AddMediatR(cfg =>
{
    cfg.AddRequestPreProcessor<CreatePersonPreProcessor>();
    cfg.AddRequestPreProcessor<CreateAstronautDutyPreProcessor>();
    cfg.AddOpenBehavior(typeof(LoggingBehavior<,>));
    cfg.RegisterServicesFromAssemblies(typeof(Program).Assembly);
});

var app = builder.Build();

// Exception handling
app.UseMiddleware<ExceptionHandlingMiddleware>();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseHttpsRedirection();

app.UseAuthorization();

app.MapControllers();

app.Run();


