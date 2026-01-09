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
    public partial class frmKho : Form
    {
        private string connectionString = "Data Source=LAPTOP-UDIS316D\\SQLEXPRESS;Initial Catalog=QuanLyKho_TienLoi;Integrated Security=True";
        private bool isAdding = false;
        private bool isEditing = false;
        public frmKho()
        {
            InitializeComponent();
        }

        private void DisableEdit()
        {
            txtMaKho.Enabled = false;
            txtTenKho.Enabled = false;
            txtViTri.Enabled = false;
            chkTrangThai.Enabled = false;
        }

        private void EnableEdit()
        {
            txtMaKho.Enabled = true;
            txtTenKho.Enabled = true;
            txtViTri.Enabled = true;
            chkTrangThai.Enabled = true;
        }

        private void ClearControls()
        {
            txtMaKho.Clear();
            txtTenKho.Clear();
            txtViTri.Clear();
            chkTrangThai.Checked = false;
        }

        private void LoadData()
        {
            using (SqlConnection conn = new SqlConnection(connectionString))
            {
                try
                {
                    conn.Open();
                    string query = "SELECT MaKho, TenKho, ViTri, TrangThai FROM Kho";
                    SqlDataAdapter da = new SqlDataAdapter(query, conn);
                    DataTable dt = new DataTable();
                    da.Fill(dt);

                    dgvKho.DataSource = dt;
                }
                catch (Exception ex)
                {
                    MessageBox.Show("Lỗi load dữ liệu: " + ex.Message);
                }
            } 
        }

        private void frmKho_Load(object sender, EventArgs e)
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
            txtMaKho.Focus();
        }

        private void btnXoa_Click(object sender, EventArgs e)
        {
            if (dgvKho.CurrentRow == null)
            {
                MessageBox.Show("Vui lòng chọn một kho để xóa.");
                return;
            }

            DataGridViewRow row = dgvKho.CurrentRow;
            string maKho = row.Cells["MaKho"].Value.ToString();

            if (MessageBox.Show($"Bạn có chắc chắn muốn xóa kho {maKho}?", "Xác nhận xóa",
                MessageBoxButtons.YesNo) == DialogResult.Yes)
            {
                using (SqlConnection conn = new SqlConnection(connectionString))
                {
                    try
                    {
                        using (SqlCommand cmd = new SqlCommand("sp_Kho_Xoa", conn))
                        {
                            cmd.CommandType = CommandType.StoredProcedure;
                            cmd.Parameters.AddWithValue("@MaKho", maKho);

                            conn.Open();
                            int result = cmd.ExecuteNonQuery();
                            if (result > 0)
                            {
                                MessageBox.Show("Xóa kho thành công.");
                                LoadData();
                            }
                            else
                            {
                                MessageBox.Show("Xóa kho thất bại. Có thể kho đang được sử dụng.");
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
            if (dgvKho.CurrentRow == null)
            {
                MessageBox.Show("Vui lòng chọn một kho để sửa.");
                return;
            }

            isAdding = false;
            isEditing = true;
            EnableEdit();
            txtMaKho.Enabled = false;
            DataGridViewRow row = dgvKho.CurrentRow;
            txtMaKho.Text = row.Cells["MaKho"].Value.ToString();
            txtTenKho.Text = row.Cells["TenKho"].Value.ToString();
            txtViTri.Text = row.Cells["ViTri"].Value?.ToString() ?? "";
            chkTrangThai.Checked = Convert.ToBoolean(row.Cells["TrangThai"].Value);
        }

        private void btnLuu_Click(object sender, EventArgs e)
        {
            if (string.IsNullOrWhiteSpace(txtMaKho.Text) || string.IsNullOrWhiteSpace(txtTenKho.Text))
            {
                MessageBox.Show("Mã kho và tên kho không được để trống.");
                return;
            }
            using (SqlConnection conn = new SqlConnection(connectionString))
            {
                try
                {
                    conn.Open();

                    if (isAdding)
                    {
                        using (SqlCommand cmd = new SqlCommand("sp_Kho_Them", conn))
                        {
                            cmd.CommandType = CommandType.StoredProcedure;
                            cmd.Parameters.AddWithValue("@MaKho", txtMaKho.Text.Trim());
                            cmd.Parameters.AddWithValue("@TenKho", txtTenKho.Text.Trim());
                            cmd.Parameters.AddWithValue("@ViTri", txtViTri.Text.Trim());
                            cmd.Parameters.AddWithValue("@TrangThai", chkTrangThai.Checked);

                            int result = cmd.ExecuteNonQuery();
                            if (result > 0)
                            {
                                MessageBox.Show("Thêm kho thành công.");
                            }
                        }
                    }
                    else if (isEditing)
                    {
                        using (SqlCommand cmd = new SqlCommand("sp_Kho_Sua", conn))
                        {
                            cmd.CommandType = CommandType.StoredProcedure;
                            cmd.Parameters.AddWithValue("@MaKho", txtMaKho.Text.Trim());
                            cmd.Parameters.AddWithValue("@TenKho", txtTenKho.Text.Trim());
                            cmd.Parameters.AddWithValue("@ViTri", txtViTri.Text.Trim());
                            cmd.Parameters.AddWithValue("@TrangThai", chkTrangThai.Checked);

                            int result = cmd.ExecuteNonQuery();
                            if (result > 0)
                            {
                                MessageBox.Show("Sửa kho thành công.");
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
            string maKho = txtTimMaKho.Text.Trim();
            using (SqlConnection conn = new SqlConnection(connectionString))
            {
                try
                {
                    conn.Open();
                    string query = "SELECT MaKho, TenKho, ViTri, TrangThai FROM Kho";

                    if (!string.IsNullOrEmpty(maKho))
                    {
                        query += " WHERE MaKho LIKE @MaKho";
                    }

                    SqlDataAdapter da = new SqlDataAdapter(query, conn);
                    if (!string.IsNullOrEmpty(maKho))
                        da.SelectCommand.Parameters.AddWithValue("@MaKho", "%" + maKho + "%");

                    DataTable dt = new DataTable();
                    da.Fill(dt);
                    dgvKho.DataSource = dt;

                    if (dt.Rows.Count == 0 && !string.IsNullOrEmpty(maKho))
                    {
                        MessageBox.Show("Không tìm thấy kho với mã: " + maKho);
                    }
                }
                catch (Exception ex)
                {
                    MessageBox.Show("Lỗi tìm kiếm: " + ex.Message);
                }
            }
        }

        private void dgvKho_SelectionChanged(object sender, EventArgs e)
        {
            if (dgvKho.CurrentRow != null && !isAdding && !isEditing)
            {
                DataGridViewRow row = dgvKho.CurrentRow;
                txtMaKho.Text = Convert.ToString(row.Cells["MaKho"].Value);
                txtTenKho.Text = Convert.ToString(row.Cells["TenKho"].Value);
                txtViTri.Text = Convert.ToString(row.Cells["ViTri"].Value);
                object trangThaiValue = row.Cells["TrangThai"].Value;
                if (trangThaiValue != DBNull.Value && trangThaiValue != null)
                {
                    chkTrangThai.Checked = Convert.ToBoolean(trangThaiValue);
                }
                else
                {
                    chkTrangThai.Checked = false;
                }
            }
        }
    }
}