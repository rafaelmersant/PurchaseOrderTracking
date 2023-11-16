using OrdenCompra.App_Start;
using OrdenCompra.Models;
using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using Utility;

namespace OrdenCompra.Controllers
{
    [Route("Suplidores")]
    public class ProviderController : Controller
    {
        // GET: Provider
        public ActionResult Index()
        {
            if (Session["role"] == null) return RedirectToAction("Index", "Home");
            if (Session["role"].ToString() != "Admin") return RedirectToAction("Index", "Home");

            try
            {
                var db = new OrdenCompraRCEntities();

                var ordersOpened = db.OrderPurchases.Where(o => o.StatusId == 1);
                var ordersProduction = db.OrderPurchases.Where(o => o.StatusId == 2);
                var ordersTransit = db.OrderPurchases.Where(o => o.StatusId == 3);
                var ordersOnPort = db.OrderPurchases.Where(o => o.StatusId == 4);
                var ordersClosed = db.OrderPurchases.Where(o => o.StatusId == 6);

                var providers = db.Providers.OrderBy(o => o.ProviderName).ToList();

                foreach (var provider in providers)
                {
                    provider.OrdersOpened = ordersOpened.Where(o => o.ProviderId == provider.ProviderCode).Count();
                    provider.OrdersProduction = ordersProduction.Where(o => o.ProviderId == provider.ProviderCode).Count();
                    provider.OrdersTransit = ordersTransit.Where(o => o.ProviderId == provider.ProviderCode).Count();
                    provider.OrdersOnPort = ordersOnPort.Where(o => o.ProviderId == provider.ProviderCode).Count();
                    provider.OrdersClosed = ordersClosed.Where(o => o.ProviderId == provider.ProviderCode).Count();
                    db.SaveChanges();
                }

                providers = db.Providers.OrderBy(o => o.ProviderName).ToList();
                return View(providers);
            }
            catch (Exception ex)
            {
                HelperUtility.SendException(ex);

                return null;
            }
        }

        [HttpPost]
        public JsonResult UpdateProviders()
        {
            try
            {
                if (Session["userID"] == null) throw new Exception("505: Por favor intente logearse de nuevo en el sistema. (La Sesión expiró)");

                var providers = HelperApp.GetProviders();
                if (providers != null && providers.Tables.Count > 0 && providers.Tables[0].Rows.Count > 0)
                {
                    var _providers = providers.Tables[0];
                    using (var db = new OrdenCompraRCEntities())
                    {
                        var __provider__ = db.Providers.ToList();

                        foreach (DataRow __provider in _providers.Rows)
                        {
                            int providerCode = int.Parse(__provider.ItemArray[0].ToString());
                            var provider = __provider__.FirstOrDefault(p => p.ProviderCode == providerCode);
                            if (provider == null)
                            {
                                db.Providers.Add(new Provider
                                {
                                    ProviderCode = providerCode,
                                    ProviderName = __provider.ItemArray[1].ToString(),
                                    CreatedDate = DateTime.Now
                                });

                                db.SaveChanges();
                            } 
                            else
                            {
                                if (provider.ProviderName != __provider.ItemArray[1].ToString())
                                {
                                    provider.ProviderName = __provider.ItemArray[1].ToString();
                                    db.SaveChanges();
                                }
                            }
                        }
                    }

                    return Json(new { result = "200", message = "Suplidores actualizados con exito!" });
                }
                else
                {
                    return Json(new { result = "404", message = "No se encontro ningun suplidor" });
                }

            }
            catch (Exception ex)
            {
                HelperUtility.SendException(ex);

                return Json(new { result = "500", message = ex.Message });
            }

        }
    }
}