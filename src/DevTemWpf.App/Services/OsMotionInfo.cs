using System.Runtime.InteropServices;

namespace DevTemWpf.App.Services;

/// <summary>
/// Reads the OS "animation effects" setting (Accessibility > Visual effects).
/// Fail-open: any interop failure assumes animations are enabled — the user
/// can always force Reduced/Off in Settings.
/// </summary>
internal static class OsMotionInfo
{
    private const uint SPI_GETCLIENTAREAANIMATION = 0x1042;

    internal static bool IsAnimationEnabled()
    {
        try
        {
            var enabled = 0;
            if (SystemParametersInfo(SPI_GETCLIENTAREAANIMATION, 0, ref enabled, 0))
            {
                return enabled != 0;
            }
        }
        catch
        {
            // Fail open (see summary).
        }

        return true;
    }

    [DllImport("user32.dll", SetLastError = true)]
    [return: MarshalAs(UnmanagedType.Bool)]
    private static extern bool SystemParametersInfo(uint uiAction, uint uiParam, ref int pvParam, uint fWinIni);
}
