using afipServices.src.Common.enums;
using afipServices.src.Encryption;
using afipServices.src.Encryption.interfaces;
using afipServices.src.TokenManager.interfaces;
using afipServices.src.WSAA;
using afipServices.src.WSAA.interfaces;

var builder = WebApplication.CreateBuilder(args);

//adds HttpClientFactory
builder.Services.AddHttpClient();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();
builder.Services.AddSingleton<IEncryptionManager, EncryptionManager>();
builder.Services.AddSingleton<IWSAAService, WSAAService>();
builder.Services.AddSingleton<ITokenManager, TokenManager>();

var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseHttpsRedirection();
//ENVIRONMENT VARIABLES!!!

//DIRS
Environment.SetEnvironmentVariable("X509CertDir", "./secrets/certificates/certificadoAfipConClaveFinal.pfx");
Environment.SetEnvironmentVariable("TokensDir", "./secrets/tokens");


//URIS
Environment.SetEnvironmentVariable("WSAALoginCmsTestingUri", "https://wsaahomo.afip.gov.ar/ws/services/LoginCms");







var k = app.Services.GetRequiredService<ITokenManager>();

await k.GetAuthToken(AfipService.wsfe);

app.Run();





