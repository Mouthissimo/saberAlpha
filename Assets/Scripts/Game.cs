﻿public static class Game
{
    private static GameTypeEnum gameType;
    private static bool gamePaused = false;

    public static GameTypeEnum GetGameType()
    {
        return gameType;
    }

    public static bool GetGamePaused()
    {
        return gamePaused;
    }

    public static void SetGameType(GameTypeEnum arg_gameType)
    {
        gameType = arg_gameType;
    }

    public static void SetGamePaused(bool arg_isGamePaused)
    {
        gamePaused = arg_isGamePaused;
    }
}