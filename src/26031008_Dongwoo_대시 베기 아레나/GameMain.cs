using System.Numerics;
using Vortice.Direct2D1;
using Vortice.Mathematics;

class GameMain : G2AppBase
{
    public override System.Drawing.Size ScreenSize => GameGlobal.ScreenSize;
    public override string GameName => GameGlobal.GameName;

    // 한 번에 하나의 화면만 표시합니다.
    enum Screen { Start, Practice, End }
    Screen currentScreen = Screen.Start;
    int selectedButton = 0;

    // 이미지마다 이름이 있는 변수를 사용합니다.
    G2Texture startImage = null!;
    G2Texture endImage = null!;
    G2Texture arenaImage = null!;
    G2Texture playerImage = null!;
    G2Texture heartImage = null!;
    G2Texture trailImage = null!;

    G2Font smallFont = null!;
    G2Font normalFont = null!;
    G2Font buttonFont = null!;
    G2Font titleFont = null!;
    ID2D1SolidColorBrush brush = null!;

    Color4 cyan = new Color4(0.28f, 0.94f, 0.85f, 1);
    Color4 white = new Color4(0.91f, 0.95f, 0.97f, 1);
    Color4 gray = new Color4(0.51f, 0.63f, 0.68f, 1);
    RectangleF firstButton = new RectangleF(62, 370, 340, 58);
    RectangleF exitButton = new RectangleF(62, 440, 340, 44);

    // Vector2는 X와 Y를 함께 저장하는 좌표입니다.
    Vector2 playerPosition = new Vector2(480, 320);
    Vector2 direction = new Vector2(1, 0);
    float dashTime = 0;
    float waitTime = 0;

    protected override void Initialize()
    {
        ClearColor = new Color4(0.035f, 0.055f, 0.075f, 1);
        brush = RenderTarget.CreateSolidColorBrush(white);
        startImage = LoadImage("start_screen.png");
        endImage = LoadImage("end_screen.png");
        arenaImage = LoadImage("arena.png");
        playerImage = LoadImage("player.png");
        heartImage = LoadImage("heart.png");
        trailImage = LoadImage("dash_trail.png");

        smallFont = new G2Font("Malgun Gothic", 12);
        normalFont = new G2Font("Malgun Gothic", 14);
        buttonFont = new G2Font("Malgun Gothic", 20);
        titleFont = new G2Font("Malgun Gothic", 26);
    }

    G2Texture LoadImage(string fileName)
    {
        string path = Path.Combine(AppContext.BaseDirectory, "resource", fileName);
        return new G2Texture(path);
    }

    // 처음 누른 순간과 계속 누르고 있는 상태를 모두 확인합니다.
    bool IsHolding(Keys key)
    {
        return Input.IsKeyDown(key) || Input.IsKeyPress(key);
    }

    bool PressedEnter()
    {
        // Alt+Enter는 라이브러리의 전체 화면 전환용입니다.
        return Input.IsKeyDown(Keys.Enter) && !IsHolding(Keys.Menu);
    }

    void StartPractice()
    {
        currentScreen = Screen.Practice;
        playerPosition = new Vector2(480, 320);
        direction = new Vector2(1, 0);
        dashTime = 0;
        waitTime = 0;
    }

    void ReturnToStart()
    {
        currentScreen = Screen.Start;
        selectedButton = 0;
    }

    protected override void Update()
    {
        if (currentScreen == Screen.Start)
        {
            UpdateStart();
        }
        else if (currentScreen == Screen.Practice)
        {
            UpdatePractice();
        }
        else
        {
            UpdateEnd();
        }
    }

    void UpdateStart()
    {
        if (Input.IsKeyDown(Keys.Escape))
        {
            currentScreen = Screen.End;
            return;
        }

        if (Input.IsKeyDown(Keys.Up) || Input.IsKeyDown(Keys.Down) || Input.IsKeyDown(Keys.Tab))
        {
            if (selectedButton == 0)
                selectedButton = 1;
            else
                selectedButton = 0;
        }

        if (Input.MouseDelta != PointF.Empty)
        {
            if (firstButton.Contains(Input.MousePosition))
                selectedButton = 0;
            else if (exitButton.Contains(Input.MousePosition))
                selectedButton = 1;
        }

        if (Input.IsButtonDown(MouseButtons.Left))
        {
            if (firstButton.Contains(Input.MousePosition))
                StartPractice();
            else if (exitButton.Contains(Input.MousePosition))
                currentScreen = Screen.End;
            return;
        }

        if (PressedEnter())
        {
            if (selectedButton == 0)
                StartPractice();
            else
                currentScreen = Screen.End;
        }
    }

    void UpdateEnd()
    {
        if (Input.IsKeyDown(Keys.Escape))
        {
            Close();
            return;
        }
        if (Input.IsKeyDown(Keys.R))
        {
            StartPractice();
            return;
        }
        if (PressedEnter())
        {
            ReturnToStart();
            return;
        }
        if (Input.IsButtonDown(MouseButtons.Left))
        {
            if (firstButton.Contains(Input.MousePosition))
                ReturnToStart();
            else if (exitButton.Contains(Input.MousePosition))
                Close();
        }
    }

    void UpdatePractice()
    {
        if (Input.IsKeyDown(Keys.Escape))
        {
            currentScreen = Screen.End;
            return;
        }

        // 한 프레임에 너무 멀리 이동하지 않도록 시간을 제한합니다.
        float seconds = (float)Math.Min(DeltaTime, 0.04);
        Vector2 movement = new Vector2(0, 0);
        if (IsHolding(Keys.A) || IsHolding(Keys.Left)) movement.X -= 1;
        if (IsHolding(Keys.D) || IsHolding(Keys.Right)) movement.X += 1;
        if (IsHolding(Keys.W) || IsHolding(Keys.Up)) movement.Y -= 1;
        if (IsHolding(Keys.S) || IsHolding(Keys.Down)) movement.Y += 1;

        // 대각선도 상하좌우와 같은 속도로 이동하도록 길이를 1로 만듭니다.
        if (movement != Vector2.Zero)
            movement = Vector2.Normalize(movement);

        waitTime -= seconds;
        if (waitTime < 0) waitTime = 0;

        if (dashTime <= 0 && movement != Vector2.Zero)
            direction = movement;

        if (Input.IsKeyDown(Keys.Space) && dashTime <= 0 && waitTime <= 0)
            dashTime = 0.15f;

        if (dashTime > 0)
        {
            // 대시는 일반 이동 속도 240의 4배입니다.
            float moveTime = Math.Min(seconds, dashTime);
            playerPosition += direction * 960 * moveTime;
            dashTime -= seconds;

            bool hitWall = playerPosition.X < 50 || playerPosition.X > 910
                || playerPosition.Y < 112 || playerPosition.Y > 530;
            if (dashTime <= 0 || hitWall)
            {
                dashTime = 0;
                waitTime = 1;
            }
        }
        else
        {
            playerPosition += movement * 240 * seconds;
        }

        // 경기장 밖으로 나가지 않게 좌표를 제한합니다.
        if (playerPosition.X < 50) playerPosition.X = 50;
        if (playerPosition.X > 910) playerPosition.X = 910;
        if (playerPosition.Y < 112) playerPosition.Y = 112;
        if (playerPosition.Y > 530) playerPosition.Y = 530;
    }

    protected override void Render()
    {
        if (currentScreen == Screen.Start)
            DrawStart();
        else if (currentScreen == Screen.Practice)
            DrawPractice();
        else
            DrawEnd();
    }

    void DrawStart()
    {
        // 고정 이미지를 그리므로 시작 화면에는 애니메이션이 없습니다.
        startImage.Draw();
        Write("DASH / SLASH / SURVIVE", smallFont, 104, 53, 350, cyan);
        Write("단 한 번의 대시, 생존을 가르는 순간.", normalFont, 62, 279, 430, white);
        Write("적을 모아 베어내고, 60초 동안 살아남으세요.", normalFont, 62, 312, 440, gray);
        Write("이동은 자유롭게. 공격은 대시로.", normalFont, 542, 444, 330, white);
        DrawButton(firstButton, "시작하기", "ENTER", selectedButton == 0);
        DrawButton(exitButton, "게임 종료", "ESC", selectedButton == 1);
        Write("WASD / 방향키  이동     /     SPACE  대시     /     60초 생존", normalFont, 62, 550, 830, white);
        Write("대시로 적을 처치하고, 충전 중에는 빈 공간으로 이동하세요.", smallFont, 62, 582, 830, gray);
    }

    void DrawEnd()
    {
        endImage.Draw();
        Write("DASH ARENA / SESSION END", smallFont, 104, 53, 350, cyan);
        Write("다시 한번, 아레나에 도전하세요.", normalFont, 62, 279, 430, white);
        Write("타이틀로 돌아가거나 게임을 종료할 수 있습니다.", normalFont, 62, 312, 440, gray);
        Write("다음 도전을 기다립니다.", normalFont, 542, 444, 330, white);
        DrawButton(firstButton, "타이틀로 돌아가기", "ENTER", firstButton.Contains(Input.MousePosition));
        DrawButton(exitButton, "게임 종료", "ESC", exitButton.Contains(Input.MousePosition));
        Write("ENTER  타이틀     /     R  다시 시작     /     ESC  종료", normalFont, 62, 550, 830, white);
        Write("플레이해 주셔서 감사합니다.", smallFont, 62, 582, 830, gray);
    }

    void DrawPractice()
    {
        arenaImage.Draw(new Rect(0, 0, 960, 640), new Rect(0, 0, 960, 540), 0.4f);
        Write("조작 연습", titleFont, 32, 24, 300, white);
        Write("ESC  플레이 종료", normalFont, 704, 32, 235, gray);
        Write("전투 연결 전 준비 화면", normalFont, 360, 69, 300, gray);
        for (int i = 0; i < 3; i++)
            heartImage.Draw(new Rect(34 + i * 34, 72, 26, 26), new Rect(0, 0, 32, 32));

        if (dashTime > 0)
            trailImage.Draw(new Rect(playerPosition.X - 90, playerPosition.Y - 28, 100, 56),
                new Rect(0, 0, 128, 64), 0.7f);
        DrawPlayer();

        FillBox(new RectangleF(300, 567, 360, 8), gray);
        FillBox(new RectangleF(300, 567, 360 * (1 - waitTime), 8), cyan);
        string dashMessage = "대시 준비";
        if (dashTime > 0)
            dashMessage = "대시 중";
        else if (waitTime > 0)
            dashMessage = "대시 충전 중";
        Write(dashMessage, normalFont, 440, 584, 200, cyan);
        Write("WASD / 방향키  이동     SPACE  대시", smallFont, 32, 605, 400, gray);
    }

    void DrawPlayer()
    {
        // 캐릭터 그림만 이동 방향으로 회전시킨 뒤 원래 화면 상태로 되돌립니다.
        // Atan2는 X, Y 방향으로부터 회전 각도를 구합니다.
        Matrix3x2 original = RenderTarget.Transform;
        float angle = MathF.Atan2(direction.Y, direction.X);
        RenderTarget.Transform = Matrix3x2.CreateRotation(angle, playerPosition) * original;
        playerImage.Draw(new Rect(playerPosition.X - 28, playerPosition.Y - 28, 56, 56),
            new Rect(0, 0, 64, 64));
        RenderTarget.Transform = original;
    }

    // 여러 화면에서 반복하는 글자와 버튼 그리기를 함수로 묶었습니다.
    void Write(string message, G2Font font, float x, float y, float width, Color4 color)
    {
        font.DrawText(message, new Rect(x, y, width, 44), color);
    }

    void FillBox(RectangleF box, Color4 color)
    {
        brush.Color = color;
        RenderTarget.FillRectangle(new Rect(box.X, box.Y, box.Width, box.Height), brush);
    }

    void DrawButton(RectangleF box, string label, string shortcut, bool selected)
    {
        Color4 textColor = white;
        if (selected)
        {
            FillBox(box, cyan);
            textColor = ClearColor;
        }
        else
        {
            brush.Color = gray;
            RenderTarget.DrawRectangle(new Rect(box.X, box.Y, box.Width, box.Height), brush, 1);
        }
        G2Font font = normalFont;
        if (box.Height > 50)
            font = buttonFont;
        Write(label, font, box.X + 22, box.Y + 12, 240, textColor);
        Write(shortcut, smallFont, box.X + 270, box.Y + 17, 68, textColor);
    }

    public override void Dispose()
    {
        // 게임을 닫을 때 이미지와 글꼴이 사용한 자원을 정리합니다.
        startImage?.Dispose();
        endImage?.Dispose();
        arenaImage?.Dispose();
        playerImage?.Dispose();
        heartImage?.Dispose();
        trailImage?.Dispose();
        smallFont?.Dispose();
        normalFont?.Dispose();
        buttonFont?.Dispose();
        titleFont?.Dispose();
        brush?.Dispose();
        base.Dispose();
    }
}
