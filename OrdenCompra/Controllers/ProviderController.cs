using OrdenCompra.App_Start;
using OrdenCompra.Models;
using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Web;
using System.Web.Mvc;

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

                var providers = db.Providers.OrderBy(o => o.ProviderName).ToList();
                return View(providers);

            }
            catch (Exception ex)
            {
                Helper.SendException(ex);

                return null;
            }
        }

        [HttpPost]
        public JsonResult UpdateProviders()
        {
            try
            {
                if (Session["userID"] == null) throw new Exception("Por favor intente logearse de nuevo en el sistema. (La Sesión expiró)");

                var providers = Helper.GetProviders();
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
                Helper.SendException(ex);

                return Json(new { result = "500", message = ex.Message });
            }

        }
    }
}