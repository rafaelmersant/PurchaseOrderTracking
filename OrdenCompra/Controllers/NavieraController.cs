using OrdenCompra.App_Start;
using OrdenCompra.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Web;
using System.Web.Mvc;
using Utility;

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
                HelperUtility.SendException(ex);

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
        public ActionResult New(ShippingCompany naviera)
        {
            ViewBag.Result = "info";

            try
            {
                using (var db = new OrdenCompraRCEntities())
                {
                    if (ModelState.IsValid)
                    {
                        var shippingCompanies = db.ShippingCompanies.ToList();

                        var _shippingCompany = shippingCompanies.FirstOrDefault(s => s.Description.ToUpper() == naviera.Description.ToUpper()
                                                                                && s.Port == naviera.Port.ToUpper());

                        if (_shippingCompany != null) throw new Exception("Esta naviera con dicho puerto ya existe en el sistema.");

                        db.ShippingCompanies.Add(new ShippingCompany
                        {
                            Description = naviera.Description.ToUpper(),
                            Port = naviera.Port.ToUpper(),
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
            if (Session["role"] != null && Session["role"].ToString() != "Admin") return RedirectToAction("Index", "Home");

            try
            {
                if (id == 0)
                {
                    return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
                }

                using (var db = new OrdenCompraRCEntities())
                {
                    ShippingCompany _naviera = db.ShippingCompanies.FirstOrDefault(s => s.Id == id);
                    if (_naviera == null)
                    {
                        return HttpNotFound();
                    }

                    return View(_naviera);
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
        public ActionResult Edit(ShippingCompany _naviera)
        {
            if (ModelState.IsValid)
            {
                try
                {
                    using (var db = new OrdenCompraRCEntities())
                    {
                        ShippingCompany naviera_edit = db.ShippingCompanies.FirstOrDefault(s => s.Id == _naviera.Id);

                        if (naviera_edit != null)
                        {
                            string currentNavieraPort = string.IsNullOrEmpty(_naviera.Port) ? _naviera.Port : _naviera.Port.ToUpper();
                            var navieras = db.ShippingCompanies.ToList();

                            var __naviera = navieras.FirstOrDefault(s => s.Description == _naviera.Description.ToUpper() 
                                                                    && s.Port == currentNavieraPort
                                                                    && s.Days == _naviera.Days);

                            if (__naviera != null) throw new Exception("Este registro existe con dicha información.");

                            naviera_edit.Description = _naviera.Description.ToUpper();
                            naviera_edit.Port = currentNavieraPort;
                            naviera_edit.Days = _naviera.Days;
                            
                            db.SaveChanges();
                            return RedirectToAction("Index", "Naviera");
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

            return View(_naviera);
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
                HelperUtility.SendException(ex);
            }

            return navieras;
        }
    }
}