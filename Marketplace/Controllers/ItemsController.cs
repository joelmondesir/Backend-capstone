﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using Marketplace.Data;
using Marketplace.Models;
using Microsoft.AspNetCore.Identity;
using Marketplace.Models.ItemViewModels;

namespace Marketplace.Controllers
{
    public class ItemsController : Controller
    {
        private readonly ApplicationDbContext _context;
        private readonly UserManager<User> _userManager;

        public ItemsController(ApplicationDbContext context, UserManager<User> userManager)
        {
            _context = context;
            _userManager = userManager;
        }
        private Task<User> GetCurrentUserAsync() => _userManager.GetUserAsync(HttpContext.User);
        // GET: Items by search
        public async Task<IActionResult> Index(string searchString)
        {
            ViewData["CurrentFilter"] = searchString;

            var applicationDbContext = _context.Item.Include(i => i.Category).Include(i => i.Seller);

            //If user enters a string into the search input field in the navbar - adding a where clause to include products whose name contains string.
            if (!String.IsNullOrEmpty(searchString))
            {
                applicationDbContext = _context.Item.Where(i => i.Title.Contains(searchString)).Include(i => i.Category).Include(i => i.Seller);
            }

            return View(await applicationDbContext.ToListAsync());
        }

        public async Task<IActionResult> MyItems()
        {
            var currentUser = await GetCurrentUserAsync();
            var applicationDbContext = _context.Item.Where(i => i.SellerId == currentUser.Id)
                                                        .Include(i => i.Category)
                                                        .Include(i => i.Seller);

            return View(await applicationDbContext.ToListAsync());
        }

        // GET: Items/BuyerDetails/5
        public async Task<IActionResult> BuyerDetails(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var item = await _context.Item
                .Include(i => i.Category)
                .Include(i => i.Seller)
                .Include(i => i.Status)
                .FirstOrDefaultAsync(m => m.ItemId == id);
            if (item == null)
            {
                return NotFound();
            }

            return View(item);
        }

        public async Task<IActionResult> Details(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var item = await _context.Item
                .Include(i => i.Category)
                .Include(i => i.Seller)
                .Include(i => i.Status)
                .FirstOrDefaultAsync(m => m.ItemId == id);
            if (item == null)
            {
                return NotFound();
            }

            return View(item);
        }

        // GET: Items/Create
        public IActionResult Create()
        {
            var viewModel = new ItemCreateEditViewModel
            {
                AvailableCategory = _context.Category.ToList(),
                AvailableStatus = _context.Status.ToList()
            };
            List<Category> avCat = getCategories();
            List<Status> avStat = getStatus();
            viewModel.AvailableCategory = avCat;
            viewModel.AvailableStatus = avStat;
            return View(viewModel);
        }

        // POST: Items/Create
        // To protect from overposting attacks, please enable the specific properties you want to bind to, for 
        // more details see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(ItemCreateEditViewModel  viewModel)
        {
            var Item = viewModel.Item;

            if (ModelState.IsValid)
            {
                var currentUser = await _userManager.GetUserAsync(HttpContext.User);
                Item.SellerId = currentUser.Id;
                _context.Add(Item);
                await _context.SaveChangesAsync();
                return RedirectToAction(nameof(MyItems));
            }

            viewModel.AvailableCategory = await _context.Category.ToListAsync();
            return View(viewModel);
        }

        // GET: Items/Edit/5
        public async Task<IActionResult> Edit(int? id)
        {
            ItemCreateEditViewModel viewModel = new ItemCreateEditViewModel();
            viewModel.AvailableCategory = _context.Category.ToList();
            viewModel.AvailableStatus = _context.Status.ToList();
            if (id == null)
            {
                return NotFound();
            }

            Item taco = await _context.Item.FindAsync(id);
            viewModel.Item = taco;
            if (taco == null)
            {
                return NotFound();
            }
            return View(viewModel);
        }

        // POST: Items/Edit/5
        // To protect from overposting attacks, please enable the specific properties you want to bind to, for 
        // more details see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, Item Item)
        {
            var currentUser = await _userManager.GetUserAsync(HttpContext.User);
            Item.SellerId = currentUser.Id;
            if (id != Item.ItemId)
            {
                return NotFound();
            }

            if (ModelState.IsValid)
            {
                try
                {
                    _context.Update(Item);
                    await _context.SaveChangesAsync();
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!ItemExists(Item.ItemId))
                    {
                        return NotFound();
                    }
                    else
                    {
                        throw;
                    }
                }
                return RedirectToAction(nameof(MyItems));
            }

            return View(Item);
        }

        // GET: Items/Delete/5
        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var item = await _context.Item
                .Include(i => i.Category)
                .Include(i => i.Seller)
                .Include(i => i.Status)
                .FirstOrDefaultAsync(m => m.ItemId == id);
            if (item == null)
            {
                return NotFound();
            }

            return View(item);
        }

        // POST: Items/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var item = await _context.Item.FindAsync(id);
            _context.Item.Remove(item);
            await _context.SaveChangesAsync();
            return RedirectToAction(nameof(MyItems));
        }

        private bool ItemExists(int id)
        {
            return _context.Item.Any(e => e.ItemId == id);
        }

        // Method used to get all categories from DB
        private List<Category> getCategories()
        {
            var categories = _context.Category.ToList();
    
            return categories;
        }

        // Method for getting the status' from DB
        private List<Status> getStatus()
        {
            var statusList = _context.Status.ToList();

            return statusList;
        }
    }
}
