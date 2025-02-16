namespace SnapArchive
{
    partial class Form1
    {
        /// <summary>
        ///  Required designer variable.
        /// </summary>
        private System.ComponentModel.IContainer components = null;

        /// <summary>
        ///  Clean up any resources being used.
        /// </summary>
        /// <param name="disposing">true if managed resources should be disposed; otherwise, false.</param>
        protected override void Dispose(bool disposing)
        {
            if (disposing && (components != null))
            {
                components.Dispose();
            }
            base.Dispose(disposing);
        }

        #region Windows Form Designer generated code

        /// <summary>
        ///  Required method for Designer support - do not modify
        ///  the contents of this method with the code editor.
        /// </summary>
        private void InitializeComponent()
        {
            Button gameFilePath_button;
            Button outFilePath_button;
            Button button1;
            gameFilePath_Browser = new FolderBrowserDialog();
            outFilePath_Browser = new FolderBrowserDialog();
            tabControl1 = new TabControl();
            file_tab = new TabPage();
            exifWorldName_label = new Label();
            exifWorldName_checkBox = new CheckBox();
            fileReName_checkBox = new CheckBox();
            fileReName_comboBox = new ComboBox();
            fileSubdivision_Group = new GroupBox();
            radioButton3 = new RadioButton();
            radioButton2 = new RadioButton();
            radioButton1 = new RadioButton();
            fileSubdivision_checkBox = new CheckBox();
            File_label = new Label();
            outFilePath_label = new Label();
            gameFilePath_label = new Label();
            outFilePath_textBox = new TextBox();
            gameFilePath_textBox = new TextBox();
            compressor_tab = new TabPage();
            compressor = new Label();
            textBox1 = new TextBox();
            checkBox1 = new CheckBox();
            gameFilePath_button = new Button();
            outFilePath_button = new Button();
            button1 = new Button();
            tabControl1.SuspendLayout();
            file_tab.SuspendLayout();
            fileSubdivision_Group.SuspendLayout();
            compressor_tab.SuspendLayout();
            SuspendLayout();
            // 
            // gameFilePath_button
            // 
            gameFilePath_button.Location = new Point(283, 73);
            gameFilePath_button.Name = "gameFilePath_button";
            gameFilePath_button.Size = new Size(63, 29);
            gameFilePath_button.TabIndex = 11;
            gameFilePath_button.Text = "参照";
            gameFilePath_button.UseVisualStyleBackColor = true;
            gameFilePath_button.Click += gameFilePath_button_Click;
            // 
            // outFilePath_button
            // 
            outFilePath_button.Location = new Point(283, 145);
            outFilePath_button.Name = "outFilePath_button";
            outFilePath_button.Size = new Size(63, 29);
            outFilePath_button.TabIndex = 13;
            outFilePath_button.Text = "参照";
            outFilePath_button.UseVisualStyleBackColor = true;
            outFilePath_button.Click += outFilePath_button_Click;
            // 
            // gameFilePath_Browser
            // 
            gameFilePath_Browser.RootFolder = Environment.SpecialFolder.MyPictures;
            // 
            // tabControl1
            // 
            tabControl1.Controls.Add(file_tab);
            tabControl1.Controls.Add(compressor_tab);
            tabControl1.Location = new Point(0, 0);
            tabControl1.Name = "tabControl1";
            tabControl1.SelectedIndex = 0;
            tabControl1.Size = new Size(432, 503);
            tabControl1.TabIndex = 7;
            // 
            // file_tab
            // 
            file_tab.Controls.Add(exifWorldName_label);
            file_tab.Controls.Add(exifWorldName_checkBox);
            file_tab.Controls.Add(fileReName_checkBox);
            file_tab.Controls.Add(fileReName_comboBox);
            file_tab.Controls.Add(fileSubdivision_Group);
            file_tab.Controls.Add(fileSubdivision_checkBox);
            file_tab.Controls.Add(File_label);
            file_tab.Controls.Add(outFilePath_label);
            file_tab.Controls.Add(gameFilePath_label);
            file_tab.Controls.Add(outFilePath_textBox);
            file_tab.Controls.Add(gameFilePath_button);
            file_tab.Controls.Add(outFilePath_button);
            file_tab.Controls.Add(gameFilePath_textBox);
            file_tab.Location = new Point(4, 29);
            file_tab.Name = "file_tab";
            file_tab.Padding = new Padding(3);
            file_tab.Size = new Size(424, 470);
            file_tab.TabIndex = 1;
            file_tab.Text = "ファイル";
            file_tab.UseVisualStyleBackColor = true;
            // 
            // exifWorldName_label
            // 
            exifWorldName_label.AutoSize = true;
            exifWorldName_label.Location = new Point(8, 412);
            exifWorldName_label.Name = "exifWorldName_label";
            exifWorldName_label.Size = new Size(343, 20);
            exifWorldName_label.TabIndex = 21;
            exifWorldName_label.Text = "ONにしておくと写真整理の時便利になります。(ON推奨)";
            // 
            // exifWorldName_checkBox
            // 
            exifWorldName_checkBox.AutoSize = true;
            exifWorldName_checkBox.Checked = true;
            exifWorldName_checkBox.CheckState = CheckState.Checked;
            exifWorldName_checkBox.Location = new Point(8, 381);
            exifWorldName_checkBox.Name = "exifWorldName_checkBox";
            exifWorldName_checkBox.Size = new Size(175, 24);
            exifWorldName_checkBox.TabIndex = 20;
            exifWorldName_checkBox.Text = "exifにワールドを追加する";
            exifWorldName_checkBox.UseVisualStyleBackColor = true;
            // 
            // fileReName_checkBox
            // 
            fileReName_checkBox.AutoSize = true;
            fileReName_checkBox.Location = new Point(8, 302);
            fileReName_checkBox.Name = "fileReName_checkBox";
            fileReName_checkBox.Size = new Size(167, 24);
            fileReName_checkBox.TabIndex = 19;
            fileReName_checkBox.Text = "ファイル名をリネームする";
            fileReName_checkBox.UseVisualStyleBackColor = true;
            fileReName_checkBox.CheckedChanged += fileReName_checkBox_CheckedChanged;
            // 
            // fileReName_comboBox
            // 
            fileReName_comboBox.Enabled = false;
            fileReName_comboBox.FormattingEnabled = true;
            fileReName_comboBox.Items.AddRange(new object[] { "年_月_日_時分_連番", "年月日_時分_連番", "年-月-日-曜日-時分-連番", "日-月-年-時分-連番", "月-日-年-時分-連番", "年.月.日.時分.連番", "時分_年月日_連番" });
            fileReName_comboBox.Location = new Point(8, 332);
            fileReName_comboBox.Name = "fileReName_comboBox";
            fileReName_comboBox.Size = new Size(338, 28);
            fileReName_comboBox.TabIndex = 18;
            fileReName_comboBox.SelectedIndexChanged += fileReName_comboBox_SelectedIndexChanged;
            // 
            // fileSubdivision_Group
            // 
            fileSubdivision_Group.Controls.Add(radioButton3);
            fileSubdivision_Group.Controls.Add(radioButton2);
            fileSubdivision_Group.Controls.Add(radioButton1);
            fileSubdivision_Group.Enabled = false;
            fileSubdivision_Group.Location = new Point(8, 219);
            fileSubdivision_Group.Name = "fileSubdivision_Group";
            fileSubdivision_Group.Size = new Size(245, 62);
            fileSubdivision_Group.TabIndex = 17;
            fileSubdivision_Group.TabStop = false;
            fileSubdivision_Group.Text = "フォルダ分け";
            // 
            // radioButton3
            // 
            radioButton3.AutoSize = true;
            radioButton3.Location = new Point(168, 26);
            radioButton3.Name = "radioButton3";
            radioButton3.Size = new Size(75, 24);
            radioButton3.TabIndex = 2;
            radioButton3.Text = "日単位";
            radioButton3.UseVisualStyleBackColor = true;
            // 
            // radioButton2
            // 
            radioButton2.AutoSize = true;
            radioButton2.Location = new Point(87, 26);
            radioButton2.Name = "radioButton2";
            radioButton2.Size = new Size(75, 24);
            radioButton2.TabIndex = 1;
            radioButton2.Text = "週単位";
            radioButton2.UseVisualStyleBackColor = true;
            radioButton2.CheckedChanged += radioButton2_CheckedChanged;
            // 
            // radioButton1
            // 
            radioButton1.AutoSize = true;
            radioButton1.Checked = true;
            radioButton1.Location = new Point(6, 26);
            radioButton1.Name = "radioButton1";
            radioButton1.Size = new Size(75, 24);
            radioButton1.TabIndex = 0;
            radioButton1.TabStop = true;
            radioButton1.Text = "月単位";
            radioButton1.UseVisualStyleBackColor = true;
            // 
            // fileSubdivision_checkBox
            // 
            fileSubdivision_checkBox.AutoSize = true;
            fileSubdivision_checkBox.Location = new Point(8, 189);
            fileSubdivision_checkBox.Name = "fileSubdivision_checkBox";
            fileSubdivision_checkBox.Size = new Size(225, 24);
            fileSubdivision_checkBox.TabIndex = 16;
            fileSubdivision_checkBox.Text = "ファイルを細かくフォルダ分けをする";
            fileSubdivision_checkBox.UseVisualStyleBackColor = true;
            fileSubdivision_checkBox.CheckedChanged += checkBox1_CheckedChanged;
            // 
            // File_label
            // 
            File_label.AutoSize = true;
            File_label.Font = new Font("ＭＳ Ｐ明朝", 13.8F, FontStyle.Bold, GraphicsUnit.Point, 0);
            File_label.Location = new Point(8, 11);
            File_label.Name = "File_label";
            File_label.Size = new Size(91, 23);
            File_label.TabIndex = 9;
            File_label.Text = "ファイル";
            // 
            // outFilePath_label
            // 
            outFilePath_label.AutoSize = true;
            outFilePath_label.Location = new Point(8, 123);
            outFilePath_label.Name = "outFilePath_label";
            outFilePath_label.Size = new Size(137, 20);
            outFilePath_label.TabIndex = 15;
            outFilePath_label.Text = "出力先ファイルを選択";
            // 
            // gameFilePath_label
            // 
            gameFilePath_label.AutoSize = true;
            gameFilePath_label.Location = new Point(8, 51);
            gameFilePath_label.Name = "gameFilePath_label";
            gameFilePath_label.Size = new Size(168, 20);
            gameFilePath_label.TabIndex = 10;
            gameFilePath_label.Text = "ゲーム写真のファイルを選択";
            // 
            // outFilePath_textBox
            // 
            outFilePath_textBox.Location = new Point(8, 146);
            outFilePath_textBox.Name = "outFilePath_textBox";
            outFilePath_textBox.Size = new Size(269, 27);
            outFilePath_textBox.TabIndex = 14;
            // 
            // gameFilePath_textBox
            // 
            gameFilePath_textBox.Location = new Point(8, 74);
            gameFilePath_textBox.Name = "gameFilePath_textBox";
            gameFilePath_textBox.Size = new Size(269, 27);
            gameFilePath_textBox.TabIndex = 12;
            // 
            // compressor_tab
            // 
            compressor_tab.Controls.Add(checkBox1);
            compressor_tab.Controls.Add(compressor);
            compressor_tab.Controls.Add(button1);
            compressor_tab.Controls.Add(textBox1);
            compressor_tab.Location = new Point(4, 29);
            compressor_tab.Name = "compressor_tab";
            compressor_tab.Padding = new Padding(3);
            compressor_tab.Size = new Size(424, 470);
            compressor_tab.TabIndex = 0;
            compressor_tab.Text = "圧縮";
            compressor_tab.UseVisualStyleBackColor = true;
            compressor_tab.Click += compressor_tab_Click;
            // 
            // compressor
            // 
            compressor.AutoSize = true;
            compressor.Font = new Font("ＭＳ Ｐ明朝", 13.8F, FontStyle.Bold, GraphicsUnit.Point, 0);
            compressor.Location = new Point(8, 11);
            compressor.Name = "compressor";
            compressor.Size = new Size(58, 23);
            compressor.TabIndex = 13;
            compressor.Text = "圧縮";
            compressor.Click += label1_Click_1;
            // 
            // button1
            // 
            button1.Location = new Point(292, 174);
            button1.Name = "button1";
            button1.Size = new Size(63, 29);
            button1.TabIndex = 15;
            button1.Text = "参照";
            button1.UseVisualStyleBackColor = true;
            // 
            // textBox1
            // 
            textBox1.Location = new Point(17, 175);
            textBox1.Name = "textBox1";
            textBox1.Size = new Size(269, 27);
            textBox1.TabIndex = 16;
            // 
            // checkBox1
            // 
            checkBox1.AutoSize = true;
            checkBox1.Location = new Point(8, 51);
            checkBox1.Name = "checkBox1";
            checkBox1.Size = new Size(211, 24);
            checkBox1.TabIndex = 17;
            checkBox1.Text = "今月以外のフォルダを圧縮する";
            checkBox1.UseVisualStyleBackColor = true;
            // 
            // Form1
            // 
            AutoScaleDimensions = new SizeF(8F, 20F);
            AutoScaleMode = AutoScaleMode.Font;
            AutoScroll = true;
            ClientSize = new Size(432, 503);
            Controls.Add(tabControl1);
            Name = "Form1";
            Text = "VRC-SnapArchive   フェファ製 ver0.1";
            tabControl1.ResumeLayout(false);
            file_tab.ResumeLayout(false);
            file_tab.PerformLayout();
            fileSubdivision_Group.ResumeLayout(false);
            fileSubdivision_Group.PerformLayout();
            compressor_tab.ResumeLayout(false);
            compressor_tab.PerformLayout();
            ResumeLayout(false);
        }

        #endregion
        private FolderBrowserDialog gameFilePath_Browser;
        private FolderBrowserDialog outFilePath_Browser;
        private TabControl tabControl1;
        private TabPage file_tab;
        private TabPage compressor_tab;
        private GroupBox fileSubdivision_Group;
        private RadioButton radioButton3;
        private RadioButton radioButton2;
        private RadioButton radioButton1;
        private CheckBox fileSubdivision_checkBox;
        private Label File_label;
        private Label outFilePath_label;
        private Label gameFilePath_label;
        private TextBox outFilePath_textBox;
        private TextBox gameFilePath_textBox;
        private ComboBox fileReName_comboBox;
        private CheckBox fileReName_checkBox;
        private Label exifWorldName_label;
        private CheckBox exifWorldName_checkBox;
        private Label compressor;
        private TextBox textBox1;
        private CheckBox checkBox1;
    }
}
