using System;
using System.Reflection;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.EventSystems;

static class Program
{
    static void Check(bool condition, string message)
    {
        if (!condition) throw new Exception(message);
        Console.WriteLine("PASS: " + message);
    }
    static void Set(object target, string field, object value) => target.GetType()
        .GetField(field, BindingFlags.NonPublic | BindingFlags.Instance).SetValue(target, value);
    static void Call(object target, string method) => target.GetType()
        .GetMethod(method, BindingFlags.NonPublic | BindingFlags.Instance).Invoke(target, null);
    static bool Near(float a, float b) => Math.Abs(a - b) < 0.0001f;
    static void Main()
    {
        VerifyEdgeReadiness();
        var viewport = new Rect(100, 50, 800, 600);
        Check(ExpeditionMapCameraController.GetEdgeDirection(new Vector2(101, 350), viewport, 24).x == -1,
            "left edge uses camera viewport offset");
        Check(ExpeditionMapCameraController.GetEdgeDirection(new Vector2(899, 350), viewport, 24).x == 1 &&
            ExpeditionMapCameraController.GetEdgeDirection(new Vector2(500, 649), viewport, 24).y == 1 &&
            ExpeditionMapCameraController.GetEdgeDirection(new Vector2(500, 51), viewport, 24).y == -1,
            "right, top and bottom edges");
        Check(ExpeditionMapCameraController.GetEdgeDirection(new Vector2(99, 350), viewport, 24) == Vector2.zero &&
            ExpeditionMapCameraController.GetEdgeDirection(new Vector2(500, 350), viewport, 24) == Vector2.zero,
            "outside and center do not scroll");
        var corner = ExpeditionMapCameraController.GetEdgeDirection(new Vector2(100, 50), viewport, 24);
        Check(Near(corner.magnitude, 1), "diagonal edge speed is normalized");
        Check(ExpeditionMapCameraController.GetEdgeDirection(new Vector2(500, 350), viewport, 10000) == Vector2.zero,
            "oversized margin does not bias viewport center");
        var clamp = ExpeditionMapCameraController.ClampToMap(new Vector2(100, -100), new Rect(-20, -10, 40, 20), 5, 2);
        Check(clamp == new Vector2(10, -5), "map bounds include full visible camera extent");
        clamp = ExpeditionMapCameraController.ClampToMap(new Vector2(100, 100), new Rect(-2, -1, 4, 2), 5, 2);
        Check(clamp == Vector2.zero, "map smaller than view is centered");

        var camera = new Camera { pixelRect = viewport };
        camera.transform.position = new Vector3(0, 0, -10);
        var controller = new ExpeditionMapCameraController();
        Set(controller, "targetCamera", camera);
        Call(controller, "OnAwake");
        Keyboard.current = new Keyboard(); Mouse.current = new Mouse();
        Mouse.current.position.value = new Vector2(500, 350);
        Time.unscaledDeltaTime = 0.5f;
        Keyboard.current.wKey.isPressed = true;
        Call(controller, "Update");
        Check(camera.transform.position.y == 4 && camera.transform.position.z == -10, "W moves north at units/second and preserves depth");
        Keyboard.current.dKey.isPressed = true;
        Vector3 before = camera.transform.position;
        Call(controller, "Update");
        Check(Near(((Vector2)camera.transform.position - (Vector2)before).magnitude, 4), "keyboard diagonal speed is normalized");
        Keyboard.current = new Keyboard(); Keyboard.current.leftArrowKey.isPressed = true;
        Mouse.current.position.value = new Vector2(900, 350);
        before = camera.transform.position; Call(controller, "Update");
        Check(Near(camera.transform.position.x, before.x - 4), "arrow keys work and override opposite mouse edge");
        Set(controller, "useArrowKeys", false);
        before = camera.transform.position; Call(controller, "Update");
        Check(Near(camera.transform.position.x, before.x + 4), "keyboard bindings can be disabled independently");
        before = camera.transform.position;
        Mouse.current.leftButton.isPressed = true; Call(controller, "Update");
        Check(camera.transform.position.Equals(before), "mouse press/drag suppresses edge movement");
        Mouse.current.leftButton.isPressed = false;
        EventSystem.current = new EventSystem { pointerOverUI = true };
        Call(controller, "Update");
        Check(camera.transform.position.Equals(before), "UI hover suppresses edge movement");
        EventSystem.current.pointerOverUI = false;
        EventSystem.current.currentSelectedGameObject = new GameObject { field = new TMPro.TMP_InputField { isFocused = true } };
        Keyboard.current.wKey.isPressed = true; Call(controller, "Update");
        Check(camera.transform.position.Equals(before), "text editing suppresses both movement inputs");
        EventSystem.current = null;
        Application.isFocused = false; Call(controller, "Update");
        Check(camera.transform.position.Equals(before), "focus loss blocks movement");
        Application.isFocused = true;
        controller.SetInputEnabled(false); Call(controller, "Update");
        Check(camera.transform.position.Equals(before), "explicit lock blocks movement");
        controller.SetInputEnabled(true);
        controller.SetMapBackground(new SpriteRenderer { bounds = new Bounds(new Vector3(-2, -1, 0), new Vector3(4, 2, 0)) });
        Call(controller, "Update");
        Check(camera.transform.position.x == 0 && camera.transform.position.y == 0 && camera.transform.position.z == -10,
            "stage background bounds constrain actual camera updates");
        controller.SetMapBackground(null); Keyboard.current = null; Mouse.current = null;
        Call(controller, "Update");
        Check(camera.transform.position.x == 0 && camera.transform.position.y == 0, "missing devices are supported");
        var invalid = new ExpeditionMapCameraController();
        Set(invalid, "targetCamera", new Camera { orthographic = false }); Call(invalid, "OnAwake");
        Check(!invalid.enabled, "perspective camera is rejected");
    }

    static void VerifyEdgeReadiness()
    {
        var camera = new Camera { pixelRect = new Rect(0, 0, 800, 600) };
        camera.transform.position = new Vector3(0, 0, -10);
        var controller = new ExpeditionMapCameraController();
        Set(controller, "targetCamera", camera);
        Call(controller, "OnAwake"); Call(controller, "OnEnabled");
        Mouse.current = new Mouse(); Keyboard.current = new Keyboard();
        Time.unscaledDeltaTime = 0.5f;
        Vector3 before = camera.transform.position;
        for (int i = 0; i < 10; i++) Call(controller, "Update");
        Check(camera.transform.position.Equals(before), "initial mouse (0,0) cannot start edge scrolling");
        Mouse.current.position.value = new Vector2(-1, 300); Call(controller, "Update");
        Mouse.current.position.value = new Vector2(800, 300); Call(controller, "Update");
        Check(camera.transform.position.Equals(before), "outside viewport does not arm edge scrolling");
        Keyboard.current.wKey.isPressed = true; Call(controller, "Update");
        Check(camera.transform.position.y == 4, "keyboard works while edge scrolling awaits entry");
        Keyboard.current.wKey.isPressed = false;
        Mouse.current.position.value = new Vector2(400, 300); Call(controller, "Update");
        before = camera.transform.position;
        Mouse.current.position.value = new Vector2(800, 300); Call(controller, "Update");
        Check(camera.transform.position.x == before.x + 4, "interior entry arms subsequent edge scrolling");
        before = camera.transform.position;
        Application.SetFocus(false); Application.SetFocus(true); // No Update while unfocused.
        Call(controller, "Update");
        Check(camera.transform.position.Equals(before), "focus return requires fresh interior entry even without an unfocused frame");
        Mouse.current.position.value = new Vector2(400, 300); Call(controller, "Update");
        controller.SetInputEnabled(false); controller.SetInputEnabled(true);
        Mouse.current.position.value = new Vector2(800, 300); Call(controller, "Update");
        Check(camera.transform.position.Equals(before), "unlocking input resets edge readiness");
        Mouse.current.position.value = new Vector2(400, 300); Call(controller, "Update");
        controller.SetInputEnabled(true);
        Mouse.current.position.value = new Vector2(800, 300); Call(controller, "Update");
        Check(camera.transform.position.x == before.x + 4, "redundant input enable preserves readiness");
        before = camera.transform.position;
        Call(controller, "OnDisabled"); Call(controller, "OnEnabled"); Call(controller, "Update");
        Check(camera.transform.position.Equals(before), "reopening map component requires fresh interior entry");
        Mouse.current.position.value = new Vector2(400, 300); Call(controller, "Update");
        before = camera.transform.position;
        Mouse.current.position.value = new Vector2(0, 0); Call(controller, "Update");
        Check(camera.transform.position.x < before.x && camera.transform.position.y < before.y,
            "actual corner remains usable after readiness is established");
        Call(controller, "OnDisabled");
        Mouse.current = null; Keyboard.current = null;
    }
}
