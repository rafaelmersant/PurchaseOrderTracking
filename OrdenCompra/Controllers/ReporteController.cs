using OrdenCompra.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using Utility;

namespace OrdenCompra.Controllers
{
    public class ReporteController : Controller
    {
        // GET: Reporte
        public ActionResult Index()
        {
            return View();
        }

        public ActionResult Existencia()
        {
            return View();
        }

        public ActionResult Aduana()
        {
            try
            {
                if (Session["role"] == null) return RedirectToAction("Index", "Home");
                
                var db = new OrdenCompraRCEntities();

                var result = db.ReportAduana();
                return View(result);

            }
            catch (Exception ex)
            {
                HelperUtility.SendException(ex);
            }

            return View();
        }

        [HttpPost]
        public JsonResult UpdateSituation(string BL, string situation)
        {
            try
            {
                if (Session["userID"] == null) throw new Exception("505: Por favor intente logearse de nuevo en el sistema. (La Sesión expiró)");

                using (var db = new OrdenCompraRCEntities())
                {
                    if (!string.IsNullOrEmpty(situation))
                    {
                        var containers = db.OrderPurchaseContainers.Where(c => c.BL == BL).ToList();
                        foreach (var container in containers)
                        {
                            container.Situation = situation;
                            db.SaveChanges();
                        }

                        return Json(new { result = "200", message = "Success" });
                    }
                }
            }
            catch (Exception ex)
            {
                HelperUtility.SendException(ex);

                return Json(new { result = "500", message = ex.Message });
            }

            return Json(new { result = "404", message = "NotFound" });
        }
    }
}