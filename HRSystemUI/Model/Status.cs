namespace HRSystemUI.Model;

public class Status
{
    public bool Success { get; set; }
    public string? Message { get; set; }
    public List<Employee>? Employees { get; set; }
    public List<EmployeeFile>? Files { get; set; }
    public List<Department>? Departments { get; set; }
    public Status()
    {

    }
    public Status(bool Success, string Message)
    {
        this.Success = Success;
        this.Message = Message;
    }
}
