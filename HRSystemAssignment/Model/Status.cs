namespace HRSystemAPI.Model;

public class Status
{
    public bool Success { get; set; }
    public string? Message { get; set; }
    public List<Employee>? Employees { get; set; }
    public List<EmployeeFile>? Files { get; set; }
    public List<Department>? Departments { get; set; }

}
