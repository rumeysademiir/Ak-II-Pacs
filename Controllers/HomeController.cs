using Microsoft.AspNetCore.Mvc;

namespace AkıllıPacs.Controllers
{
    public class HomeController : Controller
    {
        // 1. GİRİŞ SAYFASI (GET)
        [HttpGet]
        public IActionResult Login()
        {
            // Zaten giriş yapılmışsa direkt Dashboard'a yönlendir
            if (HttpContext.Session.GetString("UserLoggedIn") == "true")
            {
                return RedirectToAction("Index");
            }
            return View();
        }

        // 2. GİRİŞ KONTROLÜ (POST)
        [HttpPost]
        public IActionResult Login(string username, string password)
        {
            // Örnek Kullanıcı Adı ve Şifre Kontrolü (İsteğe göre DB'ye bağlanabilir)
            if (username == "admin" && password == "123456")
            {
                // Session (Oturum) Başlat
                HttpContext.Session.SetString("UserLoggedIn", "true");
                HttpContext.Session.SetString("Username", username);

                return RedirectToAction("Index");
            }

            ViewBag.Error = "Kullanıcı adı veya şifre hatalı!";
            return View();
        }

        // 3. ANA DASHBOARD SAYFASI (Güvenlik Kontrollü)
        public IActionResult Index()
        {
            // Giriş YAPILMAMIŞSA Login sayfasına yönlendir
            if (HttpContext.Session.GetString("UserLoggedIn") != "true")
            {
                return RedirectToAction("Login");
            }

            return View();
        }

        // 4. ÇIKIŞ YAP (LOGOUT)
        public IActionResult Logout()
        {
            HttpContext.Session.Clear();
            return RedirectToAction("Login");
        }
    }
}