using HRSystemUI.Shared;
using HRSystemUI.Statics;
using Microsoft.AspNetCore.Components.Forms;
using Microsoft.AspNetCore.Components;
using System.Net.Http.Json;
using HRSystemUI.Model;

namespace HRSystemUI.Pages;

public partial class EmployeeInfo
{
    HttpClient client = new();
    [Parameter]
    public string Id { get; set; }
    List<EmployeeFile> files = new();
    Note? note;
    Employee? emp;
    string url = APIUri.Base;
    List<Department> departments = new();
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
    async Task DeleteFile(EmployeeFile file)
    {
        try
        {
            var response = await client.PostAsJsonAsync(url + "DeleteFile", file);
            var res = await response.Content.ReadFromJsonAsync<Status>();
            await GetData();

            _ = note.ShowNote(res);
        }
        catch
        {
            _ = note.ShowNote(new Status(false, "Deleting failed"));
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
        try
        {
            var responseBody = await client.GetFromJsonAsync<Status>(url + "GetDepartments");
            departments = responseBody.Departments ?? new();
            if (!responseBody.Success) throw new Exception();
            responseBody = await client.GetFromJsonAsync<Status>(url + "GetEmployeeById?Id=" + Id);
            emp = responseBody.Employees.ElementAt(0) ?? new();
            responseBody = await client.GetFromJsonAsync<Status>(url + "GetFiles?id=" + Id);
            files = responseBody.Files ?? new();
            if (!responseBody.Success)
            {
                _ = note.ShowNote(new Status(false, "No such employee"));
                return;
            }
            this.StateHasChanged();
        }
        catch
        {
            _ = note.ShowNote(new Status(false, "Cannot connect to server"));
        }
    }

    async Task UpdateEmployee()
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
            var responseBody = await client.PostAsJsonAsync(url + "EditEmployee", emp);
            var res = await responseBody.Content.ReadFromJsonAsync<Status>();
            _ = note.ShowNote(res);
            await UploadFiles(Id.ToString());
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
        try
        {
            if (string.IsNullOrEmpty(emp.Name) || emp.Name.Trim().Length < 4)
                return "Name must be 4 letters at least";
            if (string.IsNullOrEmpty(emp.Address.Trim()))
                return "Enter the address";
            if (emp.DepartmentId == 0)
                return "Choese a department";
            return "";
        }
        catch { return "Check the filds and try again"; }
    }
}
