using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using SongSelector.Models;

namespace SongSelector.Controllers
{
    public class SongsController : Controller
    {
        SongSelectorEntities storeDB = new SongSelectorEntities();
        //
        // GET: /Songs/

        public ActionResult Index()
        {
            var songs = storeDB.Songs.ToList();
            return View(songs);
        }

        // GET: /Songs/Details/5
        public ActionResult Details(int id)
        {
            var song1 = storeDB.Songs.Find(id);
            return View(song1);
        }

        public ActionResult Browse(int id)
        {
            var playlistModel = storeDB.Playlists.Include("Songs").Single(g => g.PlaylistID == id);
            return View(playlistModel);
        }

    }
}
