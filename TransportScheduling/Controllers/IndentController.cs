using System;
using System.Collections;
using System.Collections.Generic;
using System.Data;
using System.Data.Entity.Infrastructure;
using System.Linq;
using System.Linq.Expressions;
using System.Web;
using System.Web.Mvc;
using TransportScheduling.Models;

namespace TransportScheduling.Controllers
{
    public class IndentController : Controller
    {
        // /Indent/

        TSEntities db = new TSEntities();

        /// <summary>
        /// 信息查询
        /// </summary>
        /// <returns></returns>
        [HttpGet]
        public ActionResult Main()
        {
            int id = 1;
            int totalRecord = db.T_Infor_Indent.Count();
            ViewBag.PageNum = id;
            ViewBag.TotalPage = (totalRecord - 1) / 10 + 1;
            List<T_Infor_Indent> list = LoadPageItems(10, id, out totalRecord, (u => u.IState != 5 && u.IState != 6), u => u.Iid, true).ToList();

            List<SelectListItem> statelist = db.T_Dic_IndentState.ToList().Where(c => c.Code != 5).Select(c => new SelectListItem()
            {
                Value = c.Code.ToString(),
                Text = c.Description,
                Selected = false
            }).ToList();
            ViewBag.StateList = statelist;
            return View(list);
        }

        /// <summary>
        /// 信息查询
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
        [HttpPost, ActionName("Main")]
        public ActionResult MainIndex(int id)
        {
            if (id < 1)
                id = 1;
            int totalRecord = db.T_Infor_Indent.Count();
            ViewBag.PageNum = id;
            ViewBag.TotalPage = (totalRecord - 1) / 10 + 1;
            List<T_Infor_Indent> list = LoadPageItems(10, id, out totalRecord, (u => u.IState != 5 && u.IState != 6), u => u.Iid, true).ToList();

            List<SelectListItem> statelist = db.T_Dic_IndentState.ToList().Select(c => new SelectListItem()
            {
                Value = c.Code.ToString(),
                Text = c.Description,
                Selected = false
            }).ToList();
            ViewBag.StateList = statelist;
            ViewBag.InforList = list;
            return View(list);
        }

        /// <summary>
        /// 删除
        /// </summary>
        /// <param name="id"></param>
        public void Delete(int id)
        {
            T_Infor_Indent indent = new T_Infor_Indent() { Iid = id };
            DbEntityEntry<T_Infor_Indent> entry = db.Entry<T_Infor_Indent>(indent);
            entry.State = EntityState.Deleted;
            int res = db.SaveChanges();
            if (res == 1)
                Response.Write("<script>alert('删除成功！'); window.location='/Indent/Main/1';</script>");
        }

        /// <summary>
        /// 修改
        /// </summary>
        /// <param name="model"></param>
        [HttpPost]
        public void Modify()
        {
            string date = "20" + Request.Form["IDeliveryDate"].Trim() + ":00";
            T_Infor_Indent model = new T_Infor_Indent()
            {
                Iid = Convert.ToInt32(Request.Form["Iid"]),
                IDeliveryDate = Convert.ToDateTime(Request.Form["IDeliveryDate"].Trim() + ":00"),
                IState = Convert.ToInt32(Request.Form["IState"]),
                IRemarks = Request.Form["IRemarks"]
            };
            DbEntityEntry<T_Infor_Indent> entry = db.Entry<T_Infor_Indent>(model);
            entry.State = EntityState.Unchanged;
            entry.Property("IState").IsModified = true;
            entry.Property("IDeliveryDate").IsModified = true;
            entry.Property("IRemarks").IsModified = true;
            int res = db.SaveChanges();
            if (res == 1)
                Response.Write("<script>alert('修改成功！');window.location='/Indent/Main/1'</script>");
            else
                Response.Write("<script>alert('修改失败！');window.location='/Indent/Main/1'</script>");

        }

        /// <summary>
        /// 添加新的订单
        /// </summary>
        /// <param name="model"></param>
        [HttpGet]
        public ActionResult Add() { return View(); }
        /// <summary>
        /// 添加新的订单
        /// </summary>
        /// <param name="model"></param>
        [HttpPost]
        public ActionResult Add(T_Infor_Indent model)
        {
            //查询ICode最大值
            long? maxcode = db.T_Infor_Indent.Select(o => o.ICode).Max();
            if (maxcode != null)
            {
                model.ICode = ++maxcode;
                db.T_Infor_Indent.Add(model);
                int res = db.SaveChanges();
                if (res == 1)
                    Response.Write("<script>alert('添加成功！'); window.location='/Indent/Main/1'</script>");
                return Redirect("/Indent/Main/" + ViewBag.TotalPage);
            }
            return Redirect("/Indent/Add");
        }

        /// <summary>
        /// 添加新的订单
        /// </summary>
        /// <param name="model"></param>
        [HttpPost]
        public void Add1()
        {
            try
            {
                T_Infor_Indent model = new T_Infor_Indent()
                {
                    //ICode = Convert.ToInt64(Request.Form["Code"]),
                    IContent = Request.Form["Content"],
                    ISigningDate = Convert.ToDateTime(Request.Form["SigningDate"]),
                    IExpirationDate = Convert.ToDateTime(Request.Form["ExpirationDate"]),
                    IDeliveryDate = Convert.ToDateTime(Request.Form["DeliveryDate"]),
                    IRemarks = Request.Form["Remarks"],
                    IState = Convert.ToInt32(Request.Form["State"])
                };
                //查询ICode最大值
                long? maxcode = db.T_Infor_Indent.Select(o => o.ICode).Max();
                if (maxcode != null)
                {
                    model.ICode = ++maxcode;
                    db.T_Infor_Indent.Add(model);
                    int res = db.SaveChanges();
                    if (res == 1)
                        Response.Write("<script>alert('添加成功！'); window.location='/Indent/Main/1'</script>");
                }
                Response.Write("<script>alert('添加失败！'); window.location='/Indent/Main/1'</script>");
            }
            catch (Exception)
            {
                Response.Write("<script>alert('出现错误！'); window.location='/Indent/Main/1'</script>");
            }
        }


        #region 分页

        /// <summary>  
        /// 分页查询 + 条件查询 + 排序  
        /// </summary>  
        /// <typeparam name="Tkey">泛型</typeparam>  
        /// <param name="pageSize">每页大小</param>  
        /// <param name="pageIndex">当前页码</param>  
        /// <param name="total">总数量</param>  
        /// <param name="whereLambda">查询条件</param>  
        /// <param name="orderbyLambda">排序条件</param>  
        /// <param name="isAsc">是否升序</param>  
        /// <returns>IQueryable 泛型集合</returns>  
        public IQueryable<T_Infor_Indent> LoadPageItems<Tkey>(int pageSize, int pageIndex, out int total, Expression<Func<T_Infor_Indent, bool>> whereLambda, Func<T_Infor_Indent, Tkey> orderbyLambda, bool isAsc)
        {
            total = db.Set<T_Infor_Indent>().Where(whereLambda).Count();
            if (isAsc)
            {
                var temp = db.Set<T_Infor_Indent>().Where(whereLambda)
                             .OrderBy<T_Infor_Indent, Tkey>(orderbyLambda)
                             .Skip(pageSize * (pageIndex - 1))
                             .Take(pageSize);
                return temp.AsQueryable();
            }
            else
            {
                var temp = db.Set<T_Infor_Indent>().Where(whereLambda)
                           .OrderByDescending<T_Infor_Indent, Tkey>(orderbyLambda)
                           .Skip(pageSize * (pageIndex - 1))
                           .Take(pageSize);
                return temp.AsQueryable();
            }
        }

        #endregion


    }
}
