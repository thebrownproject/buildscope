using Autodesk.Revit.UI;
using System.Windows.Controls;

namespace BuildScope
{
    public partial class ChatPanel : Page, IDockablePaneProvider
    {
        public ChatPanel()
        {
            InitializeComponent();
        }

        public void SetupDockablePane(DockablePaneProviderData data)
        {
            data.FrameworkElement = this;
            data.InitialState = new DockablePaneState
            {
                DockPosition = DockPosition.Right
            };
        }
    }
}
