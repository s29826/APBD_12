using Microsoft.EntityFrameworkCore;
using Task12.Data;
using Task12.DTOs;
using Task12.Models;

namespace Task12.Services;

public class DbService : IDbService
{
    private readonly _2019sbdContext _context;

    public DbService(_2019sbdContext context)
    {
        _context = context;
    }

    public async Task<List<TripDto>> GetTrips()
    {
        var trips = await _context.Trips
            .OrderByDescending(t => t.DateFrom)
            .Select(t => new TripDto
            {
                Name = t.Name,
                Description = t.Description,
                DateFrom = t.DateFrom,
                DateTo = t.DateTo,
                MaxPeople = t.MaxPeople,
                Countries = t.IdCountries
                    .Select(c => new CountryDto
                    {
                        Name = c.Name
                    }).ToList(),
                Clients = t.ClientTrips
                    .Select(c => new ClientDto
                    {
                        FirstName = c.IdClientNavigation.FirstName,
                        LastName = c.IdClientNavigation.LastName,
                    }).ToList()
            }).ToListAsync();
        
        return trips;
    }

    public async Task DeleteClient(int idCLient)
    {
        var client = await _context.Clients
            .FindAsync(idCLient);

        if (client == null)
        {
            throw new KeyNotFoundException("Nie ma takiego klienta!");
        }

        if (await _context.ClientTrips.AnyAsync(ct => ct.IdClient == idCLient))
        {
            throw new InvalidOperationException("Klient jest przypisany do co najmniej 1 wycieczki!");
        }

        _context.Clients.Remove(client);
        await _context.SaveChangesAsync();
    }

    public async Task AddClientToTrip(int idTrip, AddClientToTripDto dto)
    {
        bool alreadyAssigned = await _context.ClientTrips
            .AnyAsync(c => c.IdTrip == idTrip && c.IdClientNavigation.Pesel == dto.Pesel);

        if (alreadyAssigned)
        {
            throw new InvalidOperationException("Klient o takim PESEL-u, jest już zapisany na tę wycieczkę!");
        }
        
        if (await _context.Clients.AnyAsync(c => c.Pesel == dto.Pesel))
        {
            throw new InvalidOperationException("Klient o takim PESEL-u, już istnieje w bazie!");
        }

        var trip = await _context.Trips.FindAsync(idTrip);

        if (trip == null)
        {
            throw new KeyNotFoundException("Nie ma takiej wycieczki!");
        }

        if (trip.DateFrom <= DateTime.Now)
        {
            throw new InvalidOperationException("Nieprawidłowa data rozpoczęcia wycieczki!");
        }

        var client = new Client
        {
            FirstName = dto.FirstName,
            LastName = dto.LastName,
            Email = dto.Email,
            Telephone = dto.Telephone,
            Pesel = dto.Pesel
        };

        _context.Clients.Add(client);
        await _context.SaveChangesAsync();

        var clientTrip = new ClientTrip
        {
            IdClient = client.IdClient,
            IdTrip = idTrip,
            RegisteredAt = DateTime.Now,
            PaymentDate = dto.PaymentDate
        };

        _context.ClientTrips.Add(clientTrip);
        await _context.SaveChangesAsync();
    }
}