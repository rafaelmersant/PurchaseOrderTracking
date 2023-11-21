using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Mail;
using System.Net;
using System.Text;
using System.Threading.Tasks;
using System.Configuration;
using System.Data.Odbc;
using System.Data;

namespace Utility
{
    public static class HelperUtility
    {
        public static bool SendRawEmail(string emailto, string subject, string body)
        {
            try
            {
                ServicePointManager.SecurityProtocol = System.Net.SecurityProtocolType.Tls11 | System.Net.SecurityProtocolType.Tls | System.Net.SecurityProtocolType.Tls12;

                SmtpClient smtp = new SmtpClient
                {
                    Host = ConfigurationManager.AppSettings["smtpClient"],
                    Port = int.Parse(ConfigurationManager.AppSettings["PortMail"]),
                    UseDefaultCredentials = false,
                    DeliveryMethod = SmtpDeliveryMethod.Network,
                    Credentials = new NetworkCredential(ConfigurationManager.AppSettings["usrEmail"], ConfigurationManager.AppSettings["pwdEmail"]),
                    EnableSsl = true,
                };

                MailMessage message = new MailMessage();
                message.IsBodyHtml = true;
                message.Body = body;
                message.Subject = subject;
                message.To.Add(new MailAddress(emailto));

                string address = ConfigurationManager.AppSettings["EMail"];
                string displayName = ConfigurationManager.AppSettings["EMailName"];
                message.From = new MailAddress(address, displayName);


                smtp.Send(message);
                return true;
            }
            catch (Exception ex)
            {
                HelperUtility.SendException(ex);
                Console.WriteLine(ex.ToString());

                return false;
            }
        }

        public static void SendException(Exception ex, string extraInfo = "")
        {
            try
            {
                string _sentry = ConfigurationManager.AppSettings["sentry_dsn"];
                string _environment = ConfigurationManager.AppSettings["sentry_environment"];

                var ravenClient = new SharpRaven.RavenClient(_sentry);
                ravenClient.Environment = _environment;

                var exception = new SharpRaven.Data.SentryEvent(ex);

                if (!string.IsNullOrEmpty(extraInfo))
                    exception.Extra = extraInfo;

                ravenClient.Capture(exception);
            }
            catch (Exception _ex)
            {
                Console.WriteLine(_ex.ToString());
            }
        }

        public static void SendException(string message)
        {
            try
            {
                string _sentry = ConfigurationManager.AppSettings["sentry"];
                string _environment = ConfigurationManager.AppSettings["sentry_environment"];

                var ravenClient = new SharpRaven.RavenClient(_sentry);
                ravenClient.Environment = _environment;
                ravenClient.Capture(new SharpRaven.Data.SentryEvent(message));

            }
            catch (Exception _ex)
            {
                Console.WriteLine(_ex.ToString());
            }
        }

        public static string SHA256(string randomString)
        {
            var crypt = new System.Security.Cryptography.SHA256Managed();
            var hash = new StringBuilder();
            byte[] crypto = crypt.ComputeHash(Encoding.UTF8.GetBytes(randomString));

            foreach (byte theByte in crypto)
            {
                hash.Append(theByte.ToString("X2"));
            }
            return hash.ToString();
        }

        public static bool SendRecoverPasswordEmail(string newPassword, string email)
        {
            try
            {
                ServicePointManager.SecurityProtocol = System.Net.SecurityProtocolType.Tls11 | System.Net.SecurityProtocolType.Tls | System.Net.SecurityProtocolType.Tls12;

                string content = "Su nueva contraseña es: <b>" + newPassword + "</b>";

                SmtpClient smtp = new SmtpClient
                {
                    Host = ConfigurationManager.AppSettings["smtpClient"],
                    Port = int.Parse(ConfigurationManager.AppSettings["PortMail"]),
                    UseDefaultCredentials = false,
                    DeliveryMethod = SmtpDeliveryMethod.Network,
                    Credentials = new NetworkCredential(ConfigurationManager.AppSettings["usrEmail"], ConfigurationManager.AppSettings["pwdEmail"]),
                    EnableSsl = true,
                };

                MailMessage message = new MailMessage();
                message.IsBodyHtml = true;
                message.Body = content;
                message.Subject = "NUEVA CONTRASEÑA SISTEMA DE SEGUIMIENTO A ORDENES DE COMPRA";
                message.To.Add(new MailAddress(email));

                string address = ConfigurationManager.AppSettings["EMail"];
                string displayName = ConfigurationManager.AppSettings["EMailName"];
                message.From = new MailAddress(address, displayName);

                smtp.Send(message);

                return true;
            }
            catch (Exception ex)
            {
                HelperUtility.SendException(ex);

                return false;
            }
        }

        public static void GetElapsedTime(DateTime from_date, DateTime to_date,
        out int years, out int months, out int days, out int hours,
        out int minutes, out int seconds, out int milliseconds)
        {
            // If from_date > to_date, switch them around.
            if (from_date > to_date)
            {
                GetElapsedTime(to_date, from_date,
                    out years, out months, out days, out hours,
                    out minutes, out seconds, out milliseconds);
                years = -years;
                months = -months;
                days = -days;
                hours = -hours;
                minutes = -minutes;
                seconds = -seconds;
                milliseconds = -milliseconds;
            }
            else
            {
                // Handle the years.
                years = to_date.Year - from_date.Year;

                // See if we went too far.
                DateTime test_date = from_date.AddMonths(12 * years);
                if (test_date > to_date)
                {
                    years--;
                    test_date = from_date.AddMonths(12 * years);
                }
                // Add months until we go too far.
                months = 0;
                while (test_date <= to_date)
                {
                    months++;
                    test_date = from_date.AddMonths(12 * years + months);
                }
                months--;

                // Subtract to see how many more days,
                // hours, minutes, etc. we need.
                from_date = from_date.AddMonths(12 * years + months);
                TimeSpan remainder = to_date - from_date;
                days = remainder.Days;
                hours = remainder.Hours;
                minutes = remainder.Minutes;
                seconds = remainder.Seconds;
                milliseconds = remainder.Milliseconds;
            }
        }

        public static DataSet ExecuteDataSetODBC(string query, OdbcParameter[] parameters = null)
        {
            string sConn = ConfigurationManager.AppSettings["sConnSQLODBC"];

            OdbcConnection oconn = new OdbcConnection(sConn);
            OdbcCommand ocmd = new OdbcCommand(query, oconn);
            OdbcDataAdapter adapter;
            DataSet dsData = new DataSet();

            ocmd.CommandType = CommandType.Text;

            if (parameters != null)
            {
                ocmd.Parameters.Clear();

                foreach (OdbcParameter param in parameters)
                    ocmd.Parameters.Add(param);
            }

            adapter = new OdbcDataAdapter(ocmd);
            adapter.Fill(dsData);

            return dsData;
        }
    }
}
