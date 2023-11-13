using RadioCentroServicios.Model;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Utility;

namespace RadioCentroServicios
{
    internal class Program
    {
        static void Main(string[] args)
        {
            string whichServiceToRun = String.Empty;

            if (args != null && args.Length > 0)
                whichServiceToRun = args[0];

            if (whichServiceToRun == "Articulos")
                SendArticlesInventory();

            if (whichServiceToRun == "Suplidores")
                SendProviderManufacturingDate();

            if (whichServiceToRun == "ActualizaArticulos")
                UpdateAllArticleWithCurrentInventory();
        }

        public static void UpdateAllArticleWithCurrentInventory()
        {
            try
            {
                using (var db = new OrdenCompraRCServiceEntities())
                {
                    var inventory = HelperService.GetArticlesInventory();
                    if (inventory.Tables.Count > 0 && inventory.Tables[0].Rows.Count > 0)
                    {
                        Console.WriteLine($"Articulos a procesar: {inventory.Tables[0].Rows.Count}");

                        foreach (DataRow item in inventory.Tables[0].Rows)
                        {
                            try
                            {
                                var articleId = int.Parse(item.ItemArray[0].ToString());
                                var quantity = decimal.Parse(item.ItemArray[1].ToString());

                                var article = db.Articles.FirstOrDefault(a => a.Id == articleId);
                                if (article != null)
                                {
                                    article.InventoryStock = quantity;
                                    db.SaveChanges();

                                    Console.WriteLine($"Actualiza articulo: {article.Id} con inventario: {article.InventoryStock}");
                                }

                                db.InventoryHistories.Add(new InventoryHistory
                                {
                                    ArticleId = articleId,
                                    QuantityAvailable = quantity,
                                    Date = DateTime.Now
                                });
                                db.SaveChanges();
                            }
                            catch (Exception ex)
                            {
                                HelperUtility.SendException(ex, $"articleId: {item.ItemArray[0]} | quantity: {item.ItemArray[1]}");
                            }
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                HelperUtility.SendException(ex);
            }
        }

        private static void SendArticlesInventory()
        {
            try
            {
                using (var db = new OrdenCompraRCServiceEntities())
                {
                    var articleRequested = db.OrderPurchaseArticlesContainers.Where(o => o.OrderPurchase.StatusId != 6);

                    var items = (from a in db.Articles
                                 where !articleRequested.Any(o => o.ArticleId == a.Id)
                                 && a.InventoryStock <= a.QuantityMinStock
                                 select a).ToList();

                    foreach (var item in items)
                    {
                        HelperService.SendNotificationArticleMinimumStock(item.Id, item.Description, item.QuantityMinStock?? 0, item.InventoryStock?? 0);
                        HelperService.SaveEmailNotificationArticle(item.Id, item.QuantityMinStock?? 0, item.InventoryStock?? 0);
                    }
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message);
            }
        }

        private static void SendProviderManufacturingDate()
        {
            try
            {
                int days = int.Parse(ConfigurationManager.AppSettings["daysNotifyProvider"]);

                using (var db = new OrdenCompraRCServiceEntities())
                {
                    var expectedDate = DateTime.Today.AddDays(days);
                    var items = db.OrderPurchaseContainers.Where(c => c.ManufacturingDate <= expectedDate).ToList();

                    foreach(var item in items)
                    {

                        HelperUtility.GetElapsedTime(item.ManufacturingDate ?? DateTime.Today, expectedDate,
                                                     out int _years, out int _months, out int _days, out int _hours, 
                                                     out int _minutes, out int _seconds, out int _milliseconds);

                        HelperService.SendNotificationManufacturingDate(item.OrderPurchase.Provider.ProviderName, _days, item.OrderPurchaseId);
                        HelperService.SaveEmailNotificationProvider(item.OrderPurchaseId, item.ManufacturingDate.Value.ToString("dd/MM/yyyy"), _days);
                    }
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message);
            }
        }
    }
}
