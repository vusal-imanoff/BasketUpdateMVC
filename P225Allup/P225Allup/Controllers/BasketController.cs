using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Newtonsoft.Json;
using P225Allup.DAL;
using P225Allup.Models;
using P225Allup.ViewModels.Basket;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace P225Allup.Controllers
{
    public class BasketController : Controller
    {
        private readonly AppDbContext _context;

        public BasketController(AppDbContext context)
        {
            _context = context;
        }
        public IActionResult Index()
        {
            return View();
        }
        public async Task<IActionResult> AddToBasket(int? id)
        {
            if (id == null)
            {
                return BadRequest();
            }
            Product product = await _context.Products.FirstOrDefaultAsync(p => p.Id == id);

            if (product == null)
            {
                return NotFound();
            }

            List<BasketVM> basketVMs = null;

            string cookie = HttpContext.Request.Cookies["basket"];

            if (!string.IsNullOrEmpty(cookie))
            {
                basketVMs = JsonConvert.DeserializeObject<List<BasketVM>>(cookie);
                if (basketVMs.Exists(p => p.Id == id))
                {
                    basketVMs.Find(p => p.Id == id).Count++;
                }
                else
                {
                   
                    BasketVM basketVM = new BasketVM
                    {
                        Id = (int)id,
                        Name = product.Title,
                        Price = product.DiscountPrice>0 ? product.DiscountPrice : product.Price,
                        Image=product.MainImage,
                        Count = 1
                    };
                    basketVMs.Add(basketVM);
                }
            }
            else
            {
                basketVMs = new List<BasketVM>();
                BasketVM basketVM = new BasketVM
                {
                    Id = (int)id,
                    Name=product.Title,
                    Price = product.DiscountPrice > 0 ? product.DiscountPrice : product.Price,
                    Image=product.MainImage,
                    Count=1
                };
                basketVMs.Add(basketVM);
            }
            string item = JsonConvert.SerializeObject(basketVMs);
            HttpContext.Response.Cookies.Append("basket", item);

            return RedirectToAction("index", "home");
        }
        public IActionResult GetBasket()
        {
            string cookie = HttpContext.Request.Cookies["basket"];
            List<BasketVM> basketVMs = JsonConvert.DeserializeObject<List<BasketVM>>(cookie);
            return Json(basketVMs);
        }

        public IActionResult RemoveItem( int? id)
        {
            string cookie = HttpContext.Request.Cookies["basket"];
            List<BasketVM> basketVMs = JsonConvert.DeserializeObject<List<BasketVM>>(cookie);
            basketVMs.FindAll(x => x.Id != id);

            string items = JsonConvert.SerializeObject(basketVMs.FindAll(x => x.Id != id));
            HttpContext.Response.Cookies.Append("basket", items);

            return RedirectToAction("index", "home");
        }

    }
}
