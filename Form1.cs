using System.Diagnostics; // Application.StartupPathを使用する場合はSystem.Windows.Formsが必要
using System.Drawing;
using System.Drawing.Imaging;
using System.Text.RegularExpressions;
using System.Globalization; // 週単位算出に利用
using System.Text.Json;
using System.Text.Json.Serialization;

namespace SnapArchive
{
    public partial class Form1 : Form
    {
        // 設定情報用のクラス
        private class AppSettings
        {
            public string GameFilePath { get; set; } = "";
            public string OutFilePath { get; set; } = "";
            public bool FileSubdivisionChecked { get; set; }
            public bool FileReNameChecked { get; set; }
            public bool ExifEnabled { get; set; } = true; // Exif全体の有効/無効
            public bool ExifWorldNameChecked { get; set; }
            public bool ExifDateTimeChecked { get; set; }
            // "Month", "Week", "Day" のいずれか
            public string SubdivisionType { get; set; } = "";
            public string RenameFormat { get; set; } = "";
        }

        // 設定ファイルのパス（実行フォルダに保存する例）
        private readonly string settingsFilePath = Path.Combine(Application.StartupPath, "AppSettings.json");

        private FileSystemWatcher? watcher;
        private Dictionary<string, int> sequenceMap = new Dictionary<string, int>();
        private string currentWorld = "Unknown"; // ログより取得したworld名を格納

        public Form1()
        {
            InitializeComponent();
            LoadSettings();

            // 保存されているパスがあれば、監視を開始
            if (!string.IsNullOrEmpty(gameFilePath_textBox.Text))
            {
                SetupFileWatcher(gameFilePath_textBox.Text);
            }

            LoadLatestWorldName();

            toolStripProgressBar1.Visible = false;

            // Exifグループボックスの有効状態を初期設定
            Exif_groupBox.Enabled = Exif_CheckBox.Checked;
            
            // デフォルトでEXIF情報追加を有効に（設定ファイルでの上書きがなければ）
            if (!File.Exists(settingsFilePath))
            {
                Exif_CheckBox.Checked = true;
                exifWorldName_checkBox.Checked = true;
            }

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
            try
            {
                if (string.IsNullOrEmpty(path))
                {
                    LogFileActivity("エラー: 監視パスが空です");
                    return;
                }

                if (!Directory.Exists(path))
                {
                    LogFileActivity($"エラー: 監視フォルダが存在しません: {path}");
                    return;
                }

                // 既存のwatcherがあれば停止して破棄
                if (watcher != null)
                {
                    watcher.EnableRaisingEvents = false;
                    watcher.Dispose();
                }

                // 新しいwatcherの設定
                watcher = new FileSystemWatcher
                {
                    Path = path,
                    NotifyFilter = NotifyFilters.FileName
                                | NotifyFilters.LastWrite
                                | NotifyFilters.CreationTime,
                    Filter = "*.png",
                    InternalBufferSize = 65536
                };

                // イベントハンドラーの設定
                watcher.Created += OnFileCreated;
                watcher.Error += OnWatcherError;

                // 監視を開始
                watcher.EnableRaisingEvents = true;

                LogFileActivity($"ファイル監視を開始しました: {path}");
                UpdateStatus($"監視中: {path}");
            }
            catch (Exception ex)
            {
                LogFileActivity($"ファイル監視の設定でエラーが発生: {ex.Message}");
                MessageBox.Show($"ファイル監視の設定に失敗しました。\n{ex.Message}", "エラー",
                    MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        // エラーハンドラーの追加
        private void OnWatcherError(object sender, ErrorEventArgs e)
        {
            LogFileActivity($"ファイル監視エラー: {e.GetException().Message}");

            // 監視の再開を試みる
            if (watcher != null)
            {
                watcher.EnableRaisingEvents = false;
                Thread.Sleep(1000); // 1秒待機
                watcher.EnableRaisingEvents = true;
                LogFileActivity("ファイル監視を再開しました");
            }
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

        private bool IsFileReady(string filePath)
        {
            try
            {
                using (var stream = new FileStream(filePath, FileMode.Open, FileAccess.ReadWrite, FileShare.None))
                {
                    return stream.Length > 0;
                }
            }
            catch (IOException)
            {
                return false;
            }
        }

        private void OnFileCreated(object sender, FileSystemEventArgs e)
        {
            LogFileActivity($"ファイル検知開始: {e.FullPath}");

            try
            {
                // ファイル書き込み完了まで待機
                int waitRetry = 0;
                while (!IsFileReady(e.FullPath) && waitRetry < 50)
                {
                    Thread.Sleep(100);
                    waitRetry++;
                    LogFileActivity($"ファイル準備待機: {waitRetry}/50");
                }

                if (!IsFileReady(e.FullPath))
                {
                    LogFileActivity("タイムアウト: ファイルがアクセス可能になりませんでした");
                    return;
                }

                // バックアップを作成
                string backupPath = CreateBackup(e.FullPath);
                LogFileActivity($"バックアップ作成完了: {backupPath}");

                // バックアップファイルの書き込み完了確認
                waitRetry = 0;
                while (!IsFileReady(backupPath) && waitRetry < 50)
                {
                    Thread.Sleep(100);
                    waitRetry++;
                    LogFileActivity($"バックアップファイル準備待機: {waitRetry}/50");
                }

                if (!IsFileReady(backupPath))
                {
                    LogFileActivity("タイムアウト: バックアップファイルがアクセス可能になりませんでした");
                    return;
                }

                // tempファイルを処理（EXIF追加、リネーム）
                ProcessTempFile(backupPath);

                LogFileActivity($"ファイル処理完了: {e.Name}");
            }
            catch (Exception ex)
            {
                LogFileActivity($"OnFileCreated エラー: {ex.GetType().Name} - {ex.Message}");
                LogFileActivity($"スタックトレース: {ex.StackTrace}");
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
            SaveSettings();
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
                    // リネーム処理
                    DateTime creationTime = File.GetCreationTime(fullPath);
                    string timeKey = creationTime.ToString("yyyyMMdd_HHmm");
                    int count;
                    if (sequenceMap.ContainsKey(timeKey))
                    {
                        count = sequenceMap[timeKey] + 1;
                        sequenceMap[timeKey] = count;
                    }
                    else
                    {
                        count = 1;
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

                    // 一時処理フォルダとして tempフォルダを使用
                    string tempProcessFolder = Path.Combine(Application.StartupPath, "temp");
                    if (!Directory.Exists(tempProcessFolder))
                    {
                        Directory.CreateDirectory(tempProcessFolder);
                        LogFileActivity($"一時処理フォルダを作成しました: {tempProcessFolder}");
                    }

                    string ext = Path.GetExtension(fullPath);
                    string tempFilePath = Path.Combine(tempProcessFolder, newFileName + ext);

                    // 変更後の画像を一時的に保存
                    img.Save(tempFilePath);
                    LogFileActivity($"処理済み画像を一時フォルダに保存: {tempFilePath}");
                    
                    // 元画像は削除（必要に応じてコメントアウト可）
                    File.Delete(fullPath);
                    
                    // 処理済みファイルを出力先へ移動
                    MoveTempFileToOutput(tempFilePath);
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

        private async void ProcessImageAsync(string fullPath, string backupPath)
        {
            try
            {
                BeginInvoke((Action)(() =>
                {
                    toolStripStatusLabel1.Text = $"画像処理開始: {Path.GetFileName(fullPath)}";
                }));

                string formatLocal = fileReName_comboBox.SelectedItem?.ToString() ?? "年月日_時分-連番";
                bool isSubdivisionChecked = fileSubdivision_checkBox.Checked;
                bool radioMonth = radioButton1.Checked;
                bool radioWeek = radioButton2.Checked;
                bool radioDay = radioButton3.Checked;

                await Task.Run(() =>
                {
                    using (Image img = Image.FromFile(fullPath))
                    {
                        // リネーム処理
                        DateTime creationTime = File.GetCreationTime(fullPath);
                        string timeKey = creationTime.ToString("yyyyMMdd_HHmm");
                        int count;
                        lock (sequenceMap)
                        {
                            if (sequenceMap.ContainsKey(timeKey))
                            {
                                count = sequenceMap[timeKey] + 1;
                                sequenceMap[timeKey] = count;
                            }
                            else
                            {
                                count = 1;
                                sequenceMap[timeKey] = count;
                            }
                        }
                        string sequence = count.ToString("D3");
                        string newFileName = formatLocal switch
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

                        // 一時処理フォルダ
                        string tempProcessFolder = Path.Combine(Application.StartupPath, "temp");
                        if (!Directory.Exists(tempProcessFolder))
                        {
                            Directory.CreateDirectory(tempProcessFolder);
                            LogFileActivity($"一時処理フォルダを作成しました: {tempProcessFolder}");
                        }

                        string ext = Path.GetExtension(fullPath);
                        string tempFilePath = Path.Combine(tempProcessFolder, newFileName + ext);

                        // 変更後の画像を一時フォルダに保存
                        img.Save(tempFilePath);
                        
                        // 元画像は削除
                        File.Delete(fullPath);
                        
                        // 処理済みファイルを出力先へ移動
                        MoveTempFileToOutput(tempFilePath);
                        LogFileActivity($"ファイルを処理して出力しました");
                    }
                });

                BeginInvoke((Action)(() =>
                {
                    toolStripStatusLabel1.Text = $"画像処理完了: {Path.GetFileName(fullPath)}";
                }));
            }
            catch (Exception ex)
            {
                BeginInvoke((Action)(() =>
                {
                    toolStripStatusLabel1.Text = $"処理エラー: {ex.Message}";
                }));
                File.Copy(backupPath, fullPath, true);
                MessageBox.Show("ファイル処理中にエラーが発生しました。元の画像を復元します。", "エラー", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        // 設定を JSON に保存するメソッド
        private void SaveSettings()
        {
            AppSettings settings = new AppSettings()
            {
                GameFilePath = gameFilePath_textBox.Text,
                OutFilePath = outFilePath_textBox.Text,
                FileSubdivisionChecked = fileSubdivision_checkBox.Checked,
                FileReNameChecked = fileReName_checkBox.Checked,
                ExifEnabled = Exif_CheckBox.Checked, // EXIF全体の有効/無効
                ExifWorldNameChecked = exifWorldName_checkBox.Checked,
                ExifDateTimeChecked = (Controls.Find("exifDateTime_checkBox", true).FirstOrDefault() as CheckBox)?.Checked ?? true, // exifDateTime_checkBoxが存在しなければデフォルトでtrue
                SubdivisionType = radioButton1.Checked ? "Month"
                                : radioButton2.Checked ? "Week"
                                : radioButton3.Checked ? "Day" : "",
                RenameFormat = fileReName_comboBox.SelectedItem?.ToString() ?? ""
            };
            var options = new JsonSerializerOptions { WriteIndented = true };
            string json = JsonSerializer.Serialize(settings, options);
            File.WriteAllText(settingsFilePath, json);
            LogFileActivity($"設定を保存しました: {settingsFilePath}");
        }

        // 設定を JSON から読み込むメソッド
        private void LoadSettings()
        {
            try
            {
                if (File.Exists(settingsFilePath))
                {
                    string json = File.ReadAllText(settingsFilePath);
                    AppSettings? settings = JsonSerializer.Deserialize<AppSettings>(json);
                    if (settings != null)
                    {
                        // テキストボックスのパス設定を復元
                        gameFilePath_textBox.Text = settings.GameFilePath;
                        outFilePath_textBox.Text = settings.OutFilePath;

                        // チェックボックスの状態を復元
                        fileSubdivision_checkBox.Checked = settings.FileSubdivisionChecked;
                        fileReName_checkBox.Checked = settings.FileReNameChecked;
                        
                        // EXIFグループとサブ設定の復元
                        Exif_CheckBox.Checked = settings.ExifEnabled;
                        exifWorldName_checkBox.Checked = settings.ExifWorldNameChecked;
                        // exifDateTime_checkBox is not defined so its value is not restored.
                        if (Controls.Find("exifDateTime_checkBox", true).FirstOrDefault() is CheckBox dateTimeCheckBox)
                        {
                            dateTimeCheckBox.Checked = settings.ExifDateTimeChecked;
                        }

                        // EXIFグループボックスの有効/無効状態を更新
                        Exif_groupBox.Enabled = Exif_CheckBox.Checked;

                        // ラジオボタンの状態を復元
                        if (settings.SubdivisionType == "Month")
                        {
                            radioButton1.Checked = true;
                        }
                        else if (settings.SubdivisionType == "Week")
                        {
                            radioButton2.Checked = true;
                        }
                        else if (settings.SubdivisionType == "Day")
                        {
                            radioButton3.Checked = true;
                        }

                        // コンボボックスのリネーム形式設定を復元
                        if (!string.IsNullOrEmpty(settings.RenameFormat))
                        {
                            foreach (var item in fileReName_comboBox.Items)
                            {
                                if (item.ToString() == settings.RenameFormat)
                                {
                                    fileReName_comboBox.SelectedItem = item;
                                    break;
                                }
                            }
                        }

                        // UIの有効/無効状態を更新
                        fileSubdivision_Group.Enabled = fileSubdivision_checkBox.Checked;
                        fileReName_comboBox.Enabled = fileReName_checkBox.Checked;

                        LogFileActivity($"設定を読み込みました: {settingsFilePath}");
                    }
                }
                else
                {
                    LogFileActivity("設定ファイルが存在しません。初回起動か設定が保存されていません。");
                }
            }
            catch (Exception ex)
            {
                LogFileActivity($"設定ファイルの読み込みでエラーが発生しました: {ex.Message}");
                MessageBox.Show("設定の読み込みに失敗しました。デフォルト設定で起動します。",
                    "設定読み込みエラー", MessageBoxButtons.OK, MessageBoxIcon.Warning);
            }
        }

        private void ProcessTempFile(string tempFilePath)
        {
            LogFileActivity($"ProcessTempFile: 処理開始 - {tempFilePath}");

            try
            {
                if (!File.Exists(tempFilePath))
                {
                    LogFileActivity($"エラー: tempファイルがありません - {tempFilePath}");
                    return;
                }

                string? renamedTempPath = null;
                string fileName = Path.GetFileNameWithoutExtension(tempFilePath);
                string ext = Path.GetExtension(tempFilePath);
                DateTime creationTime = File.GetCreationTime(tempFilePath);
                
                // 画像の読み込み
                using (Image img = Image.FromFile(tempFilePath))
                {
                    // [Step 1] EXIF情報の追加処理（エラーが発生しても続行する）
                    if (Exif_CheckBox.Checked)
                    {
                        try
                        {
                            if (exifWorldName_checkBox.Checked)
                            {
                                try 
                                {
                                    AddExifWorldInfo(img);
                                    LogFileActivity("ワールド名情報をEXIFに追加しました");
                                }
                                catch (Exception ex)
                                {
                                    LogFileActivity($"ワールド情報の追加に失敗しましたが、処理を継続します: {ex.Message}");
                                }
                            }
                            
                            try
                            {
                                AddExifDateTimeInfo(img, tempFilePath);
                                LogFileActivity("日時情報をEXIFに追加しました");
                            }
                            catch (Exception ex)
                            {
                                LogFileActivity($"日時情報の追加に失敗しましたが、処理を継続します: {ex.Message}");
                            }
                        }
                        catch (Exception ex)
                        {
                            LogFileActivity($"EXIF処理でエラーが発生しましたが、リネーム処理は続行します: {ex.Message}");
                        }
                    }

                    // [Step 2] リネーム処理（EXIF処理の成功/失敗にかかわらず実行）
                    if (fileReName_checkBox.Checked)
                    {
                        string newFileName;
                        try
                        {
                            // 通常のファイル名生成を試みる
                            newFileName = GenerateFileNameSafe(img, tempFilePath);
                        }
                        catch (Exception)
                        {
                            // 例外が発生した場合はシンプルな名前生成を使用
                            newFileName = FallbackFileNameGenerator(tempFilePath);
                        }
                        
                        string? tempDir = Path.GetDirectoryName(tempFilePath);
                        if (tempDir != null)
                        {
                            renamedTempPath = Path.Combine(tempDir, newFileName + ext);

                            if (File.Exists(renamedTempPath))
                            {
                                File.Delete(renamedTempPath);
                            }

                            // 変更を保存
                            try
                            {
                                img.Save(renamedTempPath);
                                LogFileActivity($"temp内でリネーム保存: {renamedTempPath}");
                            }
                            catch (Exception ex)
                            {
                                LogFileActivity($"画像保存でエラー発生: {ex.Message} - ファイルコピーを試みます");
                                // 画像保存に失敗した場合は単純コピー
                                File.Copy(tempFilePath, renamedTempPath, true);
                                LogFileActivity($"単純コピーでリネーム: {renamedTempPath}");
                            }
                        }
                    }
                    else
                    {
                        // EXIF情報のみ追加した場合は上書き保存を試みる
                        try
                        {
                            img.Save(tempFilePath);
                            LogFileActivity("EXIF情報を付加しました（リネームなし）");
                        }
                        catch (Exception ex)
                        {
                            LogFileActivity($"EXIF情報のみの保存に失敗: {ex.Message}");
                        }
                    }
                }

                // ファイルハンドルを確実に閉じるためにusingブロックの外で処理
                if (renamedTempPath != null && File.Exists(renamedTempPath))
                {
                    if (File.Exists(tempFilePath))
                    {
                        try { File.Delete(tempFilePath); } catch { }
                    }
                    MoveTempFileToOutput(renamedTempPath);
                }
                else
                {
                    MoveTempFileToOutput(tempFilePath);
                }
            }
            catch (Exception ex)
            {
                LogFileActivity($"ProcessTempFile 全体でエラー: {ex.Message}");
                // 最後の手段として元ファイルを出力先に移動
                try
                {
                    MoveTempFileToOutput(tempFilePath);
                }
                catch
                {
                    LogFileActivity("ファイル処理に完全に失敗しました");
                }
            }
        }

        // 安全なファイル名生成（例外が発生してもError_Namedにならない）
        private string GenerateFileNameSafe(Image img, string originalPath)
        {
            try
            {
                // 通常のGenerateFileNameを使用
                return GenerateFileName(img, originalPath);
            }
            catch (Exception ex)
            {
                LogFileActivity($"通常のファイル名生成でエラー: {ex.Message} - フォールバック処理を使用します");
                return FallbackFileNameGenerator(originalPath);
            }
        }

        // フォールバック用のシンプルなファイル名生成器
        private string FallbackFileNameGenerator(string originalPath)
        {
            try
            {
                // 元ファイル名から日付パターンを探す
                string fileName = Path.GetFileNameWithoutExtension(originalPath);
                DateTime imageDate;
                
                // VRChatのファイル名パターン（VRChat_2023-02-27_12-34-56形式）を確認
                Regex dateRegex = new Regex(@"VRChat_(\d{4})-(\d{2})-(\d{2})_(\d{2})-(\d{2})-(\d{2})(?:\.(\d+))?(?:_\d+x\d+)?");
                var match = dateRegex.Match(fileName);
                
                if (match.Success)
                {
                    try {
                        imageDate = new DateTime(
                            int.Parse(match.Groups[1].Value),
                            int.Parse(match.Groups[2].Value),
                            int.Parse(match.Groups[3].Value),
                            int.Parse(match.Groups[4].Value),
                            int.Parse(match.Groups[5].Value),
                            int.Parse(match.Groups[6].Value)
                        );
                        LogFileActivity("フォールバック: ファイル名から日時を抽出しました");
                    }
                    catch {
                        // 数値変換エラーの場合はファイル作成日時を使用
                        imageDate = File.GetCreationTime(originalPath);
                        LogFileActivity("フォールバック: 日時パース失敗、ファイル作成日時を使用します");
                    }
                }
                else
                {
                    // パターンに一致しない場合はファイル作成日時を使用
                    imageDate = File.GetCreationTime(originalPath);
                    LogFileActivity("フォールバック: ファイル名パターン不一致、作成日時を使用します");
                }

                // 連番生成
                string timeKey = imageDate.ToString("yyyyMMdd_HHmm");
                int count;
                lock (sequenceMap)
                {
                    if (sequenceMap.ContainsKey(timeKey))
                    {
                        count = sequenceMap[timeKey] + 1;
                        sequenceMap[timeKey] = count;
                    }
                    else
                    {
                        count = 1;
                        sequenceMap[timeKey] = count;
                    }
                }
                
                string sequence = count.ToString("D3");
                string format = fileReName_comboBox.SelectedItem?.ToString() ?? "年月日_時分-連番";
                string prefix = "VRChat_";

                // 選択されたフォーマットに基づいて名前を生成
                string newFileName = format switch
                {
                    "年-月-日-時分-連番" => $"{prefix}{imageDate:yyyy-MM-dd-HHmm}-{sequence}",
                    "年_月_日_時分-連番" => $"{prefix}{imageDate:yyyy_MM_dd_HHmm}-{sequence}",
                    "年月日_時分-連番" => $"{prefix}{imageDate:yyyyMMdd_HHmm}-{sequence}",
                    "年-月-日-曜日-時分-連番" => $"{prefix}{imageDate:yyyy-MM-dd-ddd-HHmm}-{sequence}",
                    "日-月-年-時分-連番" => $"{prefix}{imageDate:dd-MM-yyyy-HHmm}-{sequence}",
                    "月-日-年-時分-連番" => $"{prefix}{imageDate:MM-dd-yyyy-HHmm}-{sequence}",
                    "年.月.日.時分.連番" => $"{prefix}{imageDate:yyyy.MM.dd.HHmm}.{sequence}",
                    "時分_年月日-連番" => $"{prefix}{imageDate:HHmm_yyyyMMdd}-{sequence}",
                    _ => $"{prefix}{imageDate:yyyyMMdd_HHmm}-{sequence}"
                };
                
                LogFileActivity($"フォールバック処理で生成されたファイル名: {newFileName}");
                return newFileName;
            }
            catch
            {
                // 絶対に失敗しない最終手段
                string prefix = "VRChat_";
                string timestamp = DateTime.Now.ToString("yyyyMMdd_HHmmss");
                string randomPart = Path.GetRandomFileName().Replace(".", "").Substring(0, 5);
                return $"{prefix}{timestamp}_{randomPart}";
            }
        }

        // (1) temp上でEXIFやリネーム完了後、最終的に出力フォルダへ移動するメソッドを新規追加
        private void MoveTempFileToOutput(string tempFilePath)
        {
            try
            {
                LogFileActivity($"MoveTempFileToOutput: 開始 {tempFilePath}");

                if (!File.Exists(tempFilePath))
                {
                    LogFileActivity("MoveTempFileToOutput: ファイルが存在しません");
                    return;
                }

                // 出力先フォルダ - outFilePath_textBoxの値を使用
                string outputFolder = string.IsNullOrEmpty(outFilePath_textBox.Text) 
                    ? Path.Combine(Application.StartupPath, "output") // デフォルト出力先
                    : outFilePath_textBox.Text; // ユーザー指定の出力先

                if (!Directory.Exists(outputFolder))
                {
                    Directory.CreateDirectory(outputFolder);
                    LogFileActivity($"出力先フォルダを作成しました: {outputFolder}");
                }

                // フォルダ分けオプションが有効な場合、サブフォルダを作成
                if (fileSubdivision_checkBox.Checked)
                {
                    DateTime fileDateTime = File.GetCreationTime(tempFilePath);
                    string subFolder = "";

                    if (radioButton1.Checked) // 月単位
                    {
                        subFolder = fileDateTime.ToString("yyyy-MM");
                    }
                    else if (radioButton2.Checked) // 週単位
                    {
                        var dfi = DateTimeFormatInfo.CurrentInfo;
                        var cal = dfi.Calendar;
                        int weekNo = cal.GetWeekOfYear(fileDateTime, dfi.CalendarWeekRule, dfi.FirstDayOfWeek);
                        subFolder = $"{fileDateTime.Year}_Week{weekNo:D2}";
                    }
                    else if (radioButton3.Checked) // 日単位
                    {
                        subFolder = fileDateTime.ToString("yyyyMMdd");
                    }

                    if (!string.IsNullOrEmpty(subFolder))
                    {
                        outputFolder = Path.Combine(outputFolder, subFolder);
                        if (!Directory.Exists(outputFolder))
                        {
                            Directory.CreateDirectory(outputFolder);
                            LogFileActivity($"フォルダ分け用サブフォルダを作成しました: {outputFolder}");
                        }
                    }
                }

                // ファイル名はtempファイルの名前をそのまま使用
                string fileName = Path.GetFileName(tempFilePath);
                string ext = Path.GetExtension(fileName);
                string nameNoExt = Path.GetFileNameWithoutExtension(fileName);
                string finalFilePath = Path.Combine(outputFolder, fileName);

                // 同名ファイルが存在する場合は末尾に連番を付加
                int dupCount = 1;
                while (File.Exists(finalFilePath))
                {
                    finalFilePath = Path.Combine(outputFolder, $"{nameNoExt}_{dupCount++}{ext}");
                }

                // 移動処理
                File.Move(tempFilePath, finalFilePath);
                LogFileActivity($"処理済みファイルを出力先へ移動しました: {finalFilePath}");
            }
            catch (Exception ex)
            {
                LogFileActivity($"MoveTempFileToOutput エラー: {ex.Message}");
            }
        }

        private void CheckFileLock(string filePath)
        {
            try
            {
                using (var fs = new FileStream(filePath, FileMode.Open, FileAccess.ReadWrite, FileShare.None))
                {
                    // 開ければロックされていない
                }
            }
            catch (IOException)
            {
                // ロックされている可能性あり
            }
        }

        // GenerateFileName メソッドの修正
        private string GenerateFileName(Image img, string originalPath)
        {
            try
            {
                // ファイル名から日時情報を抽出 - 優先的に使用
                string fileName = Path.GetFileNameWithoutExtension(originalPath);
                DateTime imageDate;
                
                // 新しい正規表現パターン - ミリ秒部分と解像度部分に対応
                Regex dateRegex = new Regex(@"VRChat_(\d{4})-(\d{2})-(\d{2})_(\d{2})-(\d{2})-(\d{2})(?:\.(\d+))?(?:_\d+x\d+)?");
                var match = dateRegex.Match(fileName);

                if (match.Success)
                {
                    try
                    {
                        imageDate = new DateTime(
                            int.Parse(match.Groups[1].Value),  // 年
                            int.Parse(match.Groups[2].Value),  // 月
                            int.Parse(match.Groups[3].Value),  // 日
                            int.Parse(match.Groups[4].Value),  // 時
                            int.Parse(match.Groups[5].Value),  // 分
                            int.Parse(match.Groups[6].Value)   // 秒
                        );
                        
                        // ミリ秒があれば追加
                        if (match.Groups.Count > 7 && match.Groups[7].Success)
                        {
                            string msStr = match.Groups[7].Value;
                            if (msStr.Length > 3) msStr = msStr.Substring(0, 3);
                            while (msStr.Length < 3) msStr += "0";
                            
                            imageDate = imageDate.AddMilliseconds(int.Parse(msStr));
                        }
                        
                        LogFileActivity("ファイル名から日時を取得しました");
                    }
                    catch (Exception ex)
                    {
                        LogFileActivity($"ファイル名からの日時解析エラー: {ex.Message}");
                        imageDate = File.GetCreationTime(originalPath);
                    }
                }
                else
                {
                    // ファイル名から日時を抽出できなければファイルの作成日時を使用
                    imageDate = File.GetCreationTime(originalPath);
                    LogFileActivity("ファイル名から日時を抽出できなかったため、作成日時を使用します");
                }

                string timeKey = imageDate.ToString("yyyyMMdd_HHmm");
                int count;
                lock (sequenceMap)
                {
                    if (sequenceMap.ContainsKey(timeKey))
                    {
                        count = sequenceMap[timeKey] + 1;
                        sequenceMap[timeKey] = count;
                    }
                    else
                    {
                        count = 1;
                        sequenceMap[timeKey] = count;
                    }
                }

                string sequence = count.ToString("D3");
                string format = fileReName_comboBox.SelectedItem?.ToString() ?? "年月日_時分-連番";

                // VRChatプレフィックスを維持
                string prefix = "VRChat_";
                string newFileName = format switch
                {
                    "年-月-日-時分-連番" => $"{prefix}{imageDate:yyyy-MM-dd-HHmm}-{sequence}",
                    "年_月_日_時分-連番" => $"{prefix}{imageDate:yyyy_MM_dd_HHmm}-{sequence}",
                    "年月日_時分-連番" => $"{prefix}{imageDate:yyyyMMdd_HHmm}-{sequence}",
                    "年-月-日-曜日-時分-連番" => $"{prefix}{imageDate:yyyy-MM-dd-ddd-HHmm}-{sequence}",
                    "日-月-年-時分-連番" => $"{prefix}{imageDate:dd-MM-yyyy-HHmm}-{sequence}",
                    "月-日-年-時分-連番" => $"{prefix}{imageDate:MM-dd-yyyy-HHmm}-{sequence}",
                    "年.月.日.時分.連番" => $"{prefix}{imageDate:yyyy.MM.dd.HHmm}.{sequence}",
                    "時分_年月日-連番" => $"{prefix}{imageDate:HHmm_yyyyMMdd}-{sequence}",
                    _ => $"{prefix}{imageDate:yyyyMMdd_HHmm}-{sequence}"
                };

                LogFileActivity($"生成されたファイル名: {newFileName}");
                return newFileName;
            }
            catch (Exception ex)
            {
                // エラー時も正常なファイル名を返す（Error_Named_は使わない）
                LogFileActivity($"ファイル名生成エラー: {ex.Message}");
                
                DateTime now = DateTime.Now;
                string timeKey = now.ToString("yyyyMMdd_HHmm");
                int count;
                lock (sequenceMap)
                {
                    if (sequenceMap.ContainsKey(timeKey))
                    {
                        count = sequenceMap[timeKey] + 1;
                        sequenceMap[timeKey] = count;
                    }
                    else
                    {
                        count = 1;
                        sequenceMap[timeKey] = count;
                    }
                }
                
                string sequence = count.ToString("D3");
                return $"VRChat_{now:yyyyMMdd_HHmm}-{sequence}";
            }
        }

        // AddExifDateTimeInfo メソッドも同様に更新
        private void AddExifDateTimeInfo(Image img, string filePath)
        {
            // 親チェックボックスがオフの場合は実行しない
            if (!Exif_CheckBox.Checked) return;

            try
            {
                // ファイル名からの日時情報を抽出
                string fileName = Path.GetFileNameWithoutExtension(filePath);
                
                // 修正された正規表現パターン
                Regex dateRegex = new Regex(@"VRChat_(\d{4})-(\d{2})-(\d{2})_(\d{2})-(\d{2})-(\d{2})(?:\.(\d+))?(?:_\d+x\d+)?");
                var match = dateRegex.Match(fileName);

                if (!match.Success)
                {
                    LogFileActivity("ファイル名から日時情報を抽出できませんでした。EXIF日時は追加されません。");
                    return;
                }

                // 日時情報の生成
                DateTime captureTime;
                try
                {
                    captureTime = new DateTime(
                        int.Parse(match.Groups[1].Value),  // 年
                        int.Parse(match.Groups[2].Value),  // 月
                        int.Parse(match.Groups[3].Value),  // 日
                        int.Parse(match.Groups[4].Value),  // 時
                        int.Parse(match.Groups[5].Value),  // 分
                        int.Parse(match.Groups[6].Value)   // 秒
                    );
                    
                    // ミリ秒の処理（あれば）
                    if (match.Groups.Count > 7 && match.Groups[7].Success)
                    {
                        string msStr = match.Groups[7].Value;
                        if (msStr.Length > 3) msStr = msStr.Substring(0, 3);
                        while (msStr.Length < 3) msStr += "0";
                        
                        captureTime = captureTime.AddMilliseconds(int.Parse(msStr));
                    }
                }
                catch (Exception ex)
                {
                    LogFileActivity($"日時のパースに失敗: {ex.Message}");
                    return;
                }

                // EXIF日時情報のフォーマット（yyyy:MM:dd HH:mm:ss）
                string exifDateTime = captureTime.ToString("yyyy:MM:dd HH:mm:ss");

                // 各種日時タグ用のPropertyItem取得
                AddExifDateProperty(img, 0x9003, exifDateTime); // DateTimeOriginal（撮影日時）
                AddExifDateProperty(img, 0x9004, exifDateTime); // DateTimeDigitized（デジタル化日時）
                AddExifDateProperty(img, 0x0132, exifDateTime); // DateTime（ファイル更新日時）

                LogFileActivity($"ファイル名から抽出した日時をEXIFに追加: {exifDateTime}");
            }
            catch (Exception ex)
            {
                LogFileActivity($"EXIF日時情報の追加に失敗: {ex.Message}");
            }
        }

        private DateTime? GetImageDate(Image img)
        {
            try
            {
                // PropertyItemsが空の場合は早期リターン
                if (img.PropertyItems == null || img.PropertyItems.Length == 0)
                {
                    return null;
                }

                // EXIF情報から日時を取得する優先順位
                PropertyItem[] dateProperties = img.PropertyItems
                    .Where(p => p.Id == 0x9003  // DateTimeOriginal
                            || p.Id == 0x9004   // DateTimeDigitized
                            || p.Id == 0x0132)  // DateTime
                    .OrderBy(p => p.Id)         // DateTimeOriginal を優先
                    .ToArray();

                foreach (var prop in dateProperties)
                {
                    if (prop.Value == null || prop.Value.Length == 0)
                        continue;

                    string dateString = System.Text.Encoding.ASCII.GetString(prop.Value)
                                            .TrimEnd('\0', ' ');

                    if (DateTime.TryParseExact(dateString,
                        "yyyy:MM:dd HH:mm:ss",
                        CultureInfo.InvariantCulture,
                        DateTimeStyles.None,
                        out DateTime result))
                    {
                        return result;
                    }
                }

                return null; // EXIF情報から日付が取得できない場合
            }
            catch (Exception ex)
            {
                LogFileActivity($"EXIF日時取得エラー: {ex.Message}");
                return null; // EXIF情報の読み取りに失敗した場合
            }
        }

        private void AddExifWorldInfo(Image img)
        {
            // 親チェックボックスがオフの場合は実行しない
            if (!Exif_CheckBox.Checked || !exifWorldName_checkBox.Checked) return;

            try
            {
                PropertyItem? prop = GetDummyPropertyItem(img);
                if (prop == null) return;

                prop.Id = 0x9286; // UserCommentタグ
                prop.Type = 2;    // ASCII

                // EXIF規格準拠のエンコーディング
                byte[] asciiHeader = System.Text.Encoding.ASCII.GetBytes("ASCII\0\0\0");
                string comment = $"World: {currentWorld}";
                byte[] commentBytes = System.Text.Encoding.ASCII.GetBytes(comment);
                byte[] fullBytes = new byte[asciiHeader.Length + commentBytes.Length];
                asciiHeader.CopyTo(fullBytes, 0);
                commentBytes.CopyTo(fullBytes, asciiHeader.Length);

                prop.Value = fullBytes;
                prop.Len = fullBytes.Length;

                img.SetPropertyItem(prop);
                LogFileActivity($"ワールド情報をEXIFに追加: {currentWorld}");
            }
            catch (Exception ex)
            {
                LogFileActivity($"EXIF情報の追加に失敗: {ex.Message}");
                if (ex.InnerException != null)
                {
                    LogFileActivity($"内部エラー: {ex.InnerException.Message}");
                }
            }
        }

        private void AddExifDateProperty(Image img, int propId, string dateTime)
        {
            try
            {
                // PropertyItemの取得
                PropertyItem? propItem = GetDummyPropertyItem(img);
                if (propItem == null) return;

                // プロパティIDと値を設定
                propItem.Id = propId;
                propItem.Type = 2; // ASCII文字列

                // ASCII文字列にNULL終端を追加
                byte[] bytes = System.Text.Encoding.ASCII.GetBytes(dateTime + '\0');
                propItem.Value = bytes;
                propItem.Len = bytes.Length;

                // 画像にプロパティを設定
                img.SetPropertyItem(propItem);

                // 確認用のログ
                LogFileActivity($"EXIF ID 0x{propId:X4} に日時情報を設定しました");
            }
            catch (Exception ex)
            {
                LogFileActivity($"EXIF ID 0x{propId:X4} の設定に失敗: {ex.Message}");
            }
        }

        private PropertyItem? GetDummyPropertyItem(Image img)
        {
            try
            {
                // すでにPropertyItemがある場合はそれをコピーして使用
                if (img.PropertyItems != null && img.PropertyItems.Length > 0)
                {
                    return img.PropertyItems[0];
                }
                
                // PropertyItemがない場合は新しいダミー画像を作成して
                // そこからPropertyItemを取得
                using (Bitmap dummyImg = new Bitmap(1, 1))
                {
                    // ダミー画像にEXIFプロパティを追加して保存
                    string tempDummyPath = Path.Combine(Path.GetTempPath(), "dummy_exif.jpg");
                    
                    // まずJPEG形式で保存（PNGよりもEXIF情報を持ちやすい）
                    var encoderParams = new EncoderParameters(1);
                    encoderParams.Param[0] = new EncoderParameter(System.Drawing.Imaging.Encoder.Quality, 90L);
                    var codec = GetEncoderInfo("image/jpeg");
                    dummyImg.Save(tempDummyPath, codec, encoderParams);
                    
                    // 保存したダミー画像を再度読み込み
                    using (Image loadedDummy = Image.FromFile(tempDummyPath))
                    {
                        // ダミーアイテムを作成
                        var dummyProp = loadedDummy.PropertyItems.FirstOrDefault();
                        if (dummyProp != null)
                        {
                            File.Delete(tempDummyPath); // 一時ファイル削除
                            return dummyProp;
                        }
                    }
                    
                    // 一時ファイル削除
                    if (File.Exists(tempDummyPath))
                        File.Delete(tempDummyPath);
                }
                
                // それでも取得できない場合はログに記録
                LogFileActivity("PropertyItemを取得できませんでした。EXIF情報の追加をスキップします。");
                return null;
            }
            catch (Exception ex)
            {
                LogFileActivity($"PropertyItem取得エラー: {ex.Message}");
                return null;
            }
        }

        // JPEG エンコーダ情報を取得するヘルパーメソッド
        private ImageCodecInfo GetEncoderInfo(string mimeType)
        {
            ImageCodecInfo[] encoders = ImageCodecInfo.GetImageEncoders();
            for (int i = 0; i < encoders.Length; i++)
            {
                if (encoders[i].MimeType == mimeType)
                    return encoders[i];
            }
            return null;
        }

        private void Exif_label_Click(object sender, EventArgs e)
        {

        }

        private void Exif_CheckBox_CheckedChanged(object sender, EventArgs e)
        {
            Exif_groupBox.Enabled = Exif_CheckBox.Checked;
        }

        private void checkBox2_CheckedChanged(object sender, EventArgs e)
        {

        }

        private void Exif_groupBox_Enter(object sender, EventArgs e)
        {

        }

        private void UpdateStatus(string status)
        {
            if (InvokeRequired)
            {
                Invoke(new Action(() =>
                {
                    toolStripStatusLabel1.Text = status;
                }));
            }
            else
            {
                toolStripStatusLabel1.Text = status;
            }
        }
    }
}
