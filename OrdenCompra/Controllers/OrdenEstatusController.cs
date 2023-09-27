using System;
using OrdenCompra.Models;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using OrdenCompra.App_Start;
using System.Net;

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

        public ActionResult New()
        {
            if (Session["role"] != null && Session["role"].ToString() != "Admin") return RedirectToAction("Index", "Home");

            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult New(StatusOrderPurchase status)
        {
            ViewBag.Result = "info";

            try
            {
                using (var db = new OrdenCompraRCEntities())
                {
                    if (ModelState.IsValid)
                    {
                        var _status = db.StatusOrderPurchases.ToList();

                        var __status = _status.FirstOrDefault(s => s.Description.ToLower() == status.Description.ToLower());

                        if (__status != null) throw new Exception("Este estatus ya existe en el sistema.");

                        db.StatusOrderPurchases.Add(new StatusOrderPurchase
                        {
                            Description = status.Description,
                            CreatedDate = DateTime.Now
                        });

                        db.SaveChanges();

                        return RedirectToAction("Index");
                    }
                }
            }
            catch (Exception ex)
            {
                Helper.SendException(ex);

                ViewBag.Result = "danger";
                ViewBag.Message = ex.Message;
            }

            return View();
        }

        public ActionResult Edit(int id)
        {
            if (Session["role"] != null && Session["role"].ToString() != "Admin") return RedirectToAction("Index", "Home");

            try
            {
                if (id == 0)
                {
                    return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
                }

                using (var db = new OrdenCompraRCEntities())
                {
                    StatusOrderPurchase _status = db.StatusOrderPurchases.FirstOrDefault(s => s.Id == id);
                    if (_status == null)
                    {
                        return HttpNotFound();
                    }

                    return View(_status);
                }
            }
            catch (Exception ex)
            {
                Helper.SendException(ex);

                return View();
            }
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Edit(StatusOrderPurchase _status)
        {
            if (ModelState.IsValid)
            {
                try
                {
                    using (var db = new OrdenCompraRCEntities())
                    {
                        StatusOrderPurchase status_edit = db.StatusOrderPurchases.FirstOrDefault(s => s.Id == _status.Id);

                        if (status_edit != null)
                        {
                            var status = db.StatusOrderPurchases.ToList();

                            var __status = status.FirstOrDefault(s => s.Description.ToLower() == _status.Description.ToLower());
                            if (__status != null) throw new Exception("Este estatus con dicha descripción ya existe.");

                            status_edit.Description = _status.Description;
                            
                            db.SaveChanges();
                            return RedirectToAction("Index", "OrdenEstatus");
                        }
                    }
                }
                catch (Exception ex)
                {
                    Helper.SendException(ex);

                    ViewBag.Result = "danger";
                    ViewBag.Message = ex.Message;
                }
            }

            return View(_status);
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