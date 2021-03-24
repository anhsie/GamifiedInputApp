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

            TreeViewNode rootNode = new TreeViewNode() { Content = "Minigames", IsExpanded = true };

            IEnumerable<Type> minigameTypes = Assembly.GetExecutingAssembly().GetTypes().Where(type =>
                (type.Namespace?.StartsWith(baseNamespace)).GetValueOrDefault() &&
                (type.GetInterface(typeof(IMinigame).Name) != null));

            foreach (Type minigameType in minigameTypes)
            {
                IMinigame minigame = (IMinigame)Activator.CreateInstance(minigameType);

                IEnumerable<string> contentLabels = minigameType.Namespace.Split('.').Skip(basePath.Length);
                TreeViewNode currentNode = rootNode;
                foreach (object contentLabel in contentLabels)
                {
                    try
                    {
                        currentNode = currentNode.Children.First(node => contentLabel.Equals(node.Content));
                    }
                    catch (InvalidOperationException)
                    {
                        currentNode.Children.Add(currentNode = new TreeViewNode() { Content = contentLabel, IsExpanded = true });
                    }
                }

                currentNode.Children.Add(currentNode = new TreeViewNode() { Content = minigame.Info });
            }

            MinigamePicker.RootNodes.Add(rootNode);
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
                gameCore.Run(MinigamePicker.SelectedNodes
                    .Where(node => node.Content is MinigameInfo)
                    .Select(node => (MinigameInfo)node.Content));
                Menu.Visibility = Visibility.Collapsed;
            }
            catch (ArgumentException ex)
            {
                Console.WriteLine(ex.ToString());
            }
        }

        private void GoToMenuButton_Click(object sender, RoutedEventArgs e)
        {
            Results.Visibility = Visibility.Collapsed;
            Menu.Visibility = Visibility.Visible;
        }
    }
}
