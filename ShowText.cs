using Raylib_cs;
using System.Numerics;
using System.Text;

namespace 基于UT文本引擎的字幕_by_无聊的Ag {
    public static class ShowText {
        private static readonly List<CharTxt> TextList = [];
        private static readonly List<CharTxt_Out> TextOutList = [];
        private static readonly StringBuilder numtiem = new(4);
        private static int Lenth;
        private static int sleep;
        private static int NextCharTick;
        private static int Text_Con;
        private static int size;
        private static int TimeOut = 0;
        private static bool Shake = false;
        private static bool ChangeColor = false;
        private static int ChangeColor_con = 300;
        private static bool SmallShake = false;
        private static bool LRShake = false;
        private static bool UpDown = false;
        private static Color color = Color.White;
        private static Font font;
        private static OutMode mod;
        private static bool Bg;
        private static Vector2 BgPoint;
        private static Color BgColor;
        private static int Width;
        private static int Height;
        private static float width;

        public static void Tick() {
            if (Lenth == 0) return;
            switch (mod) {
                case OutMode.Normal:
                    if (Text_Con < Lenth) {
                        if (NextCharTick <= 0) {
                            do {
                                Text_Con++;
                                if (Text_Con < Lenth) NextCharTick = TextList[Text_Con].sleep;
                                else NextCharTick = sleep;
                            } while (NextCharTick == -1);
                        }
                        else NextCharTick--;
                    }
                    for (int i = 0; i < Text_Con; i++) {
                        CharTxt c = TextList[i];
                        if (c.Shake) {
                            if (c.Shake_con == 0) {
                                c.Shake_con = 2;
                                c.point = c.Base_point + new Vector2(size / 2) + new Vector2(Data.rad.Next(-size / 8, size / 8), Data.rad.Next(-size / 8, size / 8));
                            }
                            else c.Shake_con--;
                        }
                        if (c.ChangeColor) c.color = Color.FromHSV((ChangeColor_con - i * 4) % 360, 1, 1);
                        if (c.UpDown) c.point.Y = c.Base_point.Y + 5 * MathF.Sin((c.UpDown_con += 4) * MathF.PI / 180) + size / 2;
                        if (c.SmallShake) {
                            if (c.SmallShake_con < 4 && c.SmallShake_con != 0) c.SmallShake_con++;
                            else if (c.SmallShake_con >= 4) {
                                c.point = c.Base_point + new Vector2(size / 2);
                                c.SmallShake_con = 0;
                            }
                        }
                        if (c.LRShake) {
                            c.point = c.Base_point + new Vector2(size / 3) + new Vector2(MathF.Sin(c.LRShake_con * MathF.PI / 180)
                                * size / 6, -MathF.Cos(c.LRShake_con * MathF.PI / 180) * size / 6);
                            c.LRShake_con += 4;
                        }
                        TextList[i] = c;
                    }
                    if (Data.rad.Next(Lenth / 4 + 1) == 0) {
                        int con = Data.rad.Next(Text_Con);
                        CharTxt c = TextList[con];
                        if (c.SmallShake) {
                            c.SmallShake_con++;
                            c.point = c.Base_point + new Vector2(size / 2) + new Vector2(Data.rad.Next(-1, 1), Data.rad.Next(-1, 1));
                            TextList[con] = c;
                        }
                    }
                    break;
                case OutMode.OutBreak:
                    if (TextOutList.Count == 0) return;
                    for (int i = 0; i < TextOutList.Count; i++) {
                        CharTxt_Out c = TextOutList[i];
                        c.point -= c.speed;
                        c.speed.Y -= 0.2f;
                        c.r += c.r_speed;
                        if (c.ChangeColor) c.color = Color.FromHSV((ChangeColor_con - i * 4) % 360, 1, 1);
                        if (c.point.Y >= Data.Height + Math.Sqrt(size * size * 2)) TextOutList.RemoveAt(i);
                        else TextOutList[i] = c;
                    }
                    break;
            }
            ChangeColor_con++;
        }
        public static void Draw() {
            if (Lenth == 0) return;
            switch (mod) {
                case OutMode.Normal:
                    if (Bg) Raylib.DrawRectangle((int)BgPoint.X, (int)BgPoint.Y, Width, Height, BgColor);
                    for (int i = 0; i < Text_Con; i++) {
                        CharTxt c = TextList[i];
                        Raylib.DrawTextPro(font, c.txt, c.point, new Vector2(size / 2), c.r, size, 0, c.color);
                    }
                    break;
                case OutMode.OutBreak:
                    if (TextOutList.Count == 0) return;
                    for (int i = 0; i < TextOutList.Count; i++) {
                        CharTxt_Out c = TextOutList[i];
                        Raylib.DrawTextPro(font, c.txt, c.point, new Vector2(size / 2), c.r, size, 0, c.color);
                    }
                    break;
            }
        }
        /// <summary>
        /// 设置文本(\)<br/>
        /// 1:震动 2:渐变色 3:微震 4:转圈 5:上下震动<br/>
        /// R:红 W:白 Y:黄 B:蓝 P:紫 C:青 G:绿 O:橙 D:黑<br/>
        /// t:休眠刻 T:修改休眠刻 x:x = 0 y:\r n:\n r:重置特效<br/>
        /// (/)
        /// B:背景 B+颜色字符可以改底色,如:/By :淡黄色背景
        /// R:红 W:白 Y:黄 B:蓝 P:紫 C:青 G:绿 O:橙 D:黑
        /// 小写为半透明
        /// </summary>
        public static void SetText(string text, Font f, Vector2 p, int s) {
            Shake = false;
            ChangeColor = false;
            SmallShake = false;
            LRShake = false;
            UpDown = false;
            Lenth = text.Length;
            sleep = -1;
            font = f;
            size = s;
            TextList.Clear();
            Text_Con = 0;
            NextCharTick = 0;
            color = Color.White;
            Vector2 point = p;
            mod = OutMode.Normal;
            Bg = false;
            Width = 0;
            width = 0;
            Height = s;
            BgPoint = p - new Vector2(6, 8);
            BgColor = new(0, 0, 0, 192);
            for (int i = 0; i < Lenth; i++) {
                string chr = new(text[i], 1);
                if (chr == "\\") {
                    switch (text[++i]) {
                        case '\\':
                            chr = "\\";
                            TextList.Add(new CharTxt() {
                                txt = chr,
                                point = new Vector2(point.X, point.Y) + new Vector2(size / 2),
                                Base_point = point,
                                color = color,
                                sleep = TimeOut == 0 ? sleep : TimeOut,
                                Shake = Shake,
                                ChangeColor = ChangeColor,
                                SmallShake = SmallShake,
                                SmallShake_con = 0,
                                LRShake = LRShake,
                                UpDown = UpDown,
                                UpDown_con = 0,
                                r = 0
                            });
                            point.X += Raylib.MeasureTextEx(font, chr, size, 0).X;
                            width += Raylib.MeasureTextEx(font, chr, size, 0).X;
                            break;
                        case 'b':
                            point.X += Raylib.MeasureTextEx(font, " ", size, 0).X;
                            width += Raylib.MeasureTextEx(font, " ", size, 0).X;
                            break;
                        case '1':
                            Shake = !Shake;
                            continue;
                        case '2':
                            ChangeColor = !ChangeColor;
                            continue;
                        case '3':
                            SmallShake = !SmallShake;
                            continue;
                        case '4':
                            LRShake = !LRShake;
                            continue;
                        case '5':
                            UpDown = !UpDown;
                            continue;
                        case 'W':
                            color = new(255, 255, 255);
                            continue;
                        case 'D':
                            color = new(0, 0, 0);
                            continue;
                        case 'R':
                            color = new(255, 0, 0);
                            continue;
                        case 'O':
                            color = new(255, 160, 0);
                            continue;
                        case 'Y':
                            color = new(255, 255, 0);
                            continue;
                        case 'B':
                            color = new(0, 0, 255);
                            continue;
                        case 'G':
                            color = new(0, 255, 0);
                            continue;
                        case 'P':
                            color = new(160, 0, 255);
                            continue;
                        case 'C':
                            color = new(0, 255, 255);
                            break;
                        case 't':
                            numtiem.Clear();
                            for (i++; i < Lenth && text[i] != '|'; i++) numtiem.Append(text[i]);
                            TimeOut = int.Parse(numtiem.ToString());
                            continue;
                        case 'T':
                            numtiem.Clear();
                            for (i++; i < Lenth && text[i] != '|'; i++) numtiem.Append(text[i]);
                            sleep = int.Parse(numtiem.ToString());
                            continue;
                        case 'x':
                            point.X = p.X;
                            continue;
                        case 'y':
                            point.Y += size;
                            Height += size;
                            Width = Width < (int)width ? (int)width : Width;
                            width = 0;
                            continue;
                        case 'n':
                            point.Y += size;
                            Height += size;
                            Width = Width < (int)width ? (int)width : Width;
                            width = 0;
                            point.X = p.X;
                            continue;
                        case 'r':
                            Shake = false;
                            ChangeColor = false;
                            SmallShake = false;
                            LRShake = false;
                            UpDown = false;
                            color = Color.White;
                            break;
                        case '|':
                            continue;
                        default:
                            i--;
                            continue;
                    }
                }
                else if (chr == "/") {
                    switch (text[++i]) {
                        case '/':
                            chr = "/";
                            TextList.Add(new CharTxt() {
                                txt = chr,
                                point = new Vector2(point.X, point.Y) + new Vector2(size / 2),
                                Base_point = point,
                                color = color,
                                sleep = TimeOut == 0 ? sleep : TimeOut,
                                Shake = Shake,
                                ChangeColor = ChangeColor,
                                SmallShake = SmallShake,
                                SmallShake_con = 0,
                                LRShake = LRShake,
                                UpDown = UpDown,
                                UpDown_con = 0,
                                r = 0
                            });
                            point.X += Raylib.MeasureTextEx(font, chr, size, 0).X;
                            width += Raylib.MeasureTextEx(font, chr, size, 0).X;
                            break;
                        case 'B':
                            switch (text[++i]) {
                                case 'd':
                                    BgColor = new(0, 0, 0, 192);
                                    break;
                                case 'D':
                                    BgColor = new(0, 0, 0);
                                    break;
                                case 'w':
                                    BgColor = new(255, 255, 255, 192);
                                    break;
                                case 'W':
                                    BgColor = new(255, 255, 255);
                                    break;
                                case 'b':
                                    BgColor = new(0, 0, 255, 192);
                                    break;
                                case 'B':
                                    BgColor = new(0, 0, 255);
                                    break;
                                case 'y':
                                    BgColor = new(255, 255, 0, 192);
                                    break;
                                case 'Y':
                                    BgColor = new(255, 255, 0);
                                    break;
                                case 'g':
                                    BgColor = new(0, 255, 0, 192);
                                    break;
                                case 'G':
                                    BgColor = new(0, 255, 0);
                                    break;
                                case 'r':
                                    BgColor = new(255, 0, 0, 192);
                                    break;
                                case 'R':
                                    BgColor = new(255, 0, 0);
                                    break;
                                case 'c':
                                    BgColor = new(0, 255, 255, 192);
                                    break;
                                case 'C':
                                    BgColor = new(0, 255, 255);
                                    break;
                                case 'o':
                                    BgColor = new(255, 160, 0, 192);
                                    break;
                                case 'O':
                                    BgColor = new(255, 160, 0);
                                    break;
                                case 'p':
                                    BgColor = new(160, 0, 255, 192);
                                    break;
                                case 'P':
                                    BgColor = new(160, 0, 255);
                                    break;
                                default:
                                    --i;
                                    break;
                            }
                            Bg = true;
                            continue;
                        default:
                            i--;
                            continue;
                    }
                    continue;
                }
                else {
                    TextList.Add(new CharTxt() {
                        txt = chr,
                        point = new Vector2(point.X, point.Y) + new Vector2(size / 2),
                        Base_point = point,
                        color = color,
                        sleep = TimeOut == 0 ? sleep : TimeOut,
                        Shake = Shake,
                        ChangeColor = ChangeColor,
                        SmallShake = SmallShake,
                        SmallShake_con = 0,
                        Shake_con = 0,
                        LRShake = LRShake,
                        UpDown = UpDown,
                        UpDown_con = 0,
                        r = 0
                    });
                    TimeOut = 0;
                    point.X += Raylib.MeasureTextEx(font, chr, size, 0).X;
                    width += Raylib.MeasureTextEx(font, chr, size, 0).X;
                }
            }
            Lenth = TextList.Count;
            if (sleep == -1) Text_Con = Lenth;
            Width = Width < (int)width ? (int)width : Width;
            Width += 16;
            Height += 16;
            sleep = 0;
        }
        public static void EndText(OutMode m = OutMode.Normal) {
            TextOutList.Clear();
            switch (m) {
                case OutMode.OutBreak:
                    mod = m;
                    for (int i = 0; i < Text_Con; i++) {
                        CharTxt c = TextList[i];
                        if (c.txt == " ") continue;
                        TextOutList.Add(new CharTxt_Out() {
                            txt = c.txt,
                            color = c.color,
                            point = c.point,
                            ChangeColor = c.ChangeColor,
                            speed = new(Data.rad.Next(-10, 10), Data.rad.Next(4)),
                            r_speed = Data.rad.Next(-20, 20),
                            r = 0
                        });
                    }
                    break;
                default:
                    Lenth = 0;
                    break;
            }
        }
        public struct CharTxt {
            public string txt;
            public Vector2 Base_point;
            public Vector2 point;
            public Color color;
            public int sleep;
            public int r;
            public bool Shake;
            public sbyte Shake_con;
            public bool ChangeColor;
            public bool SmallShake;
            public sbyte SmallShake_con;
            public bool UpDown;
            public ushort UpDown_con;
            public bool LRShake;
            public ushort LRShake_con;
        }
        public struct CharTxt_Out {
            public string txt;
            public Color color;
            public bool ChangeColor;
            public Vector2 point;
            public Vector2 speed;
            public int r_speed;
            public int r;
        }
    }
}
