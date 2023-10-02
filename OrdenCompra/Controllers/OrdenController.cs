using Newtonsoft.Json;
using OrdenCompra.App_Start;
using OrdenCompra.Models;
using OrdenCompra.ViewModels;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Diagnostics;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using System.Web.UI.WebControls.WebParts;
using System.Web.WebSockets;
using Utility;

namespace OrdenCompra.Controllers
{
    public class OrdenController : Controller
    {
        // GET: Orden
        public ActionResult Index()
        {
            try
            {
                if (Session["role"] == null) return RedirectToAction("Index", "Home");

                ViewBag.Navieras = new NavieraController().GetNavieras();
                ViewBag.OrderStatus = new OrdenEstatusController().GetOrderStatus();
                ViewBag.ContainerStatus = new ContainerStatusController().GetContainerStatus();
            }
            catch (Exception ex)
            {
                HelperUtility.SendException(ex);
            }

            return View();
        }

        public ActionResult Listado()
        {
            if (Session["role"] == null) return RedirectToAction("Index", "Home");
            if (Session["role"].ToString() != "Admin") return RedirectToAction("Index", "Home");

            try
            {
                var db = new OrdenCompraRCEntities();

                var ordenes = db.OrderPurchases.OrderBy(o => o.CreatedDate).ToList();
                return View(ordenes);

            }
            catch (Exception ex)
            {
                HelperUtility.SendException(ex);

                return null;
            }
        }

        private OrderPurchase GetOrderPurchaseHeader(int OrderId)
        {
            try
            {
                using (var db = new OrdenCompraRCEntities())
                {
                    return db.OrderPurchases.FirstOrDefault(o => o.OrderPurchaseId == OrderId);
                }
            }
            catch (Exception ex)
            {
                HelperUtility.SendException(ex);
            }

            return null;
        }

        private DateTime? GetDateFromAS400Field(string value)
        {
            if (!string.IsNullOrEmpty(value) && value.Length >= 8)
            {
                int year = int.Parse(value.Substring(0, 4));
                int month = int.Parse(value.Substring(4, 2));
                int day = int.Parse(value.Substring(6, 2));

                return new DateTime(year, month, day);
            }

            return null;
        }

        private void PopulateOrderPurchaseHeader(DataSet orderHeader, int orderId)
        {
            try
            {
                if (orderHeader == null || orderHeader.Tables.Count == 0) return;

                var _orderHeader = orderHeader.Tables[0].Rows[0];
                using (var db = new OrdenCompraRCEntities())
                {
                    DateTime? dateDMA = GetDateFromAS400Field(_orderHeader.ItemArray[1].ToString());
                    DateTime? dateRequest = GetDateFromAS400Field(_orderHeader.ItemArray[4].ToString());
                    
                    OrderPurchase orderPurchase = new OrderPurchase()
                    {
                        CreatedBy = int.Parse(Session["userID"].ToString()),
                        CreatedDate = DateTime.Now,
                        DateDMA = dateDMA,
                        DateRequest = dateRequest,
                        ProviderId = int.Parse(_orderHeader.ItemArray[2].ToString().Substring(4,5)),
                        Observation = _orderHeader.ItemArray[3].ToString(),
                        OrderPurchaseId = int.Parse(_orderHeader.ItemArray[0].ToString()),
                        StatusId = 1
                    };

                    db.OrderPurchases.Add(orderPurchase);
                    db.SaveChanges();
                }
            }
            catch (Exception ex)
            {
                HelperUtility.SendException(ex, $"orderID: {orderId}");
            }
        }

        private void PopulateOrderPurchaseDetailRequest(DataSet orderDetail, int orderId)
        {
            try
            {
                if (orderDetail.Tables.Count == 0) return;

                using (var db = new OrdenCompraRCEntities())
                {
                    OrderPurchaseContainer orderPurchaseContainer = new OrderPurchaseContainer()
                    {
                        CreatedDate = DateTime.Now,
                        OrderPurchaseId = orderId,
                        StatusId = 1,
                        SortIndex = 1
                    };
                    
                    var _container = db.OrderPurchaseContainers.Add(orderPurchaseContainer);
                    db.SaveChanges();

                    List<OrderPurchaseArticlesContainer> articles = new List<OrderPurchaseArticlesContainer>();

                    foreach (DataRow item in orderDetail.Tables[0].Rows)
                    {
                        DateTime? orderDate = GetDateFromAS400Field(item.ItemArray[2].ToString());

                        decimal quantityRequested = string.IsNullOrEmpty(item.ItemArray[4].ToString()) ? 0M : decimal.Parse(item.ItemArray[4].ToString());
                        decimal quantityFactory = string.IsNullOrEmpty(item.ItemArray[5].ToString()) ? 0M : decimal.Parse(item.ItemArray[5].ToString());
                        decimal quantityTraffic = string.IsNullOrEmpty(item.ItemArray[6].ToString()) ? 0M : decimal.Parse(item.ItemArray[6].ToString());
                        decimal quantityLeft = string.IsNullOrEmpty(item.ItemArray[7].ToString()) ? 0M : decimal.Parse(item.ItemArray[7].ToString());
                        decimal quantityAduana = string.IsNullOrEmpty(item.ItemArray[8].ToString()) ? 0M : decimal.Parse(item.ItemArray[8].ToString());

                        articles.Add(new OrderPurchaseArticlesContainer
                        {
                            ContainerId = _container.Id,
                            AddedDate = DateTime.Now,
                            OrderPurchaseId = int.Parse(item.ItemArray[0].ToString()),
                            ArticleId = int.Parse(item.ItemArray[1].ToString()),
                            QuantityRequested = quantityRequested,
                            QuantityFactory = quantityFactory,
                            QuantityTraffic = quantityTraffic, 
                            QuantityLeft = quantityLeft,
                            QuantityAduana = quantityAduana,
                            Price = decimal.Parse(item.ItemArray[9].ToString())
                        });
                    }

                    if (articles.Count > 0)
                    {
                        db.OrderPurchaseArticlesContainers.AddRange(articles);
                        db.SaveChanges();
                    }
                }
            }
            catch (Exception ex)
            {
                HelperUtility.SendException(ex, $"OrderID: {orderId}");
            }
        }

        private void PopulateOrderPurchaseDetailReceived(DataSet orderDetail)
        {
            try
            {
                if (orderDetail.Tables.Count == 0) return;

                using (var db = new OrdenCompraRCEntities())
                {
                    List<OrderPurchaseDeliver> items = new List<OrderPurchaseDeliver>();
                    var orderId = int.Parse(orderDetail.Tables[0].Rows[0].ItemArray[0].ToString());

                    var _container = db.OrderPurchaseContainers.FirstOrDefault(o => o.OrderPurchaseId == orderId);

                    foreach (DataRow item in orderDetail.Tables[0].Rows)
                    {
                        DateTime? receivedDate = GetDateFromAS400Field(item.ItemArray[3].ToString());

                        decimal quantityReceived = string.IsNullOrEmpty(item.ItemArray[4].ToString()) ? 0M : decimal.Parse(item.ItemArray[4].ToString());
                        
                        items.Add(new OrderPurchaseDeliver
                        {
                            ContainerId = _container != null ? _container.Id : 0,
                            OrderPurchaseId = orderId,
                            ArticleId = int.Parse(item.ItemArray[1].ToString()),
                            QuantityDelivered = quantityReceived,
                            ReceivedDate = receivedDate
                        });
                    }

                    if (items.Count > 0)
                    {
                        db.OrderPurchaseDelivers.AddRange(items);
                        db.SaveChanges();
                    }
                }
            }
            catch (Exception ex)
            {
                HelperUtility.SendException(ex);
            }
        }

        private void CreateRequiredContainersForOrderPerArticlesOLD(int orderId)
        {
            try
            {
                using (var db = new OrdenCompraRCEntities())
                {
                    var _orderArticles = db.OrderPurchaseArticlesContainers.Where(o => o.OrderPurchaseId == orderId);
                    decimal containers = 0;

                    var articles = db.Articles.Where(a => _orderArticles.Any(x => x.ArticleId == a.Id)).ToList();
                                        
                    foreach (var article in _orderArticles)
                    {
                        var _article = articles.FirstOrDefault(a => a.Id == article.ArticleId);
                        if (_article == null)
                        {
                            var __article = HelperApp.GetArticleById(article.ArticleId);
                            if (__article != null && __article.Tables.Count > 0 && __article.Tables[0].Rows.Count > 0)
                            {
                                HelperApp.AddNewArticle(__article.Tables[0].Rows[0]);
                                _article = db.Articles.FirstOrDefault(a => a.Id == article.ArticleId);
                            }
                        }

                        if (_article != null)
                        {
                            var maxPerContainer = _article.QuantityMaxPerContainer <= 0 ? article.QuantityRequested : _article.QuantityMaxPerContainer ?? 1M;
                            var _containers = maxPerContainer != 99 ? article.QuantityRequested / maxPerContainer : 1;
                            containers += _containers;
                        }
                    }

                    if (containers <= 1) return;

                    //backup original detail before continue
                    foreach (var __article__ in _orderArticles.ToList())
                    {
                        db.OrderPurchaseArticlesContainerTmps.Add(new OrderPurchaseArticlesContainerTmp()
                        {
                            Id = __article__.Id,
                            AddedDate = __article__.AddedDate,
                            ArticleId = __article__.ArticleId,
                            ContainerId = __article__.ContainerId,
                            ManufacturingDate = __article__.ManufacturingDate,
                            OrderPurchaseId = __article__.OrderPurchaseId,
                            Price = __article__.Price,
                            QuantityRequested = __article__.QuantityRequested,
                            QuantityLeft = __article__.QuantityLeft,
                            QuantityTraffic = __article__.QuantityTraffic,
                            QuantityFactory = __article__.QuantityFactory,
                            QuantityAduana = __article__.QuantityAduana,
                            QuantityTmp = __article__.QuantityRequested
                        });
                        db.SaveChanges();
                    }

                    var orderArticles = db.OrderPurchaseArticlesContainerTmps.Where(t => t.OrderPurchaseId == orderId).ToList();
                    //If there are more than 1 container then we need to create them with the articles and their max quantity per container
                    for (int icontainer = 0; icontainer < containers; icontainer++)
                    {
                        OrderPurchaseContainer _container = null;

                        if (icontainer == 0)
                            _container = db.OrderPurchaseContainers.FirstOrDefault(c => c.OrderPurchaseId == orderId);
                        else
                        {
                            _container = db.OrderPurchaseContainers.Add(new OrderPurchaseContainer
                            {
                                CreatedDate = DateTime.Now,
                                OrderPurchaseId = orderId,
                                StatusId = 1,
                                SortIndex = icontainer + 1,
                            });
                            db.SaveChanges();
                        }
                        
                        for (int i = 0; i < orderArticles.Count; i++)
                        {
                            decimal maxPerContainer = orderArticles[i].QuantityRequested;
                            
                            int articleId = orderArticles[i].ArticleId;
                            Article _article = db.Articles.FirstOrDefault(a => a.Id == articleId);
                            maxPerContainer = _article != null && _article.QuantityMaxPerContainer <= 0 ? maxPerContainer : _article.QuantityMaxPerContainer?? 1M;

                            decimal quantityToDeduct = orderArticles[i].QuantityRequested > maxPerContainer ? maxPerContainer : orderArticles[i].QuantityRequested;
                            orderArticles[i].QuantityRequested -= quantityToDeduct;
                            
                            if (quantityToDeduct > 0)
                            {
                                if (icontainer == 0)
                                {
                                    var item = db.OrderPurchaseArticlesContainers.FirstOrDefault(a => a.OrderPurchaseId == orderId && a.ArticleId == articleId);
                                    item.QuantityRequested = quantityToDeduct;
                                } 
                                else
                                {
                                    db.OrderPurchaseArticlesContainers.Add(new OrderPurchaseArticlesContainer
                                    {
                                        ContainerId = _container.Id,
                                        AddedDate = DateTime.Now,
                                        OrderPurchaseId = orderId,
                                        ArticleId = orderArticles[i].ArticleId,
                                        QuantityRequested = quantityToDeduct,
                                        QuantityFactory = orderArticles[i].QuantityFactory,
                                        QuantityTraffic = orderArticles[i].QuantityTraffic,
                                        QuantityLeft = orderArticles[i].QuantityLeft,
                                        QuantityAduana = orderArticles[i].QuantityAduana,
                                        Price = orderArticles[i].Price
                                    });
                                }
                                
                                db.SaveChanges();
                            }
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                HelperUtility.SendException(ex);
            }
        }

        private void CreateBackupOriginalOrderPurchase(List<OrderPurchaseArticlesContainer> _orderArticles)
        {
            try
            {
                using (var db = new OrdenCompraRCEntities())
                {
                    //backup original detail before continue
                    foreach (var __article__ in _orderArticles)
                    {
                        db.OrderPurchaseArticlesContainerTmps.Add(new OrderPurchaseArticlesContainerTmp()
                        {
                            Id = __article__.Id,
                            AddedDate = __article__.AddedDate,
                            ArticleId = __article__.ArticleId,
                            ContainerId = __article__.ContainerId,
                            ManufacturingDate = __article__.ManufacturingDate,
                            OrderPurchaseId = __article__.OrderPurchaseId,
                            Price = __article__.Price,
                            QuantityRequested = __article__.QuantityRequested,
                            QuantityLeft = __article__.QuantityLeft,
                            QuantityTraffic = __article__.QuantityTraffic,
                            QuantityFactory = __article__.QuantityFactory,
                            QuantityAduana = __article__.QuantityAduana,
                            QuantityTmp = __article__.QuantityRequested
                        });
                        db.SaveChanges();
                    }
                }
            }
            catch (Exception ex)
            {
                HelperUtility.SendException(ex);
            }
        }

        private void CreateContainersWithArticles(int orderId, int articleId, decimal containersPerArticle, ref int container)
        {
            try
            {
                using (var db = new OrdenCompraRCEntities())
                {
                    var orderArticle = db.OrderPurchaseArticlesContainerTmps.FirstOrDefault(t => t.OrderPurchaseId == orderId && t.ArticleId == articleId);

                    for (int i = 0; i < containersPerArticle; i++)
                    {
                        OrderPurchaseContainer _container = db.OrderPurchaseContainers.Add(new OrderPurchaseContainer
                        {
                            CreatedDate = DateTime.Now,
                            OrderPurchaseId = orderId,
                            StatusId = 1,
                            SortIndex = container,
                        });
                        db.SaveChanges();

                        decimal maxPerContainer = orderArticle.QuantityRequested;

                        Article _article = db.Articles.FirstOrDefault(a => a.Id == articleId);
                        maxPerContainer = _article != null && _article.QuantityMaxPerContainer <= 0 ? maxPerContainer : _article.QuantityMaxPerContainer ?? 1M;

                        decimal quantityToDeduct = orderArticle.QuantityRequested > maxPerContainer ? maxPerContainer : orderArticle.QuantityRequested;
                        orderArticle.QuantityRequested -= quantityToDeduct;

                        if (quantityToDeduct > 0)
                        {
                            db.OrderPurchaseArticlesContainers.Add(new OrderPurchaseArticlesContainer
                            {
                                ContainerId = _container.Id,
                                AddedDate = DateTime.Now,
                                OrderPurchaseId = orderId,
                                ArticleId = orderArticle.ArticleId,
                                QuantityRequested = quantityToDeduct,
                                QuantityFactory = orderArticle.QuantityFactory,
                                QuantityTraffic = orderArticle.QuantityTraffic,
                                QuantityLeft = orderArticle.QuantityLeft,
                                QuantityAduana = orderArticle.QuantityAduana,
                                Price = orderArticle.Price
                            });

                            db.SaveChanges();
                        }

                        container += 1;
                    }
                }
            }
            catch (Exception ex)
            {
                HelperUtility.SendException(ex);
            }
        }

        private void CreateRequiredContainersForOrder(int orderId)
        {
            try
            {
                using (var db = new OrdenCompraRCEntities())
                {
                    var _orderArticles = db.OrderPurchaseArticlesContainers.Where(o => o.OrderPurchaseId == orderId);
                    int containers = 1;
                    decimal containersPerArticle = 1M;

                    var articles = db.Articles.Where(a => _orderArticles.Any(x => x.ArticleId == a.Id)).ToList();

                    CreateBackupOriginalOrderPurchase(_orderArticles.ToList());

                    var orderContainer = db.OrderPurchaseContainers.FirstOrDefault(c => c.OrderPurchaseId == orderId);
                    db.OrderPurchaseContainers.Remove(orderContainer);
                    db.SaveChanges();

                    var __orderArticles = db.OrderPurchaseArticlesContainerTmps.Where(o => o.OrderPurchaseId == orderId);
                    foreach (var article in __orderArticles)
                    {
                        var _article = articles.FirstOrDefault(a => a.Id == article.ArticleId);
                        if (_article == null)
                        {
                            var __article = HelperApp.GetArticleById(article.ArticleId);
                            if (__article != null && __article.Tables.Count > 0 && __article.Tables[0].Rows.Count > 0)
                            {
                                HelperApp.AddNewArticle(__article.Tables[0].Rows[0]);
                                _article = db.Articles.FirstOrDefault(a => a.Id == article.ArticleId);
                            }
                        }

                        if (_article != null)
                        {
                            var maxPerContainer = _article.QuantityMaxPerContainer ?? 1M;
                            if (_article.QuantityMaxPerContainer <= 0 || _article.QuantityMaxPerContainer == 99)
                            {
                                maxPerContainer = article.QuantityRequested;
                            }

                            containersPerArticle = article.QuantityRequested / maxPerContainer;

                            CreateContainersWithArticles(orderId, _article.Id, containersPerArticle, ref containers);
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                HelperUtility.SendException(ex, $"orderID: {orderId}");
            }
        }

        private bool CreateAnotherContainerForArticle(OrderPurchaseArticlesContainer detail, decimal remainQuantity)
        {
            try
            {
                using (var db = new OrdenCompraRCEntities())
                {
                    var lastContainer = db.OrderPurchaseContainers.Where(c => c.OrderPurchaseId == detail.OrderPurchaseId).OrderByDescending(o => o.SortIndex).FirstOrDefault();
                    if (lastContainer != null)
                    {
                        var container = db.OrderPurchaseContainers.Add(new OrderPurchaseContainer
                        {
                            CreatedDate = DateTime.Now,
                            OrderPurchaseId = lastContainer.OrderPurchaseId,
                            StatusId = 1,
                            SortIndex = lastContainer.SortIndex + 1,
                        });
                        db.SaveChanges();

                        db.OrderPurchaseArticlesContainers.Add(new OrderPurchaseArticlesContainer
                        {
                            ContainerId = container.Id,
                            AddedDate = DateTime.Now,
                            OrderPurchaseId = container.OrderPurchaseId,
                            ArticleId = detail.ArticleId,
                            QuantityRequested = remainQuantity,
                            QuantityFactory = remainQuantity,
                            QuantityTraffic = 0,
                            QuantityLeft = 0,
                            QuantityAduana = 0,
                            Price = detail.Price
                        });
                        db.SaveChanges();

                        return true;
                    }
                }
            }
            catch (Exception ex)
            {
                HelperUtility.SendException(ex, $"orderId: {detail.OrderPurchaseId} | containerId: {detail.ContainerId} | articleId: {detail.ArticleId}");
            }

            return false;
        }

        [HttpPost]
        public JsonResult GetPurchaseOrder(int orderId)
        {
            try
            {
                if (Session["userID"] == null) throw new Exception("505: Por favor intente logearse de nuevo en el sistema. (La Sesión expiró)");

                //Check first if the orderID was saved in the local DB (not AS400)
                var orderHeader = GetOrderPurchaseHeader(orderId);
                //If it's the first time to retrieve from AS400 then do it and populate

                if (orderHeader == null)
                {
                    //Take order header from AS400
                    var __orderHeader = HelperApp.GetOrderPurchaseHeader(orderId);
                    PopulateOrderPurchaseHeader(__orderHeader, orderId);

                    //Take order detail from AS400
                    var __orderDetail = HelperApp.GetOrderPurchaseDetail(orderId);
                    PopulateOrderPurchaseDetailRequest(__orderDetail, orderId);

                    //Create other containers if apply
                    CreateRequiredContainersForOrder(orderId);

                    //Take order detail received from AS400
                    //var __orderDetailReceived = Helper.GetOrderPurchaseDetailReceived(orderId);
                    //PopulateOrderPurchaseDetailReceived(__orderDetailReceived);

                }

                OrderPurchaseViewModel order = new OrderPurchaseViewModel();
                List<OrderPurchaseContainerViewModel> containers = new List<OrderPurchaseContainerViewModel>();

                using (var db = new OrdenCompraRCEntities())
                {
                    var _orderHeader = db.GetPurchaseOrderHeader(orderId).FirstOrDefault();
                    var _orderDetail = db.GetPurchaseOrderDetail(orderId).ToList();

                    if (_orderHeader == null) return Json(new { result = "404", message = $"La orden de compra #{orderId} no fue encontrada." });

                    var _containers = db.OrderPurchaseContainers.Where(c => c.OrderPurchaseId == orderId);
                    foreach (var container in _containers)
                    {
                        var __container = db.GetPurchaseOrderContainer(container.Id).FirstOrDefault();
                        var __detail = _orderDetail.Where(o => o.containerId == container.Id).ToList();

                        containers.Add(new OrderPurchaseContainerViewModel
                        {
                            container = __container,
                            Details = __detail
                        });
                    }

                    order.Header = _orderHeader;
                    order.Containers = containers;

                    return Json(new { result = "200", message = order });
                }
            }
            catch (Exception ex)
            {
                HelperUtility.SendException(ex, $"orderID: {orderId}");

                return Json(new { result = "500", message = ex.Message });
            }
        }

        [HttpPost]
        public JsonResult UpdateContainerField(int containerId, string type, string value)
        {
            try
            {
                if (Session["userID"] == null) throw new Exception("505: Por favor intente logearse de nuevo en el sistema. (La Sesión expiró)");

                using (var db = new OrdenCompraRCEntities())
                {
                    var container = db.OrderPurchaseContainers.FirstOrDefault(c => c.Id == containerId);
                    if (container == null) return Json(new { result = "404", message = "Contenedor no encontrado." });
                    if (string.IsNullOrEmpty(value)) return Json(new { result = "404", message = "Valor no encontrado." });

                    if (type == "ManufacturingDate")
                    {
                        int year = int.Parse(value.Substring(6, 4));
                        int month = int.Parse(value.Substring(3, 2));
                        int day = int.Parse(value.Substring(0, 2));

                        container.ManufacturingDate = new DateTime(year, month, day);
                    }

                    if (type == "DueDate")
                    {
                        int year = int.Parse(value.Substring(6, 4));
                        int month = int.Parse(value.Substring(3, 2));
                        int day = int.Parse(value.Substring(0, 2));

                        container.DueDate = new DateTime(year, month, day);
                    }

                    if (type == "Status")
                        container.StatusId = int.Parse(value);
                    
                    if (type == "Naviera")
                        container.ShippingCompanyId = int.Parse(value);

                    db.SaveChanges();
                }

                return Json(new { result = "200", message = "success" });
            }
            catch (Exception ex)
            {
                HelperUtility.SendException(ex, $"containerId: {containerId} | type: {type} | value: {value}");

                return Json(new { result = "500", message = ex.Message });
            }
        }

        [HttpPost]
        public JsonResult UpdateOrderField(int orderId, string type, string value)
        {
            try
            {
                if (Session["userID"] == null) throw new Exception("505: Por favor intente logearse de nuevo en el sistema. (La Sesión expiró)");

                using (var db = new OrdenCompraRCEntities())
                {
                    var order = db.OrderPurchases.FirstOrDefault(o => o.OrderPurchaseId == orderId);
                    if (order== null) return Json(new { result = "404", message = "Orden no encontrada." });
                    
                    if (type == "orderDate")
                    {
                        int year = int.Parse(value.Substring(6, 4));
                        int month = int.Parse(value.Substring(3, 2));
                        int day = int.Parse(value.Substring(0, 2));

                        order.DateDMA = new DateTime(year, month, day);
                    }

                    if (type == "status")
                        order.StatusId = int.Parse(value);

                    db.SaveChanges();
                }

                return Json(new { result = "200", message = "success" });
            }
            catch (Exception ex)
            {
                HelperUtility.SendException(ex, $"orderId: {orderId} | type: {type} | value: {value}");

                return Json(new { result = "500", message = ex.Message });
            }
        }

        [HttpPost]
        public JsonResult UpdateQuantityField(int detailId, decimal newQuantity, decimal traffic, decimal factory, string BL)
        {
            try
            {
                bool anotherContainer = false;

                if (Session["userID"] == null) throw new Exception("505: Por favor intente logearse de nuevo en el sistema. (La Sesión expiró)");

                using (var db = new OrdenCompraRCEntities())
                {
                    var detail = db.OrderPurchaseArticlesContainers.FirstOrDefault(o => o.Id == detailId);
                    if (detail == null) return Json(new { result = "404", message = "No se pudo actualizar la cantidad para dicho articulo." });

                    detail.QuantityRequested = newQuantity;
                    detail.QuantityTraffic = traffic;
                    detail.QuantityFactory = factory;
                    db.SaveChanges();

                    if (traffic < newQuantity)
                    {
                        decimal remainQuantity = newQuantity - traffic;
                        anotherContainer = CreateAnotherContainerForArticle(detail, remainQuantity);
                    }
                        
                    if (!string.IsNullOrEmpty(BL))
                    {
                        var container = db.OrderPurchaseContainers.FirstOrDefault(o => o.Id == detail.ContainerId);
                        if (container != null && container.BL != BL)
                        {
                            container.BL = BL;
                            db.SaveChanges();
                        }
                    }

                    if (anotherContainer)
                        return Json(new { result = "201", message = "success" });
                }

                return Json(new { result = "200", message = "success" });
            }
            catch (Exception ex)
            {
                HelperUtility.SendException(ex);

                return Json(new { result = "500", message = ex.Message });
            }
        }

        [HttpPost]
        public JsonResult AddNewArticleToContainer(int containerId, int articleId, decimal quantity)
        {
            try
            {
                if (Session["userID"] == null) throw new Exception("505: Por favor intente logearse de nuevo en el sistema. (La Sesión expiró)");

                using (var db = new OrdenCompraRCEntities())
                {
                    var container = db.OrderPurchaseContainers.FirstOrDefault(c => c.Id == containerId);
                    if (container == null) return Json(new { result = "404", message = "Contenedor no encontrado." });

                    var article = db.OrderPurchaseArticlesContainers.FirstOrDefault(a => a.ArticleId == articleId);
                    if (article == null) return Json(new { result = "404", message = "Articulo no encontrado." });

                    db.OrderPurchaseArticlesContainers.Add(new OrderPurchaseArticlesContainer
                    {
                        ContainerId = containerId,
                        ArticleId = articleId,
                        OrderPurchaseId = container.OrderPurchaseId,
                        AddedDate = DateTime.Now,
                        QuantityRequested = quantity,
                        QuantityFactory = 0,
                        QuantityTraffic = 0,
                        QuantityLeft = 0,
                        QuantityAduana = 0,
                        Price = article != null ? article.Price : 0,
                    });

                    db.SaveChanges();
                }

                return Json(new { result = "200", message = "success" });
            }
            catch (Exception ex)
            {
                HelperUtility.SendException(ex, $"containerId: {containerId} | articleId: {articleId} | quantity: {quantity}");

                return Json(new { result = "500", message = ex.Message });
            }
        }
    }
}