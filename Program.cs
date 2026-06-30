using Raylib_cs;
using System.Numerics;
using System.Runtime.InteropServices;
using System.Text;

namespace 基于UT文本引擎的字幕_by_无聊的Ag {
    internal class Program {
        [DllImport("user32.dll")] public static extern int GetSystemMetrics(int nIndex);
        [DllImport("user32")] public static extern int GetAsyncKeyState(int vKey);
        [DllImport("user32.dll")] static extern bool GetCursorPos(out POINT lpPoint);
        [StructLayout(LayoutKind.Sequential)] public struct POINT { public int X; public int Y; }
        [DllImport("user32.dll")] static extern bool SetCursorPos(int x, int y);
        static void Main(string[] arge) {
            if (arge.Length == 1) {
                switch (arge[0].ToLower()) {
                    case "cf":
                    case "cratefile":
                    case "help":
                        Encoding.RegisterProvider(CodePagesEncodingProvider.Instance);
                        File.WriteAllText("Text.txt", $"set {GetSystemMetrics(0)} {GetSystemMetrics(1)} to \\b //这是一段示例，这一行末尾是唯一可以写注释的地方" +
                            $"，除非你知道你在做什么，否则不要修改第一行前3个参数\r\n300 400,50 \\T3|/B\\5Hello\\b\\5\\t40|我是\\2Ag大爹\\2\\Y开发\\W的\\T30|字幕菌 text 80" +
                            $"\r\n120 ob\r\n120 700,880 \\T3|\\1我可以震动\\1 text 80\r\n80 ob\r\n120 700,880 \\T3|\\2可以变色\\2 text 80\r\n80 ob\r\n" +
                            $"120 700,800 \\T3|\\3偷偷震动\\3 text 80\r\n90 ob\r\n120 700,800 \\T3|\\4转圈圈ing...\\4 text 80\r\n90 ob\r\n" +
                            $"120 700,800 \\T3|\\5上下浮动\\5 text 80\r\n90 ob\r\n120 700,800 \\T3|\\R以\\Y及\\B变\\P色\\C哦\\G。 text 80\r\n90 ob\r\n" +
                            $"120 100,100 \\T3|还可以自定义位置 text 80\r\n110 ob\r\n360 400,600 \\T2|换行\\n\\t60|任意换行?\\y\\t60|123456\\n\\T40|" +
                            $"A\\xg\\x大\\x爹\\T2|\\n重叠字符 text 60\r\n100 ob\r\n360 400,300 \\T3|\\1\\R叠\\G加\\C特\\1\\W\\3\\P效也\\3\\W\\5\\Y\"可以" +
                            $"\"\\5\\2\\4的哦\\4\\5...\\5\\n\\W\\1\\2\\3\\4\\5(部分特效间不兼容)\\r text 80\r\n120 ob\r\n400 150,200 \\T8|/B\\Ybey\\R=（\\n" +
                            $"\\W对了，对Win7有点不支持呵呵\\nF3+ESC可以提前退出\\n(我去怎么不早说) text 120\r\n120 ob");
                        File.WriteAllText("启动测试文件.bat", $"\"{Path.GetFileName(Environment.ProcessPath)}\" Text.txt", Encoding.GetEncoding("GB2312"));
                        return;
                    default:
                        Data.FilePath = arge[0];
                        break;
                }
            }
            else if (arge.Length > 1) return;
            GetCursorPos(out POINT mouse);
            Raylib.SetConfigFlags(ConfigFlags.UndecoratedWindow | ConfigFlags.TransparentWindow | ConfigFlags.TopmostWindow | ConfigFlags.MousePassthroughWindow);
            Raylib.InitWindow(0, 0, "");
            SetCursorPos(mouse.X, mouse.Y);
            Raylib.SetTargetFPS(60);
            MyFont.Init();
            Data.Error = Data.LoadFile();
            if (Data.speak != 0 && Data.ShowTiShi && !Data.Error) {
                Raylib.BeginDrawing();
                Raylib.ClearBackground(Color.Blank);
                string RunString = $"按下{Function.CtoS(Data.speak)}键开始";
                Raylib.DrawTextPro(MyFont.text, RunString, new((Data.Width - Raylib.MeasureTextEx(MyFont.text, RunString, 32, 0).X) / 2, Data.Height * 0.85f),
                    new(0), 0, 32, 0, Color.White);
                Raylib.EndDrawing();
            }
            while ((GetAsyncKeyState(27) & 0x8000) == 0 || (GetAsyncKeyState(114) & 0x8000) == 0) {
#if DEBUG
                if (Raylib.IsKeyPressed(KeyboardKey.F1)) Raylib.SetTargetFPS(10);
                if (Raylib.IsKeyPressed(KeyboardKey.F2)) Raylib.SetTargetFPS(60);
                if (Raylib.IsKeyPressed(KeyboardKey.F4)) Raylib.SetTargetFPS(300);
                if (Raylib.IsKeyPressed(KeyboardKey.F3)) Data.debug = !Data.debug;
#endif
                if (!Data.Error) {
                    if (Data.speak != 0) {
                        if ((GetAsyncKeyState(Data.speak) & 0x8000) != 0) Data.Run = true;
                        if (!Data.Run) continue;
                    }
                    if (Data.TimeOut <= 0 && Data.NextLine <= Data.txtdata.Count) {
                        if (Data.NextLine == Data.txtdata.Count) break;
                        TextData td = Data.txtdata[Data.NextLine];
                        Data.TimeOut = td.TimeOut;

                        if (string.IsNullOrEmpty(td.text)) ShowText.EndText(td.outmod);
                        else {
                            char error = ShowText.SetText(td.text, MyFont.Get(td.font), td.point);
                            if (error != 0) {
                                ShowText.SetText(@$"\T2|/B\RError:第{Data.NextLine + 1}句\\{error}转义错误!", MyFont.text,
                                    new((Data.Width - Raylib.MeasureTextEx(MyFont.text, @$"Error:第{Data.NextLine + 1}句\{error}转义错误!", 40, 0).X) / 2, Data.Height * 0.85f));
                                Data.Error = true;
                            }
                        }
                        Data.NextLine++;
                    }
                    else Data.TimeOut--;
                }
                ShowText.Tick();
                Data.Tick++;

                Raylib.BeginDrawing();
                Raylib.ClearBackground(Color.Blank);
                ShowText.Draw();
#if DEBUG
                if (Data.debug) Raylib.DrawFPS(0, 0);
#endif
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
        public static bool Error = false;
        public static int NextLine = 0;
        public static int TimeOut = 0;
        public static string Text = "";
        public static int Width = Program.GetSystemMetrics(0);
        public static int Height = Program.GetSystemMetrics(1);
        public static int FHeight = 1080;
        public static float WinSize;
        public static int Tick = 0;
        public static char speak = (char)0;
        public static bool Run = false;
        public static bool ShowTiShi = true;
        public static bool LoadFile() {
            if (!File.Exists(FilePath)) {
                ShowText.SetText(@$"/B\RFile Not Find!\n没找到\Y{FilePath}\R启动文件!", MyFont.text,
                    new((Width - Raylib.MeasureTextEx(MyFont.text, $"没找到\\Y{FilePath}\\R启动文件!", 40, 0).X) / 2, Height * 0.85f));
                return true;
            }
            using StreamReader TextFile = new(FilePath, Encoding.UTF8);
            List<(int time, string[] data)> rawLines = [];
            int lineNum = 0;

            while ((Text = TextFile.ReadLine()) != null) {
                if (lineNum == 0) if (Text[..3] != "set") return HasError(1, "开头必须是set");
                lineNum++;
                Text = Text.Trim();
                if (string.IsNullOrEmpty(Text)) continue;

                string[] parts = Text.Split(' ');
                if (parts.Length == 0) continue;

                int time = 0;
                if (parts[0] == "set") {
                    if (lineNum != 1) return HasError(lineNum, "set只能有一个且必须在第一行");
                    if (parts.Length < 4) return HasError(1, "设置参数至少要为4个");
                    if (!int.TryParse(parts[2], out FHeight)) return HasError(1, "原始窗口大小设置错误");
                    WinSize = Height / (float)FHeight;
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
                            return HasError(1, "时间模式设置错误");
                    }
                    if (parts.Length >= 5) {
                        speak = parts[4][0] == '\\' ? Function.GetKey(parts[4]) : parts[4][0];
                        speak = char.IsLower(speak) ? char.ToUpper(speak) : speak;
                        if (parts[4][^1] == '|') ShowTiShi = false;
                    }
                    else Run = true;
                    continue;
                }
                else if (!int.TryParse(parts[0], out time)) return HasError(lineNum, "时间格式错误");
                if (timemod == TimeMode.TimeLine) if (time <= 0) return HasError(lineNum, "时间线模式下Tick不能为0");
                rawLines.Add((time, parts));
            }
            if (rawLines.Count == 0) {
                ShowText.SetText(@$"/B\RError:{FilePath}文件为空!", MyFont.text,
                    new((Width - Raylib.MeasureTextEx(MyFont.text, $"Error:{FilePath}文件为空!", 40, 0).X) / 2, Height * 0.85f));
                return true;
            }
            for (int i = 0; i < rawLines.Count; i++) {
                var (time, data) = rawLines[i];
                string[] parts = data;
                int X, Y;
                int timeDiff = timemod == TimeMode.TimeLine ? i < rawLines.Count - 1 ? rawLines[i + 2].time - time : 0 : time;
                if (parts.Length == 1) txtdata.Add(new TextData { TimeOut = timeDiff, text = "" });
                else if (parts.Length == 2) {
                    OutMode m = parts[1].ToLower() switch {
                        "ob" => OutMode.OutBreak,
                        "outbreak" => OutMode.OutBreak,
                        _ => OutMode.Normal,
                    };
                    txtdata.Add(new TextData { TimeOut = timeDiff, text = "", outmod = m });
                }
                else if (parts.Length <= 5) {
                    int size = 50;
                    string font = "text";
                    if (parts.Length < 3) return HasError(i + 2, @"参数过少,\n目标参数至少为4个");
                    string[] SPoint = parts[1].Split(',');
                    if (SPoint.Length == 2) {
                        if (!int.TryParse(SPoint[0], out X)) return HasError(i + 2, "X坐标错误");
                        if (!int.TryParse(SPoint[1], out Y)) return HasError(i + 2, "Y坐标错误");
                    }else return HasError(i + 2, "坐标错误");
                    if (parts.Length >= 4) {
                        if (File.Exists($"{MyFont.Path}{parts[3]}.ttf")) font = parts[3]; else return HasError(i + 2, $"缺失字体{parts[3]}");
                        if (parts.Length == 5) if (!int.TryParse(parts[4], out size)) return HasError(i + 2, "字体大小错误");
                    }

                    txtdata.Add(new TextData {
                        TimeOut = timeDiff,
                        point = new Vector2(X, Y) * WinSize,
                        outmod = OutMode.Normal,
                        text = parts[2],
                        font = font,
                        fontsize = (int)(size * WinSize)
                    });
                }
                else return HasError(i + 2, @"格式错误\n(可能是文本中包含空格,请将空格用\\b代替)");
            }
            foreach (TextData t in txtdata) MyFont.Add(t.font, t.text, t.fontsize);
            MyFont.Set();
            NextLine = 0;
            TimeOut = 0;
            return false;
        }
        private static bool HasError(int line,string info) {
            string text = @$"\T2|/B\R\3文件加载出错:第{line}行{info}";
            if (info.Contains("\\n")) {
                float width = 0, w = 0;
                for (int i = 10; i < text.Length; i++) {
                    if ((text.Length - i) > 1) switch (text.Substring(i, 2)) {
                            case "\\n":
                                width = width < w ? w : width;
                                w = 0;
                                i++;
                                continue;
                            case "\\b":
                                w += Raylib.MeasureTextEx(MyFont.text, " ", 40, 0).X;
                                continue;
                        }
                    w += Raylib.MeasureTextEx(MyFont.text, text[i].ToString(), 40, 0).X;
                }
                ShowText.SetText(text, MyFont.text, new((Width - (width < w ? w : width)) / 2, Height * 0.85f));
            }
            else ShowText.SetText(text, MyFont.text, new((Width - Raylib.MeasureTextEx(MyFont.text, text, 40, 0).X) / 2, Height * 0.85f));
            return true;
        }
    }
    public static class MyFont {
        public static List<SFont> Fonts = [];
        public static int[] ASCII = new int[95];
        public const string txt = "没找左右到启动文件加载出错误第缺失行按键头开必须在至少一格模式只句转义有且需要个段字线下不据体大小延迟坐原始窗口设置标时间可能是本中包含空请将空用代替件为空参数过少目至";
        public const string Path = "Font\\";
        public static Font text;
        public static void Init() {
            for (char i = (char)0; i < ASCII.Length; i++) ASCII[i] = i + 32;
            text = LoadChinaFont("text", txt, 40);
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
    public static class Function {
        public static char GetKey(string s) {
            if (s.Length == 1) return (char)220;
            else {
                return s[1] switch {
                    'E' => (char)27,
                    'b' => (char)32,
                    'W' => (char)91,
                    'w' => (char)92,
                    '!' => (char)112,
                    '@' => (char)113,
                    '#' => (char)114,
                    '$' => (char)115,
                    '%' => (char)116,
                    '^' => (char)117,
                    '&' => (char)118,
                    '*' => (char)119,
                    '(' => (char)120,
                    ')' => (char)121,
                    '_' => (char)122,
                    '+' => (char)123,
                    'S' => (char)160,
                    's' => (char)161,
                    'C' => (char)162,
                    'c' => (char)163,
                    'A' => (char)164,
                    'a' => (char)165,
                    _ => s[1],
                };
            }
        }
        public static string CtoS(char c) {
            return (int)c switch {
                32 => "空格",
                220 => "\\",
                112 => "F1",
                113 => "F2",
                114 => "F3",
                115 => "F4",
                116 => "F5",
                117 => "F6",
                118 => "F7",
                119 => "F8",
                120 => "F9",
                121 => "F10",
                122 => "F11",
                123 => "F12",
                27 => "ESC?",
                160 => "左Shift",
                161 => "右Shift",
                162 => "左Ctrl",
                163 => "右Ctrl",
                164 => "左Alt",
                165 => "右Alt",
                91 => "左Win",
                92 => "右Win",
                _ => c.ToString()
            };
        }

    }
    public struct TextData {
        public int TimeOut;
        public Vector2 point;
        public OutMode outmod;
        public string text;
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
