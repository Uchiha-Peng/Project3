﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Project3.Data.Service.Interface;
using Project3.Web.Attributes;
using Project3.Web.Models.DefaultVM;

namespace Project3.Web.Controllers
{
    public class DefaultController : BaseController
    {
        private readonly IContentManagerService cms;
        private readonly IOptionsCache optionsCache;
        private readonly IMetasManagerService mms;
        private readonly ICache cache;
        public DefaultController(IContentManagerService cms, IOptionsCache optionsCache, IMetasManagerService mms,ICache cache)
        {
            this.cms = cms;
            this.optionsCache = optionsCache;
            this.mms = mms;
            this.cache = cache;
        }

        [Theme]
        public async Task<IActionResult> Index(string search, int? mid, int pageindex = 1)
        {
       

            var options = await optionsCache.Get();
            var VM = new IndexViewModel();
            VM.search = search;
            VM.contentlist = await cms.GetContentsAsync(0, search, mid ?? 0, pageindex, int.Parse(options.postslistsize), 0);
            VM.mid = mid;
            if (mid > 0)
            {
                VM.meta = await mms.GetByMidAsync(mid.Value);
            }

            ViewData.Model = VM;

            return View();
        }

    }
}