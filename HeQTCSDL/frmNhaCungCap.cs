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
    public partial class frmNhaCungCap : Form
    {
        private string connectionString = "Data Source=LAPTOP-UDIS316D\\SQLEXPRESS;Initial Catalog=QuanLyKho_TienLoi;Integrated Security=True";
        private bool isAdding = false;
        private bool isEditing = false;

        public frmNhaCungCap()
        {
            InitializeComponent();
        }

        private void DisableEdit()
        {
            txtMaNCC.Enabled = false;
            txtTenNCC.Enabled = false;
            txtDiaChi.Enabled = false;
            txtSDT.Enabled = false;
        }

        private void EnableEdit()
        {
            txtMaNCC.Enabled = true;
            txtTenNCC.Enabled = true;
            txtDiaChi.Enabled = true;
            txtSDT.Enabled = true;
        }

        private void ClearControls()
        {
            txtMaNCC.Clear();
            txtTenNCC.Clear();
            txtDiaChi.Clear();
            txtSDT.Clear();
        }

        private void LoadData()
        {
            using (SqlConnection conn = new SqlConnection(connectionString))
            {
                try
                {
                    conn.Open();
                    string query = "SELECT MaNCC, TenNCC, DiaChi, SDT FROM NhaCungCap";
                    SqlDataAdapter da = new SqlDataAdapter(query, conn);
                    DataTable dt = new DataTable();
                    da.Fill(dt);
                    dgvNCC.Columns.Clear();
                    dgvNCC.DataSource = dt;
                }
                catch (Exception ex)
                {
                    MessageBox.Show("Lỗi load dữ liệu: " + ex.Message);
                }
            }
        }

        private void frmNhaCungCap_Load(object sender, EventArgs e)
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
            ClearControls();
            txtMaNCC.Focus();
        }

        private void btnXoa_Click(object sender, EventArgs e)
        {
            if (dgvNCC.CurrentRow == null)
            {
                MessageBox.Show("Vui lòng chọn một nhà cung cấp để xóa.");
                return;
            }

            DataGridViewRow row = dgvNCC.CurrentRow;
            string maNCC = row.Cells["MaNCC"].Value.ToString();

            if (MessageBox.Show($"Bạn có chắc chắn muốn xóa nhà cung cấp {maNCC}?", "Xác nhận xóa",
                MessageBoxButtons.YesNo) == DialogResult.Yes)
            {
                using (SqlConnection conn = new SqlConnection(connectionString))
                {
                    try
                    {
                        using (SqlCommand cmd = new SqlCommand("sp_NhaCungCap_Xoa", conn))
                        {
                            cmd.CommandType = CommandType.StoredProcedure;
                            cmd.Parameters.AddWithValue("@MaNCC", maNCC);

                            conn.Open();
                            int result = cmd.ExecuteNonQuery();
                            if (result > 0)
                            {
                                MessageBox.Show("Xóa nhà cung cấp thành công.");
                                LoadData();
                            }
                            else
                            {
                                MessageBox.Show("Xóa nhà cung cấp thất bại. Có thể nhà cung cấp đang được sử dụng.");
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
            if (dgvNCC.CurrentRow == null)
            {
                MessageBox.Show("Vui lòng chọn một nhà cung cấp để sửa.");
                return;
            }

            isAdding = false;
            isEditing = true;
            EnableEdit();
            txtMaNCC.Enabled = false;
            DataGridViewRow row = dgvNCC.CurrentRow;
            txtMaNCC.Text = GetSafeStringValue(row, "MaNCC");
            txtTenNCC.Text = GetSafeStringValue(row, "TenNCC");
            txtDiaChi.Text = GetSafeStringValue(row, "DiaChi");
            txtSDT.Text = GetSafeStringValue(row, "SDT");
        }

        private void btnLuu_Click(object sender, EventArgs e)
        {
            if (string.IsNullOrWhiteSpace(txtMaNCC.Text) || string.IsNullOrWhiteSpace(txtTenNCC.Text))
            {
                MessageBox.Show("Mã NCC và tên NCC không được để trống.");
                return;
            }

            using (SqlConnection conn = new SqlConnection(connectionString))
            {
                try
                {
                    conn.Open();

                    if (isAdding)
                    {
                        using (SqlCommand cmd = new SqlCommand("sp_NhaCungCap_Them", conn))
                        {
                            cmd.CommandType = CommandType.StoredProcedure;
                            cmd.Parameters.AddWithValue("@MaNCC", txtMaNCC.Text.Trim());
                            cmd.Parameters.AddWithValue("@TenNCC", txtTenNCC.Text.Trim());
                            cmd.Parameters.AddWithValue("@DiaChi", txtDiaChi.Text.Trim());
                            cmd.Parameters.AddWithValue("@DienThoai", txtSDT.Text.Trim());

                            int result = cmd.ExecuteNonQuery();
                            if (result > 0)
                            {
                                MessageBox.Show("Thêm nhà cung cấp thành công.");
                            }
                        }
                    }
                    else if (isEditing)
                    {
                        using (SqlCommand cmd = new SqlCommand("sp_NhaCungCap_Sua", conn))
                        {
                            cmd.CommandType = CommandType.StoredProcedure;
                            cmd.Parameters.AddWithValue("@MaNCC", txtMaNCC.Text.Trim());
                            cmd.Parameters.AddWithValue("@TenNCC", txtTenNCC.Text.Trim());
                            cmd.Parameters.AddWithValue("@DiaChi", txtDiaChi.Text.Trim());
                            cmd.Parameters.AddWithValue("@DienThoai", txtSDT.Text.Trim());

                            int result = cmd.ExecuteNonQuery();
                            if (result > 0)
                            {
                                MessageBox.Show("Sửa nhà cung cấp thành công.");
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
            string maNCC = txtTimKiem.Text.Trim();

            using (SqlConnection conn = new SqlConnection(connectionString))
            {
                try
                {
                    conn.Open();
                    string query = "SELECT MaNCC, TenNCC, DiaChi, SDT FROM NhaCungCap";

                    if (!string.IsNullOrEmpty(maNCC))
                    {
                        query += " WHERE MaNCC LIKE @MaNCC";
                    }

                    SqlDataAdapter da = new SqlDataAdapter(query, conn);
                    if (!string.IsNullOrEmpty(maNCC))
                        da.SelectCommand.Parameters.AddWithValue("@MaNCC", "%" + maNCC + "%");

                    DataTable dt = new DataTable();
                    da.Fill(dt);
                    dgvNCC.DataSource = dt;

                    if (dt.Rows.Count == 0 && !string.IsNullOrEmpty(maNCC))
                    {
                        MessageBox.Show("Không tìm thấy nhà cung cấp với mã: " + maNCC);
                    }
                }
                catch (Exception ex)
                {
                    MessageBox.Show("Lỗi tìm kiếm: " + ex.Message);
                }
            }
        }

        private void dgvNhaCungCap_SelectionChanged(object sender, EventArgs e)
        {
            if (dgvNCC.CurrentRow != null && !isAdding && !isEditing)
            {
                DataGridViewRow row = dgvNCC.CurrentRow;
                txtMaNCC.Text = GetSafeStringValue(row, "MaNCC");
                txtTenNCC.Text = GetSafeStringValue(row, "TenNCC");
                txtDiaChi.Text = GetSafeStringValue(row, "DiaChi");
                txtSDT.Text = GetSafeStringValue(row, "SDT");
            }
        }
        private string GetSafeStringValue(DataGridViewRow row, string columnName)
        {
            if (row.Cells[columnName].Value != null && row.Cells[columnName].Value != DBNull.Value)
                return row.Cells[columnName].Value.ToString();
            else
                return string.Empty;
        }


        private void label1_Click(object sender, EventArgs e)
        {

        }
    }
}
