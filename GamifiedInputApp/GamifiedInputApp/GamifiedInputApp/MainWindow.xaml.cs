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

namespace GamifiedInputApp
{
    /// <summary>
    /// An empty window that can be used on its own or navigated to within a Frame.
    /// </summary>
    public sealed partial class MainWindow : Window
    {
        public IntPtr? Handle { get; private set; } = null;
        public Visual RootVisual { get; private set; } = null;

        private GameCore GameCore;
        private ObservableCollection<MinigameItem> TreeSource;
        private ObservableCollection<ScoreItem> ScoreSource;
        private IList<object> MinigameItems;
        private Panel[] screens;

        public MainWindow()
        {
            this.InitializeComponent();

            this.screens = new Panel[] { MenuScreen, MinigameScreen, ResultsScreen };
            this.SetScreen(MenuScreen);
            MinigameScreen.LayoutUpdated += MinigameScreen_LayoutUpdated;

            TreeSource = new ObservableCollection<MinigameItem>();
            ScoreSource = new ObservableCollection<ScoreItem>();

            PopulateMinigames();
            RootVisual = MinigamePanel.GetVisualInternal();

            GameCore = new GameCore(this);
            GameCore.Results += GameCore_GoToResults;
        }

        public Rect GameBounds
        {
            get
            {
                GeneralTransform gt = MinigamePanel.TransformToVisual(Root);
                Point offset = gt.TransformPoint(new Point(0.0, 0.0));
                Point size = new Point(MinigamePanel.ActualSize.X, MinigamePanel.ActualSize.Y);
                return new Rect(offset.X, offset.Y, size.X, size.Y);
            }
        }

        private void PopulateMinigames()
        {
            const string baseNamespace = "GamifiedInputApp.Minigames";
            string[] basePath = baseNamespace.Split('.');

            MouseInputPicker.DataContext = SupportedDeviceTypes.Mouse;
            TouchInputPicker.DataContext = SupportedDeviceTypes.Touch;
            PenInputPicker.DataContext = SupportedDeviceTypes.Pen;
            KeyInputPicker.DataContext = SupportedDeviceTypes.Keyboard;

            // Create a root "Select All" node
            MinigameItem rootNode = new MinigameItem() { Content = "Select All" };
            TreeSource.Add(rootNode);

            // get minigame types (in the baseNamespace and implementing IMinigame)
            IEnumerable<Type> minigameTypes = Assembly.GetExecutingAssembly().GetTypes().Where(type =>
                (type.Namespace?.StartsWith(baseNamespace)).GetValueOrDefault() &&
                (type.GetInterface(typeof(IMinigame).Name) != null));
            MinigameItems = new List<object>(minigameTypes.Count());

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
                MinigameItems.Add(currentNode);
            }
        }

        private Panel SetScreen(Panel target)
        {
            Panel previous = null;
            foreach (Panel screen in this.screens)
            {
                if (screen.Visibility == Visibility.Visible) { previous = screen; }
                screen.Visibility = (screen == target) ? Visibility.Visible : Visibility.Collapsed;
            }
            return previous;
        }

        private void StartGame()
        {
            try
            {
                GameCore.Run(MinigamePicker.SelectedItems
                    .Where(item => (item as MinigameItem).IsMinigame)
                    .Select(item => (item as MinigameItem).Info));
            }
            catch (InvalidOperationException ex)
            {
                // no minigames selected
                Console.WriteLine(ex.ToString());
            }
        }

        private void Window_Activated(object sender, object args) => Handle = PInvoke.User32.GetActiveWindow();

        private void MinigameScreen_LayoutUpdated(object sender, object e)
        {
            if (MinigameScreen.Visibility == Visibility.Visible && !GameCore.IsRunning)
            {
                // try to run the game
                DispatcherQueue.TryEnqueue(StartGame);
            }
        }

        private void GameCore_GoToResults(object sender, ResultsEventArgs e)
        {
            TimeRemaining.Text = e.TimeLeft;
            if (ScoreSource.Count() == 0)
            {
                foreach (ScoreItem item in e.Results) { ScoreSource.Add(item); }
            }

            if (e.GoToResults)
            {
                SetScreen(ResultsScreen);
            }
        }

        private void startButton_Click(object sender, RoutedEventArgs e)
        {
            SetScreen(MinigameScreen); // game runs in MinigameScreen_LayoutUpdated
        }

        private void GoToMenuButton_Click(object sender, RoutedEventArgs e)
        {
            SetScreen(MenuScreen);
        }

        private void InputPicker_Checked(object sender, RoutedEventArgs e)
        {
            SupportedDeviceTypes device = (SupportedDeviceTypes)(sender as CheckBox).DataContext;
            IEnumerable<object> selected = MinigameItem.WhereDevice(MinigamePicker.SelectedItems, device);
            IEnumerable<object> unselected = MinigameItem.WhereDevice(MinigameItems, device).Except(selected);

            if (unselected.Count() > 0)
            {
                foreach (object item in unselected) { MinigamePicker.SelectedItems.Add(item); }
                MinigamePicker_SelectionChanged(MinigamePicker, null);
            }
        }

        private void InputPicker_Unchecked(object sender, RoutedEventArgs e)
        {
            SupportedDeviceTypes device = (SupportedDeviceTypes)(sender as CheckBox).DataContext;
            IEnumerable<object> selected = MinigameItem.WhereDevice(MinigamePicker.SelectedItems, device);

            if (selected.Count() > 0)
            {
                selected = new List<object>(selected); // clone selected out of MinigamePicker.SelectedItems
                foreach (object item in selected) { MinigamePicker.SelectedItems.Remove(item); }
                MinigamePicker_SelectionChanged(MinigamePicker, null);
            }
        }

        private void InputPicker_Indeterminate(object sender, RoutedEventArgs e)
        {
            SupportedDeviceTypes device = (SupportedDeviceTypes)(sender as CheckBox).DataContext;
            // If all options are selected, clicking the box will change it to its indeterminate state.
            // Instead, we want to uncheck all the boxes, so we do this programatically.
            if (MinigameItem.CountDevice(MinigameItems, device) == MinigameItem.CountDevice(MinigamePicker.SelectedItems, device))
            {
                (sender as CheckBox).IsChecked = false;
                // This will cause InputPicker_Unchecked to be executed, which will trigger SelectionChanged.
            }
        }

        private void MinigamePicker_SelectionChanged(TreeView sender, object args)
        {
            foreach (CheckBox child in InputDevicePickers.Children.Where(child => child is CheckBox))
            {
                SupportedDeviceTypes device = (SupportedDeviceTypes)child.DataContext;
                int selected = MinigameItem.CountDevice(MinigamePicker.SelectedItems, device);
                int unselected = MinigameItem.CountDevice(MinigameItems, device) - selected;

                child.IsChecked = (selected == 0) ? false : (unselected == 0) ? true : null;  // null for indeterminate
            }
        }
    }

    public class ScoreItem
    {
        public string Title { get; set; }
        public string Value { get; set; }
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

        public bool IsDevice(SupportedDeviceTypes deviceType) => IsMinigame && Info.Devices.HasFlag(deviceType);

        private Visibility GetDeviceVisibility(SupportedDeviceTypes deviceType) =>
            IsDevice(deviceType) ? Visibility.Visible : Visibility.Collapsed;

        public static IEnumerable<object> WhereDevice(IEnumerable<object> list, SupportedDeviceTypes deviceType) =>
            list.Where(item => ((item as MinigameItem)?.IsDevice(deviceType)).GetValueOrDefault());
        public static int CountDevice(IEnumerable<object> list, SupportedDeviceTypes deviceType) =>
            list.Count(item => ((item as MinigameItem)?.IsDevice(deviceType)).GetValueOrDefault());
    }

    class MinigameTemplateSelector : DataTemplateSelector
    {
        public DataTemplate CategoryTemplate { get; set; }
        public DataTemplate MinigameTemplate { get; set; }

        protected override DataTemplate SelectTemplateCore(object item)
        {
            MinigameItem minigameItem = (MinigameItem)item;
            return (minigameItem.IsMinigame) ? MinigameTemplate : CategoryTemplate;
        }
    }
}
