using jwt.Services;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.IdentityModel.Tokens;
using System.Text;

var builder = WebApplication.CreateBuilder(args);
var config = builder.Configuration;
builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowDocuSign", builder =>
    {
        builder.WithOrigins("http://localhost")
               .AllowAnyHeader()
               .AllowAnyMethod();
    });
});

builder.Services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
    .AddJwtBearer(options =>
    {
        options.TokenValidationParameters = new TokenValidationParameters
        {
            ValidateIssuer = true,
            ValidIssuer = builder.Configuration["Jwt:Issuer"],
            ValidateAudience = true,
            ValidAudience = builder.Configuration["Jwt:Audience"],
            ValidateLifetime = true,
            ValidateIssuerSigningKey = true,
            IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(builder.Configuration["Jwt:SecretKey"]))
        };
    });

builder.Services.Configure<DocuSignConfig>(builder.Configuration.GetSection("DocuSignConfig"));
builder.Services.AddScoped<DocuSignAuthenticationService>();
builder.Services.AddScoped<DocuSignService>();
builder.Services.AddHttpClient<OAuthUserInfoService>();
builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();
var app = builder.Build();
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}
app.UseHttpsRedirection();
//app.UseCors("AllowDocuSign");
app.UseCors("AllowDocuSign");
app.UseAuthentication();
app.UseAuthorization();
app.MapControllers();
app.Run();