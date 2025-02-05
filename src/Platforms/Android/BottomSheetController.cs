﻿using Android.Views;
using Microsoft.Maui.Platform;
using Google.Android.Material.BottomSheet;
using Android.Widget;
using AndroidX.CoordinatorLayout.Widget;
using AndroidX.DrawerLayout.Widget;
using Android.Content.Res;

namespace The49.Maui.BottomSheet;

public class BottomSheetController : IBottomSheetController
{
    BottomSheetBehavior _behavior = new BottomSheetBehavior
    {
        State = BottomSheetBehavior.StateHidden,
    };
    public BottomSheetBehavior Behavior => _behavior;

    CoordinatorLayout _coordinatorLayout;
    ViewGroup _frame;

    IMauiContext _windowMauiContext { get; }
    BottomSheet _sheet { get; }

    public BottomSheetController(IMauiContext windowMauiContext, BottomSheet sheet)
    {
        _windowMauiContext = windowMauiContext;
        _sheet = sheet;
    }

    public void Dismiss()
    {
        Behavior.Hideable = true;
        Behavior.State = BottomSheetBehavior.StateHidden;
    }

    void Dispose()
    {
        var navigationRootManager = _windowMauiContext.Services.GetRequiredService<NavigationRootManager>();
        if (navigationRootManager.RootView is CoordinatorLayout coordinatorLayout && _coordinatorLayout == coordinatorLayout)
        {
            _frame.RemoveFromParent();
        }
        else
        {
            _coordinatorLayout.RemoveFromParent();
        }
        _frame = null;
        _coordinatorLayout = null;
    }

    public void Layout()
    {
        // TODO: verify that, maybe handle statusbar and navigationbar
        var maxSheetHeight = _sheet.Window.Height;
        BottomSheetManager.LayoutDetents(_behavior, _frame, _sheet, maxSheetHeight);
    }

    public void UpdateBackground()
    {
        Paint paint = _sheet.BackgroundBrush;
        if (_frame != null && paint != null)
        {
            _frame.BackgroundTintList = ColorStateList.ValueOf(paint.ToColor().ToPlatform());
        }
    }

    void SetupCoordinatorLayout()
    {
        var navigationRootManager = _windowMauiContext.Services.GetRequiredService<NavigationRootManager>();

        if (navigationRootManager.RootView is ContainerView cv && cv.MainView is DrawerLayout drawerLayout)
        {
            _coordinatorLayout = new CoordinatorLayout(_windowMauiContext.Context);
            _coordinatorLayout.SetFitsSystemWindows(true);

            drawerLayout.AddView(_coordinatorLayout, new ViewGroup.LayoutParams(ViewGroup.LayoutParams.MatchParent, ViewGroup.LayoutParams.MatchParent));
        }
        else if (navigationRootManager.RootView is CoordinatorLayout coordinatorLayout)
        {
            _coordinatorLayout = coordinatorLayout;
        }
        else
        {
            throw new Exception("Unrecognized RootView");
        }

        _frame = new FrameLayout(new ContextThemeWrapper(_windowMauiContext.Context, Resource.Style.Widget_Material3_BottomSheet_Modal), null, 0);

        _behavior = new BottomSheetBehavior(new ContextThemeWrapper(_windowMauiContext.Context, Resource.Style.Widget_Material3_BottomSheet_Modal), null);
        _behavior.State = BottomSheetBehavior.StateHidden;

        _coordinatorLayout.AddView(_frame, new CoordinatorLayout.LayoutParams(ViewGroup.LayoutParams.MatchParent, ViewGroup.LayoutParams.WrapContent)
        {
            Gravity = (int)(GravityFlags.CenterHorizontal | GravityFlags.Top),
            Behavior = _behavior,
        });
    }

    public void Show()
    {
        SetupCoordinatorLayout();

        var layout = BottomSheetManager.CreateLayout(_sheet, _windowMauiContext);

        layout.LayoutChange += (s, e) => Layout();

        _frame.AddView(layout);

        var callback = new BottomSheetCallback(_sheet);
        callback.StateChanged += Callback_StateChanged;
        Behavior.AddBottomSheetCallback(callback);
        _sheet.Dispatcher.Dispatch(() =>
        {
            UpdateBackground();
            Layout();

            Behavior.State = Behavior.SkipCollapsed ? BottomSheetBehavior.StateExpanded : BottomSheetBehavior.StateCollapsed;

            _behavior.Hideable = _sheet.Cancelable;
        });
    }

    private void Callback_StateChanged(object sender, EventArgs e)
    {
        if (Behavior.State == BottomSheetBehavior.StateHidden)
        {
            _sheet.NotifyDismissed();
            Dispose();
        }
    }
}
