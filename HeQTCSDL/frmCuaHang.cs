using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Data.SqlClient;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace HeQTCSDL
{
    public partial class frmCuaHang : Form
    {
        private string connectionString = "Data Source=LAPTOP-UDIS316D\\SQLEXPRESS;Initial Catalog=QuanLyKho_TienLoi;Integrated Security=True";
        private bool isAdding = false;
        private bool isEditing = false;

        public frmCuaHang()
        {
            InitializeComponent();
        }

        private void DisableEdit()
        {
            txtMaCuaHang.Enabled = false;
            txtTenCuaHang.Enabled = false;
            txtDiaChi.Enabled = false;
            txtKhuVuc.Enabled = false;
        }

        private void EnableEdit()
        {
            txtMaCuaHang.Enabled = true;
            txtTenCuaHang.Enabled = true;
            txtDiaChi.Enabled = true;
            txtKhuVuc.Enabled = true;
        }

        private void ClearDuLieu()
        {
            txtMaCuaHang.Clear();
            txtTenCuaHang.Clear();
            txtDiaChi.Clear();
            txtKhuVuc.Clear();
        }

        private void LoadData()
        {
            using (SqlConnection conn = new SqlConnection(connectionString))
            {
                try
                {
                    conn.Open();
                    string query = "SELECT MaCuaHang, TenCuaHang, DiaChi, KhuVuc FROM CuaHang";
                    SqlDataAdapter da = new SqlDataAdapter(query, conn);
                    DataTable dt = new DataTable();
                    da.Fill(dt);
                    dgvCuaHang.AutoGenerateColumns = false;
                    dgvCuaHang.DataSource = dt;
                }
                catch (Exception ex)
                {
                    MessageBox.Show("Lỗi load dữ liệu: " + ex.Message);
                }
            }
        }

        private void frmCuaHang_Load(object sender, EventArgs e)
        {
            LoadData();
            DisableEdit();
            if (!UserSession.LaAdmin)
            {
                if (btnXoa != null)
                {
                    btnXoa.Enabled = false;
                }
            }
        }

        private void btnThem_Click(object sender, EventArgs e)
        {
            isAdding = true;
            isEditing = false;
            EnableEdit();
            ClearDuLieu();
            txtMaCuaHang.Focus();
        }

        private void btnXoa_Click(object sender, EventArgs e)
        {
            if (dgvCuaHang.CurrentRow == null)
            {
                MessageBox.Show("Vui lòng chọn một cửa hàng để xóa.");
                return;
            }

            DataGridViewRow row = dgvCuaHang.CurrentRow;
            string maCuaHang = GetSafeStringValue(row, "MaCuaHang");

            if (string.IsNullOrEmpty(maCuaHang))
            {
                MessageBox.Show("Không thể xóa dòng trống.");
                return;
            }

            if (MessageBox.Show($"Bạn có chắc chắn muốn xóa cửa hàng {maCuaHang}?", "Xác nhận xóa",
                MessageBoxButtons.YesNo) == DialogResult.Yes)
            {
                using (SqlConnection conn = new SqlConnection(connectionString))
                {
                    try
                    {
                        using (SqlCommand cmd = new SqlCommand("sp_CuaHang_Xoa", conn))
                        {
                            cmd.CommandType = CommandType.StoredProcedure;
                            cmd.Parameters.AddWithValue("@MaCuaHang", maCuaHang);

                            conn.Open();
                            int result = cmd.ExecuteNonQuery();
                            if (result > 0)
                            {
                                MessageBox.Show("Xóa cửa hàng thành công.");
                                LoadData();
                            }
                            else
                            {
                                MessageBox.Show("Xóa cửa hàng thất bại. Có thể cửa hàng đang được sử dụng trong phiếu xuất kho.");
                            }
                        }
                    }
                    catch (Exception ex)
                    {
                        MessageBox.Show("Lỗi: " + ex.Message);
                    }
                }
            }
        }

        private void btnSua_Click(object sender, EventArgs e)
        {
            if (dgvCuaHang.CurrentRow == null)
            {
                MessageBox.Show("Vui lòng chọn một cửa hàng để sửa.");
                return;
            }

            isAdding = false;
            isEditing = true;
            EnableEdit();
            txtMaCuaHang.Enabled = false;
            DataGridViewRow row = dgvCuaHang.CurrentRow;
            txtMaCuaHang.Text = GetSafeStringValue(row, "MaCuaHang");
            txtTenCuaHang.Text = GetSafeStringValue(row, "TenCuaHang");
            txtDiaChi.Text = GetSafeStringValue(row, "DiaChi");
            txtKhuVuc.Text = GetSafeStringValue(row, "KhuVuc");
        }

        private void btnLuu_Click(object sender, EventArgs e)
        {
            if (string.IsNullOrWhiteSpace(txtMaCuaHang.Text) || string.IsNullOrWhiteSpace(txtTenCuaHang.Text))
            {
                MessageBox.Show("Mã cửa hàng và tên cửa hàng không được để trống.");
                return;
            }

            using (SqlConnection conn = new SqlConnection(connectionString))
            {
                try
                {
                    conn.Open();

                    if (isAdding)
                    {
                        using (SqlCommand cmd = new SqlCommand("sp_CuaHang_Them", conn))
                        {
                            cmd.CommandType = CommandType.StoredProcedure;
                            cmd.Parameters.AddWithValue("@MaCuaHang", txtMaCuaHang.Text.Trim());
                            cmd.Parameters.AddWithValue("@TenCuaHang", txtTenCuaHang.Text.Trim());
                            cmd.Parameters.AddWithValue("@DiaChi", txtDiaChi.Text.Trim());
                            cmd.Parameters.AddWithValue("@KhuVuc", txtKhuVuc.Text.Trim());

                            int result = cmd.ExecuteNonQuery();
                            if (result > 0)
                            {
                                MessageBox.Show("Thêm cửa hàng thành công.");
                            }
                        }
                    }
                    else if (isEditing)
                    {
                        using (SqlCommand cmd = new SqlCommand("sp_CuaHang_Sua", conn))
                        {
                            cmd.CommandType = CommandType.StoredProcedure;
                            cmd.Parameters.AddWithValue("@MaCuaHang", txtMaCuaHang.Text.Trim());
                            cmd.Parameters.AddWithValue("@TenCuaHang", txtTenCuaHang.Text.Trim());
                            cmd.Parameters.AddWithValue("@DiaChi", txtDiaChi.Text.Trim());
                            cmd.Parameters.AddWithValue("@KhuVuc", txtKhuVuc.Text.Trim());

                            int result = cmd.ExecuteNonQuery();
                            if (result > 0)
                            {
                                MessageBox.Show("Sửa cửa hàng thành công.");
                            }
                        }
                    }

                    LoadData();
                    DisableEdit();
                    isAdding = false;
                    isEditing = false;
                }
                catch (Exception ex)
                {
                    MessageBox.Show("Lỗi: " + ex.Message);
                }
            }
        }

        private void btnTimKiem_Click(object sender, EventArgs e)
        {
            string maCuaHang = txtTimKiem.Text.Trim();

            using (SqlConnection conn = new SqlConnection(connectionString))
            {
                try
                {
                    conn.Open();
                    string query = "SELECT MaCuaHang, TenCuaHang, DiaChi, KhuVuc FROM CuaHang";

                    if (!string.IsNullOrEmpty(maCuaHang))
                    {
                        query += " WHERE MaCuaHang LIKE @MaCuaHang";
                    }

                    SqlDataAdapter da = new SqlDataAdapter(query, conn);
                    if (!string.IsNullOrEmpty(maCuaHang))
                        da.SelectCommand.Parameters.AddWithValue("@MaCuaHang", "%" + maCuaHang + "%");

                    DataTable dt = new DataTable();
                    da.Fill(dt);

                    dgvCuaHang.AutoGenerateColumns = false;
                    dgvCuaHang.DataSource = dt;

                    if (dt.Rows.Count == 0 && !string.IsNullOrEmpty(maCuaHang))
                    {
                        MessageBox.Show("Không tìm thấy cửa hàng với mã: " + maCuaHang);
                    }
                }
                catch (Exception ex)
                {
                    MessageBox.Show("Lỗi tìm kiếm: " + ex.Message);
                }
            }
        }

        private void dgvCuaHang_SelectionChanged(object sender, EventArgs e)
        {
            if (dgvCuaHang.CurrentRow != null && !isAdding && !isEditing)
            {
                DataGridViewRow row = dgvCuaHang.CurrentRow;
                txtMaCuaHang.Text = GetSafeStringValue(row, "MaCuaHang");
                txtTenCuaHang.Text = GetSafeStringValue(row, "TenCuaHang");
                txtDiaChi.Text = GetSafeStringValue(row, "DiaChi");
                txtKhuVuc.Text = GetSafeStringValue(row, "KhuVuc");
            }
        }
        private string GetSafeStringValue(DataGridViewRow row, string columnName)
        {
            if (row.Cells[columnName] != null && row.Cells[columnName].Value != null && row.Cells[columnName].Value != DBNull.Value)
                return row.Cells[columnName].Value.ToString();
            else
                return string.Empty;
        }
    }
}
