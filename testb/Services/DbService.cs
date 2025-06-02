using Microsoft.Data.SqlClient;
using Template.Exceptions;
using Template.Models;

namespace Template.Services;
public class DbService : IDbService
{
    private readonly string _connectionString;

    public DbService(IConfiguration configuration)
    {
        _connectionString = configuration.GetConnectionString("Default")!;
    }

    public async Task<VisitDto> GetVisitByIdAsync(int visitId)
    {
        const string query = @"
            SELECT v.date,
                   c.first_name, c.last_name, c.date_of_birth,
                   m.mechanic_id, m.licence_number,
                   s.name, vs.service_fee
            FROM Visit v
            JOIN Client c ON c.client_id = v.client_id
            JOIN Mechanic m ON m.mechanic_id = v.mechanic_id
            JOIN Visit_Service vs ON vs.visit_id = v.visit_id
            JOIN Service s ON s.service_id = vs.service_id
            WHERE v.visit_id = @VisitId;
        ";

        await using var connection = new SqlConnection(_connectionString);
        await using var command = new SqlCommand(query, connection);
        command.Parameters.AddWithValue("@VisitId", visitId);

        await connection.OpenAsync();
        var reader = await command.ExecuteReaderAsync();

        VisitDto? visit = null;

        while (await reader.ReadAsync())
        {
            if (visit is null)
            {
                visit = new VisitDto
                {
                    Date = reader.GetDateTime(0),
                    Client = new ClientDto
                    {
                        FirstName = reader.GetString(1),
                        LastName = reader.GetString(2),
                        DateOfBirth = reader.GetDateTime(3)
                    },
                    Mechanic = new MechanicDto
                    {
                        MechanicId = reader.GetInt32(4),
                        LicenceNumber = reader.GetString(5)
                    },
                    VisitServices = new List<VisitServiceDto>()
                };
            }

            string serviceName = reader.GetString(6);
            decimal serviceFee = reader.GetDecimal(7);

            if (!visit.VisitServices.Any(s => s.Name == serviceName))
            {
                visit.VisitServices.Add(new VisitServiceDto
                {
                    Name = serviceName,
                    ServiceFee = serviceFee
                });
            }
        }

        if (visit is null)
        {
            throw new NoVisitEx("Visit not found");
        }

        return visit;
    }
    
    public async Task AddVisitAsync(CreateVisitRequestDto request)
{
    await using var connection = new SqlConnection(_connectionString);
    await connection.OpenAsync();
    await using var command = new SqlCommand();
    command.Connection = connection;
    var transaction = await connection.BeginTransactionAsync();
    command.Transaction = (SqlTransaction)transaction;

    try
    {
        // Перевірка клієнта
        command.CommandText = "SELECT 1 FROM Client WHERE client_id = @ClientId";
        command.Parameters.AddWithValue("@ClientId", request.ClientId);
        var clientExists = await command.ExecuteScalarAsync();
        if (clientExists is null)
            throw new Exception("Client not found");

        // Знайти механіка за licence number
        command.Parameters.Clear();
        command.CommandText = "SELECT mechanic_id FROM Mechanic WHERE licence_number = @Licence";
        command.Parameters.AddWithValue("@Licence", request.MechanicLicenceNumber);
        var mechanicIdObj = await command.ExecuteScalarAsync();
        if (mechanicIdObj is null)
            throw new Exception("Mechanic not found");
        int mechanicId = (int)mechanicIdObj;

        // Вставити Visit
        command.Parameters.Clear();
        command.CommandText = @"
            INSERT INTO Visit (visit_id, client_id, mechanic_id, date)
            VALUES (@VisitId, @ClientId, @MechanicId, @Date)";
        command.Parameters.AddWithValue("@VisitId", request.VisitId);
        command.Parameters.AddWithValue("@ClientId", request.ClientId);
        command.Parameters.AddWithValue("@MechanicId", mechanicId);
        command.Parameters.AddWithValue("@Date", DateTime.Now);

        await command.ExecuteNonQueryAsync();

        // Додати Visit_Service для кожної послуги
        foreach (var service in request.Services)
        {
            // Знайти service_id за ім’ям
            command.Parameters.Clear();
            command.CommandText = "SELECT service_id FROM Service WHERE name = @Name";
            command.Parameters.AddWithValue("@Name", service.ServiceName);
            var serviceIdObj = await command.ExecuteScalarAsync();
            if (serviceIdObj is null)
                throw new Exception($"Service not found: {service.ServiceName}");
            int serviceId = (int)serviceIdObj;

            // Вставити у Visit_Service
            command.Parameters.Clear();
            command.CommandText = @"
                INSERT INTO Visit_Service (visit_id, service_id, service_fee)
                VALUES (@VisitId, @ServiceId, @Fee)";
            command.Parameters.AddWithValue("@VisitId", request.VisitId);
            command.Parameters.AddWithValue("@ServiceId", serviceId);
            command.Parameters.AddWithValue("@Fee", service.ServiceFee);

            await command.ExecuteNonQueryAsync();
        }

        await transaction.CommitAsync();
    }
    catch (Exception e)
    {
        await transaction.RollbackAsync();
        throw;
    }
}
}