public class VisitDto
{
    public DateTime Date { get; set; }
    public ClientDto Client { get; set; } = new();
    public MechanicDto Mechanic { get; set; } = new();
    public List<VisitServiceDto> VisitServices { get; set; } = new();
}

public class ClientDto
{
    public string FirstName { get; set; } = string.Empty;
    public string LastName { get; set; } = string.Empty;
    public DateTime DateOfBirth { get; set; }
}

public class MechanicDto
{
    public int MechanicId { get; set; }
    public string LicenceNumber { get; set; } = string.Empty;
}

public class VisitServiceDto
{
    public string Name { get; set; } = string.Empty;
    public decimal ServiceFee { get; set; }
}