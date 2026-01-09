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
    public partial class frmNhomNhietDo : Form
    {
        private string connectionString = "Data Source=LAPTOP-UDIS316D\\SQLEXPRESS;Initial Catalog=QuanLyKho_TienLoi;Integrated Security=True";
        private bool isAdding = false;
        private bool isEditing = false;

        public frmNhomNhietDo()
        {
            InitializeComponent();
        }

        private void DisableEdit()
        {
            txtMaNhom.Enabled = false;
            txtTenNhom.Enabled = false;
            txtTmin.Enabled = false;
            txtTmax.Enabled = false;
        }

        private void EnableEdit()
        {
            txtMaNhom.Enabled = true;
            txtTenNhom.Enabled = true;
            txtTmin.Enabled = true;
            txtTmax.Enabled = true;
        }

        private void ClearControls()
        {
            txtMaNhom.Clear();
            txtTenNhom.Clear();
            txtTmin.Clear();
            txtTmax.Clear();
        }

        private void LoadData()
        {
            using (SqlConnection conn = new SqlConnection(connectionString))
            {
                try
                {
                    conn.Open();
                    string query = "SELECT MaNhom, TenNhom, Tmin, Tmax FROM NhomNhietDo";
                    SqlDataAdapter da = new SqlDataAdapter(query, conn);
                    DataTable dt = new DataTable();
                    da.Fill(dt);

                    dgvNhom.AutoGenerateColumns = false;
                    dgvNhom.DataSource = dt;
                }
                catch (Exception ex)
                {
                    MessageBox.Show("Lỗi load dữ liệu: " + ex.Message);
                }
            }
        }

        private void frmNhomNhietDo_Load(object sender, EventArgs e)
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
            txtMaNhom.Focus();
        }

        private void btnXoa_Click(object sender, EventArgs e)
        {
            if (dgvNhom.CurrentRow == null)
            {
                MessageBox.Show("Vui lòng chọn một nhóm nhiệt độ để xóa.");
                return;
            }

            DataGridViewRow row = dgvNhom.CurrentRow;
            string maNhom = GetSafeStringValue(row, "MaNhom");

            if (string.IsNullOrEmpty(maNhom))
            {
                MessageBox.Show("Không thể xóa dòng trống.");
                return;
            }

            if (MessageBox.Show($"Bạn có chắc chắn muốn xóa nhóm nhiệt độ {maNhom}?", "Xác nhận xóa",
                MessageBoxButtons.YesNo) == DialogResult.Yes)
            {
                using (SqlConnection conn = new SqlConnection(connectionString))
                {
                    try
                    {
                        using (SqlCommand cmd = new SqlCommand("sp_NhomNhietDo_Delete", conn))
                        {
                            cmd.CommandType = CommandType.StoredProcedure;
                            cmd.Parameters.AddWithValue("@MaNhom", maNhom);

                            conn.Open();
                            int result = cmd.ExecuteNonQuery();
                            if (result > 0)
                            {
                                MessageBox.Show("Xóa nhóm nhiệt độ thành công.");
                                LoadData();
                            }
                            else
                            {
                                MessageBox.Show("Xóa nhóm nhiệt độ thất bại. Có thể nhóm nhiệt độ đang được sử dụng bởi hàng hóa.");
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
            if (dgvNhom.CurrentRow == null)
            {
                MessageBox.Show("Vui lòng chọn một nhóm nhiệt độ để sửa.");
                return;
            }

            isAdding = false;
            isEditing = true;
            EnableEdit();
            txtMaNhom.Enabled = false;
            DataGridViewRow row = dgvNhom.CurrentRow;
            txtMaNhom.Text = GetSafeStringValue(row, "MaNhom");
            txtTenNhom.Text = GetSafeStringValue(row, "TenNhom");
            txtTmin.Text = GetSafeStringValue(row, "Tmin");
            txtTmax.Text = GetSafeStringValue(row, "Tmax");
        }

        private void btnLuu_Click(object sender, EventArgs e)
        {
            if (string.IsNullOrWhiteSpace(txtMaNhom.Text) ||
                string.IsNullOrWhiteSpace(txtTenNhom.Text) ||
                string.IsNullOrWhiteSpace(txtTmin.Text) ||
                string.IsNullOrWhiteSpace(txtTmax.Text))
            {
                MessageBox.Show("Vui lòng điền đầy đủ thông tin.");
                return;
            }
            if (!decimal.TryParse(txtTmin.Text, out decimal tmin) || !decimal.TryParse(txtTmax.Text, out decimal tmax))
            {
                MessageBox.Show("Tmin và Tmax phải là số.");
                return;
            }

            using (SqlConnection conn = new SqlConnection(connectionString))
            {
                try
                {
                    conn.Open();

                    if (isAdding)
                    {
                        using (SqlCommand cmd = new SqlCommand("sp_NhomNhietDo_Them", conn))
                        {
                            cmd.CommandType = CommandType.StoredProcedure;
                            cmd.Parameters.AddWithValue("@MaNhom", txtMaNhom.Text.Trim());
                            cmd.Parameters.AddWithValue("@TenNhom", txtTenNhom.Text.Trim());
                            cmd.Parameters.AddWithValue("@Tmin", tmin);
                            cmd.Parameters.AddWithValue("@Tmax", tmax);

                            int result = cmd.ExecuteNonQuery();
                            if (result > 0)
                            {
                                MessageBox.Show("Thêm nhóm nhiệt độ thành công.");
                            }
                        }
                    }
                    else if (isEditing)
                    {
                        using (SqlCommand cmd = new SqlCommand("sp_NhomNhietDo_Sua", conn))
                        {
                            cmd.CommandType = CommandType.StoredProcedure;
                            cmd.Parameters.AddWithValue("@MaNhom", txtMaNhom.Text.Trim());
                            cmd.Parameters.AddWithValue("@TenNhom", txtTenNhom.Text.Trim());
                            cmd.Parameters.AddWithValue("@Tmin", tmin);
                            cmd.Parameters.AddWithValue("@Tmax", tmax);

                            int result = cmd.ExecuteNonQuery();
                            if (result > 0)
                            {
                                MessageBox.Show("Sửa nhóm nhiệt độ thành công.");
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

        private void dgvNhomNhietDo_SelectionChanged(object sender, EventArgs e)
        {
            if (dgvNhom.CurrentRow != null && !isAdding && !isEditing)
            {
                DataGridViewRow row = dgvNhom.CurrentRow;
                txtMaNhom.Text = GetSafeStringValue(row, "MaNhom");
                txtTenNhom.Text = GetSafeStringValue(row, "TenNhom");
                txtTmin.Text = GetSafeStringValue(row, "Tmin");
                txtTmax.Text = GetSafeStringValue(row, "Tmax");
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
