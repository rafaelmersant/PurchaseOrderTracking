using System;
using System.Collections.Generic;
using System.Data.Odbc;
using System.Data;
using System.Linq;
using System.Net.Mail;
using System.Net;
using System.Text;
using System.Threading.Tasks;
using System.Configuration;
using Utility;
using RadioCentroServicios.Model;
using System.Data.Entity.Validation;
using System.Runtime.Remoting.Messaging;

namespace RadioCentroServicios
{
    public class HelperService
    {
        public static void SaveEmailNotificationProvider(int orderId, string manufacturingDate, int elapsedDays)
        {
            try
            {
                using (var db = new OrdenCompraRCServiceEntities())
                {
                    db.NotificationCenters.Add(new NotificationCenter
                    {
                        Type = "Suplidor",
                        SentDate = DateTime.Now,
                        OrderPurchaseId = orderId,
                        Condition = $"Fecha de Fabricación: {manufacturingDate} (Faltan {elapsedDays} días)"
                    });

                    db.SaveChanges();
                }
            }
            catch (Exception ex)
            {
                HelperUtility.SendException(ex);
            }
        }

        public static void SaveEmailNotificationArticle(int articleId, decimal minimumStock, decimal currentQuantity)
        {
            try
            {
                using (var db = new OrdenCompraRCServiceEntities())
                {
                    db.NotificationCenters.Add(new NotificationCenter
                    {
                        Type = "Articulo",
                        SentDate = DateTime.Now,
                        ArticleId = articleId,
                        Condition = $"Mínimo de existencia {minimumStock} (Tiene {currentQuantity} actualmente)",
                        Active = true
                    });

                    db.SaveChanges();
                }
            }
            catch (DbEntityValidationException e)
            {
                HelperUtility.SendException(e.ToString());

                string validErrors = "";
                foreach (var eve in e.EntityValidationErrors)
                {
                    foreach (var ve in eve.ValidationErrors)
                    {
                        validErrors += string.Format("- Property: \"{0}\", Error: \"{1}\" <br/>", ve.PropertyName, ve.ErrorMessage);
                    }
                }

                HelperUtility.SendException(e, "ValidErrors:" + validErrors);
            }
            catch (Exception ex)
            {
                HelperUtility.SendException(ex);
            }
        }

        public static void SendNotificationManufacturingDate(string providerName, int elapsedDays, int orderId)
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
                    EnableSsl = true,
                };

                string formTemplate = System.IO.Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "Templates/");
                formTemplate = System.IO.Path.Combine(formTemplate, "ProviderNotification.html");

                string content = System.IO.File.ReadAllText(formTemplate);

                content = content.Replace("##PROVIDERNAME##", providerName);
                content = content.Replace("##DAYS##", elapsedDays.ToString());
                content = content.Replace("##NOORDEN##", orderId.ToString());

                MailMessage message = new MailMessage();

                AlternateView htmlView = AlternateView.CreateAlternateViewFromString(content, null, "text/html");
                LinkedResource theEmailImage = new LinkedResource(System.IO.Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "Content/Images/", "RCLogo.PNG"));
                theEmailImage.ContentId = "logoID";
                htmlView.LinkedResources.Add(theEmailImage);

                message.AlternateViews.Add(htmlView);
                message.IsBodyHtml = true;
                message.Body = content;
                message.Subject = $"Notificación Suplidor Orden No. {orderId} (Alerta Fecha Fabricación)";
                
                using (var db = new OrdenCompraRCServiceEntities())
                {
                    var sendTo = db.NotificationGroups.FirstOrDefault(g => g.Type == "Suplidor");
                    if (sendTo != null)
                    {
                        var targets = sendTo.SendTo.Split(';');
                        foreach (var target in targets)
                            if (!string.IsNullOrEmpty(target))
                                message.To.Add(new MailAddress(target));
                    }
                }

                string address = ConfigurationManager.AppSettings["EMail"];
                string displayName = ConfigurationManager.AppSettings["EMailName"];
                message.From = new MailAddress(address, displayName);

                smtp.Send(message);
            }
            catch (Exception ex)
            {
                HelperUtility.SendException(ex);
            }
        }

        public static void SendNotificationArticleMinimumStock(int articleId, string articleDescrp, decimal minimumStock, decimal currentQuantity)
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
                    EnableSsl = true,
                };

                string formTemplate = System.IO.Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "Templates/");
                formTemplate = System.IO.Path.Combine(formTemplate, "ArticleNotification.html");

                string content = System.IO.File.ReadAllText(formTemplate);

                content = content.Replace("##ARTICLE##", $"{articleId} - {articleDescrp}");
                content = content.Replace("##QUANTITY##", currentQuantity.ToString());
                content = content.Replace("##MINIMUMSTOCK##", minimumStock.ToString());

                MailMessage message = new MailMessage();

                AlternateView htmlView = AlternateView.CreateAlternateViewFromString(content, null, "text/html");
                LinkedResource theEmailImage = new LinkedResource(System.IO.Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "Content/Images/", "RCLogo.PNG"));
                theEmailImage.ContentId = "logoID";
                htmlView.LinkedResources.Add(theEmailImage);

                message.AlternateViews.Add(htmlView);
                message.IsBodyHtml = true;
                message.Body = content;
                message.Subject = $"Notificación Articulo {articleId} (Alerta stock mínimo)";

                using (var db = new OrdenCompraRCServiceEntities())
                {
                    var sendTo = db.NotificationGroups.FirstOrDefault(g => g.Type == "Suplidor");
                    if (sendTo != null)
                    {
                        var targets = sendTo.SendTo.Split(';');
                        foreach (var target in targets)
                            if (!string.IsNullOrEmpty(target))
                                message.To.Add(new MailAddress(target));
                    }
                }

                string address = ConfigurationManager.AppSettings["EMail"];
                string displayName = ConfigurationManager.AppSettings["EMailName"];
                message.From = new MailAddress(address, displayName);

                smtp.Send(message);
            }
            catch (Exception ex)
            {
                HelperUtility.SendException(ex);
            }
        }

        //GET Articles (Inventory)
        public static DataSet GetArticlesInventory()
        {
            try
            {
                string sQuery = $"SELECT EANUMARTIC, SUM(EAEXACTUAL) EAEXACTUAL FROM [QS36F.RCFAMP00] WHERE EASTATUS != 9 GROUP BY EANUMARTIC";

                if (ConfigurationManager.AppSettings["EnvironmentOrdenCompra"] != "DEV")
                    sQuery = sQuery.Replace("[", "").Replace("]", "");

                return HelperUtility.ExecuteDataSetODBC(sQuery, null);
            }
            catch (Exception ex)
            {
                HelperUtility.SendException(ex);
            }

            return null;
        }
    }
}
