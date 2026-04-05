using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using PositiveNews.Infrastructure.Persistence;
using PositiveNews.Web.Models;
using System.Diagnostics;

namespace PositiveNews.Web.Controllers
{
    public class HomeController : Controller
    {
        private readonly AppDbContext _context;
        private const int PageSize = 10;

        public HomeController(AppDbContext context)
        {
            _context = context;
        }

        // GET: /Home/Index?page=1
        public async Task<IActionResult> Index(int page = 1)
        {
            // 1. Base query for active articles
            var query = _context.ArticlesMetadata
                .Include(a => a.Source)
                .Where(a => a.IsActive)
                .OrderByDescending(a => a.PublishedAt)
                .AsNoTracking();

            // 2. Pagination logic
            var totalArticles = await query.CountAsync();
            var totalPages = (int)Math.Ceiling(totalArticles / (double)PageSize);

            var articles = await query
                .Skip((page - 1) * PageSize)
                .Take(PageSize)
                .Select(a => new ArticlePreviewViewModel
                {
                    Id = a.Id,
                    SourceName = a.Source.Name,
                    SourceLogoUrl = a.Source.LogoUrl,
                    Title = a.Title,
                    Author = a.Author,
                    PublishedAt = a.PublishedAt,
                    ImageTag = a.ImageTag,
                    SummaryShort = a.SummaryShort ?? "No summary available."
                })
                .ToListAsync();

            var viewModel = new FeedIndexViewModel
            {
                Articles = articles,
                CurrentPage = page,
                TotalPages = totalPages
            };

            return View(viewModel);
        }

        // GET: /Home/ReadArticle/5
        public async Task<IActionResult> ReadArticle(long id)
        {
            var article = await _context.ArticlesMetadata
                .Include(a => a.Source)
                .Include(a => a.Content)
                .AsNoTracking()
                .FirstOrDefaultAsync(a => a.Id == id && a.IsActive);

            if (article == null)
            {
                return NotFound();
            }

            var viewModel = new ArticleDetailViewModel
            {
                Title = article.Title,
                SourceName = article.Source.Name,
                Author = article.Author,
                PublishedAt = article.PublishedAt,
                ContentHtml = article.Content?.ContentRaw
            };

            return View(viewModel);
        }

        [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
        public IActionResult Error()
        {
            return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
        }
    }
}