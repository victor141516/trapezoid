namespace Trapezoid;

internal static class WindowLayoutCalculator
{
    private const int MinimumWidth = 160;
    private const int MinimumHeight = 100;
    private static readonly double[] DefaultCycleFractions = { 0.5, 2.0 / 3.0, 1.0 / 3.0 };

    private static readonly WindowActionId[] CornerThirdCycle =
    {
        WindowActionId.TopLeftThird,
        WindowActionId.TopRightThird,
        WindowActionId.BottomLeftThird,
        WindowActionId.BottomRightThird
    };

    private static readonly WindowActionId[] SixthsCycle =
    {
        WindowActionId.TopLeftSixth,
        WindowActionId.TopCenterSixth,
        WindowActionId.TopRightSixth,
        WindowActionId.BottomLeftSixth,
        WindowActionId.BottomCenterSixth,
        WindowActionId.BottomRightSixth
    };

    private static readonly WindowActionId[] EighthsCycle =
    {
        WindowActionId.TopLeftEighth,
        WindowActionId.TopCenterLeftEighth,
        WindowActionId.TopCenterRightEighth,
        WindowActionId.TopRightEighth,
        WindowActionId.BottomLeftEighth,
        WindowActionId.BottomCenterLeftEighth,
        WindowActionId.BottomCenterRightEighth,
        WindowActionId.BottomRightEighth
    };

    private static readonly WindowActionId[] NinthsCycle =
    {
        WindowActionId.TopLeftNinth,
        WindowActionId.TopCenterNinth,
        WindowActionId.TopRightNinth,
        WindowActionId.MiddleLeftNinth,
        WindowActionId.MiddleCenterNinth,
        WindowActionId.MiddleRightNinth,
        WindowActionId.BottomLeftNinth,
        WindowActionId.BottomCenterNinth,
        WindowActionId.BottomRightNinth
    };

    private static readonly WindowActionId[] TwelfthsCycle =
    {
        WindowActionId.TopLeftTwelfth,
        WindowActionId.TopCenterLeftTwelfth,
        WindowActionId.TopCenterRightTwelfth,
        WindowActionId.TopRightTwelfth,
        WindowActionId.MiddleLeftTwelfth,
        WindowActionId.MiddleCenterLeftTwelfth,
        WindowActionId.MiddleCenterRightTwelfth,
        WindowActionId.MiddleRightTwelfth,
        WindowActionId.BottomLeftTwelfth,
        WindowActionId.BottomCenterLeftTwelfth,
        WindowActionId.BottomCenterRightTwelfth,
        WindowActionId.BottomRightTwelfth
    };

    private static readonly WindowActionId[] SixteenthsCycle =
    {
        WindowActionId.TopLeftSixteenth,
        WindowActionId.TopCenterLeftSixteenth,
        WindowActionId.TopCenterRightSixteenth,
        WindowActionId.TopRightSixteenth,
        WindowActionId.UpperMiddleLeftSixteenth,
        WindowActionId.UpperMiddleCenterLeftSixteenth,
        WindowActionId.UpperMiddleCenterRightSixteenth,
        WindowActionId.UpperMiddleRightSixteenth,
        WindowActionId.LowerMiddleLeftSixteenth,
        WindowActionId.LowerMiddleCenterLeftSixteenth,
        WindowActionId.LowerMiddleCenterRightSixteenth,
        WindowActionId.LowerMiddleRightSixteenth,
        WindowActionId.BottomLeftSixteenth,
        WindowActionId.BottomCenterLeftSixteenth,
        WindowActionId.BottomCenterRightSixteenth,
        WindowActionId.BottomRightSixteenth
    };

    public static Rectangle Calculate(
        WindowActionId action,
        Rectangle current,
        Screen sourceScreen,
        AppSettings settings,
        int repeatedExecutionCount)
    {
        var work = sourceScreen.WorkingArea;
        var target = action switch
        {
            WindowActionId.LeftHalf => HorizontalSide(work, leading: true, CycleFraction(repeatedExecutionCount)),
            WindowActionId.RightHalf => HorizontalSide(work, leading: false, CycleFraction(repeatedExecutionCount)),
            WindowActionId.TopHalf => VerticalSide(work, leading: true, CycleFraction(repeatedExecutionCount)),
            WindowActionId.BottomHalf => VerticalSide(work, leading: false, CycleFraction(repeatedExecutionCount)),
            WindowActionId.CenterHalf => CenterHalf(work, CycleFraction(repeatedExecutionCount)),
            WindowActionId.TopLeft => Corner(work, left: true, top: true, CycleFraction(repeatedExecutionCount)),
            WindowActionId.TopRight => Corner(work, left: false, top: true, CycleFraction(repeatedExecutionCount)),
            WindowActionId.BottomLeft => Corner(work, left: true, top: false, CycleFraction(repeatedExecutionCount)),
            WindowActionId.BottomRight => Corner(work, left: false, top: false, CycleFraction(repeatedExecutionCount)),
            WindowActionId.Maximize => work,
            WindowActionId.AlmostMaximize => Centered(work, 0.9, 0.9),
            WindowActionId.MaximizeHeight => MaximizeHeight(current, work),
            WindowActionId.Center => Center(current, work),
            WindowActionId.Larger => Resize(current, work, settings.ResizeStep, settings.ResizeStep),
            WindowActionId.Smaller => Resize(current, work, -settings.ResizeStep, -settings.ResizeStep),
            WindowActionId.LargerWidth => Resize(current, work, settings.ResizeStep, 0),
            WindowActionId.SmallerWidth => Resize(current, work, -settings.ResizeStep, 0),
            WindowActionId.LargerHeight => Resize(current, work, 0, settings.ResizeStep),
            WindowActionId.SmallerHeight => Resize(current, work, 0, -settings.ResizeStep),
            WindowActionId.PreviousDisplay => MoveToAdjacentDisplay(current, sourceScreen, -1),
            WindowActionId.NextDisplay => MoveToAdjacentDisplay(current, sourceScreen, 1),
            WindowActionId.DisplayOne => MoveToDisplay(current, sourceScreen, 0),
            WindowActionId.DisplayTwo => MoveToDisplay(current, sourceScreen, 1),
            WindowActionId.DisplayThree => MoveToDisplay(current, sourceScreen, 2),
            WindowActionId.DisplayFour => MoveToDisplay(current, sourceScreen, 3),
            WindowActionId.DisplayFive => MoveToDisplay(current, sourceScreen, 4),
            WindowActionId.DisplaySix => MoveToDisplay(current, sourceScreen, 5),
            WindowActionId.DisplaySeven => MoveToDisplay(current, sourceScreen, 6),
            WindowActionId.DisplayEight => MoveToDisplay(current, sourceScreen, 7),
            WindowActionId.DisplayNine => MoveToDisplay(current, sourceScreen, 8),
            WindowActionId.MoveLeft => MoveToEdge(current, work, DockStyle.Left),
            WindowActionId.MoveRight => MoveToEdge(current, work, DockStyle.Right),
            WindowActionId.MoveUp => MoveToEdge(current, work, DockStyle.Top),
            WindowActionId.MoveDown => MoveToEdge(current, work, DockStyle.Bottom),
            WindowActionId.FirstThird => OrientationStrip(work, CycleIndex(0, 3, repeatedExecutionCount), 1, 3),
            WindowActionId.CenterThird => OrientationStrip(work, 1, 1, 3),
            WindowActionId.LastThird => OrientationStrip(work, CycleIndex(2, 3, -repeatedExecutionCount), 1, 3),
            WindowActionId.FirstTwoThirds => OrientationStrip(work, CycleIndex(0, 2, repeatedExecutionCount), 2, 3),
            WindowActionId.CenterTwoThirds => OrientationStrip(work, 0.5, 2, 3),
            WindowActionId.LastTwoThirds => OrientationStrip(work, CycleIndex(1, 2, repeatedExecutionCount), 2, 3),
            WindowActionId.TopVerticalThird => VerticalStrip(work, CycleIndex(0, 3, repeatedExecutionCount), 1, 3),
            WindowActionId.MiddleVerticalThird => VerticalStrip(work, 1, 1, 3),
            WindowActionId.BottomVerticalThird => VerticalStrip(work, CycleIndex(2, 3, -repeatedExecutionCount), 1, 3),
            WindowActionId.TopVerticalTwoThirds => VerticalStrip(work, CycleIndex(0, 2, repeatedExecutionCount), 2, 3),
            WindowActionId.BottomVerticalTwoThirds => VerticalStrip(work, CycleIndex(1, 2, repeatedExecutionCount), 2, 3),
            WindowActionId.FirstFourth => OrientationStrip(work, CycleIndex(0, 4, repeatedExecutionCount), 1, 4),
            WindowActionId.SecondFourth => OrientationStrip(work, CycleIndex(1, 4, repeatedExecutionCount), 1, 4),
            WindowActionId.ThirdFourth => OrientationStrip(work, CycleIndex(2, 4, repeatedExecutionCount), 1, 4),
            WindowActionId.LastFourth => OrientationStrip(work, CycleIndex(3, 4, repeatedExecutionCount), 1, 4),
            WindowActionId.FirstThreeFourths => OrientationStrip(work, CycleIndex(0, 2, repeatedExecutionCount), 3, 4),
            WindowActionId.CenterThreeFourths => OrientationStrip(work, 0.5, 3, 4),
            WindowActionId.LastThreeFourths => OrientationStrip(work, CycleIndex(1, 2, repeatedExecutionCount), 3, 4),
            WindowActionId.TopCenterSixth or WindowActionId.BottomCenterSixth
                => CenterSixth(action, work, repeatedExecutionCount),
            _ => TryGridCycle(action, work, repeatedExecutionCount, out var cycled)
                ? cycled
                : PortionFromDefinition(action, work)
        };

        if (target.IsEmpty)
        {
            return target;
        }

        return ApplyGap(action, target, work, settings.GapSize);
    }

    private static Rectangle CenterSixth(WindowActionId action, Rectangle work, int repeatCount)
    {
        var center = PortionFromDefinition(action, work);
        return CycleIndex(0, 3, repeatCount) switch
        {
            1 => Rectangle.FromLTRB(center.Left, center.Top, work.Right, center.Bottom),
            2 => Rectangle.FromLTRB(work.Left, center.Top, center.Right, center.Bottom),
            _ => center
        };
    }

    private static bool TryGridCycle(WindowActionId action, Rectangle work, int repeatCount, out Rectangle target)
    {
        foreach (var cycle in new[]
                 {
                     CornerThirdCycle,
                     SixthsCycle,
                     EighthsCycle,
                     NinthsCycle,
                     TwelfthsCycle,
                     SixteenthsCycle
                 })
        {
            var index = Array.IndexOf(cycle, action);
            if (index < 0)
            {
                continue;
            }

            var actionForCycle = cycle[CycleIndex(index, cycle.Length, repeatCount)];
            target = PortionFromDefinition(actionForCycle, work);
            return true;
        }

        target = Rectangle.Empty;
        return false;
    }

    private static int CycleIndex(int start, int length, int offset)
    {
        return ((start + offset) % length + length) % length;
    }

    private static double CycleFraction(int repeatedExecutionCount)
    {
        return DefaultCycleFractions[CycleIndex(0, DefaultCycleFractions.Length, repeatedExecutionCount)];
    }

    private static Rectangle PortionFromDefinition(WindowActionId action, Rectangle work)
    {
        if (!WindowActions.ById.TryGetValue(action, out var definition) || definition.Preview is null)
        {
            return Rectangle.Empty;
        }

        return Portion(work, definition.Preview.Value);
    }

    private static Rectangle Portion(Rectangle work, LayoutPreview preview)
    {
        var left = work.Left + (int)Math.Floor(work.Width * preview.X);
        var top = work.Top + (int)Math.Floor(work.Height * preview.Y);
        var right = work.Left + (int)Math.Floor(work.Width * (preview.X + preview.Width));
        var bottom = work.Top + (int)Math.Floor(work.Height * (preview.Y + preview.Height));
        return Rectangle.FromLTRB(left, top, right, bottom);
    }

    private static Rectangle HorizontalSide(Rectangle work, bool leading, double fraction)
    {
        var width = (int)Math.Floor(work.Width * fraction);
        var x = leading ? work.Left : work.Right - width;
        return new Rectangle(x, work.Top, width, work.Height);
    }

    private static Rectangle VerticalSide(Rectangle work, bool leading, double fraction)
    {
        var height = (int)Math.Floor(work.Height * fraction);
        var y = leading ? work.Top : work.Bottom - height;
        return new Rectangle(work.Left, y, work.Width, height);
    }

    private static Rectangle CenterHalf(Rectangle work, double fraction)
    {
        if (work.Width >= work.Height)
        {
            var width = (int)Math.Round(work.Width * fraction);
            return new Rectangle(work.Left + (work.Width - width) / 2, work.Top, width, work.Height);
        }

        var height = (int)Math.Round(work.Height * fraction);
        return new Rectangle(work.Left, work.Top + (work.Height - height) / 2, work.Width, height);
    }

    private static Rectangle Corner(Rectangle work, bool left, bool top, double horizontalFraction)
    {
        var width = (int)Math.Floor(work.Width * horizontalFraction);
        var height = (int)Math.Floor(work.Height * 0.5);
        var x = left ? work.Left : work.Right - width;
        var y = top ? work.Top : work.Bottom - height;
        return new Rectangle(x, y, width, height);
    }

    private static Rectangle Centered(Rectangle work, double widthRatio, double heightRatio)
    {
        var width = (int)Math.Round(work.Width * widthRatio);
        var height = (int)Math.Round(work.Height * heightRatio);
        return new Rectangle(
            work.Left + (work.Width - width) / 2,
            work.Top + (work.Height - height) / 2,
            width,
            height);
    }

    private static Rectangle Center(Rectangle current, Rectangle work)
    {
        var width = Math.Min(current.Width, work.Width);
        var height = Math.Min(current.Height, work.Height);
        return new Rectangle(
            work.Left + (work.Width - width) / 2,
            work.Top + (work.Height - height) / 2,
            width,
            height);
    }

    private static Rectangle MaximizeHeight(Rectangle current, Rectangle work)
    {
        var width = Math.Min(current.Width, work.Width);
        var x = Math.Clamp(current.Left, work.Left, work.Right - width);
        return new Rectangle(x, work.Top, width, work.Height);
    }

    private static Rectangle Resize(Rectangle current, Rectangle work, int widthDelta, int heightDelta)
    {
        var width = Math.Clamp(current.Width + widthDelta, MinimumWidth, work.Width);
        var height = Math.Clamp(current.Height + heightDelta, MinimumHeight, work.Height);
        var x = current.Left - (width - current.Width) / 2;
        var y = current.Top - (height - current.Height) / 2;
        return ClampToWorkArea(new Rectangle(x, y, width, height), work);
    }

    private static Rectangle MoveToEdge(Rectangle current, Rectangle work, DockStyle edge)
    {
        var width = Math.Min(current.Width, work.Width);
        var height = Math.Min(current.Height, work.Height);
        var x = Math.Clamp(current.Left, work.Left, work.Right - width);
        var y = Math.Clamp(current.Top, work.Top, work.Bottom - height);

        switch (edge)
        {
            case DockStyle.Left:
                x = work.Left;
                y = work.Top + (work.Height - height) / 2;
                break;
            case DockStyle.Right:
                x = work.Right - width;
                y = work.Top + (work.Height - height) / 2;
                break;
            case DockStyle.Top:
                x = work.Left + (work.Width - width) / 2;
                y = work.Top;
                break;
            case DockStyle.Bottom:
                x = work.Left + (work.Width - width) / 2;
                y = work.Bottom - height;
                break;
        }

        return new Rectangle(x, y, width, height);
    }

    private static Rectangle OrientationStrip(Rectangle work, double start, int span, int divisions)
    {
        return work.Width >= work.Height
            ? HorizontalStrip(work, start, span, divisions)
            : VerticalStrip(work, start, span, divisions);
    }

    private static Rectangle HorizontalStrip(Rectangle work, double start, int span, int divisions)
    {
        var unit = work.Width / (double)divisions;
        var x = start == 0.5
            ? work.Left + (int)Math.Round((work.Width - unit * span) / 2)
            : work.Left + (int)Math.Floor(unit * start);
        var width = (int)Math.Floor(unit * span);
        return new Rectangle(x, work.Top, width, work.Height);
    }

    private static Rectangle VerticalStrip(Rectangle work, double start, int span, int divisions)
    {
        var unit = work.Height / (double)divisions;
        var y = start == 0.5
            ? work.Top + (int)Math.Round((work.Height - unit * span) / 2)
            : work.Top + (int)Math.Floor(unit * start);
        var height = (int)Math.Floor(unit * span);
        return new Rectangle(work.Left, y, work.Width, height);
    }

    private static Rectangle MoveToAdjacentDisplay(Rectangle current, Screen sourceScreen, int direction)
    {
        var screens = OrderedScreens();
        if (screens.Length < 2)
        {
            return Rectangle.Empty;
        }

        var currentIndex = Array.FindIndex(screens, screen => screen.DeviceName == sourceScreen.DeviceName);
        if (currentIndex < 0)
        {
            currentIndex = 0;
        }

        var next = (currentIndex + direction + screens.Length) % screens.Length;
        return MoveToDisplay(current, sourceScreen.WorkingArea, screens[next].WorkingArea);
    }

    private static Rectangle MoveToDisplay(Rectangle current, Screen sourceScreen, int displayIndex)
    {
        var screens = OrderedScreens();
        if (displayIndex < 0 || displayIndex >= screens.Length)
        {
            return Rectangle.Empty;
        }

        return MoveToDisplay(current, sourceScreen.WorkingArea, screens[displayIndex].WorkingArea);
    }

    private static Rectangle MoveToDisplay(Rectangle current, Rectangle sourceWork, Rectangle targetWork)
    {
        var relativeX = (current.Left - sourceWork.Left) / (double)Math.Max(1, sourceWork.Width);
        var relativeY = (current.Top - sourceWork.Top) / (double)Math.Max(1, sourceWork.Height);
        var relativeWidth = current.Width / (double)Math.Max(1, sourceWork.Width);
        var relativeHeight = current.Height / (double)Math.Max(1, sourceWork.Height);

        var width = Math.Clamp((int)Math.Round(targetWork.Width * relativeWidth), MinimumWidth, targetWork.Width);
        var height = Math.Clamp((int)Math.Round(targetWork.Height * relativeHeight), MinimumHeight, targetWork.Height);
        var x = targetWork.Left + (int)Math.Round(targetWork.Width * relativeX);
        var y = targetWork.Top + (int)Math.Round(targetWork.Height * relativeY);

        return ClampToWorkArea(new Rectangle(x, y, width, height), targetWork);
    }

    private static Rectangle ApplyGap(WindowActionId action, Rectangle target, Rectangle work, int gap)
    {
        if (gap <= 0 || NoGap(action))
        {
            return target;
        }

        var left = target.Left + gap;
        var top = target.Top + gap;
        var right = target.Right - gap;
        var bottom = target.Bottom - gap;

        if (action == WindowActionId.MaximizeHeight)
        {
            left = target.Left;
            right = target.Right;
        }

        if (right - left < MinimumWidth || bottom - top < MinimumHeight)
        {
            return target;
        }

        return ClampToWorkArea(Rectangle.FromLTRB(left, top, right, bottom), work);
    }

    private static bool NoGap(WindowActionId action)
    {
        return action is WindowActionId.Center
            or WindowActionId.Restore
            or WindowActionId.Larger
            or WindowActionId.Smaller
            or WindowActionId.LargerWidth
            or WindowActionId.SmallerWidth
            or WindowActionId.LargerHeight
            or WindowActionId.SmallerHeight
            or WindowActionId.PreviousDisplay
            or WindowActionId.NextDisplay
            or WindowActionId.DisplayOne
            or WindowActionId.DisplayTwo
            or WindowActionId.DisplayThree
            or WindowActionId.DisplayFour
            or WindowActionId.DisplayFive
            or WindowActionId.DisplaySix
            or WindowActionId.DisplaySeven
            or WindowActionId.DisplayEight
            or WindowActionId.DisplayNine
            or WindowActionId.MoveLeft
            or WindowActionId.MoveRight
            or WindowActionId.MoveUp
            or WindowActionId.MoveDown;
    }

    private static Rectangle ClampToWorkArea(Rectangle rect, Rectangle work)
    {
        var width = Math.Min(rect.Width, work.Width);
        var height = Math.Min(rect.Height, work.Height);
        var x = Math.Clamp(rect.Left, work.Left, work.Right - width);
        var y = Math.Clamp(rect.Top, work.Top, work.Bottom - height);
        return new Rectangle(x, y, width, height);
    }

    private static Screen[] OrderedScreens()
    {
        return Screen.AllScreens
            .OrderBy(screen => screen.Bounds.Left)
            .ThenBy(screen => screen.Bounds.Top)
            .ToArray();
    }
}
