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
    public HRPanel(IConfiguration configuration)
    {
        employeeDAL = new EmployeeDAL(configuration);
    }
    [HttpGet("GetEmployees")]
    public async Task<IActionResult> GetEmployees(string? query)
    {
        try
        {
            if (query == null)
                return Ok(await employeeDAL.AllEmployeesAsync());
            
            return Ok(await employeeDAL.EmployeesByName(query));
        }
        catch
        {
            return StatusCode(500);
        }
    }
    [HttpGet("GetFiles")]
    public async Task<IActionResult> GetFiles(int id)
    {
        try
        {
            return Ok(await employeeDAL.GetEmployeeFiles(id)) ;
        }
        catch
        {
            return StatusCode(500);
        }
    }

    [HttpPost("AddFile")]
    public async Task<IActionResult> AddFile(List<IFormFile> files, int id)
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
                        formFile.CopyTo(stream);
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
            return StatusCode(500);
        }
        return Ok(st);
    }
    [HttpPost("DeleteFile")]
    public async Task<IActionResult> DeleteFile(int id)
    {
        Status st = new();
        try
        {

            string FileName = await employeeDAL.GetFileName(id);
            string FilePath = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot", "Files", FileName);
            
            System.IO.File.Delete(FilePath);
            var deleted = await employeeDAL.DeleteFile(id);
            st.Success = true;
            st.Message = "Done";
        }
        catch
        {
            st.Success = false;
            st.Message = "No such file";
        }
        return Ok(st);

    }
}
