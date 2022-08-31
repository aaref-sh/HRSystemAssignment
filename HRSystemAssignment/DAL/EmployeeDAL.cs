using HRSystemAPI.Model;
using System.Data.SqlClient;

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
    internal async Task<Status> AllEmployeesAsync()
    {
        Status st = new();
        try
        {
            var Employees = new List<Employee>();
            using(var cmd = connection.CreateCommand())
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
                        }
                    );
                }
            }
            st.Success = true;
            st.Employees = Employees;
        }
        catch
        {
            st.Success = false;
            st.Message = "An error acoured, refresh the page and try again";
        }
        finally
        {
            await connection.CloseAsync();
        }
        return st;
    }
    internal async Task<Status> EmployeesByName(string query)
    {

        Status st = new();
        try
        {
            var Employees = new List<Employee>();
            using(var cmd = connection.CreateCommand())
            {
                cmd.CommandText = "select * from employee where name like '%"+query+"%'";
                await connection.OpenAsync();
                var rd = await cmd.ExecuteReaderAsync();
            
                while (await rd.ReadAsync())
                {
                    Employees.Add(
                        new Employee
                        {
                            Id = Convert.ToInt32(rd["Id"]),
                            Name = Convert.ToString(rd["Name"]),
                        }
                    );
                }
            }
            st.Success = true;
            st.Employees = Employees;
        }
        catch
        {
            st.Success = false;
            st.Message = "An error acoured, refresh the page and try again";
        }
        finally
        {
            await connection.CloseAsync();
        }
        return st;
    }

    internal async Task<Status> GetEmployeeFiles(int Id)
    {
        var st = new Status();
        try
        {
            var Files = new List<EmployeeFile>();
            using(var cmd = connection.CreateCommand())
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
            st.Success = true;
            st.Files = Files;

        }
        catch
        {
            st.Success = false;
            st.Message = "An error accoured, refresh the page and try again";

        }
        finally { await connection.CloseAsync(); }
        return st;
    }

    internal async Task AddFile(int id, string fileName, long fileSize)
    {
        try
        {
            using(var cmd = connection.CreateCommand())
            {
                cmd.CommandText = "insert into employeefile (filename,employee_id,filesize) values " +
                    "('" + fileName+"',"+id+","+fileSize+")";
                await connection.OpenAsync();
                await cmd.ExecuteNonQueryAsync();
            }
        }
        catch { throw new Exception(); }
        finally { 
            await connection.CloseAsync();
            connection.Dispose();
        }
    }

    internal async Task<string> GetFileName(int id)
    {
        string filename = "";
        try
        {
            using(var cmd = connection.CreateCommand())
            {
                cmd.CommandText = "select filename from employeefile where id = " + id;
                await connection.OpenAsync();
                filename = (await cmd.ExecuteScalarAsync()) as string;
            }
        }
        catch { }
        finally { await connection.CloseAsync(); }
        return filename;
    }

    internal async Task<int> DeleteFile(int id)
    {
        int res=0;
        try
        {
            using(var cmd = connection.CreateCommand())
            {
                cmd.CommandText = "delete from employeefile where id = " + id;
                await connection.OpenAsync();
                res = await cmd.ExecuteNonQueryAsync();
            }
        }
        catch { }
        finally { await connection.CloseAsync(); }
        return res;
    }
}
