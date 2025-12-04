using System;
using System.Collections.Generic;
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
using System.Windows.Navigation;
using System.Windows.Shapes;

namespace Гибет41размер
{
    /// <summary>
    /// Логика взаимодействия для ProductPage.xaml
    /// </summary>
    public partial class ProductPage : Page
    {
        private User user;
        int CountRecords; //все записи
        int SortedPage; //отсортированные записи
        public ProductPage(User currentUser)
        {
            InitializeComponent();
            user = currentUser;
            FIOTB.Text = user.UserSurname + " " + user.UserName + " " + user.UserPatronymic;
            if (user.UserRole != 4)
            {
                switch (user.UserRole)
                {
                    case 1:
                        RoleTB.Text = "Клиент"; break;
                    case 2:
                        RoleTB.Text = "Менеджер"; break;
                    case 3:
                        RoleTB.Text = "Администратор"; break;
                }
                RolePanel.Visibility = Visibility.Visible;
            }
            else
            {
                RolePanel.Visibility = Visibility.Collapsed;
            }
            


            var currentProduct = Gibet41Entities.GetContext().Product.ToList();
            CountRecords = currentProduct.Count; //общее кол-во записей 
            ProductListView.ItemsSource = currentProduct;
            ComboType.SelectedIndex = 0;
            UpdateServices();
        }

        private void UpdateServices()
        {
            var currentProduct = Gibet41Entities.GetContext().Product.ToList();
            if(ComboType.SelectedIndex == 0)
            {
                currentProduct = currentProduct.Where(p => (Convert.ToInt32(p.ProductDiscountAmount) >= 0 && Convert.ToInt32(p.ProductDiscountAmount) <= 100)).ToList();
            }
            if (ComboType.SelectedIndex == 1)
            {
                currentProduct = currentProduct.Where(p => (Convert.ToInt32(p.ProductDiscountAmount) >= 0 && Convert.ToInt32(p.ProductDiscountAmount) <= 10)).ToList();
            }
            if (ComboType.SelectedIndex == 2)
            {
                currentProduct = currentProduct.Where(p => (Convert.ToInt32(p.ProductDiscountAmount) >= 10 && Convert.ToInt32(p.ProductDiscountAmount) <= 15)).ToList();
            }
            if (ComboType.SelectedIndex == 3)
            {
                currentProduct = currentProduct.Where(p => (Convert.ToInt32(p.ProductDiscountAmount) >= 15 && Convert.ToInt32(p.ProductDiscountAmount) <= 100)).ToList();
            }

            currentProduct = currentProduct.Where(p => p.ProductName.ToLower().Contains(TBoxSearch.Text.ToLower())).ToList();

            SortedPage = currentProduct.Count;

           ProductListView.ItemsSource = currentProduct.ToList();

            if (RButtonDown.IsChecked.Value)
            {
                ProductListView.ItemsSource = currentProduct.OrderByDescending(p => p.ProductCost).ToList();
            }
            if (RButtonUp.IsChecked.Value)
            {
                ProductListView.ItemsSource = currentProduct.OrderBy(p => p.ProductCost).ToList();
            }
            
            UpdateRecordsCount();
        }

        private void UpdateRecordsCount()
        {
            TBCount.Text = $"Количество {SortedPage} из {CountRecords}";
        }

        private void TBoxSearch_TextChanged(object sender, TextChangedEventArgs e)
        {
            UpdateServices();
        }

        private void ComboType_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            UpdateServices();

        }

        private void RadioButton_Checked(object sender, RoutedEventArgs e)
        {
            UpdateServices();

        }

        private void RButtonUp_Checked(object sender, RoutedEventArgs e)
        {
            UpdateServices();

        }

        private void RButtonDown_Checked(object sender, RoutedEventArgs e)
        {
            UpdateServices();
        }
    }
}
