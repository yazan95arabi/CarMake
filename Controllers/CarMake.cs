using Microsoft.AspNetCore.Mvc;
using System.Data;
using System;
using System.Collections.Generic;
using Newtonsoft.Json;
using System.Formats.Asn1;
using System.Globalization;
using System.IO;
using CsvHelper;
using CsvHelper.Configuration;
using System.Linq;
using System.Net.Http;
using System.Threading.Tasks;

namespace GetCarMake.Controllers
{
    [Route("api/")]
    [ApiController]
    public class CarMake : Controller
    {
        string rootDirectory = AppContext.BaseDirectory;

        //https://localhost:44343/api/index
        [HttpGet]
        [Route("index/")]
        public IActionResult Index()
        {
            return Ok("Welcome To Carseer Company API");
        }

        //https://localhost:44343/api/models?modelyear=2015&make=Honda
        [HttpGet]
        [Route("models/")]
        public async Task<IActionResult> GetCarMakeAsync([FromQuery] string modelyear, [FromQuery] string make)
        {
            DataTable dt = new DataTable();
            string JsonResult = "";

            string filePath = Path.Combine(rootDirectory, "CarMake.csv");
            var dataDictionary = ReadCsvFileToDictionary(filePath);

            foreach (var item in dataDictionary)
            {
                if (make.ToLower() == (item.Value).ToLower())
                {
                    JsonResult = await CallApiWithParameters(modelyear, item.Key);
                    break;
                }
            }

            return Ok(JsonResult);
        }

        static async Task<string> CallApiWithParameters(string modelyear, string makeId)
        {
            // Create an instance of HttpClient
            using (HttpClient httpClient = new HttpClient())
            {
                try
                {
                    // Construct the API URL with parameters
                    string apiUrl = "https://vpic.nhtsa.dot.gov/api/vehicles/GetModelsForMakeIdYear/makeId/" + makeId + "/modelyear/" + modelyear + "?format=json";

                    // Send a GET request and wait for the response
                    HttpResponseMessage httpResponse = await httpClient.GetAsync(apiUrl);

                    // Check if the response is successful (status code 200)
                    if (httpResponse.IsSuccessStatusCode)
                    {
                        // Read the content of the response as a string
                        string responseBody = await httpResponse.Content.ReadAsStringAsync();
                        return responseBody;
                    }
                    else
                    {
                        Console.WriteLine($"Error: {httpResponse.StatusCode}");
                        return null;
                    }
                }
                catch (HttpRequestException ex)
                {
                    Console.WriteLine($"HTTP request error: {ex.Message}");
                    return null;
                }
            }
        }

        public Dictionary<string, string> ReadCsvFileToDictionary(string filePath)
        {
            var result = new Dictionary<string, string>();
            try
            {
                using (var reader = new StreamReader(filePath))
                using (var csv = new CsvReader(reader, new CsvConfiguration(CultureInfo.InvariantCulture)))
                {
                    var records = csv.GetRecords<CsvData>().ToList();

                    // Create a dictionary from the CSV data
                    var dictionary = records.ToDictionary(record => record.make_id, record => record.make_name);

                    result = dictionary;
                }
            }
            catch(Exception ex)
            {
                string error = ex.Message;
            }

            return result;
        }
    }

    public class CsvData
    {
        public string make_id { get; set; }
        public string make_name { get; set; }
    }
}
