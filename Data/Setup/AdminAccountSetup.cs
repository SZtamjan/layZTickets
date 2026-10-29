using Microsoft.AspNetCore.Identity;

namespace layZTickets.Data;

public static class AdminAccountSetup
{
    public static async Task CreateAsync(IServiceProvider services, IConfiguration configuration)
    {
        var email = configuration["BootstrapAdmin:Email"]?.Trim();
        var password = configuration["BootstrapAdmin:Password"];
        if (string.IsNullOrWhiteSpace(email) || string.IsNullOrWhiteSpace(password))
            throw new InvalidOperationException("Brakuje danych BootstrapAdmin w konfiguracji.");

        await using var scope = services.CreateAsyncScope();
        var users = scope.ServiceProvider.GetRequiredService<UserManager<ApplicationUser>>();
        var roles = scope.ServiceProvider.GetRequiredService<RoleManager<IdentityRole>>();
        var existing = await users.FindByNameAsync(email);
        if (existing is not null)
        {
            if (!await users.IsInRoleAsync(existing, "Admin"))
                throw new InvalidOperationException("Login administratora jest zajęty przez konto bez roli Admin. Wybierz inny login.");
            return;
        }

        var firstName = configuration["BootstrapAdmin:FirstName"]?.Trim() ?? "Admin";
        var lastName = configuration["BootstrapAdmin:LastName"]?.Trim() ?? "Admin";
        if (string.IsNullOrWhiteSpace(firstName) || firstName.Length > 100 ||
            string.IsNullOrWhiteSpace(lastName) || lastName.Length > 100)
            throw new InvalidOperationException("Imię i nazwisko są wymagane i mogą mieć najwyżej 100 znaków każde.");

        var db = scope.ServiceProvider.GetRequiredService<AppDbContext>();
        await using var transaction = await db.Database.BeginTransactionAsync();
        if (!await roles.RoleExistsAsync("Admin"))
            EnsureSuccess(await roles.CreateAsync(new IdentityRole("Admin")));

        var user = new ApplicationUser
        {
            UserName = email, Email = email, FirstName = firstName, LastName = lastName
        };
        EnsureSuccess(await users.CreateAsync(user, password));
        EnsureSuccess(await users.AddToRoleAsync(user, "Admin"));
        await transaction.CommitAsync();
    }

    private static void EnsureSuccess(IdentityResult result)
    {
        if (!result.Succeeded)
            throw new InvalidOperationException(string.Join("; ", result.Errors.Select(error => error.Description)));
    }
}
