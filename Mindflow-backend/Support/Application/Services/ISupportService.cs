using Mindflow_backend.Support.Domain.Model.Entities;

namespace Mindflow_backend.Support.Application.Services;

public interface ISupportService
{
    Task<SupportTicket> CreateTicketAsync(int userId, string userEmail, string subject, string message);
    Task<IEnumerable<SupportTicket>> GetUserTicketsAsync(int userId);
}
