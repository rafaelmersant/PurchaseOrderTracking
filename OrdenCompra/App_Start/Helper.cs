using OrdenCompra.Models;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data;
using System.Data.Odbc;
using System.Linq;
using System.Net;
using System.Net.Mail;
using System.Text;
using System.Web;
using System.Web.Mvc;

namespace OrdenCompra.App_Start
{
    public class Helper
    {
        public static bool SendRawEmail(string emailto, string subject, string body)
        {
            try
            {
                SmtpClient smtp = new SmtpClient
                {
                    Host = ConfigurationManager.AppSettings["smtpClient"],
                    Port = int.Parse(ConfigurationManager.AppSettings["PortMail"]),
                    UseDefaultCredentials = false,
                    DeliveryMethod = SmtpDeliveryMethod.Network,
                    Credentials = new NetworkCredential(ConfigurationManager.AppSettings["usrEmail"], ConfigurationManager.AppSettings["pwdEmail"]),
                    EnableSsl = false,
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
                Helper.SendException(ex);

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
                string content = "Su nueva contraseña es: <b>" + newPassword + "</b>";

                SmtpClient smtp = new SmtpClient
                {
                    Host = ConfigurationManager.AppSettings["smtpClient"],
                    Port = int.Parse(ConfigurationManager.AppSettings["PortMail"]),
                    UseDefaultCredentials = false,
                    DeliveryMethod = SmtpDeliveryMethod.Network,
                    Credentials = new NetworkCredential(ConfigurationManager.AppSettings["usrEmail"], ConfigurationManager.AppSettings["pwdEmail"]),
                    EnableSsl = false,
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
                Helper.SendException(ex);

                return false;
            }
        }

        public static void AddNewArticle(DataRow article)
        {
            try
            {
                using (var db = new OrdenCompraRCEntities())
                {
                    decimal _inventoryStock = string.IsNullOrEmpty(article.ItemArray[5].ToString()) ? 0M : decimal.Parse(article.ItemArray[5].ToString());
                    decimal _maxPerContainer = string.IsNullOrEmpty(article.ItemArray[6].ToString()) ? 0M : decimal.Parse(article.ItemArray[6].ToString());

                    db.Articles.Add(new Article()
                    {
                        Id = int.Parse(article.ItemArray[0].ToString()),
                        Description = article.ItemArray[1].ToString(),
                        MarkId = int.Parse(article.ItemArray[3].ToString()),
                        Model = article.ItemArray[4].ToString(),
                        InventoryStock = _inventoryStock,
                        QuantityMaxPerContainer = _maxPerContainer,
                        AddedBy = 1,
                        AddedDate = DateTime.Now
                    });
                    db.SaveChanges();
                }
            }
            catch (Exception ex)
            {
                Helper.SendException(ex);
            }
        }

        //GET Article by Id
        public static DataSet GetArticleById(int id)
        {
            try
            {
                string sQuery = $"SELECT MPNARTICUL, MPDESCRIPC, MPUMAYMED, MPNUMMARCA, MPMODELO, MPSTOCKCRI, MPSTOCKMAX FROM [QS36F.RCFAMP00] WHERE MPNARTICUL = {id}";

                if (ConfigurationManager.AppSettings["EnvironmentOrdenCompra"] != "DEV")
                    sQuery = sQuery.Replace("[", "").Replace("]", "");

                return ExecuteDataSetODBC(sQuery, null);
            }
            catch (Exception ex)
            {
                Helper.SendException(ex);
            }

            return null;
        }

        //GET Purchase Order Header
        public static DataSet GetOrderPurchaseHeader(int purchaseOrderId)
        {
            try
            {
                string sQuery = $"SELECT PDADNUMPED, PDADFECPED, PDADCOSUP1, PDADOBSER1, PDADFAPERT, PDADHAPERT FROM [QS36F.RCADPD00] WHERE PDADNUMPED= {purchaseOrderId}";

                if (ConfigurationManager.AppSettings["EnvironmentOrdenCompra"] != "DEV")
                    sQuery = sQuery.Replace("[", "").Replace("]", "");

                return ExecuteDataSetODBC(sQuery, null);
            }
            catch (Exception ex)
            {
                Helper.SendException(ex);
            }

            return null;
        }

        //GET Purchase Order Detail Requested
        public static DataSet GetOrderPurchaseDetail(int purchaseOrderId)
        {
            try
            {
                string sQuery = $"SELECT DPADNUMPED, DPADNUMART, DPADFECPED, DPADFECREC, DPCANTIDAD, DPCANTFABR, " +
                                $"DPCANTTRAN, DPCANTPEND, DPCANTADUA, DPADPRECV1 FROM [QS36F.RCADDP10] WHERE DPADTIPTRA='P' AND DPADNUMPED = {purchaseOrderId}";

                if (ConfigurationManager.AppSettings["EnvironmentOrdenCompra"] != "DEV")
                    sQuery = sQuery.Replace("[", "").Replace("]", "");

                return ExecuteDataSetODBC(sQuery, null);
            }
            catch (Exception ex)
            {
                Helper.SendException(ex);
            }

            return null;
        }

        //GET Purchase Order Detail Received
        public static DataSet GetOrderPurchaseDetailReceived(int purchaseOrderId)
        {
            try
            {
                string sQuery = $"SELECT DPADNUMPED, DPADNUMART, DPADFECPED, DPADFECREC, DPCANTIDAD, DPCANTFABR, " +
                                $"DPCANTTRAN, DPCANTPEND, DPCANTADUA, DPADPRECV1 FROM [QS36F.RCADDP00] WHERE DPADTIPTRA='R' AND DPADNUMPED = {purchaseOrderId}";

                if (ConfigurationManager.AppSettings["EnvironmentOrdenCompra"] != "DEV")
                    sQuery = sQuery.Replace("[", "").Replace("]", "");

                return ExecuteDataSetODBC(sQuery, null);
            }
            catch (Exception ex)
            {
                Helper.SendException(ex);
            }

            return null;
        }

        //GET Providers
        public static DataSet GetProviders()
        {
            try
            {
                string sQuery = $"SELECT OCMCODI, OCMNOMB FROM [IFUENTELIB.OCMPROV00] WHERE OCMCODI >= 20000 AND OCMCODI <= 59999";

                if (ConfigurationManager.AppSettings["EnvironmentOrdenCompra"] != "DEV")
                    sQuery = sQuery.Replace("[", "").Replace("]", "");

                return ExecuteDataSetODBC(sQuery, null);
            }
            catch (Exception ex)
            {
                Helper.SendException(ex);
            }

            return null;
        }

        //GET Articles
        public static DataSet GetArticles()
        {
            try
            {
                string sQuery = $"SELECT MPNARTICUL, MPDESCRIPC, MPNUMMARCA, MPMODELO, MPSTOCKCRI, MPSTOCKMAX FROM [QS36F.RCFAMP00] WHERE MPCOREGIST != 9";

                if (ConfigurationManager.AppSettings["EnvironmentOrdenCompra"] != "DEV")
                    sQuery = sQuery.Replace("[", "").Replace("]", "");

                return ExecuteDataSetODBC(sQuery, null);
            }
            catch (Exception ex)
            {
                Helper.SendException(ex);
            }

            return null;
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

        public static void SendEmailVacationNotification(string EmailResponsableDepto, string NombreColaborador, string PuestoColaborador,
                                                         string FechaInicio, string FechaHasta, string FechaRetorno, string RedirectURL)
        {
            try
            {
                SmtpClient smtp = new SmtpClient
                {
                    Host = ConfigurationManager.AppSettings["smtpClient"],
                    Port = int.Parse(ConfigurationManager.AppSettings["PortMail"]),
                    UseDefaultCredentials = false,
                    DeliveryMethod = SmtpDeliveryMethod.Network,
                    Credentials = new NetworkCredential(ConfigurationManager.AppSettings["usrEmail"], ConfigurationManager.AppSettings["pwdEmail"]),
                    EnableSsl = false,
                };

                string formTemplate = System.IO.Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "Templates/");
                formTemplate = System.IO.Path.Combine(formTemplate, "NotificacionSolicitud.html");

                string content = System.IO.File.ReadAllText(formTemplate);

                content = content.Replace("##NombreColaborador##", NombreColaborador);
                content = content.Replace("##PuestoColaborador##", PuestoColaborador);
                content = content.Replace("##FechaInicio##", FechaInicio);
                content = content.Replace("##FechaHasta##", FechaHasta);
                content = content.Replace("##FechaRetorno##", FechaRetorno);
                content = content.Replace("##RedirectTo##", RedirectURL);

                MailMessage message = new MailMessage();

                AlternateView htmlView = AlternateView.CreateAlternateViewFromString(content, null, "text/html");
                LinkedResource theEmailImage = new LinkedResource(System.IO.Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "Content/Images/", "RCLogo.PNG"));
                theEmailImage.ContentId = "logoID";
                htmlView.LinkedResources.Add(theEmailImage);

                message.AlternateViews.Add(htmlView);
                message.IsBodyHtml = true;
                //message.Body = content;
                message.Subject = "Nueva Solicitud de Vacaciones";
                message.To.Add(new MailAddress(EmailResponsableDepto));

                string address = ConfigurationManager.AppSettings["EMail"];
                string displayName = ConfigurationManager.AppSettings["EMailName"];
                message.From = new MailAddress(address, displayName);

                smtp.Send(message);
            }
            catch (Exception ex)
            {
                Helper.SendException(ex);
            }
        }
    }
}