using HRSystemUI.Model;
using HRSystemUI.Shared;
using HRSystemUI.Statics;
using Microsoft.AspNetCore.Components.Forms;
using Microsoft.AspNetCore.Components.WebAssembly.Hosting;
using System.Net.Http.Json;


namespace HRSystemUI.Pages;

public partial class Index
{
    HttpClient client = new();
    Note? note = new Note();
    string url = APIUri.Base;
    List<Employee> employees = new();
    List<Department> departments = new();
    string? message;
    Employee EmployeeToAdd = new();
    Employee EmployeeToSearch = new();
    bool uploading = false;
    int maxFiles = 10;
    long maxSize = 21500000000;
    MultipartFormDataContent content = new MultipartFormDataContent();
    private void FilesLoad(InputFileChangeEventArgs e)
    {
        content = new MultipartFormDataContent();
        foreach (var file in e.GetMultipleFiles(maxFiles))
        {
            var fileContent = new StreamContent(file.OpenReadStream(maxSize));

            fileContent.Headers.ContentType = new System.Net.Http.Headers.MediaTypeHeaderValue(file.ContentType);

            content.Add(
                content: fileContent,
                name: "\"files\"",
                fileName: file.Name);

        }
    }
    async Task UploadFiles(string id)
    {
        if (!content.Any()) return;
        try
        {
            var response = await client.PostAsync(url + "AddFiles?id=" + id, content);
            var res = await response.Content.ReadFromJsonAsync<Status>();

            _ = note.ShowNote(res);
        }
        catch
        {
            _ = note.ShowNote(new Status(false, "Uploading failed"));
        }
    }
    protected async override Task OnInitializedAsync()
    {
        await GetData();
    }

    async Task GetData()
    {
        message = "";
        try
        {
            var responseBody = await client.GetFromJsonAsync<Status>(url + "GetDepartments");
            departments = responseBody.Departments ?? new();
            responseBody = await client.GetFromJsonAsync<Status>(url + "GetEmployees");
            employees = responseBody.Employees ?? new();
            message = "There are no employees yet";
            this.StateHasChanged();
        }
        catch
        {
            _ = note.ShowNote(new Status(false, "Cannot connect to server"));
        }
    }

    async Task SearchEmployees()
    {
        message = "";
        try
        {
            var responseBody = await client.PostAsJsonAsync(url + "SearchEmployees", EmployeeToSearch);
            var res = await responseBody.Content.ReadFromJsonAsync<Status>();
            employees = res?.Employees ?? new();
            if(!employees.Any())
            message = "There is no employee matches the search";
            StateHasChanged();
        }
        catch
        {
            _ = note.ShowNote(new Status(false, "Cannot connect to server"));
        }
    }
    async Task DeleteEmployee(Employee emp)
    {
        try
        {
            var responseBody = await client.PostAsJsonAsync(url + "DeleteEmployee", emp);
            var res = await responseBody.Content.ReadFromJsonAsync<Status>();
            _ = note.ShowNote(res);

            await GetData();
        }
        catch
        {
            _ = note.ShowNote(new Status(false, "Failed to Delete the employee"));
        }
    }
    async Task AddEmployee()
    {
        uploading = true;
        string valid = ValidEmployee();
        if (valid != "")
        {
            _ = note.ShowNote(new Status(false, valid));
            uploading = false;
            return;
        }
        try
        {
            var responseBody = await client.PostAsJsonAsync(url + "AddEmployee", EmployeeToAdd);
            var res = await responseBody.Content.ReadFromJsonAsync<Status>();
            if (res.Success)
            {
                _ = note.ShowNote(new Status(true, "User Added"));
                await UploadFiles(res.Message);
            }
            EmployeeToAdd = new();
            await GetData();
        }
        catch
        {
            _ = note.ShowNote(new Status(false, "Failed to add the employee"));
        }
        uploading = false;
    }
    string ValidEmployee()
    {
        if (string.IsNullOrEmpty(EmployeeToAdd.Name) || EmployeeToAdd.Name.Trim().Length < 4)
            return "Name must be 4 letters at least";
        if (string.IsNullOrEmpty(EmployeeToAdd.Address.Trim()))
            return "Enter the address";
        if (EmployeeToAdd.DepartmentId == 0)
            return "Choese a department";
        return "";
    }
}
