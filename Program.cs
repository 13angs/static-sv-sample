using System.Text;
using System.Text.Json;
using static_sample_sv.Interfaces;
using static_sample_sv.Models;
using static_sample_sv.Services;

var builder = WebApplication.CreateBuilder(args);

IConfiguration Configuration = builder.Configuration;

builder.Services.AddHttpContextAccessor();
builder.Services.AddScoped<IRequestValidator, RequestValidator>();

var app = builder.Build();


app.MapGet("/", () => "Hello World!");

app.MapGet("/api/v1/statics", async (IHttpContextAccessor contextAccessor, IRequestValidator requestValidator) => {
    string filePath = Path.Combine(app.Environment.ContentRootPath, "Files", "Base64Image.txt");
    string fileContent = File.ReadAllText(filePath);

    StaticModel model = new StaticModel{
        Type="image/png",
        Base64EncodedFile=fileContent
    };

    string signature = requestValidator.Validate(model);

    HttpClient client = new HttpClient();

    string url = $"{Configuration["Static:Url"]}/api/v1/statics";

    HttpContent content = new StringContent(JsonSerializer.Serialize(model), Encoding.UTF8, "application/json");
    client.DefaultRequestHeaders.Add("x-static-signature", signature);
    HttpResponseMessage res = await client.PostAsync(url, content);
    string resStr = await res.Content.ReadAsStringAsync();
    return resStr;
});

app.Run();
