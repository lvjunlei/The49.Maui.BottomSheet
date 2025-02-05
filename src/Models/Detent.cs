﻿using Maui.BindableProperty.Generator.Core;

namespace The49.Maui.BottomSheet;

public abstract partial class Detent : BindableObject
{
    [AutoBindable(DefaultValue = "true")]
    readonly bool isEnabled = true;
    public abstract double GetHeight(BottomSheet page, double maxSheetHeight);
}
