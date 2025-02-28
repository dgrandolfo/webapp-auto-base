using GestApp.Client.Services.Interfaces;
using MudBlazor;

namespace GestApp.Client.Services;

public class BreadcrumbService : IBreadcrumbService
{
    private List<BreadcrumbItem> _items = [];

    public List<BreadcrumbItem> Items
    {
        get => _items;
        private set
        {
            _items = value;
            NotifyStateChanged();
        }
    }

    public event Action? OnChange;

    public void SetBreadcrumb(List<BreadcrumbItem> items)
    {
        Items = items;
    }

    public void AddBreadcrumbItem(BreadcrumbItem item)
    {
        _items.Add(item);
        NotifyStateChanged();
    }

    public void ClearBreadcrumb()
    {
        _items.Clear();
        NotifyStateChanged();
    }

    private void NotifyStateChanged() => OnChange?.Invoke();
}