using System.Reflection.Metadata;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.OpenApi.Models;
using TodoApi;

var builder = WebApplication.CreateBuilder(args);

//servicees- הוספת הקשר לבסיס הנתונים בהזרקה ל
var connectionString = builder.Configuration.GetConnectionString("ToDoDB");
builder.Services.AddDbContext<ToDoDbContext>(options =>
    options.UseMySql(connectionString, ServerVersion.AutoDetect(connectionString)));

builder.Services.AddControllers();

// הוספת Swagger
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(options =>
{
    options.SwaggerDoc("v1", new OpenApiInfo
    {
        Version = "v1",
        Title = "ToDo API",
        Description = "An ASP.NET Core Web API for managing ToDo items",
        // TermsOfService = new Uri("https://example.com/terms"),
        // Contact = new OpenApiContact
        // {
        //     Name = "Example Contact",
        //     // Url = new Uri("https://example.com/contact")
        // },
        // License = new OpenApiLicense
        // {
        //     Name = "Example License",
        //     // Url = new Uri("https://example.com/license")
        // }
    });
});

//cors- פתירת בעית ה
builder.Services.AddCors(options =>
{
    options.AddPolicy("CorsPolicy", builder =>
    {
        builder.AllowAnyOrigin()
               .AllowAnyMethod()
               .AllowAnyHeader();
    });
});

var app = builder.Build();

app.UseCors("CorsPolicy");

// if (app.Environment.IsDevelopment())
// {
//     app.UseSwagger();
//     app.UseSwaggerUI(c =>
//     {
//         c.SwaggerEndpoint("/swagger/v1/swagger.json", "ToDo API v1");
//         c.RoutePrefix = string.Empty;
//     });
// }

// שליפת כל המשימות
app.MapGet("/Tasks", async (ToDoDbContext db) =>
{
    return await db.Items.ToListAsync();
});

// הוספת משימה
app.MapPost("/Tasks", async (ToDoDbContext db, Item item) =>
{
    db.Items.Add(item);
    await db.SaveChangesAsync();
    return await db.Items.ToListAsync();
});

// עדכון משימה
app.MapPatch("/Tasks/{id}", async (ToDoDbContext db, int id, Item item) =>
{
    var obj = await db.Items.FindAsync(id);
    if (obj == null)
        return Results.NotFound();
    if (item.Name != "")
        obj.Name = item.Name;
    if (item.IsComplete != null)
    obj.IsComplete = item.IsComplete;

    await db.SaveChangesAsync();
    return Results.Ok(obj);
});

// מחיקת משימה
app.MapDelete("/Tasks/{id}", async (ToDoDbContext db, int id) =>
{
    var obj = await db.Items.FindAsync(id);
    if (obj == null)
        return Results.NotFound();
    db.Remove(obj);

    await db.SaveChangesAsync();
    return Results.Ok();
});

app.MapGet("/", ()=> "ToDoList_Server is running!");

app.Run();
