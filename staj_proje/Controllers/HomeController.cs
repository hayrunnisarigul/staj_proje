using ClosedXML.Excel;
using DocumentFormat.OpenXml.Office2010.PowerPoint;
using DocumentFormat.OpenXml.Wordprocessing;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using staj_proje.Helpers;
using staj_proje.Models;
using System.IO;
using System.Linq;
using System.Xml.Linq;
using iTextSharp.text;
using iTextSharp.text.pdf;
using System.Text;

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

        // Profil görüntüleme
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

        // Profil güncelleme
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
                ViewBag.Hata = "Tüm alanları doldurunuz.";
                return View(user);
            }

            // Eski şifre doğru mu kontrol et
            var hashedEskiSifre = SecurityHelper.Sha256Hash(eskiSifre);
            if (user.Password != hashedEskiSifre)
            {
                ViewBag.Hata = "Mevcut şifreniz yanlış.";
                return View(user);
            }

            // Yeni kullanıcı adı zaten var mı kontrol et
            if (_context.Users.Any(u => u.Username == yeniKullaniciAdi && u.Id != user.Id))
            {
                ViewBag.Hata = "Bu kullanıcı adı başka biri tarafından kullanılıyor.";
                return View(user);
            }

            try
            {
                // Güncelleme işlemi
                user.Username = yeniKullaniciAdi;
                user.Password = SecurityHelper.Sha256Hash(yeniSifre);
                _context.SaveChanges();

                // Session'ı güncelle
                HttpContext.Session.SetString("Username", yeniKullaniciAdi);

                TempData["Basarili"] = "Profil başarıyla güncellendi.";
                return RedirectToAction("Profile");
            }
            catch (Exception ex)
            {
                ViewBag.Hata = "Profil güncellenirken bir hata oluştu.";
                return View(user);
            }
        }

        // Tüm kullanıcıları listele
        public IActionResult AllUsers()
        {
            if (HttpContext.Session.GetString("Username") == null)
                return RedirectToAction("Login", "Account");

            // ID'ye göre sıralama
            var users = _context.Users.OrderBy(u => u.Id).ToList();
            ViewBag.ShowMenu = true;
            return View(users ?? new List<User>());
        }

        // Kullanıcı silme
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
                    TempData["Basarili"] = "Kullanıcı başarıyla silindi.";
                }
                catch (Exception ex)
                {
                    TempData["Hata"] = "Kullanıcı silinirken bir hata oluştu.";
                }
            }
            else if (user?.Username == currentUsername)
            {
                TempData["Hata"] = "Kendi hesabınızı silemezsiniz.";
            }

            return RedirectToAction("AllUsers");
        }

        // Kullanıcı düzenleme sayfası
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

        // Kullanıcı düzenleme işlemi
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
                ViewBag.Hata = "Boş alan bırakmayınız.";
                return View(user);
            }

            // Aynı kullanıcı adı var mı kontrol
            if (_context.Users.Any(u => u.Username == kullaniciAdi && u.Id != user.Id))
            {
                ViewBag.Hata = "Bu kullanıcı adı başka biri tarafından kullanılıyor.";
                return View(user);
            }

            try
            {
                user.Username = kullaniciAdi;
                user.Password = SecurityHelper.Sha256Hash(sifre);
                _context.SaveChanges();

                TempData["Basarili"] = "Kullanıcı başarıyla güncellendi.";
                return RedirectToAction("AllUsers");
            }
            catch (Exception ex)
            {
                ViewBag.Hata = "Kullanıcı güncellenirken bir hata oluştu.";
                return View(user);
            }
        }

        // Çıkış yapma
        public IActionResult Logout()
        {
            HttpContext.Session.Clear();
            return RedirectToAction("Login", "Account");
        }

        // Excel Export
        public IActionResult ExportToExcel()
        {
            if (HttpContext.Session.GetString("Username") == null)
                return RedirectToAction("Login", "Account");

            try
            {
                // ID'ye göre sıralama yaparak tutarlılığı sağla
                var users = _context.Users.OrderBy(u => u.Id).ToList();

                using (var workbook = new XLWorkbook())
                {
                    var worksheet = workbook.Worksheets.Add("Kullanicilar");

                    // Başlıklar
                    worksheet.Cell(1, 1).Value = "ID";
                    worksheet.Cell(1, 2).Value = "Kullanıcı Adı";
                    worksheet.Cell(1, 3).Value = "Kayıt Tarihi";

                    // Başlık satırını kalın yap
                    worksheet.Range("A1:C1").Style.Font.Bold = true;
                    worksheet.Range("A1:C1").Style.Fill.BackgroundColor = XLColor.LightGray;

                    // Veriler
                    for (int i = 0; i < users.Count; i++)
                    {
                        worksheet.Cell(i + 2, 1).Value = users[i].Id;
                        worksheet.Cell(i + 2, 2).Value = users[i].Username;
                        worksheet.Cell(i + 2, 3).Value = users[i].CreatedDate.ToString("dd.MM.yyyy HH:mm");
                    }

                    // Otomatik sütun genişliği
                    worksheet.Columns().AdjustToContents();

                    // Tablo stilini uygula
                    var tableRange = worksheet.Range($"A1:C{users.Count + 1}");
                    tableRange.Style.Border.OutsideBorder = XLBorderStyleValues.Thin;
                    tableRange.Style.Border.InsideBorder = XLBorderStyleValues.Thin;

                    using (var stream = new MemoryStream())
                    {
                        workbook.SaveAs(stream);
                        var content = stream.ToArray();
                        var fileName = $"Kullanicilar_{DateTime.Now:yyyyMMdd_HHmmss}.xlsx";

                        return File(content, "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet", fileName);
                    }
                }
            }
            catch (Exception ex)
            {
                TempData["Hata"] = "Excel dosyası oluşturulurken hata: " + ex.Message;
                return RedirectToAction("AllUsers");
            }
        }

        // PDF Export
        public IActionResult ExportToPdf()
        {
            if (HttpContext.Session.GetString("Username") == null)
                return RedirectToAction("Login", "Account");

            try
            {
                // ID'ye göre sıralama yaparak tutarlılığı sağla
                var users = _context.Users.OrderBy(u => u.Id).ToList();

                using (var stream = new MemoryStream())
                {
                    // PDF dokümanı oluştur
                    var document = new iTextSharp.text.Document(iTextSharp.text.PageSize.A4, 25, 25, 30, 30);
                    var writer = PdfWriter.GetInstance(document, stream);

                    document.Open();

                    // Türkçe karakter desteği için font
                    string fontPath = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.Fonts), "arial.ttf");
                    BaseFont baseFont;

                    try
                    {
                        baseFont = BaseFont.CreateFont(fontPath, BaseFont.IDENTITY_H, BaseFont.EMBEDDED);
                    }
                    catch
                    {
                        // Arial bulunamazsa varsayılan font kullan
                        baseFont = BaseFont.CreateFont(BaseFont.HELVETICA, BaseFont.CP1252, BaseFont.NOT_EMBEDDED);
                    }

                    var titleFont = new iTextSharp.text.Font(baseFont, 18, iTextSharp.text.Font.BOLD);
                    var headerFont = new iTextSharp.text.Font(baseFont, 12, iTextSharp.text.Font.BOLD);
                    var normalFont = new iTextSharp.text.Font(baseFont, 10, iTextSharp.text.Font.NORMAL);

                    // Başlık
                    var title = new iTextSharp.text.Paragraph("KULLANICI LİSTESİ", titleFont);
                    title.Alignment = Element.ALIGN_CENTER;
                    title.SpacingAfter = 20f;
                    document.Add(title);

                    // Oluşturulma tarihi
                    var dateInfo = new iTextSharp.text.Paragraph($"Oluşturulma Tarihi: {DateTime.Now:dd.MM.yyyy HH:mm}", normalFont);
                    dateInfo.Alignment = Element.ALIGN_RIGHT;
                    dateInfo.SpacingAfter = 20f;
                    document.Add(dateInfo);

                    // Tablo oluştur
                    var table = new PdfPTable(3);
                    table.WidthPercentage = 100;
                    table.SetWidths(new float[] { 1f, 3f, 2f }); // Sütun genişlikleri

                    // Tablo başlıkları
                    var idHeader = new PdfPCell(new Phrase("ID", headerFont));
                    idHeader.BackgroundColor = BaseColor.LIGHT_GRAY;
                    idHeader.HorizontalAlignment = Element.ALIGN_CENTER;
                    idHeader.Padding = 8f;
                    table.AddCell(idHeader);

                    var usernameHeader = new PdfPCell(new Phrase("Kullanıcı Adı", headerFont));
                    usernameHeader.BackgroundColor = BaseColor.LIGHT_GRAY;
                    usernameHeader.HorizontalAlignment = Element.ALIGN_CENTER;
                    usernameHeader.Padding = 8f;
                    table.AddCell(usernameHeader);

                    var dateHeader = new PdfPCell(new Phrase("Kayıt Tarihi", headerFont));
                    dateHeader.BackgroundColor = BaseColor.LIGHT_GRAY;
                    dateHeader.HorizontalAlignment = Element.ALIGN_CENTER;
                    dateHeader.Padding = 8f;
                    table.AddCell(dateHeader);

                    // Veriler
                    foreach (var user in users)
                    {
                        var idCell = new PdfPCell(new Phrase(user.Id.ToString(), normalFont));
                        idCell.HorizontalAlignment = Element.ALIGN_CENTER;
                        idCell.Padding = 5f;
                        table.AddCell(idCell);

                        var usernameCell = new PdfPCell(new Phrase(user.Username, normalFont));
                        usernameCell.HorizontalAlignment = Element.ALIGN_LEFT;
                        usernameCell.Padding = 5f;
                        table.AddCell(usernameCell);

                        var dateCell = new PdfPCell(new Phrase(user.CreatedDate.ToString("dd.MM.yyyy HH:mm"), normalFont));
                        dateCell.HorizontalAlignment = Element.ALIGN_CENTER;
                        dateCell.Padding = 5f;
                        table.AddCell(dateCell);
                    }

                    document.Add(table);

                    // Özet bilgi
                    var summary = new iTextSharp.text.Paragraph($"\nToplam Kullanıcı Sayısı: {users.Count}", headerFont);
                    summary.SpacingBefore = 20f;
                    document.Add(summary);

                    document.Close();

                    var content = stream.ToArray();
                    var fileName = $"Kullanicilar_{DateTime.Now:yyyyMMdd_HHmmss}.pdf";

                    return File(content, "application/pdf", fileName);
                }
            }
            catch (Exception ex)
            {
                TempData["Hata"] = "PDF dosyası oluşturulurken hata: " + ex.Message;
                return RedirectToAction("AllUsers");
            }
        }

    }
}