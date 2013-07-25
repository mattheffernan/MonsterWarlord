using System.Web.Routing;

namespace Web.NavigationRoutes
{
    public interface INavigationRouteFilter
    {
        bool ShouldRemove(Route navigationRoutes);
    }
}