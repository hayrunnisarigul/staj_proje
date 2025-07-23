using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Http;
using staj_proje.Models;
using staj_proje.Helpers;
using System.Linq;

namespace staj_proje.Controllers
{
    public class HomeController : Controller
    {
        private readonly AppDbContext _context;

        public HomeController(AppDbContext context)
        {
            _context = context;
        }

        // Ana sayfa
        public IActionResult Index()
        {
            if (HttpContext.Session.GetString("Username") == null)
                return RedirectToAction("Login", "Account");

            ViewBag.ShowMenu = true;
            ViewBag.Username = HttpContext.Session.GetString("Username");
            return View();
        }

        // Profil g�r�nt�leme
        [HttpGet]
        public IActionResult Profile()
        {
            var username = HttpContext.Session.GetString("Username");
            if (username == null)
                return RedirectToAction("Login", "Account");

            var user = _context.Users.FirstOrDefault(u => u.Username == username);
            if (user == null)
                return RedirectToAction("Login", "Account");

            ViewBag.ShowMenu = true;
            return View(user);
        }

        // Profil g�ncelleme
        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult Profile(string yeniKullaniciAdi, string eskiSifre, string yeniSifre)
        {
            var username = HttpContext.Session.GetString("Username");
            var user = _context.Users.FirstOrDefault(u => u.Username == username);

            if (user == null)
                return RedirectToAction("Login", "Account");

            ViewBag.ShowMenu = true;

            // Validasyon kontrolleri
            if (string.IsNullOrWhiteSpace(yeniKullaniciAdi) ||
                string.IsNullOrWhiteSpace(eskiSifre) ||
                string.IsNullOrWhiteSpace(yeniSifre))
            {
                ViewBag.Hata = "T�m alanlar� doldurunuz.";
                return View(user);
            }

            // Eski �ifre do�ru mu kontrol et
            var hashedEskiSifre = SecurityHelper.Sha256Hash(eskiSifre);
            if (user.Password != hashedEskiSifre)
            {
                ViewBag.Hata = "Mevcut �ifreniz yanl��.";
                return View(user);
            }

            // Yeni kullan�c� ad� zaten var m� kontrol et
            if (_context.Users.Any(u => u.Username == yeniKullaniciAdi && u.Id != user.Id))
            {
                ViewBag.Hata = "Bu kullan�c� ad� ba�ka biri taraf�ndan kullan�l�yor.";
                return View(user);
            }

            try
            {
                // G�ncelleme i�lemi
                user.Username = yeniKullaniciAdi;
                user.Password = SecurityHelper.Sha256Hash(yeniSifre);
                _context.SaveChanges();

                // Session'� g�ncelle
                HttpContext.Session.SetString("Username", yeniKullaniciAdi);

                TempData["Basarili"] = "Profil ba�ar�yla g�ncellendi.";
                return RedirectToAction("Profile");
            }
            catch (Exception ex)
            {
                ViewBag.Hata = "Profil g�ncellenirken bir hata olu�tu.";
                return View(user);
            }
        }

        // T�m kullan�c�lar� listele
        public IActionResult AllUsers()
        {
            if (HttpContext.Session.GetString("Username") == null)
                return RedirectToAction("Login", "Account");

            var users = _context.Users.OrderBy(u => u.Username).ToList();
            ViewBag.ShowMenu = true;
            return View(users ?? new List<User>());
        }

        // Kullan�c� silme
        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult DeleteUser(int id)
        {
            if (HttpContext.Session.GetString("Username") == null)
                return RedirectToAction("Login", "Account");

            var user = _context.Users.Find(id);
            var currentUsername = HttpContext.Session.GetString("Username");

            if (user != null && user.Username != currentUsername)
            {
                try
                {
                    _context.Users.Remove(user);
                    _context.SaveChanges();
                    TempData["Basarili"] = "Kullan�c� ba�ar�yla silindi.";
                }
                catch (Exception ex)
                {
                    TempData["Hata"] = "Kullan�c� silinirken bir hata olu�tu.";
                }
            }
            else if (user?.Username == currentUsername)
            {
                TempData["Hata"] = "Kendi hesab�n�z� silemezsiniz.";
            }

            return RedirectToAction("AllUsers");
        }

        // Kullan�c� d�zenleme sayfas�
        [HttpGet]
        public IActionResult EditUser(int id)
        {
            if (HttpContext.Session.GetString("Username") == null)
                return RedirectToAction("Login", "Account");

            var user = _context.Users.Find(id);
            if (user == null)
                return RedirectToAction("AllUsers");

            ViewBag.ShowMenu = true;
            return View(user);
        }

        // Kullan�c� d�zenleme i�lemi
        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult EditUser(int id, string kullaniciAdi, string sifre)
        {
            if (HttpContext.Session.GetString("Username") == null)
                return RedirectToAction("Login", "Account");

            var user = _context.Users.Find(id);
            if (user == null)
                return RedirectToAction("AllUsers");

            ViewBag.ShowMenu = true;

            if (string.IsNullOrWhiteSpace(kullaniciAdi) || string.IsNullOrWhiteSpace(sifre))
            {
                ViewBag.Hata = "Bo� alan b�rakmay�n�z.";
                return View(user);
            }

            // Ayn� kullan�c� ad� var m� kontrol
            if (_context.Users.Any(u => u.Username == kullaniciAdi && u.Id != user.Id))
            {
                ViewBag.Hata = "Bu kullan�c� ad� ba�ka biri taraf�ndan kullan�l�yor.";
                return View(user);
            }

            try
            {
                user.Username = kullaniciAdi;
                user.Password = SecurityHelper.Sha256Hash(sifre);
                _context.SaveChanges();

                TempData["Basarili"] = "Kullan�c� ba�ar�yla g�ncellendi.";
                return RedirectToAction("AllUsers");
            }
            catch (Exception ex)
            {
                ViewBag.Hata = "Kullan�c� g�ncellenirken bir hata olu�tu.";
                return View(user);
            }
        }

        // ��k�� yapma
        public IActionResult Logout()
        {
            HttpContext.Session.Clear();
            return RedirectToAction("Login", "Account");
        }
    }
}