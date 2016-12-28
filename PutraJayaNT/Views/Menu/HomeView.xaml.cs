namespace ECERP.Views.Menu
{
    using System;
    using System.Diagnostics;
    using System.Windows;
    using System.Windows.Controls;
    using System.Windows.Media.Imaging;

    /// <summary>
    /// Interaction logic for HomeView.xaml
    /// </summary>
    public partial class HomeView
    {
        public HomeView()
        {
            InitializeComponent();
        }

        private void Image_Loaded(object sender, RoutedEventArgs e)
        {
            var imagePath = System.IO.Path.Combine(Environment.CurrentDirectory, @"Images\\PJLogo.jpg");
            var imageSource = new BitmapImage(new Uri(imagePath));
            var image = sender as Image;
            Debug.Assert(image != null, "image != null");
            image.Source = imageSource;
        }
    }
}
