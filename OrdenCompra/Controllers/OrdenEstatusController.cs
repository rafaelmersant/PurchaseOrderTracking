using System;
using OrdenCompra.Models;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using OrdenCompra.App_Start;

namespace OrdenCompra.Controllers
{
    [Route("OrdenEstatus")]
    public class OrdenEstatusController : Controller
    {
        // GET: OrdenEstatus
        public ActionResult Index()
        {
            if (Session["role"] == null) return RedirectToAction("Index", "Home");
            if (Session["role"].ToString() != "Admin") return RedirectToAction("Index", "Home");

            try
            {
                var db = new OrdenCompraRCEntities();

                var status = db.StatusOrderPurchases.OrderBy(o => o.Id).ToList();
                return View(status);

            }
            catch (Exception ex)
            {
                Helper.SendException(ex);

                return null;
            }
        }

        public List<SelectListItem> GetOrderStatus()
        {
            List<SelectListItem> status = new List<SelectListItem>();

            try
            {
                var db = new OrdenCompraRCEntities();
                status.Add(new SelectListItem { Text = "", Value = "" });
                var _status = db.StatusOrderPurchases.ToArray();
                foreach (var item in _status)
                    status.Add(new SelectListItem { Text = item.Description, Value = item.Id.ToString() });
            }
            catch (Exception ex)
            {
                Helper.SendException(ex);
            }

            return status;
        }
    }
}