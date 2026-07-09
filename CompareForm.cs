using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace CompareCrsdAndVets
{
    public partial class CompareForm : Form
    {
        public CompareForm()
        {
            InitializeComponent();
        }

        /// <summary>
        /// 「比較」ボタン押下時処理
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void btnCompare_Click(object sender, EventArgs e)
        {
            Project.CompareFiles compare = new Project.CompareFiles();
            string ErrorMessage = string.Empty;

            // エラーチェックを行う
            if (!compare.CheckData(txtCrsd7000.Text, txtTestProcedure.Text, out ErrorMessage))
            {
                ShowError(ErrorMessage);
                return;
            }

            try
            {
                // 処理の実行
                if (!compare.Compare(txtCrsd7000.Text, txtTestProcedure.Text, out ErrorMessage))
                {
                    ShowError(ErrorMessage);
                    return;
                }

                ShowInfo(compare.Mesage);
            }
            catch(Exception ex)
            {
                ShowError(ex.Message);
                return;
            }

        }

        /// <summary>
        /// 「終了」ボタン押下時処理
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        public void btnClose_Click(object sender, EventArgs e)
        {
            this.Close();
        }

        /// <summary>
        /// エラーメッセージを表示する
        /// </summary>
        /// <param name="Message"></param>
        private void ShowError(string Message)
        {
            MessageBox.Show(Message, "エラー", MessageBoxButtons.OK, MessageBoxIcon.Error);
        }

        /// <summary>
        /// 情報メッセージを表示する
        /// </summary>
        /// <param name="Message"></param>
        private void ShowInfo(string Message)
        {
            MessageBox.Show(Message, "比較ツール", MessageBoxButtons.OK, MessageBoxIcon.Information);
        }

        /// <summary>
        /// 「参照」ボタン押下時処理(CRSD7000)
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void btnCrsd7000_Click(object sender, EventArgs e)
        {
            OpenFolderDialog(txtCrsd7000);
        }

        /// <summary>
        /// 「参照」ボタン押下時処理(Test Procedure)
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void btnTestProcedure_Click(object sender, EventArgs e)
        {
            OpenFolderDialog(txtTestProcedure);
        }

        /// <summary>
        /// フォルダダイアログの表示
        /// </summary>
        /// <param name="pTextBox"></param>
        private void OpenFolderDialog(TextBox pTextBox)
        {
            string DirPath = string.Empty;
            if (!string.IsNullOrEmpty(pTextBox.Text) && Directory.Exists(pTextBox.Text))
            {
                DirPath = pTextBox.Text;
            }

            using (FolderBrowserDialog folderDialog = new FolderBrowserDialog())
            {
                folderDialog.SelectedPath = DirPath;
                folderDialog.ShowNewFolderButton = true;

                if (folderDialog.ShowDialog() == DialogResult.OK)
                {
                    pTextBox.Text = folderDialog.SelectedPath;
                }
            }
        }

        /// <summary>
        /// ドラッグ処理
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void txtDirPath_DragEnter(object sender, DragEventArgs e)
        {
            if (e.Data == null) return;

            if (e.Data.GetDataPresent(DataFormats.FileDrop))
            {
                e.Effect = DragDropEffects.Copy;
            }
            else
            {
                e.Effect = DragDropEffects.None;
            }
        }

        /// <summary>
        /// ドラッグ&ドロップ処理
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void txtDirPath_DragDrop(object sender, DragEventArgs e)
        {
            if (e.Data == null) return;
            else if (!e.Data.GetDataPresent(DataFormats.FileDrop)) return;

            string[] dragFilePathArr = (string[])e.Data.GetData(DataFormats.FileDrop, false);
            ((TextBox)sender).Text = dragFilePathArr[0];
        }
    }
}
