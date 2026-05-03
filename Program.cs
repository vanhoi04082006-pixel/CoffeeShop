using System;
using System.Data;
using System.Data.SqlClient;
using System.Drawing;
using System.Windows.Forms;

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
        // Nhớ đổi lại tên Server của bạn nếu cần nhé
        private string connStr = @"Server=VANHOI12; Database=CoffeeShopDB; Trusted_Connection=True; TrustServerCertificate=True;";

        private Panel panelSidebar;
        private Panel panelHeader;
        private Panel panelMain;
        private DataGridView dgvData;
        private Label lblHeaderTitle;

        public MainForm()
        {
            InitializeComponent();
        }

        private void InitializeComponent()
        {
            // 1. Cấu hình Form
            this.Text = "Phần Mềm Quản Lý Quán Cà Phê";
            this.Size = new Size(1100, 700); // Tăng size lên xíu để xem đủ 8 bảng cho rõ
            this.StartPosition = FormStartPosition.CenterScreen;
            this.Font = new Font("Segoe UI", 10F, FontStyle.Regular, GraphicsUnit.Point);
            this.BackColor = Color.FromArgb(245, 245, 245);

            // 2. Sidebar
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

            // Thêm nút THOÁT ở dưới cùng
            AddMenuButton("Thoát", () => Application.Exit(), DockStyle.Bottom);

            // Thêm 8 nút bấm cho 8 bảng (Xếp ngược từ dưới lên để hiển thị đúng thứ tự từ trên xuống)
            AddMenuButton("Thanh Toán", () => LoadTableData("Payments", "Lịch Sử Thanh Toán"), DockStyle.Top);
            AddMenuButton("Chi Tiết Đơn", () => LoadTableData("OrderDetails", "Chi Tiết Đơn Hàng"), DockStyle.Top);
            AddMenuButton("Đơn Hàng", () => LoadTableData("Orders", "Danh Sách Đơn Hàng"), DockStyle.Top);
            AddMenuButton("Quản Lý Bàn", () => LoadTableData("TablesCafe", "Danh Sách Bàn"), DockStyle.Top);
            AddMenuButton("Khách Hàng", () => LoadTableData("Customers", "Danh Sách Khách Hàng"), DockStyle.Top);
            AddMenuButton("Nhân Viên", () => LoadTableData("Employees", "Danh Sách Nhân Viên"), DockStyle.Top);
            AddMenuButton("Sản Phẩm", () => LoadTableData("Products", "Danh Sách Sản Phẩm"), DockStyle.Top);
            AddMenuButton("Danh Mục", () => LoadTableData("Categories", "Danh Mục Sản Phẩm"), DockStyle.Top);

            // 3. Header
            panelHeader = new Panel();
            panelHeader.Dock = DockStyle.Top;
            panelHeader.Height = 60;
            panelHeader.BackColor = Color.FromArgb(93, 64, 55);

            lblHeaderTitle = new Label();
            lblHeaderTitle.Text = "CHÀO MỪNG BẠN ĐẾN VỚI HỆ THỐNG";
            lblHeaderTitle.ForeColor = Color.White;
            lblHeaderTitle.Font = new Font("Segoe UI", 14F, FontStyle.Bold);
            lblHeaderTitle.AutoSize = false;
            lblHeaderTitle.Dock = DockStyle.Fill;
            lblHeaderTitle.TextAlign = ContentAlignment.MiddleCenter;
            panelHeader.Controls.Add(lblHeaderTitle);

            // 4. DataGridView
            panelMain = new Panel();
            panelMain.Dock = DockStyle.Fill;
            panelMain.Padding = new Padding(20);

            dgvData = new DataGridView();
            dgvData.Dock = DockStyle.Fill;
            dgvData.BackgroundColor = Color.White;
            dgvData.BorderStyle = BorderStyle.None;
            dgvData.AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.Fill;
            dgvData.AllowUserToAddRows = false;
            dgvData.AllowUserToDeleteRows = false;
            dgvData.ReadOnly = true;
            dgvData.SelectionMode = DataGridViewSelectionMode.FullRowSelect;
            dgvData.RowHeadersVisible = false;

            dgvData.EnableHeadersVisualStyles = false;
            dgvData.ColumnHeadersDefaultCellStyle.BackColor = Color.FromArgb(141, 110, 99);
            dgvData.ColumnHeadersDefaultCellStyle.ForeColor = Color.White;
            dgvData.ColumnHeadersDefaultCellStyle.Font = new Font("Segoe UI", 11F, FontStyle.Bold);
            dgvData.ColumnHeadersHeight = 40;
            dgvData.AlternatingRowsDefaultCellStyle.BackColor = Color.FromArgb(239, 235, 233);
            dgvData.DefaultCellStyle.SelectionBackColor = Color.FromArgb(215, 204, 200);
            dgvData.DefaultCellStyle.SelectionForeColor = Color.Black;

            panelMain.Controls.Add(dgvData);

            this.Controls.Add(panelMain);
            this.Controls.Add(panelHeader);
            this.Controls.Add(panelSidebar);
        }

        private void AddMenuButton(string text, Action onClickAction, DockStyle dockStyle)
        {
            Button btn = new Button();
            btn.Text = "  " + text;
            btn.Dock = dockStyle;
            btn.Height = 50; // Chỉnh nhỏ lại xíu để vừa 8 nút
            btn.FlatStyle = FlatStyle.Flat;
            btn.FlatAppearance.BorderSize = 0;
            btn.ForeColor = Color.White;
            btn.Font = new Font("Segoe UI", 11F, FontStyle.Regular);
            btn.TextAlign = ContentAlignment.MiddleLeft;
            btn.Cursor = Cursors.Hand;

            btn.MouseEnter += (s, e) => btn.BackColor = Color.FromArgb(78, 52, 46);
            btn.MouseLeave += (s, e) => btn.BackColor = Color.Transparent;

            btn.Click += (s, e) => onClickAction();

            panelSidebar.Controls.Add(btn);
            btn.BringToFront();
        }

        // HÀM XỬ LÝ CHÍNH: TẢI VÀ VIỆT HÓA 8 BẢNG
        private void LoadTableData(string tableName, string title)
        {
            lblHeaderTitle.Text = title.ToUpper();

            try
            {
                using (SqlConnection conn = new SqlConnection(connStr))
                {
                    conn.Open();
                    string sql = "";

                    // Sử dụng switch-case để gán câu lệnh SQL tương ứng cho từng bảng
                    switch (tableName)
                    {
                        case "Categories":
                            sql = "SELECT CategoryID AS [Mã Danh Mục], CategoryName AS [Tên Danh Mục] FROM Categories";
                            break;

                        case "Products":
                            sql = @"SELECT 
                                        p.ProductID AS [Mã SP], 
                                        p.ProductName AS [Tên Món], 
                                        p.Price AS [Giá Tiền], 
                                        c.CategoryName AS [Loại], 
                                        p.Status AS [Trạng Thái]
                                    FROM Products p
                                    JOIN Categories c ON p.CategoryID = c.CategoryID";
                            break;

                        case "Employees":
                            sql = @"SELECT 
                                        EmployeeID AS [Mã NV], 
                                        FullName AS [Họ Tên Nhân Viên], 
                                        Phone AS [Số Điện Thoại], 
                                        Position AS [Chức Vụ] 
                                    FROM Employees";
                            break;

                        case "Customers":
                            sql = @"SELECT 
                                        CustomerID AS [Mã KH], 
                                        FullName AS [Họ Tên Khách Hàng], 
                                        Phone AS [Số Điện Thoại], 
                                        Points AS [Điểm Tích Lũy] 
                                    FROM Customers";
                            break;

                        case "TablesCafe":
                            sql = "SELECT TableID AS [Mã Bàn], TableName AS [Tên Bàn], Status AS [Trạng Thái] FROM TablesCafe";
                            break;

                        case "Orders":
                            // Nối 4 bảng: Đơn Hàng + Nhân Viên + Khách Hàng + Bàn
                            sql = @"SELECT 
                                        o.OrderID AS [Mã Đơn], 
                                        o.OrderDate AS [Thời Gian Lập], 
                                        e.FullName AS [Người Lập Đơn], 
                                        c.FullName AS [Tên Khách Hàng], 
                                        t.TableName AS [Bàn]
                                    FROM Orders o
                                    LEFT JOIN Employees e ON o.EmployeeID = e.EmployeeID
                                    LEFT JOIN Customers c ON o.CustomerID = c.CustomerID
                                    LEFT JOIN TablesCafe t ON o.TableID = t.TableID";
                            break;

                        case "OrderDetails":
                            // Nối bảng Chi Tiết Đơn với bảng Sản Phẩm và nhân cột Thành Tiền
                            sql = @"SELECT 
                                        od.OrderID AS [Mã Đơn Hàng], 
                                        p.ProductName AS [Tên Đồ Uống], 
                                        od.Quantity AS [Số Lượng], 
                                        od.UnitPrice AS [Đơn Giá],
                                        (od.Quantity * od.UnitPrice) AS [Thành Tiền]
                                    FROM OrderDetails od
                                    JOIN Products p ON od.ProductID = p.ProductID";
                            break;

                        case "Payments":
                            sql = @"SELECT 
                                        PaymentID AS [Mã Giao Dịch], 
                                        OrderID AS [Mã Đơn Hàng], 
                                        PaymentMethod AS [Phương Thức TT], 
                                        PaymentDate AS [Ngày Thanh Toán] 
                                    FROM Payments";
                            break;
                    }

                    SqlDataAdapter da = new SqlDataAdapter(sql, conn);
                    DataTable dt = new DataTable();
                    da.Fill(dt);

                    dgvData.DataSource = dt;
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show("Lỗi kết nối CSDL: " + ex.Message, "Lỗi", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }
    }
}