

using SeedACloud.Grpc;
using Microsoft.EntityFrameworkCore;

var builder = WebApplication.CreateBuilder(args);

// Additional configuration is required to successfully run gRPC on macOS.
// For instructions on how to configure Kestrel and gRPC clients on macOS, visit https://go.microsoft.com/fwlink/?linkid=2099682

// Add services to the container.
builder.Services.AddGrpc();
builder.Services.AddDbContext<CourseDbContext>(options => options.UseSqlite("Data Source=Data\\coursedb.sqlite"));

builder.Services.AddGrpcReflection();

//CORS
builder.Services.AddCors(o => o.AddPolicy("GWCORS_POLICY", builder =>
{
    builder.AllowAnyOrigin()
           .AllowAnyMethod()
           .AllowAnyHeader()
           .WithExposedHeaders("Grpc-Status", "Grpc-Message", "Grpc-Timeout", "Grpc-Encoding", "Grpc-Accept-Encoding");
}));


var app = builder.Build();
app.UseHttpsRedirection();
app.MapGrpcService<GrpcCourseService>().RequireCors("GWCORS_POLICY").EnableGrpcWeb();
app.UseCors();
app.MapGet("/", async context =>
{
    await context.Response.WriteAsync("Communication with gRPC endpoints must be made through a gRPC client. To learn how to create a client, visit: https://go.microsoft.com/fwlink/?linkid=2086909");
});

if (app.Environment.IsDevelopment())
{
    app.MapGrpcReflectionService();
}



app.Run();
