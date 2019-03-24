using MySql.Data.MySqlClient;
using System;
using System.Collections.Generic;
using System.Data.SqlClient;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ScrapingWithDatabase
{
    class Program
    {
        //Since void Main cannot be async I did some research and got way around.
        static void Main(string[] args)
        {
            MainAsync(args).GetAwaiter().GetResult();
        }

        // Consider this as main method
        static async Task MainAsync(string[] args)
        {
            //Make connection with connection string
            SqlConnection connection = new SqlConnection(@"Server=DESKTOP-6FF3SOR\SQLEXPRESS;Database=Words;Trusted_Connection=True");

            //I use this just before the program starts so I could measure the time of my program
            DateTime dateTimePrePocetka = DateTime.Now;

            //Get page number
            int pageNumber = ScrapingHelperClass.GetPageNumber();

            //Create empty list of word objects
            List<WordDto> Words = new List<WordDto>();

            //Open connection to database
            connection.Open();
            //This is loop that will loop through every page on website
            for (int i = 2; i <= pageNumber; i++)
            {
                //Set words from page number [i]
                Words = await ScrapingHelperClass.GetDataFromPageAsync(i);
                //Insert every word in database
                foreach (var word in Words)
                {
                    DatabaseManipulation.Insert(word.Word, connection);
                }
                //Clear words list, so it could get new ones
                Words.Clear();
                Console.WriteLine($"Done with {i} page.");
            }
            //Close connection to database
            connection.Close();
            
            //The time in minutes it took my crawler to finish
            Console.WriteLine($"Minute: {DateTime.Now.Minute - dateTimePrePocetka.Minute}\n");

            Console.WriteLine("Inserted all words!\nPress any key to close program . . .");
            Console.ReadKey();
        }
    }
}
