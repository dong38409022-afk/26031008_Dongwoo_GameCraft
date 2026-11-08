using System.Numerics;

// 빠른 대시는 도착 지점뿐 아니라 지나온 경로 전체를 확인합니다.
static class DashCollision
{
    public static bool Hits(Vector2 start, Vector2 end, Vector2 enemy)
    {
        Vector2 path = end - start;
        float lengthSquared = path.LengthSquared();
        Vector2 closest = start;

        if (lengthSquared > 0)
        {
            // 적과 가장 가까운 경로 위 지점을 구합니다.
            float progress = Vector2.Dot(enemy - start, path) / lengthSquared;
            if (progress < 0) progress = 0;
            if (progress > 1) progress = 1;
            closest = start + path * progress;
        }

        // 플레이어 반지름 20 + 적 반지름 18
        return Vector2.DistanceSquared(closest, enemy) <= 38 * 38;
    }
}
