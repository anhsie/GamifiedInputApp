using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Controls.Primitives;
using Microsoft.UI.Xaml.Data;
using Microsoft.UI.Xaml.Input;
using Microsoft.UI.Xaml.Media;
using Microsoft.UI.Xaml.Navigation;
using System;
using System.Reflection;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices.WindowsRuntime;
using Windows.Foundation;
using Windows.Foundation.Collections;
using Microsoft.UI.Composition;
using Microsoft.UI.Xaml.Hosting;
//using Microsoft.Windows.Sdk;

// To learn more about WinUI, the WinUI project structure,
// and more about our project templates, see: http://aka.ms/winui-project-info.

using GamifiedInputApp.Minigames;
using System.Diagnostics;

namespace GamifiedInputApp
{
    /// <summary>
    /// An empty window that can be used on its own or navigated to within a Frame.
    /// </summary>
    public sealed partial class MainWindow : Window
    {
        GameCore gameCore;
        ContainerVisual rootVisual;
        private ObservableCollection<MinigameItem> DataSource;

        public MainWindow()
        {
            this.InitializeComponent();
            Results.Visibility = Visibility.Collapsed;
            PopulateMinigames();

            rootVisual = Compositor.CreateContainerVisual();
            ElementCompositionPreview.SetElementChildVisual(Root, rootVisual);
            gameCore = new GameCore(rootVisual);
            gameCore.Results += GameCore_GoToResults;

            //var hwnd = new HWND(this.GetWindowHandle());
            //PInvoke.ShowWindow(hwnd, (SHOW_WINDOW_CMD)3);
            //PInvoke.CreateWindowEx(WINDOWS_EX_STYLE.WS_EX_APPWINDOW, );
        }

        private void PopulateMinigames()
        {
            const string baseNamespace = "GamifiedInputApp.Minigames";
            string[] basePath = baseNamespace.Split('.');

            DataSource = new ObservableCollection<MinigameItem>();

            // get minigame types (in the baseNamespace and implementing IMinigame)
            IEnumerable<Type> minigameTypes = Assembly.GetExecutingAssembly().GetTypes().Where(type =>
                (type.Namespace?.StartsWith(baseNamespace)).GetValueOrDefault() &&
                (type.GetInterface(typeof(IMinigame).Name) != null));

            // populate the TreeView with each minigame
            foreach (Type minigameType in minigameTypes)
            {
                IMinigame minigame = (IMinigame)Activator.CreateInstance(minigameType);

                // get the content labels based on the namespace, skipping the baseNamespace
                // e.g. "GamifiedInputApp.Minigames.Gesture.Holding" would be \Gesture\Holding in the treeview
                IEnumerable<string> contentLabels = minigameType.Namespace.Split('.').Skip(basePath.Length);
                ObservableCollection<MinigameItem> currentNode = DataSource;
                foreach (object contentLabel in contentLabels)
                {
                    try
                    {
                        // find child with this label
                        currentNode = currentNode.First(node => contentLabel.Equals(node.Content)).Children;
                    }
                    catch (InvalidOperationException)
                    {
                        // no child with this label, add one
                        MinigameItem newItem = new MinigameItem() { Content = contentLabel };
                        currentNode.Add(newItem); currentNode = newItem.Children;
                    }
                }

                // add minigame node
                currentNode.Add(new MinigameItem() { Content = minigame.Info });
            }
        }

        private void GameCore_GoToResults(object sender, ResultsEventArgs e)
        {
            ScoreText.Text = e.Score.ToString();
            Results.Visibility = Visibility.Visible;
        }

        private void startButton_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                // run selected minigames
                gameCore.Run(MinigamePicker.SelectedNodes
                    .Where(node => ((node.Content as MinigameItem)?.IsMinigame).GetValueOrDefault())
                    .Select(node => (node.Content as MinigameItem).Info));
                Menu.Visibility = Visibility.Collapsed;
            }
            catch (InvalidOperationException ex)
            {
                // no minigames selected
                Console.WriteLine(ex.ToString());
            }
        }

        private void GoToMenuButton_Click(object sender, RoutedEventArgs e)
        {
            Results.Visibility = Visibility.Collapsed;
            Menu.Visibility = Visibility.Visible;
        }
    }

    public class MinigameItem
    {
        public object Content { get; set; }
        public bool IsMinigame { get { return Content is MinigameInfo; } }
        public MinigameInfo Info { get { return Content as MinigameInfo; } }
        public ObservableCollection<MinigameItem> Children { get; set; } = new ObservableCollection<MinigameItem>();

        public Visibility MouseVisibility { get { return GetDeviceVisibility(SupportedDeviceTypes.Mouse); } }
        public Visibility TouchVisibility { get { return GetDeviceVisibility(SupportedDeviceTypes.Touch); } }
        public Visibility PenVisibility { get { return GetDeviceVisibility(SupportedDeviceTypes.Pen); } }
        public Visibility KeyVisibility { get { return GetDeviceVisibility(SupportedDeviceTypes.Keyboard); } }

        private Visibility GetDeviceVisibility(SupportedDeviceTypes deviceType)
        {
            return Info.Devices.HasFlag(deviceType) ? Visibility.Visible : Visibility.Collapsed;
        }
    }

    class MinigameTemplateSelector : DataTemplateSelector
    {
        public DataTemplate CategoryTemplate { get; set; }
        public DataTemplate MinigameTemplate { get; set; }

        protected override DataTemplate SelectTemplateCore(object item)
        {
            MinigameItem minigameItem = (MinigameItem)item;
            return (minigameItem.Content is MinigameInfo) ? MinigameTemplate : CategoryTemplate;
        }
    }
}
