using Microsoft.AspNetCore.Authentication.JwtBearer;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddHttpClient();

builder.Services.AddRazorPages();

// Add a CORS policy to allow cross-origin requests from specified origins.
builder.Services.AddCors(options =>
{
    // Define a policy named "AllowAPI" that allows requests from a specific origin.
    options.AddPolicy("AllowAPI", policy =>
    {
        // Allow requests from the specified API origin.
        policy.WithOrigins("http://localhost:5148")
               // Allow any HTTP method (GET, POST, PUT, DELETE, etc.).
              .AllowAnyMethod()
              // Allow any HTTP headers to be sent with requests.
              .AllowAnyHeader()
              // Allow credentials (such as cookies) to be sent with cross-origin requests.
              .AllowCredentials(); 
    });
});

// Configure Authentication to use cookie-based authentication.
builder.Services.AddAuthentication("CookieAuth")
    .AddCookie("CookieAuth", options =>
    {
        // Set the name of the authentication cookie, matching the cookie used by the API
        options.Cookie.Name = "jwt";
        // Make the cookie accessible only over HTTP (not JavaScript).
        options.Cookie.HttpOnly = true;
        // Set SameSite to Lax to allow limited cross-origin usage, depending on the application's needs.
        options.Cookie.SameSite = SameSiteMode.Lax;
        // Set the Secure policy for the cookie, which should be set to Always in production for HTTPS.
        options.Cookie.SecurePolicy = CookieSecurePolicy.None;
        // Define the path to the login page, redirecting unauthenticated users.
        options.LoginPath = "/Account/Login";
        // Define the path to the access denied page, redirecting unauthorized users.
        options.AccessDeniedPath = "/AccessDenied"; 
    });

// Configure authorization policies for role-based access control.
builder.Services.AddAuthorization(options =>
{
    // Add a policy named "AdminOnly" that requires the user to have the "Admin" role.
    options.AddPolicy("AdminOnly", policy => policy.RequireRole("Admin"));
    // Add a policy named "UserOnly" that requires the user to have the "User" role.
    options.AddPolicy("UserOnly", policy => policy.RequireRole("User"));
});

var app = builder.Build();

// Enable middleware for development
if (app.Environment.IsDevelopment())
{
    app.UseDeveloperExceptionPage();
}

// Use HTTPS redirection
app.UseHttpsRedirection();
app.UseStaticFiles();

// Ensure CORS is applied before authentication and authorization
app.UseCors("AllowAPI");

// Set up authentication and authorization middleware
app.UseRouting();
app.UseAuthentication();
app.UseAuthorization();

// Map Razor Pages
app.MapRazorPages();

app.Run();
