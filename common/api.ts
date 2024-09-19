import {
  Offsets,
  offsetsValidator,
  Action,
  availableActionNames,
} from "./window";
import { ShortcutAction, shortcutActionValidator } from "./shortcuts";
import { z } from "zod";

export const METHOD_NAMES = {
  INITIALIZE: "initialize",
  STOP: "stop",
  OFFSETS: "offsets",
  SHORTCUTS_ACTIONS: "shortcuts-actions",
  AVAILABLE_ACTIONS: "available-actions",
} as const;

const responseBodyBuilder = <T>(responseDataValidator: z.ZodType<T>) => {
  return z
    .object({
      ok: z.literal(true),
      data: responseDataValidator,
    })
    .or(
      z.object({
        ok: z.literal(false),
        error: z.string(),
      })
    );
};

export type ApiPost<
  Method extends keyof typeof METHOD_NAMES,
  Body,
  ResponseData
> = {
  verb: "post";
  url: `/api/${(typeof METHOD_NAMES)[Method]}`;
  body: z.Schema<Body>;
  response: ReturnType<typeof responseBodyBuilder<ResponseData>>;
};

export type ApiGet<Method extends keyof typeof METHOD_NAMES, ResponseData> = {
  verb: "get";
  url: `/api/${(typeof METHOD_NAMES)[Method]}`;
  body: z.ZodUndefined;
  response: ReturnType<typeof responseBodyBuilder<ResponseData>>;
};

const initialize: ApiPost<"INITIALIZE", undefined, undefined> = {
  verb: "post",
  url: "/api/initialize",
  body: z.undefined(),
  response: responseBodyBuilder(z.undefined()),
};

const stop: ApiPost<"STOP", undefined, undefined> = {
  verb: "post",
  url: "/api/stop",
  body: z.undefined(),
  response: responseBodyBuilder(z.undefined()),
};

const setOffsets: ApiPost<"OFFSETS", Offsets, undefined> = {
  verb: "post",
  url: "/api/offsets",
  body: offsetsValidator,
  response: responseBodyBuilder(z.undefined()),
};

const setShortcutsActions: ApiPost<
  "SHORTCUTS_ACTIONS",
  ShortcutAction[],
  undefined
> = {
  verb: "post",
  url: "/api/shortcuts-actions",
  body: z.array(shortcutActionValidator),
  response: responseBodyBuilder(z.undefined()),
};

const getOffsets: ApiGet<"OFFSETS", Offsets> = {
  verb: "get",
  url: "/api/offsets",
  body: z.undefined(),
  response: responseBodyBuilder(offsetsValidator),
};

const getShortcutsActions: ApiGet<"SHORTCUTS_ACTIONS", ShortcutAction[]> = {
  verb: "get",
  url: "/api/shortcuts-actions",
  body: z.undefined(),
  response: responseBodyBuilder(z.array(shortcutActionValidator)),
};

const getAvailableActions: ApiGet<"AVAILABLE_ACTIONS", readonly Action[]> = {
  verb: "get",
  url: "/api/available-actions",
  body: z.undefined(),
  response: responseBodyBuilder(z.array(z.enum(availableActionNames))),
};

export const endpoints = {
  POST: {
    initialize,
    stop,
    offsets: setOffsets,
    shortcutsActions: setShortcutsActions,
  },
  GET: {
    offsets: getOffsets,
    shortcutsActions: getShortcutsActions,
    availableActions: getAvailableActions,
  },
};
