
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using RE_Waste.Data;
using RE_Waste.Models;

namespace RE_Waste.Areas.Manager.Controllers
{
    [Area("Manager")]
    [Authorize(Roles = AppRoles.Manager)]
    public class ManagerController : Controller
    {
        public IActionResult Index()
        {
            return View();
        }
    }
}
