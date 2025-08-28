using Microsoft.AspNetCore.Mvc.Filters;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using System;

namespace PTM2._0.Filters
{
    public class CustomActionFilter : ActionFilterAttribute
    {
        private readonly ILogger<CustomActionFilter> _logger;

        public CustomActionFilter(ILogger<CustomActionFilter> logger)
        {
            _logger = logger;
        }

        public override void OnActionExecuting(ActionExecutingContext context)
        {
            // 日志记录部分
            var para = context.HttpContext.Request.QueryString.Value;
            var controllerName = context.ActionDescriptor.RouteValues["controller"];
            var actionName = context.ActionDescriptor.RouteValues["action"];
            _logger.LogInformation($"执行{controllerName}控制器--{actionName}方法；参数为：{para}");
            
            // 登录验证部分
            byte[] id;
            context.HttpContext.Session.TryGetValue("UserName", out id);
            
            if (!controllerName.Equals("Login", StringComparison.OrdinalIgnoreCase) && id == null)
            {
                context.Result = new RedirectToActionResult("Index", "Login", null);
                _logger.LogWarning($"未登录用户尝试访问 {controllerName}/{actionName}，已重定向到登录页");
            }

            base.OnActionExecuting(context);
        }

        public override void OnActionExecuted(ActionExecutedContext context)
        {
            // 日志记录部分
            string resultJson = context.Result is ObjectResult objectResult 
                ? JsonConvert.SerializeObject(objectResult.Value) 
                : context.Result?.ToString();

            var controllerName = context.ActionDescriptor.RouteValues["controller"];
            var actionName = context.ActionDescriptor.RouteValues["action"];
            
            _logger.LogInformation($"执行{controllerName}控制器--{actionName}方法:执行结果为：{resultJson}");
            
            base.OnActionExecuted(context);
        }
    }
}