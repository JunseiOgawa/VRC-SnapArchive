using System.Diagnostics; // Application.StartupPathを使用する場合はSystem.Windows.Formsが必要

namespace SnapArchive
{
    public partial class Form1 : Form
    {
        private FileSystemWatcher? watcher;
        // 連番管理用の辞書（キー: 作成日時の「yyyyMMdd_HHmm」、値: 連番カウント）
        private Dictionary<string, int> sequenceMap = new Dictionary<string, int>();

        public Form1()
        {
            InitializeComponent();
        }

        private void label1_Click(object sender, EventArgs e)
        {

        }
        /// <summary>
        /// ゲーム写真ファイルパスを取得する説明
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void gameFilePath_MouseHover(object sender, EventArgs e)
        {

        }

        private void gameFilePath_MouseLeave(object sender, EventArgs e)
        {

        }

        private void label3_Click(object sender, EventArgs e)
        {

        }
        /// <summary>
        /// ゲーム写真ファイルパスを取得する
        /// ダイアログが開かれてかつOKが押された場合、テキストボックスに選択されたパスを表示する
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>

        private void textBox1_TextChanged(object sender, EventArgs e)
        {

        }

        private void label3_Click_1(object sender, EventArgs e)
        {

        }

        private void textBox2_TextChanged(object sender, EventArgs e)
        {

        }


        private void gameFilePath_button_Click(object sender, EventArgs e)
        {
            if (gameFilePath_Browser.ShowDialog() == DialogResult.OK)
            {
                gameFilePath_textBox.Text = gameFilePath_Browser.SelectedPath;
                SetupFileWatcher(gameFilePath_Browser.SelectedPath);
            }
        }

        private void SetupFileWatcher(string path)
        {
            // 既存のwatcherがあれば停止して破棄
            if (watcher != null)
            {
                watcher.EnableRaisingEvents = false;
                watcher.Dispose();
            }

            // watcher 監視するディレクトリを設定
            watcher = new FileSystemWatcher
            {
                Path = path,
                // 監視するファイルの種類を設定
                NotifyFilter = NotifyFilters.FileName
                            | NotifyFilters.DirectoryName
                            | NotifyFilters.LastWrite,
                Filter = "*.png",  // PNG画像のみを監視
                EnableRaisingEvents = true
            };

            // イベントハンドラーを設定
            watcher.Created += OnFileCreated;
            watcher.Deleted += OnFileDeleted;
            watcher.Changed += OnFileChanged;
            watcher.Renamed += OnFileRenamed;
        }

        private void OnFileCreated(object sender, FileSystemEventArgs e)
        {
            LogFileActivity($"ファイルが作成されました: {e.Name}");
            // ファイル名リネーム機能が有効の場合、リネーム処理を実行
            if (fileReName_checkBox.Checked)
            {
                RenameFile(e.FullPath);
            }
        }

        private void RenameFile(string fullPath)
        {
            try
            {
                // 作成日時を取得
                DateTime creationTime = File.GetCreationTime(fullPath);

                // 作成日時（分単位）をキーとして利用
                string timeKey = creationTime.ToString("yyyyMMdd_HHmm");
                int count = 1;
                if (sequenceMap.ContainsKey(timeKey))
                {
                    count = sequenceMap[timeKey] + 1;
                    sequenceMap[timeKey] = count;
                }
                else
                {
                    sequenceMap[timeKey] = count;
                }
                // 連番をゼロ埋め3桁の文字列に変換
                string sequence = count.ToString("D3");

                string newFileName = string.Empty;

                // fileReName_comboBox から選択したフォーマット文字列を取得
                string format = fileReName_comboBox.SelectedItem?.ToString() ?? "年月日_時分-連番";
                if (string.IsNullOrEmpty(format))
                {
                    // 未選択の場合はデフォルト
                    format = "年月日_時分-連番";
                }

                // 選択フォーマットに応じた名前を生成 (拡張子は元ファイルと同じ)
                switch (format)
                {
                    case "年-月-日-時分-連番":
                        newFileName = creationTime.ToString("yyyy-MM-dd-HHmm") + "-" + sequence;
                        break;
                    case "年_月_日_時分-連番":
                        newFileName = creationTime.ToString("yyyy_MM_dd_HHmm") + "-" + sequence;
                        break;
                    case "年月日_時分-連番":
                        newFileName = creationTime.ToString("yyyyMMdd_HHmm") + "-" + sequence;
                        break;
                    case "年-月-日-曜日-時分-連番":
                        newFileName = creationTime.ToString("yyyy-MM-dd-ddd-HHmm") + "-" + sequence;
                        break;
                    case "日-月-年-時分-連番":
                        newFileName = creationTime.ToString("dd-MM-yyyy-HHmm") + "-" + sequence;
                        break;
                    case "月-日-年-時分-連番":
                        newFileName = creationTime.ToString("MM-dd-yyyy-HHmm") + "-" + sequence;
                        break;
                    case "年.月.日.時分.連番":
                        newFileName = creationTime.ToString("yyyy.MM.dd.HHmm") + "." + sequence;
                        break;
                    case "時分_年月日-連番":
                        newFileName = creationTime.ToString("HHmm_yyyyMMdd") + "-" + sequence;
                        break;
                    default:
                        newFileName = creationTime.ToString("yyyyMMdd_HHmm") + "-" + sequence;
                        break;
                }

                // 元の拡張子を取得して新しいファイル名に付加する
                string ext = Path.GetExtension(fullPath);
                string newFullPath = Path.Combine(Path.GetDirectoryName(fullPath)!, newFileName + ext);

                // 既に同名のファイルがある場合はエラーメッセージを表示
                if (File.Exists(newFullPath))
                {
                    LogFileActivity($"リネーム先のファイルが既に存在します: {newFileName + ext}");
                }
                else
                {
                    File.Move(fullPath, newFullPath);
                    LogFileActivity($"ファイル名をリネームしました: {Path.GetFileName(fullPath)} → {newFileName + ext}");
                }
            }
            catch (Exception ex)
            {
                LogFileActivity($"リネームエラー: {ex.Message}");
            }
        }

        private void OnFileDeleted(object sender, FileSystemEventArgs e)
        {
            LogFileActivity($"ファイルが削除されました: {e.Name}");
        }

        private void OnFileChanged(object sender, FileSystemEventArgs e)
        {
            LogFileActivity($"ファイルが変更されました: {e.Name}");
        }

        private void OnFileRenamed(object sender, RenamedEventArgs e)
        {
            LogFileActivity($"ファイル名が変更されました: {e.OldName} → {e.Name}");
        }

        private void LogFileActivity(string message)
        {
            if (InvokeRequired)
            {
                Invoke(new Action(() =>
                {
                    Console.WriteLine(message);
                    var listBox = Controls.OfType<ListBox>().FirstOrDefault();
                    if (listBox != null)
                    {
                        listBox.Items.Insert(0, $"[{DateTime.Now:HH:mm:ss}] {message}");
                    }
                }));
            }
            else
            {
                Console.WriteLine(message);
                var listBox = Controls.OfType<ListBox>().FirstOrDefault();
                if (listBox != null)
                {
                    listBox.Items.Insert(0, $"[{DateTime.Now:HH:mm:ss}] {message}");
                }
            }
        }

        protected override void OnFormClosing(FormClosingEventArgs e)
        {
            // フォーム終了時にwatcherを破棄
            if (watcher != null)
            {
                watcher.EnableRaisingEvents = false;
                watcher.Dispose();
            }
            base.OnFormClosing(e);
        }

        private void outFilePath_button_Click(object sender, EventArgs e)
        {
            {
                if (outFilePath_Browser.ShowDialog() == DialogResult.OK)
                {
                    outFilePath_textBox.Text = outFilePath_Browser.SelectedPath;
                }
            }
        }

        private void checkBox1_CheckedChanged(object sender, EventArgs e)
        {
            fileSubdivision_Group.Enabled = fileSubdivision_checkBox.Checked;
        }

        private void fileReName_checkBox_CheckedChanged(object sender, EventArgs e)
        {
            fileReName_comboBox.Enabled = fileReName_checkBox.Checked;
        }

        private void fileReName_comboBox_SelectedIndexChanged(object sender, EventArgs e)
        {

        }

        private void radioButton2_CheckedChanged(object sender, EventArgs e)
        {

        }

        private void label1_Click_1(object sender, EventArgs e)
        {

        }

        private void compressor_tab_Click(object sender, EventArgs e)
        {

        }

        // 新しくバックアップ用メソッドを変更
        private void CreateBackup(string fullPath)
        {
            try
            {
                // アプリケーションの実行ディレクトリをルートとする
                string rootDir = Application.StartupPath;
                string tempFolder = Path.Combine(rootDir, "temp");
                if (!Directory.Exists(tempFolder))
                {
                    Directory.CreateDirectory(tempFolder);
                    LogFileActivity($"tempフォルダを作成しました: {tempFolder}");
                }

                // バックアップ対象ファイルのサイズを取得
                long fileSize = new FileInfo(fullPath).Length;
                // tempフォルダがあるドライブの空き容量を確認
                DriveInfo drive = new DriveInfo(Path.GetPathRoot(tempFolder)!);
                if (drive.AvailableFreeSpace < fileSize)
                {
                    MessageBox.Show("一時保存用の空き容量が不足しています。", "エラー", MessageBoxButtons.OK, MessageBoxIcon.Error);
                    Environment.Exit(1);
                }

                string backupPath = Path.Combine(tempFolder, Path.GetFileName(fullPath));
                File.Copy(fullPath, backupPath, overwrite: true);
                LogFileActivity($"バックアップを作成しました: {backupPath}");
            }
            catch (Exception ex)
            {
                LogFileActivity($"バックアップ作成エラー: {ex.Message}");
                MessageBox.Show($"バックアップ作成エラー: {ex.Message}", "エラー", MessageBoxButtons.OK, MessageBoxIcon.Error);
                Environment.Exit(1);
            }
        }
    }
}

