using afipServices.src.Common.enums;
using afipServices.src.Encryption;
using afipServices.src.Encryption.interfaces;
using afipServices.src.TokenManager.interfaces;
using afipServices.src.WSAA;
using afipServices.src.WSAA.interfaces;
using afipServices.src.WSAA.models;
using afipServices.src.WSFE;
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
Environment.SetEnvironmentVariable("WSAALoginCmsTestingUri", "https://wsaahomo.afip.gov.ar/ws/services/LoginCms");







var k = app.Services.GetRequiredService<WSFEService>();

var token = new WSAAAuthToken
{
    Token = "ASdsa",
    Sign = "SADASD"
};

Invoice invoice = new Invoice
{
    AmountOfInvoices = 1,
    PtoVta = 1,
    Type = 1,
    Concept = 1,
    DocumentType = 80, // Example: 80 represents CUIT for Argentinian documents
    DocumentNumber = 20345678901,
    FromInvoiceNumber = 1,
    ToInvoiceNumber = 1,
    GenerationDate = DateTime.Now.AddHours(1),
    FinalTotalValue = 1000.0,
    NonTaxedNetValue = 0.0,
    TaxedNetValue = 1000.0,
    ExemptValue = 0.0,
    TributeValue = 0.0,
    IVAValue = 0.0
};

invoice.AddActivity(new Activity {Id = 123 });

invoice.AddBuyer(new Buyer{ DocumentNumber = 534534, DocumentType= 80, OwnershipPercentage = 20.4});
invoice.AddBuyer(new Buyer{ DocumentNumber = 45345345, DocumentType= 80, OwnershipPercentage = 26.4});



string s = k.GenerateAuthorizeInvoiceRequest(token, 232123, invoice);
Console.WriteLine(s);
app.Run();





