using Microsoft.AspNetCore.Components.Forms;
using layZTickets.DataSets;

namespace layZTickets.Data;

public sealed record AttachmentSelection(IReadOnlyList<IBrowserFile> Files, bool IsValid = true)
{
    public static AttachmentSelection Empty => new(Array.Empty<IBrowserFile>());
}

public sealed record AttachmentDraft(string FileName, byte[] Content);

public static class AttachmentUpload
{
    public const long MaxFileSize = 5 * 1024 * 1024;
    public const int MaxFiles = 5;

    public static string SafeFileName(string name)
    {
        var fileName = Path.GetFileName(name.Replace('\\', '/'));
        fileName = string.Concat(fileName.Where(character => !char.IsControl(character))).Trim();
        if (string.IsNullOrWhiteSpace(fileName) || fileName is "." or ".." || fileName.Length > 255)
            throw new InvalidDataException("Nazwa pliku musi mieć od 1 do 255 znaków.");
        return fileName;
    }

    public static void ValidateSelection(IReadOnlyList<IBrowserFile> files)
    {
        if (files.Count > MaxFiles)
            throw new InvalidDataException($"Możesz wybrać najwyżej {MaxFiles} plików naraz.");
        foreach (var file in files)
        {
            SafeFileName(file.Name);
            if (file.Size < 0 || file.Size > MaxFileSize)
                throw new InvalidDataException("Każdy plik może mieć najwyżej 5 MB.");
        }
    }

    public static async Task<List<AttachmentDraft>> ReadAsync(IReadOnlyList<IBrowserFile> files)
    {
        ValidateSelection(files);
        var drafts = new List<AttachmentDraft>();
        foreach (var file in files)
        {
            // Limit strumienia i licznik bajtów nie polegają wyłącznie na rozmiarze z przeglądarki.
            await using var source = file.OpenReadStream(MaxFileSize);
            using var destination = new MemoryStream();
            var buffer = new byte[64 * 1024];
            int read;
            while ((read = await source.ReadAsync(buffer)) > 0)
            {
                if (destination.Length + read > MaxFileSize)
                    throw new InvalidDataException("Każdy plik może mieć najwyżej 5 MB.");
                await destination.WriteAsync(buffer.AsMemory(0, read));
            }
            drafts.Add(new AttachmentDraft(SafeFileName(file.Name), destination.ToArray()));
        }
        return drafts;
    }

    public static List<TicketAttachment> CreateEntities(IReadOnlyList<AttachmentDraft> drafts, string userId)
    {
        if (drafts.Count > MaxFiles || drafts.Any(file => file.Content.LongLength > MaxFileSize))
            throw new InvalidDataException("Maksymalnie 5 plików, każdy do 5 MB.");
        return drafts.Select(file => new TicketAttachment
        {
            FileName = SafeFileName(file.FileName), Content = file.Content,
            Size = file.Content.LongLength, UploadedByUserId = userId
        }).ToList();
    }

    public static string FormatSize(long size) => size >= 1024 * 1024
        ? $"{size / (1024d * 1024):0.##} MB" : size >= 1024 ? $"{size / 1024d:0.##} KB" : $"{size} B";
}
