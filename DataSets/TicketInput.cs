using System.ComponentModel.DataAnnotations;

namespace layZTickets.DataSets;

public sealed class TicketInput
{
    public const int MaxTitleLength = 200;
    public const int MaxTextLength = 10000;

    [Required(ErrorMessage = "Podaj tytuł zgłoszenia.")]
    [StringLength(MaxTitleLength, ErrorMessage = "Tytuł może mieć najwyżej 200 znaków.")]
    public string Title { get; set; } = string.Empty;

    [Required(ErrorMessage = "Opisz, czego dotyczy zgłoszenia.")]
    [StringLength(MaxTextLength, ErrorMessage = "Opis może mieć najwyżej 10000 znaków.")]
    public string Description { get; set; } = string.Empty;

    [EnumDataType(typeof(TicketCategory), ErrorMessage = "Wybierz poprawną kategorię.")]
    public TicketCategory Category { get; set; }

    [EnumDataType(typeof(TicketPriority), ErrorMessage = "Wybierz poprawny priorytet.")]
    public TicketPriority Priority { get; set; } = TicketPriority.Normal;
}
