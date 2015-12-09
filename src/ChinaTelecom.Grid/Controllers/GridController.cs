﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.Data.Entity;
using Microsoft.AspNet.Mvc;
using Microsoft.AspNet.Authorization;
using ChinaTelecom.Grid.Models;

namespace ChinaTelecom.Grid.Controllers
{
    [Authorize]
    public class GridController : BaseController
    {
        public IActionResult Index()
        {
            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult CreateEstate(string title, double lon, double lat, string rules, string area)
        {
            var estate = new Estate
            {
                Area = area,
                Lat = lat,
                Lon = lon,
                Title = title
            };
            DB.Estates.Add(estate);
            if (!string.IsNullOrEmpty(rules))
                foreach (var x in rules.Split('\n'))
                    DB.EstateRules.Add(new EstateRule { Rule = x, EstateId = estate.Id });
            DB.SaveChanges();
            return Content("ok");
        }

        [HttpPost]
        public IActionResult ModifyPosition(Guid id, double lon, double lat)
        {
            var estate = DB.Estates.Where(x => x.Id == id).Single();
            estate.Lon = lon;
            estate.Lat = lat;
            DB.SaveChanges();
            return Content("ok");
        }

        [HttpGet]
        public IActionResult GetEstates(double left, double right, double top, double bottom)
        {
            var estates = DB.Estates.Where(x => x.Lon >= left && x.Lon <= right && x.Lat <= top && x.Lat >= bottom);
            foreach(var x in estates)
            {
                var tmp = DB.Houses
                    .Include(y => y.Building)
                    .Where(y => y.Building.EstateId == x.Id && y.HouseStatus == HouseStatus.中国电信);
                x.TotalCTUsers = tmp.Count();
                x.TotalInUsingUsers = tmp.Where(y => y.ServiceStatus == ServiceStatus.在用).Count();
                x.TotalNonCTUsers = DB.Houses
                    .Include(y => y.Building)
                    .Where(y => y.Building.EstateId == x.Id && y.HouseStatus != HouseStatus.中国电信)
                    .Count();
                if (x.TotalCTUsers == 0)
                    x.UsingRate = 0;
                else
                    x.UsingRate = (double)x.TotalInUsingUsers / (double)x.TotalCTUsers;
            }
            return Json(estates);
        }
    }
}