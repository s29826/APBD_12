using Task12.DTOs;

namespace Task12.Services;

public interface IDbService
{
    Task<List<TripDto>> GetTrips();
    Task DeleteClient(int idCLient);
    Task AddClientToTrip(int idTrip, AddClientToTripDto dto);

}