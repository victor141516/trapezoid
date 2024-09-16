import { windowManager, type Window } from "node-window-manager";
import { IRectangle } from "node-window-manager/dist/interfaces";

import { type getOffsets as offsetsGetter } from "../settings";
import { z } from "zod";

let getOffsets: typeof offsetsGetter = () => ({
  top: 0,
  left: 0,
  bottom: 0,
  right: 0,
});
export const setOffsetsGetter = (offsetsGetter: typeof getOffsets) => {
  getOffsets = offsetsGetter;
};

const getMonitorSizes = (window: Window) => {
  const monitor = window.getMonitor();
  const { height, width, x, y } = monitor.getBounds();
  const scaleFactor = monitor.getScaleFactor();
  if (
    height === undefined ||
    width === undefined ||
    x === undefined ||
    y === undefined
  ) {
    throw new Error("Could not get real resolution");
  }
  return {
    height: (height + getOffsets!().bottom) / scaleFactor,
    width: (width + getOffsets!().right) / scaleFactor,
    x: (x + getOffsets!().left) / scaleFactor,
    y: (y + getOffsets!().top) / scaleFactor,
    scaleFactor,
  };
};

const isWindowAtBounds = (window: Window, bounds: IRectangle) => {
  const { height, width, x, y } = window.getBounds();
  if (
    bounds.x === undefined ||
    bounds.y === undefined ||
    bounds.width === undefined ||
    bounds.height === undefined ||
    x === undefined ||
    y === undefined ||
    width === undefined ||
    height === undefined
  ) {
    console.warn("This should not happen");
    return false;
  }

  return (
    Math.abs(bounds.x - x) < 1 &&
    Math.abs(bounds.y - y) < 1 &&
    Math.abs(bounds.width - width) < 1 &&
    Math.abs(bounds.height - height) < 1
  );
};

const fullScreen = (window: Window) => {
  const { height, width, x, y } = getMonitorSizes(window);
  return {
    x,
    y,
    width,
    height,
  };
};

const leftOneThird = (window: Window) => {
  const { height, width, x, y } = getMonitorSizes(window);
  return {
    x,
    y,
    width: width / 3,
    height,
  };
};

const leftTwoThirds = (window: Window) => {
  const { height, width, x, y } = getMonitorSizes(window);
  return {
    x,
    y,
    width: (width / 3) * 2,
    height,
  };
};

const rightOneThird = (window: Window) => {
  const { height, width, x, y } = getMonitorSizes(window);
  const newWidth = width / 3;
  return {
    x: x + newWidth * 2,
    y,
    width: width / 3,
    height,
  };
};

const rightTwoThirds = (window: Window) => {
  const { height, width, x, y } = getMonitorSizes(window);
  const newWidth = width / 3;
  return {
    x: x + newWidth,
    y,
    width: (width / 3) * 2,
    height,
  };
};

const leftHalf = (window: Window) => {
  const { height, width, x, y } = getMonitorSizes(window);
  return {
    x,
    y,
    width: width / 2,
    height,
  };
};

const dynamicLeftHalf = (window: Window) => {
  const leftHalfBounds = leftHalf(window);
  const leftOneThirdBounds = leftOneThird(window);
  const leftTwoThirdsBounds = leftTwoThirds(window);

  if (isWindowAtBounds(window, leftHalfBounds)) {
    return leftTwoThirds(window);
  } else if (isWindowAtBounds(window, leftTwoThirdsBounds)) {
    return leftOneThird(window);
  } else if (isWindowAtBounds(window, leftOneThirdBounds)) {
    return leftHalfBounds;
  } else {
    return leftHalfBounds;
  }
};

const dynamicRightHalf = (window: Window) => {
  const rightHalfBounds = rightHalf(window);
  const rightOneThirdBounds = rightOneThird(window);
  const rightTwoThirdsBounds = rightTwoThirds(window);

  if (isWindowAtBounds(window, rightHalfBounds)) {
    return rightTwoThirds(window);
  } else if (isWindowAtBounds(window, rightTwoThirdsBounds)) {
    return rightOneThird(window);
  } else if (isWindowAtBounds(window, rightOneThirdBounds)) {
    return rightHalfBounds;
  } else {
    return rightHalfBounds;
  }
};

const rightHalf = (window: Window) => {
  const { height, width, x, y } = getMonitorSizes(window);
  return {
    x: x + width / 2,
    y,
    width: width / 2,
    height,
  };
};

const topLeftSixth = (window: Window) => {
  const { height, width, x, y } = getMonitorSizes(window);
  return {
    x,
    y,
    width: width / 3,
    height: height / 2,
  };
};

const topMiddleSixth = (window: Window) => {
  const { height, width, x, y } = getMonitorSizes(window);
  const newWidth = width / 3;
  return {
    x: x + newWidth,
    y,
    width: newWidth,
    height: height / 2,
  };
};

const topRightSixth = (window: Window) => {
  const { height, width, x, y } = getMonitorSizes(window);
  const newWidth = width / 3;
  return {
    x: x + newWidth * 2,
    y,
    width: newWidth,
    height: height / 2,
  };
};

const bottomLeftSixth = (window: Window) => {
  const { height, width, x, y } = getMonitorSizes(window);
  const newHeight = height / 2;
  return {
    x,
    y: y + newHeight,
    width: width / 3,
    height: newHeight,
  };
};

const bottomMiddleSixth = (window: Window) => {
  const { height, width, x, y } = getMonitorSizes(window);
  const newWidth = width / 3;
  const newHeight = height / 2;
  return {
    x: x + newWidth,
    y: y + newHeight,
    width: newWidth,
    height: newHeight,
  };
};

const bottomRightSixth = (window: Window) => {
  const { height, width, x, y } = getMonitorSizes(window);
  const newWidth = width / 3;
  const newHeight = height / 2;
  return {
    x: x + newWidth * 2,
    y: y + newHeight,
    width: newWidth,
    height: newHeight,
  };
};

export const setWindow = (window: Window, bounds: IRectangle) => {
  window.setBounds(bounds);
};

const toWindowMover = (bounsGetter: (window: Window) => IRectangle) => {
  return () => {
    const window = windowManager.getActiveWindow();
    window.setBounds(bounsGetter(window));
  };
};

export const getShortcutActions = () => {
  return {
    fullScreen: toWindowMover(fullScreen),
    leftOneThird: toWindowMover(leftOneThird),
    leftTwoThirds: toWindowMover(leftTwoThirds),
    rightOneThird: toWindowMover(rightOneThird),
    rightTwoThirds: toWindowMover(rightTwoThirds),
    leftHalf: toWindowMover(leftHalf),
    dynamicLeftHalf: toWindowMover(dynamicLeftHalf),
    dynamicRightHalf: toWindowMover(dynamicRightHalf),
    rightHalf: toWindowMover(rightHalf),
    topLeftSixth: toWindowMover(topLeftSixth),
    topMiddleSixth: toWindowMover(topMiddleSixth),
    topRightSixth: toWindowMover(topRightSixth),
    bottomLeftSixth: toWindowMover(bottomLeftSixth),
    bottomMiddleSixth: toWindowMover(bottomMiddleSixth),
    bottomRightSixth: toWindowMover(bottomRightSixth),
  } as const;
};

type TempRet = keyof ReturnType<typeof getShortcutActions>;

export const availableActions = Object.keys(
  getShortcutActions()
) as unknown as readonly [TempRet, ...TempRet[]];

export const actionValidator = z.enum(availableActions);

export type Action = z.output<typeof actionValidator>;

export const offsetsValidator = z.object({
  top: z.number(),
  left: z.number(),
  bottom: z.number(),
  right: z.number(),
});

export type Offsets = z.output<typeof offsetsValidator>;
