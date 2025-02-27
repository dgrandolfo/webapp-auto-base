using MudBlazor;

namespace GestApp.Client.Services.Interfaces;

public interface IBreadcrumbService
{
    List<BreadcrumbItem> Items { get; }
    event Action OnChange;
    void SetBreadcrumb(List<BreadcrumbItem> items);
    void AddBreadcrumbItem(BreadcrumbItem item);
    void ClearBreadcrumb();
}