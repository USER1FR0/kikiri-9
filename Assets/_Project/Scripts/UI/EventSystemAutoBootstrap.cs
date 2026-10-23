// Runtime shim that configures the InputSystemUIInputModule when it has no actions
// assigned. This happens when the EventSystem is created from an editor script: the
// New Input System module is added but the InputActionAsset reference stays empty, so
// no UI event ever fires. This builds a minimal UI action map in memory and wires it
// to the module at Awake, which makes every button clickable.

using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.InputSystem.UI;

namespace Kikiri9.UI
{
    [RequireComponent(typeof(InputSystemUIInputModule))]
    public class EventSystemAutoBootstrap : MonoBehaviour
    {
        private InputActionAsset asset;

        private void Awake()
        {
            var module = GetComponent<InputSystemUIInputModule>();
            if (module == null || module.actionsAsset != null) return;

            asset = ScriptableObject.CreateInstance<InputActionAsset>();
            asset.name = "UI_DefaultActions_Runtime";
            var map = asset.AddActionMap("UI");

            var point = map.AddAction("Point", InputActionType.PassThrough);
            point.AddBinding("<Mouse>/position");
            point.AddBinding("<Pen>/position");
            point.AddBinding("<Touchscreen>/primaryTouch/position");

            var click = map.AddAction("Click", InputActionType.PassThrough);
            click.AddBinding("<Mouse>/leftButton");
            click.AddBinding("<Pen>/tip");
            click.AddBinding("<Touchscreen>/primaryTouch/press");

            var middle = map.AddAction("MiddleClick", InputActionType.PassThrough);
            middle.AddBinding("<Mouse>/middleButton");

            var right = map.AddAction("RightClick", InputActionType.PassThrough);
            right.AddBinding("<Mouse>/rightButton");

            var scroll = map.AddAction("Scroll", InputActionType.PassThrough);
            scroll.AddBinding("<Mouse>/scroll");

            var navigate = map.AddAction("Navigate", InputActionType.PassThrough);
            navigate.AddCompositeBinding("2DVector")
                .With("Up", "<Keyboard>/upArrow")
                .With("Down", "<Keyboard>/downArrow")
                .With("Left", "<Keyboard>/leftArrow")
                .With("Right", "<Keyboard>/rightArrow");
            navigate.AddBinding("<Gamepad>/dpad");
            navigate.AddBinding("<Gamepad>/leftStick");

            var submit = map.AddAction("Submit", InputActionType.Button);
            submit.AddBinding("<Keyboard>/enter");
            submit.AddBinding("<Gamepad>/buttonSouth");

            var cancel = map.AddAction("Cancel", InputActionType.Button);
            cancel.AddBinding("<Keyboard>/escape");
            cancel.AddBinding("<Gamepad>/buttonEast");

            module.actionsAsset = asset;
            module.point = InputActionReference.Create(point);
            module.leftClick = InputActionReference.Create(click);
            module.middleClick = InputActionReference.Create(middle);
            module.rightClick = InputActionReference.Create(right);
            module.scrollWheel = InputActionReference.Create(scroll);
            module.move = InputActionReference.Create(navigate);
            module.submit = InputActionReference.Create(submit);
            module.cancel = InputActionReference.Create(cancel);

            map.Enable();
        }
    }
}
