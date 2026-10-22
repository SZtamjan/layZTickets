using System.ComponentModel.DataAnnotations;

namespace layZTickets.DataSets;

public enum AccountRole
{
    User = 0,
    Agent = 1
}

public class AccountRoleSelection : IValidatableObject
{
    public AccountRole Role { get; set; } = AccountRole.User;
    public TicketCategory? AgentCategory { get; set; }

    public IEnumerable<ValidationResult> Validate(ValidationContext validationContext)
    {
        if (!Enum.IsDefined(Role))
            yield return new ValidationResult("Wybierz rolę Użytkownik lub Agent.", new[] { nameof(Role) });

        if (Role == AccountRole.Agent && AgentCategory is null)
            yield return new ValidationResult("Wybierz kategorię agenta.", new[] { nameof(AgentCategory) });
        else if (AgentCategory.HasValue && !Enum.IsDefined(AgentCategory.Value))
            yield return new ValidationResult("Wybierz poprawną kategorię agenta.", new[] { nameof(AgentCategory) });
    }
}

public class AccountRoleData : AccountRoleSelection
{
    [Required(ErrorMessage = "Wybierz konto.")]
    public string AccountId { get; set; } = string.Empty;
}
