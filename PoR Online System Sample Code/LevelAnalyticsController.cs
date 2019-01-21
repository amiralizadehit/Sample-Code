using PoR_Server_Side.Models;
using System.Collections.Generic;
using System.Data.Entity;
using System.Linq;
using System.Net;
using System.Web.Http;

namespace PoR_Server_Side.Controllers.API
{
    public class LevelAnalyticsController : ApiController
    {
        private ApplicationDbContext _context;


        public LevelAnalyticsController()
        {
            _context = new ApplicationDbContext();
        }

        // GET /api/levelanalytics
        public IEnumerable<LevelAnalytic> GetLevelAnalytics()
        {

            return _context.LevelAnalytics.Include(l => l.PlayerInfo).ToList();
        }

        // GET /api/levelanalytics/1
        public LevelAnalytic GeLevelAnalytic(int id)
        {
            var levelAnalytic = _context.LevelAnalytics.Include(l => l.PlayerInfo).SingleOrDefault(c => c.Id == id);
            if (levelAnalytic == null)
                throw new HttpResponseException(HttpStatusCode.NotFound);
            return levelAnalytic;
        }

        // POST /api/levelanalytics
        [HttpPost]
        public LevelAnalytic CreateLevelAnalytic(LevelAnalytic levelAnalytic)
        {
            if (!ModelState.IsValid)
                throw new HttpResponseException(HttpStatusCode.BadRequest);

            _context.LevelAnalytics.Add(levelAnalytic);
            _context.SaveChanges();

            return levelAnalytic;
        }

        // DELETE /api/levelanalytics/1
        public void DeleteLevelAnalytic(int id)
        {
            var levelAnalyticInDb = _context.LevelAnalytics.SingleOrDefault(c => c.Id == id);
            if (levelAnalyticInDb == null)
            {
                throw new HttpResponseException(HttpStatusCode.NotFound);
            }
            _context.LevelAnalytics.Remove(levelAnalyticInDb);
            _context.SaveChanges();
        }
    }
}