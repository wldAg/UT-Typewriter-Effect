using Raylib_cs;
using System.Data;
using System.Numerics;
using System.Runtime.InteropServices;
using System.Text;

namespace 基于UT文本引擎的字幕_by_无聊的Ag {
    internal class Program {
        [DllImport("user32")] public static extern int GetAsyncKeyState(int vKey);
        [DllImport("user32.dll")] static extern bool GetCursorPos(out POINT lpPoint);
        [StructLayout(LayoutKind.Sequential)] public struct POINT { public int X; public int Y; }
        [DllImport("user32.dll")] static extern bool SetCursorPos(int x, int y);
        static void Main(string[] arge) {
            if (arge.Length == 1) Data.FilePath = arge[0] + ".txt";
            else if (arge.Length > 1) return;
            GetCursorPos(out POINT mouse);
            Raylib.SetConfigFlags(ConfigFlags.UndecoratedWindow | ConfigFlags.TransparentWindow | ConfigFlags.TopmostWindow | ConfigFlags.MousePassthroughWindow);
            Raylib.InitWindow(0, 0, "");
            SetCursorPos(mouse.X, mouse.Y);
            Data.Width = Raylib.GetMonitorWidth(Raylib.GetCurrentMonitor());
            Data.Height = Raylib.GetMonitorHeight(Raylib.GetCurrentMonitor());
            Raylib.SetTargetFPS(60);
            TextBook.Init();
            Data.LoadFile();
            Data.WinSize = Data.FHeight / (float)Data.Height;
            while ((GetAsyncKeyState(27) & 0x8000) == 0 || (GetAsyncKeyState(114) & 0x8000) == 0) {
                if (Data.HasFile) {
                    if (Data.TimeOut <= 0 && Data.NextLine < Data.txtdata.Count) {
                        TextData td = Data.txtdata[Data.NextLine];
                        Data.TimeOut = td.TimeOut;

                        if (string.IsNullOrEmpty(td.text)) {
                            ShowText.EndText();
                        }
                        else {
                            ShowText.SetText(td.text, MyFont.LoadChinaFont(td.font, td.text),
                                td.point, td.sleep, td.fontsize);
                        }
                        Data.NextLine++;
                    }
                    else if (Data.NextLine == Data.txtdata.Count) break;
                    else Data.TimeOut--;
                }
                ShowText.Tick();
                Data.Tick++;

                Raylib.BeginDrawing();
                Raylib.ClearBackground(Color.Blank);
                ShowText.Draw();
                Raylib.EndDrawing();
            }
            Raylib.CloseWindow();
        }
    }
    public static class Data {
        public static List<TextData> txtdata = [];
        public static string FilePath = "Run.txt";
        public static bool HasFile = false;
        public static int NextLine = 0;
        public static int TimeOut = 0;
        public static string Text = "";
        public static int Width;
        public static int Height;
        public static int FHeight = 1080;
        public static float WinSize;
        public static int Tick = 0;
        public static void LoadFile() {
            if (!File.Exists(FilePath)) {
                ShowText.SetText(@$"\RFile Not Find!\n没找到\Y{FilePath}\R启动文件!", MyFont.text, new(Width * 0.38f, Height * 0.85f), 2, 40, miaobian: false);
                return;
            }

            HasFile = true;
            txtdata.Clear();

            using StreamReader TextFile = new(FilePath, Encoding.UTF8);
            List<(int time, string[] data)> rawLines = [];
            int lineNum = 0;

            while ((Text = TextFile.ReadLine()) != null) {
                lineNum++;
                Text = Text.Trim();
                if (string.IsNullOrEmpty(Text)) continue;

                string[] parts = Text.Split(' ');
                if (parts.Length == 0) continue;

                int time = 0;
                if (parts[0] == "size") {
                    if (!int.TryParse(parts[2], out FHeight)) {
                        ShowText.SetText(@$"\R\3文件加载出错:第{lineNum}行原始窗口大小设置错误", MyFont.text, new(Width * 0.38f, Height * 0.85f), 2, 40, miaobian: false);
                        HasFile = false;
                        return;
                    }
                    continue;
                }
                else if (!int.TryParse(parts[0], out time)) {
                    ShowText.SetText(@$"\R\3文件加载出错:第{lineNum}行时间格式错误", MyFont.text, new(Width * 0.38f, Height * 0.85f), 2, 40, miaobian: false);
                    HasFile = false;
                    return;
                }

                rawLines.Add((time, parts));
            }

            if (rawLines.Count == 0) {
                ShowText.SetText(@$"\RError:{FilePath}文件为空!", MyFont.text, new(Width * 0.38f, Height * 0.85f), 2, 40, miaobian: false);
                HasFile = false;
                return;
            }
            for (int i = 0; i < rawLines.Count; i++) {
                var (time, data) = rawLines[i];
                string[] parts = data;
                int timeDiff = i < rawLines.Count - 1 ? rawLines[i + 1].time - time : 0;
                if (parts.Length == 1) txtdata.Add(new TextData { TimeOut = timeDiff, text = "" });
                else if (parts.Length <= 7) {
                    int X = 0, Y = 0, timeout = 3, size = 32;
                    string font = "text";
                    PointMode PMode = PointMode.None;
                    if (parts.Length < 3) {
                        ShowText.SetText(@$"\R\3文件加载出错:第{i + 1}行参数过少,\n目标参数至少为4个", MyFont.text, new(Width * 0.38f, Height * 0.85f), 2, 40, miaobian: false);
                        HasFile = false;
                        return;
                    }

                    string[] SPoint = parts[1].Split(',');
                    switch (SPoint[0].ToLower()) {
                        case "ac":
                        case "alwayscenter":
                            PMode = PointMode.AlwaysCenter;
                            break;
                        default:
                            if (!int.TryParse(SPoint[0], out X)) {
                                ShowText.SetText(@$"\R\3文件加载出错:第{i + 1}行X坐标错误", MyFont.text, new(Width * 0.38f, Height * 0.85f), 2, 40, miaobian: false);
                                HasFile = false;
                                return;
                            }
                            break;
                    }
                    switch (SPoint[1].ToLower()) {
                        case "ac":
                        case "alwayscenter":
                            PMode = PointMode.AlwaysCenter;
                            break;
                        default:
                            if (!int.TryParse(SPoint[1], out Y)) {
                                ShowText.SetText(@$"\R\3文件加载出错:第{i + 1}行Y坐标错误", MyFont.text, new(Width * 0.38f, Height * 0.85f), 2, 40, miaobian: false);
                                HasFile = false;
                                return;
                            }
                            break;
                    }

                    if (parts.Length >= 4) {
                        if (!int.TryParse(parts[3], out timeout)) {
                            ShowText.SetText(@$"\R\3文件加载出错:第{i + 1}行延迟错误", MyFont.text, new(Width * 0.38f, Height * 0.85f), 2, 40, miaobian: false);
                            HasFile = false;
                            return;
                        }
                        if (parts.Length >= 5) {
                            font = parts[4];
                            if (parts.Length == 6) if (!int.TryParse(parts[5], out size)) {
                                ShowText.SetText(@$"\R\3文件加载出错:第{i + 1}行字体大小错误", MyFont.text, new(Width * 0.38f, Height * 0.85f), 2, 40, miaobian: false);
                                HasFile = false;
                                return;
                            }
                        }
                    }

                    txtdata.Add(new TextData {
                        TimeOut = timeDiff,
                        point = new Vector2(X, Y),
                        pMode = PMode,
                        text = parts[2],
                        sleep = timeout,
                        font = font,
                        fontsize = size
                    });
                }
                else {
                    ShowText.SetText(@$"\R\3文件加载出错:第{i + 1}行格式错误\n(可能是文本中包含空格,请将空格用\b代替)",
                        MyFont.text, new(Width * 0.38f, Height * 0.85f), 2, 40, miaobian: false);
                    HasFile = false;
                    return;
                }
            }
            NextLine = 0;
            TimeOut = 0;
        }
    }
    public static class TextBook {
        public static string ASCII = "";
        public const string text = "没找到启动文件加载出错误第行格式需要个段字体大小延迟坐原始窗口设置标时间可能是本中包含空请将空用代替件为空参数过少目至";
        public static void Init() {
            StringBuilder s = new();
            for (char i = (char)0; i < 255; i++) {
                s.Append(i);
            }
            ASCII = s.ToString();
        }
    }
    public static class MyFont {
        public const string Path = "Font\\";
        public static Font text = LoadChinaFont("text", TextBook.text);
        public static Font LoadChinaFont(string font, string textfile, int size = 32) {
            int[] codepoints = [.. (TextBook.ASCII + textfile).Distinct().Select(c => (int)c)];
            return Raylib.LoadFontEx($"{Path}{font}.ttf", size, codepoints, codepoints.Length); ;
        }
    }
    public struct TextData {
        public int TimeOut;
        public Vector2 point;
        public PointMode pMode;
        public string text;
        public int sleep;
        public string font;
        public int fontsize;
    }
    public enum PointMode {
        None,
        AlwaysCenter
    }
}
