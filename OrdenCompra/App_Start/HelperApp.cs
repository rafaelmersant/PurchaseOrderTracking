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
using Utility;

namespace OrdenCompra.App_Start
{
    public class HelperApp
    {
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
               HelperUtility.SendException(ex);
            }
        }

        public static void SaveTimeLineOrder(int orderId, string source, string comment, int userLogged)
        {
            try
            {
                using (var db = new OrdenCompraRCEntities())
                {
                    db.TimeLineOrders.Add(new TimeLineOrder
                    {
                        OrderId = orderId,
                        Source = source,
                        Comment = comment,
                        CreatedDate = DateTime.Now,
                        CreatedBy = userLogged
                    });
                    db.SaveChanges();
                }
            }
            catch (Exception ex)
            {
                HelperUtility.SendException(ex, $"orderId:{orderId}");
            }
        }

        //GET Article by Id
        public static DataSet GetArticleById(int id)
        {
            try
            {
                string sQuery = $"SELECT MAADNUMART, MAADDESART, MAADUNIMAY, MAADMARCA, MAADMODELO, MAADSTMICO, MAADCANXFU FROM [QS36F.RCADMA00] WHERE MAADNUMART = {id}";

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
        
        //GET Purchase Order Header
        public static DataSet GetOrderPurchaseHeader(int purchaseOrderId)
        {
            try
            {
                string sQuery = $"SELECT PDADNUMPED, PDADFECPED, PDADCOSUP1, PDADOBSER1, PDADFAPERT, PDADHAPERT FROM [QS36F.RCADPD00] WHERE PDADNUMPED= {purchaseOrderId}";

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

        //GET Purchase Order Detail Requested
        public static DataSet GetOrderPurchaseDetail(int purchaseOrderId)
        {
            try
            {
                string sQuery = $"SELECT DPADNUMPED, DPADNUMART, DPADFECPED, DPADFECREC, DPCANTIDAD, DPCANTFABR, " +
                                $"DPCANTTRAN, DPCANTPEND, DPCANTADUA, DPADPRECV1 FROM [QS36F.RCADDP10] WHERE DPADCODELI != 9 AND DPADTIPTRA='P' AND DPADNUMPED = {purchaseOrderId}";

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

        //GET Purchase Order Detail Received
        public static DataSet GetOrderPurchaseDetailReceived(int purchaseOrderId)
        {
            try
            {
                string sQuery = $"SELECT DPADNUMPED, DPADNUMART, DPADFECPED, DPADFECREC, DPCANTIDAD, DPCANTFABR, " +
                                $"DPCANTTRAN, DPCANTPEND, DPCANTADUA, DPADPRECV1 FROM [QS36F.RCADDP00] WHERE DPADTIPTRA='R' AND DPADNUMPED = {purchaseOrderId}";

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

        //GET Providers
        public static DataSet GetProviders()
        {
            try
            {
                string sQuery = $"SELECT OCMCODI, OCMNOMB FROM [IFUENTELIB.OCMPROV00] WHERE OCMCODI >= 20000 AND OCMCODI <= 59999";

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

        //GET Articles
        public static DataSet GetArticles()
        {
            try
            {
                string sQuery = $"SELECT MAADNUMART, MAADDESART, MAADMARCA, MAADMODELO, MAADSTMICO, MAADCANXFU FROM [QS36F.RCADMA00] WHERE MAADSTAT != 9";

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