using Microsoft.WindowsAzure.MobileServices;
using OVCClient.DataObjects;
using System.Linq;
using System.Threading.Tasks;
using Windows.UI.Popups;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Navigation;

// The Blank Page item template is documented at http://go.microsoft.com/fwlink/?LinkId=234238

namespace OVCClient.Pages
{
    /// <summary>
    /// An empty page that can be used on its own or navigated to within a Frame.
    /// </summary>
    public sealed partial class MessagesOverview : Page
    {
        private MobileServiceCollection<UnitLog, UnitLog> items;
        private IMobileServiceTable<UnitLog> unitLogTable = App.MobileService.GetTable<UnitLog>();


        public MessagesOverview()
        {
            this.InitializeComponent();
        }

        protected override async void OnNavigatedTo(NavigationEventArgs e)
        {
            await RefreshTodoItems();
        }

        private async Task RefreshTodoItems()
        {
            MobileServiceInvalidOperationException exception = null;
            try
            {
                // This code refreshes the entries in the list view by querying the TodoItems table.
                // The query excludes completed TodoItems
                items = await unitLogTable.OrderByDescending(unitLog => unitLog.ReportedTime)
                    .ToCollectionAsync();
            }
            catch (MobileServiceInvalidOperationException e)
            {
                exception = e;
            }

            if (exception != null)
            {
                new MessageDialog(exception.Message, "Error loading items").ShowAsync();
            }
            else
            {
                ListItems.ItemsSource = items;
            }
        }

        private async void Refresh_Click(object sender, RoutedEventArgs e){
            await RefreshTodoItems();
        }

        private async void UpdateLog_Click(object sender, RoutedEventArgs e)
        {
            await RefreshTodoItems();
        }
    }
}
