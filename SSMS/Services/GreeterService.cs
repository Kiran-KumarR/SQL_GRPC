using Google.Protobuf.WellKnownTypes;
using Grpc.Core;
using SSMS;
using SSMS.Protos;
using System.Data;
using System.Data.SqlClient;

namespace SSMS.Services
{
    public class GreeterService : Greeter.GreeterBase
    {


        /* private readonly ILogger<GreeterService> _logger;
         public GreeterService(ILogger<GreeterService> logger)
         {
             _logger = logger;
         }

         public override Task<HelloReply> SayHello(HelloRequest request, ServerCallContext context)
         {
             return Task.FromResult(new HelloReply
             {
                 Message = "Hello " + request.Name
             });
         }*/

        private readonly string _connectionString = "Data Source=(localdb)\\MSSQLLocalDB;Initial Catalog=Customer;Integrated Security=True;";

        public override async Task<CustomerList> ExceuteQuery(Empty empty, ServerCallContext context)
        {
            List<Customer> customers = new List<Customer>();

            using (SqlConnection connection = new SqlConnection(_connectionString))
            {
                await connection.OpenAsync();

                //using (SqlCommand command = new SqlCommand(request.SqlQuery, connection))
                using (SqlCommand command = new SqlCommand("select * from Customer", connection))
               // using (SqlCommand command = new SqlCommand("EXEC GetFullNameById @Id = 5", connection))
                using (SqlDataReader reader = await command.ExecuteReaderAsync())
                {
                    while (reader.Read())
                    {
                        customers.Add(new Customer
                        {   
                            Id= reader.GetInt32(0),
                            FirstName = reader.GetString(1),
                            LastName = reader.GetString(2),
                            Age = reader.GetInt32(3),
                            City = reader.GetString(4),
                        });
                    }
                }
            }

            return new CustomerList { Customer = { customers } };
        }

        public override Task<Customer>GetCustomerDetail(CustomerID request, ServerCallContext context) {
            int customerID = request.Id;
            var customerDetails = GetCustomerDetailFromDataBase(customerID);
            return Task.FromResult(customerDetails);
        }

        public override Task<Customer> GetCustomerDetailBySP(CustomerID request, ServerCallContext context)
        {
            int customerId = request.Id;
            // Call stored procedure to fetch student details
            var customerDetail = GetCustomerDetailUsingSP(customerId);
            return Task.FromResult(customerDetail);
        }

       
        public  Customer GetCustomerDetailFromDataBase(int CustomerID)
        {
            // Implement logic to retrieve student details from the database
            // You can use ADO.NET or an ORM like Entity Framework
            // Example using ADO.NET:
            using (var connection = new SqlConnection(_connectionString))
            {
                connection.Open();
                using (var command = new SqlCommand("SELECT * from Customer WHERE Id = @CustomerId", connection))
                {
                    command.Parameters.AddWithValue("@CustomerId", CustomerID);
                    using (var reader = command.ExecuteReader())
                    {
                        if (reader.Read())
                        {
                            return new Customer
                            {
                                Id = reader.GetInt32(0),
                                FirstName = reader.GetString(1),
                                LastName = reader.GetString(2),
                                Age = reader.GetInt32(3),
                                City = reader.GetString(4)
                            };
                        }
                    }
                }
            }
            return null; // Return null if student is not found
        }
        public Customer GetCustomerDetailUsingSP(int CustomerID)
        {
            // Implement logic to call the stored procedure and retrieve student details
            // Use ADO.NET or an ORM, depending on your preference
            using (var connection = new SqlConnection(_connectionString))
            {
                
                using (var command = new SqlCommand("GetFullNameById", connection))
                {
                    command.CommandType = CommandType.StoredProcedure;
                    command.Parameters.AddWithValue("@Id", CustomerID);
                    connection.Open();

                    var reader = command.ExecuteReader();
                    if (reader.HasRows)
                    {
                        while (reader.Read())
                        {
                            return new Customer
                            {

                                //Id = reader.GetInt32(0),
                                //FirstName = reader.GetString(1),
                               // LastName = reader.GetString(2),
                               // Age = reader.GetInt32(3),
                               // City = reader.GetString(4)
                                //Id = Convert.ToInt16(reader.GetValue(0)),
                                FirstName = Convert.ToString(reader.GetValue(0)),
                                LastName = Convert.ToString(reader.GetValue(1)),
                                //Age = Convert.ToInt16(reader.GetValue(3)),
                               // City = Convert.ToString(reader.GetValue(4))
                            };
                        }
                    }
                       
                    };
                };
            return null; // Return null if student is not found

        }
    }
}
