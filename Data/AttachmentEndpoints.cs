using Microsoft.EntityFrameworkCore;

namespace layZTickets.Data;

public static class AttachmentEndpoints
{
    public static void MapTicketAttachments(this IEndpointRouteBuilder endpoints)
    {
        endpoints.MapGet("/attachments/{attachmentId:guid}", DownloadAsync).RequireAuthorization();
    }

    private static async Task<IResult> DownloadAsync(Guid attachmentId, HttpContext context,
        IDbContextFactory<AppDbContext> factory, ILoggerFactory loggerFactory)
    {
        context.Response.Headers.CacheControl = "private, no-store";
        context.Response.Headers.XContentTypeOptions = "nosniff";
        try
        {
            await using var db = await factory.CreateDbContextAsync(context.RequestAborted);
            var actor = await TicketAccess.GetActorAsync(db, context.User, context.RequestAborted);
            if (actor is null) return Results.NotFound();
            var file = await AttachmentAccess.VisibleAttachments(db, actor)
                .Where(file => file.Id == attachmentId)
                .Select(file => new { file.FileName, file.Content })
                .SingleOrDefaultAsync(context.RequestAborted);
            if (file is null) return Results.NotFound();

            // Każdy typ pobieramy jako plik; przeglądarka nie wykonuje np. przesłanego HTML.
            return Results.File(file.Content, "application/octet-stream", file.FileName);
        }
        catch (OperationCanceledException) when (context.RequestAborted.IsCancellationRequested)
        {
            throw;
        }
        catch (Exception exception)
        {
            loggerFactory.CreateLogger("AttachmentDownload").LogError(exception,
                "Nie udało się pobrać załącznika {AttachmentId}.", attachmentId);
            return Results.Problem(statusCode: StatusCodes.Status503ServiceUnavailable,
                title: "Nie udało się pobrać pliku. Spróbuj ponownie.");
        }
    }
}
