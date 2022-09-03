using System.ComponentModel.DataAnnotations;

namespace HRSystemAPI.Model;

public class Employee
{
    public int Id { get; set; }
    public string? Name { get; set; }
    public DateTime DateOfBirth { get; set; }
    public string? Address { get; set; }
    public int DepartmentId { get; set; }
}
