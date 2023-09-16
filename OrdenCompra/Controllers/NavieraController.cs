using OrdenCompra.App_Start;
using OrdenCompra.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace OrdenCompra.Controllers
{
    public class NavieraController : Controller
    {
        // GET: Naviera
        public ActionResult Index()
        {
            if (Session["role"] == null) return RedirectToAction("Index", "Home");
            if (Session["role"].ToString() != "Admin") return RedirectToAction("Index", "Home");

            try
            {
                var db = new OrdenCompraRCEntities();

                var navieras = db.ShippingCompanies.OrderBy(o => o.Description).ToList();
                return View(navieras);

            }
            catch (Exception ex)
            {
                Helper.SendException(ex);

                return null;
            }
        }

        public ShippingCompany[] GetNavieras()
        {
            ShippingCompany[] navieras = new ShippingCompany[0];

            try
            {
                var db = new OrdenCompraRCEntities();
                navieras = db.ShippingCompanies.ToArray();
            }
            catch (Exception ex)
            {
                Helper.SendException(ex);
            }

            return navieras;
        }
    }
}