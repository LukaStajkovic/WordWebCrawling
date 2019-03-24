using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ScrapingWithDatabase
{
    public static class DatabaseManipulation
    {
        //Insert method for inserting words in database
        public static void Insert(string value, SqlConnection connection)
        {
            try
            {
                //Create SQL command
                SqlCommand commandInsert = new SqlCommand("INSERT INTO Words VALUES (@Words)", connection);
                //Register parameter
                commandInsert.Parameters.AddWithValue("@Words", value);
                //Execute command
                commandInsert.ExecuteNonQuery();
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Problem occcured while trying to insert value in database ! Error: \n\n {ex.ToString()}");
            }
        }
    }
}
