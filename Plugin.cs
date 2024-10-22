using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;
using Newtonsoft.Json;
using NLog;
using PhoneApp.Domain;
using PhoneApp.Domain.Attributes;
using PhoneApp.Domain.DTO;
using PhoneApp.Domain.Interfaces;


namespace EmployeesNewPeoplesAddPlugin
{
    [Author(Name = "Ivan Safeikin")]
    public class Plugin : IPluggable
    {
            private static readonly NLog.Logger logger = NLog.LogManager.GetCurrentClassLogger();

            public IEnumerable<DataTransferObject> Run(IEnumerable<DataTransferObject> args)
            {
                // Принудительный TLS 1.2 для .NET 4.5
                ServicePointManager.SecurityProtocol = SecurityProtocolType.Tls12;

                List<EmployeesDTO> employees = new List<EmployeesDTO>(args as List<EmployeesDTO>);

                try
                {
                    var newEmployees = FetchUsersFromApi();
                    employees.AddRange(newEmployees);
                    logger.Info($"Loaded {newEmployees.Count} new employees from API.");
                }
                catch (Exception ex)
                {
                    logger.Error($"Error fetching users from API: {ex.Message}");
                    logger.Trace(ex.StackTrace);
                }

                return employees;
            }

            private List<EmployeesDTO> FetchUsersFromApi()
            {
                var users = new List<EmployeesDTO>();
                try
                {
                    using (var client = new HttpClient())
                    {
                        string apiUrl = "https://dummyjson.com/users";
                        var responseTask = client.GetStringAsync(apiUrl);
                        responseTask.Wait();  // Синхронный вызов для .NET 4.5
                        var response = responseTask.Result;

                        var result = JsonConvert.DeserializeObject<UserApiResponse>(response);

                        foreach (var user in result.Users)
                        {
                            var employee = new EmployeesDTO
                            {
                                Name = $"{user.FirstName} {user.LastName}"
                            };
                            employee.AddPhone(user.Phone);
                            users.Add(employee);
                        }
                    }
                }
                catch (Exception ex)
                {
                    logger.Error($"Error fetching users from API: {ex.Message}");
                    logger.Trace(ex.StackTrace);
                }
                return users;
            }
        }

        // Модель данных для десериализации JSON
        public class UserApiResponse
        {
            public List<User> Users { get; set; }
        }

        public class User
        {
            public string FirstName { get; set; }
            public string LastName { get; set; }
            public string Phone { get; set; }
        }
    }

