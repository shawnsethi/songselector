using System;
using System.Collections.Generic;
using System.Data;
using System.Data.Entity;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using SongSelector.Models;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System.Text;
using System.Net;
using System.Xml;
using System.Xml.XPath;
using System.Xml.Linq;

namespace SongSelector.Controllers
{
    public class SongManagerController : Controller
    {
        private SongSelectorEntities db = new SongSelectorEntities();

        //
        // GET: /SongManager/

        public ActionResult Index()
        {
            var foundSongs = from s in db.Songs
                             where ((s.BPM >= 10) && (s.BPM < 200) && (s.Key >= 0))
                             orderby s.Artist, s.Title
                             select s;
            var unfoundSongs = from s in db.Songs
                                   where ((s.BPM < 10) || (s.BPM > 200) || (s.Key == null ))
                                   orderby s.Artist, s.Title
                                   select s;
            ViewBag.unfoundSongs = unfoundSongs.ToList();
            return View(foundSongs);
        }

        //
        // GET: /SongManager/Details/5

        public ActionResult Details(int id = 0)
        {
            Song song = db.Songs.Find(id);
            if (song == null)
            {
                return HttpNotFound();
            }
            int keyminus1 = -2;
            int keyplus1 = -2;
            if (song.Key >= 0 && song.BPM >= 0)
            {
                keyminus1 = song.Key - 1;
                if (keyminus1 == -1)
                    keyminus1 = 11;
                keyplus1 = song.Key + 1;
                if (keyplus1 == 12)
                    keyplus1 = 0;
            }
      
            ViewBag.relatedSongs = from s in db.Songs
                                   where ( (s.BPM > (song.BPM - 7)) && (s.BPM < (song.BPM + 7)) && ( (s.Key == keyminus1) || (s.Key == song.Key) || (s.Key == keyplus1) ) && (s.SongID != song.SongID))
                                   orderby s.Artist, s.Title
                                   select s;
            return View(song);
        }

        //
        // GET: /SongManager/Create

        public ActionResult Create()
        {
            return View();
        }

        //
        // POST: /SongManager/Create

        [HttpPost]
        public ActionResult Create(Song song)
        {
            if (ModelState.IsValid)
            {
                db.Songs.Add(song);
                db.SaveChanges();
                return RedirectToAction("Index");
            }

            return View(song);
        }

        //
        // GET: /SongManager/Edit/5

        public ActionResult Edit(int id = 0)
        {
            Song song = db.Songs.Find(id);
            if (song == null)
            {
                return HttpNotFound();
            }
            
            return View(song);
        }

        //
        // POST: /SongManager/Edit/5

        [HttpPost]
        public ActionResult Edit(Song newsong)
        {
            if (ModelState.IsValid)
            {
                SongSelectorEntities db2 = new SongSelectorEntities();
                
                Song oldsong = db2.Songs.Find(newsong.SongID);
                if ( (newsong.Artist != oldsong.Artist) || (newsong.Title != oldsong.Title) )
                {
                    string uri = "http://developer.echonest.com/api/v4/song/search?api_key=6XUOAXHJOW28GGGRH&format=xml&results=1&artist=" + HttpUtility.UrlEncode(newsong.Artist) + "&title=" + HttpUtility.UrlEncode(newsong.Title) + "&bucket=audio_summary";
                    WebClient c = new WebClient();
                    string result = c.DownloadString(uri);
                    XDocument doc = XDocument.Parse(result);
                    result = c.DownloadString(uri);
                    doc = XDocument.Parse(result);
                    if (doc.Descendants("key").Any())
                    {
                        newsong.BPM = (float)doc.Descendants("tempo").First();
                        newsong.Key = (int)doc.Descendants("key").First();
                    }
                }

                db.Entry(newsong).State = EntityState.Modified;
                db.SaveChanges();
                return RedirectToAction("Index");
            }
            return View(newsong);
        }

        //
        // GET: /SongManager/Delete/5

        public ActionResult Delete(int id = 0)
        {
            Song song = db.Songs.Find(id);
            if (song == null)
            {
                return HttpNotFound();
            }
            return View(song);
        }

        //
        // POST: /SongManager/Delete/5

        [HttpPost, ActionName("Delete")]
        public ActionResult DeleteConfirmed(int id)
        {
            Song song = db.Songs.Find(id);
            db.Songs.Remove(song);
            db.SaveChanges();
            return RedirectToAction("Index");
        }

        protected override void Dispose(bool disposing)
        {
            db.Dispose();
            base.Dispose(disposing);
        }

        public ActionResult Import()
        {
            return View();
        }

        [HttpPost]
        public ActionResult Import(string raw)
        {
            string[] spotify_ids = raw.Split(new string[] { "\r\n" }, StringSplitOptions.RemoveEmptyEntries);
            int success_counter = 0;
            int fail_counter = 0;
            string trimmed_id;
            foreach (string id in spotify_ids)
            {
                if (id.Contains("spotify:track:"))
                {
                    trimmed_id = id.Replace("spotify:track:", "").Trim();
                    Song newsong = new Song();
                    newsong.SpotifyID = trimmed_id;
                    newsong = lookup_song(newsong);
                    success_counter++;
                    System.Threading.Thread.Sleep(500); // Echonest has a rate limit for non-paying API keys
                    db.Songs.Add(newsong);
                    db.SaveChanges();
                }
                else
                {
                    fail_counter++;

                }
            }
            TempData["message"] = success_counter + " imported.  " + fail_counter + " failed.";
            return RedirectToAction("Index");
        }




        private Song lookup_song(Song song)
        {
            string uri = "http://developer.echonest.com/api/v4/track/profile?api_key=6XUOAXHJOW28GGGRH&format=xml&id=spotify-WW:track:" + song.SpotifyID + "&bucket=audio_summary";
            WebClient c = new WebClient();
            string result = c.DownloadString(uri);
            XDocument doc = XDocument.Parse(result);

            if ((string)doc.Descendants("message").First() == "Success")
            {
                song.Title = (string)doc.Descendants("title").First();
                song.Artist = (string)doc.Descendants("artist").First();
                if ( (!doc.Descendants("key").Any()) || ((float)doc.Descendants("tempo").First() > 200) )
                {
                    uri = "http://developer.echonest.com/api/v4/song/search?api_key=PARKQO4U8BV39WY6S&format=xml&results=1&artist=" + HttpUtility.UrlEncode(song.Artist) + "&title=" + HttpUtility.UrlEncode(song.Title) + "&bucket=audio_summary";
                    result = c.DownloadString(uri);
                    doc = XDocument.Parse(result);
                    if (doc.Descendants("key").Any())
                    {
                        song.BPM = (float)doc.Descendants("tempo").First();
                        song.Key = (int)doc.Descendants("key").First();
                    }
                }
                else
                {
                    song.BPM = (float)doc.Descendants("tempo").First();
                    song.Key = (int)doc.Descendants("key").First();
                }
            }
           return song;
        }
    }




}