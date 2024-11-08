using System.Windows.Controls;

namespace BakalarkaWpf.Contracts.Views
{
    public interface IShellWindow
    {
        Frame GetNavigationFrame();

        void ShowWindow();

        void CloseWindow();
    }
}
