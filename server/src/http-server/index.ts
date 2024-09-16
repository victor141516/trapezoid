import express from "express";
import cors from "cors";
import { z } from "zod";
import { findFreePorts } from "find-free-ports";

import * as settings from "../settings";
import * as keyboard from "../keyboard";
import { shortcutActionValidator } from "../keyboard/types";
import * as windowManagement from "../window-management";
import { exampleShortcuts } from "../example-shortcuts";
import { offsetsValidator } from "../window-management";

const app = express();
app.use(cors());
app.use(express.json());

const METHOD_NAMES = {
  INITIALIZE: "initialize",
  STOP: "stop",
  OFFSETS: "offsets",
  SHORTCUTS_ACTIONS: "shortcuts-actions",
  AVAILABLE_ACTIONS: "available-actions",
} as const;

app.post(`/api/${METHOD_NAMES.INITIALIZE}`, (req, res) => {
  settings.setOffsets({
    top: -1,
    left: -5,
    bottom: -42,
    right: 7,
  });
  settings.setConfiguredShortcuts(exampleShortcuts);

  windowManagement.setOffsetsGetter(settings.getOffsets);
  keyboard.setShortcutsGetter(settings.getConfiguredShortcuts);
  keyboard.setActionsGetter(windowManagement.getShortcutActions);

  keyboard.listen();

  res.json({ ok: true });
});

app.post(`/api/${METHOD_NAMES.STOP}`, (req, res) => {
  keyboard.stop();
  res.json({ ok: true });
});

app.post(`/api/${METHOD_NAMES.OFFSETS}`, (req, res) => {
  const body = offsetsValidator.safeParse(req.body);
  if (body.error) {
    res.status(400).json({ ok: false, error: body.error });
    return;
  }
  settings.setOffsets(body.data);
  res.json({ ok: true });
});

app.post(`/api/${METHOD_NAMES.SHORTCUTS_ACTIONS}`, (req, res) => {
  const body = z.array(shortcutActionValidator).safeParse(req.body);
  if (body.error) {
    res.status(400).json({ ok: false, error: body.error });
    return;
  }
  settings.setConfiguredShortcuts(body.data);
  res.json({ ok: true });
});

app.get(`/api/${METHOD_NAMES.SHORTCUTS_ACTIONS}`, (req, res) => {
  const shortcuts = settings.getConfiguredShortcuts();
  res.json({ ok: true, shortcuts });
});

app.get(`/api/${METHOD_NAMES.AVAILABLE_ACTIONS}`, (req, res) => {
  res.json({ ok: true, actions: windowManagement.availableActions });
});

app.get(`/api/${METHOD_NAMES.OFFSETS}`, (req, res) => {
  res.json({ ok: true, offsets: settings.getOffsets() });
});

export const port = findFreePorts(1, { startPort: 8000 }).then(
  (ports) => ports[0]
);
export const start = async () => {
  app.listen(await port, "127.0.0.1", async () => {
    console.log("Trapezoid server started on port:", await port);
  });
};
