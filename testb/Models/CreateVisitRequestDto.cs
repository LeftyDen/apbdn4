namespace Template.Models;

public class CreateVisitRequestDto
{
    public int VisitId { get; set; }
    public int ClientId { get; set; }
    public string MechanicLicenceNumber { get; set; } = string.Empty;
    public List<CreateVisitServiceDto> Services { get; set; } = new();
}

public class CreateVisitServiceDto
{
    public string ServiceName { get; set; } = string.Empty;
    public decimal ServiceFee { get; set; }
}