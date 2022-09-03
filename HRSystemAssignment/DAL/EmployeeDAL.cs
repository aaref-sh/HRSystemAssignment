using HRSystemAPI.Model;
using System.Data.SqlClient;
using System.IO;

namespace HRSystemAPI.DAL;

public class EmployeeDAL
{
    string ConnectionString;
    SqlConnection connection;
    internal EmployeeDAL(IConfiguration configuration)
    {
        ConnectionString = configuration.GetConnectionString("DefaultConnection");
        connection = new SqlConnection(ConnectionString);
    }
    internal async Task<List<Employee>> AllEmployeesAsync()
    {
        var Employees = new List<Employee>();
        try
        {
            using (var cmd = connection.CreateCommand())
            {
                cmd.CommandText = "select * from employee";
                await connection.OpenAsync();
                var rd = await cmd.ExecuteReaderAsync();

                while (await rd.ReadAsync())
                {
                    Employees.Add(
                        new Employee
                        {
                            Id = Convert.ToInt32(rd["Id"]),
                            Name = Convert.ToString(rd["Name"]),
                            Address = Convert.ToString(rd["Address"]),
                            DateOfBirth = Convert.ToDateTime(rd["Dateofbirth"]),
                            DepartmentId = Convert.ToInt32(rd["department_id"]),
                        }
                    );
                }
            }
        }
        catch { throw new Exception(); }
        finally { await connection.CloseAsync(); }
        return Employees;
    }
    internal async Task<List<EmployeeFile>> GetEmployeeFiles(int Id)
    {
        var Files = new List<EmployeeFile>();
        try
        {
            using (var cmd = connection.CreateCommand())
            {
                cmd.CommandText = "select * from employeefile where employee_id = " + Id;
                await connection.OpenAsync();
                var rd = await cmd.ExecuteReaderAsync();

                while (await rd.ReadAsync())
                {
                    Files.Add(
                        new EmployeeFile
                        {
                            FileName = Convert.ToString(rd["filename"]),
                            FileSize = Convert.ToInt64(rd["filesize"]),
                            Id = Convert.ToInt32(rd["Id"])
                        }
                    );
                }
            }
        }
        catch { throw new Exception(); }
        finally { await connection.CloseAsync(); }
        return Files;
    }

    internal async Task AddFile(int id, string fileName, long fileSize)
    {
        try
        {
            using (var cmd = connection.CreateCommand())
            {
                cmd.CommandText = "insert into employeefile (filename,employee_id,filesize) values " +
                    "('" + fileName + "'," + id + "," + fileSize + ");";
                await connection.OpenAsync();
                await cmd.ExecuteNonQueryAsync();
            }
        }
        catch { throw new Exception(); }
        finally { await connection.CloseAsync();
        }
    }

    internal async Task<string> GetFileName(int id)
    {
        string filename = "";
        try
        {
            using (var cmd = connection.CreateCommand())
            {
                cmd.CommandText = "select filename from employeefile where id = " + id;
                await connection.OpenAsync();
                filename = (await cmd.ExecuteScalarAsync()) as string;
            }
        }
        catch { throw new Exception(); }
        finally { await connection.CloseAsync(); }
        return filename;
    }

    internal async Task<int> DeleteFile(int id)
    {
        int res = 0;
        try
        {
            using (var cmd = connection.CreateCommand())
            {
                cmd.CommandText = "delete from employeefile where id = " + id;
                await connection.OpenAsync();
                res = await cmd.ExecuteNonQueryAsync();
            }
        }
        catch { throw new Exception(); }
        finally { await connection.CloseAsync(); }
        return res;
    }
    internal async Task<int> AddEmployee(Employee emp)
    {
        int res;
        try
        {
            using (var cmd = connection.CreateCommand())
            {
                cmd.CommandText = "insert into employee (name,dateofbirth,address,department_id) values " +
                    "('" + emp.Name.Trim() + "',(CAST('" + emp.DateOfBirth.ToString("yyyy-MM-dd") + "' AS Date)), '"
                    + emp.Address.Trim() + "'," + emp.DepartmentId + " ) SELECT CAST(scope_identity() AS int)";
                await connection.OpenAsync();
                res = (int) await cmd.ExecuteScalarAsync();
            }
        }
        catch { throw new Exception(); }
        finally { await connection.CloseAsync(); }
        return res;

    } 
    internal async Task DeleteEmployee(int id)
    {
        var Files = await GetEmployeeFiles(id);
        try
        {
            using (var cmd = connection.CreateCommand())
            {

                cmd.CommandText = "select name from employee where id = " + id;
                await connection.OpenAsync();

                var x = (await cmd.ExecuteScalarAsync()) as string;
                if (string.IsNullOrEmpty(x)) throw new Exception();
                cmd.CommandText = "delete from employeefile where employee_id = " + id+";" +
                    "delete from employee where id = "+id;
                await cmd.ExecuteNonQueryAsync();
            }

            // cleaning files from desk
            // in case we dont really wish to delete records or files 
            // its enough to add Is_Deleted column in our table and set it true
            foreach (var file in Files)
                File.Delete(Path.Combine(Directory.GetCurrentDirectory(), "wwwroot", "Files", file.FileName));
            
        }
        catch { throw new Exception(); }
        finally { await connection.CloseAsync(); }
        

    }

    internal async Task UpdateEmployee(Employee employee)
    {
        try
        {
            using (var cmd = connection.CreateCommand())
            {

                cmd.CommandText = "update employee set name = '" + employee.Name.Trim() + "' , address = '" +
                    employee.Address.Trim() + "' , dateofbirth = (CAST('" + employee.DateOfBirth.ToString("yyyy-MM-dd")
                    + "' AS Date)), department_id = " + employee.DepartmentId + " where id = " + employee.Id;
                await connection.OpenAsync();
                await cmd.ExecuteNonQueryAsync();
            }
        }
        catch { throw new Exception(); }
        finally { await connection.CloseAsync(); }
    }

    internal async Task<List<Department>> GetDepartments()
    {

        var Departments = new List<Department>();
        try
        {
            using (var cmd = connection.CreateCommand())
            {
                cmd.CommandText = "select * from Department";
                await connection.OpenAsync();
                var rd = await cmd.ExecuteReaderAsync();

                while (await rd.ReadAsync())
                {
                    Departments.Add(
                        new Department
                        {
                            Id = Convert.ToInt32(rd["Id"]),
                            Name = Convert.ToString(rd["Name"]),
                        }
                    );
                }
            }
        }
        catch { throw new Exception(); }
        finally { await connection.CloseAsync(); }
        return Departments;
    }

    internal async Task AddDepartment(string name)
    {
        try
        {
            using (var cmd = connection.CreateCommand())
            {
                cmd.CommandText = "select name from department where name = '" + name + "'";
                await connection.OpenAsync();
                var x = (await cmd.ExecuteScalarAsync()) as string;
                if (!string.IsNullOrEmpty(x)) throw new Exception();
                cmd.CommandText = "insert into Department (name) values ('" + name + "')";
               
                await cmd.ExecuteScalarAsync();
            }
        }
        catch { throw new Exception(); }
        finally { await connection.CloseAsync(); }
    }
    internal async Task DeleteDepartment(int id)
    {
        int res;
        try
        {
            using (var cmd = connection.CreateCommand())
            {
                List<int> ids = new();
                cmd.CommandText = "select * from employee where department_id ="+id;
                await connection.OpenAsync();
                var rd = await cmd.ExecuteReaderAsync();

                while (await rd.ReadAsync())
                {
                    try
                    {
                        ids.Add(Convert.ToInt32(rd["Id"]));
                    }
                    catch { }
                }
                await connection.CloseAsync();
                foreach(var i in ids) 
                        await DeleteEmployee(i);

                cmd.CommandText = "delete from department where id = " + id;
                await connection.OpenAsync();
                res = await cmd.ExecuteNonQueryAsync();
                if(res < 1) { throw new Exception(); }
            }
        }
        catch { throw new Exception(); }
        finally { await connection.CloseAsync(); }
    }

    internal async Task<Employee> GetEmployeeById(int id)
    {
        Employee employee = new();
        try
        {
            using (var cmd = connection.CreateCommand())
            {
                cmd.CommandText = "select * from employee where id = "+id;
                await connection.OpenAsync();
                var rd = await cmd.ExecuteReaderAsync();

                if(await rd.ReadAsync())
                {
                    employee = new Employee
                    {
                        Id = Convert.ToInt32(rd["Id"]),
                        Name = Convert.ToString(rd["Name"]),
                        Address = Convert.ToString(rd["Address"]),
                        DateOfBirth = Convert.ToDateTime(rd["Dateofbirth"]),
                        DepartmentId = Convert.ToInt32(rd["department_id"]),
                    };
                }
            }
        }
        catch { throw new Exception(); }
        finally { await connection.CloseAsync(); }
        return employee;
    }

    internal async Task<List<Employee>?> SearchEmployeesAsync(Employee emp)
    {

        var Employees = new List<Employee>();
        try
        {
            using (var cmd = connection.CreateCommand())
            {
                string text = "select * from employee where id>0 and name like '%" + emp.Name?.Trim() + "%' and " +
                    "address like '%" + emp.Address?.Trim() +"%' ";
                if (emp.DepartmentId != 0) text += " and department_id = " + emp.DepartmentId;
                cmd.CommandText = text;
                await connection.OpenAsync();
                var rd = await cmd.ExecuteReaderAsync();

                while (await rd.ReadAsync())
                {
                    Employees.Add(
                        new Employee
                        {
                            Id = Convert.ToInt32(rd["Id"]),
                            Name = Convert.ToString(rd["Name"]),
                            Address = Convert.ToString(rd["Address"]),
                            DateOfBirth = Convert.ToDateTime(rd["Dateofbirth"]),
                            DepartmentId = Convert.ToInt32(rd["department_id"]),
                        }
                    );
                }
            }
        }
        catch { throw new Exception(); }
        finally { await connection.CloseAsync(); }
        return Employees;
    }
}
