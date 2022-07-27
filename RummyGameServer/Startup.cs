using System;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.IdentityModel.Tokens;
using RummyGameServer.Hubs;

namespace RummyGameServer
{
    public class Startup
    {
        private readonly SymmetricSecurityKey _securityKey = new SymmetricSecurityKey(System.Text.Encoding.ASCII.GetBytes("This is the dummy key"));
        private readonly JwtSecurityTokenHandler _jwtTokenHandler = new JwtSecurityTokenHandler();
        
        public void ConfigureServices(IServiceCollection services)
        {
            services.AddAuthorization(options =>
            {
                options.AddPolicy(JwtBearerDefaults.AuthenticationScheme, policy =>
                {
                    policy.AddAuthenticationSchemes(JwtBearerDefaults.AuthenticationScheme);
                    policy.RequireClaim(ClaimTypes.NameIdentifier);
                });
            });

            services.AddAuthentication(options =>
            {
                // Identity made Cookie authentication the default.
                // However, we want JWT Bearer Auth to be the default.
                options.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
                options.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
            }).AddJwtBearer(options =>
                {
                        // Configure JWT Bearer Auth to expect our security key
                        options.TokenValidationParameters =
                        new TokenValidationParameters
                        {
                            LifetimeValidator = (before, expires, token, param) =>
                            {
                                return expires > DateTime.UtcNow;
                            },
                            ValidateAudience = false,
                            ValidateIssuer = false,
                            ValidateActor = false,
                            ValidateLifetime = false,
                            ValidateIssuerSigningKey = false,
                            ValidateTokenReplay = false,
                            IssuerSigningKey = _securityKey,
                        };

                        // We have to hook the OnMessageReceived event in order to
                        // allow the JWT authentication handler to read the access
                        // token from the query string when a WebSocket or 
                        // Server-Sent Events request comes in.
                        options.Events = new JwtBearerEvents {
                                OnMessageReceived = context =>
                                {
                                    string accessToken = context.Request.Query["access_token"].ToString();
                                    
                                    // If the request is for our hub...
                                    var path = context.HttpContext.Request.Path;
                                    if (!string.IsNullOrEmpty(accessToken) && (path.StartsWithSegments("/gameHub")))
                                    {
                                            // Read the token out of the query string
                                            context.Token = accessToken;
                                    }
                                    return Task.CompletedTask;
                                }
                            };
                });
            services.AddSignalR();
            services.AddSingleton<TurnManager>();
            services.AddSingleton<GameManager>();
            services.AddTransient<IGameServices, GameServices>();
        }

        // This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
        public void Configure(IApplicationBuilder app, IWebHostEnvironment env)
        {
            if (env.IsDevelopment())
            {
                app.UseDeveloperExceptionPage();
            }else
            {
                app.UseExceptionHandler("/Error");
                // The default HSTS value is 30 days. You may want to change this for production scenarios, see https://aka.ms/aspnetcore-hsts.
                app.UseHsts();
            }

            app.UseHttpsRedirection();
            app.UseStaticFiles();

            app.UseRouting();
            
            app.UseAuthentication();
            app.UseAuthorization();
            
            app.UseEndpoints(endpoints =>
            {
                endpoints.MapHub<GameHub>("/gameHub");
            });
            
            app.Run(async (context) =>
            {
                if (context.Request.Path.StartsWithSegments("/generateJwtToken"))
                {
                    var userName = context.Request.Form["user"];
                    Console.WriteLine("UserName "+userName);
                    await context.Response.WriteAsync(GenerateJwtToken(userName));
                }
            });
        }
        
        private string GenerateJwtToken(string userName)
        {
            var claims = new[] { new Claim(ClaimTypes.NameIdentifier, userName) };
            var credentials = new SigningCredentials(_securityKey, SecurityAlgorithms.HmacSha256);
            var token = new JwtSecurityToken("Game Issuer", "Rummy Game Users", claims, expires: DateTime.Now.AddHours(15), signingCredentials: credentials);
            return _jwtTokenHandler.WriteToken(token);
        }
    }
}