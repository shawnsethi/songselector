using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace SongSelector.Models
{
    public class Playlist
    {
        public string Name { get; set; }
        public int PlaylistID { get; set; }
        public string UserID { get; set; }
        public List<Song> Songs { get; set; }
    }
}