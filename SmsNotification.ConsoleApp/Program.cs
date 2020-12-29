using SmsNotification.Core;
using SmsNotification.Core.Models.Domains;
using SMSApi.Api;
using SmsSender;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Configuration;
using Cipher;

namespace SmsNotification.ConsoleApp
{
    class Program
    {
        static void Main(string[] args)
        {
            int interval = 10;
            var generator = new GenerateMessage();
            var _sms = new Sms();

            string token = ConfigurationManager.AppSettings["Token"];
			// string token = DecryptToken();
			string phoneNumber = ConfigurationManager.AppSettings["PhoneNumber"];
            
           
            // pobranie listy błędów
            var errors = new List<Error>
            {
                new Error {Message = "błąd testowy 1", Date = DateTime.Now},
                new Error {Message = "błąd testowy 2", Date = DateTime.Now},
            };

            var message = generator.GenerateErrors(errors,interval);
            Console.WriteLine(token);
            Console.WriteLine(phoneNumber);
            Console.WriteLine(message);
            
            _sms.Send(message, phoneNumber, token).Wait();
            
			var info = SmsSender2(message, phoneNumber, token);
            Console.WriteLine(info);
			
			Console.ReadLine();
		}

        private static string DecryptToken()
        {
            StringCipher stringCipher = new StringCipher("163F0C86-673A-426F-97CA-2A60A44134C7");
            string noEncryptedTokenPrefix = "encrypt:";

            var encryptedToken = ConfigurationManager.AppSettings["Token"];
            if (encryptedToken.StartsWith(noEncryptedTokenPrefix))
            {
                var tokenToEncrypt = encryptedToken.Replace(noEncryptedTokenPrefix, string.Empty);
                encryptedToken = stringCipher.Encrypt(tokenToEncrypt);
                var configFile = ConfigurationManager.OpenExeConfiguration(ConfigurationUserLevel.None);
                configFile.AppSettings.Settings["Token"].Value = encryptedToken;
                configFile.Save();
            }
            return stringCipher.Decrypt(encryptedToken);
        }

        private static string SmsSender2(string messageText, string phoneNumber, string token)
        {
			string errors = string.Empty;
			try
			{
				IClient client = new ClientOAuth(token);
				var smsApi = new SMSFactory(client);

				// .SetSender(_senderName) //Sender name
				var result =
					smsApi.ActionSend()
						.SetText(messageText)
						.SetTo(phoneNumber)
						.Execute();

				// System.Console.WriteLine("Send: " + result.Count);

				string[] ids = new string[result.Count];

				for (int i = 0, l = 0; i < result.List.Count; i++)
				{
					if (!result.List[i].isError())
					{
						if (!result.List[i].isFinal())
						{
							ids[l] = result.List[i].ID;
							l++;
						}
					}
				}

			}
			catch (SMSApi.Api.ActionException e)
			{
				errors += $"Action error: {e.Message}\n";
			}
			catch (SMSApi.Api.ClientException e)
			{
				/**
				 * Error codes (list available in smsapi docs). Example:
				 * 101 	Invalid authorization info
				 * 102 	Invalid username or password
				 * 103 	Insufficient credits on Your account
				 * 104 	No such template
				 * 105 	Wrong IP address (for IP filter turned on)
				 * 110	Action not allowed for your account
				 */
				errors += $"Client error: {e.Message}\n";
			}
			catch (SMSApi.Api.HostException e)
			{
				/* 
				 * Server errors
				 * SMSApi.Api.HostException.E_JSON_DECODE - problem with parsing data
				 */
				errors += $"Server error: {e.Message}\n";

			}
			catch (SMSApi.Api.ProxyException e)
			{
				// communication problem between client and sever
				errors += $"Proxy error: {e.Message}\n";
			}
			return errors;
		}


    }
}
