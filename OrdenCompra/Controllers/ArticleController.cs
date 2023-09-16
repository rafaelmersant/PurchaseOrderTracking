using OrdenCompra.App_Start;
using OrdenCompra.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace OrdenCompra.Controllers
{
    [Route("Articulos")]
    public class ArticleController : Controller
    {
        // GET: Article
        public ActionResult Index()
        {
            if (Session["role"] == null) return RedirectToAction("Index", "Home");
            if (Session["role"].ToString() != "Admin") return RedirectToAction("Index", "Home");

            try
            {
                var db = new OrdenCompraRCEntities();

                var articles = db.Articles.OrderBy(o => o.Description).ToList();
                return View(articles);

            }
            catch (Exception ex)
            {
                Helper.SendException(ex);

                return null;
            }
        }
    }
}