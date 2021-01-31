using System;
using Workflows.Models.DataEvents;

namespace Models
{
    class Program
    {
        static void Main(string[] args)
        {
            var helloRequest = new DataLocalization
            {
                Region = "region1"
            };

            Console.WriteLine($"Hello {helloRequest.Region}!");
        }
    }
}