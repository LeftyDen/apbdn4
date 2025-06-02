using Template.Models;

namespace Template.Services;

public interface IDbService
{
    Task<VisitDto> GetVisitByIdAsync(int visitId);
    Task AddVisitAsync(CreateVisitRequestDto request);
}