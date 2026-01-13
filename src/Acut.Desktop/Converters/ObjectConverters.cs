using Avalonia.Data.Converters;

namespace Acut.Desktop;

public static class ObjectConverters
{
    public static readonly IValueConverter IsNull =
        new FuncValueConverter<object?, bool>(x => x is null);

    public static readonly IValueConverter IsNotNull =
        new FuncValueConverter<object?, bool>(x => x is not null);

    public static readonly IValueConverter PlayPauseIcon =
        new FuncValueConverter<bool, string>(isPlaying => isPlaying ? "⏸" : "▶");
}
