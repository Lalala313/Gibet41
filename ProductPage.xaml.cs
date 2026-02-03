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

        private List<OrderProduct> selectedOrderProducts = new List<OrderProduct>();
        private List<Product> selectedProducts = new List<Product>();
        private int currentOrderID = 1;
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

            //генерируем id заказа
            var lastOrder = Gibet41Entities.GetContext().Order.OrderByDescending(o => o.OrderID).FirstOrDefault();
            if (lastOrder != null)
                currentOrderID = lastOrder.OrderID + 1;

            UpdateServices();

            var currentProduct = Gibet41Entities.GetContext().Product.Where(p => p.ProductQuantityInStock > 0).ToList();

            CountRecords = currentProduct.Count; //общее кол-во записей 
            ProductListView.ItemsSource = currentProduct;
            ComboType.SelectedIndex = 0;
            UpdateServices();
        }

       
        private void UpdateServices()
        {
            var currentProduct = Gibet41Entities.GetContext().Product.ToList();
            currentProduct = currentProduct.Where(p => p.ProductQuantityInStock > 0).ToList();

            CountRecords = currentProduct.Count;

            if (ComboType.SelectedIndex == 0)
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

        private void ProductListView_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {

        }

        private void MenuItem_Click(object sender, RoutedEventArgs e)
        {
            if (ProductListView.SelectedIndex >= 0)
            {
                var prod = ProductListView.SelectedItem as Product;
                selectedProducts.Add(prod);

                var newOrderProd = new OrderProduct();
                newOrderProd.OrderID = currentOrderID;
                newOrderProd.ProductArticleNumber = prod.ProductArticleNumber;
                newOrderProd.OrderProductCount = 1;

                var selOP = selectedOrderProducts.Where(p => Equals(p.ProductArticleNumber, prod.ProductArticleNumber));

                if (selOP.Count() == 0)
                {
                    selectedOrderProducts.Add(newOrderProd);
                }
                else
                {
                    foreach (OrderProduct p in selectedOrderProducts)
                    {
                        if (p.ProductArticleNumber == prod.ProductArticleNumber)
                            p.OrderProductCount++;

                    }
                }
            }
            OrderButton.Visibility = Visibility.Visible;
            ProductListView.SelectedIndex = -1;
        }

        private void ProductListView_MouseRightButtonUp(object sender, MouseButtonEventArgs e)
        {

        }

        private void OrderButton_Click(object sender, RoutedEventArgs e)
        {
            selectedProducts = selectedProducts.Distinct().ToList();
            OrderWindow orderWindow = new OrderWindow(selectedOrderProducts, selectedProducts, FIOTB.Text, user);

            bool? result = orderWindow.ShowDialog();

            if (result == true) //если нажата кнопка сохранить 
            {
                selectedProducts.Clear();
                selectedOrderProducts.Clear();
                UpdateServices();
            }
            OrderButton.Visibility = selectedProducts.Count > 0 ? Visibility.Visible : Visibility.Collapsed;

        }
    }
}
