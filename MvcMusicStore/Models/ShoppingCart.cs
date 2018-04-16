using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace MvcMusicStore.Models
{
    /// <summary>
    /// 购物车业务类
    /// </summary>
    public class ShoppingCart
    {
        /// <summary>
        /// 购物编号
        /// </summary>
        public string ShoppingCartId { get; set; }

        /// <summary>
        /// Session键名
        /// </summary>
        public const string CartSessionKey = "CartId";

        private MvcMusicStoreEntities context = new MvcMusicStoreEntities();

        #region 根据用户的请求，获取购物车id
        /// <summary>
        /// 根据用户的请求，获取Session的信息
        /// </summary>
        /// <param name="context"></param>
        /// <returns></returns>
        public string GetCartId(HttpContextBase context)
        {
            if (context.Session[CartSessionKey] == null)
            {
                //对于没有登录的用户，我们需要为他们创建一个临时的唯一标识，这里使用CUID，全局唯一标识符，
                //对于已经登录的用户，我们直接使用他们的名称，保存在Session中
                //if (!string.IsNullOrWhiteSpace(context.User.Identity.Name))
                //{
                //    context.Session[CartSessionKey] = context.User.Identity.Name;
                //}
                if (context.Session["User"] != null)
                {
                    context.Session[CartSessionKey] = (context.Session["User"] as UserInfo).LoginId;
                }
                else
                {
                    Guid tempCartId = Guid.NewGuid();
                    context.Session[CartSessionKey] = tempCartId.ToString();
                }
            }
            return context.Session[CartSessionKey].ToString();
        } 
        #endregion

        #region 创建购物车对象
        /// <summary>
        /// 创建购物车对象
        /// </summary>
        /// <param name="context"></param>
        /// <returns></returns>
        public static ShoppingCart GetCart(HttpContextBase context)
        {
            var cart = new ShoppingCart();
            cart.ShoppingCartId = cart.GetCartId(context);
            return cart;
        } 
        #endregion

        public static ShoppingCart GetCart(Controller control)
        {
            return GetCart(control.HttpContext);
        }

        #region 将专辑添加到购物车
        /// <summary>
        /// 将专辑作为参数加入到购物车中，在Cart表中跟踪每个专辑的数量
        /// 在这个方法中，我们将会检查是在表中增加一行，还是仅仅在用户已经选择的专辑上增加数量
        /// </summary>
        /// <param name="album"></param>
        public void AddToCart(Album album)
        {
            //根据cartId和购买专辑的编号获取购物车对象
            var cartItem = context.Carts
                                .Where(m => m.CartId == ShoppingCartId && m.AlbumId == album.AlbumId)
                                .Select(m => m).FirstOrDefault();
            //如果没有该条信息，则新增一条
            if (cartItem == null)
            {
                cartItem = new Cart 
                { 
                    AlbumId = album.AlbumId,
                    CartId = ShoppingCartId,
                    Count = 1,
                    DateCreated = DateTime.Now
                };
                //添加Cart
                context.Carts.Add(cartItem);
            }
            else
            {
                //如果存在，则改变该商品购买的数量
                cartItem.Count++;
            }
            context.SaveChanges();
        } 
        #endregion

        #region 根据购物车id从购物车中删除一项
        /// <summary>
        /// 根据购物车id从购物车中删除一项
        /// </summary>
        /// <param name="recordId"></param>
        public void RemoveFromCart(int recordId)
        {
            Cart cart = new Cart { RecordId = recordId};
            context.Carts.Attach(cart);
            context.Carts.Remove(cart);
            context.SaveChanges();
        } 
        #endregion

        #region 更新数量
        /// <summary>
        /// 更新数量（ajax修改购物车数量时调用）
        /// </summary>
        /// <param name="recordId"></param>
        /// <param name="count"></param>
        /// <returns></returns>
        public int ChangeCount(int recordId, int count)
        {
            //根据购物车编号获取购物车对象
            var cartItem = context.Carts.Find(recordId);
            if (cartItem != null)
            {
                cartItem.Count = count;
                context.SaveChanges();
                return 1;
            }
            else
            {
                return -1;
            }
        } 
        #endregion

        #region 删除用户购物车中所有的项目
        /// <summary>
        /// 删除用户购物车中所有的项目
        /// </summary>
        public void EmptyCart()
        {
            var carts = context.Carts
                            .Where(m=>m.CartId == ShoppingCartId)
                            .Select(m=>m).ToList();
            foreach (var c in carts)
            {
                context.Carts.Remove(c);
            }
            context.SaveChanges();
        } 
        #endregion

        #region 获取购物车项目的列表用来显示或处理
        /// <summary>
        /// 获取购物车项目的列表用来显示或处理
        /// </summary>
        /// <returns></returns>
        public IList<Cart> GetCartItems()
        {
            return context.Carts
                        .Where(m => m.CartId == ShoppingCartId)
                        .Select(m => m).ToList();
        } 
        #endregion

        #region 获取购物车商品数量
        /// <summary>
        /// 获取购物车商品数量
        /// </summary>
        /// <returns></returns>
        public int GetCount()
        {
            int count = 0;
            foreach (var c in GetCartItems())
            {
                count += c.Count;
            }
            return count;
        } 
        #endregion

        #region 获取购物车商品总价
		/// <summary>
        /// 获取购物车商品总价
        /// </summary>
        /// <returns></returns>
        public decimal GetTotal()
        {
            decimal totalMoneys = 0.00M;
            foreach (var c in GetCartItems())
            {
                totalMoneys += c.Album.Price * c.Count;
            }
            return totalMoneys;
        }
        #endregion

        #region 提交订单
        /// <summary>
        /// 将购物车转换为结账处理过程中的订单
        /// </summary>
        /// <param name="order"></param>
        /// <returns></returns>
        public int CreateOrder(Order order)
        {
            //获取所有的购物车商品列表
            var cartItems = GetCartItems();
            //遍历购物车中的商品，增加订单的详细信息
            foreach (var cart in cartItems)
            {
                var orderDetail = new OrderDetail
                {
                    OrderId = order.OrderId,
                    AlbumId = cart.Album.AlbumId,
                    Quantity = cart.Count,
                    UnitPrice = cart.Album.Price
                };
                //添加一项详细订单
                context.OrderDetails.Add(orderDetail);
            }
            context.SaveChanges();
            //清空购物车
            EmptyCart();
            //返回订单号
            return order.OrderId;
        } 
        #endregion

        #region 当用户已经登录，将他们的购物车与用户名相关联
        /// <summary>
        /// 当用户已经登录，将他们的购物车与用户名相关联
        /// </summary>
        /// <param name="userName"></param>
        public void MigrateCart(string userName)
        {
            //获取所有cartId为用户名的购物车商品列表
            var cartItems_userName = context.Carts
                                    .Where(m => m.CartId == userName)
                                    .Select(m => m).ToList();
            //获取所有cartId为Guid的购物车商品列表
            var cartItems_Guid = GetCartItems();
            //遍历集合，如果存在购物车里相同的两张专辑，
            //就删除cartId为Guid的那张专辑，再修改cartId为用户名的专辑购买数量
            foreach (var cu in cartItems_userName)
            {
                foreach (var cg in cartItems_Guid)
                {
                    if (cg.AlbumId == cu.AlbumId)
                    {
                        //删除一项
                        context.Carts.Remove(cg);
                        //修改购买数量
                        cu.Count += cg.Count;
                    }
                }
            }
            foreach (var item in cartItems_Guid)
            {
                //修改购物车表的用户Id
                if (item.CartId != userName)
                    item.CartId = userName;
            }
            context.SaveChanges();
        } 
        #endregion
    }
}