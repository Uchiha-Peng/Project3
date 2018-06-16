﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;
using Project3.Data.Service.Interface;
using Project3.Web.Controllers.Attributes;
using Project3.Web.Filters;

namespace Project3.Web.Controllers
{
    [DataError]
    [ActionErrorFilter]
    public class BaseController : Controller
    {
        public bool IsError { get; set; } = false;
        public string ErrorTitle { get; set; } = "遇到了一个错误";
        public string ErrorContent { get; set; } = "服务器内部错误，请返回。";
       
    }
}