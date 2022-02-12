using EPiServer.Core;
using EPiServer.Web.Mvc;
using EPiServer.Web.Routing;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Infrastructure;
using Microsoft.AspNetCore.Routing;
using Microsoft.Extensions.DependencyInjection;
using System;
using System.Threading.Tasks;

namespace Foundation.Infrastructure.Helpers
{
    public class FoundationRedirectToContentResult : RedirectToContentResult
    {
        public RouteValueDictionary RouteValues { get; set; }

        public FoundationRedirectToContentResult(ContentReference contentLink, string actionName, string language, object routeValues) : base(
            contentLink, actionName, language)
        {
            RouteValues = ((routeValues == null) ? null : new RouteValueDictionary(routeValues));
        }

        public override Task ExecuteResultAsync(ActionContext context)
        {
            context = context ?? throw new ArgumentNullException("context");
            return context.HttpContext.RequestServices.GetRequiredService<IActionResultExecutor<FoundationRedirectToContentResult>>().ExecuteAsync(context, this);
        }
    }

    public class FoundationRedirectToContentResultExecutor : IActionResultExecutor<FoundationRedirectToContentResult>
    {
        private readonly UrlResolver _urlResolver;

        private readonly IActionResultExecutor<RedirectResult> _actionResultExecutor;

        public FoundationRedirectToContentResultExecutor(UrlResolver urlResolver, IActionResultExecutor<RedirectResult> actionResultExecutor)
        {
            _urlResolver = urlResolver;
            _actionResultExecutor = actionResultExecutor;
        }

        public Task ExecuteAsync(ActionContext context, FoundationRedirectToContentResult result)
        {
            context = context ?? throw new ArgumentNullException("context");
            string url = _urlResolver.GetUrl(result.ContentLink, result.Language, new VirtualPathArguments
            {
                Action = (result.ActionName ?? "Index")
            });
            if (result.RouteValues.Count > 0)
            {
                url += "?";
                foreach (var routeValue in result.RouteValues)
                {
                    url += string.Format("{0}={1}", routeValue.Key.ToString(), routeValue.Value.ToString());
                }
            }

            return _actionResultExecutor.ExecuteAsync(context, new RedirectResult(url));
        }
    }
}

