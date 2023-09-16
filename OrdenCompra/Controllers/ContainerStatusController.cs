using System;
using OrdenCompra.Models;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using OrdenCompra.App_Start;

namespace OrdenCompra.Controllers
{
    [Route("ContenedorEstatus")]
    public class ContainerStatusController : Controller
    {
        // GET: ContainerStatus
        public ActionResult Index()
        {
            if (Session["role"] == null) return RedirectToAction("Index", "Home");
            if (Session["role"].ToString() != "Admin") return RedirectToAction("Index", "Home");

            try
            {
                var db = new OrdenCompraRCEntities();

                var status = db.StatusContainers.OrderBy(o => o.Id).ToList();
                return View(status);

            }
            catch (Exception ex)
            {
                Helper.SendException(ex);

                return null;
            }
        }

        public StatusContainer[] GetContainerStatus()
        {
            StatusContainer[] status = new StatusContainer[0];

            try
            {
                var db = new OrdenCompraRCEntities();
                status = db.StatusContainers.ToArray();
            }
            catch (Exception ex)
            {
                Helper.SendException(ex);
            }

            return status;
        }
    }
}