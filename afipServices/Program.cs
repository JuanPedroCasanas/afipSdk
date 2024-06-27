using afipServices.src.Common.enums;
using afipServices.src.Encryption;
using afipServices.src.Encryption.interfaces;
using afipServices.src.TokenManager.interfaces;
using afipServices.src.WSAA;
using afipServices.src.WSAA.interfaces;
using afipServices.src.WSAA.models;
using afipServices.src.WSFE;
using afipServices.src.WSFE.enums;
using afipServices.src.WSFE.models;

var builder = WebApplication.CreateBuilder(args);

//adds HttpClientFactory
builder.Services.AddHttpClient();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();
builder.Services.AddSingleton<IEncryptionManager, EncryptionManager>();
builder.Services.AddSingleton<IWSAAService, WSAAService>();
builder.Services.AddSingleton<ITokenManager, TokenManager>();
builder.Services.AddSingleton<WSFEService>();

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
//WSAA
Environment.SetEnvironmentVariable("WSAALoginCmsUri", "https://wsaahomo.afip.gov.ar/ws/services/LoginCms");

//WSFE
Environment.SetEnvironmentVariable("WSFEParamGetUri_INVOICE_TYPES", "https://wswhomo.afip.gov.ar/wsfev1/service.asmx?op=FEParamGetTiposCbte");
Environment.SetEnvironmentVariable("WSFEParamGetUri_CONCEPT_TYPES", "https://wswhomo.afip.gov.ar/wsfev1/service.asmx?op=FEParamGetTiposConcepto");
Environment.SetEnvironmentVariable("WSFEParamGetUri_DOCUMENT_TYPES", "https://wswhomo.afip.gov.ar/wsfev1/service.asmx?op=FEParamGetTiposDoc");
Environment.SetEnvironmentVariable("WSFEParamGetUri_IVA_TYPES", "https://wswhomo.afip.gov.ar/wsfev1/service.asmx?op=FEParamGetTiposIva");
Environment.SetEnvironmentVariable("WSFEParamGetUri_CURRENCY_TYPES", "https://wswhomo.afip.gov.ar/wsfev1/service.asmx?op=FEParamGetTiposMonedas");
Environment.SetEnvironmentVariable("WSFEParamGetUri_OPTIONAL_TYPES", "https://wswhomo.afip.gov.ar/wsfev1/service.asmx?op=FEParamGetTiposOpcional");
Environment.SetEnvironmentVariable("WSFEParamGetUri_TRIBUTE_TYPES", "https://wswhomo.afip.gov.ar/wsfev1/service.asmx?op=FEParamGetTiposTributos");





var s = app.Services.GetRequiredService<ITokenManager>();
var k = app.Services.GetRequiredService<WSFEService>();

var token = await s.GetAuthToken(AfipService.wsfe);

if (token != null)
{
var list = await k.GetVariableTypeValues(token, WSFEVariableTypes.TRIBUTE_TYPES);
if(list != null)
{
    foreach(var e in list)
    {
        Console.WriteLine(e.ToString());
    }
}
}


app.Run();





