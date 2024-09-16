import { ShortcutAction } from "../keyboard/types";

let configuredShortcuts: Array<ShortcutAction> = [];

export const getConfiguredShortcuts = () => configuredShortcuts;
export const setConfiguredShortcuts = (shortcuts: Array<ShortcutAction>) => {
  configuredShortcuts = shortcuts;
};
