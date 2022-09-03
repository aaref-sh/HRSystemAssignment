namespace HRSystemUI.Pages;
using HRSystemUI.Model;
using HRSystemUI.Shared;
using HRSystemUI.Statics;
using System.Net.Http.Json;

public partial class Departments
{
    private Note? note;
    string url = APIUri.Base;
    string? depname;
    List<Department> departments = new();
    string? message;
    HttpClient client = new();
    bool adding = false;
    protected async override Task OnInitializedAsync()
    {
        await GetDepartments();
    }

    async Task GetDepartments()
    {
        try
        {
            var responseBody = await client.GetFromJsonAsync<Status>(url + "GetDepartments");
            departments = responseBody.Departments ?? new();
            this.StateHasChanged();
            if (!departments.Any()) message = "there is no departments yet";
        }
        catch
        {
            message = "unable to retrive data";
        }
    }
    async Task DeleteDepartment(int id)
    {
        try
        {
            var responseBody = await client.PostAsJsonAsync(url + "DeleteDepartment", new { Id = id });
            var res = await responseBody.Content.ReadFromJsonAsync<Status>();
            _ = note.ShowNote(res);
            await GetDepartments();
        }
        catch
        {
            _ = note.ShowNote(new Status(false, "Cannot connect to server"));
        }
    }
    async Task AddDepartment()
    {
        adding = true;
        if (string.IsNullOrEmpty(depname) || depname.Length < 2)
        {
            _ = note.ShowNote(new Status(false, "Name length must be 2 at least" ));
            adding = false;
            return;
        }
        try
        {
            var responseBody = await client.PostAsJsonAsync(url + "AddDepartment", new { Id = 0, Name = depname.Trim() });
            var res = await responseBody.Content.ReadFromJsonAsync<Status>();
            _ = note.ShowNote(res);
            depname = "";
            await GetDepartments();
        }
        catch
        {
            _ = note.ShowNote(new Status(false,"Cannot connect to server"));
        }
        adding = false;
    }

}
