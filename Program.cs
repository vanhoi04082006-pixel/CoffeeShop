using System;
using System.Data;
using System.Data.SqlClient;
using System.Drawing;
using System.Windows.Forms;
using Microsoft.VisualBasic;
using System.IO; // Thư viện để đọc/ghi file cấu hình

namespace CoffeeShopManager
{
    static class Program
    {
        [STAThread]
        static void Main()
        {
            Application.EnableVisualStyles();
            Application.SetCompatibleTextRenderingDefault(false);
            Application.Run(new MainForm());
        }
    }

    public class MainForm : Form
    {
        // Biến lưu tên file cấu hình và tên Server
        private string configFilePath = "server_config.txt";
        private string serverName = "";

        private Panel panelSidebar;
        private Panel panelHeader;
        private Panel panelMain;

        private DataGridView dgvData;

        private Label lblHeaderTitle;

        // CRUD BUTTONS
        private Button btnAdd;
        private Button btnEdit;
        private Button btnDelete;
        private Button btnRefresh;

        // Lưu bảng hiện tại
        private string currentTable = "";

        public MainForm()
        {
            InitializeComponent();
            
            // Chạy hàm kiểm tra và cấu hình kết nối ngay khi khởi động form
            EnsureDatabaseConnection();
        }

        // =====================================
        // XỬ LÝ CHUỖI KẾT NỐI ĐỘNG
        // =====================================
        private string GetConnectionString()
        {
            return $@"Server={serverName}; Database=CoffeeShopDB; Trusted_Connection=True; TrustServerCertificate=True;";
        }

        private void EnsureDatabaseConnection()
        {
            bool isConnected = false;

            // 1. Nếu đã có file lưu cấu hình thì đọc từ file và test kết nối
            if (File.Exists(configFilePath))
            {
                serverName = File.ReadAllText(configFilePath).Trim();
                isConnected = TestConnection(GetConnectionString());
            }

            // 2. Vòng lặp yêu cầu nhập Server nếu chưa kết nối được (chưa có file hoặc file sai)
            while (!isConnected)
            {
                serverName = Interaction.InputBox(
                    "Vui lòng nhập tên SQL Server của máy bạn (Ví dụ: .\\SQLEXPRESS hoặc TÊN_MÁY):\n\n(Lưu ý: Nếu nhấn Cancel hoặc để trống, phần mềm sẽ thoát)",
                    "Cấu Hình Database Lần Đầu",
                    serverName);

                // Nếu người dùng bấm Cancel hoặc không nhập gì -> Thoát ứng dụng
                if (string.IsNullOrWhiteSpace(serverName))
                {
                    Environment.Exit(0);
                }

                // Thử kết nối với tên Server vừa nhập
                string tempConnStr = GetConnectionString();
                isConnected = TestConnection(tempConnStr);

                if (isConnected)
                {
                    // Nếu thành công -> Lưu lại vào file text để lần sau không hỏi nữa
                    File.WriteAllText(configFilePath, serverName);
                    MessageBox.Show("Kết nối Database thành công!", "Thông báo", MessageBoxButtons.OK, MessageBoxIcon.Information);
                }
                else
                {
                    // Nếu thất bại -> Báo lỗi và vòng lặp sẽ quay lại hộp thoại nhập
                    MessageBox.Show(
                        "Kết nối thất bại!\n\nVui lòng kiểm tra lại:\n1. Tên Server đã nhập đúng chưa.\n2. SQL Server đang chạy.\n3. Đã có Database tên là 'CoffeeShopDB'.", 
                        "Lỗi kết nối", 
                        MessageBoxButtons.OK, 
                        MessageBoxIcon.Error);
                }
            }
        }

        private bool TestConnection(string connectionString)
        {
            try
            {
                using (SqlConnection conn = new SqlConnection(connectionString))
                {
                    conn.Open();
                    return true;
                }
            }
            catch
            {
                return false;
            }
        }

        private void InitializeComponent()
        {
            // =========================
            // FORM
            // =========================
            this.Text = "Phần Mềm Quản Lý Quán Cà Phê";
            this.Size = new Size(1200, 700);
            this.StartPosition = FormStartPosition.CenterScreen;
            this.Font = new Font("Segoe UI", 10F);
            this.BackColor = Color.FromArgb(245, 245, 245);

            // =========================
            // SIDEBAR
            // =========================
            panelSidebar = new Panel();
            panelSidebar.Dock = DockStyle.Left;
            panelSidebar.Width = 220;
            panelSidebar.BackColor = Color.FromArgb(62, 39, 35);

            Label lblLogo = new Label();
            lblLogo.Text = "☕ COFFEE\nMANAGER";
            lblLogo.ForeColor = Color.White;
            lblLogo.Font = new Font("Segoe UI", 16F, FontStyle.Bold);
            lblLogo.Dock = DockStyle.Top;
            lblLogo.Height = 100;
            lblLogo.TextAlign = ContentAlignment.MiddleCenter;

            panelSidebar.Controls.Add(lblLogo);

            // MENU BUTTONS
            AddMenuButton("Thoát", () => Application.Exit(), DockStyle.Bottom);

            AddMenuButton("Thanh Toán",
                () => LoadTableData("Payments", "Lịch Sử Thanh Toán"),
                DockStyle.Top);

            AddMenuButton("Chi Tiết Đơn",
                () => LoadTableData("OrderDetails", "Chi Tiết Đơn Hàng"),
                DockStyle.Top);

            AddMenuButton("Đơn Hàng",
                () => LoadTableData("Orders", "Danh Sách Đơn Hàng"),
                DockStyle.Top);

            AddMenuButton("Quản Lý Bàn",
                () => LoadTableData("TablesCafe", "Danh Sách Bàn"),
                DockStyle.Top);

            AddMenuButton("Khách Hàng",
                () => LoadTableData("Customers", "Danh Sách Khách Hàng"),
                DockStyle.Top);

            AddMenuButton("Nhân Viên",
                () => LoadTableData("Employees", "Danh Sách Nhân Viên"),
                DockStyle.Top);

            AddMenuButton("Sản Phẩm",
                () => LoadTableData("Products", "Danh Sách Sản Phẩm"),
                DockStyle.Top);

            AddMenuButton("Danh Mục",
                () => LoadTableData("Categories", "Danh Mục Sản Phẩm"),
                DockStyle.Top);

            // =========================
            // HEADER
            // =========================
            panelHeader = new Panel();
            panelHeader.Dock = DockStyle.Top;
            panelHeader.Height = 60;
            panelHeader.BackColor = Color.FromArgb(93, 64, 55);

            lblHeaderTitle = new Label();
            lblHeaderTitle.Text = "CHÀO MỪNG BẠN ĐẾN VỚI HỆ THỐNG";
            lblHeaderTitle.ForeColor = Color.White;
            lblHeaderTitle.Font = new Font("Segoe UI", 14F, FontStyle.Bold);
            lblHeaderTitle.Dock = DockStyle.Fill;
            lblHeaderTitle.TextAlign = ContentAlignment.MiddleCenter;

            panelHeader.Controls.Add(lblHeaderTitle);

            // =========================
            // MAIN PANEL
            // =========================
            panelMain = new Panel();
            panelMain.Dock = DockStyle.Fill;
            panelMain.Padding = new Padding(20);

            // =========================
            // CRUD PANEL
            // =========================
            FlowLayoutPanel crudPanel = new FlowLayoutPanel();
            crudPanel.Dock = DockStyle.Top;
            crudPanel.Height = 60;

            btnAdd = CreateCrudButton("➕ Thêm");
            btnEdit = CreateCrudButton("✏️ Sửa");
            btnDelete = CreateCrudButton("🗑 Xóa");
            btnRefresh = CreateCrudButton("🔄 Làm Mới");

            btnAdd.Click += BtnAdd_Click;
            btnEdit.Click += BtnEdit_Click;
            btnDelete.Click += BtnDelete_Click;
            btnRefresh.Click += BtnRefresh_Click;

            crudPanel.Controls.Add(btnAdd);
            crudPanel.Controls.Add(btnEdit);
            crudPanel.Controls.Add(btnDelete);
            crudPanel.Controls.Add(btnRefresh);

            // =========================
            // DATAGRIDVIEW
            // =========================
            dgvData = new DataGridView();
            dgvData.Dock = DockStyle.Fill;
            dgvData.BackgroundColor = Color.White;
            dgvData.BorderStyle = BorderStyle.None;

            dgvData.AutoSizeColumnsMode =
                DataGridViewAutoSizeColumnsMode.Fill;

            dgvData.AllowUserToAddRows = false;
            dgvData.AllowUserToDeleteRows = false;
            dgvData.ReadOnly = true;

            dgvData.SelectionMode =
                DataGridViewSelectionMode.FullRowSelect;

            dgvData.MultiSelect = false;
            dgvData.RowHeadersVisible = false;

            // STYLE
            dgvData.EnableHeadersVisualStyles = false;

            dgvData.ColumnHeadersDefaultCellStyle.BackColor =
                Color.FromArgb(141, 110, 99);

            dgvData.ColumnHeadersDefaultCellStyle.ForeColor =
                Color.White;

            dgvData.ColumnHeadersDefaultCellStyle.Font =
                new Font("Segoe UI", 11F, FontStyle.Bold);

            dgvData.ColumnHeadersHeight = 40;

            dgvData.AlternatingRowsDefaultCellStyle.BackColor =
                Color.FromArgb(239, 235, 233);

            dgvData.DefaultCellStyle.SelectionBackColor =
                Color.FromArgb(215, 204, 200);

            dgvData.DefaultCellStyle.SelectionForeColor =
                Color.Black;

            // ADD CONTROLS
            panelMain.Controls.Add(dgvData);
            panelMain.Controls.Add(crudPanel);

            this.Controls.Add(panelMain);
            this.Controls.Add(panelHeader);
            this.Controls.Add(panelSidebar);
        }

        // =====================================
        // TẠO NÚT MENU
        // =====================================
        private void AddMenuButton(
            string text,
            Action onClickAction,
            DockStyle dockStyle)
        {
            Button btn = new Button();

            btn.Text = "  " + text;
            btn.Dock = dockStyle;
            btn.Height = 50;

            btn.FlatStyle = FlatStyle.Flat;
            btn.FlatAppearance.BorderSize = 0;

            btn.ForeColor = Color.White;

            btn.Font = new Font("Segoe UI", 11F);

            btn.TextAlign = ContentAlignment.MiddleLeft;

            btn.Cursor = Cursors.Hand;

            btn.MouseEnter +=
                (s, e) => btn.BackColor =
                Color.FromArgb(78, 52, 46);

            btn.MouseLeave +=
                (s, e) => btn.BackColor =
                Color.Transparent;

            btn.Click += (s, e) => onClickAction();

            panelSidebar.Controls.Add(btn);

            btn.BringToFront();
        }

        // =====================================
        // TẠO NÚT CRUD
        // =====================================
        private Button CreateCrudButton(string text)
        {
            Button btn = new Button();

            btn.Text = text;

            btn.Width = 120;
            btn.Height = 40;

            btn.BackColor = Color.FromArgb(93, 64, 55);

            btn.ForeColor = Color.White;

            btn.FlatStyle = FlatStyle.Flat;

            btn.FlatAppearance.BorderSize = 0;

            btn.Cursor = Cursors.Hand;

            return btn;
        }

        // =====================================
        // LOAD DATA
        // =====================================
        private void LoadTableData(string tableName, string title)
        {
            currentTable = tableName;

            lblHeaderTitle.Text = title.ToUpper();

            try
            {
                // GỌI HÀM GetConnectionString() TẠI ĐÂY
                using (SqlConnection conn =
                    new SqlConnection(GetConnectionString()))
                {
                    conn.Open();

                    string sql = "";

                    switch (tableName)
                    {
                        case "Categories":

                            sql =
                                @"SELECT
                                    CategoryID AS [Mã Danh Mục],
                                    CategoryName AS [Tên Danh Mục]
                                  FROM Categories";

                            break;

                        case "Products":

                            sql =
                                @"SELECT
                                    p.ProductID AS [Mã SP],
                                    p.ProductName AS [Tên Món],
                                    p.Price AS [Giá Tiền],
                                    c.CategoryName AS [Loại],
                                    p.Status AS [Trạng Thái]
                                  FROM Products p
                                  JOIN Categories c
                                  ON p.CategoryID = c.CategoryID";

                            break;

                        case "Employees":

                            sql =
                                @"SELECT
                                    EmployeeID AS [Mã NV],
                                    FullName AS [Họ Tên Nhân Viên],
                                    Phone AS [Số Điện Thoại],
                                    Position AS [Chức Vụ]
                                  FROM Employees";

                            break;

                        case "Customers":

                            sql =
                                @"SELECT
                                    CustomerID AS [Mã KH],
                                    FullName AS [Họ Tên Khách Hàng],
                                    Phone AS [Số Điện Thoại],
                                    Points AS [Điểm Tích Lũy]
                                  FROM Customers";

                            break;

                        case "TablesCafe":

                            sql =
                                @"SELECT
                                    TableID AS [Mã Bàn],
                                    TableName AS [Tên Bàn],
                                    Status AS [Trạng Thái]
                                  FROM TablesCafe";

                            break;

                        case "Orders":

                            sql =
                                @"SELECT
                                    o.OrderID AS [Mã Đơn],
                                    o.OrderDate AS [Thời Gian Lập],
                                    e.FullName AS [Người Lập Đơn],
                                    c.FullName AS [Tên Khách Hàng],
                                    t.TableName AS [Bàn]
                                  FROM Orders o
                                  LEFT JOIN Employees e
                                  ON o.EmployeeID = e.EmployeeID
                                  LEFT JOIN Customers c
                                  ON o.CustomerID = c.CustomerID
                                  LEFT JOIN TablesCafe t
                                  ON o.TableID = t.TableID";

                            break;

                        case "OrderDetails":

                            sql =
                                @"SELECT
                                    od.OrderID AS [Mã Đơn Hàng],
                                    p.ProductName AS [Tên Đồ Uống],
                                    od.Quantity AS [Số Lượng],
                                    od.UnitPrice AS [Đơn Giá],
                                    (od.Quantity * od.UnitPrice)
                                    AS [Thành Tiền]
                                  FROM OrderDetails od
                                  JOIN Products p
                                  ON od.ProductID = p.ProductID";

                            break;

                        case "Payments":

                            sql =
                                @"SELECT
                                    PaymentID AS [Mã Giao Dịch],
                                    OrderID AS [Mã Đơn Hàng],
                                    PaymentMethod AS [Phương Thức TT],
                                    PaymentDate AS [Ngày Thanh Toán]
                                  FROM Payments";

                            break;
                    }

                    SqlDataAdapter da =
                        new SqlDataAdapter(sql, conn);

                    DataTable dt = new DataTable();

                    da.Fill(dt);

                    dgvData.DataSource = dt;
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show(
                    "Lỗi kết nối CSDL: " + ex.Message,
                    "Lỗi",
                    MessageBoxButtons.OK,
                    MessageBoxIcon.Error);
            }
        }

        // =====================================
        // CREATE PRODUCT
        // =====================================
        private void BtnAdd_Click(object sender, EventArgs e)
        {
            if (currentTable != "Products")
            {
                MessageBox.Show(
                    "Chỉ hỗ trợ CRUD cho bảng Sản Phẩm!");
                return;
            }

            string productName =
                Interaction.InputBox(
                    "Nhập tên món:",
                    "Thêm Sản Phẩm");

            if (string.IsNullOrWhiteSpace(productName))
                return;

            string priceText =
                Interaction.InputBox(
                    "Nhập giá:",
                    "Thêm Sản Phẩm");

            decimal price;

            if (!decimal.TryParse(priceText, out price))
            {
                MessageBox.Show("Giá không hợp lệ!");
                return;
            }

            using (SqlConnection conn =
                new SqlConnection(GetConnectionString()))
            {
                conn.Open();

                string sql =
                    @"INSERT INTO Products
                    (
                        ProductName,
                        Price,
                        CategoryID,
                        Status
                    )
                    VALUES
                    (
                        @name,
                        @price,
                        1,
                        N'Còn bán'
                    )";

                SqlCommand cmd =
                    new SqlCommand(sql, conn);

                cmd.Parameters.AddWithValue(
                    "@name",
                    productName);

                cmd.Parameters.AddWithValue(
                    "@price",
                    price);

                cmd.ExecuteNonQuery();
            }

            MessageBox.Show("Thêm sản phẩm thành công!");

            LoadTableData(
                "Products",
                "Danh Sách Sản Phẩm");
        }

        // =====================================
        // UPDATE PRODUCT
        // =====================================
        private void BtnEdit_Click(object sender, EventArgs e)
        {
            if (currentTable != "Products")
            {
                MessageBox.Show(
                    "Chỉ hỗ trợ CRUD cho bảng Sản Phẩm!");
                return;
            }

            if (dgvData.SelectedRows.Count == 0)
            {
                MessageBox.Show("Vui lòng chọn sản phẩm!");
                return;
            }

            int id =
                Convert.ToInt32(
                    dgvData.SelectedRows[0].Cells[0].Value);

            string oldName =
                dgvData.SelectedRows[0].Cells[1].Value.ToString();

            string oldPrice =
                dgvData.SelectedRows[0].Cells[2].Value.ToString();

            string oldCategory =
                dgvData.SelectedRows[0].Cells[3].Value.ToString();

            string oldStatus =
                dgvData.SelectedRows[0].Cells[4].Value.ToString();

            // NHẬP DỮ LIỆU MỚI
            string newName =
                Interaction.InputBox(
                    "Nhập tên mới:",
                    "Cập Nhật",
                    oldName);

            string newPriceText =
                Interaction.InputBox(
                    "Nhập giá mới:",
                    "Cập Nhật",
                    oldPrice);

            string newCategoryIDText =
                Interaction.InputBox(
                    "Nhập CategoryID mới:",
                    "Cập Nhật",
                    "1");

            string newStatus =
                Interaction.InputBox(
                    "Nhập trạng thái:",
                    "Cập Nhật",
                    oldStatus);

            decimal newPrice;
            int newCategoryID;

            // KIỂM TRA GIÁ
            if (!decimal.TryParse(newPriceText, out newPrice))
            {
                MessageBox.Show("Giá không hợp lệ!");
                return;
            }

            // KIỂM TRA CATEGORY ID
            if (!int.TryParse(newCategoryIDText, out newCategoryID))
            {
                MessageBox.Show("CategoryID không hợp lệ!");
                return;
            }

            using (SqlConnection conn =
                new SqlConnection(GetConnectionString()))
            {
                conn.Open();

                string sql =
                    @"UPDATE Products
              SET ProductName = @name,
                  Price = @price,
                  CategoryID = @categoryID,
                  Status = @status
              WHERE ProductID = @id";

                SqlCommand cmd =
                    new SqlCommand(sql, conn);

                cmd.Parameters.AddWithValue(
                    "@name",
                    newName);

                cmd.Parameters.AddWithValue(
                    "@price",
                    newPrice);

                cmd.Parameters.AddWithValue(
                    "@categoryID",
                    newCategoryID);

                cmd.Parameters.AddWithValue(
                    "@status",
                    newStatus);

                cmd.Parameters.AddWithValue(
                    "@id",
                    id);

                cmd.ExecuteNonQuery();
            }

            MessageBox.Show("Cập nhật thành công!");

            LoadTableData(
                "Products",
                "Danh Sách Sản Phẩm");
        }

        // =====================================
        // DELETE PRODUCT
        // =====================================
        private void BtnDelete_Click(object sender, EventArgs e)
        {
            if (currentTable != "Products")
            {
                MessageBox.Show(
                    "Chỉ hỗ trợ CRUD cho bảng Sản Phẩm!");
                return;
            }

            if (dgvData.SelectedRows.Count == 0)
            {
                MessageBox.Show(
                    "Vui lòng chọn sản phẩm!");
                return;
            }

            int id =
                Convert.ToInt32(
                    dgvData.SelectedRows[0].Cells[0].Value);

            DialogResult result =
                MessageBox.Show(
                    "Bạn có chắc muốn xóa sản phẩm này?",
                    "Xác Nhận",
                    MessageBoxButtons.YesNo,
                    MessageBoxIcon.Warning);

            if (result == DialogResult.Yes)
            {
                using (SqlConnection conn =
                    new SqlConnection(GetConnectionString()))
                {
                    conn.Open();

                    string sql =
                        @"DELETE FROM Products
                          WHERE ProductID = @id";

                    SqlCommand cmd =
                        new SqlCommand(sql, conn);

                    cmd.Parameters.AddWithValue(
                        "@id",
                        id);

                    cmd.ExecuteNonQuery();
                }

                MessageBox.Show("Xóa thành công!");

                LoadTableData(
                    "Products",
                    "Danh Sách Sản Phẩm");
            }
        }

        // =====================================
        // REFRESH
        // =====================================
        private void BtnRefresh_Click(
            object sender,
            EventArgs e)
        {
            if (currentTable != "")
            {
                LoadTableData(
                    currentTable,
                    lblHeaderTitle.Text);
            }
        }
    }
}