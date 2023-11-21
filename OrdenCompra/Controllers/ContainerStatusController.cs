using System;
using OrdenCompra.Models;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using OrdenCompra.App_Start;
using System.Net;
using Utility;

namespace OrdenCompra.Controllers
{
    [Route("ContenedorEstatus")]
    public class ContainerStatusController : Controller
    {
        // GET: ContainerStatus
        public ActionResult Index()
        {
            if (Session["role"] == null) return RedirectToAction("Index", "Home");
            
            try
            {
                var db = new OrdenCompraRCEntities();

                var status = db.StatusContainers.OrderBy(o => o.Id).ToList();
                return View(status);

            }
            catch (Exception ex)
            {
                HelperUtility.SendException(ex);

                return null;
            }
        }

        public ActionResult New()
        {
            if (Session["role"] == null) return RedirectToAction("Index", "Home");

            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult New(StatusContainer status)
        {
            ViewBag.Result = "info";

            try
            {
                using (var db = new OrdenCompraRCEntities())
                {
                    if (ModelState.IsValid)
                    {
                        var _status = db.StatusContainers.ToList();

                        var __status = _status.FirstOrDefault(s => s.Description.ToLower() == status.Description.ToLower());

                        if (__status != null) throw new Exception("Este estatus ya existe en el sistema.");

                        db.StatusContainers.Add(new StatusContainer
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
                HelperUtility.SendException(ex);

                ViewBag.Result = "danger";
                ViewBag.Message = ex.Message;
            }

            return View();
        }

        public ActionResult Edit(int id)
        {
            if (Session["role"] == null) return RedirectToAction("Index", "Home");

            try
            {
                if (id == 0)
                {
                    return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
                }

                using (var db = new OrdenCompraRCEntities())
                {
                    StatusContainer _status = db.StatusContainers.FirstOrDefault(s => s.Id == id);
                    if (_status == null)
                    {
                        return HttpNotFound();
                    }

                    return View(_status);
                }
            }
            catch (Exception ex)
            {
                HelperUtility.SendException(ex);

                return View();
            }
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Edit(StatusContainer _status)
        {
            if (ModelState.IsValid)
            {
                try
                {
                    using (var db = new OrdenCompraRCEntities())
                    {
                        StatusContainer status_edit = db.StatusContainers.FirstOrDefault(s => s.Id == _status.Id);

                        if (status_edit != null)
                        {
                            var status = db.StatusContainers.ToList();

                            var __status = status.FirstOrDefault(s => s.Description.ToLower() == _status.Description.ToLower());
                            if (__status != null) throw new Exception("Este estatus con dicha descripción ya existe.");

                            status_edit.Description = _status.Description;

                            db.SaveChanges();
                            return RedirectToAction("Index", "ContainerStatus");
                        }
                    }
                }
                catch (Exception ex)
                {
                    HelperUtility.SendException(ex);

                    ViewBag.Result = "danger";
                    ViewBag.Message = ex.Message;
                }
            }

            return View(_status);
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
                HelperUtility.SendException(ex);
            }

            return status;
        }
    }
}