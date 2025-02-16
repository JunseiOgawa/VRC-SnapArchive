using System.Diagnostics; // Application.StartupPathを使用する場合はSystem.Windows.Formsが必要
using System.Drawing;
using System.Drawing.Imaging;
using System.Text.RegularExpressions;

namespace SnapArchive
{
    public partial class Form1 : Form
    {
        private FileSystemWatcher? watcher;
        // 連番管理用の辞書（キー: 作成日時の「yyyyMMdd_HHmm」、値: 連番カウント）
        private Dictionary<string, int> sequenceMap = new Dictionary<string, int>();
        private string currentWorld = "Unknown"; // ログより取得したworld名を格納

        public Form1()
        {
            InitializeComponent();
            LoadLatestWorldName();

            // クリックイベントを使わずにステータスバーを自動更新
            toolStripStatusLabel1.Click -= toolStripStatusLabel1_Click;
            toolStripProgressBar1.Click -= toolStripProgressBar1_Click;
            toolStripProgressBar1.Visible = false;

            // 例: 起動時
            UpdateStatus("アプリ起動完了");
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

        // 起動時に最新のVRChatログファイルからworld名を取得するメソッド
        private void LoadLatestWorldName()
        {
            try
            {
                string logFolder = @"C:\Users\junse\AppData\LocalLow\VRChat\VRChat";
                if (!Directory.Exists(logFolder))
                {
                    LogFileActivity("VRChatログフォルダが存在しません。");
                    return;
                }
                // ファイル名が output_log_ で始まるファイルを取得
                var files = Directory.GetFiles(logFolder, "output_log_*");
                if (files.Length == 0)
                {
                    LogFileActivity("VRChatログファイルが見つかりませんでした。");
                    return;
                }
                // ログファイル名例: output_log_2025-02-16_15-07-00
                // ファイル名から日付情報を抽出して最新のファイルを探す
                DateTime latestTime = DateTime.MinValue;
                string latestFile = string.Empty;
                Regex fileRegex = new Regex(@"output_log_(\d{4}-\d{2}-\d{2})_(\d{2}-\d{2}-\d{2})");
                foreach (var file in files)
                {
                    var fileName = Path.GetFileNameWithoutExtension(file);
                    Match m = fileRegex.Match(fileName);
                    if (m.Success)
                    {
                        string datePart = m.Groups[1].Value;
                        string timePart = m.Groups[2].Value.Replace('-', ':');
                        if (DateTime.TryParse($"{datePart} {timePart}", out DateTime dt))
                        {
                            if (dt > latestTime)
                            {
                                latestTime = dt;
                                latestFile = file;
                            }
                        }
                    }
                }
                if (string.IsNullOrEmpty(latestFile))
                {
                    LogFileActivity("最新のVRChatログファイルが取得できませんでした。");
                    return;
                }
                // 最新ログファイルから"Entering Room:" に続くworld名を取得（最後の行を採用）
                string[] logLines = File.ReadAllLines(latestFile);
                foreach (string line in logLines)
                {
                    if (line.Contains("Entering Room:"))
                    {
                        int idx = line.IndexOf("Entering Room:") + "Entering Room:".Length;
                        string world = line.Substring(idx).Trim();
                        if (!string.IsNullOrEmpty(world))
                        {
                            currentWorld = world;
                        }
                    }
                }
                LogFileActivity($"最新のworld名を取得しました: {currentWorld}");
            }
            catch (Exception ex)
            {
                LogFileActivity($"world名取得エラー: {ex.Message}");
            }
        }

        private void OnFileCreated(object sender, FileSystemEventArgs e)
        {
            UpdateStatus($"新規写真検知: {e.Name}");
            LogFileActivity($"ファイルが作成されました: {e.Name}");
            if (fileReName_checkBox.Checked)
            {
                // まずバックアップを作成（戻し用）
                string backupPath = CreateBackup(e.FullPath);
                // バックアップができたら画像処理（EXIF追加＋リネームして出力）
                ProcessImage(e.FullPath, backupPath);
            }
            UpdateStatus($"リネーム完了: {e.Name}");
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

        // バックアップ作成メソッドを戻り値ありに変更
        private string CreateBackup(string fullPath)
        {
            try
            {
                // アプリケーション実行ディレクトリ内の temp フォルダにバックアップを作成
                string rootDir = Application.StartupPath;
                string tempFolder = Path.Combine(rootDir, "temp");
                if (!Directory.Exists(tempFolder))
                {
                    Directory.CreateDirectory(tempFolder);
                    LogFileActivity($"tempフォルダを作成しました: {tempFolder}");
                }
                // バックアップ対象ファイルのサイズを取得
                long fileSize = new FileInfo(fullPath).Length;
                DriveInfo drive = new DriveInfo(Path.GetPathRoot(tempFolder)!);
                if (drive.AvailableFreeSpace < fileSize)
                {
                    MessageBox.Show("一時保存用の空き容量が不足しています。", "エラー", MessageBoxButtons.OK, MessageBoxIcon.Error);
                    Environment.Exit(1);
                }
                string backupPath = Path.Combine(tempFolder, Path.GetFileName(fullPath));
                File.Copy(fullPath, backupPath, overwrite: true);
                LogFileActivity($"バックアップを作成しました: {backupPath}");
                return backupPath;
            }
            catch (Exception ex)
            {
                LogFileActivity($"バックアップ作成エラー: {ex.Message}");
                MessageBox.Show($"バックアップ作成エラー: {ex.Message}", "エラー", MessageBoxButtons.OK, MessageBoxIcon.Error);
                Environment.Exit(1);
                return string.Empty; // 到達しない
            }
        }

        // 画像処理：EXIF更新＋リネーム＋出力を行います
        private void ProcessImage(string fullPath, string backupPath)
        {
            try
            {
                using (Image img = Image.FromFile(fullPath))
                {
                    // EXIF更新：UserComment (タグ: 0x9286) に "world" を追加
                    PropertyItem propItem = GetDummyPropertyItem(img);
                    propItem.Id = 0x9286;
                    propItem.Type = 2; // ASCII
                    string comment = currentWorld;
                    byte[] commentBytes = System.Text.Encoding.ASCII.GetBytes(comment + "\0");
                    propItem.Value = commentBytes;
                    propItem.Len = commentBytes.Length;
                    img.SetPropertyItem(propItem);

                    // リネーム処理（元のロジックを再利用）
                    DateTime creationTime = File.GetCreationTime(fullPath);
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
                    string sequence = count.ToString("D3");
                    string format = fileReName_comboBox.SelectedItem?.ToString() ?? "年月日_時分-連番";
                    string newFileName = format switch
                    {
                        "年-月-日-時分-連番" => creationTime.ToString("yyyy-MM-dd-HHmm") + "-" + sequence,
                        "年_月_日_時分-連番" => creationTime.ToString("yyyy_MM_dd_HHmm") + "-" + sequence,
                        "年月日_時分-連番" => creationTime.ToString("yyyyMMdd_HHmm") + "-" + sequence,
                        "年-月-日-曜日-時分-連番" => creationTime.ToString("yyyy-MM-dd-ddd-HHmm") + "-" + sequence,
                        "日-月-年-時分-連番" => creationTime.ToString("dd-MM-yyyy-HHmm") + "-" + sequence,
                        "月-日-年-時分-連番" => creationTime.ToString("MM-dd-yyyy-HHmm") + "-" + sequence,
                        "年.月.日.時分.連番" => creationTime.ToString("yyyy.MM.dd.HHmm") + "." + sequence,
                        "時分_年月日-連番" => creationTime.ToString("HHmm_yyyyMMdd") + "-" + sequence,
                        _ => creationTime.ToString("yyyyMMdd_HHmm") + "-" + sequence,
                    };

                    // 出力先フォルダの確保（実行ディレクトリ内の output フォルダ）
                    string outputFolder = Path.Combine(Application.StartupPath, "output");
                    if (!Directory.Exists(outputFolder))
                    {
                        Directory.CreateDirectory(outputFolder);
                        LogFileActivity($"出力先フォルダを作成しました: {outputFolder}");
                    }
                    string ext = Path.GetExtension(fullPath);
                    string newFullPath = Path.Combine(outputFolder, newFileName + ext);

                    // 変更後の画像を出力先に保存
                    img.Save(newFullPath);
                    LogFileActivity($"ファイルをリネームして出力しました: {newFullPath}");
                    // 画像処理が完了したので元画像は削除（必要に応じてコメントアウト可）
                    File.Delete(fullPath);
                }
            }
            catch (Exception ex)
            {
                LogFileActivity($"処理エラー: {ex.Message}");
                // エラー時はバックアップから元の画像を復元
                File.Copy(backupPath, fullPath, overwrite: true);
                MessageBox.Show("ファイル処理中にエラーが発生しました。元の画像を復元します。", "エラー", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        // EXIF用の PropertyItem を取得するヘルパーメソッド
        private PropertyItem GetDummyPropertyItem(Image img)
        {
            PropertyItem propItem;
            try
            {
                // 既存のプロパティ項目を利用
                propItem = img.PropertyItems[0];
            }
            catch
            {
                // 空の Bitmap からプロパティ項目を取得（既存項目がない場合のワークアラウンド）
                using (Bitmap bmp = new Bitmap(1, 1))
                {
                    propItem = bmp.PropertyItems[0];
                }
            }
            return propItem;
        }

        private void toolStripProgressBar1_Click(object sender, EventArgs e)
        {

        }

        private void toolStripStatusLabel1_Click(object sender, EventArgs e)
        {

        }

        private void UpdateStatus(string message)
        {
            toolStripStatusLabel1.Text = message;
            Application.DoEvents();
        }

        private void SetProgress(bool visible, int value = 0)
        {
            toolStripProgressBar1.Visible = visible;
            if (visible)
            {
                toolStripProgressBar1.Value = value;
            }
        }
    }
}
