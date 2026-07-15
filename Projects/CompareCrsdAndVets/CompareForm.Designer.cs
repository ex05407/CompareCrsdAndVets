using System.Drawing;
using System.Windows.Forms;

namespace CompareCrsdAndVets
{
    partial class CompareForm
    {
        /// <summary>
        /// 必要なデザイナー変数です。
        /// </summary>
        private System.ComponentModel.IContainer components = null;

        /// <summary>
        /// 使用中のリソースをすべてクリーンアップします。
        /// </summary>
        /// <param name="disposing">マネージド リソースを破棄する場合は true を指定し、その他の場合は false を指定します。</param>
        protected override void Dispose(bool disposing)
        {
            if (disposing && (components != null))
            {
                components.Dispose();
            }
            base.Dispose(disposing);
        }

        #region Windows フォーム デザイナーで生成されたコード

        /// <summary>
        /// デザイナー サポートに必要なメソッドです。このメソッドの内容を
        /// コード エディターで変更しないでください。
        /// </summary>
        private void InitializeComponent()
        {
            this.lblCrsd7000 = new System.Windows.Forms.Label();
            this.txtCrsd7000 = new System.Windows.Forms.TextBox();
            this.btnCrsd7000 = new System.Windows.Forms.Button();
            this.lblTestProcedure = new System.Windows.Forms.Label();
            this.txtTestProcedure = new System.Windows.Forms.TextBox();
            this.btnTestProcedure = new System.Windows.Forms.Button();
            this.btnCompare = new System.Windows.Forms.Button();
            this.btnClose = new System.Windows.Forms.Button();
            this.SuspendLayout();
            // 
            // lblCrsd7000
            // 
            this.lblCrsd7000.AutoSize = true;
            this.lblCrsd7000.Location = new System.Drawing.Point(20, 32);
            this.lblCrsd7000.Name = "lblCrsd7000";
            this.lblCrsd7000.Size = new System.Drawing.Size(171, 18);
            this.lblCrsd7000.TabIndex = 0;
            this.lblCrsd7000.Text = "CRSD7000フォルダパス";
            // 
            // txtCrsd7000
            // 
            this.txtCrsd7000.AllowDrop = true;
            this.txtCrsd7000.Location = new System.Drawing.Point(250, 28);
            this.txtCrsd7000.Margin = new System.Windows.Forms.Padding(3, 2, 3, 2);
            this.txtCrsd7000.Name = "txtCrsd7000";
            this.txtCrsd7000.Size = new System.Drawing.Size(340, 25);
            this.txtCrsd7000.TabIndex = 1;
            this.txtCrsd7000.DragDrop += new System.Windows.Forms.DragEventHandler(this.txtDirPath_DragDrop);
            this.txtCrsd7000.DragEnter += new System.Windows.Forms.DragEventHandler(this.txtDirPath_DragEnter);
            // 
            // btnCrsd7000
            // 
            this.btnCrsd7000.AutoSize = true;
            this.btnCrsd7000.Location = new System.Drawing.Point(600, 28);
            this.btnCrsd7000.Margin = new System.Windows.Forms.Padding(3, 2, 3, 2);
            this.btnCrsd7000.Name = "btnCrsd7000";
            this.btnCrsd7000.Size = new System.Drawing.Size(112, 28);
            this.btnCrsd7000.TabIndex = 2;
            this.btnCrsd7000.Text = "参照";
            this.btnCrsd7000.UseVisualStyleBackColor = true;
            this.btnCrsd7000.Click += new System.EventHandler(this.btnCrsd7000_Click);
            // 
            // lblTestProcedure
            // 
            this.lblTestProcedure.AutoSize = true;
            this.lblTestProcedure.Location = new System.Drawing.Point(20, 92);
            this.lblTestProcedure.Name = "lblTestProcedure";
            this.lblTestProcedure.Size = new System.Drawing.Size(200, 18);
            this.lblTestProcedure.TabIndex = 0;
            this.lblTestProcedure.Text = "TestProcedureフォルダパス";
            // 
            // txtTestProcedure
            // 
            this.txtTestProcedure.AllowDrop = true;
            this.txtTestProcedure.Location = new System.Drawing.Point(250, 88);
            this.txtTestProcedure.Margin = new System.Windows.Forms.Padding(3, 2, 3, 2);
            this.txtTestProcedure.Name = "txtTestProcedure";
            this.txtTestProcedure.Size = new System.Drawing.Size(340, 25);
            this.txtTestProcedure.TabIndex = 3;
            this.txtTestProcedure.DragDrop += new System.Windows.Forms.DragEventHandler(this.txtDirPath_DragDrop);
            this.txtTestProcedure.DragEnter += new System.Windows.Forms.DragEventHandler(this.txtDirPath_DragEnter);
            // 
            // btnTestProcedure
            // 
            this.btnTestProcedure.AllowDrop = true;
            this.btnTestProcedure.AutoSize = true;
            this.btnTestProcedure.Location = new System.Drawing.Point(600, 88);
            this.btnTestProcedure.Margin = new System.Windows.Forms.Padding(3, 2, 3, 2);
            this.btnTestProcedure.Name = "btnTestProcedure";
            this.btnTestProcedure.Size = new System.Drawing.Size(112, 28);
            this.btnTestProcedure.TabIndex = 4;
            this.btnTestProcedure.Text = "参照";
            this.btnTestProcedure.UseVisualStyleBackColor = true;
            this.btnTestProcedure.Click += new System.EventHandler(this.btnTestProcedure_Click);
            // 
            // btnCompare
            // 
            this.btnCompare.AutoSize = true;
            this.btnCompare.Location = new System.Drawing.Point(470, 145);
            this.btnCompare.Margin = new System.Windows.Forms.Padding(3, 2, 3, 2);
            this.btnCompare.Name = "btnCompare";
            this.btnCompare.Size = new System.Drawing.Size(112, 28);
            this.btnCompare.TabIndex = 5;
            this.btnCompare.Text = "比較";
            this.btnCompare.UseVisualStyleBackColor = true;
            this.btnCompare.Click += new System.EventHandler(this.btnCompare_Click);
            // 
            // btnClose
            // 
            this.btnClose.AutoSize = true;
            this.btnClose.Location = new System.Drawing.Point(600, 145);
            this.btnClose.Margin = new System.Windows.Forms.Padding(3, 2, 3, 2);
            this.btnClose.Name = "btnClose";
            this.btnClose.Size = new System.Drawing.Size(112, 28);
            this.btnClose.TabIndex = 6;
            this.btnClose.Text = "終了";
            this.btnClose.UseVisualStyleBackColor = true;
            this.btnClose.Click += new System.EventHandler(this.btnClose_Click);
            // 
            // CompareForm
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(10F, 18F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.AutoSizeMode = System.Windows.Forms.AutoSizeMode.GrowAndShrink;
            this.ClientSize = new System.Drawing.Size(738, 194);
            this.Controls.Add(this.btnClose);
            this.Controls.Add(this.btnCompare);
            this.Controls.Add(this.btnTestProcedure);
            this.Controls.Add(this.txtTestProcedure);
            this.Controls.Add(this.btnCrsd7000);
            this.Controls.Add(this.lblTestProcedure);
            this.Controls.Add(this.txtCrsd7000);
            this.Controls.Add(this.lblCrsd7000);
            this.Margin = new System.Windows.Forms.Padding(3, 2, 3, 2);
            this.MaximizeBox = false;
            this.MinimizeBox = false;
            this.Name = "CompareForm";
            this.Text = "CRSD-7000→VETS比較ツール";
            this.DragDrop += new System.Windows.Forms.DragEventHandler(this.txtDirPath_DragDrop);
            this.DragEnter += new System.Windows.Forms.DragEventHandler(this.txtDirPath_DragEnter);
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private Label lblCrsd7000;
        private TextBox txtCrsd7000;
        private Button btnCrsd7000;
        private Label lblTestProcedure;
        private TextBox txtTestProcedure;
        private Button btnTestProcedure;
        private Button btnCompare;
        private Button btnClose;
    }
}

