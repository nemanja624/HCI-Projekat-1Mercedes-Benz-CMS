using Mercedes_Models_CMS.Helpers;
using Mercedes_Models_CMS.Models;
using Microsoft.Win32;
using SharpVectors.Dom.Svg;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO;
using System.Linq;
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
using static System.Net.Mime.MediaTypeNames;

namespace Mercedes_Models_CMS.Pages
{
    /// <summary>
    /// Interaction logic for AddCarModel.xaml
    /// </summary>
    public partial class AddCarModel : Window
    {
        ObservableCollection<CarModel> Models { get; set; }
        DataPage DataPage { get; set; }
        CarModel Model {  get; set; }
        private bool _editMode = false;
        public AddCarModel()
        {
            InitializeComponent();
        }
        public AddCarModel(ObservableCollection<CarModel> models, DataPage dataPage ) : this()
        {
            Model = null;
            
            FontFamilyComboBox.ItemsSource = Fonts.SystemFontFamilies.OrderBy(f => f.Source);
            Models = models;
            DataPage = dataPage;
        }
        public AddCarModel(ObservableCollection<CarModel> models, DataPage dataPage, CarModel model) : this(models, dataPage)
        {
            Model = model;
            _editMode = true;
            Add_Button.Content = "SAVE CHANGES";
            ModelName_TextBox.Text = model.Name;
            HoursePower_TextBox.Text = model.HorsePower.ToString();
            try
            {
                BitmapImage bitmap = new BitmapImage(new Uri(model.ImagePath, UriKind.RelativeOrAbsolute));
                ModelImage.Source = bitmap;
            }
            catch (Exception ex)
            {
                MessageBox.Show("Error loading image: " + ex.Message);
            }
            if (!string.IsNullOrEmpty(model.RtfFilePath))
            {
                try
                {
                    FlowDocument flowDoc = new FlowDocument();
                    TextRange textRange = new TextRange(flowDoc.ContentStart, flowDoc.ContentEnd);
                    using (var stream = new FileStream(model.RtfFilePath, FileMode.Open))
                    {
                        textRange.Load(stream, DataFormats.Rtf);
                    }
                    EditorRichTextBox.Document = flowDoc;
                }
                catch (Exception ex)
                {
                    MessageBox.Show("Error loading RTF document: " + ex.Message);
                }
            }
        }
        private void Window_MouseDown(object sender, MouseButtonEventArgs e)
        {
            this.DragMove();
        }


        private void Browse_Button_MouseEnter(object sender, MouseEventArgs e)
        {
            Fa5Image.Foreground = Brushes.Black;
        }

        private void Browse_Button_MouseLeave(object sender, MouseEventArgs e)
        {
            Fa5Image.Foreground = (Brush)FindResource("AccentGoldBrush");
        }
        private void EditorRichTextBox_KeyUp(object sender, KeyEventArgs e)
        {
            int numberOfWords = 0;

            string text = new TextRange(EditorRichTextBox.Document.ContentStart, EditorRichTextBox.Document.ContentEnd).Text.Trim();

            if (!string.IsNullOrEmpty(text))
            {
                numberOfWords = text.Split(new char[] { ' ', '\n', '\r' }, System.StringSplitOptions.RemoveEmptyEntries).Length;
            }

            Words_Label.Content = $"Words: {numberOfWords}";
        }
        private void EditorRichTextBox_SelectionChanged(object sender, RoutedEventArgs e)
        {
            object fontWeight = EditorRichTextBox.Selection.GetPropertyValue(Inline.FontWeightProperty);
            BoldToggleButton.IsChecked = (fontWeight != DependencyProperty.UnsetValue) && (fontWeight.Equals(FontWeights.Bold));

            object fontFamily = EditorRichTextBox.Selection.GetPropertyValue(Inline.FontFamilyProperty);
            FontFamilyComboBox.SelectedItem = fontFamily;

            object fontSize = EditorRichTextBox.Selection.GetPropertyValue(TextElement.FontSizeProperty);
            foreach(ComboBoxItem item in FontSizeCombox.Items)
            {
                if(item.Content.ToString() == fontSize.ToString())
                {
                    FontSizeCombox.SelectedItem = item;
                    break;
                }
            }

            object foregroundColor = EditorRichTextBox.Selection.GetPropertyValue(TextElement.ForegroundProperty);

            if (foregroundColor is SolidColorBrush solidColorBrush)
            {
                ColorPickerText.SelectedColor = solidColorBrush.Color;
            }
            object fontItalic = EditorRichTextBox.Selection.GetPropertyValue(Inline.FontStyleProperty);

            ItalicButton.IsChecked = (fontItalic != DependencyProperty.UnsetValue && fontItalic.Equals(FontStyles.Italic));

            object underLine = EditorRichTextBox.Selection.GetPropertyValue(Inline.TextDecorationsProperty);
            UnderlineButton.IsChecked = (underLine != DependencyProperty.UnsetValue) &&
                                     (underLine is TextDecorationCollection decorations) &&
                                     decorations.Contains(TextDecorations.Underline[0]);
        }

        private void FontFamilyComboBox_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (FontFamilyComboBox.SelectedItem == null) return;

            var font = (FontFamily)FontFamilyComboBox.SelectedItem;
            if (!EditorRichTextBox.Selection.IsEmpty)
            {
                EditorRichTextBox.Selection.ApplyPropertyValue(Inline.FontFamilyProperty, FontFamilyComboBox.SelectedItem);
            }
            else
            {
                EditorRichTextBox.Focus();
            }
        }

        private void ColorPickerText_SelectedColorChanged(object sender, RoutedPropertyChangedEventArgs<Color?> e)
        {

            EditorRichTextBox.Selection.ApplyPropertyValue(TextElement.ForegroundProperty, new SolidColorBrush((Color)ColorPickerText.SelectedColor));

        }
        private void FontSizeCombox_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (FontSizeCombox.SelectedItem != null)
            {
                
                if (!EditorRichTextBox.Selection.IsEmpty)
                {

                    ComboBoxItem selectedFontSize = FontSizeCombox.SelectedItem as ComboBoxItem;

                    if (double.TryParse(selectedFontSize.Content.ToString(), out double FontSize))
                    {
                        EditorRichTextBox.Selection.ApplyPropertyValue(TextElement.FontSizeProperty, FontSize);
                    }

                }
            }
        }
        private string SaveRTF()
        {

            string rtfFilePath = @"../../../Data/RTFs/" + ModelName_TextBox.Text.Trim() + ".rtf";

            TextRange textRange = new TextRange(EditorRichTextBox.Document.ContentStart, EditorRichTextBox.Document.ContentEnd);

            using (FileStream stream = new FileStream(rtfFilePath, FileMode.Create))
            {
                textRange.Save(stream, DataFormats.Rtf);
            }

            return rtfFilePath;
        }

        private void Browse_Button_Click(object sender, RoutedEventArgs e)
        {
            OpenFileDialog openFileDialog = new OpenFileDialog();
            openFileDialog.Filter = "Image Files|*.jpg;*.jpeg;*.png;*.bmp";
            if (openFileDialog.ShowDialog() == true)
            {
                ModelImage.Source = new BitmapImage(new Uri(openFileDialog.FileName));
            }
        }

        private void Add_Button_Click(object sender, RoutedEventArgs e)
        {

            if (_editMode == false && ValidateModel())
            {
                string rtfPath = SaveRTF();
                Models.Add(new CarModel
                {
                    Name = ModelName_TextBox.Text.Trim(),
                    ImagePath = ModelImage.Source.ToString(),
                    DateAdded = DateTime.Now,
                    RtfFilePath = rtfPath,
                    HorsePower = Int32.Parse(HoursePower_TextBox.Text.Trim()),
                    IsSelected = false
                });
                ToastNotification toastNotification = new ToastNotification("Successfully Added New Model", "", Notification.Wpf.NotificationType.Success);
                DataPage.ShowToastNotification(toastNotification);
                this.Close();
            }
            else if (_editMode == true && ValidateModel()) 
            {
                string rtfPath = SaveRTF();
                Model.Name = ModelName_TextBox.Text.Trim();
                Model.ImagePath = ModelImage.Source.ToString();
                Model.DateAdded = DateTime.Now;
                Model.RtfFilePath = rtfPath;
                Model.HorsePower = Int32.Parse(HoursePower_TextBox.Text.Trim());
                ToastNotification toastNotification = new ToastNotification("Successfully Changed Existing Model", "", Notification.Wpf.NotificationType.Success);
                DataPage.ShowToastNotification(toastNotification);
                DataPage.DataGridCarModels.Items.Refresh();
                this.Close();
            }
        }
        private bool ValidateModel()
        {
            bool IsValid = true;
            if( ModelImage.Source == null)
            {
                Image_Error_Label.Content = "You must enter an image.";
                IsValid =  false;
            }
            else
            {
                Image_Error_Label.Content = "";
            }
            if (string.IsNullOrEmpty(ModelName_TextBox.Text))
            {
                
                ModelName_TextBox.BorderBrush = Brushes.Red;
                ModelName_Error_Label.Content = "Please enter Model Name";
                IsValid = false;
            }
            else
            {
                ModelName_TextBox.BorderBrush = (Brush)FindResource("AccentGoldBrush");
                ModelName_Error_Label.Content = "";
            }
            if (int.TryParse(HoursePower_TextBox.Text, out int hp))
            {
                if (hp <= 0)
                {
                    HorsePower_Error_Label.Content = "Horse Power must be a positive whole number";
                    HoursePower_TextBox.BorderBrush = Brushes.Red;
                    IsValid = false;
                }
                else
                {
                    HorsePower_Error_Label.Content = "";
                    HoursePower_TextBox.BorderBrush = (Brush)FindResource("AccentGoldBrush");
                }
            }
            else
            {
                HorsePower_Error_Label.Content = "Please Enter Horse Power";
                HoursePower_TextBox.BorderBrush = Brushes.Red;
                IsValid = false;
            }
            
            string text = new TextRange(EditorRichTextBox.Document.ContentStart, EditorRichTextBox.Document.ContentEnd).Text.Trim();

            if (string.IsNullOrWhiteSpace(text))
            {
                RichTextBox_Error_Label.Content = "You must enter description of a model";
                IsValid = false;
            }else
            {
                RichTextBox_Error_Label.Content = "";
            }

                return IsValid;
            
        }

        private void Cancel_Button_Click(object sender, RoutedEventArgs e)
        {
            this.Close();
        }
    }
    
}
