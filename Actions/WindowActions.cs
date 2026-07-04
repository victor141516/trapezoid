namespace Trapezoid;

internal enum WindowActionId
{
    LeftHalf,
    RightHalf,
    CenterHalf,
    TopHalf,
    BottomHalf,
    TopLeft,
    TopRight,
    BottomLeft,
    BottomRight,
    Maximize,
    AlmostMaximize,
    MaximizeHeight,
    Center,
    Restore,
    PreviousDisplay,
    NextDisplay,
    Larger,
    Smaller,
    LargerWidth,
    SmallerWidth,
    LargerHeight,
    SmallerHeight,
    FirstThird,
    FirstTwoThirds,
    CenterThird,
    CenterTwoThirds,
    LastTwoThirds,
    LastThird,
    TopVerticalThird,
    MiddleVerticalThird,
    BottomVerticalThird,
    TopVerticalTwoThirds,
    BottomVerticalTwoThirds,
    MoveLeft,
    MoveRight,
    MoveUp,
    MoveDown,
    FirstFourth,
    SecondFourth,
    ThirdFourth,
    LastFourth,
    FirstThreeFourths,
    CenterThreeFourths,
    LastThreeFourths,
    TopLeftSixth,
    TopCenterSixth,
    TopRightSixth,
    BottomLeftSixth,
    BottomCenterSixth,
    BottomRightSixth,
    TopLeftThird,
    TopRightThird,
    BottomLeftThird,
    BottomRightThird,
    TopLeftEighth,
    TopCenterLeftEighth,
    TopCenterRightEighth,
    TopRightEighth,
    BottomLeftEighth,
    BottomCenterLeftEighth,
    BottomCenterRightEighth,
    BottomRightEighth,
    TopLeftNinth,
    TopCenterNinth,
    TopRightNinth,
    MiddleLeftNinth,
    MiddleCenterNinth,
    MiddleRightNinth,
    BottomLeftNinth,
    BottomCenterNinth,
    BottomRightNinth,
    TopLeftTwelfth,
    TopCenterLeftTwelfth,
    TopCenterRightTwelfth,
    TopRightTwelfth,
    MiddleLeftTwelfth,
    MiddleCenterLeftTwelfth,
    MiddleCenterRightTwelfth,
    MiddleRightTwelfth,
    BottomLeftTwelfth,
    BottomCenterLeftTwelfth,
    BottomCenterRightTwelfth,
    BottomRightTwelfth,
    TopLeftSixteenth,
    TopCenterLeftSixteenth,
    TopCenterRightSixteenth,
    TopRightSixteenth,
    UpperMiddleLeftSixteenth,
    UpperMiddleCenterLeftSixteenth,
    UpperMiddleCenterRightSixteenth,
    UpperMiddleRightSixteenth,
    LowerMiddleLeftSixteenth,
    LowerMiddleCenterLeftSixteenth,
    LowerMiddleCenterRightSixteenth,
    LowerMiddleRightSixteenth,
    BottomLeftSixteenth,
    BottomCenterLeftSixteenth,
    BottomCenterRightSixteenth,
    BottomRightSixteenth,
    DisplayOne,
    DisplayTwo,
    DisplayThree,
    DisplayFour,
    DisplayFive,
    DisplaySix,
    DisplaySeven,
    DisplayEight,
    DisplayNine
}

internal readonly record struct LayoutPreview(float X, float Y, float Width, float Height);

internal sealed record WindowActionDefinition(
    WindowActionId Id,
    string DisplayName,
    string Category,
    LayoutPreview? Preview,
    Hotkey? DefaultShortcut);

internal static class WindowActions
{
    private const string Sides = "Sides";
    private const string Corners = "Corners";
    private const string Maximize = "Maximize";
    private const string Size = "Size";
    private const string Display = "Display";
    private const string Thirds = "Thirds";
    private const string Move = "Move to Edge";
    private const string Fourths = "Fourths";
    private const string Sixths = "Sixths";
    private const string Eighths = "Eighths";
    private const string Ninths = "Ninths";
    private const string Twelfths = "Twelfths";
    private const string Sixteenths = "Sixteenths";

    public static IReadOnlyList<WindowActionDefinition> All { get; } = BuildActions();

    public static IReadOnlyDictionary<WindowActionId, WindowActionDefinition> ById { get; } =
        All.ToDictionary(action => action.Id);

    public static IEnumerable<IGrouping<string, WindowActionDefinition>> Groups =>
        All.GroupBy(action => action.Category);

    private static IReadOnlyList<WindowActionDefinition> BuildActions()
    {
        var actions = new List<WindowActionDefinition>
        {
            Def(WindowActionId.LeftHalf, "Left Side", Sides, Portion(0, 0, 0.5f, 1), CtrlAlt(Keys.Left)),
            Def(WindowActionId.RightHalf, "Right Side", Sides, Portion(0.5f, 0, 0.5f, 1), CtrlAlt(Keys.Right)),
            Def(WindowActionId.CenterHalf, "Center Section", Sides, Portion(0.25f, 0, 0.5f, 1)),
            Def(WindowActionId.TopHalf, "Top Side", Sides, Portion(0, 0, 1, 0.5f), CtrlAlt(Keys.Up)),
            Def(WindowActionId.BottomHalf, "Bottom Side", Sides, Portion(0, 0.5f, 1, 0.5f), CtrlAlt(Keys.Down)),

            Def(WindowActionId.TopLeft, "Top Left", Corners, Portion(0, 0, 0.5f, 0.5f), CtrlAlt(Keys.U)),
            Def(WindowActionId.TopRight, "Top Right", Corners, Portion(0.5f, 0, 0.5f, 0.5f), CtrlAlt(Keys.I)),
            Def(WindowActionId.BottomLeft, "Bottom Left", Corners, Portion(0, 0.5f, 0.5f, 0.5f), CtrlAlt(Keys.J)),
            Def(WindowActionId.BottomRight, "Bottom Right", Corners, Portion(0.5f, 0.5f, 0.5f, 0.5f), CtrlAlt(Keys.K)),

            Def(WindowActionId.Maximize, "Maximize", Maximize, Portion(0, 0, 1, 1), CtrlAlt(Keys.Return)),
            Def(WindowActionId.AlmostMaximize, "Almost Maximize", Maximize, Portion(0.05f, 0.05f, 0.9f, 0.9f)),
            Def(WindowActionId.MaximizeHeight, "Maximize Height", Maximize, Portion(0.2f, 0, 0.6f, 1), CtrlAltShift(Keys.Up)),
            Def(WindowActionId.Center, "Center", Maximize, Portion(0.25f, 0.25f, 0.5f, 0.5f), CtrlAlt(Keys.C)),
            Def(WindowActionId.Restore, "Restore", Maximize, null, CtrlAlt(Keys.Delete)),

            Def(WindowActionId.PreviousDisplay, "Previous Display", Display, null, CtrlAltWin(Keys.Left)),
            Def(WindowActionId.NextDisplay, "Next Display", Display, null, CtrlAltWin(Keys.Right)),
            Def(WindowActionId.DisplayOne, "Display 1", Display, null),
            Def(WindowActionId.DisplayTwo, "Display 2", Display, null),
            Def(WindowActionId.DisplayThree, "Display 3", Display, null),
            Def(WindowActionId.DisplayFour, "Display 4", Display, null),
            Def(WindowActionId.DisplayFive, "Display 5", Display, null),
            Def(WindowActionId.DisplaySix, "Display 6", Display, null),
            Def(WindowActionId.DisplaySeven, "Display 7", Display, null),
            Def(WindowActionId.DisplayEight, "Display 8", Display, null),
            Def(WindowActionId.DisplayNine, "Display 9", Display, null),

            Def(WindowActionId.Larger, "Larger", Size, null, CtrlAlt(Keys.Oemplus)),
            Def(WindowActionId.Smaller, "Smaller", Size, null, CtrlAlt(Keys.OemMinus)),
            Def(WindowActionId.LargerWidth, "Larger Width", Size, null),
            Def(WindowActionId.SmallerWidth, "Smaller Width", Size, null),
            Def(WindowActionId.LargerHeight, "Larger Height", Size, null),
            Def(WindowActionId.SmallerHeight, "Smaller Height", Size, null),

            Def(WindowActionId.FirstThird, "First Third", Thirds, Portion(0, 0, 1f / 3f, 1), CtrlAlt(Keys.D)),
            Def(WindowActionId.FirstTwoThirds, "First Two Thirds", Thirds, Portion(0, 0, 2f / 3f, 1), CtrlAlt(Keys.E)),
            Def(WindowActionId.CenterThird, "Center Third", Thirds, Portion(1f / 3f, 0, 1f / 3f, 1), CtrlAlt(Keys.F)),
            Def(WindowActionId.CenterTwoThirds, "Center Two Thirds", Thirds, Portion(1f / 6f, 0, 2f / 3f, 1), CtrlAlt(Keys.R)),
            Def(WindowActionId.LastTwoThirds, "Last Two Thirds", Thirds, Portion(1f / 3f, 0, 2f / 3f, 1), CtrlAlt(Keys.T)),
            Def(WindowActionId.LastThird, "Last Third", Thirds, Portion(2f / 3f, 0, 1f / 3f, 1), CtrlAlt(Keys.G)),
            Def(WindowActionId.TopVerticalThird, "Top Third", Thirds, Portion(0, 0, 1, 1f / 3f)),
            Def(WindowActionId.MiddleVerticalThird, "Middle Third", Thirds, Portion(0, 1f / 3f, 1, 1f / 3f)),
            Def(WindowActionId.BottomVerticalThird, "Bottom Third", Thirds, Portion(0, 2f / 3f, 1, 1f / 3f)),
            Def(WindowActionId.TopVerticalTwoThirds, "Top Two Thirds", Thirds, Portion(0, 0, 1, 2f / 3f)),
            Def(WindowActionId.BottomVerticalTwoThirds, "Bottom Two Thirds", Thirds, Portion(0, 1f / 3f, 1, 2f / 3f)),

            Def(WindowActionId.MoveLeft, "Move Left", Move, Portion(0, 0.25f, 0.5f, 0.5f)),
            Def(WindowActionId.MoveRight, "Move Right", Move, Portion(0.5f, 0.25f, 0.5f, 0.5f)),
            Def(WindowActionId.MoveUp, "Move Up", Move, Portion(0.25f, 0, 0.5f, 0.5f)),
            Def(WindowActionId.MoveDown, "Move Down", Move, Portion(0.25f, 0.5f, 0.5f, 0.5f)),

            Def(WindowActionId.FirstFourth, "First Fourth", Fourths, Portion(0, 0, 0.25f, 1)),
            Def(WindowActionId.SecondFourth, "Second Fourth", Fourths, Portion(0.25f, 0, 0.25f, 1)),
            Def(WindowActionId.ThirdFourth, "Third Fourth", Fourths, Portion(0.5f, 0, 0.25f, 1)),
            Def(WindowActionId.LastFourth, "Last Fourth", Fourths, Portion(0.75f, 0, 0.25f, 1)),
            Def(WindowActionId.FirstThreeFourths, "First Three Fourths", Fourths, Portion(0, 0, 0.75f, 1)),
            Def(WindowActionId.CenterThreeFourths, "Center Three Fourths", Fourths, Portion(0.125f, 0, 0.75f, 1)),
            Def(WindowActionId.LastThreeFourths, "Last Three Fourths", Fourths, Portion(0.25f, 0, 0.75f, 1)),

            Def(WindowActionId.TopLeftSixth, "Top Left Sixth", Sixths, Grid(0, 0, 3, 2)),
            Def(WindowActionId.TopCenterSixth, "Top Center Sixth", Sixths, Grid(1, 0, 3, 2)),
            Def(WindowActionId.TopRightSixth, "Top Right Sixth", Sixths, Grid(2, 0, 3, 2)),
            Def(WindowActionId.BottomLeftSixth, "Bottom Left Sixth", Sixths, Grid(0, 1, 3, 2)),
            Def(WindowActionId.BottomCenterSixth, "Bottom Center Sixth", Sixths, Grid(1, 1, 3, 2)),
            Def(WindowActionId.BottomRightSixth, "Bottom Right Sixth", Sixths, Grid(2, 1, 3, 2)),

            Def(WindowActionId.TopLeftThird, "Top Left Third", Thirds, Portion(0, 0, 2f / 3f, 0.5f)),
            Def(WindowActionId.TopRightThird, "Top Right Third", Thirds, Portion(1f / 3f, 0, 2f / 3f, 0.5f)),
            Def(WindowActionId.BottomLeftThird, "Bottom Left Third", Thirds, Portion(0, 0.5f, 2f / 3f, 0.5f)),
            Def(WindowActionId.BottomRightThird, "Bottom Right Third", Thirds, Portion(1f / 3f, 0.5f, 2f / 3f, 0.5f)),

            Def(WindowActionId.TopLeftEighth, "Top Left Eighth", Eighths, Grid(0, 0, 4, 2)),
            Def(WindowActionId.TopCenterLeftEighth, "Top Center Left Eighth", Eighths, Grid(1, 0, 4, 2)),
            Def(WindowActionId.TopCenterRightEighth, "Top Center Right Eighth", Eighths, Grid(2, 0, 4, 2)),
            Def(WindowActionId.TopRightEighth, "Top Right Eighth", Eighths, Grid(3, 0, 4, 2)),
            Def(WindowActionId.BottomLeftEighth, "Bottom Left Eighth", Eighths, Grid(0, 1, 4, 2)),
            Def(WindowActionId.BottomCenterLeftEighth, "Bottom Center Left Eighth", Eighths, Grid(1, 1, 4, 2)),
            Def(WindowActionId.BottomCenterRightEighth, "Bottom Center Right Eighth", Eighths, Grid(2, 1, 4, 2)),
            Def(WindowActionId.BottomRightEighth, "Bottom Right Eighth", Eighths, Grid(3, 1, 4, 2)),

            Def(WindowActionId.TopLeftNinth, "Top Left Ninth", Ninths, Grid(0, 0, 3, 3)),
            Def(WindowActionId.TopCenterNinth, "Top Center Ninth", Ninths, Grid(1, 0, 3, 3)),
            Def(WindowActionId.TopRightNinth, "Top Right Ninth", Ninths, Grid(2, 0, 3, 3)),
            Def(WindowActionId.MiddleLeftNinth, "Middle Left Ninth", Ninths, Grid(0, 1, 3, 3)),
            Def(WindowActionId.MiddleCenterNinth, "Middle Center Ninth", Ninths, Grid(1, 1, 3, 3)),
            Def(WindowActionId.MiddleRightNinth, "Middle Right Ninth", Ninths, Grid(2, 1, 3, 3)),
            Def(WindowActionId.BottomLeftNinth, "Bottom Left Ninth", Ninths, Grid(0, 2, 3, 3)),
            Def(WindowActionId.BottomCenterNinth, "Bottom Center Ninth", Ninths, Grid(1, 2, 3, 3)),
            Def(WindowActionId.BottomRightNinth, "Bottom Right Ninth", Ninths, Grid(2, 2, 3, 3)),

            Def(WindowActionId.TopLeftTwelfth, "Top Left Twelfth", Twelfths, Grid(0, 0, 4, 3)),
            Def(WindowActionId.TopCenterLeftTwelfth, "Top Center Left Twelfth", Twelfths, Grid(1, 0, 4, 3)),
            Def(WindowActionId.TopCenterRightTwelfth, "Top Center Right Twelfth", Twelfths, Grid(2, 0, 4, 3)),
            Def(WindowActionId.TopRightTwelfth, "Top Right Twelfth", Twelfths, Grid(3, 0, 4, 3)),
            Def(WindowActionId.MiddleLeftTwelfth, "Middle Left Twelfth", Twelfths, Grid(0, 1, 4, 3)),
            Def(WindowActionId.MiddleCenterLeftTwelfth, "Middle Center Left Twelfth", Twelfths, Grid(1, 1, 4, 3)),
            Def(WindowActionId.MiddleCenterRightTwelfth, "Middle Center Right Twelfth", Twelfths, Grid(2, 1, 4, 3)),
            Def(WindowActionId.MiddleRightTwelfth, "Middle Right Twelfth", Twelfths, Grid(3, 1, 4, 3)),
            Def(WindowActionId.BottomLeftTwelfth, "Bottom Left Twelfth", Twelfths, Grid(0, 2, 4, 3)),
            Def(WindowActionId.BottomCenterLeftTwelfth, "Bottom Center Left Twelfth", Twelfths, Grid(1, 2, 4, 3)),
            Def(WindowActionId.BottomCenterRightTwelfth, "Bottom Center Right Twelfth", Twelfths, Grid(2, 2, 4, 3)),
            Def(WindowActionId.BottomRightTwelfth, "Bottom Right Twelfth", Twelfths, Grid(3, 2, 4, 3)),

            Def(WindowActionId.TopLeftSixteenth, "Top Left Sixteenth", Sixteenths, Grid(0, 0, 4, 4)),
            Def(WindowActionId.TopCenterLeftSixteenth, "Top Center Left Sixteenth", Sixteenths, Grid(1, 0, 4, 4)),
            Def(WindowActionId.TopCenterRightSixteenth, "Top Center Right Sixteenth", Sixteenths, Grid(2, 0, 4, 4)),
            Def(WindowActionId.TopRightSixteenth, "Top Right Sixteenth", Sixteenths, Grid(3, 0, 4, 4)),
            Def(WindowActionId.UpperMiddleLeftSixteenth, "Upper Middle Left Sixteenth", Sixteenths, Grid(0, 1, 4, 4)),
            Def(WindowActionId.UpperMiddleCenterLeftSixteenth, "Upper Middle Center Left Sixteenth", Sixteenths, Grid(1, 1, 4, 4)),
            Def(WindowActionId.UpperMiddleCenterRightSixteenth, "Upper Middle Center Right Sixteenth", Sixteenths, Grid(2, 1, 4, 4)),
            Def(WindowActionId.UpperMiddleRightSixteenth, "Upper Middle Right Sixteenth", Sixteenths, Grid(3, 1, 4, 4)),
            Def(WindowActionId.LowerMiddleLeftSixteenth, "Lower Middle Left Sixteenth", Sixteenths, Grid(0, 2, 4, 4)),
            Def(WindowActionId.LowerMiddleCenterLeftSixteenth, "Lower Middle Center Left Sixteenth", Sixteenths, Grid(1, 2, 4, 4)),
            Def(WindowActionId.LowerMiddleCenterRightSixteenth, "Lower Middle Center Right Sixteenth", Sixteenths, Grid(2, 2, 4, 4)),
            Def(WindowActionId.LowerMiddleRightSixteenth, "Lower Middle Right Sixteenth", Sixteenths, Grid(3, 2, 4, 4)),
            Def(WindowActionId.BottomLeftSixteenth, "Bottom Left Sixteenth", Sixteenths, Grid(0, 3, 4, 4)),
            Def(WindowActionId.BottomCenterLeftSixteenth, "Bottom Center Left Sixteenth", Sixteenths, Grid(1, 3, 4, 4)),
            Def(WindowActionId.BottomCenterRightSixteenth, "Bottom Center Right Sixteenth", Sixteenths, Grid(2, 3, 4, 4)),
            Def(WindowActionId.BottomRightSixteenth, "Bottom Right Sixteenth", Sixteenths, Grid(3, 3, 4, 4))
        };

        return actions;
    }

    private static WindowActionDefinition Def(
        WindowActionId id,
        string displayName,
        string category,
        LayoutPreview? preview,
        Hotkey? defaultShortcut = null)
    {
        return new WindowActionDefinition(id, displayName, category, preview, defaultShortcut);
    }

    private static LayoutPreview Portion(float x, float y, float width, float height)
    {
        return new LayoutPreview(x, y, width, height);
    }

    private static LayoutPreview Grid(int column, int row, int columns, int rows)
    {
        return new LayoutPreview(
            column / (float)columns,
            row / (float)rows,
            1f / columns,
            1f / rows);
    }

    private static Hotkey CtrlAlt(Keys key)
    {
        return new Hotkey(HotkeyModifiers.Control | HotkeyModifiers.Alt, key);
    }

    private static Hotkey CtrlAltShift(Keys key)
    {
        return new Hotkey(HotkeyModifiers.Control | HotkeyModifiers.Alt | HotkeyModifiers.Shift, key);
    }

    private static Hotkey CtrlAltWin(Keys key)
    {
        return new Hotkey(HotkeyModifiers.Control | HotkeyModifiers.Alt | HotkeyModifiers.Win, key);
    }
}
