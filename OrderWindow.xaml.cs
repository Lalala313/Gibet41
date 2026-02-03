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
using System.Windows.Shapes;

namespace Гибет41размер
{
    /// <summary>
    /// Логика взаимодействия для OrderWindow.xaml
    /// </summary>
    public partial class OrderWindow : Window
    {
        List<OrderProduct> selectedOrderProducts = new List<OrderProduct>();
        List<Product> selectedProducts = new List<Product>();
        private User currentUser;
        private Order currentOrder = new Order();
        private OrderProduct currentOrderProduct = new OrderProduct();

        private void SetDeliveryDate()
        {
            int delivaryDays = CalculateDeliveryDays();
            DateTime deliveryDate = DateTime.Now.AddDays(delivaryDays);
            OrderDeliveryDP.SelectedDate = deliveryDate;
        }

        private int CalculateDeliveryDays()
        {
            int deliveryDays = 6;

            bool allProductsAvailable = true;
            foreach (var orderProduct in selectedOrderProducts)
            {
                var productInDB = Gibet41Entities.GetContext().Product.FirstOrDefault(p => p.ProductArticleNumber == orderProduct.ProductArticleNumber);
                if (productInDB != null)
                {
                    if (productInDB.ProductQuantityInStock < 3 || productInDB.ProductQuantityInStock < orderProduct.OrderProductCount)
                    {
                        allProductsAvailable = false;
                        break;
                    }
                }
                else
                {
                    allProductsAvailable = false;
                    break;
                }
            }
            if (allProductsAvailable)
            {
                deliveryDays = 3;
            }

            return deliveryDays;
        }

        public OrderWindow(List<OrderProduct> selectedOrderProducts, List<Product> selectedProducts, string FIO, User user)
        {
            InitializeComponent();

            var currentPickUps = Gibet41Entities.GetContext().PickUpPoint.ToList();
            PickUpCombo.ItemsSource = currentPickUps;
            ClientTB.Text = FIO;
            this.currentUser = user;
            if (selectedOrderProducts.Any())
                TBOrderID.Text = selectedOrderProducts.First().OrderID.ToString();

            foreach (Product p in selectedProducts)
            {
                var orderProduct = selectedOrderProducts.FirstOrDefault(op => op.ProductArticleNumber == p.ProductArticleNumber);

                if (orderProduct != null)
                    p.QuantityInOrder = orderProduct.OrderProductCount;
                else
                    p.QuantityInOrder = 1;
            }
            ShoeListView.ItemsSource = selectedProducts;

            this.selectedOrderProducts = selectedOrderProducts;
            this.selectedProducts = selectedProducts;
            OderDP.Text = DateTime.Now.ToString();
            SetDeliveryDate();

            CalculateAmounts();

            if (selectedProducts.Count == 0)
            {
                SaveButton.IsEnabled = false;
            }
        }

        private void CalculateAmounts()
        {
            decimal totalAmount = 0;      // Общая сумма без скидки
            decimal discountAmount = 0;   // Сумма скидки
            decimal finalAmount = 0;      // Итоговая сумма со скидкой
            foreach (var product in selectedProducts)
            {
                // ищем соответствующий OrderProduct для получения количества
                var orderProduct = selectedOrderProducts.FirstOrDefault(op => op.ProductArticleNumber == product.ProductArticleNumber);

                int quantity = orderProduct != null ? orderProduct.OrderProductCount : product.QuantityInOrder;

                //сумма без скидки 
                decimal productTotal = product.ProductCost * quantity;
                totalAmount += productTotal;

                //сумма скидки 
                decimal productDiscount = productTotal * (product.ProductDiscountAmount / 100m);
                discountAmount += productDiscount;

                //итоговая сумма 
                finalAmount += productTotal - productDiscount;
            }

            //обновляем отображение
            TotalAmountTB.Text = $"{totalAmount:F2} руб";
            DiscountAmountTB.Text = $"-{discountAmount:F2} руб";
            FinalAmountTB.Text = $"{finalAmount:F2} руб";
        }

        private void ShoeListView_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {

        }

        private void SaveButton_Click(object sender, RoutedEventArgs e)
        {
            StringBuilder errors = new StringBuilder();
            if (PickUpCombo.SelectedItem == null)
                errors.AppendLine("Выберите пункт выдачи");
            if (selectedProducts.Count == 0)
                errors.AppendLine("Добавьте хотя бы один товар в заказ");
            if (OrderDeliveryDP.SelectedDate.Value <= DateTime.Now)
                errors.AppendLine("Дата доставки должна быть в будущем");

            foreach (var orderProduct in selectedOrderProducts)
            {
                var productInDB = Gibet41Entities.GetContext().Product
                    .FirstOrDefault(p => p.ProductArticleNumber == orderProduct.ProductArticleNumber);

                if (productInDB == null)
                {
                    errors.AppendLine($"Товар с артикулом {orderProduct.ProductArticleNumber} не найден в базе");
                }
                else if (productInDB.ProductQuantityInStock < orderProduct.OrderProductCount)
                {
                    errors.AppendLine($"Недостаточно товара '{productInDB.ProductName}' на складе.");
                }
            }

            if (errors.Length > 0)
            {
                MessageBox.Show(errors.ToString());
                return;
            }

            try
            {
                var selectedPickUp = PickUpCombo.SelectedItem as PickUpPoint;

                Random random = new Random();
                int orderCode = random.Next(100, 1000);
                Order newOrder = new Order
                {
                    OrderDate = DateTime.Now,
                    OrderDeliveryDate = OrderDeliveryDP.SelectedDate.Value,
                    OrderPickupPoint = selectedPickUp.PickUpPointID,
                    OrderClientID = currentUser.UserRole == 4 ? (int?)null : currentUser.UserID,
                    OrderCode = orderCode,
                    OrderStatus = "Новый"
                };

                Gibet41Entities.GetContext().Order.Add(newOrder);
                Gibet41Entities.GetContext().SaveChanges();

                foreach (var orderProduct in selectedOrderProducts)
                {
                    orderProduct.OrderID = newOrder.OrderID;
                    Gibet41Entities.GetContext().OrderProduct.Add(orderProduct);
                    var productInDB = Gibet41Entities.GetContext().Product
                        .FirstOrDefault(p => p.ProductArticleNumber == orderProduct.ProductArticleNumber);

                    if (productInDB != null)
                    {
                        productInDB.ProductQuantityInStock -= orderProduct.OrderProductCount;
                    }
                }

                Gibet41Entities.GetContext().SaveChanges();
                MessageBox.Show("Заказ успешно создан!");
                this.DialogResult = true;
                this.Close();
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Ошибка: {ex.Message}");
            }
        }
        private void BtnMinus_Click(object sender, RoutedEventArgs e)
        {
            var prod = (sender as Button).DataContext as Product;

            if (prod.QuantityInOrder > 1)
            {
                prod.QuantityInOrder--;
                var orderProduct = selectedOrderProducts.FirstOrDefault(p => p.ProductArticleNumber == prod.ProductArticleNumber);

                if (orderProduct != null)
                {
                    orderProduct.OrderProductCount--;
                }

                ShoeListView.Items.Refresh();
                CalculateAmounts();
            }
            else
            {
                if (MessageBox.Show("Удалить товар из заказа?", "Подтверждение", MessageBoxButton.YesNo) == MessageBoxResult.Yes)
                {
                    selectedProducts.Remove(prod);

                    var orderProduct = selectedOrderProducts.FirstOrDefault(p => p.ProductArticleNumber == prod.ProductArticleNumber);

                    if (orderProduct != null)
                    {
                        selectedOrderProducts.Remove(orderProduct);
                    }

                    ShoeListView.ItemsSource = null;
                    ShoeListView.ItemsSource = selectedProducts;
                    CalculateAmounts();
                }
            }
        }

        private void BtnPlus_Click(object sender, RoutedEventArgs e)
        {
            var prod = (sender as Button).DataContext as Product;

            //Проверяем, есть ли товар на складе
            var productInDB = Gibet41Entities.GetContext().Product.FirstOrDefault(p => p.ProductArticleNumber == prod.ProductArticleNumber);

            if (productInDB != null && prod.QuantityInOrder < productInDB.ProductQuantityInStock)
            {
                prod.QuantityInOrder++;

                //Обновляем OrderProduct
                var orderProduct = selectedOrderProducts.FirstOrDefault(p => p.ProductArticleNumber == prod.ProductArticleNumber);

                if (orderProduct != null)
                {
                    orderProduct.OrderProductCount++;
                }

                ShoeListView.Items.Refresh();
                CalculateAmounts();
            }
            else
            {
                MessageBox.Show("Недостаточно товара на складе");
            }
        }

    } 
}
