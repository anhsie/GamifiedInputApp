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
using Microsoft.UI.Hosting.Experimental;

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
        NativeWindowHelper nativeWindow;
        ExpDesktopWindowBridge desktopBridge;
        Compositor compositor;
        ContentHelper content;
        private ObservableCollection<MinigameItem> DataSource;
        private Dictionary<SupportedDeviceTypes, ObservableCollection<MinigameItem>> MinigamesByDevice;

        public MainWindow()
        {
            this.InitializeComponent();
            Results.Visibility = Visibility.Collapsed;
            PopulateMinigames();

            rootVisual = Compositor.CreateContainerVisual();
            ElementCompositionPreview.SetElementChildVisual(Root, rootVisual);
            gameCore = new GameCore(rootVisual);
            gameCore.Results += GameCore_GoToResults;

            //nativeWindow = new NativeWindowHelper();
            //nativeWindow.Show();

            //compositor = new Compositor();
            //desktopBridge = ExpDesktopWindowBridge.Create(compositor, nativeWindow.WindowId);

            //content = new ContentHelper(compositor);
            //desktopBridge.Connect(content.Content, content.InputSite);
        }

        private void PopulateMinigames()
        {
            const string baseNamespace = "GamifiedInputApp.Minigames";
            string[] basePath = baseNamespace.Split('.');

            MouseInputPicker.DataContext = SupportedDeviceTypes.Mouse;
            TouchInputPicker.DataContext = SupportedDeviceTypes.Touch;
            PenInputPicker.DataContext = SupportedDeviceTypes.Pen;
            KeyInputPicker.DataContext = SupportedDeviceTypes.Keyboard;

            MinigamesByDevice = new Dictionary<SupportedDeviceTypes, ObservableCollection<MinigameItem>>();
            foreach (FrameworkElement child in InputDevicePickers.Children.Where(child => child is CheckBox))
            {
                SupportedDeviceTypes device = (SupportedDeviceTypes)child.DataContext;
                MinigamesByDevice.Add(device, new ObservableCollection<MinigameItem>());
            }

            // Create a root "Select All" node
            DataSource = new ObservableCollection<MinigameItem>();
            MinigameItem rootNode = new MinigameItem() { Content = "Select All" };
            DataSource.Add(rootNode);

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
                MinigameItem currentNode = rootNode;
                foreach (object contentLabel in contentLabels)
                {
                    try
                    {
                        // find child with this label
                        currentNode = currentNode.Children.First(node => contentLabel.Equals(node.Content));
                    }
                    catch (InvalidOperationException)
                    {
                        // no child with this label, add one
                        currentNode.Children.Add(currentNode = new MinigameItem() { Content = contentLabel });
                    }
                }
                // add minigame node
                currentNode.Children.Add(currentNode = new MinigameItem() { Content = minigame.Info });

                // record it by device type
                foreach (FrameworkElement child in InputDevicePickers.Children.Where(child => child is CheckBox))
                {
                    SupportedDeviceTypes device = (SupportedDeviceTypes)child.DataContext;
                    if (minigame.Info.Devices.HasFlag(device))
                    {
                        MinigamesByDevice[device].Add(currentNode);
                    }
                }
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
        private void InputPicker_Checked(object sender, RoutedEventArgs e)
        {
            SupportedDeviceTypes device = (SupportedDeviceTypes)(sender as CheckBox).DataContext;
            foreach (MinigameItem minigame in MinigamesByDevice[device])
            {
                MinigamePicker.SelectedItems.Add(minigame);
            }
            MinigamePicker_SelectionChanged(MinigamePicker, null);
        }

        private void InputPicker_Unchecked(object sender, RoutedEventArgs e)
        {
            SupportedDeviceTypes device = (SupportedDeviceTypes)(sender as CheckBox).DataContext;
            foreach (MinigameItem minigame in MinigamesByDevice[device])
            {
                MinigamePicker.SelectedItems.Remove(minigame);
            }
            MinigamePicker_SelectionChanged(MinigamePicker, null);
        }

        private void InputPicker_Indeterminate(object sender, RoutedEventArgs e)
        {
            SupportedDeviceTypes device = (SupportedDeviceTypes)(sender as CheckBox).DataContext;
            // If all options are selected, clicking the box will change it to its indeterminate state.
            // Instead, we want to uncheck all the boxes, so we do this programatically.
            foreach (MinigameItem minigame in MinigamesByDevice[device])
            {
                if (!MinigamePicker.SelectedItems.Contains(minigame)) { return; }
            }
            // This will cause InputPicker_Unchecked to be executed, so
            // we don't need to uncheck the other boxes here.
            (sender as CheckBox).IsChecked = false;
        }

        private void MinigamePicker_SelectionChanged(TreeView sender, object args)
        {
            foreach (FrameworkElement child in InputDevicePickers.Children.Where(child => child is CheckBox))
            {
                SupportedDeviceTypes device = (SupportedDeviceTypes)child.DataContext;

                CheckBox checkBox = child as CheckBox;
                if (MinigamesByDevice[device].All(minigame => MinigamePicker.SelectedItems.Contains(minigame)))
                {
                    checkBox.IsChecked = true;
                }
                else if (MinigamesByDevice[device].Any(minigame => MinigamePicker.SelectedItems.Contains(minigame)))
                {
                    checkBox.IsChecked = null; // indeterminate
                }
                else
                {
                    checkBox.IsChecked = false;
                }
            }
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
