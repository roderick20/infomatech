using Newtonsoft.Json;
using OpenTokSDK;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Threading.Tasks;

namespace SealWebRTC.Data
{
    public class OpenTokService
    {
        public Session Session { get; set; }
        public OpenTok OpenTok { get; set; }

        [JsonConstructor]
        public OpenTokService()
        {
        }

            public void Generar(string apiKeyString, string apiSecret)
        {
            int apiKey = 0;
            //string apiSecret = null;
            try
            {
                //string apiKeyString = "47356491";// ConfigurationManager.AppSettings["API_KEY"];
                //apiSecret = "9e4709c439735fb885262f6df0159f943a1b47b7";// ConfigurationManager.AppSettings["API_SECRET"];
                apiKey = Convert.ToInt32(apiKeyString);

                   
            }

            catch (Exception ex)
            {
                //if (!(ex is ConfigurationErrorsException || ex is FormatException || ex is OverflowException))
                //{
                //    throw ex;
                //}
            }

            finally
            {
                if (apiKey == 0 || apiSecret == null)
                {
                    Console.WriteLine(
                        "The OpenTok API Key and API Secret were not set in the application configuration. " +
                        "Set the values in App.config and try again. (apiKey = {0}, apiSecret = {1})", apiKey, apiSecret);
                    Console.ReadLine();
                    Environment.Exit(-1);
                }
            }

            ServicePointManager.SecurityProtocol = SecurityProtocolType.Tls12;

            this.OpenTok = new OpenTok(apiKey, apiSecret);

            //this.Session = this.OpenTok.CreateSession();
            this.Session = this.OpenTok.CreateSession(mediaMode: MediaMode.ROUTED);
        }
    }
}
