using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Web;
using System.ComponentModel;
using System.Web.Mvc;

namespace SongSelector.Models
{
    [Bind(Exclude = "SongID")]
    public class Song
    {
        [ScaffoldColumn(false)]
        public int SongID { get; set; }
        public string Artist { get; set; }
        public string Title { get; set; }

        [DisplayFormat(DataFormatString = "{0:n1}")]
        public float BPM { get; set; }
        public int Key { get; set; }
        public string SpotifyID { get; set; }
        public string UserID { get; set; }

    }
}