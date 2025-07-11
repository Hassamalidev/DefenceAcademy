using DefenceAcademy;
using DefenceAcademy.Repo;
using DefenceAcademy.Repo.Notes;
using System.Data;
using System.Data.SqlClient;
var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.Services.AddScoped<IDbConnection>(sp => new SqlConnection(builder.Configuration.GetConnectionString("DefaultConnection")));
builder.Services.AddSingleton<DapperContext>();
builder.Services.AddScoped<IQuestion, QuestionService>();
builder.Services.AddScoped<INote, NoteService>();

builder.Services.AddControllers();
// Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();
builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowAll", policy =>
    {
        policy.AllowAnyOrigin()
              .AllowAnyMethod()
              .AllowAnyHeader();
    });
});

var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}


app.UseCors("AllowAll");

app.UseHttpsRedirection();

app.UseAuthorization();

app.MapControllers();

app.Run();
