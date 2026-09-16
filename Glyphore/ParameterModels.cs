namespace Glyphore;

internal readonly record struct ParamOption(string Es, string En);

internal readonly record struct ParamDesc(
    string Label,
    string Key,
    double Min,
    double Max,
    double Default,
    int Digits = 2,
    string Help = "",
    ParamOption[]? Options = null);
