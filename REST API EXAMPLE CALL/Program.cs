using Newtonsoft.Json.Linq;
using System;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Text;

namespace REST_API_EXAMPLE_CALL
{
    internal class Program
    {
        static HttpClient httpClient;
        static string bearerToken;
        static string apiUrl = "https://127.0.0.1:50301/api";

        static string banner = @"   _____           _____ _    _ __  __       _______ _____ _____            _____ _____ 
  / ____|   /\    / ____| |  | |  \/  |   /\|__   __|_   _/ ____|     /\   |  __ \_   _|
 | |       /  \  | (___ | |__| | \  / |  /  \  | |    | || |         /  \  | |__) || |  
 | |      / /\ \  \___ \|  __  | |\/| | / /\ \ | |    | || |        / /\ \ |  ___/ | |  
 | |____ / ____ \ ____) | |  | | |  | |/ ____ \| |   _| || |____   / ____ \| |    _| |_ 
  \_____/_/    \_\_____/|_|  |_|_|  |_/_/    \_\_|  |_____\_____| /_/    \_\_|   |_____|
                                                                                        
                                                                                        ";

        static void Main(string[] args)
        {
            string ip = "127.0.0.1";

            Console.Write("IP ADDRESS (Empty if using Simulator): ");
            ip = Console.ReadLine().Trim();
            
            if(ip != "")
            {
                apiUrl = "https://" + ip + ":50301/api";
            }

            log("IP ADDRESS SET: " + apiUrl);

            HttpClientHandler handler = new HttpClientHandler();//creating a handler for my HttpClient
            handler.ServerCertificateCustomValidationCallback = HttpClientHandler.DangerousAcceptAnyServerCertificateValidator;//ignoring invalid SSL certificates (Cashmatic is on HTTP)
            httpClient = new HttpClient(handler);//init

            Console.ForegroundColor = ConsoleColor.Blue;
            Console.WriteLine(banner);
            Console.ForegroundColor = ConsoleColor.Gray;
            bool loop = true;

            while (loop)
            {
                Console.ForegroundColor = ConsoleColor.Yellow;
                Console.WriteLine(Environment.NewLine);
                Console.WriteLine("[1] login");
                Console.WriteLine("[2] start payment");
                Console.WriteLine("[3] cancelpayment");
                Console.WriteLine("[4] startrefill");
                Console.WriteLine("[5] stoprefill");
                Console.WriteLine("[6] active transaction");
                Console.WriteLine("[7] QUIT");
                Console.Write("PLEASE WRITE A COMMAND: ");
                Console.ForegroundColor = ConsoleColor.Gray;

                string[] read = Console.ReadLine().Split(' ');

                switch (read[0].ToString()) {
                    case "1":
                        log("LOGIN COMMAND");
                        var username = "";
                        var password = "";

                        Console.Write("USERNAME: ");
                        username = Console.ReadLine();

                        Console.Write("PASSWORD: ");
                        password = Console.ReadLine();
            
                        Login(username, password);
                        break;
                    case "2":
                        log("PAYMENT COMMAND");
                        var amount = 0;
                        while (amount <= 0)
                        {
                            Console.Write("AMOUNT IN CENTS: ");
                            try
                            {
                                amount = Convert.ToInt32(Console.ReadLine());
                            }
                            catch (Exception) { amount = 0; }
                        }
                        Payment(amount);
                        break;
                    case "3":
                        log("CANCEL PAYMENT COMMAND");
                        CancelPayment();
                        break;
                    case "4":
                        log("START REFILL COMMAND");
                        StartRefill();
                        break;
                    case "5":
                        log("STOP REFILL COMMAND");
                        StopRefill();
                        break;
                    case "6":
                        log("ACTIVE TRANSACTION");
                        ActiveTransaction();
                        break;
                    case "7":
                        error("QUITTING");
                        loop = false;
                        break;

                    default:
                        error("Command not found.");
                        break;
                }
            }

            Console.ReadKey();
        }
        static HttpResponseMessage SendCommand(string command_url, string content)
        {
            var body = new StringContent(content, Encoding.UTF8, "application/json");

            if (command_url != "/user/Login")//no need to use token for Login command
            {
                httpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", bearerToken);
            }

            HttpResponseMessage response = null;

            try
            {
                 response = httpClient.PostAsync(apiUrl + command_url, body).Result;
                log("RESPONSE CODE : " + response.StatusCode);

                Console.WriteLine(response.Content.ReadAsStringAsync().Result);
            }
            catch (Exception ex){
                error(ex.Message);
            }


            return response;
        }

        static void Login(string username, string password)
        {
            var json = new JObject();
            json["username"] = username;
            json["password"] = password;

            var response = SendCommand("/user/Login", json.ToString());

            if (response is null)
            {
                error("Failed sending the request");
                return;
            }

            JObject responseBody = JObject.Parse(response.Content.ReadAsStringAsync().Result);

            bearerToken = responseBody["data"]["token"].ToString();

            log("- BEARER TOKEN SAVED");
        }
        static void Payment(int amount)//amount in cents
        {
            var json = new JObject();
            json["amount"] = amount;

            SendCommand("/transaction/StartPayment", json.ToString());
        }

        static void CancelPayment()
        {
            var response = SendCommand("/transaction/CancelPayment", new JObject().ToString());
        }

        static void StartRefill()
        {
            var response = SendCommand("/transaction/StartRefill", new JObject().ToString());
        }

        static void StopRefill()
        {
            var response = SendCommand("/transaction/StopRefill", new JObject().ToString());
        }
        static void ActiveTransaction()
        {
            var response = SendCommand("/device/ActiveTransaction", new JObject().ToString());
        }

        static void log(string msg)
        {
            Console.ForegroundColor = ConsoleColor.Blue;
            Console.WriteLine(msg + Environment.NewLine);
            Console.ForegroundColor = ConsoleColor.Gray;
        }

        static void error(string msg)
        {
            Console.ForegroundColor = ConsoleColor.Red;
            Console.WriteLine(msg + Environment.NewLine);
            Console.ForegroundColor = ConsoleColor.Gray;
        }
    }
}
