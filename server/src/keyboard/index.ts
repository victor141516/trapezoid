import {
  GlobalKeyboardListener,
  type IGlobalKeyListener,
} from "node-global-key-listener";
import { type getConfiguredShortcuts as shortcutsGetter } from "../settings";
import { type getShortcutActions as actionsGetter } from "../window-management";

const handler: IGlobalKeyListener = (key, down) => {
  if (key.state === "UP") return;

  const configuredShortcuts = getConfiguredShortcuts!();
  for (const { action, shortcut } of configuredShortcuts) {
    const matches =
      Boolean(down["LEFT ALT"] || down["RIGHT ALT"]) === shortcut.ALT &&
      Boolean(down["LEFT CTRL"] || down["RIGHT CTRL"]) === shortcut.CTRL &&
      Boolean(down["LEFT META"] || down["RIGHT META"]) === shortcut.META &&
      Boolean(down["LEFT SHIFT"] || down["RIGHT SHIFT"]) === shortcut.SHIFT &&
      key.name === shortcut.KEY;

    if (matches) {
      const shortcutActions = getShortcutActions!();
      shortcutActions[action]();
      break;
    }
  }
};

const hotkeys = new GlobalKeyboardListener();
let isInitialized = false;

export const listen = () => {
  if (isInitialized) {
    return;
  }
  if (!getShortcutActions) {
    throw new Error("Actions getter not set");
  }
  if (!getConfiguredShortcuts) {
    throw new Error("Shortcuts getter not set");
  }
  isInitialized = true;
  hotkeys.addListener(handler);
};

export const stop = () => {
  isInitialized = false;
  hotkeys.removeListener(handler);
};

let getShortcutActions: typeof actionsGetter | null = null;
export const setActionsGetter = (actionsGetter: typeof getShortcutActions) => {
  getShortcutActions = actionsGetter;
};

let getConfiguredShortcuts: typeof shortcutsGetter | null = null;
export const setShortcutsGetter = (
  shortcutsGetter: typeof getConfiguredShortcuts
) => {
  getConfiguredShortcuts = shortcutsGetter;
};
