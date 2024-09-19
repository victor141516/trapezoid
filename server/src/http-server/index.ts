import express, { RequestHandler } from "express";
import cors from "cors";
import { z } from "zod";
import { findFreePorts } from "find-free-ports";
import {
  ApiGet,
  ApiPost,
  endpoints,
  METHOD_NAMES,
} from "@trapezoid/common/api";

import * as settings from "../settings";
import * as keyboard from "../keyboard";
import { shortcutActionValidator } from "../keyboard/types";
import * as windowManagement from "../window-management";
import { exampleShortcuts } from "../example-shortcuts";
import {
  availableActionNames,
  offsetsValidator,
} from "@trapezoid/common/window";

const app = express();
app.use(cors());
app.use(express.json());

const useEndpoint = <
  Body,
  ResponseData,
  Method extends keyof typeof METHOD_NAMES
>(
  app: express.Express,
  config: ApiPost<Method, Body, ResponseData> | ApiGet<Method, ResponseData>,
  handler: RequestHandler<unknown, z.input<typeof config.response>, Body>
) => {
  app[config.verb]<typeof config.url, unknown, z.input<typeof config.response>>(
    config.url,
    handler
  );
};

useEndpoint(app, endpoints.POST.initialize, (req, res) => {
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

useEndpoint(app, endpoints.POST.stop, (req, res) => {
  keyboard.stop();
  res.json({ ok: true });
});

useEndpoint(app, endpoints.POST.offsets, (req, res) => {
  const body = offsetsValidator.safeParse(req.body);
  if (body.error) {
    res.status(400).json({ ok: false, error: body.error.toString() });
    return;
  }
  settings.setOffsets(body.data);
  res.json({ ok: true });
});

useEndpoint(app, endpoints.POST.shortcutsActions, (req, res) => {
  const body = z.array(shortcutActionValidator).safeParse(req.body);
  if (body.error) {
    res.status(400).json({ ok: false, error: body.error.toString() });
    return;
  }
  settings.setConfiguredShortcuts(body.data);
  res.json({ ok: true });
});

useEndpoint(app, endpoints.GET.shortcutsActions, (req, res) => {
  const shortcuts = settings.getConfiguredShortcuts();
  res.json({ ok: true, data: shortcuts });
});

useEndpoint(app, endpoints.GET.availableActions, (req, res) => {
  res.json({ ok: true, data: availableActionNames });
});

useEndpoint(app, endpoints.GET.offsets, (req, res) => {
  res.json({ ok: true, data: settings.getOffsets() });
});

export const port = findFreePorts(1, { startPort: 8000 }).then(
  (ports) => ports[0]
);
export const start = async () => {
  app.listen(await port, "127.0.0.1", async () => {
    console.log("Trapezoid server started on port:", await port);
  });
};
