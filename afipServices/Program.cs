using afipServices.src.Common.enums;
using afipServices.src.Encryption;
using afipServices.src.Encryption.interfaces;
using afipServices.src.WSAA;
using afipServices.src.WSAA.interfaces;

var builder = WebApplication.CreateBuilder(args);

//adds HttpClientFactory
builder.Services.AddHttpClient();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();
builder.Services.AddSingleton<IEncryptionManager, EncryptionManager>();
builder.Services.AddSingleton<IWSAAService, WSAAService>();

var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseHttpsRedirection();

Environment.SetEnvironmentVariable("X509CertDir", "./secrets/certificates/certificadoAfipConClaveFinal.pfx");
EncryptionManager encryption = app.Services.GetRequiredService<EncryptionManager>();
string? x = encryption.GetEncryptedLoginTicketRequest(AfipService.wsfe);
string y = (x == null) ? "NULL" : x;
Console.WriteLine(y);
var k = app.Services.GetRequiredService<IWSAAService>();

app.Run();





