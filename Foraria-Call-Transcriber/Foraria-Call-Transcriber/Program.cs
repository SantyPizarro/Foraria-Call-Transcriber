using Foraria.CallTranscriber.Services;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddControllers();

builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

builder.Services.AddHttpClient<ForariaCallbackClient>();
builder.Services.AddScoped<IForariaCallbackClient, ForariaCallbackClient>();

builder.Services.AddSingleton<IWhisperTranscriptionService, WhisperTranscriptionService>();

builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowAll", policy =>
    {
        policy
            .AllowAnyOrigin()
            .AllowAnyHeader()
            .AllowAnyMethod();
    });
});

var modelPath = builder.Configuration["Whisper:ModelPath"];
if (!File.Exists(modelPath))
{
    Console.WriteLine("Descargando modelo de Whisper...");
    var ps = System.Diagnostics.Process.Start("powershell", "./Scripts/download-model.ps1");
    ps.WaitForExit();
}

var app = builder.Build();

if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseCors("AllowAll");

app.UseRouting();
app.UseAuthorization();
app.MapControllers();

app.Run();
