using HeQTCSDL;
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
using System.Xml.Linq;

namespace HeQTCSDL
{
    public partial class MainForm : Form
    {
        private Form currentChildForm;

        public MainForm()
        {
            InitializeComponent();
            PhanQuyen();
        }

        private void MainForm_Load(object sender, EventArgs e)
        {
            CapNhatThongBao();
        }
        private void PhanQuyen()
        {
            if (UserSession.VaiTro == null)
            {
                MessageBox.Show("Bạn không có quyền truy cập vào hệ thống!", "Thông báo",
                                MessageBoxButtons.OK, MessageBoxIcon.Error);
                this.Close();
                frmDangNhap loginForm = new frmDangNhap();
                loginForm.ShowDialog();
            }
            else if (UserSession.LaNhapKho)
            {
                btnXuatKho.Enabled = false;
                xuấtKhoToolStripMenuItem.Visible = false;
            }
            else if (UserSession.LaXuatKho)
            {
                btnNhapKho.Enabled = false;
                nhậpKhoToolStripMenuItem.Visible = false;
            }
        }
        private void CapNhatThongBao()
        {
            try
            {
                string connectionString = "Data Source=LAPTOP-UDIS316D\\SQLEXPRESS;Initial Catalog=QuanLyKho_TienLoi;Integrated Security=True";

                using (SqlConnection conn = new SqlConnection(connectionString))
                {
                    conn.Open();
                    SqlCommand cmdSapHetHan = new SqlCommand("SELECT dbo.fn_DemLoSapHetHan()", conn);
                    int soLoSapHetHan = (int)cmdSapHetHan.ExecuteScalar();

                    SqlCommand cmdDaHetHan = new SqlCommand("SELECT dbo.fn_DemLoDaHetHan()", conn);
                    int soLoDaHetHan = (int)cmdDaHetHan.ExecuteScalar();

                    txtSapHetHan.Text = $"{soLoSapHetHan}";
                    txtDaHetHan.Text = $"{soLoDaHetHan}";

                    if (soLoSapHetHan > 0)
                    {
                        txtSapHetHan.ForeColor = System.Drawing.Color.Orange;
                    }

                    if (soLoDaHetHan > 0)
                    {
                        txtDaHetHan.ForeColor = System.Drawing.Color.Red;
                    }
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show("Lỗi khi cập nhật thông báo: " + ex.Message);
            }
        }
        private void OpenChildForm(Form childForm)
        {
            if (currentChildForm != null)
            {
                currentChildForm.Close();
            }

            currentChildForm = childForm;
            childForm.TopLevel = false;
            childForm.FormBorderStyle = FormBorderStyle.None;
            childForm.Dock = DockStyle.Fill;
            panelMain.Controls.Add(childForm);
            panelMain.Tag = childForm;
            childForm.BringToFront();
            childForm.Show();
        }

        private void btnNhapKho_Click(object sender, EventArgs e)
        {
            if (UserSession.LaAdmin || UserSession.LaNhapKho)
            {
                OpenChildForm(new frmPhieuNK());
            }
            else
            {
                MessageBox.Show("Bạn không có quyền truy cập chức năng Nhập kho!", "Thông báo",
                    MessageBoxButtons.OK, MessageBoxIcon.Warning);
            }
        }

        private void btnXuatKho_Click(object sender, EventArgs e)
        {
            if (UserSession.LaAdmin || UserSession.LaXuatKho)
            {
                OpenChildForm(new frmPhieuXK());
            }
            else
            {
                MessageBox.Show("Bạn không có quyền truy cập chức năng Xuất kho!", "Thông báo",
                    MessageBoxButtons.OK, MessageBoxIcon.Warning);
            }
        }

        private void btnBaoCao_Click(object sender, EventArgs e)
        {
            OpenChildForm(new frmBaoCao());
        }
        private void btnNhaCungCap_Click(object sender, EventArgs e)
        {
            OpenChildForm(new frmNhaCungCap());
        }

        private void btnKho_Click(object sender, EventArgs e)
        {
            OpenChildForm(new frmKho());
        }

        private void btnCuaHang_Click(object sender, EventArgs e)
        {
            OpenChildForm(new frmCuaHang());
        }

        private void btnNhanVien_Click(object sender, EventArgs e)
        {
            OpenChildForm(new frmNhanVien());
        }

        private void btnHangHoa_Click(object sender, EventArgs e)
        {
            OpenChildForm(new frmHangHoa());
        }

        private void btnLoHang_Click(object sender, EventArgs e)
        {
            OpenChildForm(new frmLoHang());
        }

        private void btnRefresh_Click(object sender, EventArgs e)
        {
            CapNhatThongBao();
        }
        private void btnDangXuat_Click(object sender, EventArgs e)
        {
            var result = MessageBox.Show(
                "Bạn có chắc chắn muốn đăng xuất?",
                "Xác nhận đăng xuất",
                MessageBoxButtons.YesNo,
                MessageBoxIcon.Question
            );

            if (result == DialogResult.Yes)
            {
                this.Hide();
                UserSession.Reset();
                frmDangNhap loginForm = new frmDangNhap();
                loginForm.ShowDialog();
                this.Close();
            }
        }

        private void btnThoat_Click(object sender, EventArgs e)
        {
            var result = MessageBox.Show(
             "Bạn có chắc chắn muốn thoát?",
             "Xác nhận thoát",
             MessageBoxButtons.YesNo,
             MessageBoxIcon.Warning
         );

            if (result == DialogResult.Yes)
            {
                Close();
            }
        }
    }
}