using System;
using System.Collections.Generic;
using System.Threading;

class Program
{
    static int width = 30;
    static int height = 20;

    static int offsetX;
    static int offsetY;

    static List<(int x, int y)> snake = new List<(int, int)>();
    static (int x, int y) food;

    static int dx = 1;
    static int dy = 0;

    static bool gameOver = false;
    static Random rand = new Random();
    static int score = 0;

    static int speed = 100;

    static void Main()
    {
        Console.CursorVisible = false;

        // Ekranın ortasına hizalama
        offsetX = (Console.WindowWidth - width) / 2;
        offsetY = (Console.WindowHeight - height) / 2;

        // Ortadan başlat
        int startX = width / 2;
        int startY = height / 2;
        snake.Add((startX, startY));

        GenerateFood();

        while (!gameOver)
        {
            Draw();
            Input();
            Update();
            Thread.Sleep(speed);
        }

        Console.SetCursorPosition(offsetX, offsetY + height + 3);
        Console.WriteLine($"Oyun Bitti! Puan: {score}");
    }

    static void Draw()
    {
        // Üst sınır
        Console.SetCursorPosition(offsetX, offsetY);
        Console.Write("+");
        for (int i = 0; i < width; i++)
            Console.Write("-");
        Console.Write("+");

        for (int y = 0; y < height; y++)
        {
            Console.SetCursorPosition(offsetX, offsetY + y + 1);
            Console.Write("|");

            for (int x = 0; x < width; x++)
            {
                if (snake.Exists(s => s.x == x && s.y == y))
                    Console.Write("O");
                else if (x == food.x && y == food.y)
                    Console.Write("X");
                else
                    Console.Write(" ");
            }

            Console.Write("|");
        }

        // Alt sınır
        Console.SetCursorPosition(offsetX, offsetY + height + 1);
        Console.Write("+");
        for (int i = 0; i < width; i++)
            Console.Write("-");
        Console.Write("+");

        // Skor
        Console.SetCursorPosition(offsetX, offsetY + height + 2);
        Console.Write($"Puan: {score}   ");
    }

    static void Input()
    {
        if (Console.KeyAvailable)
        {
            var key = Console.ReadKey(true).Key;

            switch (key)
            {
                case ConsoleKey.UpArrow:
                    if (dy != 1) { dx = 0; dy = -1; }
                    break;
                case ConsoleKey.DownArrow:
                    if (dy != -1) { dx = 0; dy = 1; }
                    break;
                case ConsoleKey.LeftArrow:
                    if (dx != 1) { dx = -1; dy = 0; }
                    break;
                case ConsoleKey.RightArrow:
                    if (dx != -1) { dx = 1; dy = 0; }
                    break;
            }
        }
    }

    static void Update()
    {
        var head = snake[0];
        var newHead = (head.x + dx, head.y + dy);

        // Duvara çarpma
        if (newHead.Item1 < 0 || newHead.Item1 >= width || newHead.Item2 < 0 || newHead.Item2 >= height)
        {
            gameOver = true;
            return;
        }

        // Kendine çarpma
        if (snake.Contains(newHead))
        {
            gameOver = true;
            return;
        }

        snake.Insert(0, newHead);

        // Yem yeme
        if (newHead.Item1 == food.x && newHead.Item2 == food.y)
        {
            score += 10;

            // Her 100 puanda hız artışı
            if (score % 100 == 0 && speed > 20)
            {
                speed = (int)(speed / 1.2);
            }

            GenerateFood();
        }
        else
        {
            snake.RemoveAt(snake.Count - 1);
        }
    }

    static void GenerateFood()
    {
        food = (rand.Next(0, width), rand.Next(0, height));
    }
}