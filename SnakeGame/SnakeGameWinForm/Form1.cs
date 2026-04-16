using System;
using System.Collections.Generic;
using System.Drawing;
using System.IO;
using System.Text.Json;
using System.Windows.Forms;
using static System.Windows.Forms.VisualStyles.VisualStyleElement;
using Timer = System.Windows.Forms.Timer;

namespace SnakeGame
{
    public partial class Form1 : Form
    {
        List<Point> snake = new List<Point>();
        List<Point> foods = new List<Point>();

        int width = 30, height = 20;
        int cellSize;
        int offsetX, offsetY;

        int dx = 1, dy = 0;
        int score = 0;
       
        float headX;
        float headY;

        int moveDelay = 70;
        int lastMoveTime = 0;

        bool directionChanged = false;
        bool gameStarted = false;
        bool gameOver = false;

        Rectangle retryBtn;
        Rectangle menuBtn;


        int foodCount = 1;

        // ✅ YENİ
        string speedMode = "Medium";

        Timer timer = new Timer();
        Random rand = new Random();

        Dictionary<string, int> scores = new Dictionary<string, int>();
        string playerName = "Player";

        List<Color> colors = new List<Color>();
        Color selectedColor = Color.Lime;
        bool colorPanel = false;

        Rectangle startBtn, mapBtn, foodBtn, colorBtn, nameBtn, speedBtn;

        public Form1()
        {
            InitializeComponent();

            DoubleBuffered = true;
            WindowState = FormWindowState.Maximized;

            Resize += ResizeGame;
            MouseDown += MouseClick;
            KeyDown += KeyPress;

            timer.Interval = 16;
            timer.Tick += GameLoop;

            GenerateColors();
            LoadScores();
        }

        void SetSpeed()
        {
                if (speedMode == "Slow") moveDelay = 110;
                else if (speedMode == "Medium") moveDelay = 90;
                else if (speedMode == "Fast") moveDelay = 65;
            
        }

        void GenerateColors()
        {
            for (int r = 0; r <= 255; r += 60)
                for (int g = 0; g <= 255; g += 60)
                    for (int b = 0; b <= 255; b += 60)
                        colors.Add(Color.FromArgb(r, g, b));
        }

        void ResizeGame(object sender, EventArgs e)
        {
            int w = ClientSize.Width / width;
            int h = (ClientSize.Height - 100) / height;

            cellSize = Math.Min(w, h);
            cellSize = (int)(cellSize * 1.10f);

            offsetX = (ClientSize.Width - width * cellSize) / 2;
            offsetY = (ClientSize.Height - height * cellSize) / 2;

            int centerX = ClientSize.Width / 2 - 100;

            startBtn = new Rectangle(centerX, 200, 200, 40);
            nameBtn = new Rectangle(centerX, 250, 200, 40);
            foodBtn = new Rectangle(centerX, 300, 200, 40);
            colorBtn = new Rectangle(centerX, 350, 200, 40);
            speedBtn = new Rectangle(centerX, 400, 200, 40);
        }

        void SetMap()
        {
            width = 30;
            height = 20;
        }

        void StartGame()
        {
            if (string.IsNullOrWhiteSpace(playerName))
                playerName = "Player";
            gameOver = false; // ✅ ÇOK ÖNEMLİ
            SetMap();
            SetSpeed(); // ✅ hız buradan ayarlanır

            snake.Clear();
            foods.Clear();

            snake.Add(new Point(width / 2, height / 2));
            headX = width / 2f;
            headY = height / 2f;

            score = 0;
            dx = 1;
            dy = 0;

            lastMoveTime = 0;

            GenerateFoods();

            gameStarted = true;
            timer.Start();
        }

        void GenerateFoods()
        {
            foods.Clear();

            for (int i = 0; i < foodCount; i++)
            {
                Point f;
                do
                {
                    f = new Point(rand.Next(width), rand.Next(height));
                }
                while (snake.Contains(f) || foods.Contains(f));

                foods.Add(f);
            }
        }

        void GameLoop(object sender, EventArgs e)
        {


            if (gameStarted)
            {
                if (Environment.TickCount - lastMoveTime > moveDelay)
                {
                    UpdateGame();
                    lastMoveTime = Environment.TickCount;
                }
            }

            Invalidate();
        }

        void UpdateGame()
        {
            directionChanged = false;

            Point newHead = new Point(snake[0].X + dx, snake[0].Y + dy);

            if (newHead.X < 0 || newHead.X >= width ||
                newHead.Y < 0 || newHead.Y >= height ||
                snake.Contains(newHead))
            {
                GameOver();
                return;
            }

            snake.Insert(0, newHead);

            for (int i = 0; i < foods.Count; i++)
            {
                if (newHead == foods[i])
                {
                    score += 10;

                    Point f;
                    do
                    {
                        f = new Point(rand.Next(width), rand.Next(height));
                    }
                    while (snake.Contains(f) || foods.Contains(f));

                    foods[i] = f;
                    return;
                }
            }

            snake.RemoveAt(snake.Count - 1);
        }

        void GameOver()
        {
            timer.Stop();
            gameStarted = false;
            gameOver = true; // ✅ EKLENDİ

            if (!scores.ContainsKey(playerName) || score > scores[playerName])
                scores[playerName] = score;

            File.WriteAllText("scores.txt", JsonSerializer.Serialize(scores));

            // buton pozisyonları
            int centerX = ClientSize.Width / 2 - 100;

            retryBtn = new Rectangle(centerX, 250, 200, 40);
            menuBtn = new Rectangle(centerX, 300, 200, 40);
        }

        void LoadScores()
        {
            if (File.Exists("scores.txt"))
                scores = JsonSerializer.Deserialize<Dictionary<string, int>>(File.ReadAllText("scores.txt"));
        }

        string GetLeader()
        {
            int max = -1;
            string name = "None";

            foreach (var s in scores)
            {
                if (s.Value > max)
                {
                    max = s.Value;
                    name = s.Key;
                }
            }

            return name + " : " + max;
        }

        protected override void OnPaint(PaintEventArgs e)
        {
            Graphics g = e.Graphics;
            using (var brush = new System.Drawing.Drawing2D.LinearGradientBrush(
     ClientRectangle,
     Color.FromArgb(15, 15, 15),
     Color.FromArgb(40, 40, 40),
     90f))
            {
                g.FillRectangle(brush, ClientRectangle);
            }

            g.SmoothingMode = System.Drawing.Drawing2D.SmoothingMode.AntiAlias;
            for (int x = 0; x < width; x++)
            {
                for (int y = 0; y < height; y++)
                {
                    bool dark = (x + y) % 2 == 0;

                    Brush brush = dark
                        ? new SolidBrush(Color.FromArgb(35, 35, 35))
                        : new SolidBrush(Color.FromArgb(55, 55, 55)); ;

                    g.FillRectangle(brush,
                        offsetX + x * cellSize,
                        offsetY + y * cellSize,
                        cellSize,
                        cellSize);
                }
            }


            // ❗ ÖNCE MENU
            // ❗ 1. önce game over kontrolü
            if (gameOver)
            {
                DrawGameOver(g);
                return;
            }

            // ❗ 2. sonra menu
            if (!gameStarted)
            {
                DrawMenu(g);
                return;
            }

            g.DrawRectangle(Pens.White, offsetX, offsetY, width * cellSize, height * cellSize);
           

            // 👤 PLAYER + SCORE
            string playerText = playerName + " | Score: " + score;

            g.DrawString(playerText,
                new Font("Arial", 14, FontStyle.Bold),
                Brushes.White,
                10,
                10);

            using (Brush b = new SolidBrush(selectedColor))

            {
                // 👑 LEADER (oyun sırasında)
                string leader = "👑 " + GetLeader();
                Font leaderFont = new Font("Segoe UI Emoji", 14, FontStyle.Bold);

                SizeF ls = g.MeasureString(leader, leaderFont);

                g.DrawString(leader,
                    leaderFont,
                    Brushes.Gold,
                    ClientSize.Width - ls.Width - 10,
                    10);

                // 🐍 SNAKE ÇİZİMİ
                for (int i = 0; i < snake.Count; i++)
                {
                    var s = snake[i];

                    float x = offsetX + s.X * cellSize;
                    float y = offsetY + s.Y * cellSize;

                    // 🔥 KUYRUK İNCELME

                    float scale = 1f - (i / (float)snake.Count) * 0.4f;
                    if (scale < 0.6f) scale = 0.6f;

                    // 🧠 KAFA BÜYÜK
                    if (i == 0) scale = 1.2f;

                    float size = cellSize * scale;

                    float drawX = x + (cellSize - size) / 2;
                    float drawY = y + (cellSize - size) / 2;

                    // 🎨 gradient
                    Color c1 = selectedColor;
                    Color c2 = Color.FromArgb(
                        Math.Max(c1.R - 40, 0),
                        Math.Max(c1.G - 40, 0),
                        Math.Max(c1.B - 40, 0));

                    using (var brush = new System.Drawing.Drawing2D.LinearGradientBrush(
                        new RectangleF(drawX, drawY, size, size),
                        c1,
                        c2,
                        45f))
                    {
                        // 🧠 kafa
                        if (i == 0)
                        {
                            g.FillEllipse(brush, drawX, drawY, size, size);
                        }

                        // 🐍 KUYRUK (DÜZELTİLDİ)
                        else if (i == snake.Count - 1)
                        {
                            var tail = snake[snake.Count - 1];
                            var beforeTail = snake[snake.Count - 2];

                            int tdx = tail.X - beforeTail.X;
                            int tdy = tail.Y - beforeTail.Y;

                            // 🧱 YARIM KARE
                            Rectangle rect;

                            if (tdx == 1) // sağ
                                rect = new Rectangle((int)drawX, (int)drawY, (int)(size / 2), (int)size);
                            else if (tdx == -1) // sol
                                rect = new Rectangle((int)(drawX + size / 2), (int)drawY, (int)(size / 2), (int)size);
                            else if (tdy == 1) // aşağı
                                rect = new Rectangle((int)drawX, (int)drawY, (int)size, (int)(size / 2));
                            else // yukarı
                                rect = new Rectangle((int)drawX, (int)(drawY + size / 2), (int)size, (int)(size / 2));

                            g.FillRectangle(brush, rect);

                            // 🔵 YARIM YUVARLAK
                            float startAngle;

                            if (tdx == 1) startAngle = 270;
                            else if (tdx == -1) startAngle = 90;
                            else if (tdy == 1) startAngle = 0;
                            else startAngle = 180;

                            g.FillPie(brush, drawX, drawY, size, size, startAngle, 180);
                        }

                        // 🐍 GÖVDE (KARE YAPTIK)
                        else
                        {
                            g.FillRectangle(brush, drawX, drawY, size, size);
                        }
                    }

                    // ✨ parlaklık (AYNI KALDI)
                    using (var shine = new SolidBrush(Color.FromArgb(80, Color.White)))
                    {
                        if (i == 0)
                        {
                            g.FillEllipse(shine,
                                drawX + size * 0.2f,
                                drawY + size * 0.2f,
                                size * 0.4f,
                                size * 0.4f);
                        }
                        else
                        {
                            g.FillRectangle(shine,
                                drawX + size * 0.2f,
                                drawY + size * 0.2f,
                                size * 0.4f,
                                size * 0.4f);
                        }
                    }

                    // 👁️ SADECE KAFA
                    if (i == 0)
                    {
                        float eyeW = size / 4;
                        float eyeH = size / 6;

                        float ex1 = drawX + size * 0.2f;
                        float ey1 = drawY + size * 0.2f;

                        float ex2 = drawX + size * 0.6f;
                        float ey2 = drawY + size * 0.2f;

                        g.FillEllipse(Brushes.White, ex1, ey1, eyeW, eyeH);
                        g.FillEllipse(Brushes.White, ex2, ey2, eyeW, eyeH);

                        g.FillEllipse(Brushes.Black, ex1 + 2, ey1 + 2, eyeW / 2, eyeH / 2);
                        g.FillEllipse(Brushes.Black, ex2 + 2, ey2 + 2, eyeW / 2, eyeH / 2);

                        // 👅 DİL
                        using (Pen tongue = new Pen(Color.Red, 1.5f))
                        {
                            float tx = drawX + size / 2f;
                            float ty = drawY + size / 2f;

                            float len = size * 0.3f;
                            float spread = size * 0.08f;

                            if (dx == 1)
                            {
                                g.DrawLine(tongue, tx + 6, ty, tx + 6 + len, ty - spread);
                                g.DrawLine(tongue, tx + 6, ty, tx + 6 + len, ty + spread);
                            }
                            else if (dx == -1)
                            {
                                g.DrawLine(tongue, tx - 6, ty, tx - 6 - len, ty - spread);
                                g.DrawLine(tongue, tx - 6, ty, tx - 6 - len, ty + spread);
                            }
                            else if (dy == 1)
                            {
                                g.DrawLine(tongue, tx, ty + 6, tx - spread, ty + 6 + len);
                                g.DrawLine(tongue, tx, ty + 6, tx + spread, ty + 6 + len);
                            }
                            else if (dy == -1)
                            {
                                g.DrawLine(tongue, tx, ty - 6, tx - spread, ty - 6 - len);
                                g.DrawLine(tongue, tx, ty - 6, tx + spread, ty - 6 - len);
                            }
                        }
                    }
                }
            }

            foreach (var f in foods)
            {
                float x = offsetX + f.X * cellSize;
                float y = offsetY + f.Y * cellSize;

                // 🍎 elmanın gövdesi
                g.FillEllipse(Brushes.Red, x, y, cellSize, cellSize);

                // ✨ parlaklık efekti
                g.FillEllipse(new SolidBrush(Color.FromArgb(120, Color.White)),
                    x + cellSize * 0.2f,
                    y + cellSize * 0.2f,
                    cellSize * 0.3f,
                    cellSize * 0.3f);

                // 🌿 sap
                g.DrawLine(new Pen(Color.Brown, 2),
                    x + cellSize / 2,
                    y,
                    x + cellSize / 2,
                    y - cellSize / 4);

                // 🍃 yaprak
                g.FillEllipse(Brushes.Green,
                    x + cellSize * 0.5f,
                    y - cellSize * 0.2f,
                    cellSize * 0.3f,
                    cellSize * 0.2f);

            }

            



        }



        private void DrawGameOver(Graphics g)
        {
            // 🟥 GAME OVER
            Font f = new Font("Arial", 36, FontStyle.Bold);

            string text = "GAME OVER";
            SizeF ts = g.MeasureString(text, f);

            g.DrawString(text, f, Brushes.Red,
                (ClientSize.Width - ts.Width) / 2, 150);

            // 🎯 SCORE
            string scoreText = playerName + " | Score: " + score;
            Font sf = new Font("Arial", 20);

            SizeF ss = g.MeasureString(scoreText, sf);

            g.DrawString(scoreText, sf, Brushes.White,
                (ClientSize.Width - ss.Width) / 2, 220);

            // 🔁 RETRY (YAZI BUTONU)
            string retryText = "RETRY";
            Font btnFont = new Font("Arial", 18, FontStyle.Bold);

            SizeF rSize = g.MeasureString(retryText, btnFont);

            float rx = (ClientSize.Width - rSize.Width) / 2;
            float ry = 300;

            g.DrawString(retryText, btnFont, Brushes.White, rx, ry);

            // ✅ TIKLANABİLİR ALAN
            retryBtn = new Rectangle((int)rx, (int)ry, (int)rSize.Width, (int)rSize.Height);


            // 🏠 MENU (YAZI BUTONU)
            string menuText = "MENU";
            SizeF mSize = g.MeasureString(menuText, btnFont);

            float mx = (ClientSize.Width - mSize.Width) / 2;
            float my = 350;

            g.DrawString(menuText, btnFont, Brushes.White, mx, my);

            menuBtn = new Rectangle((int)mx, (int)my, (int)mSize.Width, (int)mSize.Height);
        }







        void DrawMenu(Graphics g)
        {
            Font f = new Font("Arial", 24, FontStyle.Bold);

            // 🟢 SNAKE GAME ORTADA
            string title = "SNAKE GAME";
            SizeF ts = g.MeasureString(title, f);

            g.DrawString(title, f, Brushes.Lime,
                (ClientSize.Width - ts.Width) / 2, 100);

           
            // 👤 PLAYER + SCORE (sol üst)
            string playerText = playerName + " | Score: " + score;

            g.DrawString(playerText,
                new Font("Arial", 14, FontStyle.Bold),
                Brushes.White,
                10,
                10);



            /// 👑 LEADER (oyun sırasında da gözüksün)
            string leader = "👑 " + GetLeader();
            Font leaderFont = new Font("Segoe UI Emoji", 14, FontStyle.Bold);

            SizeF ls = g.MeasureString(leader, leaderFont);

            g.DrawString(leader,
                leaderFont,
                Brushes.Gold,
                ClientSize.Width - ls.Width - 10,
                10);

            // 🔘 BUTONLAR (SENİN KODUN AYNEN)
            DrawBtn(g, startBtn, "START");
            DrawBtn(g, nameBtn, "Name: " + playerName);
            DrawBtn(g, foodBtn, "Food: " + foodCount);
            DrawBtn(g, colorBtn, "Color");
            DrawBtn(g, speedBtn, "Speed: " + speedMode);

            // 🎨 COLOR PANEL (SENİN KODUN)
            if (colorPanel)
            {
                int x = ClientSize.Width / 2 - 150;
                int y = 470;
                int size = 20;
                int cols = 10;

                for (int i = 0; i < colors.Count; i++)
                {
                    float scale = 1f - (i * 0.02f);
                    if (scale < 0.6f) scale = 0.6f;
                    int cx = x + (i % cols) * (size + 5);
                    int cy = y + (i / cols) * (size + 5);

                    g.FillRectangle(new SolidBrush(colors[i]), cx, cy, size, size);
                }
            }
        }




        void DrawBtn(Graphics g, Rectangle r, string text)
        {
            g.FillRectangle(Brushes.DarkSlateGray, r);
            g.DrawRectangle(Pens.White, r);
            g.DrawString(text, new Font("Arial", 12), Brushes.White, r.X + 10, r.Y + 10);
        }

        void MouseClick(object sender, MouseEventArgs e)
        {
            // ✅ 1. GAME OVER DURUMU
            if (gameOver)
            {
                if (retryBtn.Contains(e.Location))
                {
                    StartGame();
                }
                else if (menuBtn.Contains(e.Location))
                {
                    gameOver = false;
                    gameStarted = false;
                    colorPanel = false;

                    Invalidate();
                }

                return; // 🔥 EN ÖNEMLİ SATIR (bug fix)
            }

            // ✅ 2. MENU DURUMU
            if (!gameStarted)
            {
                if (startBtn.Contains(e.Location)) StartGame();

                if (nameBtn.Contains(e.Location))
                {
                    string input = Microsoft.VisualBasic.Interaction.InputBox("Name:", "", playerName);
                    if (!string.IsNullOrWhiteSpace(input))
                        playerName = input;
                }

                if (foodBtn.Contains(e.Location))
                {
                    foodCount++;
                    if (foodCount > 5) foodCount = 1;
                }

                if (colorBtn.Contains(e.Location))
                    colorPanel = !colorPanel;

                if (speedBtn.Contains(e.Location))
                {
                    if (speedMode == "Slow") speedMode = "Medium";
                    else if (speedMode == "Medium") speedMode = "Fast";
                    else speedMode = "Slow";
                }

                if (colorPanel)
                {
                    int x = ClientSize.Width / 2 - 150;
                    int y = 470;
                    int size = 20;
                    int cols = 10;

                    for (int i = 0; i < colors.Count; i++)
                    {
                        int cx = x + (i % cols) * (size + 5);
                        int cy = y + (i / cols) * (size + 5);

                        Rectangle r = new Rectangle(cx, cy, size, size);

                        if (r.Contains(e.Location))
                        {
                            selectedColor = colors[i];
                            colorPanel = false;
                            break;
                        }
                    }
                }

                Invalidate();
            }
        }

        void KeyPress(object sender, KeyEventArgs e)
        {
            if (!gameStarted && e.KeyCode == Keys.Enter)
                StartGame();

            if (directionChanged) return;

            switch (e.KeyCode)
            {
                case Keys.Up: if (dy != 1) { dx = 0; dy = -1; directionChanged = true; } break;
                case Keys.Down: if (dy != -1) { dx = 0; dy = 1; directionChanged = true; } break;
                case Keys.Left: if (dx != 1) { dx = -1; dy = 0; directionChanged = true; } break;
                case Keys.Right: if (dx != -1) { dx = 1; dy = 0; directionChanged = true; } break;
                case Keys.W: if (dy != 1) { dx = 0; dy = -1; directionChanged = true; } break;
                case Keys.S: if (dy != -1) { dx = 0; dy = 1; directionChanged = true; } break;
                case Keys.A: if (dx != 1) { dx = -1; dy = 0; directionChanged = true; } break;
                case Keys.D: if (dx != -1) { dx = 1; dy = 0; directionChanged = true; } break;
                case Keys.Space: timer.Enabled = !timer.Enabled; break;
            }
        }

        private void Form1_Load(object sender, EventArgs e)
        {

        }
    }
}