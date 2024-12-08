using Woof.Api;

var builder = WebApplication.CreateBuilder(args);

builder.AddAppServices();
builder.Services.AddLogging();

var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseHttpsRedirection();

app.AddDefinitionEndpoints();
app.AddRunEndpoints();

app.Run();