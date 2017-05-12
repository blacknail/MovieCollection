using System;
using System.Data;
using System.Data.Entity;
using System.Linq;
using System.Net;
using System.Web;
using System.Web.Mvc;
using MovieCollection.Models;
using PagedList;
using System.IO;
using Microsoft.AspNet.Identity;

namespace MovieCollection.Controllers
{
    public class MoviesController : Controller
    {
        private ApplicationDbContext db = new ApplicationDbContext();

        // GET: Movies
        public ActionResult Index(string sortOrder, string currentFilter, string searchString, int? page)
        {
            ViewBag.CurrentSort = sortOrder;
            ViewBag.TitleSortParm = String.IsNullOrEmpty(sortOrder) ? "title_desc" : "";
            ViewBag.ReleaseDateSortParm = sortOrder == "ReleaseDate" ? "releaseDate_desc" : "ReleaseDate";
            ViewBag.CurrentUser= User.Identity.GetUserId();

            if (searchString != null)
            {
                page = 1;
            }
            else
            {
                searchString = currentFilter;
            }

            ViewBag.CurrentFilter = searchString;

            var movies = from s in db.Movies
                           select s;
            if (!String.IsNullOrEmpty(searchString))
            {
                movies = movies.Where(s => s.Title.Contains(searchString)
                                       || s.Description.Contains(searchString));
            }
            switch (sortOrder)
            {
                case "title_desc":
                    movies = movies.OrderByDescending(s => s.Title);
                    break;
                case "ReleaseDate":
                    movies = movies.OrderBy(s => s.ReleaseDate);
                    break;
                case "releaseDate_desc":
                    movies = movies.OrderByDescending(s => s.ReleaseDate);
                    break;
                default: 
                    movies = movies.OrderBy(s => s.Title);
                    break;
            }

            int pageSize = 3;
            int pageNumber = (page ?? 1);
            return View(movies.ToPagedList(pageNumber, pageSize));
        }

        // GET: Movies/Details/5
        public ActionResult Details(int? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            Movie movie = db.Movies.Find(id);
            if (movie == null)
            {
                return HttpNotFound();
            }
            else {
                ViewBag.CurrentUser = User.Identity.GetUserId();
            }           

            return View(movie);
        }

        // GET: Movies/Create
        public ActionResult Create()
        {
            return View();
        }

        // POST: Movies/Create
        // To protect from overposting attacks, please enable the specific properties you want to bind to, for 
        // more details see https://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Create([Bind(Include = "MovieID,Title,Description,ReleaseDate,Director,Poster")] Movie movie, HttpPostedFileBase upload)
        {
            if (ModelState.IsValid)
            {
                var fileName = upload !=null? Guid.NewGuid().ToString() + Path.GetExtension(upload.FileName): String.Empty;
                var path = Path.Combine(Server.MapPath("~/Content/images"), fileName);

                movie.Poster = fileName;
                var currentUserId = User.Identity.GetUserId();
                movie.User = db.Users.FirstOrDefault(x => x.Id == currentUserId);
                db.Movies.Add(movie);
                db.SaveChanges();
                if (!String.IsNullOrEmpty(fileName))
                    upload.SaveAs(path);

                return RedirectToAction("Index");
            }

            return View(movie);
        }

        // GET: Movies/Edit/5
        public ActionResult Edit(int? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            Movie movie = db.Movies.Find(id);
            if (movie == null || 
                (movie.User != null && 
                    movie.User.Id != User.Identity.GetUserId())
                )
            {
                return HttpNotFound();
            }

            return View(movie);
        }

        // POST: Movies/Edit/5
        // To protect from overposting attacks, please enable the specific properties you want to bind to, for 
        // more details see https://go.microsoft.com/fwlink/?LinkId=317598.
        [Authorize]
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Edit(Movie movie, HttpPostedFileBase upload)
        {
            if (movie.User != null && movie.User.Id != User.Identity.GetUserId())
            {
                return HttpNotFound();
            }

            if (ModelState.IsValid)
            {
                var fileName = upload != null ? Guid.NewGuid().ToString() + Path.GetExtension(upload.FileName) : String.Empty;
                var path = Path.Combine(Server.MapPath("~/Content/images"), fileName);

                var oldPoster = db.Movies.Find(movie.MovieID).Poster;
                if (!String.IsNullOrEmpty(oldPoster))
                {
                    var delpath = Path.Combine(Server.MapPath("~/Content/images"),oldPoster);
                    if (System.IO.File.Exists(delpath))
                    {
                        System.IO.File.Delete(delpath);
                    }
                }

                var movieInDb = db.Movies.Find(movie.MovieID);

                movie.Poster = fileName;

                db.Entry(movieInDb).CurrentValues.SetValues(movie);
                db.Entry(movieInDb).State = EntityState.Modified;
                
                db.SaveChanges();

                if (!String.IsNullOrEmpty(fileName))
                    upload.SaveAs(path);

                return RedirectToAction("Index");
            }
            return View(movie);
        }

        // GET: Movies/Delete/5
        [Authorize]
        public ActionResult Delete(int? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            Movie movie = db.Movies.Find(id);
            if (movie == null ||
                (movie.User != null &&
                    movie.User.Id != User.Identity.GetUserId())
                )
            {
                return HttpNotFound();
            }
            return View(movie);
        }

        // POST: Movies/Delete/5
        [Authorize]
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public ActionResult DeleteConfirmed(int id)
        {
            Movie movie = db.Movies.Find(id);

            if (movie.User != null && movie.User.Id != User.Identity.GetUserId())
            {
                return HttpNotFound();
            }

            var delpath = Path.Combine(Server.MapPath("~/Content/images"), movie.Poster);
            if (System.IO.File.Exists(delpath))
            {
                System.IO.File.Delete(delpath);
            }

            db.Movies.Remove(movie);
            db.SaveChanges();
            return RedirectToAction("Index");
        }

        protected override void Dispose(bool disposing)
        {
            if (disposing)
            {
                db.Dispose();
            }
            base.Dispose(disposing);
        }
    }
}
