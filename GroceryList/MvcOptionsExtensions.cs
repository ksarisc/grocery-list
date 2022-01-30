using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Routing;
using Microsoft.AspNetCore.Mvc.ApplicationModels;
using System.Linq;
using System.Threading.Tasks;

namespace GroceryList
{
    //public static class MvcOptionsExtensions{
    //    public static void UseGeneralRoutePrefix(this MvcOptions opts, IRouteTemplateProvider routeAttribute){
    //        opts.Conventions.Add(new RoutePrefixConvention(routeAttribute));
    //    }

    //    public static void UseGeneralRoutePrefix(this MvcOptions opts, string prefix){
    //        opts.UseGeneralRoutePrefix(new RouteAttribute(prefix));
    //    }
    //}

    //public class RoutePrefixConvention : IApplicationModelConvention{
    //    private readonly AttributeRouteModel _routePrefix;

    //    public RoutePrefixConvention(IRouteTemplateProvider route){
    //        _routePrefix = new AttributeRouteModel(route);
    //    }

    //    public void Apply(ApplicationModel application){
    //        foreach (var selector in application.Controllers.SelectMany(c => c.Selectors)){
    //            selector.AttributeRouteModel = selector.AttributeRouteModel != null ?
    //                AttributeRouteModel.CombineAttributeRouteModel(_routePrefix, selector.AttributeRouteModel) :
    //                selector.AttributeRouteModel = _routePrefix;
    //        }
    //    }
    //}
}
