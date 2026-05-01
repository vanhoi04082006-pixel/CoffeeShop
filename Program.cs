using System;
using System.Data;
using System.Data.SqlClient;

class Program
{
    static string connStr =
    @"Server=VANHOI12;
      Database=CoffeeShopDB;
      Trusted_Connection=True;
      TrustServerCertificate=True;";

    static void Main()
    {
        Console.OutputEncoding = System.Text.Encoding.UTF8;
        while (true)
        {
            Console.Clear();
            
            Console.WriteLine("===== QUAN LY CUA HANG CA PHE =====");
            Console.WriteLine("1. Xem Sản Phẩm");
            Console.WriteLine("2. Xem Nhân Viên");
            Console.WriteLine("3. Xem Khách Hàng");
            Console.WriteLine("4. Xem Bàn");
            Console.WriteLine("5. Thoat");
            Console.Write("Chon: ");

            string chon = Console.ReadLine();

            switch (chon)
            {
                case "1":
                    ShowTable("Products");
                    break;
                case "2":
                    ShowTable("Employees");
                    break;
                case "3":
                    ShowTable("Customers");
                    break;
                case "4":
                    ShowTable("TablesCafe");
                    break;
                case "5":
                    return;
            }
        }
    }

    static void ShowTable(string tableName)
    {
        using (SqlConnection conn = new SqlConnection(connStr))
        {
            conn.Open();

            string sql = "SELECT * FROM " + tableName;

            SqlDataAdapter da = new SqlDataAdapter(sql, conn);
            DataTable dt = new DataTable();
            da.Fill(dt);

            Console.Clear();
            Console.WriteLine("===== " + tableName.ToUpper() + " =====");

            foreach (DataColumn col in dt.Columns)
                Console.Write(col.ColumnName + "\t");

            Console.WriteLine();

            foreach (DataRow row in dt.Rows)
            {
                foreach (var item in row.ItemArray)
                    Console.Write(item.ToString() + "\t");

                Console.WriteLine();
            }
        }

        Console.WriteLine("\nNhan phim bat ky...");
        Console.ReadKey();
    }
}