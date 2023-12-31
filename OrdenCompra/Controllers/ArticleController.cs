﻿using OrdenCompra.App_Start;
using OrdenCompra.Models;
using RadioCentroServicios;
using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Net;
using System.Web;
using System.Web.Mvc;
using Utility;

namespace OrdenCompra.Controllers
{
    [Route("Articulos")]
    public class ArticleController : Controller
    {
        // GET: Article
        public ActionResult Index()
        {
            if (Session["role"] == null) return RedirectToAction("Index", "Home");
            
            try
            {
                var db = new OrdenCompraRCEntities();

                var articles = db.Articles.OrderBy(o => o.Description).ToList();

                var lastUpdateInventory = db.InventoryHistories.OrderByDescending(o => o.Date).FirstOrDefault();
                ViewBag.LastInventoryUpdate = lastUpdateInventory != null ? lastUpdateInventory.Date.ToString("dd/MM/yyyy hh:mm tt") : "";
                return View(articles);
            }
            catch (Exception ex)
            {
                HelperUtility.SendException(ex);

                return null;
            }
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
                HelperUtility.SendException(ex);

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
                            article_edit.Size = string.IsNullOrEmpty(_article.Size) ? "" : _article.Size.ToUpper();
                            article_edit.Mix = _article.Mix;
                            
                            db.SaveChanges();
                            return RedirectToAction("Index", "Article");
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
                HelperUtility.SendException(ex);
            }
        }

        [HttpPost]
        public JsonResult UpdateArticles()
        {
            try
            {
                if (Session["userID"] == null) throw new Exception("505: Por favor intente logearse de nuevo en el sistema. (La Sesión expiró)");

                var articles = HelperApp.GetArticles();
                if (articles != null && articles.Tables.Count > 0 && articles.Tables[0].Rows.Count > 0)
                {
                    var _articles = articles.Tables[0];
                    using (var db = new OrdenCompraRCEntities())
                    {
                        int _result = db.Database.ExecuteSqlCommand("UPDATE Article set Active = 0");

                        var __article__ = db.Articles.ToList();

                        foreach (DataRow __article in _articles.Rows)
                        {
                            try
                            {
                                if (__article.ItemArray[0].ToString().Substring(0, 1) == "3") continue; //Ignore this kind of article

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
                                        Description = __article.ItemArray[1].ToString().Trim(),
                                        MarkId = markId,
                                        Model = __article.ItemArray[3].ToString().Trim(),
                                        QuantityMinStock = stockMinimum,
                                        QuantityMaxPerContainer = maxPerContainer,
                                        QuantityTraffic = 0,
                                        QuantityFactory = 0,
                                        QuantityAduana = 0,
                                        InventoryStock = 0,
                                        AddedDate = DateTime.Now,
                                        AddedBy = int.Parse(Session["userID"].ToString()),
                                        Active = true,
                                        Mix = maxPerContainer == 0
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
                                        article.Active = true;
                                        article.Mix = maxPerContainer == 0;
                                        db.SaveChanges();
                                    }
                                }
                            }
                            catch (Exception ex)
                            {
                                HelperUtility.SendException(ex, $"articulo ID: {__article.ItemArray[0]}");
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
                HelperUtility.SendException(ex);

                return Json(new { result = "500", message = ex.Message });
            }
        }

        [HttpPost]
        public JsonResult UpdateAllArticleWithCurrentInventory()
        {
            try
            {
                HelperUtility.SendRawEmail("rafaelmersant@sagaracorp.com", "ActualizaArticulos inicio", "Esperando....");

                using (var db = new OrdenCompraRCEntities())
                {
                    var inventory = HelperService.GetArticlesInventory();
                    if (inventory != null && inventory.Tables.Count > 0 && inventory.Tables[0].Rows.Count > 0)
                    {
                        Console.WriteLine($"Articulos a procesar: {inventory.Tables[0].Rows.Count}");

                        HelperUtility.SendRawEmail("rafaelmersant@sagaracorp.com", "ActualizaArticulos inicio", "Procesando:" + inventory.Tables[0].Rows.Count);

                        foreach (DataRow item in inventory.Tables[0].Rows)
                        {
                            try
                            {
                                var articleId = int.Parse(item.ItemArray[0].ToString());
                                var quantity = decimal.Parse(item.ItemArray[1].ToString());

                                var article = db.Articles.FirstOrDefault(a => a.Id == articleId);
                                if (article != null)
                                {
                                    article.InventoryStock = quantity;
                                    db.SaveChanges();

                                    Console.WriteLine($"Actualiza articulo: {article.Id} con inventario: {article.InventoryStock}");
                                }

                                db.InventoryHistories.Add(new InventoryHistory
                                {
                                    ArticleId = articleId,
                                    QuantityAvailable = quantity,
                                    Date = DateTime.Now
                                });
                                db.SaveChanges();
                            }
                            catch (Exception ex)
                            {
                                HelperUtility.SendException(ex, $"articleId: {item.ItemArray[0]} | quantity: {item.ItemArray[1]}");
                            }
                        }
                    }
                }

                HelperUtility.SendRawEmail("rafaelmersant@sagaracorp.com", "ActualizaArticulos finalizo", "......");

                return Json(new { result = "200", message = "Cantidad de articulos en inventario actualizada con exito!" });
            }
            catch (Exception ex)
            {
                HelperUtility.SendException(ex);
                return Json(new { result = "500", message = ex.ToString() });
            }
        }

        public List<SelectListItem> GetArticles(string type = "article")
        {
            List<SelectListItem> array = new List<SelectListItem>();

            try
            {
                var db = new OrdenCompraRCEntities();
                array.Add(new SelectListItem { Text = "SELECCIONAR...", Value = "0" });
                var _articles = db.Articles.Where(a => a.Active).OrderBy(o => o.Description).ToArray();

                if (type == "article")
                    foreach (var item in _articles)
                        array.Add(new SelectListItem { Text = $"{item.Id}-{item.Description}", Value = item.Id.ToString() });

                if (type == "model")
                {
                    var models = db.Articles.Select(s => s.Model).Distinct().OrderBy(o => o);
                    
                    foreach (var item in models)
                        array.Add(new SelectListItem { Text = item, Value = item });
                }

                if (type == "mark")
                {
                    var marks = db.Marks.OrderBy(o => o.Description).ToArray();

                    foreach (var mark in marks)
                        array.Add(new SelectListItem { Text = mark.Description, Value = mark.Id.ToString() });
                }
            }
            catch (Exception ex)
            {
                HelperUtility.SendException(ex);
            }

            return array;
        }
    }
}