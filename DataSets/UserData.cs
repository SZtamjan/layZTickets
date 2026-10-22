using System.ComponentModel.DataAnnotations;

namespace layZTickets.DataSets;

// Dane formularza tworzenia konta, bez mapowania na tabelę bazy danych.
public class UserData
{
    [Required(ErrorMessage = "Podaj imię.")]
    [StringLength(100, ErrorMessage = "Imię może mieć najwyżej 100 znaków.")]
    public string FirstName { get; set; } = string.Empty;

    [Required(ErrorMessage = "Podaj nazwisko.")]
    [StringLength(100, ErrorMessage = "Nazwisko może mieć najwyżej 100 znaków.")]
    public string LastName { get; set; } = string.Empty;

    [Required(ErrorMessage = "Podaj adres e-mail.")]
    [EmailAddress(ErrorMessage = "Podaj prawidłowy adres e-mail.")]
    [StringLength(256, ErrorMessage = "Adres e-mail może mieć najwyżej 256 znaków.")]
    public string Email { get; set; } = string.Empty;

    [Required(ErrorMessage = "Podaj hasło.")]
    [DataType(DataType.Password)]
    public string Password { get; set; } = string.Empty;

    [Required(ErrorMessage = "Powtórz hasło.")]
    [DataType(DataType.Password)]
    [Compare(nameof(Password), ErrorMessage = "Podane hasła muszą być identyczne.")]
    public string ConfirmPassword { get; set; } = string.Empty;
}
