using dar_system.Data;
using dar_system.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace dar_system.Controllers;

[Authorize]
[Route("chat")]
public class ChatController(DarDbContext dbContext) : AppController(dbContext)
{
    [HttpGet("{consultationId:int}")]
    public async Task<IActionResult> Index(int consultationId)
    {
        var user = await GetCurrentUserAsync();
        var consultation = await DbContext.Consultations
            .Include(c => c.Client)
            .Include(c => c.Designer)
            .FirstOrDefaultAsync(c => c.ConsultationId == consultationId);
        if (user is null || consultation is null) return NotFound();

        int? senderId = null;
        int? receiverId = null;

        if (user.UserType == "client")
        {
            var client = await GetCurrentClientAsync();
            if (client is null || consultation.ClientId != client.ClientId) return Forbid();
            senderId = client.ClientId;
            receiverId = consultation.DesignerId;
        }
        else if (user.UserType == "designer")
        {
            var designer = await GetCurrentDesignerAsync();
            if (designer is null || consultation.DesignerId != designer.DesignerId) return Forbid();
            senderId = designer.DesignerId;
            receiverId = consultation.ClientId;
        }

        if (!senderId.HasValue || !receiverId.HasValue) return Forbid();

        var model = new
        {
            Consultation = consultation,
            SenderId = senderId.Value,
            ReceiverId = receiverId.Value,
            ParticipantName = user.UserType == "client" ? consultation.Designer.FullName : consultation.Client.FullName
        };

        return View("~/Views/Chat/Index.cshtml", model);
    }

    [HttpGet("messages/{consultationId:int}")]
    public async Task<IActionResult> GetMessages(int consultationId)
    {
        var user = await GetCurrentUserAsync();
        var consultation = await DbContext.Consultations.FindAsync(consultationId);
        if (user is null || consultation is null) return NotFound();

        if (user.UserType == "client")
        {
            var client = await GetCurrentClientAsync();
            if (client is null || consultation.ClientId != client.ClientId) return Forbid();
        }
        else if (user.UserType == "designer")
        {
            var designer = await GetCurrentDesignerAsync();
            if (designer is null || consultation.DesignerId != designer.DesignerId) return Forbid();
        }

        var messages = await DbContext.ChatMessages.Where(m => m.ConsultationId == consultationId).OrderBy(m => m.SentAt).ToListAsync();
        return Json(new { success = true, messages });
    }

    [HttpPost("send")]
    public async Task<IActionResult> SendMessage(int consultationId, int receiverId, string message)
    {
        var user = await GetCurrentUserAsync();
        if (user is null || string.IsNullOrWhiteSpace(message)) return BadRequest();
        var senderId = user.UserType == "designer" ? (await GetCurrentDesignerAsync())?.DesignerId : (await GetCurrentClientAsync())?.ClientId;
        if (!senderId.HasValue) return Forbid();

        var entity = new ChatMessage
        {
            ConsultationId = consultationId,
            ReceiverId = receiverId,
            SenderId = senderId.Value,
            MessageText = message,
            SentAt = DateTime.UtcNow
        };
        DbContext.ChatMessages.Add(entity);
        await DbContext.SaveChangesAsync();
        return Json(new { success = true, message = entity });
    }
}
