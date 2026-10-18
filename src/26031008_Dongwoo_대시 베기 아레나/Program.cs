namespace _26031008_Dongwoo_대시베기아레나
{
    internal static class Program
    {
        [STAThread]
        private static void Main()
        {
            ApplicationConfiguration.Initialize();
            using GameMain game = new();
            game.Run();
        }
    }
}