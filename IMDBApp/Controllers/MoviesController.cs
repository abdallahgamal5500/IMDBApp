using IMDBApp.Models;
using IMDBApp.ViewModels;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.ViewFeatures;
using Microsoft.EntityFrameworkCore;
using NToastNotify;
using System.Diagnostics;

namespace IMDBApp.Controllers
{
    public class MoviesController : Controller
    {
        private readonly List<string> _allowedExtentions = new List<string> { ".jpg", ".png" };
        private readonly long _maxAllowedPosterSize = 1048576;
        private readonly IToastNotification _toastNotification;
        private readonly AppDbContext _context;
        public MoviesController(AppDbContext context, IToastNotification toastNotification)
        {
            _context = context;
            _toastNotification = toastNotification;
        }

        public async Task <IActionResult> Index()
        {
            var movies = await _context.Movies.OrderByDescending(x => x.Rate).ToListAsync();
            return View(movies);
        }

        public async Task<IActionResult> Create()
        {
            var viewModel = new MovieFormViewModel
            {
                Genres = await _context.Genres
                    .OrderBy(x => x.Name)
                    .ToListAsync()
            };
            return View("MovieForm", viewModel);
        }
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(MovieFormViewModel viewModel)
        {
            // this way is used to print all validations errors in the model state
            /*
            var errors = ModelState.Values.SelectMany(v => v.Errors);
            foreach (var error in errors) 
                Debug.WriteLine("error: " + error.ErrorMessage);
            */
            
            if (!ModelState.IsValid)
            {
                viewModel.Genres = await _context.Genres.OrderBy(x => x.Name).ToListAsync();
                return View("MovieForm", viewModel);
            }

            var files = Request.Form.Files;
            if (!files.Any())
            {
                viewModel.Genres = await _context.Genres.OrderBy(x => x.Name).ToListAsync();
                ModelState.AddModelError("Poster", "Please Select Movie Poster!");
                return View("MovieForm", viewModel);
            }

            var poster = files.FirstOrDefault();
            
            if (!_allowedExtentions.Contains(Path.GetExtension(poster.FileName.ToLower())))
            {
                viewModel.Genres = await _context.Genres.OrderBy(x => x.Name).ToListAsync();
                ModelState.AddModelError("Poster", "Only .jpg, .png images is allowed!");
                return View("MovieForm", viewModel);
            }

            if (poster.Length > _maxAllowedPosterSize)
            {
                viewModel.Genres = await _context.Genres.OrderBy(x => x.Name).ToListAsync();
                ModelState.AddModelError("Poster", "Posters can't be more than 1MB!");
                return View("MovieForm", viewModel);
            }
         
            using var memoryStream = new MemoryStream();
            await poster.CopyToAsync(memoryStream);

            var movie = new Movie
            {
                Title = viewModel.Title,
                GenreId = viewModel.GenreId,
                Year = viewModel.Year,
                Rate = viewModel.Rate,
                StoryLine = viewModel.StoryLine,
                Poster = memoryStream.ToArray()
            };

            _context.Add(movie);
            _context.SaveChanges();

            _toastNotification.AddSuccessToastMessage("Movie is Add Successfully!");
            return RedirectToAction("Index");
        }

        public async Task<IActionResult> Edit(int? id)
        {
            Debug.WriteLine("Error-1");
            if (id == null)
                return BadRequest();

            var movie = await _context.Movies.FindAsync(id);
            if (movie == null)
                return NotFound();

            var viewModel = new MovieFormViewModel
            {
                Id = (int) id,
                Title = movie.Title,
                GenreId = movie.GenreId,
                Year = movie.Year,
                Rate = movie.Rate,
                StoryLine = movie.StoryLine,
                Poster = movie.Poster,
                Genres = await _context.Genres.OrderBy(x => x.Name).ToListAsync()
            };
            return View("MovieForm", viewModel);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<ActionResult> Edit(MovieFormViewModel viewModel) 
        {
            if (!ModelState.IsValid)
            {
                viewModel.Genres = await _context.Genres.OrderBy(x => x.Name).ToListAsync();
                return View("MovieForm", viewModel);
            }

            if (viewModel.Id == null)
                return BadRequest();

            var movie = await _context.Movies.FindAsync(viewModel.Id);
            if (movie == null)
                return NotFound();

            var files = Request.Form.Files;
            if (files.Any())
            {
                var poster = files.FirstOrDefault();
                using var memoryStream = new MemoryStream();
                await poster.CopyToAsync(memoryStream);
                viewModel.Poster = memoryStream.ToArray();

                if (!_allowedExtentions.Contains(Path.GetExtension(poster.FileName.ToLower())))
                {
                    viewModel.Genres = await _context.Genres.OrderBy(x => x.Name).ToListAsync();
                    ModelState.AddModelError("Poster", "Only .jpg, .png images is allowed!");
                    return View("MovieForm", viewModel);
                }

                if (poster.Length > _maxAllowedPosterSize)
                {
                    viewModel.Genres = await _context.Genres.OrderBy(x => x.Name).ToListAsync();
                    ModelState.AddModelError("Poster", "Posters can't be more than 1MB!");
                    return View("MovieForm", viewModel);
                }

                movie.Poster = viewModel.Poster;
            }

            movie.Title = viewModel.Title;
            movie.GenreId = viewModel.GenreId;
            movie.Year = viewModel.Year;
            movie.Rate = viewModel.Rate;
            movie.StoryLine = viewModel.StoryLine;

            _context.SaveChanges();
            _toastNotification.AddSuccessToastMessage("Movie is Updated Successfully!");

            return RedirectToAction("Index");
        }

        public async Task <IActionResult> Details(int? id)
        {
            if (id == null)
                return BadRequest();

            var movie = await _context.Movies.Include(x => x.Genre).SingleAsync(y => y.Id == id);
            if (movie == null)
                return NotFound();

            return View(movie);
        }

        public async Task<IActionResult> Delete(int? id)
        {
            Debug.WriteLine("Error-1");
            if (id == null)
                return BadRequest();

            var movie = await _context.Movies.FindAsync(id);

            if (movie == null)
                return NotFound();

            Debug.WriteLine("Error-2");
            _context.Movies.Remove(movie);
            _context.SaveChanges();

            return Ok();
        }
    }
}
