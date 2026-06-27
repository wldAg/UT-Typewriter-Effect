using Raylib_cs;
using System.Text;
using System.Numerics;
using System.Runtime.InteropServices;

namespace 基于UT文本引擎的字幕_by_无聊的Ag {
    internal class Program {
        [DllImport("user32")] public static extern int GetAsyncKeyState(int vKey);
        [DllImport("user32.dll")] static extern bool GetCursorPos(out POINT lpPoint);
        [StructLayout(LayoutKind.Sequential)] public struct POINT { public int X; public int Y; }
        [DllImport("user32.dll")] static extern bool SetCursorPos(int x, int y);
        static void Main(string[] arge) {
            if (arge.Length == 1) Data.FilePath = arge[0];
            else if (arge.Length > 1) return;
            GetCursorPos(out POINT mouse);
            Raylib.SetConfigFlags(ConfigFlags.UndecoratedWindow | ConfigFlags.TransparentWindow | ConfigFlags.TopmostWindow | ConfigFlags.MousePassthroughWindow);
            Raylib.InitWindow(0, 0, "");
            SetCursorPos(mouse.X, mouse.Y);
            Data.Width = Raylib.GetMonitorWidth(Raylib.GetCurrentMonitor());
            Data.Height = Raylib.GetMonitorHeight(Raylib.GetCurrentMonitor());
            Raylib.SetTargetFPS(60);
            MyFont.Init();
            Data.LoadFile();
            Data.WinSize = Data.FHeight / (float)Data.Height;
            while ((GetAsyncKeyState(27) & 0x8000) == 0 || (GetAsyncKeyState(114) & 0x8000) == 0) {
                if (Raylib.IsKeyPressed(KeyboardKey.F3)) Data.debug = !Data.debug;
                if (Data.HasFile) {
                    if (Data.TimeOut <= 0 && Data.NextLine <= Data.txtdata.Count) {
                        if (Data.NextLine == Data.txtdata.Count) break;
                        TextData td = Data.txtdata[Data.NextLine];
                        Data.TimeOut = td.TimeOut;

                        if (string.IsNullOrEmpty(td.text)) ShowText.EndText(td.outmod);
                        else ShowText.SetText(td.text, MyFont.Get(td.font), td.point, td.sleep, td.fontsize);
                        Data.NextLine++;
                    }
                    else Data.TimeOut--;
                }
                ShowText.Tick();
                Data.Tick++;

                Raylib.BeginDrawing();
                Raylib.ClearBackground(Color.Blank);
                ShowText.Draw();
                if (Data.debug) Raylib.DrawFPS(0, 0);
                Raylib.EndDrawing();
            }
            Raylib.CloseWindow();
        }
    }
    public static class Data {
        public static List<TextData> txtdata = [];
        public static OutMode outmod;
        public static TimeMode timemod;
        public static Random rad = new();
        public static bool debug = false;
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
                if (parts[0] == "set") {
                    if (lineNum != 1) {
                        ShowText.SetText(@$"\R\3文件加载出错:第{lineNum}行设置必须在第一行", MyFont.text, new(Width * 0.38f, Height * 0.85f), 2, 40, miaobian: false);
                        HasFile = false;
                        return;
                    }
                    if (parts.Length < 4) {
                        ShowText.SetText(@$"\R\3文件加载出错:第1行设置参数要为4个", MyFont.text, new(Width * 0.38f, Height * 0.85f), 2, 40, miaobian: false);
                        HasFile = false;
                        return;
                    }
                    if (!int.TryParse(parts[2], out FHeight)) {
                        ShowText.SetText(@$"\R\3文件加载出错:第1行原始窗口大小设置错误", MyFont.text, new(Width * 0.38f, Height * 0.85f), 2, 40, miaobian: false);
                        HasFile = false;
                        return;
                    }
                    switch (parts[3].ToLower()) {
                        case "to":
                        case "timeout":
                            timemod = TimeMode.TimeOut;
                            break;
                        case "tl":
                        case "timeline":
                            timemod = TimeMode.TimeLine;
                            break;
                        default:
                            ShowText.SetText(@$"\R\3文件加载出错:第1行时间模式设置错误", MyFont.text, new(Width * 0.38f, Height * 0.85f), 2, 40, miaobian: false);
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
                if (timemod == TimeMode.TimeLine) if (time <= 0) {
                    ShowText.SetText(@$"\R\3时间数据出错:第{lineNum}行时间线模式下Tick不能为0", MyFont.text, new(Width * 0.38f, Height * 0.85f), 2, 40, miaobian: false);
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
                int timeDiff = timemod == TimeMode.TimeLine ? i < rawLines.Count - 1 ? rawLines[i + 1].time - time : 0 : time;
                if (parts.Length == 1) txtdata.Add(new TextData { TimeOut = timeDiff, text = "" });
                else if (parts.Length == 2) {
                    OutMode m = parts[1].ToLower() switch {
                        "ob" => OutMode.OutBreak,
                        "outbreak" => OutMode.OutBreak,
                        _ => OutMode.Normal,
                    };
                    txtdata.Add(new TextData { TimeOut = timeDiff, text = "", outmod = m });
                }
                else if (parts.Length <= 7) {
                    int timeout = 3, size = 32;
                    string font = "text";
                    if (parts.Length < 3) {
                        ShowText.SetText(@$"\R\3文件加载出错:第{i + 1}行参数过少,\n目标参数至少为4个", MyFont.text, new(Width * 0.38f, Height * 0.85f), 2, 40, miaobian: false);
                        HasFile = false;
                        return;
                    }

                    string[] SPoint = parts[1].Split(',');
                    if (!int.TryParse(SPoint[0], out int X)) {
                        ShowText.SetText(@$"\R\3文件加载出错:第{i + 1}行X坐标错误", MyFont.text, new(Width * 0.38f, Height * 0.85f), 2, 40, miaobian: false);
                        HasFile = false;
                        return;
                    }
                    if (!int.TryParse(SPoint[1], out int Y)) {
                        ShowText.SetText(@$"\R\3文件加载出错:第{i + 1}行Y坐标错误", MyFont.text, new(Width * 0.38f, Height * 0.85f), 2, 40, miaobian: false);
                        HasFile = false;
                        return;
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
                        outmod = OutMode.Normal,
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
            foreach (TextData t in txtdata) MyFont.Add(t.font, t.text, t.fontsize);
            MyFont.Set();
            NextLine = 0;
            TimeOut = 0;
        }
    }
    public static class MyFont {
        public static List<SFont> Fonts = [];
        public static int[] ASCII = new int[95];
        public const string txt = "没找到启动文件加载出错误第行必须在一格模式需要个段字线下不据体大小延迟坐原始窗口设置标时间可能是本中包含空请将空用代替件为空参数过少目至";
        public const string Path = "Font\\";
        public static Font text = LoadChinaFont("text", txt);
        public static void Init() {
            for (char i = (char)0; i < 95; i++) {
                ASCII[i] = i + 32;
            }
        }
        public static Font LoadChinaFont(string font, string textfile, int size = 32) {
            int[] chr_zn = [..textfile.Distinct().Select(c => (int)c)];
            int[] chr = new int[95 + chr_zn.Length];
            Array.Copy(ASCII, 0, chr, 0, 95);
            Array.Copy(chr_zn, 0, chr, 95, chr_zn.Length);
            return Raylib.LoadFontEx($"{Path}{font}.ttf", size, chr, chr.Length); ;
        }
        public static void Add(string f, string t, int s) {
            for (int i = 0; i < Fonts.Count; i++) {
                SFont sf = Fonts[i];
                if (sf.name == f) {
                    sf.txt.Append(t);
                    sf.size = sf.size < s ? s : sf.size;
                    Fonts[i] = sf;
                    return;
                }
            }
            Fonts.Add(new SFont() {
                name = f,
                txt = new(t),
                size = s
            });
        }
        public static void Set() {
            for (int i=0;i<Fonts.Count;i++) {
                SFont f = Fonts[i];
                f.font = LoadChinaFont(f.name, f.txt.ToString(), f.size);
                f.txt.Clear();
                Fonts[i] = f;
            }
        }
        public static Font Get(string n) {
            foreach (SFont f in Fonts) {
                if (f.name == n) return f.font;
            }
            return text;
        }
    }
    public struct TextData {
        public int TimeOut;
        public Vector2 point;
        public OutMode outmod;
        public string text;
        public int sleep;
        public string font;
        public int fontsize;
    }
    public struct SFont {
        public string name;
        public StringBuilder txt;
        public int size;
        public Font font;
    }
    public enum TimeMode {
        TimeLine,
        TimeOut
    }
    public enum OutMode {
        Normal,
        OutBreak
    }
}
