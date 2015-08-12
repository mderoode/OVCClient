
using Microsoft.WindowsAzure.MobileServices;
using OVCClient.DataObjects;
using OVCClient.Pages;
using System;
using System.Threading.Tasks;
using Windows.Networking.Sockets;
using Windows.Storage.Streams;
using Windows.UI.Popups;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Navigation;
using Windows.Media.SpeechRecognition;

// The Blank Page item template is documented at http://go.microsoft.com/fwlink/?LinkId=402352&clcid=0x409

namespace OVCClient
{
    /// <summary>
    /// An empty page that can be used on its own or navigated to within a Frame.
    /// </summary>
    public sealed partial class MainPage : Page
    {
        private SpeechRecognizer speechRecognizer;

        public MainPage()
        {
            this.InitializeComponent();
            MainFrame.Navigate(typeof(SendStatus));

            this.SizeChanged += ResponsivePage_SizeChanged;
        }

        private void ResponsivePage_SizeChanged(object sender, SizeChangedEventArgs e)
        {
            if (e.NewSize.Width > 970)
            {
                VisualStateManager.GoToState(this, "Expanded", true);
            }
            else if (e.NewSize.Width > 650)
            {
                VisualStateManager.GoToState(this, "Compact", true);
            }
            //else
            //{
            //    VisualStateManager.GoToState(this, "UltraCompact", true);
            //}
        }

        private void MessagesOverview_Click(object sender, RoutedEventArgs e)
        {
            MainFrame.Navigate(typeof(MessagesOverview));
        }

        private void SendStatus_Click(object sender, RoutedEventArgs e)
        {
            MainFrame.Navigate(typeof(SendStatus));
        }

        private void HamburgerButton_Click(object sender, RoutedEventArgs e)
        {
            MySplitView.IsPaneOpen = !MySplitView.IsPaneOpen;
        }

        private void HomeStationButton_Click(object sender, RoutedEventArgs e)
        {
            MainFrame.Navigate(typeof(HomeStation));
        }

        private void I2CButton_Click(object sender, RoutedEventArgs e)
        {
            MainFrame.Navigate(typeof(I2CTest));
        }

        private async void SpeechButton_Click(object sender, RoutedEventArgs e)
        {
            // Create an instance of SpeechRecognizer.
            this.speechRecognizer = new Windows.Media.SpeechRecognition.SpeechRecognizer();

            // You could create this array dynamically.
            string[] responses = { "Start", "Stop", "Go left", "Go right" };

            // Add a list constraint to the recognizer.
            var listConstraint = new Windows.Media.SpeechRecognition.SpeechRecognitionListConstraint(responses, "yesOrNo");

            speechRecognizer.UIOptions.ExampleText = @"Ex. 'Yes', 'No'";
            speechRecognizer.Constraints.Add(listConstraint);

            // Compile the constraint.
            await speechRecognizer.CompileConstraintsAsync();

            // Start recognition.
            Windows.Media.SpeechRecognition.SpeechRecognitionResult speechRecognitionResult = await this.speechRecognizer.RecognizeWithUIAsync();

            // Do something with the recognition result.
            var messageDialog = new Windows.UI.Popups.MessageDialog(speechRecognitionResult.Text, "Text spoken");
            await messageDialog.ShowAsync();
        }

    }
}