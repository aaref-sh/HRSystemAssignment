using System.ComponentModel.DataAnnotations;

namespace HRSystemAPI.Model;

public class Employee
{
    public int Id { get; set; }
    [Required(ErrorMessage ="Name field is required")]
    [MinLength(4,ErrorMessage ="The name must be 4 chars at least")]
    public string? Name { get; set; }
    public DateTime? DateOfBirth { get; set; }
    public string? Address { get; set; }
    public int MyProperty { get; set; }


}
