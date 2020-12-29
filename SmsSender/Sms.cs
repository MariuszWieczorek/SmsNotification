using SMSApi.Api;
using NLog;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SmsSender
{
    public class Sms
    {
		private static readonly NLog.Logger Logger = NLog.LogManager.GetCurrentClassLogger();
		public async Task Send(string messageText, string phoneNumber, string token)
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

			
		}

    }
}
