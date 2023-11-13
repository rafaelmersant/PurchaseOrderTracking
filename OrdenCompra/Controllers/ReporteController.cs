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
                if (Session["role"].ToString() != "Admin") return RedirectToAction("Index", "Home");

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
    }
}