using System;
using System.Collections.Generic;
using System.Data.Entity.Infrastructure;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;
using System.Web;

namespace MVC笔记
{
    public class EFHelper<T> where T : class,new()
    {
        /// <summary>
        /// 数据上下文对象
        /// </summary>
        Models.笔记Entities db = new Models.笔记Entities();

        #region 增_单个——int Add(T model)
        /// <summary>
        /// 新增 实体
        /// </summary>
        /// <param name="model"></param>
        /// <returns></returns>
        public int Add(T model)
        {
            db.Set<T>().Add(model);
            return db.SaveChanges();//保存成功后，会将自增的id设置给model的主键属性，并返回受影响行数
        }
        #endregion

        #region 删_根据主键——int Del(T model)
        /// <summary>
        /// 根据 id 删除
        /// </summary>
        /// <param name="model"></param>
        /// <returns></returns>
        public int Del(T model)
        {
            db.Set<T>().Attach(model);//附加到EF
            db.Set<T>().Remove(model);//设置标识为删除
            return db.SaveChanges();
        }
        #endregion

        #region 删_根据条件——int DelBy(Expression<Func<T,bool>> whereLambda)
        /// <summary>
        /// 根据条件删除
        /// </summary>
        /// <param name="delWhere"></param>
        /// <returns></returns>
        public int DelBy(Expression<Func<T, bool>> whereLambda)
        {
            //查询要删除的数据
            List<T> listDeleting = db.Set<T>().Where(whereLambda).ToList();
            //讲要删除的数据 用删除方法添加到 EF 容器中
            listDeleting.ForEach(u =>
            {
                db.Set<T>().Attach(u);
                db.Set<T>().Remove(u);
            });
            //一次性 生成sql语句到数据库 执行删除
            return db.SaveChanges();
        }
        #endregion

        #region 改_单个——int Modify(T model, params string[] proNames)
        /// <summary>
        /// 修改 如
        /// T u = new T() { uId = 1, uLoginName = "asdfasdf" };
        /// this.Modify(u, "uLoginName");
        /// </summary>
        /// <param name="model">要修改的实体对象（其属性值已更改）</param>
        /// <param name="proNames">要修改的 属性 名称</param>
        /// <returns></returns>
        public int Modify(T model, params string[] proNames)
        {
            //将 对象 添加到 EF中
            DbEntityEntry entry = db.Entry<T>(model);
            //先设置 对象的包装 状态为 Unchanged
            entry.State = System.Data.EntityState.Unchanged;
            //循环 呗修改的属性名 数组
            foreach (string proName in proNames)
            {
                //将每个 被修改的属性的状态 设置为已修改状态；
                entry.Property(proName).IsModified = true;
                //EF中，会根据属性的状态 生成相应的sql语句
            }
            return db.SaveChanges();
        }
        #endregion

        #region 改_批量修改——int Modify(T model, Expression<Func<T,bool>> whereLambda, params string[] modifiedProNames)
        /// <summary>
        /// 批量修改
        /// </summary>
        /// <param name="model">要修改的实体对象</param>
        /// <param name="whereLambda">查询条件</param>
        /// <param name="modifiedProNames">要修改的 属性 名称</param>
        /// <returns></returns>
        public int ModifyBy(T model, Expression<Func<T, bool>> whereLambda, params string[] modifiedProNames)
        {
            //第一步：
            //查询要修改的数据
            List<T> listModifing = db.Set<T>().Where(whereLambda).ToList();

            //第二步：反射为第三步提供数据支撑
            //获取 实体类 类型对象
            Type t = typeof(T);
            //获取 实体类 所有的 公有属性
            List<PropertyInfo> proInfos = t.GetProperties(BindingFlags.Instance | BindingFlags.Public).ToList();
            //创建 实体类的属性 字典集合
            Dictionary<string, PropertyInfo> dictPros = new Dictionary<string, PropertyInfo>();
            //将 实体属性 中药修改的属性名 添加到 字典集合中 （键：属性名，值：属性对象）
            proInfos.ForEach(p =>
            {
                if (modifiedProNames.Contains(p.Name))
                {
                    dictPros.Add(p.Name, p);
                }
            });

            //第三步：根据获取到的字典来逐个修改值
            //循环 要修改的属性名
            foreach (string proName in modifiedProNames)
            {
                //判断 要修改的属性名是否在 实体类的属性集合 中存在
                if (dictPros.ContainsKey(proName))
                {
                    //如果存在，则取出要修改的 属性对象
                    PropertyInfo proInfo = dictPros[proName];
                    //取出 要修改的值
                    object newValue = proInfo.GetValue(model, null);
                    //批量设置 要修改 对象的 属性
                    foreach (T _model in listModifing)
                    {
                        //为 要修改的对象 的 要修改的属性 设置新的值
                        proInfo.SetValue(_model, newValue, null);
                    }
                }
            }

            return db.SaveChanges();
        }
        #endregion

        #region 查_根据条件查询——List<T> GetListBy(Expression<Func<T,bool>> whereLambda)
        /// <summary>
        /// 根据条件查询
        /// </summary>
        /// <param name="whereLambda"></param>
        /// <returns></returns>
        public List<T> GetListBy(Expression<Func<T, bool>> whereLambda)
        {
            return db.Set<T>().Where(whereLambda).ToList();
        }
        #endregion

        #region 查_条件查询+排序——List<T> GetListBy<TKey>
        /// <summary>
        /// 根据条件查询 并 排序
        /// </summary>
        /// <typeparam name="TKey">排序字段类型</typeparam>
        /// <param name="whereLambda">查询条件 lambda表达式</param>
        /// <param name="orderLambda">排序条件 lambda表达式</param>
        /// <returns></returns>
        public List<T> GetListBu<TKey>(Expression<Func<T, bool>> whereLambda, Expression<Func<T, TKey>> orderLambda)
        {
            return db.Set<T>().Where(whereLambda).OrderBy(orderLambda).ToList();
        }
        #endregion

        #region 查_分页——GetPagedList<TKey>
        /// <summary>
        /// 分页查询
        /// </summary>
        /// <typeparam name="TKey">排序字段类型</typeparam>
        /// <param name="pageIndex">页码</param>
        /// <param name="pageSize">页容量</param>
        /// <param name="whereLambda">条件 lambda表达式</param>
        /// <param name="orderBy">排序 lambda表达式</param>
        /// <returns></returns>
        public List<T> GetPagedList<TKey>(int pageIndex, int pageSize, Expression<Func<T, bool>> whereLambda, Expression<Func<T, TKey>> orderBy)
        {
            //分页 一定要注意： Skip 之前一定要 OrderBy
            return db.Set<T>().Where(whereLambda).OrderBy(orderBy).Skip((pageIndex - 1) * pageSize).Take(pageSize).ToList();
        }
        #endregion
    }
}