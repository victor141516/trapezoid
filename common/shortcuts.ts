import { IGlobalKey } from "node-global-key-listener";
import { actionValidator } from "./window";
import { z } from "zod";

export const shortcutValidator = z.object({
  CTRL: z.boolean(),
  ALT: z.boolean(),
  SHIFT: z.boolean(),
  META: z.boolean(),
  KEY: z.string(),
});

export type Shortcut = z.output<typeof shortcutValidator> & { KEY: IGlobalKey };

export const shortcutActionValidator = z.object({
  shortcut: shortcutValidator,
  action: actionValidator,
});

export type ShortcutAction = z.output<typeof shortcutActionValidator>;
