namespace API.DTOs;

public class PhotoForApprovalDTO
{
    public int PhotoId { get; set; }
    public string Url { get; set; }
    public string Username { get; set; }
    public bool IsApproved { get; set; }
}
