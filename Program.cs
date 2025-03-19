using System.Reflection.Metadata;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.OpenApi.Models;
using TodoApi;

var builder = WebApplication.CreateBuilder(args);

//servicees- הוספת הקשר לבסיס הנתונים בהזרקה ל
var connectionString = builder.Configuration.GetConnectionString("ToDoDB");
builder.Services.AddDbContext<ToDoDbContext>(options =>
    // options.UseMySql(connectionString, ServerVersion.AutoDetect(connectionString)));
    options.UseMySql(connectionString, new MySqlServerVersion(new Version(8, 0, 22))));

builder.Services.AddControllers();

builder.Services.AddEndpointsApiExplorer();

// הוספת Swagger
builder.Services.AddSwaggerGen(options =>
{
    options.SwaggerDoc("v1", new OpenApiInfo
    {
        Version = "v1",
        Title = "ToDo API",
        Description = "An ASP.NET Core Web API for managing ToDo items",
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
    app.UseSwagger();
    // app.UseSwaggerUI();
    app.UseSwaggerUI(c =>
    {
        c.SwaggerEndpoint("/swagger/v1/swagger.json", "ToDo API v1");
        c.RoutePrefix = string.Empty;
    });
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

// כאשר האפליקציה תרוץ, יראו את זה במקום שגיאה 404
app.MapGet("/", ()=> "ToDoList_Server is running!");

app.Run();
