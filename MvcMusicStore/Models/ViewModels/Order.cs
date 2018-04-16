using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;

namespace MvcMusicStore.Models
{
    [MetadataType(typeof(OrderMetaData))]
    public partial class Order
    {
        public class OrderMetaData 
        {
            [DisplayName("订单号")]
            public int OrderId { get; set; }

            [DisplayName("用户编号")]
            public int UserId { get; set; }

            [DisplayName("订单日期")]
            public System.DateTime OrderDate { get; set; }

            [DisplayName("收货地址")]
            [Required(ErrorMessage = "{0}不能为空")]
            public string Address { get; set; }

            [DisplayName("邮编")]
            [Required(ErrorMessage = "{0}不能为空")]
            [StringLength(6, MinimumLength=6,ErrorMessage = "{0}长度为{1}位")]
            [RegularExpression(@"^[0-9]+$", ErrorMessage = "{0}只能由数字组成")]
            public string PostalCode { get; set; }

            [DisplayName("联系方式")]
            [Required(ErrorMessage = "{0}不能为空")]
            [StringLength(11, MinimumLength = 3, ErrorMessage = "{0}长度在{2}到{1}之间")]
            [RegularExpression(@"^[0-9-]+$", ErrorMessage = "{0}格式错误")]
            public string Phone { get; set; }

            [DisplayName("电子邮件")]
            [Required(ErrorMessage = "{0}不能为空")]
            [RegularExpression(@"^\w+@\w+(\.[a-zA-Z]{2,3}){1,2}$", ErrorMessage = "{0}格式错误")]
            public string Email { get; set; }
            
            [DisplayName("总金额")]
            public decimal Amount { get; set; }
        }
    }
}