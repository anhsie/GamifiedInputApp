﻿using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Controls.Primitives;
using Microsoft.UI.Xaml.Data;
using Microsoft.UI.Xaml.Input;
using Microsoft.UI.Xaml.Media;
using Microsoft.UI.Xaml.Navigation;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices.WindowsRuntime;
using Windows.Foundation;
using Windows.Foundation.Collections;
using Microsoft.UI.Composition;
using Microsoft.UI.Xaml.Hosting;

// To learn more about WinUI, the WinUI project structure,
// and more about our project templates, see: http://aka.ms/winui-project-info.

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

            rootVisual = Compositor.CreateContainerVisual();
            ElementCompositionPreview.SetElementChildVisual(Root, rootVisual);

            MinigamePicker.Items.Add("All");
            MinigamePicker.SelectedIndex = 0;

            gameCore = new GameCore(rootVisual);
        }

        private void startButton_Click(object sender, RoutedEventArgs e)
        {

        }

        private void MinigamePicker_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {

        }
    }
}
