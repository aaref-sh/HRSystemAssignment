using HRSystemAPI.Model;
using HRSystemAPI.DAL;
using Microsoft.AspNetCore.Mvc;
using System.IO;
using System.Diagnostics;

namespace HRSystemAPI.Controllers;

[Route("api/[controller]")]
[ApiController]
public class HRPanel : ControllerBase
{
    EmployeeDAL employeeDAL;
    public HRPanel(IConfiguration configuration,IWebHostEnvironment inv)
    {
        employeeDAL = new EmployeeDAL(configuration);
    }
    
    
    [HttpGet("GetEmployees")]
    public async Task<IActionResult> GetEmployees()
    {
        Status st = new();
        try
        {
            
            st.Employees = await employeeDAL.AllEmployeesAsync();
            st.Success = true;
            return Ok(st);
            
        }
        catch
        {
            st.Success = false;
            st.Message = "An error occurred, refresh the page and try again";
            return StatusCode(500,st);
        }
    }
    

    [HttpPost("SearchEmployees")]
    public async Task<IActionResult> SearchEmployees(Employee emp)
    {
        Status st = new();
        try
        {
            
            st.Employees = await employeeDAL.SearchEmployeesAsync(emp);
            st.Success = true;
            return Ok(st);
            
        }
        catch
        {
            st.Success = false;
            st.Message = "An error occurred, refresh the page and try again";
            return StatusCode(500,st);
        }
    }
    

    [HttpGet("GetFiles")]
    public async Task<IActionResult> GetFiles(int id)
    {
        var st = new Status();
        try
        {
            st.Files = await employeeDAL.GetEmployeeFiles(id);
            st.Success = true;
            return Ok(st);
        }
        catch
        {
            st.Success=false;
            st.Message = "An error occurred, refresh the page and try again";
            return StatusCode(500,st);
        }
    }

    
    [HttpPost("AddFiles")]
    public async Task<IActionResult> AddFiles(List<IFormFile> files, int id)
    {
        Status st = new();
        try
        {
            var size = files.Sum(f => f.Length);
            foreach (var formFile in files)
            {
                if (formFile.Length > 0)
                {
                    string FileName = Guid.NewGuid().ToString() + Path.GetExtension(formFile.FileName);
                    string SavePath = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot", "Files", FileName);
                    using (var stream = new FileStream(SavePath, FileMode.Create))
                    {
                        await formFile.CopyToAsync(stream);
                        var FileSize = formFile.Length;
                        await employeeDAL.AddFile(id, FileName, FileSize);
                    }
                }
            }
            st.Success = true;
            st.Message = "Done";
        }
        catch
        {
            st.Success = false;
            st.Message = "error";
            return StatusCode(500,st);
        }
        return Ok(st);
    }
    
    
    [HttpPost("DeleteFile")]
    public async Task<IActionResult> DeleteFile(EmployeeFile file)
    {
        Status st = new();
        try
        {
            string FileName = await employeeDAL.GetFileName(file.Id);
            string FilePath = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot", "Files", FileName);
            
            System.IO.File.Delete(FilePath);
            var deleted = await employeeDAL.DeleteFile(file.Id);
            st.Success = true;
            st.Message = "Done";
            return Ok(st);
        }
        catch
        {
            st.Success = false;
            st.Message = "No such file";
            return StatusCode(500, st);
        }
    }
    
    
    [HttpPost("AddEmployee")]
    public async Task<IActionResult> AddEmployee(Employee employee)
    {
        var st = new Status();
        try
        {
            var id = await employeeDAL.AddEmployee(employee);
            st.Success = true;
            st.Message = id.ToString();
            return Ok(st);
        }
        catch
        {
            st.Success = false;
            st.Message = "An error occurred, refresh the page and try again";
            return StatusCode(500, st);
        }
    }


    [HttpGet("GetEmployeeById")]
    public async Task<IActionResult> GetEmployeeById(int Id)
    {
        var st = new Status();
        try
        {
            Employee emp = await employeeDAL.GetEmployeeById(Id);
            List<Employee> employees = new List<Employee>();
            employees.Add(emp);
            st.Success = true;
            st.Employees = employees;
            return Ok(st);
        }
        catch
        {
            st.Success = false;
            st.Message = "An error occurred, refresh the page and try again";
            return StatusCode(500, st);
        }

    }


    [HttpPost("DeleteEmployee")]
    public async Task<IActionResult> DeleteEmployee(Employee emp)
    {
        var st = new Status();
        try
        {
            await employeeDAL.DeleteEmployee(emp.Id);
            st.Success = true;
            st.Message = "Done";
            return Ok(st);

        }
        catch 
        {
            st.Success = false;
            st.Message = "An error occurred, Maybe no such Employee";
            return StatusCode(500, st);
        }
    }


    [HttpPost("EditEmployee")]
    public async Task<IActionResult> EditEmployee(Employee employee)
    {
        var st = new Status();
        try
        {
            await employeeDAL.UpdateEmployee(employee);
            st.Success = true;
            st.Message = "Done";
            return Ok(st);
        }
        catch
        {
            st.Success = false;
            st.Message = "An error occurred, Maybe no such Employee";
            return StatusCode(500, st);
        }
    }


    [HttpGet("GetFileById")]
    public async Task<IActionResult> GetFileById(int id)
    {
        try
        {
            var filename = await employeeDAL.GetFileName(id);
            string path = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot", "Files", filename);
            var file = System.IO.File.ReadAllBytes(path);
            return File(file, GetType(filename));
        }
        catch
        {
            return StatusCode(500, new { Message = "An error occured, refresh the page and try again" });
        }
    }


    [HttpGet("GetDepartments")]
    public async Task<IActionResult> GetDepartments()
    {
        var st = new Status();
        try
        {
            st.Success = true;
            st.Departments = await employeeDAL.GetDepartments();
            return Ok(st);
        }
        catch
        {
            st.Success = false;
            st.Message = "An error occurred, refresh the page and try again";
            return StatusCode(500, st);
        }
    }


    [HttpPost("AddDepartment")]
    public async Task<IActionResult> AddDepartment(Department dep)
    {
        var st = new Status();
        try
        {
            await employeeDAL.AddDepartment(dep.Name.Trim());
            st.Success = true;
            st.Message = "Done";
            return Ok(st);
        }
        catch
        {
            st.Success = false;
            st.Message = "the name is already exists";
            return StatusCode(500, st);
        }
    }


    [HttpPost("DeleteDepartment")]
    public async Task<IActionResult> DeleteDepartment(Department dep)
    {
        var st = new Status();
        try
        {
            await employeeDAL.DeleteDepartment(dep.Id);
            st.Success = true;
            st.Message = "Done";
            return Ok(st);
        }
        catch
        {
            st.Success = false;
            st.Message = "no such department to delete";
            return StatusCode(500, st);
        }
    }


    private string GetType(string fileName)
    {
        var provider =
            new Microsoft.AspNetCore.StaticFiles.FileExtensionContentTypeProvider();
        string contentType;
        if (!provider.TryGetContentType(fileName, out contentType))
        {
            contentType = "application/octet-stream";
        }
        return contentType;
    }
}
