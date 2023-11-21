using OrdenCompra.App_Start;
using OrdenCompra.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using Utility;

namespace OrdenCompra.Controllers
{
    public class NotificacionController : Controller
    {
        // GET: Notificacion
        public ActionResult Index()
        {
            if (Session["role"] == null) return RedirectToAction("Index", "Home");
            
            try
            {
                var db = new OrdenCompraRCEntities();

                var notifications = db.NotificationCenters.Where(n => n.Active).OrderByDescending(o => o.Id).ToList();
                return View(notifications);
            }
            catch (Exception ex)
            {
                HelperUtility.SendException(ex);

                return null;
            }
        }

        public ActionResult Grupos()
        {
            if (Session["role"] == null) return RedirectToAction("Index", "Home");
            
            try
            {
                var db = new OrdenCompraRCEntities();

                var groups = db.NotificationGroups.OrderBy(o => o.Id).ToList();
                return View(groups);
            }
            catch (Exception ex)
            {
                HelperUtility.SendException(ex);

                return null;
            }
        }

        [HttpPost]
        public JsonResult UpdateNotificationStatus(int id)
        {
            try
            {
                if (Session["userID"] == null) throw new Exception("505: Por favor intente logearse de nuevo en el sistema. (La Sesión expiró)");

                using (var db = new OrdenCompraRCEntities())
                {
                    var notification = db.NotificationCenters.FirstOrDefault(n => n.Id == id);
                    if (notification == null) return Json(new { result = "404", message = "Notificación no encontrada." });

                    notification.Active = false;
                    notification.DeactivatedDate = DateTime.Now;
                    notification.DeactivatedBy = int.Parse(Session["userID"].ToString());
                    db.SaveChanges();
                }

                return Json(new { result = "200", message = "success" });
            }
            catch (Exception ex)
            {
                HelperUtility.SendException(ex, $"notificacion Id: {id}");

                return Json(new { result = "500", message = ex.Message });
            }
        }

        [HttpPost]
        public JsonResult AddEmail(int groupId, string email)
        {
            try
            {
                if (Session["userID"] == null) throw new Exception("505: Por favor intente logearse de nuevo en el sistema. (La Sesión expiró)");

                using (var db = new OrdenCompraRCEntities())
                {
                    var item = db.NotificationGroups.FirstOrDefault(g => g.Id == groupId);
                    if (item != null && item.SendTo.Contains(email)) return Json(new { result = "505", message = "El correo ya existe en el listado." });

                    item.SendTo += $";{email}";
                    db.SaveChanges();
                }

                return Json(new { result = "200", message = "success" });
            }
            catch (Exception ex)
            {
                HelperUtility.SendException(ex, $"groupId: {groupId} | email: {email}");

                return Json(new { result = "500", message = ex.Message });
            }
        }

        [HttpPost]
        public JsonResult RemoveEmail(int groupId, string email)
        {
            try
            {
                if (Session["userID"] == null) throw new Exception("505: Por favor intente logearse de nuevo en el sistema. (La Sesión expiró)");

                using (var db = new OrdenCompraRCEntities())
                {
                    var item = db.NotificationGroups.FirstOrDefault(g => g.Id == groupId && g.SendTo.Contains(email));
                    if (item == null) return Json(new { result = "404", message = "Correo no encontrado." });

                    item.SendTo = item.SendTo.Replace($";{email}", "");
                    db.SaveChanges();
                }

                return Json(new { result = "200", message = "success" });
            }
            catch (Exception ex)
            {
                HelperUtility.SendException(ex, $"groupId: {groupId} | email: {email}");

                return Json(new { result = "500", message = ex.Message });
            }
        }
    }
}