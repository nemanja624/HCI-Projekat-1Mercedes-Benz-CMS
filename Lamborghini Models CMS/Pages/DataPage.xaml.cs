using Mercedes_Models_CMS.Helpers;
using Mercedes_Models_CMS.Models;
using Notification.Wpf;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO;
using System.Linq;
using System.Reflection.Metadata.Ecma335;
using System.Runtime.ConstrainedExecution;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;

namespace Mercedes_Models_CMS.Pages
{
    /// <summary>
    /// Interaction logic for DataPage.xaml
    /// </summary>
    public partial class DataPage : Window
    {
        private const string CarModelsFilePath = "../../../Data/CarModels.xml";
        private const string StartUpDataFolderPath = "../../../Data/StartUpData";
        private NotificationManager _notificationManager = new NotificationManager();
        private ToastNotification? _notification;
        public ObservableCollection<CarModel> CarModels { get; set; }
        private DataIO _serializer = new DataIO();
        private User _user = null;
        public DataPage()
        {
            CarModels = new ObservableCollection<CarModel>();
            InitializeComponent();
            this.ContentRendered += OnLoad;
            CarModels = _serializer.DeSerializeObject<ObservableCollection<CarModel>>(CarModelsFilePath)
                        ?? new ObservableCollection<CarModel>();
            DataContext = this;
        }

        public void PersistCarModels()
        {
            _serializer.SerializeObject(CarModels, CarModelsFilePath);
        }

        private void OnLoad(object? sender, EventArgs e)
        {
            ShowToastNotification(_notification);
        }
        public DataPage(ToastNotification notification,User user) : this()
        {
            _notification = notification;
            if(user.Role == UserRole.VISITOR)
            {
                Add_Button.Visibility = Visibility.Collapsed;
                Delete_Button.Visibility = Visibility.Collapsed;
                dataGridColumnDelete.Visibility = Visibility.Collapsed;
            }
            _user = user;
        }
        public void ShowToastNotification(ToastNotification toastNotification)
        {
            if(toastNotification == null) return;
            _notificationManager.Show(toastNotification.Title,
            toastNotification.Message, toastNotification.Type,
            "DataNotificationArea");
        }
        private void Window_MouseDown(object sender, MouseButtonEventArgs e)
        {
            this.DragMove();
        }

        private void Logut_Button_Click(object sender, RoutedEventArgs e)
        {
            MainWindow mainWindow = new MainWindow(new ToastNotification("Successfully logged out","",NotificationType.Success));
            mainWindow.Show();
            this.Close();
        }

        private void checkBoxSelectAll_Click(object sender, RoutedEventArgs e)
        {
            var checkBox = sender as CheckBox;
            foreach (var item in CarModels)
            {
                if (checkBox.IsChecked == true)
                    item.IsSelected = true;
                else
                    item.IsSelected = false;
            }
            DataGridCarModels.Items.Refresh();
        }

        private void Add_Button_Click(object sender, RoutedEventArgs e)
        {
            AddCarModel addCarModel = new AddCarModel(CarModels,this);
            addCarModel.Owner  = this;
            addCarModel.WindowStartupLocation = WindowStartupLocation.Manual;
            addCarModel.Left = this.Left;                 
            addCarModel.Top = this.Top + 80;    

            addCarModel.ShowDialog();   
        }

        private void Hyperlink_Click(object sender, RoutedEventArgs e)
        {
            Hyperlink hl = (Hyperlink)sender;
            if(_user.Role == UserRole.ADMIN)
            {
                CarModel model = (CarModel)hl.DataContext;
                AddCarModel addCarModel = new AddCarModel(CarModels, this,model);
                addCarModel.Owner = this;
                addCarModel.WindowStartupLocation = WindowStartupLocation.Manual;
                addCarModel.Left = this.Left;
                addCarModel.Top = this.Top + 80;

                addCarModel.ShowDialog();
            }
            else
            {
                CarModel model = (CarModel)hl.DataContext;
                PreviewCarModel previewModel = new PreviewCarModel(model);
                previewModel.Owner = this;
                previewModel.WindowStartupLocation = WindowStartupLocation.Manual;
                previewModel.Left = this.Left;
                previewModel.Top = this.Top + 80;

                previewModel.ShowDialog();
            }
        }

        private void Delete_Button_Click(object sender, RoutedEventArgs e)
        {
            List<CarModel> modelsToDelete = new List<CarModel>();
            if(AreAllSelected())
            {
                MessageBoxResult result = MessageBox.Show($"Are you sure you want to delete all Models?", "Confirm Deletion", MessageBoxButton.YesNo, MessageBoxImage.Question);
                if (result == MessageBoxResult.Yes)
                {
                    foreach (var car in CarModels)
                    {
                        DeleteModelFiles(car);
                    }

                    CarModels.Clear();
                    PersistCarModels();
                    DataGridCarModels.Items.Refresh();
                    ShowToastNotification(new ToastNotification("Success", "All models were deleted", NotificationType.Success));
                }
                    return;
            }
            foreach (var car in CarModels)
            {
                if (car.IsSelected)
                {
                    MessageBoxResult result = MessageBox.Show($"Are you sure you want to delete {car.Name} Model?", "Confirm Deletion", MessageBoxButton.YesNo, MessageBoxImage.Question);

                    if (result == MessageBoxResult.Yes)
                    {
                        DeleteModelFiles(car);
                        modelsToDelete.Add(car);

                        ShowToastNotification(new ToastNotification("Succsess!", "Successfully deleted car model", NotificationType.Success));
                    }
                }
                    
            }
            foreach (var model in modelsToDelete)
            {
                CarModels.Remove(model);
            }

            if (modelsToDelete.Count > 0)
            {
                PersistCarModels();
            }
        }

        private void DeleteModelFiles(CarModel car)
        {
            if (!string.IsNullOrWhiteSpace(car.RtfFilePath) && File.Exists(car.RtfFilePath))
            {
                File.Delete(car.RtfFilePath);
            }

            string startupRtfFilePath = System.IO.Path.Combine(StartUpDataFolderPath, System.IO.Path.GetFileName(car.RtfFilePath));
            if (File.Exists(startupRtfFilePath))
            {
                File.Delete(startupRtfFilePath);
            }
        }

        bool AreAllSelected()
        {
            if (CarModels == null || CarModels.Count == 0)
                return false;

            foreach(var car in CarModels) {
                if (!car.IsSelected)
                    return false;
            }
            return true;
        }
        private void chckBox_Click(object sender, RoutedEventArgs e)
        {
            var checkBox = sender as CheckBox;
            var model = checkBox?.DataContext as CarModel;

            if (model != null)
            {
                model.IsSelected = !model.IsSelected;
            }
            if (checkBox.IsChecked == false)
                chcekBoxSelectAll.IsChecked = false;
        }
    }
    
}
