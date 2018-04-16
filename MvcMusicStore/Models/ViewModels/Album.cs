using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;

namespace MvcMusicStore.Models
{
    [MetadataType(typeof(AlbumMetaData))]
    public partial class Album
    {
        public class AlbumMetaData
        {
            [DisplayName("专辑编号")]
            public int AlbumId { get; set; }

            [DisplayName("流派")]
            [Required(ErrorMessage="{0}不能为空")]
            public int GenreId { get; set; }

            [DisplayName("艺术家")]
            [Required(ErrorMessage = "{0}不能为空")]
            public int ArtistId { get; set; }

            [DisplayName("标题")]
            [Required(ErrorMessage = "{0}不能为空")]
            public string Title { get; set; }

            [DisplayName("单价")]
            [Required(ErrorMessage = "{0}不能为空")]
            public decimal Price { get; set; }

            [DisplayName("封面图片")]
            public string AlbumArtUrl { get; set; }
        }
    }
}