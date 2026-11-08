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
    G2Texture enemyImage = null!;
    G2Texture warningImage = null!;

    // 적 하나마다 위치와 등장 전 대기시간을 저장합니다.
    class Enemy
    {
        public Vector2 Position;
        public float WarningTime = 0.5f;
    }
    List<Enemy> enemies = new List<Enemy>();
    // 처치 효과는 9장의 이미지를 순서대로 보여 줍니다.
    class KillEffect
    {
        public Vector2 Position;
        public float Time;
    }
    List<KillEffect> killEffects = new List<KillEffect>();
    G2Texture[] killImages = new G2Texture[9];
    Random random = new Random();
    float spawnTime = 0;
    float playTime = 0;

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
        startImage = LoadImage("screens/start_screen.png");
        endImage = LoadImage("screens/end_screen.png");
        arenaImage = LoadImage("backgrounds/arena.png");
        playerImage = LoadImage("characters/player.png");
        heartImage = LoadImage("ui/heart.png");
        trailImage = LoadImage("effects/dash_trail.png");
        enemyImage = LoadImage("characters/enemy.png");
        warningImage = LoadImage("effects/spawn_warning.png");

        for (int i = 0; i < killImages.Length; i++)
            killImages[i] = LoadImage("effects/kill/kill_" + i.ToString("00") + ".png");

        smallFont = new G2Font("Malgun Gothic", 12);
        normalFont = new G2Font("Malgun Gothic", 14);
        buttonFont = new G2Font("Malgun Gothic", 20);
        titleFont = new G2Font("Malgun Gothic", 26);
    }

    G2Texture LoadImage(string fileName)
    {
        DirectoryInfo? folder = new DirectoryInfo(AppContext.BaseDirectory);
        // 실행 폴더에 없다면 프로젝트 상위의 원본 resource 폴더를 찾습니다.
        while (folder != null)
        {
            string path = Path.Combine(folder.FullName, "resource", fileName);
            if (File.Exists(path))
                return new G2Texture(path);
            folder = folder.Parent;
        }
        throw new FileNotFoundException("이미지가 없습니다: resource/" + fileName
            + "\n프로젝트 전체 폴더를 복사한 뒤 다시 빌드해 주세요.");
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
        enemies.Clear();
        killEffects.Clear();
        spawnTime = 0;
        playTime = 0;
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

        // 이동 전 위치도 저장하여 빠르게 통과한 적을 놓치지 않습니다.
        Vector2 previousPosition = playerPosition;
        bool attacking = dashTime > 0;
        UpdateKillEffects(seconds);

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
        if (attacking)
            KillEnemies(previousPosition, playerPosition);
        UpdateEnemies(seconds);
    }

    void KillEnemies(Vector2 start, Vector2 end)
    {
        // 뒤에서부터 삭제해야 아직 확인하지 않은 적의 번호가 바뀌지 않습니다.
        for (int i = enemies.Count - 1; i >= 0; i--)
        {
            Enemy enemy = enemies[i];
            if (enemy.WarningTime > 0)
                continue;
            if (!DashCollision.Hits(start, end, enemy.Position))
                continue;

            KillEffect effect = new KillEffect();
            effect.Position = enemy.Position;
            killEffects.Add(effect);
            enemies.RemoveAt(i);
        }
    }

    void UpdateKillEffects(float seconds)
    {
        for (int i = killEffects.Count - 1; i >= 0; i--)
        {
            killEffects[i].Time += seconds;
            if (killEffects[i].Time >= 0.36f)
                killEffects.RemoveAt(i);
        }
    }

    void DrawKillEffects()
    {
        foreach (KillEffect effect in killEffects)
        {
            int frame = (int)(effect.Time / 0.04f);
            if (frame >= killImages.Length)
                frame = killImages.Length - 1;
            killImages[frame].Draw(
                new Rect(effect.Position.X - 48, effect.Position.Y - 48, 96, 96),
                new Rect(0, 0, 128, 128));
        }
    }

    void UpdateEnemies(float seconds)
    {
        playTime += seconds;
        spawnTime += seconds;
        float interval = 1;
        if (playTime >= 40)
            interval = 0.4f;
        else if (playTime >= 20)
            interval = 0.7f;

        // 경고 중인 적도 포함하여 최대 40마리까지만 만듭니다.
        if (spawnTime >= interval)
        {
            spawnTime = 0;
            if (enemies.Count < 40)
                SpawnEnemy();
        }

        foreach (Enemy enemy in enemies)
        {
            if (enemy.WarningTime > 0)
            {
                enemy.WarningTime -= seconds;
                continue;
            }

            // 플레이어보다 느린 속도(90)로 따라옵니다.
            Vector2 distance = playerPosition - enemy.Position;
            float length = distance.Length();
            if (length > 1)
                enemy.Position += distance / length * Math.Min(90 * seconds, length);
        }
    }

    void SpawnEnemy()
    {
        // 플레이어와 가까운 자리는 피하고 경기장 안쪽 가장자리를 고릅니다.
        for (int attempt = 0; attempt < 20; attempt++)
        {
            Vector2 position = new Vector2(random.Next(50, 911), random.Next(112, 531));
            int edge = random.Next(4);
            if (edge == 0) position.X = 50;
            else if (edge == 1) position.X = 910;
            else if (edge == 2) position.Y = 112;
            else position.Y = 530;

            if (Vector2.Distance(position, playerPosition) < 180)
                continue;

            Enemy enemy = new Enemy();
            enemy.Position = position;
            enemies.Add(enemy);
            return;
        }
    }

    void DrawEnemies()
    {
        foreach (Enemy enemy in enemies)
        {
            Rect area = new Rect(enemy.Position.X - 24, enemy.Position.Y - 24, 48, 48);
            if (enemy.WarningTime > 0)
                warningImage.Draw(area, new Rect(0, 0, 64, 64));
            else
                enemyImage.Draw(area, new Rect(0, 0, 64, 64));
        }
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
        Write("대시 처치 연습", titleFont, 32, 24, 300, white);
        Write("ESC  플레이 종료", normalFont, 704, 32, 235, gray);
        Write("적 " + enemies.Count + " / 40  (경고 포함)", normalFont, 360, 69, 300, gray);
        for (int i = 0; i < 3; i++)
            heartImage.Draw(new Rect(34 + i * 34, 72, 26, 26), new Rect(0, 0, 32, 32));

        if (dashTime > 0)
            trailImage.Draw(new Rect(playerPosition.X - 90, playerPosition.Y - 28, 100, 56),
                new Rect(0, 0, 128, 64), 0.7f);
        DrawEnemies();
        DrawKillEffects();
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
        enemyImage?.Dispose();
        warningImage?.Dispose();
        foreach (G2Texture image in killImages)
            image?.Dispose();
        smallFont?.Dispose();
        normalFont?.Dispose();
        buttonFont?.Dispose();
        titleFont?.Dispose();
        brush?.Dispose();
        base.Dispose();
    }
}




