using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Windows.Forms;
using System.IO;             
using Newtonsoft.Json;       
using System.Globalization;  

namespace RestaurantManagementApp
{
    public partial class Form1 : Form
    {
        private RestaurantManager restaurantManager;
        private BindingSource menuBindingSource;
        private BindingSource ordersBindingSource;
        private BindingSource currentOrderItemsBindingSource;
        private BindingSource usersBindingSource;
        private BindingSource userAddressesBindingSource;
        private BindingSource reviewsBindingSource; 

        private Order currentActiveOrder;
        private User currentUser; 
        private User selectedDeliveryCustomer; 
        private Address selectedDeliveryAddress; 

        private User loggedInUser; 

        public static readonly CultureInfo UsCulture = CultureInfo.GetCultureInfo("en-US");


        public Form1()
        {
            InitializeComponent();

            this.cmbOrderType.SelectedIndexChanged += new System.EventHandler(this.cmbOrderType_SelectedIndexChanged);
            this.cmbDeliveryCustomer.SelectedIndexChanged += new System.EventHandler(this.cmbDeliveryCustomer_SelectedIndexChanged);
            this.cmbDeliveryAddress.SelectedIndexChanged += new System.EventHandler(this.cmbDeliveryAddress_SelectedIndexChanged);
            this.btnUpdateOrderStatus.Click += new System.EventHandler(this.btnUpdateOrderStatus_Click);
            this.btnSubmitReview.Click += new System.EventHandler(this.btnSubmitReview_Click);       
            this.btnSwitchUser.Click += new System.EventHandler(this.btnSwitchUser_Click);
            this.lbxUserAddresses.SelectedIndexChanged += new System.EventHandler(this.lbxUserAddresses_SelectionChanged);
            Console.WriteLine("[DEBUG] ComboBox SelectedIndexChanged events attached programmatically.");

            Console.WriteLine($"[DEBUG] Constructor (after InitializeComponent): pnlDeliveryDetails.Visible = {pnlDeliveryDetails.Visible}");
        }

        private void Form1_Load(object sender, EventArgs e)
        {
            restaurantManager = new RestaurantManager();
            restaurantManager.LoadData();

            menuBindingSource = new BindingSource();
            ordersBindingSource = new BindingSource();
            usersBindingSource = new BindingSource();
            userAddressesBindingSource = new BindingSource();
            currentOrderItemsBindingSource = new BindingSource();
            reviewsBindingSource = new BindingSource(); 

            dgvMenuItems.AutoGenerateColumns = false;
            dgvMenuItems.Columns.Clear();
            dgvMenuItems.Columns.Add(new DataGridViewTextBoxColumn() { Name = "colMenuItemId", HeaderText = "ID", DataPropertyName = "Id", Visible = false });
            dgvMenuItems.Columns.Add(new DataGridViewTextBoxColumn() { Name = "colMenuItemName", HeaderText = "Name", DataPropertyName = "Name", Visible = true });
            dgvMenuItems.Columns.Add(new DataGridViewTextBoxColumn() { Name = "colMenuItemDescription", HeaderText = "Description", DataPropertyName = "Description", Visible = true });
            dgvMenuItems.Columns.Add(new DataGridViewTextBoxColumn() { Name = "colMenuItemPrice", HeaderText = "Price", DataPropertyName = "Price", Visible = true });
            dgvMenuItems.Columns.Add(new DataGridViewTextBoxColumn() { Name = "colMenuItemCategory", HeaderText = "Category", DataPropertyName = "Category", Visible = true });
            menuBindingSource.DataSource = restaurantManager.MenuItems;
            dgvMenuItems.DataSource = menuBindingSource;


            dgvOrderMenu.AutoGenerateColumns = false;
            dgvOrderMenu.Columns.Clear();
            dgvOrderMenu.Columns.Add(new DataGridViewTextBoxColumn() { Name = "colOrderMenuId", HeaderText = "ID", DataPropertyName = "Id", Visible = false });
            dgvOrderMenu.Columns.Add(new DataGridViewTextBoxColumn() { Name = "colOrderMenuName", HeaderText = "Name", DataPropertyName = "Name", Visible = true });
            dgvOrderMenu.Columns.Add(new DataGridViewTextBoxColumn() { Name = "colOrderMenuDescription", HeaderText = "Description", DataPropertyName = "Description", Visible = true });
            dgvOrderMenu.Columns.Add(new DataGridViewTextBoxColumn() { Name = "colOrderMenuPrice", HeaderText = "Price", DataPropertyName = "Price", Visible = true });
            dgvOrderMenu.Columns.Add(new DataGridViewTextBoxColumn() { Name = "colOrderMenuCategory", HeaderText = "Category", DataPropertyName = "Category", Visible = true });
            dgvOrderMenu.DataSource = menuBindingSource;


            ordersBindingSource.DataSource = restaurantManager.Orders.Where(o => o.Status != OrderStatus.Delivered && o.Status != OrderStatus.Cancelled).ToList();
            lbxExistingOrders.DataSource = ordersBindingSource;
            lbxExistingOrders.DisplayMember = "TableNumber";


            dgvUser.AutoGenerateColumns = false;
            dgvUser.Columns.Clear();
            dgvUser.Columns.Add(new DataGridViewTextBoxColumn() { Name = "colUserIdNew", HeaderText = "ID", DataPropertyName = "Id", Visible = false });
            dgvUser.Columns.Add(new DataGridViewTextBoxColumn() { Name = "colUsernameNew", HeaderText = "Username", DataPropertyName = "Username", Visible = true });
            dgvUser.Columns.Add(new DataGridViewTextBoxColumn() { Name = "colPasswordHashNew", HeaderText = "PasswordHash", DataPropertyName = "PasswordHash", Visible = false });
            dgvUser.Columns.Add(new DataGridViewTextBoxColumn() { Name = "colRoleNew", HeaderText = "Role", DataPropertyName = "Role", Visible = true });
            usersBindingSource.DataSource = restaurantManager.Users;
            dgvUser.DataSource = usersBindingSource;


            userAddressesBindingSource.DataSource = new List<Address>();
            lbxUserAddresses.DataSource = userAddressesBindingSource;
            //lbxUserAddresses.DisplayMember = "Street";

            dgvCurrentOrderItems.AutoGenerateColumns = false;
            dgvCurrentOrderItems.Columns.Clear();
            dgvCurrentOrderItems.Columns.Add(new DataGridViewTextBoxColumn() { Name = "colCurrentOrderItemName", HeaderText = "Item Name", DataPropertyName = "ItemNameDisplay", Visible = true });
            dgvCurrentOrderItems.Columns.Add(new DataGridViewTextBoxColumn() { Name = "colCurrentOrderItemQuantity", HeaderText = "Quantity", DataPropertyName = "Quantity", Visible = true });
            dgvCurrentOrderItems.Columns.Add(new DataGridViewTextBoxColumn() { Name = "colCurrentOrderItemPriceEach", HeaderText = "Price Each", DataPropertyName = "ItemPriceDisplay", Visible = true });
            dgvCurrentOrderItems.Columns.Add(new DataGridViewTextBoxColumn() { Name = "colCurrentOrderItemTotalPrice", HeaderText = "Total Price", DataPropertyName = "TotalPriceDisplay", Visible = true });
            dgvCurrentOrderItems.Columns.Add(new DataGridViewTextBoxColumn() { Name = "colCurrentOrderItemIdHidden", HeaderText = "Item ID", DataPropertyName = "Item.Id", Visible = false });
            currentOrderItemsBindingSource.DataSource = new List<OrderItem>();
            dgvCurrentOrderItems.DataSource = currentOrderItemsBindingSource;


            cmbOrderType.SelectedIndex = 0; 
            Console.WriteLine($"[DEBUG] Form1_Load (after cmbOrderType.SelectedIndex = 0): pnlDeliveryDetails.Visible = {pnlDeliveryDetails.Visible}");


            cmbDeliveryCustomer.DisplayMember = "Username";
            cmbDeliveryCustomer.ValueMember = "Id";

            cmbDeliveryAddress.DisplayMember = "Street";
            cmbDeliveryAddress.ValueMember = "Id";
            cmbDeliveryAddress.DataSource = new List<Address>(); 

            cmbRole.Items.Clear();
            cmbRole.Items.Add("Customer");
            cmbRole.Items.Add("Admin");
            cmbRole.SelectedIndex = 0; 

            cmbOrderStatus.Items.Clear();
            foreach (OrderStatus status in Enum.GetValues(typeof(OrderStatus)))
            {
                cmbOrderStatus.Items.Add(status.ToString());
            }
            cmbOrderStatus.SelectedIndex = -1; 

            lbxReviews.DataSource = reviewsBindingSource; 
            lbxReviews.DisplayMember = "ToString"; 

            cmbUserSelect.DataSource = restaurantManager.Users;
            cmbUserSelect.DisplayMember = "Username";
            cmbUserSelect.ValueMember = "Id";

            loggedInUser = restaurantManager.Users.FirstOrDefault(u => u.Username.Equals("admin", StringComparison.OrdinalIgnoreCase));
            if (loggedInUser == null)
            {
                loggedInUser = restaurantManager.Users.FirstOrDefault(); 
            }
            if (loggedInUser != null)
            {
                lblCurrentUserDisplay.Text = $"Logged In: {loggedInUser.Username} ({loggedInUser.Role})";
                cmbUserSelect.SelectedItem = loggedInUser;
            }
            else
            {
                lblCurrentUserDisplay.Text = "Logged In: No User Selected";
                cmbUserSelect.SelectedIndex = -1;
            }

            RefreshUI();
            UpdateTotalsUI();

            Console.WriteLine($"[DEBUG] Form1_Load (after RefreshUI): pnlDeliveryDetails.Visible = {pnlDeliveryDetails.Visible}");
        }

        private void Form1_FormClosing(object sender, FormClosingEventArgs e)
        {
            restaurantManager.SaveData();
            MessageBox.Show("All active data saved.", "Application Closing", MessageBoxButtons.OK, MessageBoxIcon.Information);
        }

        private void RefreshUI()
        {
            menuBindingSource.DataSource = null;
            menuBindingSource.DataSource = restaurantManager.MenuItems;
            menuBindingSource.ResetBindings(false);

            ordersBindingSource.DataSource = null;
            ordersBindingSource.DataSource = restaurantManager.Orders.Where(o => o.Status != OrderStatus.Delivered && o.Status != OrderStatus.Cancelled).ToList();
            if (lbxExistingOrders != null)
            {
                lbxExistingOrders.DisplayMember = "TableNumber";
                ordersBindingSource.ResetBindings(false);
            }

            usersBindingSource.DataSource = null;
            usersBindingSource.DataSource = restaurantManager.Users;
            usersBindingSource.ResetBindings(false);

            if (currentActiveOrder != null)
            {
                currentOrderItemsBindingSource.DataSource = null;
                currentOrderItemsBindingSource.DataSource = currentActiveOrder.Items;
                currentOrderItemsBindingSource.ResetBindings(false);
                lblCurrentOrderStatus.Text = $"Current Status: {currentActiveOrder.Status}";
                cmbOrderStatus.SelectedItem = currentActiveOrder.Status.ToString();
            }
            else
            {
                currentOrderItemsBindingSource.DataSource = new List<OrderItem>();
                currentOrderItemsBindingSource.ResetBindings(false);
                lblCurrentOrderStatus.Text = "Current Status: N/A"; 
                cmbOrderStatus.SelectedIndex = -1; 
            }

            if (currentUser != null)
            {
                userAddressesBindingSource.DataSource = null;
                userAddressesBindingSource.DataSource = currentUser.DeliveryAddresses;
                userAddressesBindingSource.ResetBindings(false);
            }
            else
            {
                userAddressesBindingSource.DataSource = new List<Address>();
                userAddressesBindingSource.ResetBindings(false);
            }

            cmbDeliveryCustomer.DataSource = null; 
            cmbDeliveryCustomer.DataSource = usersBindingSource;
            cmbDeliveryCustomer.DisplayMember = "Username";
            cmbDeliveryCustomer.ValueMember = "Id";
            cmbDeliveryCustomer.Refresh();

            if (currentActiveOrder != null)
            {
                if (currentActiveOrder.Type == OrderType.Delivery)
                {
                    cmbOrderType.SelectedItem = "Delivery"; 
                    pnlDeliveryDetails.Visible = true; 
                    Console.WriteLine($"[DEBUG] RefreshUI (active order is Delivery): pnlDeliveryDetails.Visible = {pnlDeliveryDetails.Visible}");


                    selectedDeliveryCustomer = restaurantManager.GetUserById(currentActiveOrder.CustomerId.GetValueOrDefault());
                    if (selectedDeliveryCustomer != null)
                    {
                        cmbDeliveryCustomer.SelectedItem = selectedDeliveryCustomer;
                        cmbDeliveryAddress.DataSource = null;
                        cmbDeliveryAddress.DataSource = selectedDeliveryCustomer.DeliveryAddresses;
                        cmbDeliveryAddress.DisplayMember = "Street";
                        cmbDeliveryAddress.ValueMember = "Id";

                        selectedDeliveryAddress = selectedDeliveryCustomer.DeliveryAddresses.FirstOrDefault(a => a.Id == currentActiveOrder.DeliveryAddressId);
                        if (selectedDeliveryAddress != null)
                        {
                            cmbDeliveryAddress.SelectedItem = selectedDeliveryAddress;
                        }
                        else
                        {
                            cmbDeliveryAddress.SelectedIndex = -1;
                            selectedDeliveryAddress = null;
                        }
                    }
                    else
                    {
                        cmbDeliveryCustomer.SelectedIndex = -1;
                        selectedDeliveryCustomer = null;
                        cmbDeliveryAddress.DataSource = new List<Address>();
                        cmbDeliveryAddress.SelectedIndex = -1;
                        selectedDeliveryAddress = null;
                    }
                }
                else 
                {
                    cmbOrderType.SelectedItem = "Pickup";
                    pnlDeliveryDetails.Visible = false;
                    Console.WriteLine($"[DEBUG] RefreshUI (active order is Pickup): pnlDeliveryDetails.Visible = {pnlDeliveryDetails.Visible}");
                    cmbDeliveryCustomer.SelectedIndex = -1;
                    selectedDeliveryCustomer = null;
                    cmbDeliveryAddress.DataSource = new List<Address>();
                    cmbDeliveryAddress.SelectedIndex = -1;
                    selectedDeliveryAddress = null;
                }
            }
            else
            {
                cmbOrderType.SelectedIndex = 0; 
                pnlDeliveryDetails.Visible = false;
                Console.WriteLine($"[DEBUG] RefreshUI (no active order): pnlDeliveryDetails.Visible = {pnlDeliveryDetails.Visible}");
                cmbDeliveryCustomer.SelectedIndex = -1;
                selectedDeliveryCustomer = null;
                cmbDeliveryAddress.DataSource = new List<Address>();
                cmbDeliveryAddress.SelectedIndex = -1;
                selectedDeliveryAddress = null;
            }

            UpdateReviewFields(null);

            cmbUserSelect.DataSource = null;
            cmbUserSelect.DataSource = restaurantManager.Users;
            cmbUserSelect.DisplayMember = "Username";
            cmbUserSelect.ValueMember = "Id";
            if (loggedInUser != null)
            {
                cmbUserSelect.SelectedItem = loggedInUser;
            }
            else
            {
                cmbUserSelect.SelectedIndex = -1;
            }
        }

        private void UpdateTotalsUI()
        {
            decimal orderBaseTotal = 0m;
            decimal deliveryCost = 0m;

            if (currentActiveOrder != null)
            {
                orderBaseTotal = currentActiveOrder.Items.Sum(oi => oi.GetTotalPrice());
                deliveryCost = currentActiveOrder.DeliveryCost;
            }

            lblOrderTotal.Text = $"Order Total: {orderBaseTotal.ToString("C", UsCulture)}";
            lblDeliveryCost.Text = $"Delivery Cost: {deliveryCost.ToString("C", UsCulture)}";
            lblFinalTotal.Text = $"Final Total: {((currentActiveOrder != null) ? currentActiveOrder.GetTotalBill() : 0m).ToString("C", UsCulture)}";
        }


        private void dgvMenuItems_SelectionChanged(object sender, EventArgs e)
        {
            if (dgvMenuItems.SelectedRows.Count > 0)
            {
                if (dgvMenuItems.SelectedRows[0].DataBoundItem is MenuItem selectedItem)
                {
                    txtName.Text = selectedItem.Name;
                    txtDescription.Text = selectedItem.Description;
                    txtPrice.Text = selectedItem.Price.ToString();
                    txtCategory.Text = selectedItem.Category;
                    txtImageUrl.Text = selectedItem.ImageUrl;
                    UpdateReviewFields(selectedItem);
                }
                else
                {
                    ClearMenuItemFields();
                }
            }
            else
            {
                ClearMenuItemFields();
            }
        }

        private void btnAddNewItem_Click(object sender, EventArgs e)
        {
            try
            {
                if (string.IsNullOrWhiteSpace(txtName.Text) || string.IsNullOrWhiteSpace(txtCategory.Text))
                {
                    MessageBox.Show("Name and Category cannot be empty.", "Input Error", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                    return;
                }
                if (!decimal.TryParse(txtPrice.Text, out decimal price))
                {
                    MessageBox.Show("Please enter a valid price.", "Input Error", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                    return;
                }
                MenuItem newItem = new MenuItem(txtName.Text, txtDescription.Text, price, txtCategory.Text, txtImageUrl.Text);
                restaurantManager.AddMenuItem(newItem);
                RefreshUI();
                ClearMenuItemFields();
                MessageBox.Show("Menu item added successfully!", "Success", MessageBoxButtons.OK, MessageBoxIcon.Information);
            }
            catch (Exception ex)
            {
                MessageBox.Show($"An error occurred: {ex.Message}", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private void btnEditMenuItem_Click(object sender, EventArgs e)
        {
            if (dgvMenuItems.SelectedRows.Count == 0)
            {
                MessageBox.Show("Please select an item to edit.", "Selection Error", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }
            try
            {
                MenuItem selectedItem = (MenuItem)dgvMenuItems.SelectedRows[0].DataBoundItem;
                Guid itemIdToEdit = selectedItem.Id;
                if (string.IsNullOrWhiteSpace(txtName.Text) || string.IsNullOrWhiteSpace(txtCategory.Text))
                {
                    MessageBox.Show("Name and Category cannot be empty.", "Input Error", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                    return;
                }
                if (!decimal.TryParse(txtPrice.Text, out decimal price))
                {
                    MessageBox.Show("Please enter a valid price.", "Input Error", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                    return;
                }

                MenuItem updatedItem = new MenuItem(txtName.Text, txtDescription.Text, price, txtCategory.Text, txtImageUrl.Text)
                {
                    Id = itemIdToEdit,
                    Reviews = selectedItem.Reviews 
                };
                if (restaurantManager.EditMenuItem(updatedItem))
                {
                    RefreshUI();
                    ClearMenuItemFields();
                    MessageBox.Show("Menu item updated successfully!", "Success", MessageBoxButtons.OK, MessageBoxIcon.Information);
                }
                else
                {
                    MessageBox.Show("Failed to update menu item. Item not found.", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"An error occurred: {ex.Message}", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private void btnDeleteMenuItem_Click(object sender, EventArgs e)
        {
            if (dgvMenuItems.SelectedRows.Count == 0)
            {
                MessageBox.Show("Please select an item to delete.", "Selection Error", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }
            DialogResult result = MessageBox.Show("Are you sure you want to delete the selected menu item?", "Confirm Deletion", MessageBoxButtons.YesNo, MessageBoxIcon.Question);
            if (result == DialogResult.No)
            {
                return;
            }
            try
            {
                MenuItem selectedItem = (MenuItem)dgvMenuItems.SelectedRows[0].DataBoundItem;
                Guid itemIdToDelete = selectedItem.Id;
                if (restaurantManager.DeleteMenuItem(itemIdToDelete))
                {
                    RefreshUI();
                    ClearMenuItemFields();
                    MessageBox.Show("Menu item deleted successfully!", "Success", MessageBoxButtons.OK, MessageBoxIcon.Information);
                }
                else
                {
                    MessageBox.Show("Failed to delete menu item. Item not found.", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"An error occurred: {ex.Message}", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private void btnClearMenuItemFields_Click(object sender, EventArgs e)
        {
            ClearMenuItemFields();
        }

        private void ClearMenuItemFields()
        {
            txtName.Clear();
            txtDescription.Clear();
            txtPrice.Clear();
            txtCategory.Clear();
            txtImageUrl.Clear(); 
            if (dgvMenuItems.Rows.Count > 0)
            {
                dgvMenuItems.ClearSelection();
            }
            UpdateReviewFields(null);
        }

        private void btnSearchMenuItem_Click(object sender, EventArgs e)
        {
            string searchTerm = txtSearchMenuItem.Text;
            List<MenuItem> searchResults = restaurantManager.SearchMenuItems(searchTerm);
            menuBindingSource.DataSource = null;
            menuBindingSource.DataSource = searchResults;
            menuBindingSource.ResetBindings(false);
        }

        private void UpdateReviewFields(MenuItem item)
        {
            if (item != null)
            {
                reviewsBindingSource.DataSource = null; 
                reviewsBindingSource.DataSource = item.Reviews; 
                reviewsBindingSource.ResetBindings(false);

                double averageScore = item.GetAverageScore();
                if (item.Reviews.Any())
                {
                    lblAverageRating.Text = $"Average Rating: {averageScore:F1}/5 ({item.Reviews.Count} reviews)";
                }
                else
                {
                    lblAverageRating.Text = "Average Rating: N/A (No reviews yet)";
                }

                nudReviewScore.Value = 5;
                txtReviewComment.Clear();
                groupBoxReviews.Enabled = (loggedInUser != null);
            }
            else
            {
                reviewsBindingSource.DataSource = new List<Review>();
                reviewsBindingSource.ResetBindings(false);
                lblAverageRating.Text = "Average Rating: N/A"; 

                nudReviewScore.Value = 5;
                txtReviewComment.Clear();
                groupBoxReviews.Enabled = false; 
            }
        }

        private void btnSubmitReview_Click(object sender, EventArgs e)
        {
            if (dgvMenuItems.SelectedRows.Count == 0)
            {
                MessageBox.Show("Please select a menu item to submit a review for.", "Selection Error", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            if (loggedInUser == null) 
            {
                MessageBox.Show("Please select a user to log in before submitting a review.", "Login Required", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            MenuItem selectedItem = (MenuItem)dgvMenuItems.SelectedRows[0].DataBoundItem;

            int score = (int)nudReviewScore.Value;
            string comment = txtReviewComment.Text.Trim();

            if (string.IsNullOrWhiteSpace(comment) && score == 0)
            {
                MessageBox.Show("Please provide a score and/or a comment.", "Input Required", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            Review newReview = new Review(loggedInUser.Id, loggedInUser.Username, score, comment);

            if (selectedItem.Reviews.Any(r => r.UserId == loggedInUser.Id))
            {
                DialogResult result = MessageBox.Show("You have already reviewed this item. Do you want to update your existing review?", "Update Review?", MessageBoxButtons.YesNo, MessageBoxIcon.Question);
                if (result == DialogResult.Yes)
                {
                    var existingReview = selectedItem.Reviews.First(r => r.UserId == loggedInUser.Id);
                    existingReview.Score = score;
                    existingReview.Comment = comment;
                    existingReview.ReviewDate = DateTime.Now; 
                    MessageBox.Show("Your review has been updated!", "Review Updated", MessageBoxButtons.OK, MessageBoxIcon.Information);
                }
                else
                {
                    return; 
                }
            }
            else
            {
                selectedItem.Reviews.Add(newReview);
                MessageBox.Show("Thank you for your review!", "Review Submitted", MessageBoxButtons.OK, MessageBoxIcon.Information);
            }

            restaurantManager.SaveData(); 
            UpdateReviewFields(selectedItem); 
            txtReviewComment.Clear();
            nudReviewScore.Value = 5; 
        }

        private void btnSwitchUser_Click(object sender, EventArgs e)
        {
            if (cmbUserSelect.SelectedItem != null)
            {
                loggedInUser = (User)cmbUserSelect.SelectedItem;
                lblCurrentUserDisplay.Text = $"Logged In: {loggedInUser.Username} ({loggedInUser.Role})";
                MessageBox.Show($"Switched to user: {loggedInUser.Username}", "User Switched", MessageBoxButtons.OK, MessageBoxIcon.Information);

                if (dgvMenuItems.SelectedRows.Count > 0 && dgvMenuItems.SelectedRows[0].DataBoundItem is MenuItem selectedItem)
                {
                    UpdateReviewFields(selectedItem);
                }
                else
                {
                    groupBoxReviews.Enabled = (loggedInUser != null);
                }
            }
            else
            {
                MessageBox.Show("Please select a user from the dropdown.", "No User Selected", MessageBoxButtons.OK, MessageBoxIcon.Warning);
            }
        }

        //order managment

        private void btnCreateNewOrder_Click(object sender, EventArgs e)
        {
            if (!int.TryParse(txtTableNumber.Text, out int tableNumber) || tableNumber <= 0)
            {
                MessageBox.Show("Please enter a valid table number.", "Input Error", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }
            if (restaurantManager.Orders.Any(o => o.TableNumber == tableNumber && o.Status != OrderStatus.Delivered && o.Status != OrderStatus.Cancelled))
            {
                MessageBox.Show($"An active order already exists for table {tableNumber}. Please complete or load it.", "Order Exists", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            currentActiveOrder = restaurantManager.CreateNewOrder(tableNumber);
            currentActiveOrder.Status = OrderStatus.Confirmed;

            string orderTypeString = cmbOrderType.SelectedItem?.ToString();
            if (Enum.TryParse(orderTypeString, out OrderType orderType))
            {
                currentActiveOrder.Type = orderType;
            }
            else
            {
                currentActiveOrder.Type = OrderType.Pickup; 
            }


            if (currentActiveOrder.Type == OrderType.Delivery)
            {
                if (selectedDeliveryCustomer == null || selectedDeliveryAddress == null)
                {
                    MessageBox.Show("Please select a customer and a delivery address for delivery orders.", "Delivery Error", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                    restaurantManager.CancelOrder(currentActiveOrder.Id);
                    currentActiveOrder = null;
                    RefreshUI();
                    return;
                }
                currentActiveOrder.CustomerId = selectedDeliveryCustomer.Id;
                currentActiveOrder.DeliveryAddressId = selectedDeliveryAddress.Id;
                currentActiveOrder.SetDeliveryCost(restaurantManager.GetDeliveryCost(selectedDeliveryAddress));
            }
            else
            {
                currentActiveOrder.SetDeliveryCost(0m);
            }

            MessageBox.Show($"New order created for Table {tableNumber}! Type: {currentActiveOrder.Type}", "Order Created", MessageBoxButtons.OK, MessageBoxIcon.Information);
            currentOrderItemsBindingSource.DataSource = currentActiveOrder.Items;
            currentOrderItemsBindingSource.ResetBindings(false);
            RefreshUI();
            UpdateTotalsUI();
            txtReceipt.Clear();
        }

        private void btnAddItemToOrder_Click(object sender, EventArgs e)
        {
            if (currentActiveOrder == null)
            {
                MessageBox.Show("Please create a new order or load an existing one first.", "No Active Order", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }
            if (currentActiveOrder.Status == OrderStatus.Delivered || currentActiveOrder.Status == OrderStatus.Cancelled)
            {
                MessageBox.Show("Cannot add items to a completed or cancelled order.", "Order Status", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            if (dgvOrderMenu.SelectedRows.Count == 0)
            {
                MessageBox.Show("Please select a menu item to add.", "Selection Error", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }
            if (!int.TryParse(txtQuantity.Text, out int quantity) || quantity <= 0)
            {
                MessageBox.Show("Please enter a valid quantity (positive number).", "Input Error", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }
            try
            {
                MenuItem selectedMenuItem = (MenuItem)dgvOrderMenu.SelectedRows[0].DataBoundItem;
                currentActiveOrder.AddOrderItem(selectedMenuItem, quantity);
                currentOrderItemsBindingSource.DataSource = null;
                currentOrderItemsBindingSource.DataSource = currentActiveOrder.Items;
                currentOrderItemsBindingSource.ResetBindings(false);
                UpdateTotalsUI();
                txtQuantity.Clear();
                MessageBox.Show($"{quantity} x {selectedMenuItem.Name} added to order.", "Item Added", MessageBoxButtons.OK, MessageBoxIcon.Information);
            }
            catch (Exception ex)
            {
                MessageBox.Show($"An error occurred: {ex.Message}", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private void btnLoadOrderForEdit_Click(object sender, EventArgs e)
        {
            if (lbxExistingOrders.SelectedItem == null)
            {
                MessageBox.Show("Please select an order to load from the 'Existing Orders' list.", "Selection Error", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }
            Order selectedOrder = (Order)lbxExistingOrders.SelectedItem;

            if (selectedOrder.Status == OrderStatus.Delivered || selectedOrder.Status == OrderStatus.Cancelled)
            {
                MessageBox.Show($"Order for Table {selectedOrder.TableNumber} is already {selectedOrder.Status} and cannot be edited.", "Order Status", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                currentActiveOrder = null; 
                RefreshUI();
                UpdateTotalsUI();
                txtReceipt.Clear();
                return;
            }

            currentActiveOrder = selectedOrder;
            txtTableNumber.Text = currentActiveOrder.TableNumber.ToString();
            currentOrderItemsBindingSource.DataSource = null;
            currentOrderItemsBindingSource.DataSource = currentActiveOrder.Items;
            currentOrderItemsBindingSource.ResetBindings(false);

            if (currentActiveOrder.Type == OrderType.Delivery)
            {
                cmbOrderType.SelectedItem = "Delivery";
                pnlDeliveryDetails.Visible = true;
                selectedDeliveryCustomer = restaurantManager.GetUserById(currentActiveOrder.CustomerId.GetValueOrDefault());
                if (selectedDeliveryCustomer != null)
                {
                    cmbDeliveryCustomer.SelectedItem = selectedDeliveryCustomer;
                    cmbDeliveryAddress.DataSource = selectedDeliveryCustomer.DeliveryAddresses;
                    cmbDeliveryAddress.DisplayMember = "Street";
                    cmbDeliveryAddress.ValueMember = "Id";
                    selectedDeliveryAddress = selectedDeliveryCustomer.DeliveryAddresses.FirstOrDefault(a => a.Id == currentActiveOrder.DeliveryAddressId);
                    if (selectedDeliveryAddress != null)
                    {
                        cmbDeliveryAddress.SelectedItem = selectedDeliveryAddress;
                    }
                }
            }
            else
            {
                cmbOrderType.SelectedItem = "Pickup";
                pnlDeliveryDetails.Visible = false;
                cmbDeliveryCustomer.SelectedIndex = -1;
                selectedDeliveryCustomer = null;
                cmbDeliveryAddress.DataSource = new List<Address>();
                cmbDeliveryAddress.SelectedIndex = -1;
                selectedDeliveryAddress = null;
            }

            RefreshUI(); 
            UpdateTotalsUI();
            txtReceipt.Clear();
            MessageBox.Show($"Order for Table {currentActiveOrder.TableNumber} loaded for editing. Status: {currentActiveOrder.Status}", "Order Loaded", MessageBoxButtons.OK, MessageBoxIcon.Information);
        }

        private void btnCancelOrder_Click(object sender, EventArgs e)
        {
            if (lbxExistingOrders.SelectedItem == null)
            {
                MessageBox.Show("Please select an order to cancel.", "Selection Error", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }
            Order orderToCancel = (Order)lbxExistingOrders.SelectedItem;
            if (orderToCancel.Status == OrderStatus.Delivered) 
            {
                MessageBox.Show("Cannot cancel an order that has already been delivered.", "Cancellation Error", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            DialogResult result = MessageBox.Show("Are you sure you want to cancel the selected order?", "Confirm Cancellation", MessageBoxButtons.YesNo, MessageBoxIcon.Question);
            if (result == DialogResult.No)
            {
                return;
            }

            orderToCancel.IsCompleted = true; 
            orderToCancel.Status = OrderStatus.Cancelled; 
            restaurantManager.SaveData(); 

            if (currentActiveOrder != null && currentActiveOrder.Id == orderToCancel.Id)
            {
                currentActiveOrder = null;
                currentOrderItemsBindingSource.DataSource = new List<OrderItem>();
            }
            RefreshUI();
            UpdateTotalsUI();
            MessageBox.Show($"Order for Table {orderToCancel.TableNumber} has been cancelled.", "Order Cancelled", MessageBoxButtons.OK, MessageBoxIcon.Information);
        }

        private void btnCompleteOrder_Click(object sender, EventArgs e)
        {
            if (lbxExistingOrders.SelectedItem == null)
            {
                MessageBox.Show("Please select an order to complete.", "Selection Error", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }
            Order orderToComplete = (Order)lbxExistingOrders.SelectedItem;
            if (orderToComplete.Status == OrderStatus.Delivered || orderToComplete.Status == OrderStatus.Cancelled) 
            {
                MessageBox.Show($"Order for Table {orderToComplete.TableNumber} is already {orderToComplete.Status}.", "Order Status", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            DialogResult result = MessageBox.Show($"Are you sure you want to complete (mark as paid) order for Table {orderToComplete.TableNumber}? Total: {orderToComplete.GetTotalBill().ToString("C", UsCulture)}", "Confirm Completion", MessageBoxButtons.YesNo, MessageBoxIcon.Question);
            if (result == DialogResult.No)
            {
                return;
            }

            orderToComplete.Status = OrderStatus.Delivered; 
            if (restaurantManager.CompleteOrder(orderToComplete.Id))
            {
                if (currentActiveOrder != null && currentActiveOrder.Id == orderToComplete.Id)
                {
                    currentActiveOrder = null;
                    currentOrderItemsBindingSource.DataSource = new List<OrderItem>();
                }
                RefreshUI();
                UpdateTotalsUI();
                MessageBox.Show($"Order for Table {orderToComplete.TableNumber} has been completed and removed from active orders.", "Order Completed", MessageBoxButtons.OK, MessageBoxIcon.Information);
                txtReceipt.Clear();
            }
            else
            {
                MessageBox.Show("Failed to complete order. Order not found.", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private void btnShowReceipt_Click(object sender, EventArgs e)
        {
            if (currentActiveOrder == null)
            {
                MessageBox.Show("Please create or load an order to show a receipt.", "No Active Order", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }
            System.Text.StringBuilder receipt = new System.Text.StringBuilder();
            receipt.AppendLine("-RESTAURANT RECEIPT-");
            receipt.AppendLine($"Order ID: {currentActiveOrder.Id.ToString().Substring(0, 8)}");
            receipt.AppendLine($"Order number: {currentActiveOrder.TableNumber}");
            receipt.AppendLine($"Order type: {currentActiveOrder.Type}");
            receipt.AppendLine($"Order status: {currentActiveOrder.Status}"); 
            if (currentActiveOrder.Type == OrderType.Delivery)
            {
                User customer = restaurantManager.GetUserById(currentActiveOrder.CustomerId.GetValueOrDefault());
                Address address = customer?.DeliveryAddresses.FirstOrDefault(a => a.Id == currentActiveOrder.DeliveryAddressId);
                receipt.AppendLine($"Customer: {customer?.Username}");
                receipt.AppendLine($"Delivery address: {address?.ToString()}");
            }
            receipt.AppendLine($"Order time: {currentActiveOrder.OrderTime:g}");
            receipt.AppendLine("--------------------------------------------");
            receipt.AppendLine("Items:");
            foreach (var item in currentActiveOrder.Items)
            {
                receipt.AppendLine($"- {item.Item.Name} x{item.Quantity} @ {item.Item.Price.ToString("C", UsCulture)} = {item.GetTotalPrice().ToString("C", UsCulture)}");
            }
            receipt.AppendLine("--------------------------------------------");
            receipt.AppendLine($"Order total: {currentActiveOrder.Items.Sum(oi => oi.GetTotalPrice()).ToString("C", UsCulture)}");
            receipt.AppendLine($"Delivery cost: {currentActiveOrder.DeliveryCost.ToString("C", UsCulture)}");
            receipt.AppendLine($"Final total: {currentActiveOrder.GetTotalBill().ToString("C", UsCulture)}");
            receipt.AppendLine("---------- THANK YOU! ----------");
            txtReceipt.Text = receipt.ToString();
        }

        //user management and address 

        private void dgvUser_SelectionChanged(object sender, EventArgs e)
        {
            if (dgvUser.SelectedRows.Count > 0)
            {
                if (dgvUser.SelectedRows[0].DataBoundItem is User selectedUser)
                {
                    currentUser = selectedUser;
                    txtUsername.Text = currentUser.Username;
                    txtPassword.Text = currentUser.PasswordHash;
                    cmbRole.SelectedItem = currentUser.Role;

                    userAddressesBindingSource.DataSource = null;
                    userAddressesBindingSource.DataSource = currentUser.DeliveryAddresses;
                    userAddressesBindingSource.ResetBindings(false);
                    ClearAddressFields();
                }
                else
                {
                    currentUser = null;
                    ClearUserFields();
                }
            }
            else
            {
                currentUser = null;
                ClearUserFields();
            }
        }

        private void btnAddUser_Click(object sender, EventArgs e)
        {
            try
            {
                if (string.IsNullOrWhiteSpace(txtUsername.Text) || string.IsNullOrWhiteSpace(txtPassword.Text) || cmbRole.SelectedItem == null)
                {
                    MessageBox.Show("Username, Password, and Role cannot be empty.", "Input Error", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                    return;
                }
                User newUser = new User(txtUsername.Text, txtPassword.Text, cmbRole.SelectedItem.ToString());
                if (restaurantManager.AddUser(newUser))
                {
                    RefreshUI();
                    ClearUserFields();
                    MessageBox.Show("User added successfully!", "Success", MessageBoxButtons.OK, MessageBoxIcon.Information);
                }
                else
                {
                    MessageBox.Show("User with this username already exists.", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"An error occurred: {ex.Message}", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private void btnSaveUserChanges_Click(object sender, EventArgs e)
        {
            if (currentUser == null)
            {
                MessageBox.Show("Please select a user to edit.", "Selection Error", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }
            try
            {
                if (string.IsNullOrWhiteSpace(txtUsername.Text) || string.IsNullOrWhiteSpace(txtPassword.Text) || cmbRole.SelectedItem == null)
                {
                    MessageBox.Show("Username, Password, and Role cannot be empty.", "Input Error", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                    return;
                }
                User updatedUser = new User(txtUsername.Text, txtPassword.Text, cmbRole.SelectedItem.ToString())
                {
                    Id = currentUser.Id
                };
                if (restaurantManager.EditUser(updatedUser))
                {
                    RefreshUI();
                    ClearUserFields();
                    MessageBox.Show("User updated successfully!", "Success", MessageBoxButtons.OK, MessageBoxIcon.Information);
                }
                else
                {
                    MessageBox.Show("Failed to update user. User not found.", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"An error occurred: {ex.Message}", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private void btnDeleteUser_Click(object sender, EventArgs e)
        {
            if (currentUser == null)
            {
                MessageBox.Show("Please select a user to delete.", "Selection Error", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }
            DialogResult result = MessageBox.Show($"Are you sure you want to delete user '{currentUser.Username}'?", "Confirm Deletion", MessageBoxButtons.YesNo, MessageBoxIcon.Question);
            if (result == DialogResult.No)
            {
                return;
            }
            try
            {
                if (restaurantManager.DeleteUser(currentUser.Id))
                {
                    RefreshUI();
                    ClearUserFields();
                    MessageBox.Show("User deleted successfully!", "Success", MessageBoxButtons.OK, MessageBoxIcon.Information);
                }
                else
                {
                    MessageBox.Show("Failed to delete user. User not found.", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"An error occurred: {ex.Message}", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private void btnClearUserFields_Click(object sender, EventArgs e)
        {
            ClearUserFields();
        }

        private void ClearUserFields()
        {
            txtUsername.Clear();
            txtPassword.Clear();
            cmbRole.SelectedIndex = 0; 

            if (dgvUser.Rows.Count > 0)
            {
                dgvUser.ClearSelection();
            }
            currentUser = null;
            userAddressesBindingSource.DataSource = new List<Address>();
            userAddressesBindingSource.ResetBindings(false);
            ClearAddressFields();
        }

        private void lbxUserAddresses_SelectionChanged(object sender, EventArgs e)
        {
            if (lbxUserAddresses.SelectedItem != null)
            {
                Address selectedAddress = (Address)lbxUserAddresses.SelectedItem;
                txtStreet.Text = selectedAddress.Street;
                txtCity.Text = selectedAddress.City;
                txtPostalCode.Text = selectedAddress.PostalCode;
                
            }
            else
            {
                ClearAddressFields();
            }
        }

        private void btnAddAddress_Click(object sender, EventArgs e)
        {
            if (currentUser == null)
            {
                MessageBox.Show("Please select a user first to add an address.", "Selection Error", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }
            try
            {
                if (string.IsNullOrWhiteSpace(txtStreet.Text) || string.IsNullOrWhiteSpace(txtCity.Text) || string.IsNullOrWhiteSpace(txtPostalCode.Text))
                {
                    MessageBox.Show("Street, City, and Postal Code cannot be empty for an address.", "Input Error", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                    return;
                }
                Address newAddress = new Address(txtStreet.Text, txtCity.Text, txtPostalCode.Text);
                currentUser.AddAddress(newAddress);
                RefreshUI();
                ClearAddressFields();
                MessageBox.Show("Address added successfully!", "Success", MessageBoxButtons.OK, MessageBoxIcon.Information);
            }
            catch (Exception ex)
            {
                MessageBox.Show($"An error occurred: {ex.Message}", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private void btnSaveAddressChanges_Click(object sender, EventArgs e)
        {
            if (currentUser == null)
            {
                MessageBox.Show("Please select a user first.", "Selection Error", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }
            if (lbxUserAddresses.SelectedItem == null)
            {
                MessageBox.Show("Please select an address to edit.", "Selection Error", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }
            try
            {
                Address selectedAddress = (Address)lbxUserAddresses.SelectedItem;
                Guid addressIdToEdit = selectedAddress.Id;
                if (string.IsNullOrWhiteSpace(txtStreet.Text) || string.IsNullOrWhiteSpace(txtCity.Text) || string.IsNullOrWhiteSpace(txtPostalCode.Text))
                {
                    MessageBox.Show("Street, City, and Postal Code cannot be empty for an address.", "Input Error", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                    return;
                }
                Address updatedAddress = new Address(txtStreet.Text, txtCity.Text, txtPostalCode.Text)
                {
                    Id = addressIdToEdit
                };
                if (currentUser.UpdateAddress(updatedAddress))
                {
                    RefreshUI();
                    ClearAddressFields();
                    MessageBox.Show("Address updated successfully!", "Success", MessageBoxButtons.OK, MessageBoxIcon.Information);
                }
                else
                {
                    MessageBox.Show("Failed to update address. Address not found.", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"An error occurred: {ex.Message}", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private void btnDeleteAddress_Click(object sender, EventArgs e)
        {
            if (currentUser == null)
            {
                MessageBox.Show("Please select a user first.", "Selection Error", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }
            if (lbxUserAddresses.SelectedItem == null)
            {
                MessageBox.Show("Please select an address to delete.", "Selection Error", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }
            DialogResult result = MessageBox.Show($"Are you sure you want to delete this address?", "Confirm Deletion", MessageBoxButtons.YesNo, MessageBoxIcon.Question);
            if (result == DialogResult.No)
            {
                return;
            }
            try
            {
                Address selectedAddress = (Address)lbxUserAddresses.SelectedItem;
                if (currentUser.DeleteAddress(selectedAddress.Id))
                {
                    RefreshUI();
                    ClearAddressFields();
                    MessageBox.Show("Address deleted successfully!", "Success", MessageBoxButtons.OK, MessageBoxIcon.Information);
                }
                else
                {
                    MessageBox.Show("Failed to delete address. Address not found.", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"An error occurred: {ex.Message}", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private void btnClearAddressFields_Click(object sender, EventArgs e)
        {
            ClearAddressFields();
        }

        private void ClearAddressFields()
        {
            txtStreet.Clear();
            txtCity.Clear();
            txtPostalCode.Clear();
            
            if (lbxUserAddresses.Items.Count > 0)
            {
                lbxUserAddresses.ClearSelected();
            }
        }
        private void cmbOrderType_SelectedIndexChanged(object sender, EventArgs e)
        {
            if (cmbOrderType.SelectedItem?.ToString() == "Delivery")
            {
                pnlDeliveryDetails.Visible = true;
                Console.WriteLine($"[DEBUG] cmbOrderType_SelectedIndexChanged: Selected 'Delivery'. pnlDeliveryDetails.Visible = {pnlDeliveryDetails.Visible}");
                cmbDeliveryCustomer.SelectedIndex = -1;
                selectedDeliveryCustomer = null;
                cmbDeliveryAddress.DataSource = new List<Address>();
                cmbDeliveryAddress.SelectedIndex = -1;
                selectedDeliveryAddress = null;

                if (currentActiveOrder != null)
                {
                    currentActiveOrder.Type = OrderType.Delivery;
                    currentActiveOrder.SetDeliveryCost(0m);
                }
            }
            else 
            {
                pnlDeliveryDetails.Visible = false;
                Console.WriteLine($"[DEBUG] cmbOrderType_SelectedIndexChanged: Selected 'Pickup'. pnlDeliveryDetails.Visible = {pnlDeliveryDetails.Visible}");
                cmbDeliveryCustomer.SelectedIndex = -1;
                selectedDeliveryCustomer = null;
                cmbDeliveryAddress.DataSource = new List<Address>();
                cmbDeliveryAddress.SelectedIndex = -1;
                selectedDeliveryAddress = null;

                if (currentActiveOrder != null)
                {
                    currentActiveOrder.Type = OrderType.Pickup;
                    currentActiveOrder.SetDeliveryCost(0m);
                }
            }
            UpdateTotalsUI(); 
        }

        private void cmbDeliveryCustomer_SelectedIndexChanged(object sender, EventArgs e)
        {
            if (cmbDeliveryCustomer.SelectedItem != null)
            {
                selectedDeliveryCustomer = (User)cmbDeliveryCustomer.SelectedItem;
                cmbDeliveryAddress.DataSource = selectedDeliveryCustomer.DeliveryAddresses;
                cmbDeliveryAddress.DisplayMember = "Street";
                cmbDeliveryAddress.ValueMember = "Id";
                cmbDeliveryAddress.SelectedIndex = -1; 
                selectedDeliveryAddress = null;
            }
            else
            {
                selectedDeliveryCustomer = null;
                selectedDeliveryAddress = null;
                cmbDeliveryAddress.DataSource = new List<Address>();
            }

            if (currentActiveOrder != null && currentActiveOrder.Type == OrderType.Delivery)
            {
                currentActiveOrder.SetDeliveryCost(0m);
                if (selectedDeliveryAddress != null) 
                {
                    currentActiveOrder.SetDeliveryCost(restaurantManager.GetDeliveryCost(selectedDeliveryAddress));
                }
                currentActiveOrder.CustomerId = selectedDeliveryCustomer?.Id;
            }
            UpdateTotalsUI(); 
        }

        private void cmbDeliveryAddress_SelectedIndexChanged(object sender, EventArgs e)
        {
            if (cmbDeliveryAddress.SelectedItem != null)
            {
                selectedDeliveryAddress = (Address)cmbDeliveryAddress.SelectedItem;
            }
            else
            {
                selectedDeliveryAddress = null;
            }

            if (currentActiveOrder != null && currentActiveOrder.Type == OrderType.Delivery)
            {
                if (selectedDeliveryAddress != null)
                {
                    currentActiveOrder.SetDeliveryCost(restaurantManager.GetDeliveryCost(selectedDeliveryAddress));
                }
                else
                {
                    currentActiveOrder.SetDeliveryCost(0m); 
                }
                currentActiveOrder.DeliveryAddressId = selectedDeliveryAddress?.Id;
            }
            UpdateTotalsUI(); 
        }

        private void btnUpdateOrderStatus_Click(object sender, EventArgs e) 
        {
            if (currentActiveOrder == null)
            {
                MessageBox.Show("Please load an order to update its status.", "No Order Loaded", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }
            if (cmbOrderStatus.SelectedItem == null)
            {
                MessageBox.Show("Please select a new status from the dropdown.", "No Status Selected", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            OrderStatus newStatus;
            if (Enum.TryParse(cmbOrderStatus.SelectedItem.ToString(), out newStatus))
            {
                if (currentActiveOrder.Status == OrderStatus.Delivered || currentActiveOrder.Status == OrderStatus.Cancelled)
                {
                    MessageBox.Show($"Cannot change status of an order that is already {currentActiveOrder.Status}.", "Invalid Status Change", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                    cmbOrderStatus.SelectedItem = currentActiveOrder.Status.ToString();
                    return;
                }

                if (currentActiveOrder.Status == OrderStatus.InDelivery && newStatus != OrderStatus.Delivered && newStatus != OrderStatus.Cancelled)
                {
                    MessageBox.Show("Order is in delivery. It can only be marked as Delivered or Cancelled.", "Invalid Status Change", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                    cmbOrderStatus.SelectedItem = currentActiveOrder.Status.ToString();
                    return;
                }

                if (currentActiveOrder.Type == OrderType.Delivery && newStatus == OrderStatus.InDelivery)
                {
                    if (currentActiveOrder.CustomerId == null || currentActiveOrder.DeliveryAddressId == null)
                    {
                        MessageBox.Show("Cannot set order to 'In Delivery' without a selected customer and delivery address.", "Missing Delivery Info", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                        cmbOrderStatus.SelectedItem = currentActiveOrder.Status.ToString();
                        return;
                    }
                }

                currentActiveOrder.Status = newStatus;
                if (newStatus == OrderStatus.Delivered || newStatus == OrderStatus.Cancelled)
                {
                    currentActiveOrder.IsCompleted = true;
                }
                else
                {
                    currentActiveOrder.IsCompleted = false;
                }

                restaurantManager.SaveData(); 
                RefreshUI(); 
                UpdateTotalsUI();
                MessageBox.Show($"Order status updated to: {newStatus}", "Status Updated", MessageBoxButtons.OK, MessageBoxIcon.Information);
            }
            else
            {
                MessageBox.Show("Invalid status selected.", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private void TabControlMain_SelectedIndexChanged(object sender, EventArgs e)
        {
            Console.WriteLine($"[DEBUG] TabControlMain_SelectedIndexChanged: Selected tab index: {tabControlMain.SelectedIndex}");
            Console.WriteLine($"[DEBUG] TabControlMain_SelectedIndexChanged: Selected tab name: {tabControlMain.SelectedTab?.Name}");
            Console.WriteLine($"[DEBUG] TabControlMain_SelectedIndexChanged: Selected tab text: {tabControlMain.SelectedTab?.Text}");

            RefreshUI();
            UpdateTotalsUI();

            if (tabControlMain.SelectedTab != null && tabControlMain.SelectedTab.Name == "tabPageGallery")
            {
                Console.WriteLine("[DEBUG] Tab 'Food Gallery' selected. Loading gallery...");
                LoadFoodGallery();
            }
            else
            {
                Console.WriteLine($"[DEBUG] Selected tab is NOT 'Food Gallery'. Current tab name: {tabControlMain.SelectedTab?.Name}");
            }
        }

        private void LoadFoodGallery()
        {
            Console.WriteLine("[DEBUG] Entering LoadFoodGallery method.");
            flowLayoutPanelGallery.Controls.Clear(); 
            Console.WriteLine("[DEBUG] flowLayoutPanelGallery cleared.");

            if (!restaurantManager.MenuItems.Any())
            {
                Console.WriteLine("[DEBUG] No menu items found to display in gallery.");
                return;
            }

            foreach (MenuItem item in restaurantManager.MenuItems)
            {
                Console.WriteLine($"[DEBUG] Processing menu item: {item.Name}. ImageUrl: {item.ImageUrl}");

                if (!string.IsNullOrEmpty(item.ImageUrl))
                {
                    Panel itemPanel = new Panel();
                    itemPanel.Size = new Size(220, 220); 
                    itemPanel.Margin = new Padding(10);
                    itemPanel.BorderStyle = BorderStyle.FixedSingle;
                    itemPanel.BackColor = SystemColors.ControlLightLight;

                    PictureBox pictureBox = new PictureBox();
                    pictureBox.Size = new Size(200, 150);
                    pictureBox.Location = new Point(10, 10); 
                    pictureBox.SizeMode = PictureBoxSizeMode.Zoom;
                    pictureBox.BorderStyle = BorderStyle.FixedSingle;

                    pictureBox.LoadCompleted += (s, ev) =>
                    {
                        if (ev.Error != null)
                        {
                            Console.WriteLine($"[DEBUG] ERROR loading image for {item.Name} from {item.ImageUrl}: {ev.Error.Message}");
                            try
                            {
                                
                            }
                            catch (Exception resourceEx)
                            {
                                Console.WriteLine($"[DEBUG] CRITICAL ERROR: Could not set error image. Check Properties.Resources.placeholder_error: {resourceEx.Message}");
                            }
                        }
                        else
                        {
                            Console.WriteLine($"[DEBUG] Successfully loaded image for {item.Name} from {item.ImageUrl}.");
                        }
                    };

                    try
                    {
                        Console.WriteLine($"[DEBUG] Attempting to load image for {item.Name} from {item.ImageUrl} asynchronously...");
                        pictureBox.LoadAsync(item.ImageUrl);                       
                        pictureBox.ImageLocation = item.ImageUrl; 
                    }
                    catch (Exception ex)
                    {
                        Console.WriteLine($"[DEBUG] EXCEPTION during pictureBox.LoadAsync for {item.Name}: {ex.Message}");

                        try
                        {
                            
                        }
                        catch (Exception resourceEx)
                        {
                            Console.WriteLine($"[DEBUG] CRITICAL ERROR: Could not set error image after exception. Check Properties.Resources.placeholder_error: {resourceEx.Message}");
                        }
                    }

                    Label nameLabel = new Label();
                    nameLabel.Text = item.Name;
                    nameLabel.AutoSize = false;
                    nameLabel.TextAlign = ContentAlignment.MiddleCenter;
                    nameLabel.Font = new Font("Segoe UI", 10, FontStyle.Bold);
                    nameLabel.Location = new Point(10, 170); 
                    nameLabel.Size = new Size(200, 20);

                    Label priceLabel = new Label();
                    priceLabel.Text = item.Price.ToString("C", UsCulture);
                    priceLabel.AutoSize = false;
                    priceLabel.TextAlign = ContentAlignment.MiddleCenter;
                    priceLabel.Font = new Font("Segoe UI", 9);
                    priceLabel.Location = new Point(10, 190);
                    priceLabel.Size = new Size(200, 20);


                    itemPanel.Controls.Add(pictureBox);
                    itemPanel.Controls.Add(nameLabel);
                    itemPanel.Controls.Add(priceLabel);

                    flowLayoutPanelGallery.Controls.Add(itemPanel);
                    Console.WriteLine($"[DEBUG] Added panel for {item.Name} to flowLayoutPanelGallery.");
                }
                else
                {
                    Console.WriteLine($"[DEBUG] Skipping item {item.Name} because ImageUrl is empty or null.");
                }
            }
            Console.WriteLine("[DEBUG] Exiting LoadFoodGallery method.");
        }

        private void txtPostalCode_TextChanged(object sender, EventArgs e)
        {

        }

        private void label12_Click(object sender, EventArgs e)
        {

        }
    } 

    // domain model classes

    public class MenuItem
    {
        public Guid Id { get; set; }
        public string Name { get; set; }
        public string Description { get; set; }
        public decimal Price { get; set; }
        public string Category { get; set; }
        public string ImageUrl { get; set; }
        public List<Review> Reviews { get; set; } 

        public MenuItem(string name, string description, decimal price, string category, string imageUrl = "")
        {
            Id = Guid.NewGuid();
            Name = name;
            Description = description;
            Price = price;
            Category = category;
            ImageUrl = imageUrl;
            Reviews = new List<Review>(); 
        }

        public override string ToString()
        {
            return $"{Name} ({Category}) - {Price.ToString("C", Form1.UsCulture)}";
        }

        public double GetAverageScore()
        {
            if (Reviews == null || !Reviews.Any())
            {
                return 0.0;
            }
            return Reviews.Average(r => r.Score);
        }
    }

    public class OrderItem
    {
        public MenuItem Item { get; set; }
        public int Quantity { get; set; }

        public OrderItem(MenuItem item, int quantity)
        {
            Item = item;
            Quantity = quantity;
        }

        public decimal GetTotalPrice()
        {
            return Item.Price * Quantity;
        }

        public decimal TotalPriceDisplay
        {
            get { return GetTotalPrice(); }
        }

        public string ItemNameDisplay
        {
            get { return Item?.Name; }
        }

        public decimal ItemPriceDisplay
        {
            get { return Item?.Price ?? 0m; }
        }
    }

    public class Order
    {
        public Guid Id { get; set; }
        public int TableNumber { get; set; }
        public DateTime OrderTime { get; set; }
        public List<OrderItem> Items { get; set; }
        public bool IsCompleted { get; set; }

        public OrderType Type { get; set; }
        public Guid? CustomerId { get; set; }
        public Guid? DeliveryAddressId { get; set; }
        public decimal DeliveryCost { get; private set; }

        public OrderStatus Status { get; set; }

        public Order(int tableNumber)
        {
            Id = Guid.NewGuid();
            TableNumber = tableNumber;
            OrderTime = DateTime.Now;
            Items = new List<OrderItem>();
            IsCompleted = false;
            Type = OrderType.Pickup; 
            DeliveryCost = 0m;
            Status = OrderStatus.Confirmed; 
        }

        public void SetDeliveryCost(decimal cost)
        {
            DeliveryCost = cost;
        }

        public decimal GetTotalBill()
        {
            return Items.Sum(oi => oi.GetTotalPrice()) + DeliveryCost;
        }

        public void AddOrderItem(MenuItem item, int quantity)
        {
            var existingItem = Items.FirstOrDefault(oi => oi.Item.Id == item.Id);
            if (existingItem != null)
            {
                existingItem.Quantity += quantity;
            }
            else
            {
                Items.Add(new OrderItem(item, quantity));
            }
        }

        public void RemoveOrderItem(MenuItem item, int quantityToRemove)
        {
            var existingItem = Items.FirstOrDefault(oi => oi.Item.Id == item.Id);
            if (existingItem != null)
            {
                existingItem.Quantity -= quantityToRemove;
                if (existingItem.Quantity <= 0)
                {
                    Items.Remove(existingItem);
                }
            }
        }
    }

    public enum OrderType
    {
        Pickup,
        Delivery
    }

    public enum OrderStatus
    {
        Confirmed,    
        Preparing,    
        InDelivery,   
        Delivered,    
        Cancelled     
    }

    public class Review
    {
        public Guid Id { get; set; }
        public Guid UserId { get; set; } 
        public string Username { get; set; } 
        public int Score { get; set; }       
        public string Comment { get; set; } 
        public DateTime ReviewDate { get; set; } 

        public Review(Guid userId, string username, int score, string comment)
        {
            Id = Guid.NewGuid();
            UserId = userId;
            Username = username;
            Score = score;
            Comment = comment;
            ReviewDate = DateTime.Now;
        }

        public override string ToString()
        {
            return $"{Username} ({Score}/5): {Comment}";
        }
    }

    public class Address
    {
        public Guid Id { get; set; }
        public string Street { get; set; }
        public string City { get; set; }
        public string PostalCode { get; set; }
        public string Notes { get; set; }

        public Address(string street, string city, string postalCode, string notes = "")
        {
            Id = Guid.NewGuid();
            Street = street;
            City = city;
            PostalCode = postalCode;
            Notes = notes;
        }

        public override string ToString()
        {
            return $"{Street}, {City}, {PostalCode}";
        }
    }

    public class User
    {
        public Guid Id { get; set; }
        public string Username { get; set; }
        public string PasswordHash { get; set; }
        public string Role { get; set; }
        public List<Address> DeliveryAddresses { get; set; }

        public User(string username, string password, string role = "Customer")
        {
            Id = Guid.NewGuid();
            Username = username;
            PasswordHash = password;
            Role = role;
            DeliveryAddresses = new List<Address>();
        }

        public void AddAddress(Address address)
        {
            if (!DeliveryAddresses.Any(a => a.Id == address.Id))
            {
                DeliveryAddresses.Add(address);
            }
        }

        public bool UpdateAddress(Address updatedAddress)
        {
            var existingAddress = DeliveryAddresses.FirstOrDefault(a => a.Id == updatedAddress.Id);
            if (existingAddress != null)
            {
                existingAddress.Street = updatedAddress.Street;
                existingAddress.City = updatedAddress.City;
                existingAddress.PostalCode = updatedAddress.PostalCode;
                existingAddress.Notes = updatedAddress.Notes;
                return true;
            }
            return false;
        }

        public bool DeleteAddress(Guid addressId)
        {
            var addressToRemove = DeliveryAddresses.FirstOrDefault(a => a.Id == addressId);
            if (addressToRemove != null)
            {
                DeliveryAddresses.Remove(addressToRemove);
                return true;
            }
            return false;
        }

        public override string ToString()
        {
            return $"{Username} ({Role})";
        }
    }

    public class RestaurantManager
    {
        public List<MenuItem> MenuItems { get; private set; }
        public List<Order> Orders { get; private set; }
        public List<User> Users { get; private set; }

        public RestaurantManager()
        {
            MenuItems = new List<MenuItem>();
            Orders = new List<Order>();
            Users = new List<User>();
        }

        private const string DataFilePath = "restaurant_data.json";

        public void SaveData()
        {
            try
            {
                var dataToSave = new
                {
                    MenuItems = this.MenuItems,
                    Orders = this.Orders,
                    Users = this.Users
                };
                string json = JsonConvert.SerializeObject(dataToSave, Formatting.Indented);
                File.WriteAllText(DataFilePath, json);
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error saving data: {ex.Message}");
                MessageBox.Show($"Error saving data: {ex.Message}", "Save Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        public void LoadData()
        {
            try
            {
                if (File.Exists(DataFilePath))
                {
                    string json = File.ReadAllText(DataFilePath);
                    var loadedData = JsonConvert.DeserializeAnonymousType(json, new
                    {
                        MenuItems = new List<MenuItem>(),
                        Orders = new List<Order>(),
                        Users = new List<User>()
                    });
                    this.MenuItems.Clear();
                    if (loadedData.MenuItems != null) { this.MenuItems.AddRange(loadedData.MenuItems); }
                    this.Orders.Clear();
                    if (loadedData.Orders != null) { this.Orders.AddRange(loadedData.Orders); }
                    this.Users.Clear();
                    if (loadedData.Users != null) { this.Users.AddRange(loadedData.Users); }

                    if (!MenuItems.Any() && !Orders.Any() && !Users.Any())
                    {
                        InitializeSampleData();
                    }
                }
                else
                {
                    InitializeSampleData();
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error loading data: {ex.Message}. Initializing with sample data.");
                MessageBox.Show($"Error loading data: {ex.Message}. Initializing with sample data.", "Load Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                InitializeSampleData();
            }
        }

        private readonly Dictionary<string, decimal> DeliveryCostsByCity = new Dictionary<string, decimal>(StringComparer.OrdinalIgnoreCase)
        {
            { "Poznan", 5.00m },
            { "Gdansk", 8.50m },
            { "Default", 7.00m }
        };

        public decimal GetDeliveryCost(Address address)
        {
            if (address == null) return 0m;

            if (DeliveryCostsByCity.TryGetValue(address.City, out decimal cost))
            {
                return cost;
            }

            return DeliveryCostsByCity["Default"];
        }

        public void AddMenuItem(MenuItem item) { MenuItems.Add(item); }
        public bool DeleteMenuItem(Guid itemId) { var itemToRemove = MenuItems.FirstOrDefault(mi => mi.Id == itemId); if (itemToRemove != null) { MenuItems.Remove(itemToRemove); return true; } return false; }
        public bool EditMenuItem(MenuItem updatedItem) { var existingItem = MenuItems.FirstOrDefault(mi => mi.Id == updatedItem.Id); if (existingItem != null) { existingItem.Name = updatedItem.Name; existingItem.Description = updatedItem.Description; existingItem.Price = updatedItem.Price; existingItem.Category = updatedItem.Category; existingItem.ImageUrl = updatedItem.ImageUrl; existingItem.Reviews = updatedItem.Reviews; return true; } return false; }
        public List<MenuItem> SearchMenuItems(string searchTerm) { if (string.IsNullOrWhiteSpace(searchTerm)) { return MenuItems; } return MenuItems.Where(mi => mi.Name.ToLower().Contains(searchTerm.ToLower()) || mi.Description.ToLower().Contains(searchTerm.ToLower()) || mi.Category.ToLower().Contains(searchTerm.ToLower())).ToList(); }
        public Order CreateNewOrder(int tableNumber) { var newOrder = new Order(tableNumber); Orders.Add(newOrder); return newOrder; }
        public Order GetOrderById(Guid orderId) { return Orders.FirstOrDefault(o => o.Id == orderId); }

        public bool CancelOrder(Guid orderId) { var orderToUpdate = Orders.FirstOrDefault(o => o.Id == orderId); if (orderToUpdate != null) { orderToUpdate.Status = OrderStatus.Cancelled; orderToUpdate.IsCompleted = true; return true; } return false; }

        public bool CompleteOrder(Guid orderId) { var orderToUpdate = Orders.FirstOrDefault(o => o.Id == orderId); if (orderToUpdate != null) { orderToUpdate.IsCompleted = true; orderToUpdate.Status = OrderStatus.Delivered; return true; } return false; }


        public bool AddUser(User user) { if (Users.Any(u => u.Username.Equals(user.Username, StringComparison.OrdinalIgnoreCase))) { return false; } Users.Add(user); return true; }
        public User GetUserById(Guid userId) { return Users.FirstOrDefault(u => u.Id == userId); }
        public User FindUserByUsername(string username) { return Users.FirstOrDefault(u => u.Username.Equals(username, StringComparison.OrdinalIgnoreCase)); }
        public User AuthenticateUser(string username, string password) { User user = Users.FirstOrDefault(u => u.Username.Equals(username, StringComparison.OrdinalIgnoreCase) && u.PasswordHash == password); return user; }
        public bool EditUser(User updatedUser) { var existingUser = Users.FirstOrDefault(u => u.Id == updatedUser.Id); if (existingUser != null) { existingUser.Username = updatedUser.Username; existingUser.PasswordHash = updatedUser.PasswordHash; existingUser.Role = updatedUser.Role; return true; } return false; }
        public bool DeleteUser(Guid userId) { var userToRemove = Users.FirstOrDefault(u => u.Id == userId); if (userToRemove != null) { Users.Remove(userToRemove); return true; } return false; }

        private void InitializeSampleData()
        {
            var burger = new MenuItem("Classic Burger", "Juicy beef patty with lettuce, tomato, onion, and cheese.", 15.50m, "Main courses", "https://gotujezlewiatanem.pl/wp-content/uploads/2022/05/AdobeStock_258433064-kopia.jpeg");
            var salad = new MenuItem("Caesar Salad", "Fresh romaine lettuce, croutons, parmesan cheese, and Caesar dressing.", 11.00m, "Appetizers", "https://www.seriouseats.com/thmb/Fi_FEyVa3_-_uzfXh6OdLrzal2M=/1500x0/filters:no_upscale():max_bytes(150000):strip_icc()/the-best-caesar-salad-recipe-06-40e70f549ba2489db09355abd62f79a9.jpg");
            var fries = new MenuItem("French Fries", "Crispy golden french fries, perfectly salted.", 4.00m, "Sides", "https://encrypted-tbn0.gstatic.com/images?q=tbn:ANd9GcSYOFvOMlVJiBKvxYuwjBGzEx55zkA-I1dWFQ&s");
            var cheesecake = new MenuItem("Cheesecake", "Creamy cheesecake with berry topping.", 6.75m, "Desserts", "https://www.simplyrecipes.com/thmb/QAY2WlJ6xMQMY6vrLVrlgZe7sfk=/1500x0/filters:no_upscale():max_bytes(150000):strip_icc()/Simply-Recipes-Perfect-Cheesecake-LEAD-6-97a8cb3a60c24903b883c1d5fb5a69d3.jpg");
            
            Guid customer1Id = new Guid("11111111-1111-1111-1111-111111111111");
            Guid customer2Id = new Guid("22222222-2222-2222-2222-222222222222");
            Guid customer3Id = new Guid("33333333-3333-3333-3333-333333333333");

           
            MenuItems.Add(burger);
            MenuItems.Add(salad);
            MenuItems.Add(fries);
            MenuItems.Add(cheesecake);

            User adminUser = new User("Sofiya", "adminpass", "Admin");
            adminUser.AddAddress(new Address("123 Main St", "Poznan", "12345"));
            adminUser.AddAddress(new Address("456 Oak Ave", "Gdansk", "65432"));
            Users.Add(adminUser);

            User customerUser = new User("Derek", "custpass", "Customer");
            customerUser.Id = customer1Id; 
            customerUser.AddAddress(new Address("789 Pine Ln", "Poznan", "67890"));
            Users.Add(customerUser);
        }
    }
}