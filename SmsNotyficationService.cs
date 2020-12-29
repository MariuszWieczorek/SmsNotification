using Cipher;
using NLog;
using SmsNotification.Core;
using SmsNotification.Core.Models.Repositories;
using SmsSender;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Configuration;
using System.Data;
using System.Diagnostics;
using System.Linq;
using System.ServiceProcess;
using System.Text;
using System.Threading.Tasks;
using System.Timers;

// wskazówki odnośnie używania metod asynchronicznych
// metodę wywołujemy ze słowem kluczowym await
// metoda nadrzędna musi być oznaczona jako async i zwracać zamiast void Task

namespace SmsNotification
{
    public partial class SmsNotyficationService : ServiceBase
    {
        private static readonly NLog.Logger Logger = NLog.LogManager.GetCurrentClassLogger();
        private Timer _timer; // ważne wybieramy namespace: System.Timers 
        private readonly int _intervalInMinutes;
        
        private ErrorRepository _errorRepository = new ErrorRepository();
        private GenerateMessage _generator = new GenerateMessage();
        private Sms _sms = new Sms();

        private readonly string  token;
        private readonly string  phoneNumber;

        private StringCipher _stringCipher = new StringCipher("163F0C86-673A-426F-97CA-2A60A44134C7");
        private const string NoEncryptedTokenPrefix = "encrypt:";

        public SmsNotyficationService()
        {
            InitializeComponent();
            
            _intervalInMinutes = int.Parse(ConfigurationManager.AppSettings["IntervalInMinutes"]);
            int intervalInMiliseconds = _intervalInMinutes * 60 * 1000;
            _timer = new Timer(intervalInMiliseconds);

            token = ConfigurationManager.AppSettings["Token"];
            // token = DecryptToken();
            phoneNumber = ConfigurationManager.AppSettings["PhoneNumber"];
            
        }

        private string DecryptToken()
        {
            var encryptedToken = ConfigurationManager.AppSettings["Token"];
            if (encryptedToken.StartsWith(NoEncryptedTokenPrefix))
            {
                var tokenToEncrypt = encryptedToken.Replace(NoEncryptedTokenPrefix, string.Empty);
                encryptedToken = _stringCipher.Encrypt(tokenToEncrypt);
                var configFile = ConfigurationManager.OpenExeConfiguration(ConfigurationUserLevel.None);
                configFile.AppSettings.Settings["Token"].Value = encryptedToken;
                configFile.Save();
            }
            return _stringCipher.Decrypt(encryptedToken);
        }

        protected override void OnStart(string[] args)
        {
            _timer.Elapsed += DoWork;
            _timer.Start();
            Logger.Info("Service Started ...");
        }

        // w tym przypadku wyjątkowo musimy pozostawić void zamiast Task
        // bo nie będziemy mogli się podpiąć do _timer.Elapsed
        private async void DoWork(object sender, ElapsedEventArgs e)
        {
            try
            {
               await SendError();
            }
            catch (Exception ex)
            {
                Logger.Error(ex, ex.Message);
                throw new Exception(ex.Message);
            }
        }

        private async Task SendError()
        {
            var errors = _errorRepository.GetLastErrors(_intervalInMinutes);
            if (errors == null || !errors.Any())
                return;

            var message = _generator.GenerateErrors(errors, _intervalInMinutes);

            Logger.Info(token);
            Logger.Info(phoneNumber);
            Logger.Info(message);
            await _sms.Send(message, phoneNumber,token);
            Logger.Info("Error was sent ...");
        }


        protected override void OnStop()
        {
            Logger.Info("Service Stopped ...");
        }
    }
}
