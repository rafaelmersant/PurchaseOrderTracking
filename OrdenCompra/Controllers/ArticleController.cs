using OrdenCompra.App_Start;
using OrdenCompra.Models;
using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Net;
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

        public ActionResult Edit(int id)
        {
            if (Session["role"] != null && Session["role"].ToString() != "Admin") return RedirectToAction("Index", "Home");

            try
            {
                if (id == 0)
                {
                    return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
                }

                var db = new OrdenCompraRCEntities();

                Article _article = db.Articles.FirstOrDefault(a => a.Id == id);
                if (_article == null)
                {
                    return HttpNotFound();
                }

                return View(_article);
            }
            catch (Exception ex)
            {
                Helper.SendException(ex);

                return View();
            }
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Edit(Article _article)
        {
            if (ModelState.IsValid)
            {
                try
                {
                    using (var db = new OrdenCompraRCEntities())
                    {
                        Article article_edit = db.Articles.FirstOrDefault(a => a.Id == _article.Id);

                        if (article_edit != null)
                        {
                            article_edit.Size = _article.Size.ToUpper();
                            
                            db.SaveChanges();
                            return RedirectToAction("Index", "Article");
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

            return View(_article);
        }

        private void AddMissingMark(int markId)
        {
            try
            {
                using (var db = new OrdenCompraRCEntities())
                {
                    db.Marks.Add(new Mark
                    {
                        Id = markId,
                        Description = " "
                    });

                    db.SaveChanges();
                }
            }
            catch (Exception ex)
            {
                Helper.SendException(ex);
            }
        }

        [HttpPost]
        public JsonResult UpdateArticles()
        {
            try
            {
                if (Session["userID"] == null) throw new Exception("Por favor intente logearse de nuevo en el sistema. (La Sesión expiró)");

                var articles = Helper.GetArticles();
                if (articles != null && articles.Tables.Count > 0 && articles.Tables[0].Rows.Count > 0)
                {
                    var _articles = articles.Tables[0];
                    using (var db = new OrdenCompraRCEntities())
                    {
                        var __article__ = db.Articles.ToList();

                        foreach (DataRow __article in _articles.Rows)
                        {
                            try
                            {
                                int articleCode = int.Parse(__article.ItemArray[0].ToString());
                                int markId = int.Parse(__article.ItemArray[2].ToString());
                                decimal stockMinimum = string.IsNullOrEmpty(__article.ItemArray[4].ToString()) ? 0M : decimal.Parse(__article.ItemArray[4].ToString());
                                decimal maxPerContainer = string.IsNullOrEmpty(__article.ItemArray[5].ToString()) ? 0M : decimal.Parse(__article.ItemArray[5].ToString());

                                var _mark = db.Marks.FirstOrDefault(m => m.Id == markId);
                                if (_mark == null) AddMissingMark(markId);

                                var article = __article__.FirstOrDefault(a => a.Id == articleCode);
                                if (article == null)
                                {
                                    db.Articles.Add(new Article
                                    {
                                        Id = articleCode,
                                        Description = __article.ItemArray[1].ToString(),
                                        MarkId = markId,
                                        Model = __article.ItemArray[3].ToString(),
                                        QuantityMinStock = stockMinimum,
                                        QuantityMaxPerContainer = maxPerContainer,
                                        QuantityTraffic = 0,
                                        QuantityFactory = 0,
                                        QuantityAduana = 0,
                                        AddedDate = DateTime.Now,
                                        AddedBy = int.Parse(Session["userID"].ToString())
                                    });

                                    db.SaveChanges();
                                }
                                else
                                {
                                    if (article.Description != __article.ItemArray[1].ToString())
                                    {
                                        article.Description = __article.ItemArray[1].ToString();
                                        article.MarkId = markId;
                                        article.Model = __article.ItemArray[3].ToString();
                                        article.QuantityMinStock = stockMinimum;
                                        article.QuantityMaxPerContainer = maxPerContainer;
                                        db.SaveChanges();
                                    }
                                }
                            }
                            catch (Exception ex)
                            {
                                Helper.SendException(ex, $"articulo ID: {__article.ItemArray[0]}");
                            }
                        }
                    }

                    return Json(new { result = "200", message = "Articulos actualizados con exito!" });
                }
                else
                {
                    return Json(new { result = "404", message = "No se encontro ningun articulo" });
                }
            }
            catch (Exception ex)
            {
                Helper.SendException(ex);

                return Json(new { result = "500", message = ex.Message });
            }

        }
    }
}