using Microsoft.AspNetCore.Identity;

namespace layZTickets.Data;

public static class AdminAccountSetup
{
    public static async Task CreateAsync(
    IServiceProvider services,
    IConfiguration configuration)
    {
        var email = configuration["BootstrapAdmin:Email"]?.Trim();
        var password = configuration["BootstrapAdmin:Password"];

        if (string.IsNullOrWhiteSpace(email) ||
            string.IsNullOrWhiteSpace(password))
        {
            throw new InvalidOperationException(
                "Brakuje danych BootstrapAdmin w konfiguracji.");
        }

        await using var scope = services.CreateAsyncScope();

        var roleManager = scope.ServiceProvider
            .GetRequiredService<RoleManager<IdentityRole>>();

        var userManager = scope.ServiceProvider
            .GetRequiredService<UserManager<ApplicationUser>>();

        const string adminRole = "Admin";

        if (!await roleManager.RoleExistsAsync(adminRole))
        {
            var result = await roleManager.CreateAsync(
                new IdentityRole(adminRole));

            if (!result.Succeeded)
            {
                throw new InvalidOperationException(
                    string.Join("; ",
                        result.Errors.Select(error => error.Description)));
            }
        }

        var user = await userManager.FindByNameAsync(email);

        if (user is null)
        {
            var firstName = configuration["BootstrapAdmin:FirstName"]?.Trim() ?? "Admin";
            var lastName = configuration["BootstrapAdmin:LastName"]?.Trim() ?? "Admin";

            if (string.IsNullOrWhiteSpace(firstName) || firstName.Length > 100 ||
                string.IsNullOrWhiteSpace(lastName) || lastName.Length > 100)
            {
                throw new InvalidOperationException("Imię i nazwisko administratora są wymagane i mogą mieć najwyżej 100 znaków każde.");
            }

            user = new ApplicationUser
            {
                UserName = email,
                Email = email,
                FirstName = firstName,
                LastName = lastName
            };

            var result = await userManager.CreateAsync(user, password);

            if (!result.Succeeded)
            {
                throw new InvalidOperationException(
                    string.Join("; ",
                        result.Errors.Select(error => error.Description)));
            }
        }

        if (!await userManager.IsInRoleAsync(user, adminRole))
        {
            var result = await userManager.AddToRoleAsync(user, adminRole);

            if (!result.Succeeded)
            {
                throw new InvalidOperationException(
                    string.Join("; ",
                        result.Errors.Select(error => error.Description)));
            }
        }
    }
}
