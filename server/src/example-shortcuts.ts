import { ShortcutAction } from "@trapezoid/common/shortcuts";

export const exampleShortcuts: Array<ShortcutAction> = [
  {
    shortcut: {
      CTRL: true,
      ALT: true,
      SHIFT: false,
      META: false,
      KEY: "UP ARROW",
    },
    action: "fullScreen",
  },
  {
    shortcut: {
      CTRL: true,
      ALT: true,
      SHIFT: false,
      META: false,
      KEY: "LEFT ARROW",
    },
    action: "dynamicLeftHalf",
  },
  {
    shortcut: {
      CTRL: true,
      ALT: true,
      SHIFT: false,
      META: false,
      KEY: "RIGHT ARROW",
    },
    action: "dynamicRightHalf",
  },
  {
    shortcut: {
      CTRL: true,
      ALT: true,
      SHIFT: false,
      META: true,
      KEY: "8",
    },
    action: "topLeftSixth",
  },
  {
    shortcut: {
      CTRL: true,
      ALT: true,
      SHIFT: false,
      META: true,
      KEY: "9",
    },
    action: "topMiddleSixth",
  },
  {
    shortcut: {
      CTRL: true,
      ALT: true,
      SHIFT: false,
      META: true,
      KEY: "0",
    },
    action: "topRightSixth",
  },
  {
    shortcut: {
      CTRL: true,
      ALT: true,
      SHIFT: false,
      META: true,
      KEY: "I",
    },
    action: "bottomLeftSixth",
  },
  {
    shortcut: {
      CTRL: true,
      ALT: true,
      SHIFT: false,
      META: true,
      KEY: "O",
    },
    action: "bottomMiddleSixth",
  },
  {
    shortcut: {
      CTRL: true,
      ALT: true,
      SHIFT: false,
      META: true,
      KEY: "P",
    },
    action: "bottomRightSixth",
  },
];
