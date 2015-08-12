using Microsoft.WindowsAzure.MobileServices;
using OVCClient.DataObjects;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices.WindowsRuntime;
using Windows.Foundation;
using Windows.Foundation.Collections;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Controls.Primitives;
using Windows.UI.Xaml.Data;
using Windows.UI.Xaml.Input;
using Windows.UI.Xaml.Media;
using Windows.UI.Xaml.Navigation;
using Windows.Devices.Power;
using Windows.UI.Notifications;
using Windows.Data.Xml.Dom;

// The Blank Page item template is documented at http://go.microsoft.com/fwlink/?LinkId=234238

namespace OVCClient.Pages
{
    /// <summary>
    /// An empty page that can be used on its own or navigated to within a Frame.
    /// </summary>
    public sealed partial class SendStatus : Page
    {

        public SendStatus()
        {
            this.InitializeComponent();

            var aggBattery = Battery.AggregateBattery;

            // Get report
            var report = aggBattery.GetReport();

            if(report.FullChargeCapacityInMilliwattHours == null || report.RemainingCapacityInMilliwattHours == null)
            {
                BatteryPercentage.Text = "Percentage: 0";
                return;
            }

            double batteryDifference = (double)(report.FullChargeCapacityInMilliwattHours - report.RemainingCapacityInMilliwattHours);
            double percentage = (double)(100 - (100 * (batteryDifference / report.FullChargeCapacityInMilliwattHours)));

            BatteryPercentage.Text = "Percentage: " + Math.Round(percentage, 2);
        }

        private async void SendMessage_Click(object sender, RoutedEventArgs e)
        {
            UnitLog UnitLog = new UnitLog
            {
                Message = Message.Text,
                ReportedTime = DateTime.Now,
                BatteryPercentage = Double.Parse(BatteryPercentage.Text.Replace("Percentage: ", "")),
                Id = Guid.NewGuid().ToString()
            };

            await App.MobileService.GetTable<UnitLog>().InsertAsync(UnitLog);

            ToastTemplateType toastTemplate = ToastTemplateType.ToastText02;
            XmlDocument toastXml = ToastNotificationManager.GetTemplateContent(toastTemplate);
            XmlNodeList toastTextElements = toastXml.GetElementsByTagName("text");
            toastTextElements[0].AppendChild(toastXml.CreateTextNode("Status update send"));
            toastTextElements[1].AppendChild(toastXml.CreateTextNode(Message.Text));
            ToastNotification toast = new ToastNotification(toastXml);
            ToastNotificationManager.CreateToastNotifier().Show(toast);

            Message.Text = "";
            
        }



    }
}
