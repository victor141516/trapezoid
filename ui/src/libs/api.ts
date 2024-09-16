import { Shortcut } from "../components/ShortcutRecorder.vue";

let port: number = 8000;
export const setApiPort = (newPort: number) => {
  port = newPort;
};

export type AvailableActions =
  | "fullScreen"
  | "leftOneThird"
  | "leftTwoThirds"
  | "rightOneThird"
  | "rightTwoThirds"
  | "leftHalf"
  | "dynamicLeftHalf"
  | "dynamicRightHalf"
  | "rightHalf"
  | "topLeftSixth"
  | "topMiddleSixth"
  | "topRightSixth"
  | "bottomLeftSixth"
  | "bottomMiddleSixth"
  | "bottomRightSixth";

const METHOD_NAMES = {
  INITIALIZE: "initialize",
  STOP: "stop",
  OFFSETS: "offsets",
  SHORTCUTS_ACTIONS: "shortcuts-actions",
  AVAILABLE_ACTIONS: "available-actions",
} as const;

interface Offsets {
  top: number;
  left: number;
  bottom: number;
  right: number;
}

type Method = keyof typeof METHOD_NAMES;
type MethodGet = Extract<
  Method,
  "SHORTCUTS_ACTIONS" | "AVAILABLE_ACTIONS" | "OFFSETS"
>;
type MethodPost = Extract<
  Method,
  "INITIALIZE" | "STOP" | "OFFSETS" | "SHORTCUTS_ACTIONS"
>;

interface ShortcutAction {
  shortcut: Shortcut;
  action: AvailableActions;
}

const apiCallGet = (method: MethodGet) => {
  return fetch(`http://localhost:${port}/api/${METHOD_NAMES[method]}`, {
    method: "GET",
  })
    .then((r) => r.json() as Promise<{ ok: boolean }>)
    .then((r) => {
      if (r.ok) {
        return r;
      } else {
        throw new Error(JSON.stringify(r));
      }
    });
};

const apiCallPost = (method: MethodPost, body?: any) => {
  return fetch(`http://localhost:${port}/api/${METHOD_NAMES[method]}`, {
    method: "POST",
    body: body ? JSON.stringify(body) : undefined,
    headers: {
      "Content-Type": "application/json",
    },
  })
    .then((r) => r.json() as Promise<{ ok: boolean }>)
    .then((r) => {
      if (r.ok) {
        return r;
      } else {
        throw new Error(JSON.stringify(r));
      }
    });
};

export const getAvailableActions = () => {
  return apiCallGet("AVAILABLE_ACTIONS").then(({ actions }: any) => {
    return actions as AvailableActions[];
  });
};

export const resume = () => {
  return apiCallPost("INITIALIZE");
};

export const pause = () => {
  return apiCallPost("STOP");
};

export const setShortcutsActions = (shortcutsActions: ShortcutAction[]) => {
  return apiCallPost("SHORTCUTS_ACTIONS", shortcutsActions);
};

export const getShortcutsActions = () => {
  return apiCallGet("SHORTCUTS_ACTIONS").then(({ shortcuts }: any) => {
    return shortcuts as ShortcutAction[];
  });
};

export const getOffsets = () => {
  return apiCallGet("OFFSETS").then(({ offsets }: any) => {
    return offsets as Offsets;
  });
};

export const setOffsets = (offsets: Offsets) => {
  return apiCallPost("OFFSETS", offsets);
};
